import { useMemo } from "react";
import type { ComponentDataState, MultiKey, SingleKey } from "./types";
import { SLOT_TAG } from "./constants";

export interface SlotPower {
    slot: SingleKey | MultiKey;
    label: string;
    watts: number;
}

export interface PowerStats {
    slots: SlotPower[];
    totalDraw: number;
    psu: number;
    loadPct: number;
    headroom: number;
    kind: "ok" | "warn" | "err";
}

function computeSlotPower(componentData: ComponentDataState): SlotPower[] {
    const out: SlotPower[] = [];

    if (componentData.cpu) {
        const w = Number((componentData.cpu as unknown as { tdp?: number }).tdp ?? 0);
        if (w > 0) out.push({ slot: "cpu", label: SLOT_TAG.cpu, watts: w });
    }

    (["gpu", "motherboard", "cpuCooler"] as const).forEach((k) => {
        const c = componentData[k];
        if (!c) return;
        const w = Number((c as unknown as { wattage?: number }).wattage ?? 0);
        if (w > 0) out.push({ slot: k, label: SLOT_TAG[k], watts: w });
    });

    let ramW = 0;
    for (const item of componentData.rams) {
        const w = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
        const modules = Number((item.component as unknown as { moduleQuantity?: number }).moduleQuantity ?? 1);
        ramW += w * item.quantity * modules;
    }
    if (ramW > 0) out.push({ slot: "rams", label: SLOT_TAG.rams, watts: ramW });

    (["ssds", "hdds"] as const).forEach((k) => {
        let w = 0;
        for (const item of componentData[k]) {
            const wEach = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
            w += wEach * item.quantity;
        }
        if (w > 0) out.push({ slot: k, label: SLOT_TAG[k], watts: w });
    });

    let fanW = 0;
    for (const item of componentData.fans) {
        const w = Number((item.component as unknown as { wattage?: number }).wattage ?? 0);
        const modules = Number((item.component as unknown as { moduleCount?: number }).moduleCount ?? 1);
        fanW += w * item.quantity * modules;
    }
    if (fanW > 0) out.push({ slot: "fans", label: SLOT_TAG.fans, watts: fanW });

    return out;
}

export function usePowerStats(componentData: ComponentDataState): PowerStats {
    return useMemo(() => {
        const slots = computeSlotPower(componentData);
        const totalDraw = slots.reduce((a, s) => a + s.watts, 0);
        const psu = Number((componentData.powerSupply as unknown as { wattage?: number } | null)?.wattage ?? 0);
        const loadPct = psu ? (totalDraw / psu) * 100 : 0;
        const headroom = psu - totalDraw;
        const kind: "ok" | "warn" | "err" = loadPct > 95 ? "err" : loadPct > 80 ? "warn" : "ok";
        return { slots, totalDraw, psu, loadPct, headroom, kind };
    }, [componentData]);
}
