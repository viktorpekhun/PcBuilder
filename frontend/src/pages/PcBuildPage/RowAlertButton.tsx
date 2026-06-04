import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import useAuth from "../../hooks/useAuth";
import { priceAlertService } from "../../api/priceAlert.service";
import type { ComponentType } from "../../types/component.types";
import type { IPriceAlert } from "../../types/priceAlert.types";
import styles from "./PcBuildPage.module.css";

const DEFAULT_THRESHOLD = 5;

const COMPONENT_TYPE_MAP: Record<string, ComponentType> = {
    cpu: "Cpu", gpu: "Gpu", ram: "Ram",
    motherboard: "Motherboard", cpucooler: "CpuCooler",
    pccase: "PcCase", powersupply: "PowerSupply",
    ssd: "Ssd", hdd: "Hdd", fan: "Fan",
};

function normalize(raw: string): ComponentType {
    return COMPONENT_TYPE_MAP[raw.toLowerCase()] ?? (raw as ComponentType);
}

interface Props {
    componentId: string;
    componentType: ComponentType | string;
}

export default function RowAlertButton({ componentId, componentType: rawType }: Props) {
    const { t } = useTranslation();
    const componentType = normalize(rawType);
    const { auth } = useAuth();
    const [alert, setAlert] = useState<IPriceAlert | null>(null);
    const [busy, setBusy] = useState(false);

    const isLoggedIn = Boolean(auth?.userId || auth?.accessToken);

    useEffect(() => {
        if (!isLoggedIn) return;
        let cancelled = false;
        (async () => {
            try {
                const existing = await priceAlertService.getForComponent(componentId, componentType);
                if (!cancelled) setAlert(existing);
            } catch {
                if (!cancelled) setAlert(null);
            }
        })();
        return () => { cancelled = true; };
    }, [componentId, componentType, isLoggedIn]);

    if (!isLoggedIn) return null;

    const handleClick = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (busy) return;
        setBusy(true);
        try {
            if (alert) {
                await priceAlertService.unsubscribe(alert.id);
                setAlert(null);
            } else {
                await priceAlertService.subscribe({
                    componentId, componentType, thresholdPercent: DEFAULT_THRESHOLD,
                });
                const refreshed = await priceAlertService.getForComponent(componentId, componentType);
                setAlert(refreshed);
            }
        } catch {
            // ignore - chip stays in prior state
        } finally {
            setBusy(false);
        }
    };

    const active = !!alert;
    return (
        <button
            type="button"
            className={`${styles.rowToolBtn} ${active ? styles.rowToolBtnActive : ""}`}
            onClick={handleClick}
            disabled={busy}
            title={active
                ? t("pcBuildPage.rowAlert.subscribed", { pct: alert!.thresholdPercent })
                : t("pcBuildPage.rowAlert.subscribe")}
        >
            <span className={styles.toolGly}>△</span> {t("pcBuildPage.rowAlert.label")}
        </button>
    );
}
