import { useCallback, useEffect, useRef, useState } from "react";
import { scraperService } from "../../api/scraper.service";
import { useTranslation } from "react-i18next";
import type { IScrapeJobStatus, ScrapeJobState } from "../../types/admin.types";
import styles from "./AdminScrapingPage.module.css";

interface CategoryDef {
    componentType: string;
    labelKey: string;
    routeSlug: string;
    glyph: string;
}

const CATEGORIES: CategoryDef[] = [
    { componentType: "Cpu",         labelKey: "componentTypes.cpu",         routeSlug: "cpu",          glyph: "CPU" },
    { componentType: "Gpu",         labelKey: "componentTypes.gpu",         routeSlug: "gpu",          glyph: "GPU" },
    { componentType: "Motherboard", labelKey: "componentTypes.motherboard", routeSlug: "motherboard",  glyph: "M/B" },
    { componentType: "CpuCooler",   labelKey: "componentTypes.cpuCooler",   routeSlug: "cpu-cooler",   glyph: "CLR" },
    { componentType: "PcCase",      labelKey: "componentTypes.pcCase",      routeSlug: "pc-case",      glyph: "CSE" },
    { componentType: "PowerSupply", labelKey: "componentTypes.powerSupply", routeSlug: "power-supply", glyph: "PSU" },
    { componentType: "Ram",         labelKey: "componentTypes.ram",         routeSlug: "ram",          glyph: "RAM" },
    { componentType: "Ssd",         labelKey: "componentTypes.ssd",         routeSlug: "ssd",          glyph: "SSD" },
    { componentType: "Hdd",         labelKey: "componentTypes.hdd",         routeSlug: "hdd",          glyph: "HDD" },
    { componentType: "Fan",         labelKey: "componentTypes.fan",         routeSlug: "fan",          glyph: "FAN" },
];

const ACTIVE_STATES: ScrapeJobState[] = ["Queued", "Running", "Cancelling"];
const POLL_INTERVAL_MS = 5000;

const isActive = (state: ScrapeJobState) => ACTIVE_STATES.includes(state);

const fmtDateTime = (iso: string | null) => {
    if (!iso) return "—";
    const d = new Date(iso);
    return d.toLocaleDateString("en-GB", { day: "2-digit", month: "short" }) + " " +
        d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
};

const fmtDuration = (start: string | null, end: string | null): string => {
    if (!start || !end) return "—";
    const ms = new Date(end).getTime() - new Date(start).getTime();
    if (ms < 0) return "—";
    const s = Math.floor(ms / 1000);
    if (s < 60) return `${s}s`;
    return `${Math.floor(s / 60)}m ${s % 60}s`;
};

const fmtNum = (n: number) => n.toLocaleString("en-US").replace(/,/g, " ");

type PillKind = "info" | "warn" | "ok" | "err" | "neu";

const statePillKind = (state: ScrapeJobState | "Idle"): PillKind => ({
    Running: "info", Queued: "warn", Cancelling: "warn",
    Completed: "ok", Failed: "err", Cancelled: "neu", Idle: "neu",
}[state] as PillKind ?? "neu");

const pillDotColor: Record<string, string> = {
    info: "#6FB1FC", warn: "#F5B43A", ok: "#7AE07A", err: "#FF5C5C", neu: "#70707A",
};

const pillClass: Record<PillKind, string> = {
    info: styles.pillInfo!, warn: styles.pillWarn!, ok: styles.pillOk!, err: styles.pillErr!, neu: styles.pillNeu!,
};

// ── Pill component ───────────────────────────────────────────────────────────
const Pill = ({ kind, children }: { kind: PillKind; children: React.ReactNode }) => (
    <span className={`${styles.pill} ${pillClass[kind]}`}>
        <span className={styles.pillDot} style={{ background: pillDotColor[kind] }} />
        {children}
    </span>
);

// ── Animated dots ─────────────────────────────────────────────────────────────
const AnimatedDots = () => {
    const [count, setCount] = useState(0);
    useEffect(() => {
        const id = window.setInterval(() => setCount(c => (c + 1) % 4), 500);
        return () => window.clearInterval(id);
    }, []);
    return <span className={styles.dots}>{".".repeat(count)}<span className={styles.dotsSpace}>{".".repeat(3 - count)}</span></span>;
};

const PAGE_SIZE = 15;

const AdminScrapingPage = () => {
    const { t } = useTranslation();
    const [jobs, setJobs] = useState<IScrapeJobStatus[]>([]);
    const [loading, setLoading] = useState(true);
    const [polling, setPolling] = useState(false);
    const intervalRef = useRef<number | null>(null);
    const [page, setPage] = useState(1);

    const fetchJobs = useCallback(async () => {
        try {
            const res = await scraperService.getAllJobs();
            setJobs(res.data);
        } catch { /* swallow */ }
        finally { setLoading(false); }
    }, []);

    const clearPoll = useCallback(() => {
        if (intervalRef.current != null) { window.clearInterval(intervalRef.current); intervalRef.current = null; }
        setPolling(false);
    }, []);

    const startPoll = useCallback(() => {
        if (intervalRef.current != null) return;
        setPolling(true);
        intervalRef.current = window.setInterval(fetchJobs, POLL_INTERVAL_MS);
    }, [fetchJobs]);

    useEffect(() => { fetchJobs(); return () => clearPoll(); }, [fetchJobs, clearPoll]);

    useEffect(() => {
        if (jobs.some(j => isActive(j.state))) startPoll();
        else clearPoll();
    }, [jobs, startPoll, clearPoll]);

    const latestByType = new Map<string, IScrapeJobStatus>();
    for (const job of jobs) {
        const existing = latestByType.get(job.componentType);
        if (!existing || new Date(job.queuedAt) > new Date(existing.queuedAt)) {
            latestByType.set(job.componentType, job);
        }
    }

    const kindLabel = (kind: string) =>
        kind === "PriceUpdate" ? t("admin.scrapingPage.kind.prices")
        : kind === "SingleComponent" ? t("admin.scrapingPage.kind.single")
        : t("admin.scrapingPage.kind.components");

    const handleStart = async (cat: CategoryDef) => {
        try {
            await scraperService.startCategoryScrape(cat.routeSlug);
            await fetchJobs();
        } catch (e: unknown) {
            const msg = (e as { response?: { data?: string | { message?: string } } })?.response?.data;
            window.alert(typeof msg === "string" ? msg : msg?.message || "Failed to start scrape job.");
        }
    };

    const handleUpdatePrices = async (cat: CategoryDef) => {
        try {
            await scraperService.startPriceUpdate(cat.componentType);
            await fetchJobs();
        } catch (e: unknown) {
            const msg = (e as { response?: { data?: string | { message?: string } } })?.response?.data;
            window.alert(typeof msg === "string" ? msg : msg?.message || "Failed to start price update.");
        }
    };

    const handleCancel = async (jobId: string) => {
        if (!window.confirm(t("admin.scrapingPage.cancelConfirm"))) return;
        try {
            await scraperService.cancelJob(jobId);
            await fetchJobs();
        } catch {
            window.alert(t("admin.scrapingPage.cancelFailed"));
        }
    };

    const activeCount = jobs.filter(j => isActive(j.state)).length;

    return (
        <div className={styles.page}>
            {/* Content bar */}
            <div className={styles.contentBar}>
                <span className={styles.contentMeta}>
                    {t("admin.scrapingPage.categories", { count: CATEGORIES.length })} · {t("admin.scrapingPage.active", { count: activeCount })}
                </span>
                {polling ? (
                    <span className={styles.polling}>
                        <span className={styles.pollingDot} />
                        {t("admin.scrapingPage.autoRefreshing", { interval: POLL_INTERVAL_MS / 1000 })}
                    </span>
                ) : (
                    <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}>
                        <span className={styles.idleDot} />
                        <span className={styles.idleLabel}>{t("admin.scrapingPage.idle")}</span>
                    </span>
                )}
            </div>

            {/* Cards */}
            <div className={styles.scrapeGrid}>
                {CATEGORIES.map(cat => {
                    const latest = latestByType.get(cat.componentType);
                    const state: ScrapeJobState | "Idle" = latest?.state ?? "Idle";
                    const active = latest ? isActive(latest.state) : false;
                    const running = latest?.state === "Running";

                    return (
                        <div key={cat.componentType} className={styles.scard}>
                            <div className={styles.scardHead}>
                                <div className={styles.scardNameGroup}>
                                    <span className={styles.slotGlyph}>{cat.glyph}</span>
                                    <span className={styles.scardLabel}>{t(cat.labelKey)}</span>
                                </div>
                                <Pill kind={statePillKind(state)}>{t(`admin.scrapingPage.states.${state}`)}</Pill>
                            </div>

                            {running && (
                                <div className={styles.prog}>
                                    {latest!.totalItems && latest!.totalItems > 0
                                        ? <div className={styles.progFill} style={{ width: `${Math.min(100, latest!.itemsScraped / latest!.totalItems * 100)}%` }} />
                                        : <div className={`${styles.progFill} ${styles.progIndet}`} />
                                    }
                                </div>
                            )}

                            <div className={styles.scardBody}>
                                {running && (
                                    <span className={styles.scardBodyBig}>
                                        {latest!.itemsScraped > 0
                                            ? <>{latest!.totalItems
                                                ? t("admin.scrapingPage.scrapedOf", { count: fmtNum(latest!.itemsScraped), total: fmtNum(latest!.totalItems) })
                                                : t("admin.scrapingPage.scraped", { count: fmtNum(latest!.itemsScraped) })}</>
                                            : <>{t("admin.scrapingPage.inProgressDots")}<AnimatedDots /></>
                                        }
                                    </span>
                                )}
                                {latest?.state === "Queued" && <span>{t("admin.scrapingPage.waitingInQueue")}</span>}
                                {latest?.state === "Completed" && (
                                    <>
                                        <span>{t("admin.scrapingPage.lastRun", { date: fmtDateTime(latest.completedAt) })}</span>
                                        {latest.itemsScraped > 0 && <span className={styles.scardBodyBig}>{t("admin.scrapingPage.itemsScraped", { count: fmtNum(latest.itemsScraped) })}</span>}
                                    </>
                                )}
                                {latest?.state === "Failed" && <span style={{ color: "#FF5C5C" }}>{t("admin.scrapingPage.failed", { date: fmtDateTime(latest.completedAt) })}</span>}
                                {latest?.state === "Cancelled" && <span>{t("admin.scrapingPage.cancelled", { date: fmtDateTime(latest.completedAt) })}</span>}
                                {!latest && <span style={{ color: "#4A4A52" }}>{t("admin.scrapingPage.noRunsYet")}</span>}
                            </div>

                            <div className={styles.scardFoot}>
                                <button
                                    type="button"
                                    className={`${styles.btn} ${styles.btnSec}`}
                                    disabled={active}
                                    onClick={() => handleStart(cat)}
                                >
                                    {active ? t("admin.scrapingPage.inProgress") : t("admin.scrapingPage.scrape")}
                                </button>
                                <button
                                    type="button"
                                    className={`${styles.btn} ${styles.btnGhost}`}
                                    disabled={active}
                                    onClick={() => handleUpdatePrices(cat)}
                                >
                                    {t("admin.scrapingPage.prices")}
                                </button>
                                {active && latest && latest.state !== "Cancelling" && (
                                    <button
                                        type="button"
                                        className={`${styles.btn} ${styles.btnDanger}`}
                                        onClick={() => handleCancel(latest.jobId)}
                                    >
                                        {t("admin.scrapingPage.cancel")}
                                    </button>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>

            {/* Job history */}
            <section className={styles.historySection}>
                <div className={styles.historyHead}>
                    <h2 className={styles.historyTitle}>{t("admin.scrapingPage.recentJobs")}</h2>
                    <div className={styles.historyHeadRight}>
                        <span className={styles.historyMeta}>{t("admin.scrapingPage.job_other", { count: jobs.length })}</span>
                        {jobs.length > PAGE_SIZE && (
                            <div className={styles.paginator}>
                                <button
                                    type="button"
                                    className={styles.pageBtn}
                                    disabled={page === 1}
                                    onClick={() => setPage(p => p - 1)}
                                >←</button>
                                <span className={styles.pageInfo}>{page} / {Math.ceil(jobs.length / PAGE_SIZE)}</span>
                                <button
                                    type="button"
                                    className={styles.pageBtn}
                                    disabled={page >= Math.ceil(jobs.length / PAGE_SIZE)}
                                    onClick={() => setPage(p => p + 1)}
                                >→</button>
                            </div>
                        )}
                    </div>
                </div>
                {loading ? (
                    <div className={styles.historyEmpty}>{t("admin.scrapingPage.loading")}</div>
                ) : jobs.length === 0 ? (
                    <div className={styles.historyEmpty}>{t("admin.scrapingPage.noJobs")}</div>
                ) : (
                    <div className={styles.tblWrap}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>{t("admin.scrapingPage.table.component")}</th>
                                    <th style={{ width: 110 }}>{t("admin.scrapingPage.table.kind")}</th>
                                    <th style={{ width: 120 }}>{t("admin.scrapingPage.table.state")}</th>
                                    <th style={{ width: 130 }}>{t("admin.scrapingPage.table.queued")}</th>
                                    <th style={{ width: 130 }}>{t("admin.scrapingPage.table.completed")}</th>
                                    <th className={styles.thRight} style={{ width: 80 }}>{t("admin.scrapingPage.table.duration")}</th>
                                    <th className={styles.thRight} style={{ width: 70 }}>{t("admin.scrapingPage.table.items")}</th>
                                    <th style={{ width: 40 }}>{t("admin.scrapingPage.table.err")}</th>
                                </tr>
                            </thead>
                            <tbody>
                                {jobs.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE).map(job => (
                                    <tr key={job.jobId}>
                                        <td className={styles.tdName}>{job.componentType}</td>
                                        <td><span className={styles.tag}>{kindLabel(job.kind)}</span></td>
                                        <td><Pill kind={statePillKind(job.state)}>{t(`admin.scrapingPage.states.${job.state}`)}</Pill></td>
                                        <td className={styles.tdDim}>{fmtDateTime(job.queuedAt)}</td>
                                        <td className={styles.tdDim}>{fmtDateTime(job.completedAt)}</td>
                                        <td className={styles.tdRight}>{fmtDuration(job.startedAt, job.completedAt)}</td>
                                        <td className={styles.tdRight}>{job.itemsScraped > 0 ? fmtNum(job.itemsScraped) : "—"}</td>
                                        <td className={styles.errorCell}>
                                            {job.errorMessage ? (
                                                <span className={styles.errBadge}>
                                                    ?
                                                    <span className={styles.errTooltip}>{job.errorMessage}</span>
                                                </span>
                                            ) : "—"}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </section>
        </div>
    );
};

export default AdminScrapingPage;
