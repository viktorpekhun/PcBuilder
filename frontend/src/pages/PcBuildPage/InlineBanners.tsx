import { useEffect, useState } from "react";
import styles from "./PcBuildPage.module.css";

interface InlineBannersProps {
    criticalCount: number;
    loadPct: number;
    psu: number;
    firstCriticalMessage: string | null;
}

export default function InlineBanners({
    criticalCount, loadPct, psu, firstCriticalMessage,
}: InlineBannersProps) {
    const [conflictDismissed, setConflictDismissed] = useState(false);
    const [psuDismissed, setPsuDismissed] = useState(false);

    // Reset dismissal when the underlying condition changes
    useEffect(() => {
        if (criticalCount === 0) setConflictDismissed(false);
    }, [criticalCount]);

    useEffect(() => {
        if (!(psu > 0 && loadPct > 80 && loadPct <= 95)) setPsuDismissed(false);
    }, [psu, loadPct]);

    const showConflict = criticalCount > 0 && !conflictDismissed;
    const showPsuMargin = psu > 0 && loadPct > 80 && loadPct <= 95 && !psuDismissed;

    if (!showConflict && !showPsuMargin) return null;

    return (
        <>
            {showConflict && (
                <div className={`${styles.banner} ${styles.bannerErr}`}>
                    <span className={styles.bannerGlyph}>×</span>
                    <div className={styles.bannerBody}>
                        <div className={styles.bannerTitle}>
                            INCOMPATIBLE · {criticalCount} CHECK{criticalCount === 1 ? "" : "S"} FAILED
                        </div>
                        {firstCriticalMessage && (
                            <div className={styles.bannerText}>{firstCriticalMessage}</div>
                        )}
                    </div>
                    <button
                        type="button"
                        className={styles.bannerClose}
                        onClick={() => setConflictDismissed(true)}
                        aria-label="Dismiss"
                    >×</button>
                </div>
            )}
            {showPsuMargin && (
                <div className={`${styles.banner} ${styles.bannerWarn}`}>
                    <span className={styles.bannerGlyph}>!</span>
                    <div className={styles.bannerBody}>
                        <div className={styles.bannerTitle}>PSU MARGIN LOW</div>
                        <div className={styles.bannerText}>
                            Очікуване навантаження перевищує 80% потужності БЖ. Розгляньте оновлення.
                        </div>
                    </div>
                    <button
                        type="button"
                        className={styles.bannerClose}
                        onClick={() => setPsuDismissed(true)}
                        aria-label="Dismiss"
                    >×</button>
                </div>
            )}
        </>
    );
}
