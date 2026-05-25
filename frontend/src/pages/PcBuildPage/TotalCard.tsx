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
    return (
        <div className={styles.sideCard}>
            <div className={styles.scHead}>
                <span className={styles.scEyebrow}>BUILD TOTAL</span>
                <span className={styles.scEyebrow} style={{ color: "var(--fg-3)" }}>₴ UAH</span>
            </div>
            <div className={styles.scBody}>
                <div className={styles.totalBig}>
                    <span className={styles.ccy}>₴</span>{fmt(total)}
                </div>
                <div className={styles.totalSub}>
                    <span>{partCount} {partCount === 1 ? "позиція" : "позицій"}</span>
                </div>
            </div>
        </div>
    );
}
