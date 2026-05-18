import type { IBuildCompatibilityReport, ICompatibilityIssue } from "../../types/build.types";
import type { ComponentDataState } from "./types";
import type {
    ICpuDetail,
    IGpuDetail,
    IMotherboardDetail,
    IRamDetail,
    IPcCaseDetail,
    ICpuCoolerDetail,
    IPowerSupplyDetail,
} from "../../types/component.types";

export type DesignRowKind = "ok" | "warn" | "fail" | "skip";

export interface DesignRow {
    id: string;
    label: string;
    kind: DesignRowKind;
    value: string;
}

const FAIL_CODES = {
    cpuSocket: ["SOCKET_MISMATCH", "COOLER_SOCKET_MISMATCH"],
    ramType: ["RAM_TYPE_MISMATCH", "RAM_MIXED_FREQUENCIES", "RAM_FREQUENCY_EXCEEDED"],
    ramSlots: ["RAM_SLOTS_EXCEEDED", "RAM_CAPACITY_EXCEEDED"],
    gpuLength: ["GPU_LENGTH_EXCEEDS_CASE", "GPU_PCIE_VERSION", "GPU_PCIE_LANE"],
    cooler: ["COOLER_TDP_INSUFFICIENT", "COOLER_CASE_FIT"],
    psu: ["PSU_INSUFFICIENT", "PSU_GPU_CONNECTOR_MISSING", "PSU_CPU_CONNECTOR_MISSING"],
    m2: ["M2_SLOTS_EXCEEDED", "M2_SLOTS_MISSING", "NVME_VERSION_MISMATCH"],
};

const WARN_CODES = {
    psu: ["PSU_OVERKILL"],
};

const DASH = "—";

function pickKind(
    issues: ICompatibilityIssue[],
    failCodes: string[],
    warnCodes: string[],
    canEvaluate: boolean
): DesignRowKind {
    const hasFail = issues.some(i => failCodes.includes(i.code) && i.severity === "Critical");
    if (hasFail) return "fail";
    const hasWarn = issues.some(
        i =>
            warnCodes.includes(i.code) ||
            (failCodes.includes(i.code) && i.severity === "Warning")
    );
    if (hasWarn) return "warn";
    if (!canEvaluate) return "skip";
    return "ok";
}

export function deriveDesignRows(
    report: IBuildCompatibilityReport | null,
    componentData: ComponentDataState
): DesignRow[] {
    const issues: ICompatibilityIssue[] = report?.ruleResults?.flatMap(r => r.issues ?? []) ?? [];

    const cpu = componentData.cpu as unknown as ICpuDetail | null;
    const gpu = componentData.gpu as unknown as IGpuDetail | null;
    const mb = componentData.motherboard as unknown as IMotherboardDetail | null;
    const pcCase = componentData.pcCase as unknown as IPcCaseDetail | null;
    const cooler = componentData.cpuCooler as unknown as ICpuCoolerDetail | null;
    const psu = componentData.powerSupply as unknown as IPowerSupplyDetail | null;
    const rams = componentData.rams ?? [];
    const ssds = componentData.ssds ?? [];

    // CPU SOCKET
    const cpuSocketCan = !!(cpu && mb);
    const cpuSocketKind = pickKind(issues, FAIL_CODES.cpuSocket, [], cpuSocketCan);
    const cpuSocketValue = cpuSocketCan
        ? `${cpu!.socket ?? "?"} ↔ ${mb!.socket ?? "?"}`
        : DASH;

    // RAM TYPE
    const ramTypeCan = !!(mb && rams.length > 0);
    const firstRam = rams[0]?.component as IRamDetail | undefined;
    const ramTypeKind = pickKind(issues, FAIL_CODES.ramType, [], ramTypeCan);
    const ramTypeValue = ramTypeCan
        ? `${mb!.dimmType ?? "?"} ↔ ${firstRam?.type ?? "?"}`
        : DASH;

    // RAM SLOTS
    const ramSlotsCan = !!(mb && rams.length > 0);
    const totalModules = rams.reduce((sum, r) => {
        const ram = r.component as IRamDetail;
        const modPerKit = Number(ram?.moduleQuantity ?? 1) || 1;
        return sum + (r.quantity || 1) * modPerKit;
    }, 0);
    const ramSlotsKind = pickKind(issues, FAIL_CODES.ramSlots, [], ramSlotsCan);
    const ramSlotsValue = ramSlotsCan
        ? `${totalModules} / ${mb!.dimmSlots ?? "?"} occupied`
        : DASH;

    // GPU LENGTH
    const gpuLengthCan = !!(gpu && pcCase);
    const gpuLengthKind = pickKind(issues, FAIL_CODES.gpuLength, [], gpuLengthCan);
    const gpuLengthValue = gpuLengthCan
        ? `${gpu!.sizeLength ?? "?"} / ${pcCase!.maxGpuLength ?? "?"} mm`
        : DASH;

    // COOLER
    const coolerCan = !!cooler;
    const coolerKind = pickKind(issues, FAIL_CODES.cooler, [], coolerCan);
    const coolerValue = coolerCan
        ? `${cooler!.maxPowerDissipation ?? "?"} W · ${cooler!.height ?? "?"} mm`
        : DASH;

    // PSU HEADROOM
    const psuCan = !!psu;
    let psuValue = DASH;
    if (psuCan) {
        const wattage = Number(psu!.wattage ?? 0);
        const needed = computeSystemWattage(componentData);
        const headroom = wattage - needed;
        const pct = wattage > 0 ? Math.round((needed / wattage) * 100) : 0;
        psuValue = `${headroom} W · ${pct}%`;
    }
    const psuKind = pickKind(issues, FAIL_CODES.psu, WARN_CODES.psu, psuCan);

    // FORM FACTOR — no direct code, derive
    const formFactorCan = !!(mb && pcCase);
    let formFactorKind: DesignRowKind = "skip";
    let formFactorValue = DASH;
    if (formFactorCan) {
        const mbFF = String(mb!.formFactor ?? "");
        const caseFFs = (pcCase!.pcCaseFormFactors ?? []).map(f => String(f.name ?? ""));
        const ok = !!mbFF && caseFFs.some(f => f.toLowerCase() === mbFF.toLowerCase());
        formFactorKind = ok ? "ok" : "fail";
        formFactorValue = `${mbFF || "?"} ↔ ${caseFFs.join("/") || "?"}`;
    }

    // M.2 SLOTS
    const m2Can = !!(mb && ssds.length > 0);
    const ssdCount = ssds.reduce((s, r) => s + (r.quantity || 1), 0);
    const m2SlotCount = mb?.m2Slots?.reduce((s, slot) => s + (Number(slot.quantity) || 1), 0) ?? 0;
    const m2Kind = pickKind(issues, FAIL_CODES.m2, [], m2Can);
    const m2Free = Math.max(0, m2SlotCount - ssdCount);
    const m2Value = m2Can
        ? `${ssdCount} used · ${m2Free} free`
        : DASH;

    return [
        { id: "cpuSocket", label: "CPU SOCKET", kind: cpuSocketKind, value: cpuSocketValue },
        { id: "ramType", label: "RAM TYPE", kind: ramTypeKind, value: ramTypeValue },
        { id: "ramSlots", label: "RAM SLOTS", kind: ramSlotsKind, value: ramSlotsValue },
        { id: "gpuLength", label: "GPU LENGTH", kind: gpuLengthKind, value: gpuLengthValue },
        { id: "cooler", label: "COOLER", kind: coolerKind, value: coolerValue },
        { id: "psu", label: "PSU HEADROOM", kind: psuKind, value: psuValue },
        { id: "formFactor", label: "FORM FACTOR", kind: formFactorKind, value: formFactorValue },
        { id: "m2", label: "M.2 SLOTS", kind: m2Kind, value: m2Value },
    ];
}

function computeSystemWattage(componentData: ComponentDataState): number {
    let w = 0;
    const single: Array<[Record<string, unknown> | null, string]> = [
        [componentData.cpu as unknown as Record<string, unknown> | null, "tdp"],
        [componentData.gpu as unknown as Record<string, unknown> | null, "wattage"],
        [componentData.motherboard as unknown as Record<string, unknown> | null, "wattage"],
        [componentData.cpuCooler as unknown as Record<string, unknown> | null, "wattage"],
    ];
    for (const [c, field] of single) {
        if (!c) continue;
        w += Number(c[field]) || 0;
    }

    const multiSpecs: Array<{ list: Array<{ component: Record<string, unknown>; quantity?: number }>; field: string; multField?: string }> = [
        { list: componentData.rams as unknown as Array<{ component: Record<string, unknown>; quantity?: number }>, field: "wattage", multField: "moduleQuantity" },
        { list: componentData.ssds as unknown as Array<{ component: Record<string, unknown>; quantity?: number }>, field: "wattage" },
        { list: componentData.hdds as unknown as Array<{ component: Record<string, unknown>; quantity?: number }>, field: "wattage" },
        { list: componentData.fans as unknown as Array<{ component: Record<string, unknown>; quantity?: number }>, field: "wattage", multField: "moduleCount" },
    ];
    for (const { list, field, multField } of multiSpecs) {
        for (const item of list) {
            const wattage = Number(item.component[field]) || 0;
            const multiplier = multField ? (Number(item.component[multField]) || 1) : 1;
            w += wattage * (item.quantity || 1) * multiplier;
        }
    }
    const has = Object.entries(componentData).some(([, v]) => v && (Array.isArray(v) ? v.length > 0 : true));
    if (has) w += 150;
    return w;
}
