import type { SingleKey, MultiKey } from "./types";

export const SLOT_TAG: Record<SingleKey | MultiKey, string> = {
    cpu: "CPU",
    gpu: "GPU",
    motherboard: "M/B",
    rams: "RAM",
    ssds: "SSD",
    hdds: "HDD",
    powerSupply: "PSU",
    cpuCooler: "CLR",
    pcCase: "CSE",
    fans: "FAN",
};

export const BAR_COLORS = ["#AECF55", "#D9A441", "#6CA0DC", "#9C7CB5", "#5FB58A", "#B86D6D", "#A8A8B2"];
