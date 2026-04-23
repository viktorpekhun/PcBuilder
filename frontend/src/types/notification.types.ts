export type NotificationType =
  | "ReviewDeleted"
  | "BuildDeleted"
  | "CommentBanned"
  | "PostBanned"
  | "CommentUnbanned"
  | "PostUnbanned"
  | "CommentWarning"
  | "PostWarning"
  | "NewReview"
  | "PriceAlert";

export interface INotificationPayload {
  [key: string]: string;
}

export interface INotification {
  id: string;
  type: NotificationType;
  payload: INotificationPayload;
  isRead: boolean;
  createdAt: string;
}

export interface INotificationPagedResponse {
  items: INotification[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface IUnreadCountResponse {
  count: number;
}
