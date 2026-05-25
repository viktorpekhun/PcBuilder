import { createContext, useState, useEffect, useCallback, useRef } from "react";
import type { ReactNode } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";
import type { INotification } from "../types/notification.types";
import { notificationService } from "../api/notification.service";
import useAuth from "../hooks/useAuth";

const HUB_URL = import.meta.env.VITE_HUB_URL ?? "/hubs/notifications";

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
    const connectionRef = useRef<HubConnection | null>(null);

    const isAuthenticated = !!auth?.accessToken;

    const refreshUnreadCount = useCallback(async () => {
        if (!auth?.accessToken) return;
        try {
            const res = await notificationService.getUnreadCount();
            setUnreadCount(res.data.count);
        } catch {
            // silently ignore
        }
    }, [auth?.accessToken]);

    const fetchNotifications = useCallback(async () => {
        if (!auth?.accessToken) return;
        try {
            const res = await notificationService.getNotifications(1, 10);
            setNotifications(res.data);
            await refreshUnreadCount();
        } catch {
            // silently ignore
        }
    }, [auth?.accessToken, refreshUnreadCount]);

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

    useEffect(() => {
        if (!isAuthenticated) {
            setNotifications([]);
            setUnreadCount(0);
            connectionRef.current?.stop();
            connectionRef.current = null;
            return;
        }

        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, { accessTokenFactory: () => auth.accessToken! })
            .withAutomaticReconnect()
            .build();

        connection.on("notification", (n: INotification) => {
            setNotifications((prev) => [n, ...prev].slice(0, 10));
            setUnreadCount((c) => c + 1);
        });

        connection.onreconnected(() => {
            refreshUnreadCount();
            fetchNotifications();
        });

        connectionRef.current = connection;

        connection.start()
            .then(() => {
                refreshUnreadCount();
            })
            .catch(() => {
                // fall back to initial fetch if hub fails
                refreshUnreadCount();
            });

        return () => {
            connection.stop();
            connectionRef.current = null;
        };
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isAuthenticated]);

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
