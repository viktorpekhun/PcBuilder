import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { profileService } from '../../api/profile.service';
import type { IProfileResponse } from '../../types/profile.types';
import useLogout from '../../hooks/useLogout';
import styles from './ProfilePage.module.css';

export function AccountTab({ profile }: { profile: IProfileResponse }) {
    const [confirmText, setConfirmText] = useState('');
    const [loading, setLoading] = useState(false);
    const [err, setErr] = useState('');
    const logout = useLogout();
    const navigate = useNavigate();

    const canDelete = confirmText === profile.username;

    const handleDelete = async () => {
        if (!canDelete) return;
        setLoading(true); setErr('');
        try {
            await profileService.deleteAccount();
            await logout();
            navigate('/');
        } catch {
            setErr('Error deleting account. Try again.');
            setLoading(false);
        }
    };

    return (
        <div className={styles['grid-solo']}>
            <div className={styles['col']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Your data</span>
                        <span className={styles['card-aux']}>SCOPE</span>
                    </div>
                    <div className={styles['info-body']}>
                        <p>
                            Everything you've created on PcBuilder lives under your account: published and draft builds,
                            comments and reviews you've left, price-alert subscriptions, and your uploaded profile photo.
                        </p>
                        <p className={styles['info-mono']} style={{ marginTop: 10 }}>
                            SAVED BUILDS · <b>{profile.buildCount}</b>
                        </p>
                    </div>
                </div>

                <div className={`${styles['card']} ${styles['danger-card']}`}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>Delete account</span>
                        <span className={styles['danger-ex']}>IRREVERSIBLE</span>
                    </div>
                    <div className={styles['danger-body']}>
                        <p>
                            Deleting your account purges your profile, all <b>{profile.buildCount}</b> saved builds,
                            every comment and review you've left, and your active sessions. This cannot be undone.
                        </p>
                        <ul className={styles['danger-list']}>
                            <li>The username <code>@{profile.username}</code> is released back to the pool</li>
                            <li>The refresh-token cookie on this device is cleared</li>
                            <li>Reviews you've left on other builds are deleted</li>
                            <li>Price-alert subscriptions are unsubscribed</li>
                        </ul>

                        {err && <div className={styles['error-banner']}>{err}</div>}

                        <div className={styles['field']} style={{ margin: 0 }}>
                            <div className={styles['field-lbl']}>
                                <span>TYPE YOUR USERNAME TO CONFIRM</span>
                                <span className={styles['field-cnt']}>
                                    EXPECTED · <b style={{ color: 'var(--err)', fontFamily: 'var(--font-mono)' }}>{profile.username}</b>
                                </span>
                            </div>
                            <div className={styles['danger-confirm']}>
                                <input
                                    className={styles['danger-input']}
                                    type="text"
                                    placeholder={profile.username}
                                    value={confirmText}
                                    onChange={e => setConfirmText(e.target.value)}
                                />
                                <button className={styles['danger-btn']} disabled={!canDelete || loading} onClick={handleDelete}>
                                    {loading ? '...' : '× DELETE ACCOUNT'}
                                </button>
                            </div>
                            <span className={styles['danger-hint']} style={{ color: canDelete ? 'var(--err)' : 'var(--fg-3)', fontFamily: 'var(--font-mono)', fontSize: 10, letterSpacing: '0.08em' }}>
                                {canDelete ? 'READY · CLICK TO CONFIRM DELETION' : 'MATCH REQUIRED — CASE-SENSITIVE'}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
