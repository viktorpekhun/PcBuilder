import styles from './ProfilePage.module.css';

export type BadgeVariant = 'ok' | 'warn' | 'muted' | 'live';

const BADGE_CLASSES: Record<BadgeVariant, string> = {
    ok:    styles['badge-ok']    ?? '',
    warn:  styles['badge-warn']  ?? '',
    muted: styles['badge-muted'] ?? '',
    live:  styles['badge-live']  ?? '',
};

export function Badge({ variant, children }: { variant: BadgeVariant; children: React.ReactNode }) {
    return (
        <span className={`${styles['badge'] ?? ''} ${BADGE_CLASSES[variant]}`}>
            <span className={styles['badge-d']} />
            {children}
        </span>
    );
}
