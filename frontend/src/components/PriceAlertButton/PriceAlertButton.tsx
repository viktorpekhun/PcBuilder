import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
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
    const { t } = useTranslation();
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
        if (threshold <= 0 || threshold > 100) { setError(t('components.componentPage.priceAlert.errorThreshold')); return; }
        setBusy(true); setError(null);
        try {
            await priceAlertService.subscribe({ componentId, componentType, thresholdPercent: threshold });
            const refreshed = await priceAlertService.getForComponent(componentId, componentType);
            setAlert(refreshed);
            setShowForm(false);
        } catch {
            setError(t('components.componentPage.priceAlert.errorSubscribe'));
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
            setError(t('components.componentPage.priceAlert.errorUnsubscribe'));
        } finally { setBusy(false); }
    };

    if (!isLoggedIn || loading) return (
        <div className={styles.panel}>
            <div className={styles.panelHead}>
                <span className={styles.eyebrow}>{t('components.componentPage.priceAlert.eyebrow')}</span>
            </div>
            <p className={styles.desc}>{t('components.componentPage.priceAlert.signInPrompt')}</p>
        </div>
    );

    // ── subscribed state ────────────────────────────────────────
    if (alert) {
        const watchingPrice = Math.round(alert.lastNotifiedPrice);
        return (
            <div className={styles.panel}>
                <div className={styles.panelHead}>
                    <span className={styles.eyebrow}>{t('components.componentPage.priceAlert.eyebrow')}</span>
                    <span className={styles.statusActive}><span className={styles.statusDot} /> {t('components.componentPage.priceAlert.statusActive')}</span>
                </div>

                <p className={styles.desc}>{t('components.componentPage.priceAlert.descActive')}</p>

                <div className={styles.statsRow}>
                    <div className={styles.statBox}>
                        <span className={styles.statLabel}>{t('components.componentPage.priceAlert.watching')}</span>
                        <span className={styles.statVal}>₴ {watchingPrice.toLocaleString()}</span>
                    </div>
                    <div className={styles.statBox}>
                        <span className={styles.statLabel}>{t('components.componentPage.priceAlert.threshold')}</span>
                        <span className={styles.statVal}>±{alert.thresholdPercent}%</span>
                    </div>
                </div>

                <div className={styles.trackingRow}>
                    <span className={styles.trackingDot} />
                    {t('components.componentPage.priceAlert.trackingActive')}
                </div>

                {error && <div className={styles.error}>{error}</div>}

                <button className={styles.btnUnsub} onClick={handleUnsubscribe} disabled={busy}>
                    {busy ? t('components.componentPage.priceAlert.unsubscribing') : t('components.componentPage.priceAlert.unsubscribe')}
                </button>
            </div>
        );
    }

    // ── not subscribed state ────────────────────────────────────
    return (
        <div className={styles.panel} ref={formRef}>
            <div className={styles.panelHead}>
                <span className={styles.eyebrow}>{t('components.componentPage.priceAlert.eyebrow')}</span>
                <span className={styles.statusInactive}>{t('components.componentPage.priceAlert.statusInactive')}</span>
            </div>

            <p className={styles.desc}>{t('components.componentPage.priceAlert.descInactive')}</p>

            {!showForm ? (
                <button className={styles.btnSetAlert} onClick={() => setShowForm(true)}>
                    {t('components.componentPage.priceAlert.setAlert')}
                </button>
            ) : (
                <div className={styles.form}>
                    <label className={styles.label}>
                        <span className={styles.labelText}>{t('components.componentPage.priceAlert.thresholdLabel')}</span>
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
                            {t('components.componentPage.priceAlert.cancel')}
                        </button>
                        <button className={styles.btnConfirm} onClick={handleSubscribe} disabled={busy}>
                            {busy ? t('components.componentPage.priceAlert.saving') : t('components.componentPage.priceAlert.confirm')}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
