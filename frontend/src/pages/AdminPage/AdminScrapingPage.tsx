import { useCallback, useEffect, useRef, useState } from "react";
import { scraperService } from "../../api/scraper.service";
import type { IScrapeJobStatus, ScrapeJobState } from "../../types/admin.types";
import styles from "./AdminScrapingPage.module.css";

interface CategoryDef {
    componentType: string;
    label: string;
    routeSlug: string;
    glyph: string;
}

const CATEGORIES: CategoryDef[] = [
    { componentType: "Cpu",         label: "CPU",          routeSlug: "cpu",          glyph: "CPU" },
    { componentType: "Gpu",         label: "GPU",          routeSlug: "gpu",          glyph: "GPU" },
    { componentType: "Motherboard", label: "Motherboard",  routeSlug: "motherboard",  glyph: "M/B" },
    { componentType: "CpuCooler",   label: "CPU Cooler",   routeSlug: "cpu-cooler",   glyph: "CLR" },
    { componentType: "PcCase",      label: "PC Case",      routeSlug: "pc-case",      glyph: "CSE" },
    { componentType: "PowerSupply", label: "Power Supply", routeSlug: "power-supply", glyph: "PSU" },
    { componentType: "Ram",         label: "RAM",          routeSlug: "ram",          glyph: "RAM" },
    { componentType: "Ssd",         label: "SSD",          routeSlug: "ssd",          glyph: "SSD" },
    { componentType: "Hdd",         label: "HDD",          routeSlug: "hdd",          glyph: "HDD" },
    { componentType: "Fan",         label: "Fan",          routeSlug: "fan",          glyph: "FAN" },
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

const kindLabel = (kind: string) =>
    kind === "PriceUpdate" ? "Prices" : kind === "SingleComponent" ? "Single" : "Components";

type PillKind = "info" | "warn" | "ok" | "err" | "neu";

const statePillKind = (state: ScrapeJobState | "Idle"): PillKind => ({
    Running: "info", Queued: "warn", Cancelling: "warn",
    Completed: "ok", Failed: "err", Cancelled: "neu", Idle: "neu",
}[state] as PillKind ?? "neu");

const pillDotColor: Record<string, string> = {
    info: "#6FB1FC", warn: "#F5B43A", ok: "#7AE07A", err: "#FF5C5C", neu: "#70707A",
};

const pillClass: Record<PillKind, string> = {
    info: styles.pillInfo, warn: styles.pillWarn, ok: styles.pillOk, err: styles.pillErr, neu: styles.pillNeu,
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
        if (!window.confirm("Cancel this scrape job?")) return;
        try {
            await scraperService.cancelJob(jobId);
            await fetchJobs();
        } catch {
            window.alert("Failed to cancel job.");
        }
    };

    const activeCount = jobs.filter(j => isActive(j.state)).length;

    return (
        <div className={styles.page}>
            {/* Content bar */}
            <div className={styles.contentBar}>
                <span className={styles.contentMeta}>
                    {CATEGORIES.length} categories · {activeCount} active
                </span>
                {polling ? (
                    <span className={styles.polling}>
                        <span className={styles.pollingDot} />
                        Auto-refreshing · {POLL_INTERVAL_MS / 1000}s
                    </span>
                ) : (
                    <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}>
                        <span className={styles.idleDot} />
                        <span className={styles.idleLabel}>idle</span>
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
                                    <span className={styles.scardLabel}>{cat.label}</span>
                                </div>
                                <Pill kind={statePillKind(state)}>{state}</Pill>
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
                                            ? <>{fmtNum(latest!.itemsScraped)}{latest!.totalItems ? ` / ${fmtNum(latest!.totalItems)}` : ""} scraped</>
                                            : <>In progress<AnimatedDots /></>
                                        }
                                    </span>
                                )}
                                {latest?.state === "Queued" && <span>Waiting in queue…</span>}
                                {latest?.state === "Completed" && (
                                    <>
                                        <span>Last run {fmtDateTime(latest.completedAt)}</span>
                                        {latest.itemsScraped > 0 && <span className={styles.scardBodyBig}>{fmtNum(latest.itemsScraped)} items scraped</span>}
                                    </>
                                )}
                                {latest?.state === "Failed" && <span style={{ color: "#FF5C5C" }}>Failed {fmtDateTime(latest.completedAt)}</span>}
                                {latest?.state === "Cancelled" && <span>Cancelled {fmtDateTime(latest.completedAt)}</span>}
                                {!latest && <span style={{ color: "#4A4A52" }}>No runs yet.</span>}
                            </div>

                            <div className={styles.scardFoot}>
                                <button
                                    type="button"
                                    className={`${styles.btn} ${styles.btnSec}`}
                                    disabled={active}
                                    onClick={() => handleStart(cat)}
                                >
                                    {active ? "In progress…" : "Scrape"}
                                </button>
                                <button
                                    type="button"
                                    className={`${styles.btn} ${styles.btnGhost}`}
                                    disabled={active}
                                    onClick={() => handleUpdatePrices(cat)}
                                >
                                    ↻ Prices
                                </button>
                                {active && latest && latest.state !== "Cancelling" && (
                                    <button
                                        type="button"
                                        className={`${styles.btn} ${styles.btnDanger}`}
                                        onClick={() => handleCancel(latest.jobId)}
                                    >
                                        Cancel
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
                    <h2 className={styles.historyTitle}>Recent jobs</h2>
                    <div className={styles.historyHeadRight}>
                        <span className={styles.historyMeta}>{jobs.length} job{jobs.length === 1 ? "" : "s"}</span>
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
                    <div className={styles.historyEmpty}>LOADING JOBS…</div>
                ) : jobs.length === 0 ? (
                    <div className={styles.historyEmpty}>No jobs yet.</div>
                ) : (
                    <div className={styles.tblWrap}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Component</th>
                                    <th style={{ width: 110 }}>Kind</th>
                                    <th style={{ width: 120 }}>State</th>
                                    <th style={{ width: 130 }}>Queued</th>
                                    <th style={{ width: 130 }}>Completed</th>
                                    <th className={styles.thRight} style={{ width: 80 }}>Duration</th>
                                    <th className={styles.thRight} style={{ width: 70 }}>Items</th>
                                    <th style={{ width: 40 }}>Err</th>
                                </tr>
                            </thead>
                            <tbody>
                                {jobs.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE).map(job => (
                                    <tr key={job.jobId}>
                                        <td className={styles.tdName}>{job.componentType}</td>
                                        <td><span className={styles.tag}>{kindLabel(job.kind)}</span></td>
                                        <td><Pill kind={statePillKind(job.state)}>{job.state}</Pill></td>
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
