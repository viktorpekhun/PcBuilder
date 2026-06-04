import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { profileService } from '../../api/profile.service';
import type { IProfileResponse } from '../../types/profile.types';
import { Badge } from './ProfileBadge';
import styles from './ProfilePage.module.css';

function PwField({
    id, label, value, setValue, show, toggle, hint, error,
}: {
    id: string; label: string; value: string;
    setValue: (v: string) => void; show: boolean; toggle: () => void;
    hint?: boolean; error?: string | null;
}) {
    const { t } = useTranslation();
    return (
        <div className={styles['field']}>
            <div className={styles['field-lbl']}>
                <span>{label}</span>
                {error && <span className={styles['field-error']}>× {error}</span>}
            </div>
            <div className={styles['pw-wrap']}>
                <input
                    id={id}
                    type={show ? 'text' : 'password'}
                    value={value}
                    onChange={e => setValue(e.target.value)}
                    className={styles['input']}
                    autoComplete="new-password"
                />
                <button type="button" className={styles['pw-eye']} onClick={toggle}>
                    {show ? t('profile.securityTab.hidePassword') : t('profile.securityTab.showPassword')}
                </button>
            </div>
            {hint && <span className={styles['field-hint']}>{t('profile.securityTab.pwHint')}</span>}
        </div>
    );
}

export function SecurityTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
    const { t } = useTranslation();
    const [cur, setCur] = useState('');
    const [next, setNext] = useState('');
    const [conf, setConf] = useState('');
    const [showCur, setShowCur] = useState(false);
    const [showNext, setShowNext] = useState(false);
    const [showConf, setShowConf] = useState(false);
    const [saving, setSaving] = useState(false);
    const [saved, setSaved] = useState(false);
    const [err, setErr] = useState('');

    const mismatch = next.length > 0 && conf.length > 0 && next !== conf;
    const canSubmit = profile.hasPassword
        ? (cur.length >= 8 && next.length >= 8 && next === conf)
        : (next.length >= 8 && next === conf);

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true); setErr(''); setSaved(false);
        try {
            await profileService.changePassword({ currentPassword: cur, newPassword: next });
            setSaved(true);
            setTimeout(() => setSaved(false), 2200);
            setCur(''); setNext(''); setConf('');
        } catch (err: unknown) {
            const data = (err as { response?: { data?: { errors?: string[]; message?: string } } })?.response?.data;
            const msg: string = data?.errors?.[0] ?? data?.message ?? t('profile.securityTab.changeFailed');
            setErr(msg);
        } finally { setSaving(false); }
    };

    const handleSetPassword = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true); setErr(''); setSaved(false);
        try {
            await profileService.setPassword({ newPassword: next });
            setSaved(true);
            setTimeout(() => setSaved(false), 2200);
            setNext(''); setConf('');
            onUpdate({ ...profile, hasPassword: true });
        } catch (err: unknown) {
            const e = err as { response?: { data?: { message?: string } } };
            setErr(e?.response?.data?.message ?? t('profile.securityTab.changeFailed'));
        } finally { setSaving(false); }
    };

    const pwRules = [
        t('profile.securityTab.pwRule1'),
        t('profile.securityTab.pwRule2'),
        t('profile.securityTab.pwRule3'),
        t('profile.securityTab.pwRule4'),
    ];

    return (
        <div className={styles['grid']}>
            <div className={styles['col']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>
                            {profile.hasPassword ? t('profile.securityTab.changePasswordTitle') : t('profile.securityTab.setPasswordTitle')}
                        </span>
                        <span className={styles['card-aux']}>
                            {profile.hasPassword ? 'POST /API/PROFILE/CHANGE-PASSWORD' : 'POST /API/PROFILE/SET-PASSWORD'}
                        </span>
                    </div>
                    <div className={styles['card-b']}>
                        {!profile.hasPassword && (
                            <p style={{ margin: '0 0 16px', fontSize: 13, color: 'var(--fg-1)', lineHeight: 1.5 }}
                               dangerouslySetInnerHTML={{ __html: t('profile.securityTab.googleSsoNote') }} />
                        )}
                        {err && <div className={styles['error-banner']}>{err}</div>}
                        <form onSubmit={profile.hasPassword ? handleChangePassword : handleSetPassword}>
                            {profile.hasPassword && (
                                <PwField id="cur" label={t('profile.securityTab.currentPasswordLabel')} value={cur} setValue={setCur}
                                    show={showCur} toggle={() => setShowCur(s => !s)} />
                            )}
                            <PwField id="next" label={t('profile.securityTab.newPasswordLabel')} value={next} setValue={setNext}
                                show={showNext} toggle={() => setShowNext(s => !s)} hint />
                            <PwField id="conf" label={t('profile.securityTab.confirmPasswordLabel')} value={conf} setValue={setConf}
                                show={showConf} toggle={() => setShowConf(s => !s)}
                                error={mismatch ? t('profile.securityTab.passwordMismatch') : null} />

                            <div className={styles['form-actions']}>
                                <button className={styles['btn-pri']} type="submit" disabled={!canSubmit || saving}>
                                    {profile.hasPassword ? t('profile.securityTab.updatePassword') : t('profile.securityTab.setPassword')}
                                </button>
                                <button className={styles['btn-ghost']} type="button"
                                    onClick={() => { setCur(''); setNext(''); setConf(''); setErr(''); }}>
                                    {t('profile.securityTab.clearFields')}
                                </button>
                                {saved && (
                                    <span className={styles['saved-msg']}>
                                        <span className={styles['saved-dot']} />{t('profile.securityTab.savedMsg')}
                                    </span>
                                )}
                                <span className={styles['pw-rules-hint']}>{t('profile.securityTab.pwRulesHint')}</span>
                            </div>
                        </form>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.securityTab.signInMethodsTitle')}</span>
                        <span className={styles['card-aux']}>
                            {[profile.hasPassword ? 'EMAIL' : null, profile.googleLinked ? 'GOOGLE' : null]
                                .filter((x): x is string => x !== null).join(' + ') || 'NONE'}
                        </span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <div className={styles['linked']}>
                            <div className={styles['linked-row']}>
                                <span className={styles['linked-gly']}>@</span>
                                <div className={styles['linked-nm']}>
                                    <span className={styles['linked-name']}>{t('profile.securityTab.emailPasswordMethod')}</span>
                                    <span className={styles['linked-sub']}>{profile.email}</span>
                                </div>
                                {profile.hasPassword
                                    ? <Badge variant="ok">{t('profile.securityTab.badgeActive')}</Badge>
                                    : <Badge variant="muted">{t('profile.securityTab.badgeNotSet')}</Badge>}
                            </div>
                            <div className={styles['linked-row']}>
                                <span className={styles['linked-gly']}>G</span>
                                <div className={styles['linked-nm']}>
                                    <span className={styles['linked-name']}>{t('profile.securityTab.googleMethod')}</span>
                                    <span className={styles['linked-sub']}>
                                        {profile.googleLinked ? t('profile.securityTab.googleLinkedSub', { email: profile.email }) : t('profile.securityTab.googleNotLinked')}
                                    </span>
                                </div>
                                {profile.googleLinked
                                    ? <Badge variant="ok">{t('profile.securityTab.badgeLinked')}</Badge>
                                    : <Badge variant="muted">{t('profile.securityTab.badgeUnlinked')}</Badge>}
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.securityTab.activeSessionsTitle')}</span>
                        <span className={styles['card-aux']}>{t('profile.securityTab.activeSessionsAux')}</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <div className={styles['session-row']}>
                            <span className={styles['session-gly']}>▣</span>
                            <div className={styles['session-nm']}>
                                <div className={styles['session-name']}>{t('profile.securityTab.thisBrowser')}</div>
                                <div className={styles['session-sub']}>{t('profile.securityTab.currentSession')}</div>
                            </div>
                            <Badge variant="live">{t('profile.securityTab.badgeActive')}</Badge>
                        </div>
                        <p className={styles['session-foot']}>
                            {t('profile.securityTab.sessionNote')}
                        </p>
                    </div>
                </div>
            </div>

            <aside className={styles['side']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.securityTab.authSurfaceTitle')}</span>
                        <span className={styles['card-aux']}>{t('profile.securityTab.authSurfaceAux')}</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <div className={styles['kv']}>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>{t('profile.securityTab.passwordLabel')}</span>
                                <span className={styles['kv-v']}>
                                    {profile.hasPassword
                                        ? <Badge variant="ok">{t('profile.securityTab.badgeActive')}</Badge>
                                        : <Badge variant="muted">{t('profile.securityTab.badgeNotSet')}</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>{t('profile.securityTab.googleSsoLabel')}</span>
                                <span className={styles['kv-v']}>
                                    {profile.googleLinked
                                        ? <Badge variant="ok">{t('profile.securityTab.badgeLinked')}</Badge>
                                        : <Badge variant="muted">{t('profile.securityTab.badgeUnlinked')}</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>{t('profile.securityTab.emailStatusLabel')}</span>
                                <span className={styles['kv-v']}>
                                    {profile.emailVerified
                                        ? <Badge variant="ok">{t('profile.sideCards.badgeVerified')}</Badge>
                                        : <Badge variant="warn">{t('profile.sideCards.badgeUnverified')}</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>{t('profile.securityTab.activeSessionsLabel')}</span>
                                <span className={styles['kv-v']}>1</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.securityTab.passwordRulesTitle')}</span>
                        <span className={styles['card-aux']}>{t('profile.securityTab.passwordRulesAux')}</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <ul className={styles['pw-rules-list']}>
                            {pwRules.map(r => (
                                <li key={r}>
                                    <span className={styles['pw-rules-chk']}>[✓]</span>
                                    <span>{r}</span>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            </aside>
        </div>
    );
}
