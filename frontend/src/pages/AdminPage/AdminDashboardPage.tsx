import { useCallback, useEffect, useState } from "react";
import { adminService } from "../../api/admin.service";
import { Pagination } from "../../components/Pagination/Pagination";
import { useTranslation } from "react-i18next";
import type { IAdminActivityLog, IAdminStats, IPaginationHeader } from "../../types/admin.types";
import styles from "./AdminDashboardPage.module.css";

const componentLabels: Record<string, string> = {
    cpu: "CPU", gpu: "GPU", motherboard: "Motherboard", ram: "RAM",
    ssd: "SSD", hdd: "HDD", cpuCooler: "CPU Cooler", fan: "Fan",
    pcCase: "PC Case", powerSupply: "Power Supply",
};

const componentGlyphs: Record<string, string> = {
    cpu: "CPU", gpu: "GPU", motherboard: "M/B", ram: "RAM",
    ssd: "SSD", hdd: "HDD", cpuCooler: "CLR", fan: "FAN",
    pcCase: "CSE", powerSupply: "PSU",
};

const fmtNum = (n: number) => n.toLocaleString("en-US").replace(/,/g, " ");

const ACTION_DOT: Record<string, string> = {
    BanUser:       "#F5B43A",
    UnbanUser:     "#6FB1FC",
    PromoteToAdmin:"#AECF55",
    DemoteToUser:  "#70707A",
    DeleteUser:    "#FF5C5C",
    DeleteBuild:   "#FF5C5C",
    DeleteReview:  "#FF5C5C",
    ResolveReport: "#7AE07A",
    DismissReport: "#70707A",
};

const fmtOccurred = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString("en-GB", { day: "2-digit", month: "short" }) + " " +
        d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
};

type Span = "1" | "7" | "30";
const PAGE_SIZE = 10;

// ── Segmented control ────────────────────────────────────────────────────────
interface SegOption<T> { value: T; label: string; }
function Seg<T extends string>({ value, onChange, options }: { value: T; onChange: (v: T) => void; options: SegOption<T>[]; }) {
    return (
        <div className={styles.seg}>
            {options.map(o => (
                <button
                    key={String(o.value)}
                    type="button"
                    className={`${styles.segBtn} ${value === o.value ? styles.segBtnActive : ""}`}
                    onClick={() => onChange(o.value)}
                >
                    {o.label}
                </button>
            ))}
        </div>
    );
}

const AdminDashboardPage = () => {
    const { t } = useTranslation();
    const [stats, setStats] = useState<IAdminStats | null>(null);
    const [statsLoading, setStatsLoading] = useState(true);
    const [statsError, setStatsError] = useState<string | null>(null);

    const [activity, setActivity] = useState<IAdminActivityLog[]>([]);
    const [activityPagination, setActivityPagination] = useState<IPaginationHeader | null>(null);
    const [activityLoading, setActivityLoading] = useState(true);
    const [span, setSpan] = useState<Span>("1");
    const [page, setPage] = useState(1);

    useEffect(() => {
        let cancelled = false;
        adminService.getStats()
            .then(res => { if (!cancelled) setStats(res.data); })
            .catch(() => { if (!cancelled) setStatsError(t("admin.dashboardPage.loadStatsFailed")); })
            .finally(() => { if (!cancelled) setStatsLoading(false); });
        return () => { cancelled = true; };
    }, [t]);

    const fetchActivity = useCallback(async (daysBack: number, pageNumber: number) => {
        setActivityLoading(true);
        try {
            const { items, pagination } = await adminService.getActivity(daysBack, pageNumber, PAGE_SIZE);
            setActivity(items);
            setActivityPagination(pagination);
        } catch {
            setActivity([]);
            setActivityPagination(null);
        } finally {
            setActivityLoading(false);
        }
    }, []);

    useEffect(() => { fetchActivity(Number(span), page); }, [fetchActivity, span, page]);

    const handleSpanChange = (next: Span) => { setSpan(next); setPage(1); };

    const STAT_CARDS = [
        {
            key: "totalUsers",
            label: t("admin.dashboardPage.statCards.totalUsers"),
            color: "#6FB1FC",
            renderHint: (s: IAdminStats) => <><span className={styles.statHintUp}>{t("admin.dashboardPage.statCards.newUsersHint", { count: s.newUsersLast7Days })}</span></>,
        },
        {
            key: "totalBuilds",
            label: t("admin.dashboardPage.statCards.totalBuilds"),
            color: "#AECF55",
            renderHint: (s: IAdminStats) => <>{t("admin.dashboardPage.statCards.buildsHint", { published: s.publishedBuilds, pct: Math.round(s.publishedBuilds / Math.max(1, s.totalBuilds) * 100) })}</>,
        },
        {
            key: "totalReviews",
            label: t("admin.dashboardPage.statCards.totalReviews"),
            color: "#7AE07A",
            renderHint: () => <>{t("admin.dashboardPage.statCards.reviewsHint")}</>,
        },
        {
            key: "pendingReports",
            label: t("admin.dashboardPage.statCards.pendingReports"),
            color: "#FF5C5C",
            renderHint: (s: IAdminStats) => <>{t("admin.dashboardPage.statCards.bansHint_other", { count: s.activeBannedUsers })}</>,
        },
    ];

    const SPAN_OPTIONS = (["1", "7", "30"] as Span[]).map(v => ({
        value: v,
        label: t(`admin.dashboardPage.activity.spans.${v}`),
    }));

    if (statsLoading) return <div className={styles.page}><div className={styles.loading}>{t("admin.dashboardPage.loadingStats")}</div></div>;
    if (statsError || !stats) return <div className={styles.page}><div className={styles.error}>{statsError ?? t("admin.dashboardPage.noData")}</div></div>;

    const totalParts = Object.values(stats.componentCounts).reduce((a, b) => a + b, 0);

    return (
        <div className={styles.page}>
            {/* Stat cards */}
            <div className={styles.statRow}>
                {STAT_CARDS.map(c => (
                    <div className={styles.stat} key={c.key}>
                        <span className={styles.statTopline} style={{ background: c.color }} />
                        <div className={styles.statKey}>{c.label}</div>
                        <div className={styles.statValue}>{fmtNum((stats as unknown as Record<string, number>)[c.key] ?? 0)}</div>
                        <div className={styles.statHint}>{c.renderHint(stats)}</div>
                    </div>
                ))}
            </div>

            {/* Component catalog */}
            <section className={styles.section}>
                <div className={styles.sectionHead}>
                    <h2 className={styles.sectionTitle}>{t("admin.dashboardPage.catalog.title")}</h2>
                    <span className={styles.sectionMeta}>{t("admin.dashboardPage.catalog.meta", { count: fmtNum(totalParts), cats: Object.keys(stats.componentCounts).length })}</span>
                </div>
                <div className={styles.tiles}>
                    {Object.entries(stats.componentCounts).map(([key, count]) => (
                        <div className={styles.tile} key={key}>
                            <div className={styles.tileGlyph}>{componentGlyphs[key] ?? key.toUpperCase().slice(0, 3)}</div>
                            <div>
                                <div className={styles.tileCount}>{fmtNum(count)}</div>
                                <div className={styles.tileLabel}>{componentLabels[key] ?? key}</div>
                            </div>
                        </div>
                    ))}
                </div>
            </section>

            {/* Activity feed */}
            <section className={styles.section}>
                <div className={styles.sectionHead}>
                    <h2 className={styles.sectionTitle}>{t("admin.dashboardPage.activity.title")}</h2>
                    <div className={styles.sectionHeadRight}>
                        {activityPagination && (
                            <span className={styles.sectionMeta}>
                                {t("admin.dashboardPage.activity.event_other", { count: activityPagination.totalCount })}
                            </span>
                        )}
                        <Seg value={span} onChange={handleSpanChange} options={SPAN_OPTIONS} />
                    </div>
                </div>
                <div className={styles.feed}>
                    {activityLoading ? (
                        <div className={styles.feedEmpty}>{t("admin.dashboardPage.activity.loading")}</div>
                    ) : activity.length === 0 ? (
                        <div className={styles.feedEmpty}>{t("admin.dashboardPage.activity.empty")}</div>
                    ) : (
                        activity.map(a => (
                            <div className={styles.feedRow} key={a.id}>
                                <span className={styles.feedTime}>{fmtOccurred(a.occurredAt)}</span>
                                <span>
                                    <span className={styles.feedWho}>@{a.adminUsername}</span>{" "}
                                    <span className={styles.feedText}>{a.action}</span>
                                    {a.targetName && <>{" "}<span className={styles.feedTarget}>{a.targetName}</span></>}
                                    {a.detail && <>{" "}<span className={styles.feedDetail}>{a.detail}</span></>}
                                </span>
                                <span className={styles.dot} style={{ background: ACTION_DOT[a.action] ?? "#70707A" }} />
                            </div>
                        ))
                    )}
                </div>
                {activityPagination && activityPagination.totalPages > 1 && (
                    <Pagination
                        currentPage={page}
                        totalPages={activityPagination.totalPages}
                        totalResults={activityPagination.totalCount}
                        pageSize={PAGE_SIZE}
                        onPageChange={setPage}
                    />
                )}
            </section>
        </div>
    );
};

export default AdminDashboardPage;
