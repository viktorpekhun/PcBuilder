import { axiosPrivate } from "./axios";
import type { INotification } from "../types/notification.types";
import type { IPaginationHeader } from "../types/admin.types";

const PATH = "/Notifications";

const parsePagination = (header: unknown): IPaginationHeader | null => {
    if (typeof header !== "string") return null;
    try {
        return JSON.parse(header) as IPaginationHeader;
    } catch {
        return null;
    }
};

export const notificationService = {
    getNotifications: (pageNumber = 1, pageSize = 10, onlyUnread?: boolean) =>
        axiosPrivate.get<INotification[]>(PATH, {
            params: { pageNumber, pageSize, onlyUnread },
        }),

    getNotificationsPaged: async (pageNumber = 1, pageSize = 20, onlyUnread?: boolean) => {
        const res = await axiosPrivate.get<INotification[]>(PATH, {
            params: { pageNumber, pageSize, onlyUnread },
        });
        return {
            items: res.data,
            pagination: parsePagination(res.headers["x-pagination"]),
        };
    },

    getUnreadCount: () =>
        axiosPrivate.get<{ count: number }>(`${PATH}/unread-count`),

    markRead: (id: string) =>
        axiosPrivate.patch<{ success: boolean }>(`${PATH}/${id}/read`),

    markAllRead: () =>
        axiosPrivate.patch<{ success: boolean; markedCount: number }>(`${PATH}/read-all`),
};
