import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
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
    const { t, i18n } = useTranslation();
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

    const title =
        activeTab === 'profile'  ? t('profile.titleProfile')
      : activeTab === 'security' ? t('profile.titleSecurity')
      :                            t('profile.titleAccount');
    const eyebrow =
        activeTab === 'profile'  ? t('profile.eyebrowSettings', { username: profile.username.toUpperCase() })
      : activeTab === 'security' ? t('profile.eyebrowAuthentication', { username: profile.username.toUpperCase() })
      :                            t('profile.eyebrowDestructive', { username: profile.username.toUpperCase() });

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
                            {t('profile.signedInAs')} <b>@{profile.username}</b>
                        </span>
                        <span className={styles['head-meta-sep']}>·</span>
                        <span>{t('profile.memberSince')} <b>{shortDate(profile.createdAt, i18n.language)}</b> ({accountAge(profile.createdAt, t)})</span>
                        <span className={styles['head-meta-sep']}>·</span>
                        <span><b>{profile.buildCount}</b> {t('profile.builds')}</span>
                    </div>
                </div>
            </div>

            <div className={styles['tabs']}>
                {([
                    { key: 'profile',  ic: '@', label: t('profile.tabs.profile'),  num: `${profile.buildCount}` },
                    { key: 'security', ic: '⌬', label: t('profile.tabs.security'), num: `${methodCount}/2` },
                    { key: 'account',  ic: '⌥', label: t('profile.tabs.account'),  num: undefined },
                ] as const).map(tab => (
                    <button
                        key={tab.key}
                        className={`${styles['tab']} ${activeTab === tab.key ? styles['tab-active'] : ''}`}
                        onClick={() => setActiveTab(tab.key)}
                    >
                        <span className={styles['tab-ic']}>{tab.ic}</span>
                        <span className={styles['tab-lbl']}>{tab.label}</span>
                        {tab.num != null && <span className={styles['tab-num']}>{tab.num}</span>}
                    </button>
                ))}
                <div className={styles['tab-spacer']} />
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
