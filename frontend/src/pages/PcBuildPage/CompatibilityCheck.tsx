import { useEffect, useState } from "react";
import { buildService } from "../../api/build.service";
import type { IBuildCompatibilityReport, ICompatibilityIssue, IComponentsCompatibility } from "../../types/build.types";
import type { ComponentDataState, SingleKey, MultiKey } from "./types";
import { formatIssue } from "../../utils/compatibilityMessages";

import styles from "./CompatibilityCheck.module.css";

// --- SVG icons (deduplicated) ---

const LightningIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
         className={styles['icon']} viewBox="0 0 16 16">
        <path d="M5.52.359A.5.5 0 0 1 6 0h4a.5.5 0 0 1 .474.658L8.694 6H12.5a.5.5 0 0 1 .395.807l-7 9a.5.5 0 0 1-.873-.454L6.823 9.5H3.5a.5.5 0 0 1-.48-.641z"/>
    </svg>
);

const ErrorIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
         className={styles['icon']} viewBox="0 0 16 16">
        <path d="M4.54.146A.5.5 0 0 1 4.893 0h6.214a.5.5 0 0 1 .353.146l4.394 4.394a.5.5 0 0 1 .146.353v6.214a.5.5 0 0 1-.146.353l-4.394 4.394a.5.5 0 0 1-.353.146H4.893a.5.5 0 0 1-.353-.146L.146 11.46A.5.5 0 0 1 0 11.107V4.893a.5.5 0 0 1 .146-.353zM5.1 1 1 5.1v5.8L5.1 15h5.8l4.1-4.1V5.1L10.9 1z"/>
        <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
    </svg>
);

const WarningIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
         className={styles['icon']} viewBox="0 0 16 16">
        <path d="M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z"/>
        <path d="M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
    </svg>
);

const CheckIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
         className={styles['icon']} viewBox="0 0 16 16">
        <path d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5z"/>
        <path d="m8.354 10.354 7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0"/>
    </svg>
);

// --- Wattage config ---

const SINGLE_WATTAGE: { key: SingleKey; field: string; multiplierField?: string }[] = [
    { key: 'cpu', field: 'tdp' },
    { key: 'gpu', field: 'wattage' },
    { key: 'motherboard', field: 'wattage' },
    { key: 'cpuCooler', field: 'wattage' },
];

const MULTI_WATTAGE: { key: MultiKey; field: string; multiplierField?: string }[] = [
    { key: 'rams', field: 'wattage', multiplierField: 'moduleQuantity' },
    { key: 'ssds', field: 'wattage' },
    { key: 'hdds', field: 'wattage' },
    { key: 'fans', field: 'wattage', multiplierField: 'moduleCount' },
];

// --- Props ---

interface CompatibilityCheckProps {
    selectedComponentIds: IComponentsCompatibility;
    componentData: ComponentDataState;
}

function CompatibilityCheck({ selectedComponentIds, componentData }: CompatibilityCheckProps) {
    const [compatibilityResults, setCompatibilityResults] = useState<IBuildCompatibilityReport | null>(null);
    const [loading, setLoading] = useState(false);
    const [, setError] = useState<string | null>(null);
    const [totalWattage, setTotalWattage] = useState(0);

    const hasComponents = Object.values(selectedComponentIds).some(
        value => value !== undefined && (Array.isArray(value) ? value.length > 0 : true)
    );

    // Calculate wattage
    useEffect(() => {
        if (!componentData) return;

        let wattage = 0;

        for (const { key, field } of SINGLE_WATTAGE) {
            const comp = componentData[key];
            if (comp && comp[field]) {
                wattage += Number(comp[field]) || 0;
            }
        }

        for (const { key, field, multiplierField } of MULTI_WATTAGE) {
            for (const item of componentData[key]) {
                const w = Number(item.component[field]) || 0;
                const multiplier = multiplierField ? (Number(item.component[multiplierField]) || 1) : 1;
                wattage += w * (item.quantity || 1) * multiplier;
            }
        }

        if (hasComponents) {
            wattage += 150;
        }

        setTotalWattage(wattage);
    }, [componentData, hasComponents]);

    // Check compatibility
    useEffect(() => {
        if (!hasComponents) {
            setCompatibilityResults(null);
            return;
        }

        const checkCompatibility = async () => {
            setLoading(true);
            try {
                const response = await buildService.checkCompatibility(selectedComponentIds);
                setCompatibilityResults(response.data);
                setError(null);
            } catch {
                setError("Failed to check compatibility");
            } finally {
                setLoading(false);
            }
        };

        checkCompatibility();
    }, [selectedComponentIds, hasComponents]);

    // Count issues by severity
    const allIssues: ICompatibilityIssue[] =
        compatibilityResults?.ruleResults?.flatMap(r => r.issues ?? []) ?? [];
    const criticalCount = allIssues.filter(i => i.severity === 'Critical').length;
    const warningCount  = allIssues.filter(i => i.severity === 'Warning').length;

    // Determine overall status
    const isStrict = compatibilityResults?.isStrictlyCompatible ?? false;
    const hasWarnings = warningCount > 0;

    const statusClass = !hasComponents
        ? styles['status-warning']
        : !isStrict
            ? styles['status-error']
            : hasWarnings
                ? styles['status-warning']
                : styles['status-success'];

    const statusContent = !hasComponents
        ? { icon: <WarningIcon />, text: 'Компоненти не вибрано' }
        : !isStrict
            ? { icon: <ErrorIcon />, text: 'Виявлено несумісні комплектуючі' }
            : hasWarnings
                ? { icon: <WarningIcon />, text: 'Виявлено потенційні проблеми' }
                : loading
                    ? { icon: null, text: 'Перевірка сумісності...' }
                    : { icon: <CheckIcon />, text: 'Всі компоненти сумісні' };

    const psuWattage = componentData.powerSupply
        ? Number(componentData.powerSupply.wattage ?? 0)
        : 0;

    return (
        <div className={styles['compatibility-container']}>
            <h2>Перевірка сумісності</h2>

            <div className={styles['basic-info-display']}>
                <div className={`${styles['basic-info']} ${styles['wattage-info']}`}>
                    <LightningIcon />
                    <div className={styles['info-text']}>
                        Необхідна потужність(із запасом): {totalWattage} Вт
                    </div>
                </div>
                <div className={`${styles['basic-info']} ${styles['wattage-info']}`}>
                    <LightningIcon />
                    <div className={styles['info-text']}>
                        Потужність БЖ: {psuWattage} Вт
                    </div>
                </div>
                <div className={`${styles['basic-info']} ${styles['problems-info']}`}>
                    <ErrorIcon />
                    <div className={styles['info-text']}>
                        Кількість критичних проблем у збірці: {criticalCount}
                    </div>
                </div>
                <div className={`${styles['basic-info']} ${styles['warnings-info']}`}>
                    <WarningIcon />
                    <div className={styles['info-text']}>
                        Кількість потенційних проблем: {warningCount}
                    </div>
                </div>
            </div>

            <div className={`${styles['compatibility-status']} ${statusClass}`}>
                {statusContent.icon}
                <p>{statusContent.text}</p>
            </div>

            {hasComponents && allIssues.length > 0 && (
                <ul className={styles['compatibility-messages']}>
                    {allIssues.map((issue, i) => (
                        <li
                            key={i}
                            className={`${styles['compatibility-message']} ${
                                issue.severity === 'Critical' ? styles['message-error'] :
                                issue.severity === 'Warning'  ? styles['message-warning'] :
                                                                styles['message-info']
                            }`}
                        >
                            {formatIssue(issue)}
                        </li>
                    ))}
                </ul>
            )}

            {!hasComponents && (
                <ul className={styles['compatibility-messages']}>
                    <li className={`${styles['compatibility-message']} ${styles['message-warning']}`}>
                        Додайте компоненти до збірки для перевірки сумісності
                    </li>
                </ul>
            )}
        </div>
    );
}

export default CompatibilityCheck;
