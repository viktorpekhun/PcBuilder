import { useEffect, useMemo, useState } from "react";
import { buildService } from "../../api/build.service";
import type { IBuildCompatibilityReport, ICompatibilityIssue, IComponentsCompatibility } from "../../types/build.types";
import type { ComponentDataState, SingleKey, MultiKey } from "./types";
import { formatIssue } from "../../utils/compatibilityMessages";
import { deriveDesignRows, type DesignRowKind } from "./compatRows";

import styles from "./CompatibilityCheck.module.css";

const SINGLE_WATTAGE: { key: SingleKey; field: string }[] = [
    { key: "cpu", field: "tdp" },
    { key: "gpu", field: "wattage" },
    { key: "motherboard", field: "wattage" },
    { key: "cpuCooler", field: "wattage" },
];

const MULTI_WATTAGE: { key: MultiKey; field: string; multiplierField?: string }[] = [
    { key: "rams", field: "wattage", multiplierField: "moduleQuantity" },
    { key: "ssds", field: "wattage" },
    { key: "hdds", field: "wattage" },
    { key: "fans", field: "wattage", multiplierField: "moduleCount" },
];

interface CompatibilityCheckProps {
    selectedComponentIds: IComponentsCompatibility;
    componentData: ComponentDataState;
    onCounts?: (counts: { criticalCount: number; warningCount: number; firstCriticalMessage: string | null }) => void;
}

const GLYPH: Record<DesignRowKind, string> = {
    ok: "✓",
    warn: "!",
    fail: "×",
    skip: "○",
};

const KIND_CLASS: Record<DesignRowKind, string | undefined> = {
    ok: styles.rowOk,
    warn: styles.rowWarn,
    fail: styles.rowFail,
    skip: styles.rowSkip,
};

function CompatibilityCheck({ selectedComponentIds, componentData, onCounts }: CompatibilityCheckProps) {
    const [results, setResults] = useState<IBuildCompatibilityReport | null>(null);
    const [loading, setLoading] = useState(false);
    const [, setError] = useState<string | null>(null);
    const [totalWattage, setTotalWattage] = useState(0);
    const [detailsOpen, setDetailsOpen] = useState(false);

    const hasComponents = Object.values(selectedComponentIds).some(
        v => v !== undefined && (Array.isArray(v) ? v.length > 0 : true)
    );

    useEffect(() => {
        if (!componentData) return;
        let wattage = 0;

        for (const { key, field } of SINGLE_WATTAGE) {
            const comp = componentData[key];
            if (comp && (comp as Record<string, unknown>)[field]) {
                wattage += Number((comp as Record<string, unknown>)[field]) || 0;
            }
        }

        for (const { key, field, multiplierField } of MULTI_WATTAGE) {
            for (const item of componentData[key]) {
                const w = Number((item.component as Record<string, unknown>)[field]) || 0;
                const multiplier = multiplierField
                    ? Number((item.component as Record<string, unknown>)[multiplierField]) || 1
                    : 1;
                wattage += w * (item.quantity || 1) * multiplier;
            }
        }

        if (hasComponents) wattage += 150;
        setTotalWattage(wattage);
    }, [componentData, hasComponents]);

    useEffect(() => {
        if (!hasComponents) {
            setResults(null);
            return;
        }
        const check = async () => {
            setLoading(true);
            try {
                const response = await buildService.checkCompatibility(selectedComponentIds);
                setResults(response.data);
                setError(null);
            } catch {
                setError("Failed to check compatibility");
            } finally {
                setLoading(false);
            }
        };
        check();
    }, [selectedComponentIds, hasComponents]);

    const allIssues: ICompatibilityIssue[] = results?.ruleResults?.flatMap(r => r.issues ?? []) ?? [];
    const criticalCount = allIssues.filter(i => i.severity === "Critical").length;
    const warningCount = allIssues.filter(i => i.severity === "Warning").length;

    const firstCritical = allIssues.find(i => i.severity === "Critical");
    const firstCriticalMessage = firstCritical ? formatIssue(firstCritical) : null;

    useEffect(() => {
        onCounts?.({ criticalCount, warningCount, firstCriticalMessage });
    }, [criticalCount, warningCount, firstCriticalMessage, onCounts]);

    const isStrict = results?.isStrictlyCompatible ?? false;
    const hasWarnings = warningCount > 0;

    const overallKind: DesignRowKind =
        !hasComponents ? "skip" :
        !isStrict ? "fail" :
        hasWarnings ? "warn" :
        "ok";

    const overallLabel =
        !hasComponents ? "○ PENDING" :
        loading ? "○ CHECKING" :
        !isStrict ? `× ${criticalCount} FAIL` :
        hasWarnings ? `△ ${warningCount} WARN` :
        "✓ PASS";

    const overallPill =
        overallKind === "ok" ? styles.pillOk :
        overallKind === "warn" ? styles.pillWarn :
        overallKind === "fail" ? styles.pillErr :
        styles.pillDim;

    const psuWattage = componentData.powerSupply
        ? Number((componentData.powerSupply as unknown as { wattage?: number }).wattage ?? 0)
        : 0;

    const designRows = useMemo(
        () => deriveDesignRows(results, componentData),
        [results, componentData]
    );

    return (
        <div className={styles.compat}>
            <div className={styles.compatHead}>
                <span className={styles.eyebrow}>COMPATIBILITY</span>
                <span className={`${styles.pill} ${overallPill}`}>{overallLabel}</span>
            </div>

            <div className={styles.body}>
                <div className={styles.basicGrid}>
                    <span className={styles.basicLbl}>POWER NEEDED</span>
                    <span className={styles.basicVal}>{totalWattage} W</span>
                    <span className={styles.basicLbl}>PSU CAPACITY</span>
                    <span className={`${styles.basicVal} ${psuWattage && psuWattage < totalWattage ? styles.err : ""}`}>
                        {psuWattage ? `${psuWattage} W` : "—"}
                    </span>
                    <span className={styles.basicLbl}>CRITICAL</span>
                    <span className={`${styles.basicVal} ${criticalCount > 0 ? styles.err : ""}`}>{criticalCount}</span>
                    <span className={styles.basicLbl}>WARNINGS</span>
                    <span className={`${styles.basicVal} ${warningCount > 0 ? styles.warn : ""}`}>{warningCount}</span>
                </div>
            </div>

            <div className={styles.rows}>
                {designRows.map(row => (
                    <div key={row.id} className={`${styles.row} ${KIND_CLASS[row.kind]}`}>
                        <span className={styles.rowGlyph}>{GLYPH[row.kind]}</span>
                        <span className={styles.rowLabel}>{row.label}</span>
                        <span className={styles.rowValue}>{row.value}</span>
                    </div>
                ))}
            </div>

            {hasComponents && allIssues.length > 0 && (
                <>
                    <button
                        type="button"
                        className={styles.detailsToggle}
                        onClick={() => setDetailsOpen(o => !o)}
                        aria-expanded={detailsOpen}
                    >
                        <span className={`${styles.detailsCaret} ${detailsOpen ? styles.detailsCaretOpen : ""}`}>▸</span>
                        DETAILS · {allIssues.length} ISSUE{allIssues.length === 1 ? "" : "S"}
                    </button>
                    {detailsOpen && (
                        <ul className={styles.messages}>
                            {allIssues.map((issue, i) => {
                                const cls = issue.severity === "Critical" ? styles.messageErr
                                    : issue.severity === "Warning" ? styles.messageWarn
                                    : styles.messageInfo;
                                const gly = issue.severity === "Critical" ? "×"
                                    : issue.severity === "Warning" ? "!"
                                    : "○";
                                return (
                                    <li key={i} className={`${styles.message} ${cls}`}>
                                        <span className={styles.messageGly}>{gly}</span>
                                        <span className={styles.messageText}>{formatIssue(issue)}</span>
                                    </li>
                                );
                            })}
                        </ul>
                    )}
                </>
            )}
        </div>
    );
}

export default CompatibilityCheck;
