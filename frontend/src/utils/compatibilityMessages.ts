import type { ICompatibilityIssue } from "../types/build.types";
import i18n from "../i18n";

function normalizeParams(code: string, raw: Record<string, string>): Record<string, string> {
    const p = { ...raw };

    // HddMotherboardSocketRule sends MbSataSlots; SsdMotherboardSocketRule sends FreeSataSlots.
    if (code === "SATA_SLOTS_EXCEEDED" && !p.FreeSataSlots && p.MbSataSlots) {
        p.FreeSataSlots = p.MbSataSlots;
    }

    // Backend always sends MissingConnectors (may be empty). Translations use {{ConnectorSuffix}}.
    if (code === "PSU_GPU_CONNECTOR_MISSING" || code === "PSU_CPU_CONNECTOR_MISSING") {
        p.ConnectorSuffix = p.MissingConnectors ? `: ${p.MissingConnectors}` : "";
    }

    return p;
}

export function formatIssue(issue: ICompatibilityIssue): string {
    const key = `pcBuildPage.compatibility.issues.${issue.code}`;
    const params = normalizeParams(issue.code, issue.parameters as Record<string, string>);
    const result = i18n.t(key, params);
    return result !== key ? result : issue.code;
}
