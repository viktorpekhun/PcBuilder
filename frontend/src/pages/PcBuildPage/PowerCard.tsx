import { Fragment } from "react";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState, MultiKey, SingleKey } from "./types";

interface PowerCardProps {
    componentData: ComponentDataState;
}

const SLOT_TAG: Record<SingleKey | MultiKey, string> = {
    cpu: "CPU", gpu: "GPU", motherboard: "M/B",
    rams: "RAM", ssds: "SSD", hdds: "HDD",
    powerSupply: "PSU", cpuCooler: "CLR", pcCase: "CSE", fans: "FAN",
};

const BAR_COLORS = ["#AECF55", "#D9A441", "#6CA0DC", "#9C7CB5", "#5FB58A", "#B86D6D", "#A8A8B2"];

interface SlotPower {
    slot: SingleKey | MultiKey;
    label: string;
    watts: number;
}

function computeSlotPower(componentData: ComponentDataState): SlotPower[] {
    const out: SlotPower[] = [];

    // CPU — tdp
    if (componentData.cpu) {
        const w = Number((componentData.cpu as unknown as { tdp?: number }).tdp ?? 0);
        if (w > 0) out.push({ slot: "cpu", label: SLOT_TAG.cpu, watts: w });
    }

    // GPU, motherboard, cpuCooler — wattage
    (["gpu", "motherboard", "cpuCooler"] as const).forEach((k) => {
        const c = componentData[k];
        if (!c) return;
        const w = Number((c as unknown as { wattage?: number }).wattage ?? 0);
        if (w > 0) out.push({ slot: k, label: SLOT_TAG[k], watts: w });
    });

    // RAM with module multiplier
    let ramW = 0;
    for (const item of componentData.rams) {
        const w = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
        const modules = Number((item.component as unknown as { moduleQuantity?: number }).moduleQuantity ?? 1);
        ramW += w * item.quantity * modules;
    }
    if (ramW > 0) out.push({ slot: "rams", label: SLOT_TAG.rams, watts: ramW });

    // SSD, HDD
    (["ssds", "hdds"] as const).forEach((k) => {
        let w = 0;
        for (const item of componentData[k]) {
            const wEach = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
            w += wEach * item.quantity;
        }
        if (w > 0) out.push({ slot: k, label: SLOT_TAG[k], watts: w });
    });

    // Fans
    let fanW = 0;
    for (const item of componentData.fans) {
        const w = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
        const modules = Number((item.component as unknown as { moduleCount?: number }).moduleCount ?? 1);
        fanW += w * item.quantity * modules;
    }
    if (fanW > 0) out.push({ slot: "fans", label: SLOT_TAG.fans, watts: fanW });

    return out;
}

export default function PowerCard({ componentData }: PowerCardProps) {
    const slots = computeSlotPower(componentData);
    const totalDraw = slots.reduce((a, s) => a + s.watts, 0);
    const psu = Number((componentData.powerSupply as unknown as { wattage?: number } | null)?.wattage ?? 0);

    const loadPct = psu ? (totalDraw / psu) * 100 : 0;
    const kind: "ok" | "warn" | "err" = loadPct > 95 ? "err" : loadPct > 80 ? "warn" : "ok";
    const headroom = psu - totalDraw;
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
