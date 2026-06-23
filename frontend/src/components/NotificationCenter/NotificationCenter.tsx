import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import useNotifications from "../../hooks/useNotifications";
import type { INotification } from "../../types/notification.types";
import styles from "./NotificationCenter.module.css";

/* ── helpers ── */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function translateReason(reason: string, warningsCount: string | undefined, t: (k: string, o?: any) => string): string {
    if (reason === "AUTO_BAN_WARNINGS")
        return t("notifications.row.autoBanWarnings", { count: Number(warningsCount) || 0 });
    return reason.replace(/_/g, " ").toLowerCase();
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function relTime(iso: string, t: (k: string, o?: any) => string): string {
    const mins = Math.round((Date.now() - new Date(iso).getTime()) / 60_000);
    if (mins < 1) return t("notifications.row.justNow");
    if (mins < 60) return t("notifications.row.minutesAgo", { count: mins });
    const hrs = Math.round(mins / 60);
    if (hrs < 24) return t("notifications.row.hoursAgo", { count: hrs });
    return t("notifications.row.daysAgo", { count: Math.round(hrs / 24) });
}

function dayKey(iso: string): string {
    const d = new Date(iso);
    return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function dayLabel(iso: string, t: (k: string, o?: any) => string, locale: string): string {
    const d = new Date(iso);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today); yesterday.setDate(today.getDate() - 1);
    const dd = new Date(d); dd.setHours(0, 0, 0, 0);
    if (dd.getTime() === today.getTime()) return t("notifications.row.today");
    if (dd.getTime() === yesterday.getTime()) return t("notifications.row.yesterday");
    return new Intl.DateTimeFormat(locale, { day: "2-digit", month: "short" }).format(d).toUpperCase();
}

/* ── bell icon ── */
const BellIcon = () => (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5"
         strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M10.268 21a2 2 0 0 0 3.464 0" />
        <path d="M3.262 15.326A1 1 0 0 0 4 17h16a1 1 0 0 0 .74-1.673C19.41 13.956 18 12.499 18 8A6 6 0 0 0 6 8c0 4.499-1.411 5.956-2.738 7.326" />
    </svg>
);

/* ── type tag metadata ── */
type TagKind = "review" | "deleted" | "price" | "warn" | "ban";

function getTagKind(type: string): TagKind {
    if (type === "NewReview") return "review";
    if (type === "ReviewDeleted" || type === "BuildDeleted") return "deleted";
    if (type === "PriceAlert") return "price";
    if (type === "CommentBanned" || type === "PostBanned") return "ban";
    return "warn";
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function getTagLabel(type: string, t: (k: string, o?: any) => string): string {
    const key = type.charAt(0).toLowerCase() + type.slice(1);
    return t(`notifications.row.types.${key}`, { defaultValue: type.toUpperCase() });
}

const tagKindClass: Record<TagKind, string | undefined> = {
    review:  styles.typeTagReview,
    deleted: styles.typeTagDeleted,
    price:   styles.typeTagPrice,
    warn:    styles.typeTagWarn,
    ban:     styles.typeTagBan,
};

/* ── notification path ── */
function notifPath(n: INotification): string {
    const p = n.payload;
    if (n.type === "PriceAlert") {
        const ct = (p.componentType || "").toLowerCase();
        return `/components/${ct}/${p.componentId || ""}`;
    }
    return `/builds/${p.buildId || ""}`;
}

/* ── notification text (rich) ── */
function NotifText({ n }: { n: INotification }) {
    const { t, i18n } = useTranslation();
    const p = n.payload;
    if (n.type === "NewReview") {
        return (
            <>
                <strong>{p.reviewerUsername ?? "Someone"}</strong>
                {t("notifications.row.leftReview")}
                {t("notifications.row.reviewOn")}
                <span className={styles.textLink}>/{p.buildName ?? "a build"}</span>
            </>
        );
    }
    if (n.type === "ReviewDeleted" || n.type === "BuildDeleted") {
        const label = n.type === "BuildDeleted" ? "build" : t("notifications.row.review").toLowerCase();
        return (
            <>
                Your {label}{" "}
                <span className={styles.textLink}>/{p.buildName ?? ""}</span>
                {" "}was removed by moderation
                {p.reason && (
                    <span className={styles.reason}> · {translateReason(p.reason, p.warningsCount, t)}</span>
                )}
            </>
        );
    }
    if (n.type === "PriceAlert") {
        const oldP = Number(p.oldPrice);
        const newP = Number(p.newPrice);
        const hasNums = Number.isFinite(oldP) && Number.isFinite(newP) && oldP > 0;
        const deltaPct = hasNums ? Math.abs((newP - oldP) / oldP * 100).toFixed(1) : null;
        const fmt = (v: number) => v.toLocaleString("uk-UA", { maximumFractionDigits: 0 });
        return (
            <>
                <strong>{p.componentName ?? "Component"}</strong>{" "}
                price {p.direction === "down" ? "dropped" : "rose"}
                {deltaPct && <span className={styles.drop}> ▼ {deltaPct}%</span>}
                {hasNums && (
                    <>
                        {" "}<span className={styles.price}><span className={styles.priceCcy}>₴</span>{fmt(oldP)}</span>
                        {" → "}
                        <span className={styles.price}><span className={styles.priceCcy}>₴</span>{fmt(newP)}</span>
                    </>
                )}
            </>
        );
    }
    if (n.type === "CommentBanned" || n.type === "PostBanned") {
        const until = p.banUntil
            ? new Intl.DateTimeFormat(i18n.language, { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit", hour12: false }).format(new Date(p.banUntil))
            : t("notifications.row.unknownDate");
        const restrictedKey = n.type === "CommentBanned"
            ? "notifications.row.restrictedFromCommenting"
            : "notifications.row.restrictedFromPosting";
        return <>{t(restrictedKey, { date: until })}{p.reason && p.reason !== "AUTO_BAN_WARNINGS" && <span className={styles.reason}> · {p.reason.replace(/_/g, " ").toLowerCase()}</span>}</>;
    }
    if (n.type === "CommentUnbanned") return <>{t("notifications.row.commentUnbanned")}</>;
    if (n.type === "PostUnbanned") return <>{t("notifications.row.postUnbanned")}</>;
    if (n.type === "CommentWarning") return <>{t("notifications.row.commentWarning")}</>;
    if (n.type === "PostWarning") return <>{t("notifications.row.postWarning")}</>;
    return <>{t("notifications.row.newNotification")}</>;
}

/* ── item ── */
function Item({ n, onMarkRead, onNavigate }: {
    n: INotification;
    onMarkRead: (id: string) => void;
    onNavigate: (n: INotification) => void;
}) {
    const { t } = useTranslation();
    const kind = getTagKind(n.type);
    const rating = n.type === "NewReview" ? Number(n.payload.rating) || 0 : 0;

    return (
        <button
            className={`${styles.item} ${n.isRead ? styles.itemRead : ""}`}
            onClick={() => onNavigate(n)}
        >
            <span className={styles.bullet} aria-hidden="true">{n.isRead ? "○" : "●"}</span>
            <div className={styles.main}>
                <div className={styles.line1}>
                    <span className={`${styles.typeTag} ${tagKindClass[kind]}`}>{getTagLabel(n.type, t)}</span>
                    {n.type === "NewReview" && rating > 0 && (
                        <span className={styles.stars}>
                            {"★".repeat(rating)}
                            <span className={styles.starsDim}>{"★".repeat(5 - rating)}</span>
                        </span>
                    )}
                    <span className={styles.time}>{relTime(n.createdAt, t)}</span>
                </div>

                <div className={styles.text}>
                    <NotifText n={n} />
                </div>

                <div className={styles.foot}>
                    <span className={styles.path}>{notifPath(n)}</span>
                    <span className={styles.rowActs}>
                        {!n.isRead && (
                            <button className={styles.miniBtn}
                                    onClick={e => { e.stopPropagation(); onMarkRead(n.id); }}>
                                {t("notifications.row.markRead")}
                            </button>
                        )}
                        <span className={styles.openHint}>{t("notifications.row.open")}</span>
                    </span>
                </div>
            </div>
        </button>
    );
}

/* ── grouped list ── */
function List({ items, onMarkRead, onNavigate }: {
    items: INotification[];
    onMarkRead: (id: string) => void;
    onNavigate: (n: INotification) => void;
}) {
    const { t, i18n } = useTranslation();

    const groups = useMemo(() => {
        const map = new Map<string, { label: string; items: INotification[] }>();
        items.forEach(n => {
            const k = dayKey(n.createdAt);
            if (!map.has(k)) map.set(k, { label: dayLabel(n.createdAt, t, i18n.language), items: [] });
            map.get(k)!.items.push(n);
        });
        return Array.from(map.entries());
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [items]);

    return (
        <div className={styles.list}>
            {groups.map(([key, g]) => (
                <div key={key}>
                    <div className={styles.dayRow}>
                        <span className={styles.dayLabel}>{g.label}</span>
                        <span className={styles.dayLine} />
                        <span className={styles.dayCount}>{g.items.length}</span>
                    </div>
                    {g.items.map(n => (
                        <Item key={n.id} n={n} onMarkRead={onMarkRead} onNavigate={onNavigate} />
                    ))}
                </div>
            ))}
        </div>
    );
}

/* ── empty state ── */
function EmptyState({ filter }: { filter: "all" | "unread" }) {
    const { t } = useTranslation();
    return (
        <div className={styles.empty}>
            <div className={styles.emptyCard}>
                <span className={styles.emptyEyebrow}>
                    {filter === "unread" ? t("notifications.empty.inboxZeroEyebrow") : t("notifications.empty.noNotifsEyebrow")}
                </span>
                <h3 className={styles.emptyTitle}>
                    {filter === "unread" ? t("notifications.empty.inboxZeroTitle") : t("notifications.empty.noNotifsTitle")}
                </h3>
                <p className={styles.emptyDesc}>
                    {filter === "unread" ? t("notifications.empty.inboxZeroBody") : t("notifications.empty.noNotifsBody")}
                </p>
                <span className={styles.emptyDots}>{"●  ●  ●"}</span>
            </div>
        </div>
    );
}

/* ── root ── */
export default function NotificationCenter() {
    const { t } = useTranslation();
    const {
        notifications,
        unreadCount,
        isOpen,
        setIsOpen,
        fetchNotifications,
        markRead,
        markAllRead,
    } = useNotifications();
    const navigate = useNavigate();
    const mountRef = useRef<HTMLDivElement>(null);
    const [filter, setFilter] = useState<"all" | "unread">("all");

    useEffect(() => {
        if (!isOpen) return;
        const onDown = (e: MouseEvent) => {
            if (mountRef.current && !mountRef.current.contains(e.target as Node)) {
                setIsOpen(false);
            }
        };
        const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") setIsOpen(false); };
        document.addEventListener("mousedown", onDown);
        document.addEventListener("keydown", onKey);
        return () => {
            document.removeEventListener("mousedown", onDown);
            document.removeEventListener("keydown", onKey);
        };
    }, [isOpen, setIsOpen]);

    const handleBellClick = () => {
        if (!isOpen) fetchNotifications();
        setIsOpen(!isOpen);
    };

    const handleNavigate = (n: INotification) => {
        if (!n.isRead) markRead(n.id);
        if (n.type === "NewReview" || n.type === "ReviewDeleted" || n.type === "BuildDeleted") {
            const buildId = n.payload?.buildId;
            if (buildId) { navigate(`/builds/${buildId}`); setIsOpen(false); }
        } else if (n.type === "PriceAlert") {
            const componentId = n.payload?.componentId;
            const componentType = n.payload?.componentType;
            if (componentId && componentType) {
                navigate(`/components/${componentType.toLowerCase()}/${componentId}`);
                setIsOpen(false);
            }
        }
    };

    const visible = filter === "unread"
        ? notifications.filter(n => !n.isRead)
        : notifications;

    return (
        <div className={styles.mount} ref={mountRef}>
            <button
                className={`${styles.bell} ${isOpen ? styles.open : ""}`}
                onClick={handleBellClick}
                aria-label={t("notifications.heading")}
                aria-expanded={isOpen}
            >
                <BellIcon />
                {unreadCount > 0 && (
                    <span className={styles.badge}>{unreadCount > 99 ? "99+" : unreadCount}</span>
                )}
            </button>

            {isOpen && (
                <div className={styles.pop} role="dialog" aria-label={t("notifications.heading")}>
                    {/* header */}
                    <div className={styles.head}>
                        <div className={styles.titleWrap}>
                            <h2 className={styles.title}>{t("notifications.heading")}</h2>
                        </div>
                        <span className={styles.spacer} />
                        <button className={styles.markAllBtn} onClick={markAllRead} disabled={unreadCount === 0}>
                            {t("notifications.markAllRead")}
                        </button>
                    </div>

                    {/* filters */}
                    <div className={styles.filters}>
                        <button
                            className={`${styles.chip} ${filter === "all" ? styles.chipActive : ""}`}
                            onClick={() => setFilter("all")}
                        >
                            <span className={styles.chipDot} style={{ background: "var(--bd-2)" }} />
                            {t("notifications.filter.all")} <span className={styles.chipNum}>{notifications.length}</span>
                        </button>
                        <button
                            className={`${styles.chip} ${filter === "unread" ? styles.chipActive : ""}`}
                            onClick={() => setFilter("unread")}
                        >
                            <span className={styles.chipDot}
                                  style={{ background: unreadCount > 0 ? "var(--acc)" : "var(--bd-2)" }} />
                            {t("notifications.filter.unread")} <span className={styles.chipNum}>{unreadCount}</span>
                        </button>
                    </div>

                    {/* list or empty */}
                    {visible.length === 0
                        ? <EmptyState filter={filter} />
                        : <List items={visible} onMarkRead={markRead} onNavigate={handleNavigate} />
                    }

                    {/* footer */}
                    <div className={styles.footBar}>
                        <span className={styles.summary}>
                            {t("notifications.summary", { unread: unreadCount, total: notifications.length })}
                        </span>
                        <button
                            className={styles.viewAll}
                            onClick={() => { navigate("/notifications"); setIsOpen(false); }}
                        >
                            {t("notifications.tabs.notifications")} <span className={styles.viewAllArrow}>→</span>
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
