import { useEffect, useRef, useState } from "react";
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
    cpu: "Cpu", gpu: "Gpu", ram: "Ram", motherboard: "Motherboard",
    cpucooler: "CpuCooler", pccase: "PcCase", powersupply: "PowerSupply",
    ssd: "Ssd", hdd: "Hdd", fan: "Fan",
};

function normalizeComponentType(raw: string): ComponentType {
    return COMPONENT_TYPE_MAP[raw.toLowerCase()] ?? (raw as ComponentType);
}

export default function PriceAlertButton({ componentId, componentType: rawComponentType }: Props) {
    const componentType = normalizeComponentType(rawComponentType);
    const { auth } = useAuth();
    const [alert, setAlert] = useState<IPriceAlert | null>(null);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [threshold, setThreshold] = useState<number>(DEFAULT_THRESHOLD);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const formRef = useRef<HTMLDivElement>(null);

    const isLoggedIn = Boolean(auth?.userId || auth?.accessToken);

    useEffect(() => {
        if (!isLoggedIn) { setLoading(false); return; }
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
        if (!showForm) return;
        const handler = (e: MouseEvent) => {
            if (formRef.current && !formRef.current.contains(e.target as Node)) {
                setShowForm(false);
                setError(null);
            }
        };
        document.addEventListener("mousedown", handler);
        return () => document.removeEventListener("mousedown", handler);
    }, [showForm]);

    const handleSubscribe = async () => {
        if (threshold <= 0 || threshold > 100) { setError("Enter a value between 0.1 and 100"); return; }
        setBusy(true); setError(null);
        try {
            await priceAlertService.subscribe({ componentId, componentType, thresholdPercent: threshold });
            const refreshed = await priceAlertService.getForComponent(componentId, componentType);
            setAlert(refreshed);
            setShowForm(false);
        } catch {
            setError("Failed to subscribe. Try again later.");
        } finally { setBusy(false); }
    };

    const handleUnsubscribe = async () => {
        if (!alert) return;
        setBusy(true); setError(null);
        try {
            await priceAlertService.unsubscribe(alert.id);
            setAlert(null);
            setThreshold(DEFAULT_THRESHOLD);
        } catch {
            setError("Failed to unsubscribe.");
        } finally { setBusy(false); }
    };

    if (!isLoggedIn || loading) return (
        <div className={styles.panel}>
            <div className={styles.panelHead}>
                <span className={styles.eyebrow}>Price alert</span>
            </div>
            <p className={styles.desc}>Sign in to track price drops for this component.</p>
        </div>
    );

    // ── subscribed state ────────────────────────────────────────
    if (alert) {
        const watchingPrice = Math.round(alert.lastNotifiedPrice);
        return (
            <div className={styles.panel}>
                <div className={styles.panelHead}>
                    <span className={styles.eyebrow}>Price alert</span>
                    <span className={styles.statusActive}><span className={styles.statusDot} /> ACTIVE</span>
                </div>

                <p className={styles.desc}>
                    Notify you any time the <strong>lowest live price</strong> across tracked stores drops below today's.
                </p>

                <div className={styles.statsRow}>
                    <div className={styles.statBox}>
                        <span className={styles.statLabel}>Watching</span>
                        <span className={styles.statVal}>₴ {watchingPrice.toLocaleString()}</span>
                    </div>
                    <div className={styles.statBox}>
                        <span className={styles.statLabel}>Threshold</span>
                        <span className={styles.statVal}>±{alert.thresholdPercent}%</span>
                    </div>
                </div>

                <div className={styles.trackingRow}>
                    <span className={styles.trackingDot} />
                    Alert is active for this component
                </div>

                {error && <div className={styles.error}>{error}</div>}

                <button className={styles.btnUnsub} onClick={handleUnsubscribe} disabled={busy}>
                    × {busy ? 'Removing…' : 'Unsubscribe'}
                </button>
            </div>
        );
    }

    // ── not subscribed state ────────────────────────────────────
    return (
        <div className={styles.panel} ref={formRef}>
            <div className={styles.panelHead}>
                <span className={styles.eyebrow}>Price alert</span>
                <span className={styles.statusInactive}>INACTIVE</span>
            </div>

            <p className={styles.desc}>
                Get notified when the price drops below your target threshold.
            </p>

            {!showForm ? (
                <button className={styles.btnSetAlert} onClick={() => setShowForm(true)}>
                    <span className={styles.toolGly}>△</span> Set price alert
                </button>
            ) : (
                <div className={styles.form}>
                    <label className={styles.label}>
                        <span className={styles.labelText}>Alert threshold %</span>
                        <input
                            type="number"
                            min={0.1} max={100} step={0.1}
                            value={threshold}
                            onChange={e => setThreshold(Number(e.target.value))}
                            className={styles.input}
                            autoFocus
                        />
                    </label>
                    {error && <div className={styles.error}>{error}</div>}
                    <div className={styles.formActions}>
                        <button className={styles.btnCancel} onClick={() => { setShowForm(false); setError(null); }}>
                            Cancel
                        </button>
                        <button className={styles.btnConfirm} onClick={handleSubscribe} disabled={busy}>
                            {busy ? 'Saving…' : 'Confirm'}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
