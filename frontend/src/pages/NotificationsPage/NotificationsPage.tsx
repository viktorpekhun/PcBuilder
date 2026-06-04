import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
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
function shortDate(iso: string, locale: string) {
    return new Intl.DateTimeFormat(locale, { day: "2-digit", month: "short", year: "numeric" }).format(new Date(iso));
}
function fullTime(iso: string, locale: string) {
    return new Intl.DateTimeFormat(locale, { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit", hour12: false }).format(new Date(iso));
}
function dayKey(iso: string) {
    const d = new Date(iso);
    return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}
function fmt(n: number) {
    return n.toLocaleString("uk-UA").replace(/,/g, " ");
}

type TagKind = "review" | "deleted" | "price" | "warn" | "ban";
function getTagKind(type: string): TagKind {
    if (type === "NewReview") return "review";
    if (type === "ReviewDeleted" || type === "BuildDeleted") return "deleted";
    if (type === "PriceAlert") return "price";
    if (type === "CommentBanned" || type === "PostBanned") return "ban";
    return "warn";
}

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
    const { t } = useTranslation();
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
                    <span className={styles.eyebrow}>{t("notifications.eyebrow")}</span>
                    <h1 className={styles.h1}>{tab === "inbox" ? t("notifications.heading") : t("notifications.priceAlertsHeading")}</h1>
                    <div className={styles.headMeta}>
                        {tab === "inbox" ? (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${unreadCount > 0 ? styles.dotAcc : styles.dotDim}`} />
                                    {t("notifications.unread", { count: unreadCount })}
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>{t("notifications.activeAlerts", { count: alerts.length })}</span>
                            </>
                        ) : (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${styles.dotAcc}`} />
                                    {t("notifications.active", { count: alerts.length })}
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>{t("notifications.unreadInbox", { count: unreadCount })}</span>
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
                            {t("notifications.markAllRead")}
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
                    <span className={styles.tabLbl}>{t("notifications.tabs.notifications")}</span>
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
                    <span className={styles.tabLbl}>{t("notifications.tabs.priceAlerts")}</span>
                    <span className={styles.tabCount}>{alerts.length}</span>
                </button>
                <div className={styles.tabSpacer} />
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
    const { t } = useTranslation();
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
                        {t("notifications.filter.all")} <span className={styles.fchipNum}>{totalCount}</span>
                    </button>
                    <button
                        type="button"
                        className={`${styles.fchip} ${filter === "unread" ? styles.fchipActive : ""}`}
                        onClick={() => onFilterChange("unread")}
                    >
                        <span className={`${styles.dot} ${unreadCount > 0 ? styles.dotAcc : styles.dotDim}`} />
                        {t("notifications.filter.unread")} <span className={styles.fchipNum}>{unreadCount}</span>
                    </button>
                </div>
                <span className={styles.toolbarHint}>{t("notifications.toolbar.clickToOpen")}</span>
            </div>

            {/* list */}
            {loading ? (
                <div className={styles.empty}><span className={styles.emptyText}>{t("notifications.loading")}</span></div>
            ) : items.length === 0 ? (
                <EmptyState
                    eyebrow={filter === "unread" ? t("notifications.empty.inboxZeroEyebrow") : t("notifications.empty.noNotifsEyebrow")}
                    title={filter === "unread" ? t("notifications.empty.inboxZeroTitle") : t("notifications.empty.noNotifsTitle")}
                    body={filter === "unread"
                        ? t("notifications.empty.inboxZeroBody")
                        : t("notifications.empty.noNotifsBody")}
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
    const { t, i18n } = useTranslation();
    const d = new Date(iso);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today); yesterday.setDate(today.getDate() - 1);
    const dd = new Date(d); dd.setHours(0, 0, 0, 0);
    let label: string;
    if (dd.getTime() === today.getTime()) label = t("notifications.row.today");
    else if (dd.getTime() === yesterday.getTime()) label = t("notifications.row.yesterday");
    else label = shortDate(iso, i18n.language).toUpperCase();
    return (
        <div className={styles.dayHeader}>
            <span className={styles.dayTitle}>{label}</span>
            <span className={styles.dayLine} />
            <span className={styles.dayCnt}>{t("notifications.row.item", { count })}</span>
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
    const { t, i18n } = useTranslation();
    const isReview = n.type === "NewReview";
    const rating = isReview ? Math.min(5, Math.max(0, parseInt(n.payload?.rating ?? "0", 10))) : 0;
    const tagKindClass: Record<TagKind, string> = {
        review:  styles.typeReview!,
        deleted: styles.typeDeleted!,
        price:   styles.typePrice!,
        warn:    styles.typeWarn!,
        ban:     styles.typeBan!,
    };

    function relTime(iso: string) {
        const d = new Date(iso);
        const diffMs = Date.now() - d.getTime();
        const mins = Math.round(diffMs / 60000);
        if (mins < 1) return t("notifications.row.justNow");
        if (mins < 60) return t("notifications.row.minutesAgo", { count: mins });
        const hrs = Math.round(mins / 60);
        if (hrs < 24) return t("notifications.row.hoursAgo", { count: hrs });
        const days = Math.round(hrs / 24);
        if (days < 30) return t("notifications.row.daysAgo", { count: days });
        return t("notifications.row.monthsAgo", { count: Math.round(days / 30) });
    }

    const renderRowText = () => {
        const reason = n.payload?.reason;
        const reasonLabel = reason
            ? t(`notifications.row.reasons.${reason}`, { defaultValue: reason.replace(/_/g, " ") })
            : null;

        switch (n.type) {
            case "NewReview":
                return <>
                    <strong>{n.payload?.reviewerUsername ?? "Someone"}</strong>
                    {t("notifications.row.leftReview")}
                    {t("notifications.row.reviewOn")}
                    <span className={styles.rowLink}>/{n.payload?.buildName ?? "a build"}</span>
                </>;
            case "ReviewDeleted":
                return <>
                    {t("notifications.row.reviewRemovedOn")}
                    <span className={styles.rowLink}>/{n.payload?.buildName ?? "a build"}</span>
                    {t("notifications.row.reviewRemovedBy")}
                    {reason && <span className={styles.rowReason}> · {reasonLabel}</span>}
                </>;
            case "BuildDeleted":
                return <>
                    {t("notifications.row.buildDeletedOn")}
                    <span className={styles.rowLink}>/{n.payload?.buildName ?? "a build"}</span>
                    {t("notifications.row.buildDeletedBy")}
                </>;
            case "CommentBanned":
            case "PostBanned":
                return <>
                    {n.type === "CommentBanned"
                        ? t("notifications.row.restrictedFromCommenting", { date: n.payload?.banUntil ? fullTime(n.payload.banUntil, i18n.language) : t("notifications.row.unknownDate") })
                        : t("notifications.row.restrictedFromPosting",    { date: n.payload?.banUntil ? fullTime(n.payload.banUntil, i18n.language) : t("notifications.row.unknownDate") })}
                    {reason && reason !== "AUTO_BAN_WARNINGS" && <span className={styles.rowReason}> · {reasonLabel}</span>}
                </>;
            case "CommentUnbanned":
                return <span>{t("notifications.row.commentUnbanned")}</span>;
            case "PostUnbanned":
                return <span>{t("notifications.row.postUnbanned")}</span>;
            case "CommentWarning":
            case "PostWarning":
                return <>{n.type === "CommentWarning" ? t("notifications.row.commentWarning") : t("notifications.row.postWarning")}</>;
            case "PriceAlert": {
                const newPrice = n.payload?.newPrice ? fmt(Math.round(parseFloat(n.payload.newPrice))) : "?";
                const dir = n.payload?.direction ?? "up";
                return <>
                    <strong>{n.payload?.componentName ?? "?"}</strong>
                    {" "}{t(`notifications.row.priceDirection.${dir}`)}{" "}
                    <strong>₴{newPrice}</strong>
                </>;
            }
            default:
                return <span>{n.type}</span>;
        }
    };

    return (
        <div
            className={`${styles.row} ${n.isRead ? styles.rowRead : ""}`}
            onClick={() => onClick(n)}
        >
            <span className={`${styles.bullet} ${n.isRead ? styles.bulletRead : ""}`}>
                {n.isRead ? "○" : "●"}
            </span>
            <div className={styles.rowGlyph}>
                <span className={`${styles.rowType} ${tagKindClass[getTagKind(n.type)]}`}>
                    {t(`notifications.row.types.${n.type.charAt(0).toLowerCase() + n.type.slice(1)}`, { defaultValue: n.type.toUpperCase() })}
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
                    {renderRowText()}
                </div>
                <div className={styles.rowMeta}>
                    <span>◴ {relTime(n.createdAt)}</span>
                    <span className={styles.metaSep}>·</span>
                    <span>{fullTime(n.createdAt, i18n.language)}</span>
                    {(n.type === "CommentBanned" || n.type === "PostBanned") && n.payload?.banUntil && (
                        <>
                            <span className={styles.metaSep}>·</span>
                            <span>{t("notifications.row.bannedUntil", { date: shortDate(n.payload.banUntil, i18n.language) })}</span>
                        </>
                    )}
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
                        {t("notifications.row.markRead")}
                    </button>
                )}
                <span className={styles.openBtn}>{t("notifications.row.open")}</span>
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
    const { t } = useTranslation();
    if (loading) {
        return <div className={styles.empty}><span className={styles.emptyText}>{t("notifications.loading")}</span></div>;
    }
    if (alerts.length === 0) {
        return (
            <EmptyState
                eyebrow={t("notifications.empty.noAlertsEyebrow")}
                title={t("notifications.empty.noAlertsTitle")}
                body={t("notifications.empty.noAlertsBody")}
            />
        );
    }

    return (
        <>
            <div className={styles.alertsLegend}>
                <span className={styles.alertsLegLbl}>{t("notifications.alerts.legend", { count: alerts.length })}</span>
                <span className={styles.lgItem}><span className={styles.lgDown}>▼</span> {t("notifications.alerts.priceDown")}</span>
                <span className={styles.lgItem}><span className={styles.lgUp}>▲</span> {t("notifications.alerts.priceUp")}</span>
                <span className={styles.lgItem}><span>•</span> {t("notifications.alerts.withinThreshold")}</span>
            </div>

            <div className={styles.alertsTable}>
                <div className={styles.alertHead}>
                    <span>{t("notifications.alerts.headers.slot")}</span>
                    <span></span>
                    <span>{t("notifications.alerts.headers.component")}</span>
                    <span>{t("notifications.alerts.headers.threshold")}</span>
                    <span className={styles.r}>{t("notifications.alerts.headers.baseline")}</span>
                    <span className={styles.r}>{t("notifications.alerts.headers.current")}</span>
                    <span className={styles.r}>{t("notifications.alerts.headers.delta")}</span>
                    <span></span>
                </div>
                {alerts.map(a => <AlertRow key={a.id} a={a} onUnsubscribe={onUnsubscribe} />)}
            </div>

            <div className={styles.alertsFoot}>
                <span>{t("notifications.alerts.foot")}</span>
                <span className={styles.alertsFootDim}>{t("notifications.alerts.footRefresh")}</span>
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
    const { t, i18n } = useTranslation();
    const slot = COMPONENT_TYPE_SLOT[a.componentType] ?? "?";
    const label = t(`autoBuilder.componentLabels.${a.componentType.charAt(0).toLowerCase() + a.componentType.slice(1)}`, { defaultValue: a.componentType });
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
                    {t("notifications.alerts.subscribedMeta", {
                        label: label.toUpperCase(),
                        date: shortDate(a.createdAt, i18n.language),
                        pct: a.thresholdPercent,
                    })}
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
                    : <span className={styles.alertGhost}>{t("notifications.alerts.unavail")}</span>}
            </div>
            <div className={`${styles.delta} ${styles[`dir${dir.charAt(0).toUpperCase()}${dir.slice(1)}`]} ${overThreshold ? styles.deltaFire : ""}`}>
                {dir === "n"
                    ? <span className={styles.alertGhost}>—</span>
                    : <>
                        <span className={styles.deltaArr}>{dir === "up" ? "▲" : dir === "down" ? "▼" : "•"}</span>
                        <span className={styles.deltaNum}>{Math.abs(deltaPercent ?? 0).toFixed(1)}%</span>
                        <Spark dir={dir} />
                    </>}
                {overThreshold && <span className={styles.fireTag}>{t("notifications.alerts.fires")}</span>}
            </div>
            <div className={styles.alertActs}>
                <button
                    type="button"
                    className={styles.unsubBtn}
                    onClick={() => onUnsubscribe(a.id)}
                    title={t("notifications.alerts.unsub")}
                >
                    {t("notifications.alerts.unsub")}
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
