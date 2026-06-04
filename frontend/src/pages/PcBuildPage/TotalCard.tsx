import { useTranslation } from "react-i18next";
import styles from "./PcBuildPage.module.css";

interface TotalCardProps {
    total: number;
    partCount: number;
}

function fmt(n: number): string {
    if (!n) return "0";
    return Math.round(n).toLocaleString("uk-UA");
}

export default function TotalCard({ total, partCount }: TotalCardProps) {
    const { t } = useTranslation();

    return (
        <div className={styles.sideCard}>
            <div className={styles.scHead}>
                <span className={styles.scEyebrow}>{t("pcBuildPage.totalCard.eyebrow")}</span>
                <span className={styles.scEyebrow} style={{ color: "var(--fg-3)" }}>{t("pcBuildPage.totalCard.currency")}</span>
            </div>
            <div className={styles.scBody}>
                <div className={styles.totalBig}>
                    <span className={styles.ccy}>₴</span>{fmt(total)}
                </div>
                <div className={styles.totalSub}>
                    <span>{t("pcBuildPage.totalCard.part", { count: partCount })}</span>
                </div>
            </div>
        </div>
    );
}
