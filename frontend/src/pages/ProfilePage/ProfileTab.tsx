import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { profileService } from '../../api/profile.service';
import type { IProfileResponse, IUserBanStatus } from '../../types/profile.types';
import { AvatarBox } from './ProfileAvatar';
import { Badge } from './ProfileBadge';
import { BansCard, CompletenessCard, OverviewCard } from './ProfileSideCards';
import styles from './ProfilePage.module.css';

export function ProfileTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
    const { t } = useTranslation();
    const [username, setUsername] = useState(profile.username);
    const [bio, setBio] = useState(profile.bio ?? '');
    const [saving, setSaving] = useState(false);
    const [saved, setSaved] = useState(false);
    const [saveErr, setSaveErr] = useState('');
    const [avatarUrl, setAvatarUrl] = useState(profile.avatarUrl);
    const [avatarLoading, setAvatarLoading] = useState(false);
    const [banStatus, setBanStatus] = useState<IUserBanStatus | null>(null);
    const fileRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        profileService.getBans()
            .then(r => setBanStatus(r.data))
            .catch(() => {});
    }, []);

    const dirty = username !== profile.username || (bio || '') !== (profile.bio || '');

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true); setSaveErr(''); setSaved(false);
        try {
            const res = await profileService.updateProfile({ username, ...(bio ? { bio } : {}) });
            onUpdate(res.data);
            setSaved(true);
            setTimeout(() => setSaved(false), 2200);
        } catch (err: unknown) {
            const e = err as { response?: { data?: { message?: string } } };
            setSaveErr(e?.response?.data?.message ?? t('profile.profileTab.saveFailed'));
        } finally { setSaving(false); }
    };

    const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setAvatarLoading(true);
        const fd = new FormData();
        fd.append('file', file);
        try {
            const res = await profileService.uploadAvatar(fd);
            setAvatarUrl(res.data.avatarUrl);
            onUpdate({ ...profile, avatarUrl: res.data.avatarUrl });
        } catch (err: unknown) {
            const e = err as { response?: { data?: { message?: string } } };
            setSaveErr(e?.response?.data?.message ?? t('profile.profileTab.avatarUploadFailed'));
        } finally {
            setAvatarLoading(false);
            if (fileRef.current) fileRef.current.value = '';
        }
    };

    const handleRemoveAvatar = async () => {
        setAvatarLoading(true);
        try {
            await profileService.deleteAvatar();
            setAvatarUrl(undefined);
            const { avatarUrl: _, ...profileNoAvatar } = profile;
            onUpdate(profileNoAvatar as IProfileResponse);
        } catch { setSaveErr(t('profile.profileTab.avatarRemoveFailed')); }
        finally { setAvatarLoading(false); }
    };

    const hasBanContent = banStatus && (
        banStatus.isCommentBanned || banStatus.isPostBanned || banStatus.recentWarnings.length > 0
    );

    return (
        <div className={styles['grid']}>
            <div className={styles['col']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.profileTab.photoCardTitle')}</span>
                        <span className={styles['card-aux']}>{t('profile.profileTab.photoCardAux')}</span>
                    </div>
                    <div className={styles['card-b']}>
                        <div className={styles['avatar-row']}>
                            <AvatarBox url={avatarUrl} name={profile.username} loading={avatarLoading} />
                            <div className={styles['avatar-actions']}>
                                <div className={styles['avatar-actions-row']}>
                                    <input
                                        ref={fileRef}
                                        type="file"
                                        accept="image/jpeg,image/png,image/webp"
                                        style={{ display: 'none' }}
                                        onChange={handleAvatarChange}
                                    />
                                    <button className={styles['btn-sec']} onClick={() => fileRef.current?.click()} disabled={avatarLoading}>
                                        {t('profile.profileTab.uploadPhoto')}
                                    </button>
                                    {avatarUrl && (
                                        <button className={styles['avatar-rm']} onClick={handleRemoveAvatar} disabled={avatarLoading}>
                                            {t('profile.profileTab.removePhoto')}
                                        </button>
                                    )}
                                </div>
                                <div className={styles['avatar-hint']}>
                                    {avatarUrl ? t('profile.profileTab.currentCustomPhoto') : t('profile.profileTab.currentInitialsFallback')}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.profileTab.generalCardTitle')}</span>
                        <span className={styles['card-aux']}>PUT /API/PROFILE · {dirty ? t('profile.profileTab.unsaved') : t('profile.profileTab.inSync')}</span>
                    </div>
                    <div className={styles['card-b']}>
                        {saveErr && <div className={styles['error-banner']}>{saveErr}</div>}
                        <form onSubmit={handleSave}>
                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>{t('profile.profileTab.emailLabel')}</span>
                                    <span className={styles['field-cnt']}>{t('profile.profileTab.emailReadOnly')}</span>
                                </div>
                                <div className={styles['readonly']}>
                                    <span>{profile.email}</span>
                                    {profile.emailVerified
                                        ? <Badge variant="ok">{t('profile.profileTab.emailVerified')}</Badge>
                                        : <Badge variant="warn">{t('profile.profileTab.emailUnverified')}</Badge>}
                                </div>
                            </div>

                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>{t('profile.profileTab.usernameLabel')} <span className={styles['field-lbl-req']}>*</span></span>
                                    <span className={styles['field-cnt']}><b>{username.length}</b>/30</span>
                                </div>
                                <input
                                    className={`${styles['input']} ${styles['input-mono']}`}
                                    type="text"
                                    value={username}
                                    maxLength={30}
                                    required
                                    onChange={e => { setUsername(e.target.value); setSaved(false); setSaveErr(''); }}
                                />
                                <span className={styles['field-hint']}>
                                    {t('profile.profileTab.usernameHint')}
                                </span>
                            </div>

                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>{t('profile.profileTab.bioLabel')}</span>
                                    <span className={styles['field-cnt']}><b>{bio.length}</b>/200</span>
                                </div>
                                <textarea
                                    className={styles['textarea']}
                                    rows={3}
                                    maxLength={200}
                                    value={bio}
                                    placeholder={t('profile.profileTab.bioPlaceholder')}
                                    onChange={e => { setBio(e.target.value); setSaved(false); setSaveErr(''); }}
                                />
                            </div>

                            <div className={styles['form-actions']}>
                                <button className={styles['btn-pri']} type="submit" disabled={!dirty || saving}>
                                    {saving ? '...' : `✓ ${t('profile.profileTab.save')}`}
                                </button>
                                <button
                                    className={styles['btn-ghost']}
                                    type="button"
                                    disabled={!dirty}
                                    onClick={() => { setUsername(profile.username); setBio(profile.bio ?? ''); setSaveErr(''); }}
                                >
                                    {t('profile.profileTab.revert')}
                                </button>
                                {saved && (
                                    <span className={styles['saved-msg']}>
                                        <span className={styles['saved-dot']} />{t('profile.profileTab.savedMsg')}
                                    </span>
                                )}
                            </div>
                        </form>
                    </div>
                </div>
            </div>

            <aside className={styles['side']}>
                {hasBanContent && <BansCard banStatus={banStatus!} />}
                <CompletenessCard profile={profile} />
                <OverviewCard profile={profile} />
            </aside>
        </div>
    );
}
