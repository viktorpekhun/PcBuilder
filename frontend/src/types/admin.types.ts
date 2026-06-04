export interface IAdminStats {
    totalUsers: number;
    newUsersLast7Days: number;
    totalBuilds: number;
    publishedBuilds: number;
    totalReviews: number;
    pendingReports: number;
    componentCounts: Record<string, number>;
    activeBannedUsers: number;
}

export const BanType = {
    Comment: "Comment",
    Post: "Post",
} as const;

export type BanTypeValue = typeof BanType[keyof typeof BanType];

export interface IAdminUser {
    id: string;
    username: string;
    email: string;
    avatarUrl?: string | null;
    roles: string[];
    createdAt: string;
    commentBanUntil: string | null;
    postBanUntil: string | null;
    isCommentBanned: boolean;
    isPostBanned: boolean;
    warningsCount: number;
}

export interface IAdminWarning {
    id: string;
    banType: BanTypeValue;
    reason: string;
    reasonCode?: string;
    issuedByAdminUsername?: string | null;
    issuedAt: string;
}

export interface IAdminUserDetail {
    id: string;
    username: string;
    email: string;
    avatarUrl?: string | null;
    bio?: string | null;
    roles: string[];
    createdAt: string;
    isEmailVerified: boolean;
    commentBanUntil: string | null;
    postBanUntil: string | null;
    isCommentBanned: boolean;
    isPostBanned: boolean;
    warnings: IAdminWarning[];
    buildCount: number;
    reviewCount: number;
}

export interface IAdminUsersQuery {
    searchQuery?: string;
    pageNumber?: number;
    pageSize?: number;
}

export interface IPaginationHeader {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
}

export interface IBanUserRequest {
    banType: BanTypeValue;
    durationDays: number;
    reason: string;
}

export interface IUnbanUserRequest {
    banType: BanTypeValue;
}

export interface IChangeRoleRequest {
    role: "Admin" | "User";
}

export const ReportType = {
    Review: "Review",
    Build: "Build",
} as const;
export type ReportTypeValue = typeof ReportType[keyof typeof ReportType];

export const ReportStatus = {
    Pending: "Pending",
    Resolved: "Resolved",
    Dismissed: "Dismissed",
} as const;
export type ReportStatusValue = typeof ReportStatus[keyof typeof ReportStatus];

export const ReportResolutionAction = {
    Dismiss: "Dismiss",
    DeleteContent: "DeleteContent",
    DeleteContentAndWarn: "DeleteContentAndWarn",
    DeleteContentAndBan: "DeleteContentAndBan",
} as const;
export type ReportResolutionActionValue = typeof ReportResolutionAction[keyof typeof ReportResolutionAction];

export interface IReport {
    id: string;
    reporterId: string;
    reporterUsername: string;
    reportType: ReportTypeValue;
    reportedEntityId: string;
    reportedUserId: string;
    reportedUsername: string;
    reason: string;
    status: ReportStatusValue;
    adminResolutionNote?: string | null;
    resolvedAt?: string | null;
    createdAt: string;
}

export interface IReportsQuery {
    status?: ReportStatusValue;
    pageNumber?: number;
    pageSize?: number;
}

export interface IResolveReportRequest {
    action: ReportResolutionActionValue;
    reason?: string;
    reasonCode?: string;
    banType?: BanTypeValue;
    banDurationDays?: number;
}

export interface IAdminActivityLog {
    id: string;
    adminUsername: string;
    action: string;
    targetType: string | null;
    targetId: string | null;
    targetName: string | null;
    detail: string | null;
    occurredAt: string;
}

export type ScrapeJobState =
    | "Queued"
    | "Running"
    | "Cancelling"
    | "Cancelled"
    | "Completed"
    | "Failed";

export type ScrapeJobKind = "Category" | "PriceUpdate" | "SingleComponent";

export interface IScrapeJobStatus {
    jobId: string;
    componentType: string;
    kind: ScrapeJobKind;
    state: ScrapeJobState;
    queuedAt: string;
    startedAt: string | null;
    completedAt: string | null;
    errorMessage: string | null;
    itemsScraped: number;
    totalItems: number | null;
}
