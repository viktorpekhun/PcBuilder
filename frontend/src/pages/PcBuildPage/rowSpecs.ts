import type { SingleKey, MultiKey } from "./types";
import type { IPcCaseFormFactor } from "../../types/component.types";

type RowKey = SingleKey | MultiKey;

function nonEmpty(...vals: (string | number | null | undefined)[]): string[] {
    return vals
        .filter(v => v !== null && v !== undefined && v !== "")
        .map(v => String(v).toUpperCase());
}

export function getRowSpec(type: RowKey, detail: Record<string, unknown> | null | undefined): string[] {
    if (!detail) return [];
    const d = detail;

    switch (type) {
        case "cpu":
            return nonEmpty(
                d.socket as string,
                d.cores ? `${d.cores} CORES` : undefined,
                d.tdp ? `${d.tdp} W TDP` : undefined,
            );

        case "gpu":
            return nonEmpty(
                d.memory ? `${d.memory} GB` : undefined,
                d.memoryType as string,
                d.sizeLength ? `${d.sizeLength} MM` : undefined,
            );

        case "motherboard":
            return nonEmpty(
                d.socket as string,
                d.formFactor as string,
                d.dimmType as string,
            );

        case "rams":
            return nonEmpty(
                d.type as string,
                d.frequency ? `${d.frequency} MT/S` : undefined,
                d.capacity ? `${d.capacity} GB` : undefined,
            );

        case "ssds":
            return nonEmpty(
                d.formFactor as string,
                d.interface as string,
                d.capacity ? `${d.capacity} GB` : undefined,
            );

        case "hdds":
            return nonEmpty(
                d.formFactor as string,
                d.interface as string,
                d.capacity ? `${d.capacity} GB` : undefined,
            );

        case "powerSupply":
            return nonEmpty(
                d.wattage ? `${d.wattage} W` : undefined,
                d.efficiencyStandart as string,
            );

        case "cpuCooler":
            return nonEmpty(
                d.maxPowerDissipation ? `${d.maxPowerDissipation} W` : undefined,
                d.height ? `${d.height} MM` : undefined,
            );

        case "pcCase": {
            const ffs = (d.pcCaseFormFactors as IPcCaseFormFactor[] | undefined) ?? [];
            const ffStr = ffs.map(f => String(f.name ?? "")).filter(Boolean).join("/");
            return nonEmpty(
                ffStr || undefined,
                d.maxGpuLength ? `${d.maxGpuLength} MM` : undefined,
            );
        }

        case "fans":
            return nonEmpty(
                d.sizeLength ? `${d.sizeLength} MM` : undefined,
            );

        default:
            return [];
    }
}
