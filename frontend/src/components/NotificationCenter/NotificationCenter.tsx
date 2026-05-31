import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import useNotifications from "../../hooks/useNotifications";
import type { INotification } from "../../types/notification.types";
import styles from "./NotificationCenter.module.css";

/* ── helpers ── */
function relTime(iso: string): string {
    const mins = Math.round((Date.now() - new Date(iso).getTime()) / 60_000);
    if (mins < 1) return "JUST NOW";
    if (mins < 60) return `${mins}m AGO`;
    const hrs = Math.round(mins / 60);
    if (hrs < 24) return `${hrs}h AGO`;
    return `${Math.round(hrs / 24)}d AGO`;
}

function dayKey(iso: string): string {
    const d = new Date(iso);
    return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

function dayLabel(iso: string): string {
    const d = new Date(iso);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today); yesterday.setDate(today.getDate() - 1);
    const dd = new Date(d); dd.setHours(0, 0, 0, 0);
    if (dd.getTime() === today.getTime()) return "TODAY";
    if (dd.getTime() === yesterday.getTime()) return "YESTERDAY";
    const m = ["JAN","FEB","MAR","APR","MAY","JUN","JUL","AUG","SEP","OCT","NOV","DEC"];
    return `${m[d.getMonth()]} ${d.getDate()}`;
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
type TagKind = "review" | "deleted" | "price" | "warn";

function getTagKind(type: string): TagKind {
    if (type === "NewReview") return "review";
    if (type === "ReviewDeleted" || type === "BuildDeleted") return "deleted";
    if (type === "PriceAlert") return "price";
    return "warn";
}

function getTagLabel(type: string): string {
    if (type === "NewReview") return "REVIEW";
    if (type === "ReviewDeleted" || type === "BuildDeleted") return "DELETED";
    if (type === "PriceAlert") return "PRICE ▼";
    if (type === "CommentBanned" || type === "PostBanned") return "BAN";
    if (type === "CommentUnbanned" || type === "PostUnbanned") return "UNBANNED";
    return "NOTICE";
}

const tagKindClass: Record<TagKind, string | undefined> = {
    review:  styles.typeTagReview,
    deleted: styles.typeTagDeleted,
    price:   styles.typeTagPrice,
    warn:    styles.typeTagWarn,
};

/* ── notification path ── */
function notifPath(n: INotification): string {
    const p = n.payload;
    if (n.type === "PriceAlert") {
        const t = (p.componentType || "").toLowerCase();
        return `/components/${t}/${p.componentId || ""}`;
    }
    return `/builds/${p.buildId || ""}`;
}

/* ── notification text (rich) ── */
function NotifText({ n }: { n: INotification }) {
    const p = n.payload;
    if (n.type === "NewReview") {
        const rating = Number(p.rating) || 0;
        return (
            <>
                <strong>{p.reviewerUsername ?? "Someone"}</strong> left a{" "}
                <strong>{rating}-star</strong> review on{" "}
                <span className={styles.textLink}>/{p.buildName ?? "a build"}</span>
            </>
        );
    }
    if (n.type === "ReviewDeleted" || n.type === "BuildDeleted") {
        const label = n.type === "BuildDeleted" ? "build" : "review";
        return (
            <>
                Your {label} <span className={styles.textLink}>/{p.buildName ?? ""}</span> was removed by moderation
                {p.reason && (
                    <span className={styles.reason}> · {p.reason.replace(/_/g, " ").toLowerCase()}</span>
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
                <strong>{p.componentName ?? "Component"}</strong> price{" "}
                {p.direction === "down" ? "dropped" : "rose"}
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
        const until = p.banUntil ? new Date(p.banUntil).toLocaleDateString() : "an unknown date";
        const scope = n.type === "CommentBanned" ? "commenting" : "posting";
        return <>You have been restricted from {scope} until <strong>{until}</strong>{p.reason && <span className={styles.reason}> · {p.reason.replace(/_/g, " ").toLowerCase()}</span>}</>;
    }
    if (n.type === "CommentUnbanned") return <>Your comment restriction has been lifted.</>;
    if (n.type === "PostUnbanned") return <>Your posting restriction has been lifted.</>;
    if (n.type === "CommentWarning") return <>Your comment was removed for violating community rules.</>;
    if (n.type === "PostWarning") return <>Your build was removed from active posts for violating community rules.</>;
    return <>You have a new notification.</>;
}

/* ── item ── */
function Item({ n, onMarkRead, onNavigate }: {
    n: INotification;
    onMarkRead: (id: string) => void;
    onNavigate: (n: INotification) => void;
}) {
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
                    <span className={`${styles.typeTag} ${tagKindClass[kind]}`}>{getTagLabel(n.type)}</span>
                    {n.type === "NewReview" && rating > 0 && (
                        <span className={styles.stars}>
                            {"★".repeat(rating)}
                            <span className={styles.starsDim}>{"★".repeat(5 - rating)}</span>
                        </span>
                    )}
                    <span className={styles.time}>{relTime(n.createdAt)}</span>
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
                                ✓ READ
                            </button>
                        )}
                        <span className={styles.openHint}>OPEN ↗</span>
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
    const groups = useMemo(() => {
        const map = new Map<string, { label: string; items: INotification[] }>();
        items.forEach(n => {
            const k = dayKey(n.createdAt);
            if (!map.has(k)) map.set(k, { label: dayLabel(n.createdAt), items: [] });
            map.get(k)!.items.push(n);
        });
        return Array.from(map.entries());
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
    return (
        <div className={styles.empty}>
            <div className={styles.emptyCard}>
                <span className={styles.emptyEyebrow}>
                    {filter === "unread" ? "INBOX ZERO" : "NO NOTIFICATIONS"}
                </span>
                <h3 className={styles.emptyTitle}>
                    {filter === "unread" ? "You're all caught up." : "Nothing yet."}
                </h3>
                <p className={styles.emptyDesc}>
                    {filter === "unread"
                        ? "New reviews, moderation notices and price-drop alerts will land here."
                        : "Publish a build and subscribe to a part to start a feed."}
                </p>
                <span className={styles.emptyDots}>{"●  ●  ●"}</span>
            </div>
        </div>
    );
}

/* ── root ── */
export default function NotificationCenter() {
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

    // close on outside click or Escape
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
                aria-label="Notifications"
                aria-expanded={isOpen}
            >
                <BellIcon />
                {unreadCount > 0 && (
                    <span className={styles.badge}>{unreadCount > 99 ? "99+" : unreadCount}</span>
                )}
            </button>

            {isOpen && (
                <div className={styles.pop} role="dialog" aria-label="Notification center">
                    {/* header */}
                    <div className={styles.head}>
                        <div className={styles.titleWrap}>
                            <h2 className={styles.title}>Notifications</h2>
                        </div>
                        <span className={styles.spacer} />
                        <button className={styles.markAllBtn} onClick={markAllRead} disabled={unreadCount === 0}>
                            ✓ Mark all read
                        </button>
                    </div>

                    {/* filters */}
                    <div className={styles.filters}>
                        <button
                            className={`${styles.chip} ${filter === "all" ? styles.chipActive : ""}`}
                            onClick={() => setFilter("all")}
                        >
                            <span className={styles.chipDot} style={{ background: "var(--bd-2)" }} />
                            ALL <span className={styles.chipNum}>{notifications.length}</span>
                        </button>
                        <button
                            className={`${styles.chip} ${filter === "unread" ? styles.chipActive : ""}`}
                            onClick={() => setFilter("unread")}
                        >
                            <span className={styles.chipDot}
                                  style={{ background: unreadCount > 0 ? "var(--acc)" : "var(--bd-2)" }} />
                            UNREAD <span className={styles.chipNum}>{unreadCount}</span>
                        </button>
                        <span className={styles.live}>
                            <span className={styles.liveDot} />LIVE
                        </span>
                    </div>

                    {/* list or empty */}
                    {visible.length === 0
                        ? <EmptyState filter={filter} />
                        : <List items={visible} onMarkRead={markRead} onNavigate={handleNavigate} />
                    }

                    {/* footer */}
                    <div className={styles.footBar}>
                        <span className={styles.summary}>
                            <span className={styles.summaryNum}>{unreadCount}</span> unread ·{" "}
                            <span className={styles.summaryNum}>{notifications.length}</span> total
                        </span>
                        <button
                            className={styles.viewAll}
                            onClick={() => { navigate("/notifications"); setIsOpen(false); }}
                        >
                            View all <span className={styles.viewAllArrow}>→</span>
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
