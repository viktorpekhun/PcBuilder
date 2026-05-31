import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { notificationService } from "../../api/notification.service";
import { priceAlertService } from "../../api/priceAlert.service";
import useNotifications from "../../hooks/useNotifications";
import type { INotification } from "../../types/notification.types";
import type { IUserPriceAlert } from "../../types/priceAlert.types";
import type { IPaginationHeader } from "../../types/admin.types";
import { Pagination } from "../../components/Pagination/Pagination";
import styles from "./NotificationsPage.module.css";

const PAGE_SIZE = 8;
type Tab = "inbox" | "alerts";
type Filter = "all" | "unread";

/* ── Helpers ─────────────────────────────────────────────────────── */
const MONTHS = ["JAN","FEB","MAR","APR","MAY","JUN","JUL","AUG","SEP","OCT","NOV","DEC"];

function shortDate(iso: string) {
    const d = new Date(iso);
    return `${d.getDate()} ${MONTHS[d.getMonth()]} ${d.getFullYear()}`;
}
function fullTime(iso: string) {
    const d = new Date(iso);
    const hh = String(d.getHours()).padStart(2, "0");
    const mm = String(d.getMinutes()).padStart(2, "0");
    return `${shortDate(iso)} ${hh}:${mm}`;
}
function dayKey(iso: string) {
    const d = new Date(iso);
    return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}
function relTime(iso: string) {
    const d = new Date(iso);
    const diffMs = Date.now() - d.getTime();
    const mins = Math.round(diffMs / 60000);
    if (mins < 1) return "JUST NOW";
    if (mins < 60) return `${mins}m AGO`;
    const hrs = Math.round(mins / 60);
    if (hrs < 24) return `${hrs}h AGO`;
    const days = Math.round(hrs / 24);
    if (days < 30) return `${days}d AGO`;
    return `${Math.round(days / 30)}mo AGO`;
}
function fmt(n: number) {
    return n.toLocaleString("uk-UA").replace(/,/g, " ");
}

const COMPONENT_TYPE_LABEL: Record<string, string> = {
    Cpu: "Processor", Gpu: "Graphics", Ram: "Memory",
    Motherboard: "Motherboard", CpuCooler: "CPU cooler",
    PcCase: "Case", PowerSupply: "Power supply",
    Ssd: "SSD", Hdd: "HDD", Fan: "Fan",
};
const COMPONENT_TYPE_SLOT: Record<string, string> = {
    Cpu: "CPU", Gpu: "GPU", Ram: "RAM", Motherboard: "M/B",
    CpuCooler: "CLR", PcCase: "CSE", PowerSupply: "PSU",
    Ssd: "SSD", Hdd: "HDD", Fan: "FAN",
};

/* ── Sparkline ───────────────────────────────────────────────────── */
const Spark = ({ dir }: { dir: string }) => {
    const pts =
        dir === "down" ? "0,4 6,5 12,3 18,6 24,2 30,4 36,1" :
        dir === "up"   ? "0,5 6,4 12,5 18,3 24,4 30,2 36,1" :
                         "0,3 6,4 12,3 18,4 24,3 30,4 36,3";
    return (
        <svg className={styles.spark} width="36" height="8" viewBox="0 0 36 8" preserveAspectRatio="none">
            <polyline points={pts} fill="none" strokeWidth="1.2" stroke="currentColor" />
        </svg>
    );
};

/* ── NotificationsPage ───────────────────────────────────────────── */
const NotificationsPage = () => {
    const navigate = useNavigate();
    const { markRead: ctxMarkRead, markAllRead: ctxMarkAllRead } = useNotifications();
    const [tab, setTab] = useState<Tab>("inbox");

    /* inbox state */
    const [items, setItems] = useState<INotification[]>([]);
    const [filter, setFilter] = useState<Filter>("all");
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [pageNumber, setPageNumber] = useState(1);
    const [loading, setLoading] = useState(true);

    /* alerts state */
    const [alerts, setAlerts] = useState<IUserPriceAlert[]>([]);
    const [alertsLoading, setAlertsLoading] = useState(false);
    const [alertsLoaded, setAlertsLoaded] = useState(false);

    /* fetch inbox */
    const fetchPage = useCallback(async (page: number, f: Filter) => {
        setLoading(true);
        try {
            const onlyUnread = f === "unread" ? true : undefined;
            const { items: data, pagination: pagi } =
                await notificationService.getNotificationsPaged(page, PAGE_SIZE, onlyUnread);
            setItems(data);
            setPagination(pagi);
        } catch {
            setItems([]);
            setPagination(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { fetchPage(pageNumber, filter); }, [fetchPage, pageNumber, filter]);

    /* fetch alerts lazily when switching to that tab */
    useEffect(() => {
        if (tab !== "alerts" || alertsLoaded) return;
        setAlertsLoading(true);
        priceAlertService.getMine()
            .then(r => setAlerts(r.data))
            .catch(() => setAlerts([]))
            .finally(() => { setAlertsLoading(false); setAlertsLoaded(true); });
    }, [tab, alertsLoaded]);

    const handleFilterChange = (next: Filter) => { setFilter(next); setPageNumber(1); };

    /* Use context so the sidebar count updates immediately */
    const markRead = (id: string) =>
        ctxMarkRead(id)
            .then(() => setItems(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n)));

    const markAllRead = () =>
        ctxMarkAllRead()
            .then(() => fetchPage(pageNumber, filter));

    const unsubscribe = (id: string) =>
        priceAlertService.unsubscribe(id)
            .then(() => setAlerts(prev => prev.filter(a => a.id !== id)))
            .catch(() => {});

    const handleItemClick = (n: INotification) => {
        if (!n.isRead) markRead(n.id);
        if (n.type === "NewReview" || n.type === "ReviewDeleted") {
            const buildId = n.payload?.buildId;
            if (buildId) navigate(`/builds/${buildId}`);
        }
    };

    const unreadCount = useMemo(() => items.filter(n => !n.isRead).length, [items]);
    const allRead = useMemo(() => items.every(n => n.isRead), [items]);

    /* group inbox by day */
    const grouped = useMemo(() => {
        const map = new Map<string, INotification[]>();
        items.forEach(n => {
            const k = dayKey(n.createdAt);
            if (!map.has(k)) map.set(k, []);
            map.get(k)!.push(n);
        });
        return Array.from(map.entries()).map(([key, ns]) => ({ key, items: ns }));
    }, [items]);

    return (
        <div className={styles.page}>
            {/* ── Header ── */}
            <div className={styles.head}>
                <div>
                    <span className={styles.eyebrow}>USER · NOTIFICATIONS</span>
                    <h1 className={styles.h1}>{tab === "inbox" ? "Notifications" : "Price alerts"}</h1>
                    <div className={styles.headMeta}>
                        {tab === "inbox" ? (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${unreadCount > 0 ? styles.dotAcc : styles.dotDim}`} />
                                    {unreadCount} UNREAD
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>○ {alerts.length} ACTIVE ALERTS</span>
                            </>
                        ) : (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${styles.dotAcc}`} />
                                    {alerts.length} ACTIVE
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>○ {unreadCount} UNREAD INBOX</span>
                            </>
                        )}
                    </div>
                </div>
                <div className={styles.headActions}>
                    {tab === "inbox" && (
                        <button
                            type="button"
                            className={`${styles.btn} ${styles.btnGhost}`}
                            onClick={markAllRead}
                            disabled={allRead}
                        >
                            ✓ Mark all read
                        </button>
                    )}
                </div>
            </div>

            {/* ── Primary tabs ── */}
            <div className={styles.tabs}>
                <button
                    type="button"
                    className={`${styles.tabBtn} ${tab === "inbox" ? styles.tabActive : ""}`}
                    onClick={() => setTab("inbox")}
                >
                    <span className={styles.tabIc}>●</span>
                    <span className={styles.tabLbl}>Notifications</span>
                    <span className={styles.tabCount}>
                        {unreadCount > 0
                            ? <><b>{unreadCount}</b><span className={styles.tabCountOf}>/{pagination?.totalCount ?? items.length}</span></>
                            : pagination?.totalCount ?? items.length}
                    </span>
                </button>
                <button
                    type="button"
                    className={`${styles.tabBtn} ${tab === "alerts" ? styles.tabActive : ""}`}
                    onClick={() => setTab("alerts")}
                >
                    <span className={styles.tabIc}>△</span>
                    <span className={styles.tabLbl}>Price alerts</span>
                    <span className={styles.tabCount}>{alerts.length}</span>
                </button>
                <div className={styles.tabSpacer} />
                <div className={styles.tabFoot}>
                    <span className={`${styles.dot} ${styles.dotOk}`} />
                    <span>FEED CONNECTED</span>
                </div>
            </div>

            {/* ── Panel ── */}
            <div className={styles.panel}>
                {tab === "inbox"
                    ? <InboxPanel
                        grouped={grouped}
                        items={items}
                        filter={filter}
                        onFilterChange={handleFilterChange}
                        loading={loading}
                        onMarkRead={markRead}
                        onItemClick={handleItemClick}
                    />
                    : <AlertsPanel
                        alerts={alerts}
                        loading={alertsLoading}
                        onUnsubscribe={unsubscribe}
                    />}
            </div>

            {/* ── Pagination ── */}
            {tab === "inbox" && (pagination?.totalPages ?? 1) > 1 && (
                <Pagination
                    currentPage={pageNumber}
                    totalPages={pagination!.totalPages}
                    totalResults={pagination!.totalCount}
                    pageSize={PAGE_SIZE}
                    onPageChange={setPageNumber}
                />
            )}
        </div>
    );
};

/* ── Inbox panel ─────────────────────────────────────────────────── */
interface InboxPanelProps {
    grouped: { key: string; items: INotification[] }[];
    items: INotification[];
    filter: Filter;
    onFilterChange: (f: Filter) => void;
    loading: boolean;
    onMarkRead: (id: string) => void;
    onItemClick: (n: INotification) => void;
}

const InboxPanel = ({
    grouped, items, filter, onFilterChange, loading, onMarkRead, onItemClick,
}: InboxPanelProps) => {
    const unreadCount = items.filter(n => !n.isRead).length;
    const totalCount = items.length;

    return (
        <>
            {/* toolbar */}
            <div className={styles.toolbar}>
                <div className={styles.filterChips}>
                    <button
                        type="button"
                        className={`${styles.fchip} ${filter === "all" ? styles.fchipActive : ""}`}
                        onClick={() => onFilterChange("all")}
                    >
                        <span className={`${styles.dot} ${styles.dotDim}`} />
                        ALL <span className={styles.fchipNum}>{totalCount}</span>
                    </button>
                    <button
                        type="button"
                        className={`${styles.fchip} ${filter === "unread" ? styles.fchipActive : ""}`}
                        onClick={() => onFilterChange("unread")}
                    >
                        <span className={`${styles.dot} ${unreadCount > 0 ? styles.dotAcc : styles.dotDim}`} />
                        UNREAD <span className={styles.fchipNum}>{unreadCount}</span>
                    </button>
                </div>
                <span className={styles.toolbarHint}>CLICK A ROW TO OPEN THE BUILD</span>
            </div>

            {/* list */}
            {loading ? (
                <div className={styles.empty}><span className={styles.emptyText}>LOADING…</span></div>
            ) : items.length === 0 ? (
                <EmptyState
                    eyebrow={filter === "unread" ? "INBOX ZERO" : "NO NOTIFICATIONS"}
                    title={filter === "unread" ? "You're all caught up." : "Nothing yet."}
                    body={filter === "unread"
                        ? "When someone reviews one of your published builds — or a review gets moderated — it lands here."
                        : "Publish a build to the gallery and reviews will start showing up here."}
                />
            ) : (
                <div className={styles.list}>
                    {grouped.map(group => (
                        <div key={group.key}>
                            <DayHeader iso={group.items[0]!.createdAt} count={group.items.length} />
                            {group.items.map(n => (
                                <NotificationRow
                                    key={n.id}
                                    n={n}
                                    onMarkRead={onMarkRead}
                                    onClick={onItemClick}
                                />
                            ))}
                        </div>
                    ))}
                </div>
            )}

        </>
    );
};

/* ── Day header ──────────────────────────────────────────────────── */
const DayHeader = ({ iso, count }: { iso: string; count: number }) => {
    const d = new Date(iso);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today); yesterday.setDate(today.getDate() - 1);
    const dd = new Date(d); dd.setHours(0, 0, 0, 0);
    let label: string;
    if (dd.getTime() === today.getTime()) label = "TODAY";
    else if (dd.getTime() === yesterday.getTime()) label = "YESTERDAY";
    else label = shortDate(iso).toUpperCase();
    return (
        <div className={styles.dayHeader}>
            <span className={styles.dayTitle}>{label}</span>
            <span className={styles.dayLine} />
            <span className={styles.dayCnt}>{count} {count === 1 ? "ITEM" : "ITEMS"}</span>
        </div>
    );
};

/* ── Notification row ────────────────────────────────────────────── */
const NotificationRow = ({
    n,
    onMarkRead,
    onClick,
}: {
    n: INotification;
    onMarkRead: (id: string) => void;
    onClick: (n: INotification) => void;
}) => {
    const isReview = n.type === "NewReview";
    const isDeleted = n.type === "ReviewDeleted";
    const rating = isReview ? Math.min(5, Math.max(0, parseInt(n.payload?.rating ?? "0", 10))) : 0;

    return (
        <div
            className={`${styles.row} ${n.isRead ? styles.rowRead : ""}`}
            onClick={() => onClick(n)}
        >
            <span className={`${styles.bullet} ${n.isRead ? styles.bulletRead : ""}`}>
                {n.isRead ? "○" : "●"}
            </span>
            <div className={styles.rowGlyph}>
                <span className={`${styles.rowType} ${isReview ? styles.typeReview : isDeleted ? styles.typeDeleted : ""}`}>
                    {isReview ? "REVIEW" : isDeleted ? "DELETED" : n.type.toUpperCase()}
                </span>
                {isReview && (
                    <span className={styles.stars}>
                        {"★".repeat(rating)}
                        <span className={styles.starsDim}>{"★".repeat(5 - rating)}</span>
                    </span>
                )}
            </div>
            <div className={styles.rowBody}>
                <div className={styles.rowText}>
                    {isReview && (
                        <>
                            <strong>{n.payload?.reviewerUsername ?? "Someone"}</strong>
                            {" left a "}
                            <strong>{rating}-star</strong>
                            {" review on "}
                            <span className={styles.rowLink}>/{n.payload?.buildName ?? "a build"}</span>
                        </>
                    )}
                    {isDeleted && (
                        <>
                            {"A review on "}
                            <span className={styles.rowLink}>/{n.payload?.buildName ?? "a build"}</span>
                            {" was removed by moderation"}
                            {n.payload?.reason && (
                                <span className={styles.rowReason}> · {String(n.payload.reason).replace(/_/g, " ")}</span>
                            )}
                        </>
                    )}
                    {!isReview && !isDeleted && (
                        <span>{n.payload?.buildName ?? n.type}</span>
                    )}
                </div>
                <div className={styles.rowMeta}>
                    <span>◴ {relTime(n.createdAt)}</span>
                    <span className={styles.metaSep}>·</span>
                    <span>{fullTime(n.createdAt)}</span>
                    {n.payload?.buildId && (
                        <>
                            <span className={styles.metaSep}>·</span>
                            <span>/builds/{n.payload.buildId}</span>
                        </>
                    )}
                </div>
            </div>
            <div className={styles.rowActions}>
                {!n.isRead && (
                    <button
                        type="button"
                        className={styles.actBtn}
                        onClick={e => { e.stopPropagation(); onMarkRead(n.id); }}
                    >
                        ✓ MARK READ
                    </button>
                )}
                <span className={styles.openBtn}>↗ OPEN</span>
            </div>
        </div>
    );
};


/* ── Alerts panel ────────────────────────────────────────────────── */
const AlertsPanel = ({
    alerts, loading, onUnsubscribe,
}: {
    alerts: IUserPriceAlert[];
    loading: boolean;
    onUnsubscribe: (id: string) => void;
}) => {
    if (loading) {
        return <div className={styles.empty}><span className={styles.emptyText}>LOADING…</span></div>;
    }
    if (alerts.length === 0) {
        return (
            <EmptyState
                eyebrow="NO SUBSCRIPTIONS"
                title="You haven't subscribed to any component yet."
                body="Open any component page and hit the ALERT chip on a row to track its average price. You'll get a notification when it moves more than your threshold."
            />
        );
    }

    return (
        <>
            <div className={styles.alertsLegend}>
                <span className={styles.alertsLegLbl}>SUBSCRIPTIONS · {alerts.length}</span>
                <span className={styles.lgItem}><span className={styles.lgDown}>▼</span> AVG PRICE DROP (BUY SIGNAL)</span>
                <span className={styles.lgItem}><span className={styles.lgUp}>▲</span> AVG PRICE UP</span>
                <span className={styles.lgItem}><span>•</span> WITHIN THRESHOLD</span>
            </div>

            <div className={styles.alertsTable}>
                <div className={styles.alertHead}>
                    <span>SLOT</span>
                    <span></span>
                    <span>COMPONENT</span>
                    <span>THRESHOLD</span>
                    <span className={styles.r}>BASELINE · ₴</span>
                    <span className={styles.r}>CURRENT · ₴</span>
                    <span className={styles.r}>DELTA</span>
                    <span></span>
                </div>
                {alerts.map(a => <AlertRow key={a.id} a={a} onUnsubscribe={onUnsubscribe} />)}
            </div>

            <div className={styles.alertsFoot}>
                <span>NOTIFICATIONS FIRE WHEN |Δ| ≥ THRESHOLD · ROUNDED TO NEAREST HRYVNIA</span>
                <span className={styles.alertsFootDim}>DATA REFRESHED HOURLY FROM PARTNER STORES</span>
            </div>
        </>
    );
};

const AlertRow = ({
    a,
    onUnsubscribe,
}: {
    a: IUserPriceAlert;
    onUnsubscribe: (id: string) => void;
}) => {
    const slot = COMPONENT_TYPE_SLOT[a.componentType] ?? "?";
    const label = COMPONENT_TYPE_LABEL[a.componentType] ?? a.componentType;
    const current = a.currentAveragePrice;
    const baseline = a.initialPrice;
    const deltaPercent = current != null && baseline > 0
        ? ((current - baseline) / baseline) * 100
        : null;
    const overThreshold = deltaPercent != null && Math.abs(deltaPercent) >= a.thresholdPercent;
    const dir = deltaPercent == null ? "n"
        : deltaPercent > 0.1 ? "up"
        : deltaPercent < -0.1 ? "down"
        : "flat";
    const fillWidth = deltaPercent != null
        ? Math.min(100, (Math.abs(deltaPercent) / Math.max(a.thresholdPercent * 2, 1)) * 100)
        : 0;

    return (
        <div className={styles.alertRow}>
            <span className={styles.alertSlot}>{slot}</span>
            <div className={styles.alertThumb}>
                {a.componentImageUrl && (
                    <img src={a.componentImageUrl} alt={a.componentName ?? ""} className={styles.alertThumbImg} />
                )}
            </div>
            <div className={styles.alertNm}>
                <div className={styles.alertName}>{a.componentName ?? "—"}</div>
                <div className={styles.alertNmMeta}>
                    {label.toUpperCase()} · SUBSCRIBED {shortDate(a.createdAt)} · ±{a.thresholdPercent}%
                </div>
            </div>
            <div className={styles.thrWrap}>
                <div className={styles.thrBar}>
                    <div className={styles.thrFill} style={{ width: `${fillWidth}%` }} />
                    <div className={styles.thrMid} />
                </div>
                <span className={styles.thrNum}>±{a.thresholdPercent}%</span>
            </div>
            <div className={styles.alertPrice}>
                <span className={styles.alertCcy}>₴</span>{fmt(Math.round(baseline))}
            </div>
            <div className={styles.alertPrice}>
                {current != null
                    ? <><span className={styles.alertCcy}>₴</span>{fmt(Math.round(current))}</>
                    : <span className={styles.alertGhost}>UNAVAIL.</span>}
            </div>
            <div className={`${styles.delta} ${styles[`dir${dir.charAt(0).toUpperCase()}${dir.slice(1)}`]} ${overThreshold ? styles.deltaFire : ""}`}>
                {dir === "n"
                    ? <span className={styles.alertGhost}>—</span>
                    : <>
                        <span className={styles.deltaArr}>{dir === "up" ? "▲" : dir === "down" ? "▼" : "•"}</span>
                        <span className={styles.deltaNum}>{Math.abs(deltaPercent ?? 0).toFixed(1)}%</span>
                        <Spark dir={dir} />
                    </>}
                {overThreshold && <span className={styles.fireTag}>FIRES</span>}
            </div>
            <div className={styles.alertActs}>
                <button
                    type="button"
                    className={styles.unsubBtn}
                    onClick={() => onUnsubscribe(a.id)}
                    title="Unsubscribe"
                >
                    × UNSUB
                </button>
            </div>
        </div>
    );
};

/* ── Empty state ─────────────────────────────────────────────────── */
const EmptyState = ({ eyebrow, title, body }: { eyebrow: string; title: string; body: string }) => (
    <div className={styles.empty}>
        <div className={styles.emptyCard}>
            <span className={styles.eyebrow}>{eyebrow}</span>
            <h2 className={styles.emptyTitle}>{title}</h2>
            <p className={styles.emptyBody}>{body}</p>
            <pre className={styles.emptyAscii}>{`┌─────────────┐\n│  •  •  •  • │\n└─────────────┘`}</pre>
        </div>
    </div>
);

export default NotificationsPage;
