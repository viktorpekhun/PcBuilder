import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
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
    const { t } = useTranslation();
    const [conflictDismissed, setConflictDismissed] = useState(false);
    const [psuDismissed, setPsuDismissed] = useState(false);

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
                            {criticalCount === 1
                                ? t("pcBuildPage.inlineBanners.incompatibleTitle", { count: criticalCount })
                                : t("pcBuildPage.inlineBanners.incompatibleTitle_plural", { count: criticalCount })}
                        </div>
                        {firstCriticalMessage && (
                            <div className={styles.bannerText}>{firstCriticalMessage}</div>
                        )}
                    </div>
                    <button
                        type="button"
                        className={styles.bannerClose}
                        onClick={() => setConflictDismissed(true)}
                        aria-label={t("pcBuildPage.inlineBanners.dismiss")}
                    >×</button>
                </div>
            )}
            {showPsuMargin && (
                <div className={`${styles.banner} ${styles.bannerWarn}`}>
                    <span className={styles.bannerGlyph}>!</span>
                    <div className={styles.bannerBody}>
                        <div className={styles.bannerTitle}>{t("pcBuildPage.inlineBanners.psuMarginTitle")}</div>
                        <div className={styles.bannerText}>
                            {t("pcBuildPage.inlineBanners.psuMarginText")}
                        </div>
                    </div>
                    <button
                        type="button"
                        className={styles.bannerClose}
                        onClick={() => setPsuDismissed(true)}
                        aria-label={t("pcBuildPage.inlineBanners.dismiss")}
                    >×</button>
                </div>
            )}
        </>
    );
}
