import { createContext, useState, useEffect, useCallback, useRef } from "react";
import type { ReactNode } from "react";
import type { INotification } from "../types/notification.types";
import { notificationService } from "../api/notification.service";
import useAuth from "../hooks/useAuth";

export interface INotificationContextType {
    notifications: INotification[];
    unreadCount: number;
    isOpen: boolean;
    setIsOpen: (open: boolean) => void;
    fetchNotifications: () => Promise<void>;
    markRead: (id: string) => Promise<void>;
    markAllRead: () => Promise<void>;
    refreshUnreadCount: () => Promise<void>;
}

const NotificationContext = createContext<INotificationContextType>(
    {} as INotificationContextType
);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
    const { auth } = useAuth();
    const [notifications, setNotifications] = useState<INotification[]>([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [isOpen, setIsOpen] = useState(false);
    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

    const isAuthenticated = !!auth?.accessToken;

    const refreshUnreadCount = useCallback(async () => {
        if (!isAuthenticated) return;
        try {
            const res = await notificationService.getUnreadCount();
            setUnreadCount(res.data.count);
        } catch {
            // silently ignore polling errors
        }
    }, [isAuthenticated]);

    const fetchNotifications = useCallback(async () => {
        if (!isAuthenticated) return;
        try {
            const res = await notificationService.getNotifications(1, 10);
            setNotifications(res.data);
            await refreshUnreadCount();
        } catch {
            // silently ignore
        }
    }, [isAuthenticated, refreshUnreadCount]);

    const markRead = useCallback(async (id: string) => {
        try {
            await notificationService.markRead(id);
            setNotifications((prev) =>
                prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
            );
            setUnreadCount((prev) => Math.max(0, prev - 1));
        } catch {
            // silently ignore
        }
    }, []);

    const markAllRead = useCallback(async () => {
        try {
            await notificationService.markAllRead();
            setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
            setUnreadCount(0);
        } catch {
            // silently ignore
        }
    }, []);

    // Poll unread count every 60s when authenticated
    useEffect(() => {
        if (isAuthenticated) {
            refreshUnreadCount();
            intervalRef.current = setInterval(refreshUnreadCount, 60_000);
        } else {
            setNotifications([]);
            setUnreadCount(0);
        }

        return () => {
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        };
    }, [isAuthenticated, refreshUnreadCount]);

    return (
        <NotificationContext.Provider
            value={{
                notifications,
                unreadCount,
                isOpen,
                setIsOpen,
                fetchNotifications,
                markRead,
                markAllRead,
                refreshUnreadCount,
            }}
        >
            {children}
        </NotificationContext.Provider>
    );
};

export default NotificationContext;
