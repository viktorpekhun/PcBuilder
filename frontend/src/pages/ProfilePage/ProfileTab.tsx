import { useEffect, useRef, useState } from 'react';
import { profileService } from '../../api/profile.service';
import type { IProfileResponse, IUserBanStatus } from '../../types/profile.types';
import { AvatarBox } from './ProfileAvatar';
import { Badge } from './ProfileBadge';
import { BansCard, CompletenessCard, OverviewCard } from './ProfileSideCards';
import styles from './ProfilePage.module.css';

export function ProfileTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
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
        } catch (err: any) {
            setSaveErr(err?.response?.data?.message ?? 'Save error');
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
        } catch (err: any) {
            setSaveErr(err?.response?.data?.message ?? 'Upload error');
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
        } catch { setSaveErr('Remove error'); }
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
                        <span className={styles['card-ttl']}>Profile photo</span>
                        <span className={styles['card-aux']}>JPEG · PNG · WEBP · MAX 5 MB</span>
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
                                        ↑ Upload photo
                                    </button>
                                    {avatarUrl && (
                                        <button className={styles['avatar-rm']} onClick={handleRemoveAvatar} disabled={avatarLoading}>
                                            × Remove photo
                                        </button>
                                    )}
                                </div>
                                <div className={styles['avatar-hint']}>
                                    {avatarUrl ? 'CURRENT · CUSTOM PHOTO' : 'CURRENT · INITIALS FALLBACK · NO IMAGE'}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>General</span>
                        <span className={styles['card-aux']}>PUT /API/PROFILE · {dirty ? 'UNSAVED' : 'IN SYNC'}</span>
                    </div>
                    <div className={styles['card-b']}>
                        {saveErr && <div className={styles['error-banner']}>{saveErr}</div>}
                        <form onSubmit={handleSave}>
                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>E-MAIL</span>
                                    <span className={styles['field-cnt']}>READ-ONLY</span>
                                </div>
                                <div className={styles['readonly']}>
                                    <span>{profile.email}</span>
                                    {profile.emailVerified
                                        ? <Badge variant="ok">VERIFIED</Badge>
                                        : <Badge variant="warn">UNVERIFIED · RESEND →</Badge>}
                                </div>
                            </div>

                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>USERNAME <span className={styles['field-lbl-req']}>*</span></span>
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
                                    USED ON YOUR PUBLIC PROFILE AND ON REVIEWS YOU LEAVE
                                </span>
                            </div>

                            <div className={styles['field']}>
                                <div className={styles['field-lbl']}>
                                    <span>BIO</span>
                                    <span className={styles['field-cnt']}><b>{bio.length}</b>/200</span>
                                </div>
                                <textarea
                                    className={styles['textarea']}
                                    rows={3}
                                    maxLength={200}
                                    value={bio}
                                    placeholder="A line or two about the rigs you build."
                                    onChange={e => { setBio(e.target.value); setSaved(false); setSaveErr(''); }}
                                />
                            </div>

                            <div className={styles['form-actions']}>
                                <button className={styles['btn-pri']} type="submit" disabled={!dirty || saving}>
                                    {saving ? '...' : '✓ Save changes'}
                                </button>
                                <button
                                    className={styles['btn-ghost']}
                                    type="button"
                                    disabled={!dirty}
                                    onClick={() => { setUsername(profile.username); setBio(profile.bio ?? ''); setSaveErr(''); }}
                                >
                                    ← Revert
                                </button>
                                {saved && (
                                    <span className={styles['saved-msg']}>
                                        <span className={styles['saved-dot']} />SAVED
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
