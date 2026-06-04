import styles from './BuildCover.module.css';

interface BuildCoverProps {
    /** Build title used as the deterministic seed for the generated graphic. */
    title: string;
    /** When provided, renders the actual uploaded cover image instead of the generated graphic. */
    coverUrl?: string | undefined;
    /** Optional small tag rendered top-left (e.g. socket). */
    tag?: string;
    /** Whether to render the bottom slug/ID stamp. */
    stamp?: boolean;
    className?: string;
}

function hash(seed: string): number {
    return Math.abs(seed.split('').reduce((a, c) => (a * 31 + c.charCodeAt(0)) | 0, 0));
}

/**
 * Procedurally-generated cover graphic for builds without an uploaded image.
 * Hashes the title into dual radial blooms + a 1px grid + a mono slug stamp,
 * so every build gets a distinct but on-brand graphic.
 */
export function BuildCover({ title, coverUrl, tag, stamp = true, className = '' }: BuildCoverProps) {
    const seed = title || 'untitled';

    const h = hash(seed);
    const hue = h % 360;
    const hue2 = (hue + 60 + ((h >> 3) % 80)) % 360;

    const ax = 18 + (h % 40);
    const ay = 30 + ((h >> 5) % 35);
    const bx = 60 + ((h >> 7) % 35);
    const by = 25 + ((h >> 9) % 50);
    const ang = ((h >> 11) % 60) - 30;

    const parts = seed.split(/[-_/ ]+/).filter(Boolean);
    const initials = (parts.length > 1
        ? parts.slice(0, 2).map(s => s[0]).join('')
        : seed.slice(0, 2)
    ).toUpperCase() || 'PC';
    const slugStamp = seed.slice(0, 18).toUpperCase();

    const bgImage = [
        'linear-gradient(to right, rgba(255,255,255,0.025) 1px, transparent 1px)',
        'linear-gradient(to bottom, rgba(255,255,255,0.025) 1px, transparent 1px)',
        `linear-gradient(${ang + 90}deg, transparent 48%, rgba(255,255,255,0.04) 50%, transparent 52%)`,
        `radial-gradient(circle at ${ax}% ${ay}%, hsl(${hue} 36% 38% / 0.85) 0, transparent 42%)`,
        `radial-gradient(ellipse at ${bx}% ${by}%, hsl(${hue2} 30% 30% / 0.75) 0, transparent 50%)`,
        `linear-gradient(135deg, hsl(${(hue + 120) % 360} 14% 12%), hsl(${hue} 10% 6%))`,
    ].join(', ');

    return (
        <div className={`${styles['build-cover']} ${className}`} aria-label={`Cover for ${seed}`}>
            <div
                className={styles['bc-bg']}
                style={{ backgroundImage: bgImage, backgroundSize: '32px 32px, 32px 32px, auto, auto, auto, auto' }}
            />
            {coverUrl ? (
                <img
                    src={coverUrl}
                    alt={seed}
                    style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'contain' }}
                    loading="lazy"
                />
            ) : (
                <span className={styles['bc-ini']} aria-hidden="true">{initials}</span>
            )}
            {tag && <span className={styles['bc-tag']}>{tag}</span>}
            {stamp && !coverUrl && (
                <div className={styles['bc-stamp']}>
                    <span className={styles['bc-slug']}>{slugStamp}</span>
                </div>
            )}
        </div>
    );
}
