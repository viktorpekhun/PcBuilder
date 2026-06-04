import { initials } from './profileHelpers';
import styles from './ProfilePage.module.css';

export function AvatarBox({ url, name, loading }: { url?: string; name: string; loading?: boolean }) {
    if (loading) {
        return (
            <div className={styles['avatar-spinner-box']}>
                <div className={styles['spinner']} />
            </div>
        );
    }
    return (
        <div className={`${styles['avatar-box']} ${styles['avatar-box-me']}`}>
            {url ? (
                <img src={url} alt={name} />
            ) : (
                <span className={styles['avatar-ini']}>{initials(name)}</span>
            )}
        </div>
    );
}
