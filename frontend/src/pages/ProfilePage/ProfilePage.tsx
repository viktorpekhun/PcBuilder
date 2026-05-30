import { useEffect, useState } from 'react';
import { profileService } from '../../api/profile.service';
import useAuth from '../../hooks/useAuth';
import type { IProfileResponse } from '../../types/profile.types';
import { AccountTab } from './AccountTab';
import { ProfileTab } from './ProfileTab';
import { SecurityTab } from './SecurityTab';
import { shortDate, accountAge } from './profileHelpers';
import styles from './ProfilePage.module.css';

type Tab = 'profile' | 'security' | 'account';

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
        setAuth(prev => ({
            ...prev,
            username: updated.username,
            ...(updated.avatarUrl !== undefined ? { avatarUrl: updated.avatarUrl } : {}),
        }));
    };

    if (loading) {
        return (
            <div className={styles['loading-wrap']}>
                <div className={styles['spinner']} />
            </div>
        );
    }

    if (!profile) return null;

    const title = activeTab === 'profile' ? 'Profile' : activeTab === 'security' ? 'Security' : 'Account';
    const eyebrow =
        activeTab === 'profile'  ? `USER · @${profile.username.toUpperCase()} · SETTINGS`
      : activeTab === 'security' ? `USER · @${profile.username.toUpperCase()} · AUTHENTICATION`
      :                            `USER · @${profile.username.toUpperCase()} · DESTRUCTIVE`;

    const methodCount = (profile.hasPassword ? 1 : 0) + (profile.googleLinked ? 1 : 0);

    return (
        <div className={styles['shell']}>
            <div className={styles['head']}>
                <div>
                    <span className={styles['eyebrow']}>{eyebrow}</span>
                    <h1>{title}</h1>
                    <div className={styles['head-meta']}>
                        <span>
                            <span className={styles['head-meta-dot']} style={{ background: 'var(--acc)' }} />
                            SIGNED IN AS <b>@{profile.username}</b>
                        </span>
                        <span className={styles['head-meta-sep']}>·</span>
                        <span>MEMBER SINCE <b>{shortDate(profile.createdAt)}</b> ({accountAge(profile.createdAt)})</span>
                        <span className={styles['head-meta-sep']}>·</span>
                        <span><b>{profile.buildCount}</b> BUILDS</span>
                    </div>
                </div>
            </div>

            <div className={styles['tabs']}>
                {([
                    { key: 'profile',  ic: '@', label: 'Profile',  num: `${profile.buildCount}` },
                    { key: 'security', ic: '⌬', label: 'Security', num: `${methodCount}/2` },
                    { key: 'account',  ic: '⌥', label: 'Account',  num: undefined },
                ] as const).map(t => (
                    <button
                        key={t.key}
                        className={`${styles['tab']} ${activeTab === t.key ? styles['tab-active'] : ''}`}
                        onClick={() => setActiveTab(t.key)}
                    >
                        <span className={styles['tab-ic']}>{t.ic}</span>
                        <span className={styles['tab-lbl']}>{t.label}</span>
                        {t.num != null && <span className={styles['tab-num']}>{t.num}</span>}
                    </button>
                ))}
                <div className={styles['tab-spacer']} />
                <div className={styles['tab-foot']}>
                    <span className={styles['tab-foot-dot']} />
                    <span>SESSION OK</span>
                </div>
            </div>

            <div className={styles['panel']}>
                {activeTab === 'profile'  && <ProfileTab  profile={profile} onUpdate={handleProfileUpdate} />}
                {activeTab === 'security' && <SecurityTab profile={profile} onUpdate={handleProfileUpdate} />}
                {activeTab === 'account'  && <AccountTab  profile={profile} />}
            </div>
        </div>
    );
};

export default ProfilePage;
