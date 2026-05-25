import { Fragment } from "react";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState } from "./types";
import { usePowerStats } from "./hooks";
import { BAR_COLORS } from "./constants";

interface PowerCardProps {
    componentData: ComponentDataState;
}

export default function PowerCard({ componentData }: PowerCardProps) {
    const { slots, totalDraw, psu, loadPct, headroom, kind } = usePowerStats(componentData);

    const fillClass =
        kind === "ok" ? styles.gaugeFillOk :
        kind === "warn" ? styles.gaugeFillWarn :
        styles.gaugeFillErr;

    const pillClass =
        psu === 0 ? styles.pillWarn :
        kind === "ok" ? styles.pillOk :
        kind === "warn" ? styles.pillWarn :
        styles.pillErr;

    const pillLabel =
        psu === 0 ? "○ AWAITING PSU" :
        kind === "ok" ? "✓ HEALTHY" :
        kind === "warn" ? "△ TIGHT" :
        "× OVER";

    return (
        <div className={styles.sideCard}>
            <div className={styles.scHead}>
                <span className={styles.scEyebrow}>POWER</span>
                <span className={`${styles.pill} ${pillClass}`}>{pillLabel}</span>
            </div>
            <div className={styles.scBody}>
                {psu === 0 ? (
                    <div className={styles.gaugeEmpty}>
                        ○ Додайте БЖ для розрахунку запасу.<br />
                        Поточне споживання: <span style={{ color: "var(--fg-0)" }}>{totalDraw} W</span>
                    </div>
                ) : (
                    <>
                        <div className={styles.gauge}>
                            <div className={styles.gaugeHead}>
                                <span><span className={styles.big}>{totalDraw}</span> <span className={styles.dim}>W est.</span></span>
                                <span className={styles.dim}>/ {psu} W max</span>
                            </div>
                            <div className={styles.gaugeBar}>
                                <div className={`${styles.gaugeFill} ${fillClass}`}
                                    style={{ width: `${Math.min(loadPct, 100)}%` }} />
                                <div className={styles.gaugeTick} style={{ left: "80%" }} />
                                <div className={styles.gaugeTick} style={{ left: "95%" }} />
                            </div>
                            <div className={styles.gaugeLegend}>
                                <span>0 W</span>
                                <span>SAFE 80%</span>
                                <span>MAX 95%</span>
                                <span>{psu} W</span>
                            </div>
                        </div>

                        {slots.length > 0 && totalDraw > 0 && (
                            <div className={styles.pbar}>
                                {slots.map((s, i) => (
                                    <span
                                        key={s.slot}
                                        style={{
                                            width: `${(s.watts / totalDraw) * 100}%`,
                                            background: BAR_COLORS[i % BAR_COLORS.length],
                                        }}
                                        title={`${s.label} · ${s.watts} W`}
                                    />
                                ))}
                            </div>
                        )}

                        {slots.length > 0 && (
                            <div className={styles.breakdown}>
                                {slots.map((s, i) => (
                                    <Fragment key={s.slot}>
                                        <span className={styles.lbl}>
                                            <span className={styles.swatch}
                                                style={{ background: BAR_COLORS[i % BAR_COLORS.length] }} />
                                            {s.label}
                                        </span>
                                        <span className={styles.val}>{s.watts} W</span>
                                    </Fragment>
                                ))}
                                <span className={`${styles.lbl} ${styles.totalRow}`}>HEADROOM</span>
                                <span className={`${styles.val} ${styles.totalRow}`}
                                    style={{ color: kind === "ok" ? "var(--ok)" : kind === "warn" ? "var(--warn)" : "var(--err)" }}>
                                    {headroom} W
                                </span>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}
