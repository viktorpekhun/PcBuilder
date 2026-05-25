import { useMemo, useState } from "react";
import styles from "./PcBuildPage.module.css";
import type { ComponentDataState } from "./types";
import { MULTI_TYPES, SINGLE_TYPES } from "./types";

interface JsonViewProps {
    componentData: ComponentDataState;
}

export default function JsonView({ componentData }: JsonViewProps) {
    const json = useMemo(() => {
        const build: Record<string, unknown> = {};

        for (const { key } of SINGLE_TYPES) {
            const data = componentData[key];
            build[key] = data ? {
                id: data.id,
                name: data.name,
                price: data.selectedOffer?.price ?? data.averagePrice ?? null,
                store: data.selectedOffer?.storeName ?? null,
            } : null;
        }

        for (const { key } of MULTI_TYPES) {
            build[key] = componentData[key].map((item) => ({
                id: item.componentId,
                name: item.component.name,
                quantity: item.quantity,
                price: item.price,
                store: item.storeName,
            }));
        }

        return build;
    }, [componentData]);

    const jsonStr = JSON.stringify(json, null, 2);
    const [copied, setCopied] = useState(false);

    const handleCopy = () => {
        navigator.clipboard.writeText(jsonStr).then(() => {
            setCopied(true);
            setTimeout(() => setCopied(false), 1500);
        });
    };

    return (
        <div className={styles.jsonView}>
            <div className={styles.jvHead}>
                <span className={styles.scEyebrow}>BUILD · JSON</span>
                <button className={styles.jvCopy} onClick={handleCopy}>
                    {copied ? "✓ COPIED" : "COPY →"}
                </button>
            </div>
            <pre className={styles.jvPre}>{jsonStr}</pre>
        </div>
    );
}
