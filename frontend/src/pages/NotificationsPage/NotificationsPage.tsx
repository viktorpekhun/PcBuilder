import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { notificationService } from "../../api/notification.service";
import { renderNotificationText } from "../../utils/notificationMessages";
import type { INotification } from "../../types/notification.types";
import type { IPaginationHeader } from "../../types/admin.types";
import styles from "./NotificationsPage.module.css";

const PAGE_SIZE = 20;

type Filter = "all" | "unread";

const formatDate = (iso: string) => new Date(iso).toLocaleString();

const NotificationsPage = () => {
    const navigate = useNavigate();
    const [filter, setFilter] = useState<Filter>("all");
    const [items, setItems] = useState<INotification[]>([]);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [pageNumber, setPageNumber] = useState(1);
    const [loading, setLoading] = useState(true);

    const fetchPage = useCallback(async (page: number, f: Filter) => {
        setLoading(true);
        try {
            const onlyUnread = f === "unread" ? true : undefined;
            const { items, pagination } = await notificationService.getNotificationsPaged(
                page,
                PAGE_SIZE,
                onlyUnread
            );
            setItems(items);
            setPagination(pagination);
        } catch {
            setItems([]);
            setPagination(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchPage(pageNumber, filter);
    }, [fetchPage, pageNumber, filter]);

    const handleFilterChange = (next: Filter) => {
        setFilter(next);
        setPageNumber(1);
    };

    const handleMarkRead = async (id: string) => {
        try {
            await notificationService.markRead(id);
            setItems(prev => prev.map(n => (n.id === id ? { ...n, isRead: true } : n)));
        } catch {
            /* ignore */
        }
    };

    const handleMarkAllRead = async () => {
        try {
            await notificationService.markAllRead();
            await fetchPage(pageNumber, filter);
        } catch {
            /* ignore */
        }
    };

    const handleItemClick = (n: INotification) => {
        if (!n.isRead) handleMarkRead(n.id);
        if (n.type === "NewReview" || n.type === "ReviewDeleted") {
            const buildId = n.payload?.buildId;
            if (buildId) navigate(`/builds/${buildId}`);
        }
    };

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>Сповіщення</h1>
                <button
                    type="button"
                    className={styles["mark-all-btn"]}
                    onClick={handleMarkAllRead}
                    disabled={items.every(i => i.isRead)}
                >
                    Прочитати все
                </button>
            </div>

            <div className={styles.tabs}>
                <button
                    type="button"
                    className={`${styles.tab} ${filter === "all" ? styles["tab-active"] : ""}`}
                    onClick={() => handleFilterChange("all")}
                >
                    Усі
                </button>
                <button
                    type="button"
                    className={`${styles.tab} ${filter === "unread" ? styles["tab-active"] : ""}`}
                    onClick={() => handleFilterChange("unread")}
                >
                    Непрочитані
                </button>
            </div>

            {loading ? (
                <div className={styles.empty}>Завантаження…</div>
            ) : items.length === 0 ? (
                <div className={styles.empty}>Немає сповіщень.</div>
            ) : (
                <ul className={styles.list}>
                    {items.map(n => (
                        <li
                            key={n.id}
                            className={`${styles.item} ${!n.isRead ? styles["item-unread"] : ""}`}
                            onClick={() => handleItemClick(n)}
                        >
                            <div className={styles["item-body"]}>
                                <div className={styles["item-text"]}>
                                    {renderNotificationText(n.type, n.payload)}
                                </div>
                                <div className={styles["item-meta"]}>{formatDate(n.createdAt)}</div>
                            </div>
                            {!n.isRead && (
                                <button
                                    type="button"
                                    className={styles["read-btn"]}
                                    onClick={e => {
                                        e.stopPropagation();
                                        handleMarkRead(n.id);
                                    }}
                                >
                                    Позначити прочитаним
                                </button>
                            )}
                        </li>
                    ))}
                </ul>
            )}

            {pagination && pagination.totalPages > 1 && (
                <div className={styles.pagination}>
                    <span>
                        Сторінка {pagination.pageNumber} з {pagination.totalPages} — {pagination.totalCount} всього
                    </span>
                    <div className={styles["pagination-buttons"]}>
                        <button
                            type="button"
                            className={styles["page-btn"]}
                            disabled={!pagination.hasPrevious}
                            onClick={() => setPageNumber(p => Math.max(1, p - 1))}
                        >
                            Попер.
                        </button>
                        <button
                            type="button"
                            className={styles["page-btn"]}
                            disabled={!pagination.hasNext}
                            onClick={() => setPageNumber(p => p + 1)}
                        >
                            Далі
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default NotificationsPage;
