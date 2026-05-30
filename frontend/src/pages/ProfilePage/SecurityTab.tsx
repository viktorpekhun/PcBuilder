import { useState } from 'react';
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
                    {show ? 'HIDE' : 'SHOW'}
                </button>
            </div>
            {hint && <span className={styles['field-hint']}>USED ON /LOGIN AND BEFORE DESTRUCTIVE ACTIONS</span>}
        </div>
    );
}

export function SecurityTab({ profile, onUpdate }: { profile: IProfileResponse; onUpdate: (p: IProfileResponse) => void }) {
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
        } catch (err: any) {
            setErr(err?.response?.data?.message ?? 'Error changing password');
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
        } catch (err: any) {
            setErr(err?.response?.data?.message ?? 'Error setting password');
        } finally { setSaving(false); }
    };

    return (
        <div className={styles['grid']}>
            <div className={styles['col']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>
                            {profile.hasPassword ? 'Change password' : 'Set password'}
                        </span>
                        <span className={styles['card-aux']}>
                            {profile.hasPassword ? 'POST /API/PROFILE/CHANGE-PASSWORD' : 'POST /API/PROFILE/SET-PASSWORD'}
                        </span>
                    </div>
                    <div className={styles['card-b']}>
                        {!profile.hasPassword && (
                            <p style={{ margin: '0 0 16px', fontSize: 13, color: 'var(--fg-1)', lineHeight: 1.5 }}>
                                Your account uses <b style={{ fontFamily: 'var(--font-mono)', color: 'var(--fg-0)' }}>Google SSO</b> only.
                                Setting a password lets you sign in with e-mail as a backup.
                            </p>
                        )}
                        {err && <div className={styles['error-banner']}>{err}</div>}
                        <form onSubmit={profile.hasPassword ? handleChangePassword : handleSetPassword}>
                            {profile.hasPassword && (
                                <PwField id="cur" label="CURRENT PASSWORD" value={cur} setValue={setCur}
                                    show={showCur} toggle={() => setShowCur(s => !s)} />
                            )}
                            <PwField id="next" label="NEW PASSWORD" value={next} setValue={setNext}
                                show={showNext} toggle={() => setShowNext(s => !s)} hint />
                            <PwField id="conf" label="CONFIRM NEW PASSWORD" value={conf} setValue={setConf}
                                show={showConf} toggle={() => setShowConf(s => !s)}
                                error={mismatch ? 'PASSWORDS DO NOT MATCH' : null} />

                            <div className={styles['form-actions']}>
                                <button className={styles['btn-pri']} type="submit" disabled={!canSubmit || saving}>
                                    {profile.hasPassword ? '✓ Update password' : '✓ Set password'}
                                </button>
                                <button className={styles['btn-ghost']} type="button"
                                    onClick={() => { setCur(''); setNext(''); setConf(''); setErr(''); }}>
                                    ← Clear
                                </button>
                                {saved && (
                                    <span className={styles['saved-msg']}>
                                        <span className={styles['saved-dot']} />SAVED
                                    </span>
                                )}
                                <span className={styles['pw-rules-hint']}>MIN 8 · UPPER · LOWER · DIGIT</span>
                            </div>
                        </form>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Sign-in methods</span>
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
                                    <span className={styles['linked-name']}>E-mail &amp; password</span>
                                    <span className={styles['linked-sub']}>{profile.email}</span>
                                </div>
                                {profile.hasPassword
                                    ? <Badge variant="ok">ACTIVE</Badge>
                                    : <Badge variant="muted">NOT SET</Badge>}
                            </div>
                            <div className={styles['linked-row']}>
                                <span className={styles['linked-gly']}>G</span>
                                <div className={styles['linked-nm']}>
                                    <span className={styles['linked-name']}>Google</span>
                                    <span className={styles['linked-sub']}>
                                        {profile.googleLinked ? `Linked · ${profile.email}` : 'Not linked'}
                                    </span>
                                </div>
                                {profile.googleLinked
                                    ? <Badge variant="ok">LINKED</Badge>
                                    : <Badge variant="muted">UNLINKED</Badge>}
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Active sessions</span>
                        <span className={styles['card-aux']}>1 DEVICE</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <div className={styles['session-row']}>
                            <span className={styles['session-gly']}>▣</span>
                            <div className={styles['session-nm']}>
                                <div className={styles['session-name']}>This browser</div>
                                <div className={styles['session-sub']}>CURRENT SESSION</div>
                            </div>
                            <Badge variant="live">ACTIVE</Badge>
                        </div>
                        <p className={styles['session-foot']}>
                            SESSION REVOCATION ISN'T EXPOSED BY /API/PROFILE — SIGNING OUT CLEARS THE REFRESH-TOKEN COOKIE ON THIS DEVICE ONLY.
                        </p>
                    </div>
                </div>
            </div>

            <aside className={styles['side']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Auth surface</span>
                        <span className={styles['card-aux']}>SUMMARY</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <div className={styles['kv']}>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>Password</span>
                                <span className={styles['kv-v']}>
                                    {profile.hasPassword
                                        ? <Badge variant="ok">SET</Badge>
                                        : <Badge variant="muted">NOT SET</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>Google SSO</span>
                                <span className={styles['kv-v']}>
                                    {profile.googleLinked
                                        ? <Badge variant="ok">LINKED</Badge>
                                        : <Badge variant="muted">UNLINKED</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>E-mail</span>
                                <span className={styles['kv-v']}>
                                    {profile.emailVerified
                                        ? <Badge variant="ok">VERIFIED</Badge>
                                        : <Badge variant="warn">UNVERIFIED</Badge>}
                                </span>
                            </div>
                            <div className={styles['kv-row']}>
                                <span className={styles['kv-k']}>Active sessions</span>
                                <span className={styles['kv-v']}>1</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Password rules</span>
                        <span className={styles['card-aux']}>SERVER-SIDE</span>
                    </div>
                    <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                        <ul className={styles['pw-rules-list']}>
                            {['Minimum 8 characters', 'At least one uppercase letter', 'At least one lowercase letter', 'At least one digit'].map(r => (
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
