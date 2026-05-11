import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import useNotifications from "../../hooks/useNotifications";
import { renderNotificationText } from "../../utils/notificationMessages";
import styles from "./NotificationCenter.module.css";

function timeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const minutes = Math.floor(diff / 60_000);
    if (minutes < 1) return "щойно";
    if (minutes < 60) return `${minutes} хв тому`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} год тому`;
    const days = Math.floor(hours / 24);
    return `${days} дн тому`;
}

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
    const panelRef = useRef<HTMLDivElement>(null);

    // Close on outside click
    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
                setIsOpen(false);
            }
        };
        if (isOpen) {
            document.addEventListener("mousedown", handler);
        }
        return () => document.removeEventListener("mousedown", handler);
    }, [isOpen, setIsOpen]);

    const handleBellClick = () => {
        if (!isOpen) {
            fetchNotifications();
        }
        setIsOpen(!isOpen);
    };

    const handleItemClick = (notification: typeof notifications[0]) => {
        if (!notification.isRead) {
            markRead(notification.id);
        }

        // Navigate based on notification type
        if (
            notification.type === "NewReview" ||
            notification.type === "ReviewDeleted"
        ) {
            const buildId = notification.payload?.buildId;
            if (buildId) {
                navigate(`/builds/${buildId}`);
                setIsOpen(false);
            }
        } else if (notification.type === "PriceAlert") {
            const componentId = notification.payload?.componentId;
            const componentType = notification.payload?.componentType;
            if (componentId && componentType) {
                navigate(`/components/${componentType.toLowerCase()}/${componentId}`);
                setIsOpen(false);
            }
        }
    };

    return (
        <div className={styles.wrapper} ref={panelRef}>
            <button
                className={styles.bellButton}
                onClick={handleBellClick}
                aria-label="Сповіщення"
            >
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="20"
                    height="20"
                    fill="currentColor"
                    viewBox="0 0 16 16"
                >
                    <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2m.995-14.901a1 1 0 1 0-1.99 0A5 5 0 0 0 3 6c0 1.098-.5 6-2 7h14c-1.5-1-2-5.902-2-7 0-2.42-1.72-4.44-4.005-4.901" />
                </svg>
                {unreadCount > 0 && (
                    <span className={styles.badge}>
                        {unreadCount > 99 ? "99+" : unreadCount}
                    </span>
                )}
            </button>

            {isOpen && (
                <div className={styles.panel}>
                    <div className={styles.header}>
                        <span className={styles.headerTitle}>Сповіщення</span>
                        {unreadCount > 0 && (
                            <button
                                className={styles.markAllBtn}
                                onClick={markAllRead}
                            >
                                Прочитати все
                            </button>
                        )}
                    </div>

                    <div className={styles.list}>
                        {notifications.length === 0 ? (
                            <div className={styles.empty}>
                                Немає сповіщень
                            </div>
                        ) : (
                            notifications.map((n) => (
                                <button
                                    key={n.id}
                                    className={`${styles.item} ${!n.isRead ? styles.itemUnread : ""}`}
                                    onClick={() => handleItemClick(n)}
                                >
                                    <div className={styles.itemText}>
                                        {renderNotificationText(n.type, n.payload)}
                                    </div>
                                    <div className={styles.itemTime}>
                                        {timeAgo(n.createdAt)}
                                    </div>
                                </button>
                            ))
                        )}
                    </div>

                    <div className={styles.footer}>
                        <button
                            className={styles.viewAllBtn}
                            onClick={() => {
                                navigate("/notifications");
                                setIsOpen(false);
                            }}
                        >
                            Переглянути всі
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
