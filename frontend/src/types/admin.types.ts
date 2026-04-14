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
    Comment: 0,
    Post: 1,
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

export interface IWarnUserRequest {
    banType: BanTypeValue;
    reason: string;
}

export interface IBanUserRequest {
    banType: BanTypeValue;
    durationDays: number;
    reason: string;
}

export interface IChangeRoleRequest {
    role: "Admin" | "User";
}

export const ReportType = {
    Review: 0,
    Build: 1,
} as const;
export type ReportTypeValue = typeof ReportType[keyof typeof ReportType];

export const ReportStatus = {
    Pending: 0,
    Resolved: 1,
    Dismissed: 2,
} as const;
export type ReportStatusValue = typeof ReportStatus[keyof typeof ReportStatus];

export const ReportResolutionAction = {
    Dismiss: 0,
    DeleteContent: 1,
    DeleteContentAndWarn: 2,
    DeleteContentAndBan: 3,
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
    banType?: BanTypeValue;
    banDurationDays?: number;
}

export type ScrapeJobState =
    | "Queued"
    | "Running"
    | "Cancelling"
    | "Cancelled"
    | "Completed"
    | "Failed";

export interface IScrapeJobStatus {
    jobId: string;
    componentType: string;
    state: ScrapeJobState;
    queuedAt: string;
    startedAt: string | null;
    completedAt: string | null;
    errorMessage: string | null;
    itemsScraped: number;
    totalItems: number | null;
}
