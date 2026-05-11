import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { profileService } from '../../api/profile.service';
import { Button } from '../../components/Button/Button';
import useAuth from '../../hooks/useAuth';
import useLogout from '../../hooks/useLogout';
import type { IProfileResponse, IUserBanStatus } from '../../types/profile.types';
import styles from './ProfilePage.module.css';

type Tab = 'profile' | 'security' | 'account';

// ─── Shared SVG icons ─────────────────────────────────────────────────────────

const EyeOn = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" fill="currentColor" viewBox="0 0 16 16">
        <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/><path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
    </svg>
);
const EyeOff = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" fill="currentColor" viewBox="0 0 16 16">
        <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"/><path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"/><path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"/>
    </svg>
);

// ─── Avatar helper ────────────────────────────────────────────────────────────

const AVATAR_COLORS = ['#7c3aed', '#2563eb', '#0891b2', '#059669', '#d97706', '#dc2626', '#db2777'];

function avatarColor(name: string) {
    return AVATAR_COLORS[name.charCodeAt(0) % AVATAR_COLORS.length];
}

function Avatar({ url, name, size }: { url: string | undefined; name: string; size: number }) {
    if (url) {
        return (
            <img
                src={url}
                alt={name}
                style={{ width: size, height: size, borderRadius: '50%', objectFit: 'cover', display: 'block' }}
            />
        );
    }
    return (
        <div
            style={{
                width: size, height: size, borderRadius: '50%',
                backgroundColor: avatarColor(name),
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: Math.round(size * 0.38), flexShrink: 0,
            }}
        >
            {name[0]?.toUpperCase() ?? '?'}
        </div>
    );
}

// ─── Profile completeness ─────────────────────────────────────────────────────

function CompletenessCard({ profile }: { profile: IProfileResponse }) {
    const checks = [
        { label: 'Фото профілю', done: !!profile.avatarUrl },
        { label: 'Електронна пошта підтверджена', done: profile.emailVerified },
        { label: 'Інформація «Про себе»', done: !!profile.bio },
        { label: 'Пароль встановлено', done: profile.hasPassword },
    ];
    const done = checks.filter(c => c.done).length;
    const pct = Math.round((done / checks.length) * 100);

    return (
        <div className={styles['card']}>
            <p className={styles['card-title']}>Завершеність профілю</p>
            <div className={styles['progress-header']}>
                <span className={styles['progress-pct']}>{pct}%</span>
                <span className={styles['progress-count']}>{done} з {checks.length}</span>
            </div>
            <div className={styles['progress-track']}>
                <div className={styles['progress-fill']} style={{ width: `${pct}%` }} />
            </div>
            <ul className={styles['check-list']}>
                {checks.map(c => (
                    <li key={c.label} className={c.done ? styles['check-done'] : styles['check-pending']}>
                        {c.done ? (
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
                            </svg>
                        ) : (
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                            </svg>
                        )}
                        {c.label}
                    </li>
                ))}
            </ul>
        </div>
    );
}

// ─── Account overview card ────────────────────────────────────────────────────

function AccountOverviewCard({ profile }: { profile: IProfileResponse }) {
    const memberSince = new Date(profile.createdAt).toLocaleDateString('uk-UA', {
        year: 'numeric', month: 'long', day: 'numeric',
    });
    const accountAge = (() => {
        const now = new Date();
        const created = new Date(profile.createdAt);
        const diffMs = now.getTime() - created.getTime();
        const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));
        if (days < 1) return 'Сьогодні';
        if (days === 1) return '1 день';
        if (days < 30) return `${days} дн.`;
        const months = Math.floor(days / 30);
        if (months < 12) return `${months} міс.`;
        const years = Math.floor(months / 12);
        const rem = months % 12;
        return rem > 0 ? `${years} р. ${rem} міс.` : `${years} р.`;
    })();

    const loginMethods: string[] = [];
    if (profile.hasPassword) loginMethods.push('E-mail / Пароль');
    if (profile.googleLinked) loginMethods.push('Google');

    return (
        <div className={styles['card']}>
            <p className={styles['card-title']}>Огляд акаунта</p>
            <div className={styles['overview-grid']}>
                <div className={styles['overview-item']}>
                    <span className={styles['overview-label']}>Дата реєстрації</span>
                    <span className={styles['overview-value']}>{memberSince}</span>
                </div>
                <div className={styles['overview-item']}>
                    <span className={styles['overview-label']}>Вік акаунта</span>
                    <span className={styles['overview-value']}>{accountAge}</span>
                </div>
                <div className={styles['overview-item']}>
                    <span className={styles['overview-label']}>Збірок створено</span>
                    <span className={styles['overview-value']}>{profile.buildCount}</span>
                </div>
                <div className={styles['overview-item']}>
                    <span className={styles['overview-label']}>Методи входу</span>
                    <span className={styles['overview-value']}>{loginMethods.join(', ') || 'Немає'}</span>
                </div>
                <div className={styles['overview-item']}>
                    <span className={styles['overview-label']}>Статус пошти</span>
                    <span className={styles['overview-value']}>
                        {profile.emailVerified ? (
                            <span className={styles['badge-verified']}>Підтверджено</span>
                        ) : (
                            <span className={styles['badge-unverified']}>Не підтверджено</span>
                        )}
                    </span>
                </div>
            </div>
        </div>
    );
}

// ─── Bans panel ──────────────────────────────────────────────────────────────

function BansPanel() {
    const [banStatus, setBanStatus] = useState<IUserBanStatus | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        profileService.getBans()
            .then(r => setBanStatus(r.data))
            .catch(() => {})
            .finally(() => setLoading(false));
    }, []);

    if (loading || !banStatus) return null;

    const hasBans = banStatus.isCommentBanned || banStatus.isPostBanned;
    const hasWarnings = banStatus.recentWarnings.length > 0;
    if (!hasBans && !hasWarnings) return null;

    const formatBanRemaining = (dateStr: string) => {
        const diff = new Date(dateStr).getTime() - Date.now();
        if (diff <= 0) return 'завершується';
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(hours / 24);
        if (days > 0) return `${days} дн. ${hours % 24} год.`;
        const minutes = Math.floor((diff / (1000 * 60)) % 60);
        return `${hours} год. ${minutes} хв.`;
    };

    const formatDate = (dateStr: string) =>
        new Date(dateStr).toLocaleDateString('uk-UA', {
            day: 'numeric', month: 'short', year: 'numeric',
        });

    return (
        <div className={styles['bans-card']}>
            <p className={styles['bans-title']}>Обмеження акаунта</p>

            {hasBans && (
                <div className={styles['bans-active']}>
                    {banStatus.isCommentBanned && banStatus.commentBanUntil && (
                        <div className={styles['ban-item']}>
                            <span className={styles['ban-badge-red']}>Бан коментарів</span>
                            <span className={styles['ban-remaining']}>
                                Залишилось: {formatBanRemaining(banStatus.commentBanUntil)}
                            </span>
                        </div>
                    )}
                    {banStatus.isPostBanned && banStatus.postBanUntil && (
                        <div className={styles['ban-item']}>
                            <span className={styles['ban-badge-red']}>Бан публікацій</span>
                            <span className={styles['ban-remaining']}>
                                Залишилось: {formatBanRemaining(banStatus.postBanUntil)}
                            </span>
                        </div>
                    )}
                </div>
            )}

            {hasWarnings && (
                <div className={styles['warnings-section']}>
                    <p className={styles['warnings-label']}>
                        Попередження за останні 180 днів ({banStatus.recentWarnings.length})
                    </p>
                    <ul className={styles['warnings-list']}>
                        {banStatus.recentWarnings.map(w => (
                            <li key={w.id} className={styles['warning-item']}>
                                <div className={styles['warning-header']}>
                                    <span className={w.banType === 'Comment' ? styles['ban-badge-amber'] : styles['ban-badge-amber']}>
                                        {w.banType === 'Comment' ? 'Коментарі' : 'Публікації'}
                                    </span>
                                    <span className={styles['warning-date']}>{formatDate(w.issuedAt)}</span>
                                </div>
                                <p className={styles['warning-reason']}>{w.reason}</p>
                            </li>
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
}

// ─── Tab: Profile ─────────────────────────────────────────────────────────────

function ProfileTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
    const [username, setUsername] = useState(profile.username);
    const [bio, setBio] = useState(profile.bio ?? '');
    const [saving, setSaving] = useState(false);
    const [saveMsg, setSaveMsg] = useState('');
    const [saveErr, setSaveErr] = useState('');
    const [avatarUrl, setAvatarUrl] = useState(profile.avatarUrl);
    const [avatarLoading, setAvatarLoading] = useState(false);
    const fileRef = useRef<HTMLInputElement>(null);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true); setSaveErr(''); setSaveMsg('');
        try {
            const res = await profileService.updateProfile({ username, bio: bio || undefined });
            onUpdate(res.data);
            setSaveMsg('Профіль збережено');
        } catch (err: any) {
            setSaveErr(err?.response?.data?.message ?? 'Помилка збереження');
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
            setSaveErr(err?.response?.data?.message ?? 'Помилка завантаження фото');
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
            onUpdate({ ...profile, avatarUrl: undefined });
        } catch { setSaveErr('Помилка видалення фото'); }
        finally { setAvatarLoading(false); }
    };

    return (
        <>
            <BansPanel />

            <CompletenessCard profile={profile} />

            <AccountOverviewCard profile={profile} />

            {/* Avatar */}
            <div className={styles['card']}>
                <p className={styles['card-title']}>Фото профілю</p>
                <div className={styles['avatar-section']}>
                    <div className={styles['avatar-wrap']}>
                        {avatarLoading ? (
                            <div className={styles['avatar-spinner-wrap']}>
                                <div className={styles['spinner']} />
                            </div>
                        ) : (
                            <Avatar url={avatarUrl} name={profile.username} size={88} />
                        )}
                    </div>
                    <div className={styles['avatar-actions']}>
                        <input
                            ref={fileRef}
                            type="file"
                            accept="image/jpeg,image/png,image/webp"
                            style={{ display: 'none' }}
                            onChange={handleAvatarChange}
                        />
                        <Button variant="outline-secondary" size="sm" onClick={() => fileRef.current?.click()} disabled={avatarLoading}>
                            Змінити фото
                        </Button>
                        {avatarUrl && (
                            <button className={styles['remove-link']} onClick={handleRemoveAvatar} disabled={avatarLoading}>
                                Видалити фото
                            </button>
                        )}
                        <p className={styles['avatar-hint']}>JPEG, PNG або WebP, до 5 МБ</p>
                    </div>
                </div>
            </div>

            {/* Info form */}
            <div className={styles['card']}>
                <p className={styles['card-title']}>Загальна інформація</p>
                {saveMsg && <p className={styles['success-msg']}>{saveMsg}</p>}
                {saveErr && <p className={styles['error-msg']}>{saveErr}</p>}
                <form onSubmit={handleSave}>
                    <div className={styles['form-group']}>
                        <label>E-mail</label>
                        <div className={styles['readonly-field']}>
                            <span>{profile.email}</span>
                            {profile.emailVerified ? (
                                <span className={styles['badge-verified']}>Підтверджено</span>
                            ) : (
                                <span className={styles['badge-unverified']}>Не підтверджено</span>
                            )}
                        </div>
                    </div>
                    <div className={styles['form-group']}>
                        <label htmlFor="username">Ім'я користувача</label>
                        <input
                            id="username" type="text" value={username}
                            onChange={e => { setUsername(e.target.value); setSaveMsg(''); setSaveErr(''); }}
                            maxLength={30} required
                        />
                    </div>
                    <div className={styles['form-group']}>
                        <label htmlFor="bio">Про себе</label>
                        <textarea
                            id="bio" value={bio}
                            onChange={e => { setBio(e.target.value); setSaveMsg(''); setSaveErr(''); }}
                            maxLength={200} rows={3} placeholder="Розкажіть трохи про себе..."
                        />
                        <span className={styles['char-count']}>{bio.length}/200</span>
                    </div>
                    <Button type="submit" variant="primary" size="md" disabled={saving}>
                        {saving ? 'Збереження...' : 'Зберегти'}
                    </Button>
                </form>
            </div>
        </>
    );
}

// ─── Tab: Security ────────────────────────────────────────────────────────────

function SecurityTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
    // Change-password state
    const [cur, setCur] = useState('');
    const [next, setNext] = useState('');
    const [confirm, setConfirm] = useState('');
    const [showCur, setShowCur] = useState(false);
    const [showNext, setShowNext] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const [saving, setSaving] = useState(false);
    const [msg, setMsg] = useState('');
    const [err, setErr] = useState('');

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();
        if (next !== confirm) { setErr('Паролі не збігаються'); return; }
        setSaving(true); setErr(''); setMsg('');
        try {
            await profileService.changePassword({ currentPassword: cur, newPassword: next });
            setMsg('Пароль успішно змінено');
            setCur(''); setNext(''); setConfirm('');
        } catch (err: any) {
            setErr(err?.response?.data?.message ?? 'Помилка зміни пароля');
        } finally { setSaving(false); }
    };

    const handleSetPassword = async (e: React.FormEvent) => {
        e.preventDefault();
        if (next !== confirm) { setErr('Паролі не збігаються'); return; }
        setSaving(true); setErr(''); setMsg('');
        try {
            await profileService.setPassword({ newPassword: next });
            setMsg('Пароль встановлено! Тепер ви можете входити через e-mail та пароль.');
            setNext(''); setConfirm('');
            onUpdate({ ...profile, hasPassword: true });
        } catch (err: any) {
            setErr(err?.response?.data?.message ?? 'Помилка встановлення пароля');
        } finally { setSaving(false); }
    };

    const pwField = (id: string, label: string, val: string, set: (v: string) => void, show: boolean, toggle: () => void) => (
        <div key={id} className={styles['form-group']}>
            <label htmlFor={id}>{label}</label>
            <div className={styles['pw-wrap']}>
                <input
                    id={id} type={show ? 'text' : 'password'} value={val}
                    onChange={e => { set(e.target.value); setMsg(''); setErr(''); }}
                    required autoComplete="new-password"
                />
                <button type="button" className={styles['pw-toggle']} onClick={toggle}>
                    {show ? <EyeOn /> : <EyeOff />}
                </button>
            </div>
        </div>
    );

    return (
        <>
            {/* Password section */}
            <div className={styles['card']}>
                {profile.hasPassword ? (
                    <>
                        <p className={styles['card-title']}>Зміна пароля</p>
                        {msg && <p className={styles['success-msg']}>{msg}</p>}
                        {err && <p className={styles['error-msg']}>{err}</p>}
                        <form onSubmit={handleChangePassword}>
                            {pwField('cur', 'Поточний пароль', cur, setCur, showCur, () => setShowCur(p => !p))}
                            {pwField('next', 'Новий пароль', next, setNext, showNext, () => setShowNext(p => !p))}
                            {pwField('confirm', 'Підтвердіть пароль', confirm, setConfirm, showConfirm, () => setShowConfirm(p => !p))}
                            <div className={styles['pw-hint']}>
                                Мінімум 8 символів, велика літера, мала літера, цифра
                            </div>
                            <Button type="submit" variant="primary" size="md" disabled={saving}>
                                {saving ? 'Збереження...' : 'Змінити пароль'}
                            </Button>
                        </form>
                    </>
                ) : (
                    <>
                        <p className={styles['card-title']}>Встановити пароль</p>
                        <p className={styles['info-text']} style={{ marginBottom: 16 }}>
                            Ваш акаунт використовує вхід через Google. Встановіть пароль, щоб також мати можливість входити через e-mail.
                        </p>
                        {msg && <p className={styles['success-msg']}>{msg}</p>}
                        {err && <p className={styles['error-msg']}>{err}</p>}
                        <form onSubmit={handleSetPassword}>
                            {pwField('next', 'Новий пароль', next, setNext, showNext, () => setShowNext(p => !p))}
                            {pwField('confirm', 'Підтвердіть пароль', confirm, setConfirm, showConfirm, () => setShowConfirm(p => !p))}
                            <div className={styles['pw-hint']}>
                                Мінімум 8 символів, велика літера, мала літера, цифра
                            </div>
                            <Button type="submit" variant="primary" size="md" disabled={saving}>
                                {saving ? 'Збереження...' : 'Встановити пароль'}
                            </Button>
                        </form>
                    </>
                )}
            </div>

            {/* Linked accounts */}
            <div className={styles['card']}>
                <p className={styles['card-title']}>Методи входу</p>

                {/* Email / password row */}
                <div className={styles['linked-account']}>
                    <div className={styles['linked-icon']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M2 2a2 2 0 0 0-2 2v8.01A2 2 0 0 0 2 14h5.5a.5.5 0 0 0 0-1H2a1 1 0 0 1-.966-.741l5.64-3.471L8 9.583l7-4.2V8.5a.5.5 0 0 0 1 0V4a2 2 0 0 0-2-2zm3.708 6.208L1 11.105V5.383zM1 4.217V4a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v.217l-7 4.2z"/>
                        </svg>
                    </div>
                    <div className={styles['linked-info']}>
                        <span className={styles['linked-name']}>E-mail / Пароль</span>
                        {profile.hasPassword ? (
                            <span className={styles['badge-verified']}>Активний</span>
                        ) : (
                            <span className={styles['badge-unverified']}>Не налаштовано</span>
                        )}
                    </div>
                </div>

                <div className={styles['linked-divider']} />

                {/* Google row */}
                <div className={styles['linked-account']}>
                    <div className={styles['linked-icon']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 48 48">
                            <path fill="#FFC107" d="M43.611 20.083H42V20H24v8h11.303c-1.649 4.657-6.08 8-11.303 8-6.627 0-12-5.373-12-12s5.373-12 12-12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 12.955 4 4 12.955 4 24s8.955 20 20 20 20-8.955 20-20c0-1.341-.138-2.65-.389-3.917z"/>
                            <path fill="#FF3D00" d="m6.306 14.691 6.571 4.819C14.655 15.108 18.961 12 24 12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 16.318 4 9.656 8.337 6.306 14.691z"/>
                            <path fill="#4CAF50" d="M24 44c5.166 0 9.86-1.977 13.409-5.192l-6.19-5.238A11.91 11.91 0 0 1 24 36c-5.202 0-9.619-3.317-11.283-7.946l-6.522 5.025C9.505 39.556 16.227 44 24 44z"/>
                            <path fill="#1976D2" d="M43.611 20.083H42V20H24v8h11.303a12.04 12.04 0 0 1-4.087 5.571l.003-.002 6.19 5.238C36.971 39.205 44 34 44 24c0-1.341-.138-2.65-.389-3.917z"/>
                        </svg>
                    </div>
                    <div className={styles['linked-info']}>
                        <span className={styles['linked-name']}>Google</span>
                        {profile.googleLinked ? (
                            <span className={styles['badge-verified']}>Підключено</span>
                        ) : (
                            <span className={styles['badge-unverified']}>Не підключено</span>
                        )}
                    </div>
                </div>
            </div>

            {/* Active sessions info */}
            <div className={styles['card']}>
                <p className={styles['card-title']}>Активні сесії</p>
                <div className={styles['session-row']}>
                    <div className={styles['session-icon']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M13.5 3a.5.5 0 0 1 .5.5V11H2V3.5a.5.5 0 0 1 .5-.5h11zm-11-1A1.5 1.5 0 0 0 1 3.5V12h14V3.5A1.5 1.5 0 0 0 13.5 2h-11zM0 12.5h16a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 0 12.5z"/>
                        </svg>
                    </div>
                    <div className={styles['session-info']}>
                        <span className={styles['session-name']}>Поточний пристрій</span>
                        <span className={styles['session-detail']}>Цей браузер</span>
                    </div>
                    <span className={styles['badge-verified']}>Активна</span>
                </div>
            </div>
        </>
    );
}

// ─── Tab: Account ─────────────────────────────────────────────────────────────

function AccountTab({ username }: { username: string }) {
    const [confirmText, setConfirmText] = useState('');
    const [loading, setLoading] = useState(false);
    const [err, setErr] = useState('');
    const logout = useLogout();
    const navigate = useNavigate();

    const handleDelete = async () => {
        if (confirmText !== username) return;
        setLoading(true); setErr('');
        try {
            await profileService.deleteAccount();
            await logout();
            navigate('/');
        } catch {
            setErr('Помилка видалення акаунта. Спробуйте ще раз.');
            setLoading(false);
        }
    };

    return (
        <>
            {/* Export data hint */}
            <div className={styles['card']}>
                <p className={styles['card-title']}>Ваші дані</p>
                <p className={styles['info-text']}>
                    Усі ваші збірки та налаштування зберігаються у вашому акаунті.
                    Ви можете переглянути свої збірки на сторінці «Мої Збірки».
                </p>
            </div>

            {/* Danger zone */}
            <div className={styles['danger-card']}>
                <p className={styles['danger-title']}>Видалити акаунт</p>
                <p className={styles['danger-text']}>
                    Ця дія незворотна. Всі ваші збірки та дані будуть видалені назавжди.
                    Для підтвердження введіть ваш логін: <strong>{username}</strong>
                </p>
                {err && <p className={styles['error-msg']}>{err}</p>}
                <input
                    className={styles['danger-input']}
                    type="text"
                    placeholder={username}
                    value={confirmText}
                    onChange={e => setConfirmText(e.target.value)}
                />
                <button
                    className={styles['danger-btn']}
                    onClick={handleDelete}
                    disabled={confirmText !== username || loading}
                >
                    {loading ? 'Видалення...' : 'Видалити акаунт'}
                </button>
            </div>
        </>
    );
}

// ─── Main page ────────────────────────────────────────────────────────────────

const ProfilePage = () => {
    const [activeTab, setActiveTab] = useState<Tab>('profile');
    const [profile, setProfile] = useState<IProfileResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const { setAuth } = useAuth();

    useEffect(() => {
        profileService.getProfile()
            .then(r => setProfile(r.data))
            .finally(() => setLoading(false));
    }, []);

    const handleProfileUpdate = (updated: IProfileResponse) => {
        setProfile(updated);
        setAuth(prev => ({ ...prev, username: updated.username, avatarUrl: updated.avatarUrl }));
    };

    if (loading) {
        return (
            <div className={styles['loading-wrap']}>
                <div className={styles['spinner']} />
            </div>
        );
    }

    if (!profile) return null;

    const tabs: { key: Tab; label: string; icon: React.ReactNode }[] = [
        {
            key: 'profile', label: 'Профіль',
            icon: <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0zm4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4zm-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.029 10 8 10c-2.029 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664h10z"/></svg>,
        },
        {
            key: 'security', label: 'Безпека',
            icon: <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2zm3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/></svg>,
        },
        {
            key: 'account', label: 'Акаунт',
            icon: <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872l-.1-.34zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z"/></svg>,
        },
    ];

    return (
        <div className={styles['page']}>
            {/* Sidebar */}
            <aside className={styles['sidebar']}>
                <div className={styles['sidebar-user']}>
                    <Avatar url={profile.avatarUrl} name={profile.username} size={72} />
                    <p className={styles['sidebar-name']}>{profile.username}</p>
                    <p className={styles['sidebar-email']}>{profile.email}</p>
                </div>
                <nav className={styles['sidebar-nav']}>
                    {tabs.map(t => (
                        <button
                            key={t.key}
                            className={`${styles['nav-btn']} ${activeTab === t.key ? styles['nav-btn-active'] : ''}`}
                            onClick={() => setActiveTab(t.key)}
                        >
                            {t.icon}
                            {t.label}
                        </button>
                    ))}
                </nav>
            </aside>

            {/* Content */}
            <main className={styles['content']}>
                {activeTab === 'profile' && <ProfileTab profile={profile} onUpdate={handleProfileUpdate} />}
                {activeTab === 'security' && <SecurityTab profile={profile} onUpdate={handleProfileUpdate} />}
                {activeTab === 'account' && <AccountTab username={profile.username} />}
            </main>
        </div>
    );
};

export default ProfilePage;
