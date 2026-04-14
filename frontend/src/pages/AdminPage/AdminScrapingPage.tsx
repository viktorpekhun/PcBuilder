import { useCallback, useEffect, useRef, useState } from "react";
import { scraperService } from "../../api/scraper.service";
import type { IScrapeJobStatus, ScrapeJobState } from "../../types/admin.types";
import styles from "./AdminScrapingPage.module.css";

interface CategoryDef {
    componentType: string;
    label: string;
    routeSlug: string;
}

const CATEGORIES: CategoryDef[] = [
    { componentType: "Cpu", label: "CPU", routeSlug: "cpu" },
    { componentType: "Gpu", label: "GPU", routeSlug: "gpu" },
    { componentType: "Motherboard", label: "Motherboard", routeSlug: "motherboard" },
    { componentType: "CpuCooler", label: "CPU Cooler", routeSlug: "cpu-cooler" },
    { componentType: "PcCase", label: "PC Case", routeSlug: "pc-case" },
    { componentType: "PowerSupply", label: "Power Supply", routeSlug: "power-supply" },
    { componentType: "Ram", label: "RAM", routeSlug: "ram" },
    { componentType: "Ssd", label: "SSD", routeSlug: "ssd" },
    { componentType: "Hdd", label: "HDD", routeSlug: "hdd" },
    { componentType: "Fan", label: "Fan", routeSlug: "fan" },
];

const ACTIVE_STATES: ScrapeJobState[] = ["Queued", "Running", "Cancelling"];

const POLL_INTERVAL_MS = 5000;

const isActive = (state: ScrapeJobState) => ACTIVE_STATES.includes(state);

const formatDate = (iso: string | null) =>
    iso ? new Date(iso).toLocaleString() : "—";

const formatDuration = (start: string | null, end: string | null): string => {
    if (!start || !end) return "—";
    const ms = new Date(end).getTime() - new Date(start).getTime();
    if (ms < 0) return "—";
    const s = Math.floor(ms / 1000);
    if (s < 60) return `${s}s`;
    const m = Math.floor(s / 60);
    const rem = s % 60;
    return `${m}m ${rem}s`;
};

const badgeClass = (state: ScrapeJobState): string => {
    switch (state) {
        case "Queued": return "badge-queued";
        case "Running": return "badge-running";
        case "Cancelling": return "badge-cancelling";
        case "Completed": return "badge-completed";
        case "Failed": return "badge-failed";
        case "Cancelled": return "badge-cancelled";
        default: return "badge-idle";
    }
};

const AdminScrapingPage = () => {
    const [jobs, setJobs] = useState<IScrapeJobStatus[]>([]);
    const [loading, setLoading] = useState(true);
    const [polling, setPolling] = useState(false);
    const intervalRef = useRef<number | null>(null);

    const fetchJobs = useCallback(async () => {
        try {
            const res = await scraperService.getAllJobs();
            setJobs(res.data);
        } catch {
            /* swallow — transient */
        } finally {
            setLoading(false);
        }
    }, []);

    const clearPoll = useCallback(() => {
        if (intervalRef.current != null) {
            window.clearInterval(intervalRef.current);
            intervalRef.current = null;
        }
        setPolling(false);
    }, []);

    const startPoll = useCallback(() => {
        if (intervalRef.current != null) return;
        setPolling(true);
        intervalRef.current = window.setInterval(fetchJobs, POLL_INTERVAL_MS);
    }, [fetchJobs]);

    useEffect(() => {
        fetchJobs();
        return () => clearPoll();
    }, [fetchJobs, clearPoll]);

    useEffect(() => {
        const hasActive = jobs.some(j => isActive(j.state));
        if (hasActive) {
            startPoll();
        } else {
            clearPoll();
        }
    }, [jobs, startPoll, clearPoll]);

    const latestByType = new Map<string, IScrapeJobStatus>();
    for (const job of jobs) {
        const existing = latestByType.get(job.componentType);
        if (!existing || new Date(job.queuedAt) > new Date(existing.queuedAt)) {
            latestByType.set(job.componentType, job);
        }
    }

    const handleStart = async (category: CategoryDef) => {
        try {
            await scraperService.startCategoryScrape(category.routeSlug);
            await fetchJobs();
        } catch (e: unknown) {
            const message = (e as { response?: { data?: string | { message?: string } } })
                ?.response?.data;
            const text = typeof message === "string"
                ? message
                : message?.message || "Failed to start scrape job.";
            window.alert(text);
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

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>Scraping</h1>
                {polling && (
                    <div className={styles["polling-indicator"]}>
                        <span className={styles["polling-dot"]} />
                        Auto-refreshing every {POLL_INTERVAL_MS / 1000}s
                    </div>
                )}
            </div>

            <div className={styles.grid}>
                {CATEGORIES.map(category => {
                    const latest = latestByType.get(category.componentType);
                    const state: ScrapeJobState | "Idle" = latest?.state ?? "Idle";
                    const active = latest ? isActive(latest.state) : false;
                    const badgeKey = latest ? badgeClass(latest.state) : "badge-idle";

                    return (
                        <div key={category.componentType} className={styles.card}>
                            <div className={styles["card-header"]}>
                                <h3 className={styles["card-name"]}>{category.label}</h3>
                                <span className={`${styles.badge} ${styles[badgeKey]}`}>
                                    {state}
                                </span>
                            </div>

                            <div className={styles["card-body"]}>
                                {latest?.state === "Running" && latest.startedAt && (
                                    <span>Started {formatDate(latest.startedAt)}</span>
                                )}
                                {latest?.state === "Completed" && (
                                    <span>Last finished {formatDate(latest.completedAt)}</span>
                                )}
                                {latest?.state === "Failed" && (
                                    <span>Failed {formatDate(latest.completedAt)}</span>
                                )}
                                {!latest && <span>No runs yet.</span>}
                                {latest && latest.itemsScraped > 0 && (
                                    <span>{latest.itemsScraped} items scraped</span>
                                )}
                            </div>

                            <div className={styles["card-footer"]}>
                                <button
                                    type="button"
                                    className={styles.btn}
                                    disabled={active}
                                    onClick={() => handleStart(category)}
                                >
                                    {active ? "In progress…" : "Start Scraping"}
                                </button>
                                {active && latest && latest.state !== "Cancelling" && (
                                    <button
                                        type="button"
                                        className={`${styles.btn} ${styles["btn-cancel"]}`}
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

            <section className={styles["history-section"]}>
                <div className={styles["history-header"]}>
                    <h2 className={styles["history-title"]}>Recent Jobs</h2>
                </div>
                {loading ? (
                    <div className={styles["history-empty"]}>Loading jobs…</div>
                ) : jobs.length === 0 ? (
                    <div className={styles["history-empty"]}>No jobs yet.</div>
                ) : (
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Component</th>
                                <th>State</th>
                                <th>Queued</th>
                                <th>Completed</th>
                                <th>Duration</th>
                                <th>Items</th>
                                <th>Error</th>
                            </tr>
                        </thead>
                        <tbody>
                            {jobs.slice(0, 50).map(job => (
                                <tr key={job.jobId}>
                                    <td>{job.componentType}</td>
                                    <td>
                                        <span className={`${styles.badge} ${styles[badgeClass(job.state)]}`}>
                                            {job.state}
                                        </span>
                                    </td>
                                    <td>{formatDate(job.queuedAt)}</td>
                                    <td>{formatDate(job.completedAt)}</td>
                                    <td>{formatDuration(job.startedAt, job.completedAt)}</td>
                                    <td>{job.itemsScraped}</td>
                                    <td className={styles["error-cell"]} title={job.errorMessage ?? ""}>
                                        {job.errorMessage ?? "—"}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </section>
        </div>
    );
};

export default AdminScrapingPage;
