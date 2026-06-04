import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { profileService } from '../../api/profile.service';
import type { IProfileResponse } from '../../types/profile.types';
import useLogout from '../../hooks/useLogout';
import styles from './ProfilePage.module.css';

export function AccountTab({ profile }: { profile: IProfileResponse }) {
    const { t } = useTranslation();
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
            setErr(t('profile.accountTab.deleteError'));
            setLoading(false);
        }
    };

    return (
        <div className={styles['grid-solo']}>
            <div className={styles['col']}>
                <div className={styles['card']}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.accountTab.yourDataTitle')}</span>
                        <span className={styles['card-aux']}>{t('profile.accountTab.yourDataAux')}</span>
                    </div>
                    <div className={styles['info-body']}>
                        <p>{t('profile.accountTab.yourDataBody')}</p>
                        <p className={styles['info-mono']} style={{ marginTop: 10 }}>
                            {t('profile.accountTab.savedBuilds')} · <b>{profile.buildCount}</b>
                        </p>
                    </div>
                </div>

                <div className={`${styles['card']} ${styles['danger-card']}`}>
                    <div className={styles['card-h']}>
                        <span className={styles['card-ttl']}>{t('profile.accountTab.deleteAccountTitle')}</span>
                        <span className={styles['danger-ex']}>{t('profile.accountTab.irreversible')}</span>
                    </div>
                    <div className={styles['danger-body']}>
                        <p>
                            {t('profile.accountTab.deleteAccountBody')} <b>{profile.buildCount}</b>
                        </p>
                        <ul className={styles['danger-list']}>
                            <li>{t('profile.accountTab.deleteItem1', { username: profile.username })}</li>
                            <li>{t('profile.accountTab.deleteItem2')}</li>
                            <li>{t('profile.accountTab.deleteItem3')}</li>
                            <li>{t('profile.accountTab.deleteItem4')}</li>
                        </ul>

                        {err && <div className={styles['error-banner']}>{err}</div>}

                        <div className={styles['field']} style={{ margin: 0 }}>
                            <div className={styles['field-lbl']}>
                                <span>{t('profile.accountTab.confirmLabel')}</span>
                                <span className={styles['field-cnt']}>
                                    {t('profile.accountTab.confirmExpected')} · <b style={{ color: 'var(--err)', fontFamily: 'var(--font-mono)' }}>{profile.username}</b>
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
                                    {loading ? '...' : t('profile.accountTab.deleteBtn')}
                                </button>
                            </div>
                            <span className={styles['danger-hint']} style={{ color: canDelete ? 'var(--err)' : 'var(--fg-3)', fontFamily: 'var(--font-mono)', fontSize: 10, letterSpacing: '0.08em' }}>
                                {canDelete ? t('profile.accountTab.readyToDelete') : t('profile.accountTab.matchRequired')}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
