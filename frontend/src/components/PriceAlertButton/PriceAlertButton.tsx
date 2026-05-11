import { useEffect, useRef, useState } from "react";
import { Button } from "../Button/Button";
import useAuth from "../../hooks/useAuth";
import { priceAlertService } from "../../api/priceAlert.service";
import type { ComponentType } from "../../types/component.types";
import type { IPriceAlert } from "../../types/priceAlert.types";
import styles from "./PriceAlertButton.module.css";

interface Props {
    componentId: string;
    componentType: ComponentType | string;
}

const DEFAULT_THRESHOLD = 5;

const COMPONENT_TYPE_MAP: Record<string, ComponentType> = {
    cpu: "Cpu",
    gpu: "Gpu",
    ram: "Ram",
    motherboard: "Motherboard",
    cpucooler: "CpuCooler",
    pccase: "PcCase",
    powersupply: "PowerSupply",
    ssd: "Ssd",
    hdd: "Hdd",
    fan: "Fan",
};

function normalizeComponentType(raw: string): ComponentType {
    const key = raw.toLowerCase();
    return COMPONENT_TYPE_MAP[key] ?? (raw as ComponentType);
}

export default function PriceAlertButton({ componentId, componentType: rawComponentType }: Props) {
    const componentType = normalizeComponentType(rawComponentType);
    const { auth } = useAuth();
    const [alert, setAlert] = useState<IPriceAlert | null>(null);
    const [loading, setLoading] = useState(true);
    const [open, setOpen] = useState(false);
    const [threshold, setThreshold] = useState<number>(DEFAULT_THRESHOLD);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const wrapperRef = useRef<HTMLDivElement>(null);

    const isLoggedIn = Boolean(auth?.userId || auth?.accessToken);

    useEffect(() => {
        if (!isLoggedIn) {
            setLoading(false);
            return;
        }
        let cancelled = false;
        (async () => {
            try {
                const existing = await priceAlertService.getForComponent(componentId, componentType);
                if (cancelled) return;
                setAlert(existing);
                if (existing) setThreshold(existing.thresholdPercent);
            } catch {
                if (!cancelled) setAlert(null);
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();
        return () => { cancelled = true; };
    }, [componentId, componentType, isLoggedIn]);

    useEffect(() => {
        if (!open) return;
        const handler = (e: MouseEvent) => {
            if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
                setOpen(false);
                setError(null);
            }
        };
        document.addEventListener("mousedown", handler);
        return () => document.removeEventListener("mousedown", handler);
    }, [open]);

    if (!isLoggedIn || loading) return null;

    const handleSubscribe = async () => {
        if (threshold <= 0 || threshold > 100) {
            setError("Введіть значення від 0.1 до 100");
            return;
        }
        setBusy(true);
        setError(null);
        try {
            await priceAlertService.subscribe({ componentId, componentType, thresholdPercent: threshold });
            const refreshed = await priceAlertService.getForComponent(componentId, componentType);
            setAlert(refreshed);
            setOpen(false);
        } catch {
            setError("Не вдалося підписатися. Спробуйте пізніше.");
        } finally {
            setBusy(false);
        }
    };

    const handleUnsubscribe = async () => {
        if (!alert) return;
        setBusy(true);
        setError(null);
        try {
            await priceAlertService.unsubscribe(alert.id);
            setAlert(null);
            setThreshold(DEFAULT_THRESHOLD);
            setOpen(false);
        } catch {
            setError("Не вдалося скасувати підписку.");
        } finally {
            setBusy(false);
        }
    };

    return (
        <div className={styles.wrapper} ref={wrapperRef}>
            <Button
                variant={alert ? "secondary" : "outline-primary"}
                size="sm"
                onClick={() => setOpen((v) => !v)}
            >
                {alert ? `🔕 Підписано ±${alert.thresholdPercent}%` : "🔔 Сповіщати про зміну ціни"}
            </Button>

            {open && (
                <div className={styles.popover}>
                    {alert ? (
                        <>
                            <div className={styles.info}>
                                Ви отримаєте сповіщення, коли середня ціна зміниться на ±{alert.thresholdPercent}%
                                від {Math.round(alert.lastNotifiedPrice)} грн.
                            </div>
                            <div className={styles.actions}>
                                <Button variant="danger" size="sm" onClick={handleUnsubscribe} disabled={busy}>
                                    Скасувати підписку
                                </Button>
                            </div>
                        </>
                    ) : (
                        <>
                            <label className={styles.label}>
                                Поріг зміни, %
                                <input
                                    type="number"
                                    min={0.1}
                                    max={100}
                                    step={0.1}
                                    value={threshold}
                                    onChange={(e) => setThreshold(Number(e.target.value))}
                                    className={styles.input}
                                />
                            </label>
                            <div className={styles.actions}>
                                <Button variant="primary" size="sm" onClick={handleSubscribe} disabled={busy}>
                                    Підписатися
                                </Button>
                            </div>
                        </>
                    )}
                    {error && <div className={styles.error}>{error}</div>}
                </div>
            )}
        </div>
    );
}
