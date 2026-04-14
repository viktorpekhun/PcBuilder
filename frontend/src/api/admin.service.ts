import { axiosPrivate } from './axios';
import type {
    IAdminStats,
    IAdminUser,
    IAdminUserDetail,
    IAdminUsersQuery,
    IBanUserRequest,
    IChangeRoleRequest,
    IPaginationHeader,
    IReport,
    IReportsQuery,
    IResolveReportRequest,
    IWarnUserRequest,
} from '../types/admin.types';

const PATH = '/admin';

const parsePagination = (header: unknown): IPaginationHeader | null => {
    if (typeof header !== 'string') return null;
    try {
        return JSON.parse(header) as IPaginationHeader;
    } catch {
        return null;
    }
};

export const adminService = {
    getStats: () =>
        axiosPrivate.get<IAdminStats>(`${PATH}/stats`),

    getUsers: async (query: IAdminUsersQuery = {}) => {
        const res = await axiosPrivate.get<IAdminUser[]>(`${PATH}/users`, {
            params: {
                searchQuery: query.searchQuery || undefined,
                pageNumber: query.pageNumber ?? 1,
                pageSize: query.pageSize ?? 20,
            },
        });
        return {
            items: res.data,
            pagination: parsePagination(res.headers['x-pagination']),
        };
    },

    getUserDetail: (userId: string) =>
        axiosPrivate.get<IAdminUserDetail>(`${PATH}/users/${userId}`),

    warnUser: (userId: string, data: IWarnUserRequest) =>
        axiosPrivate.post(`${PATH}/users/${userId}/warn`, data),

    banUser: (userId: string, data: IBanUserRequest) =>
        axiosPrivate.post(`${PATH}/users/${userId}/ban`, data),

    changeUserRole: (userId: string, data: IChangeRoleRequest) =>
        axiosPrivate.patch<{ success: boolean; promoted: boolean }>(`${PATH}/users/${userId}/role`, data),

    deleteUser: (userId: string) =>
        axiosPrivate.delete(`${PATH}/users/${userId}`),

    deleteBuild: (buildId: string) =>
        axiosPrivate.delete(`${PATH}/content/builds/${buildId}`),

    deleteReview: (reviewId: string) =>
        axiosPrivate.delete(`${PATH}/content/reviews/${reviewId}`),

    getReports: async (query: IReportsQuery = {}) => {
        const res = await axiosPrivate.get<IReport[]>(`${PATH}/reports`, {
            params: {
                status: query.status,
                pageNumber: query.pageNumber ?? 1,
                pageSize: query.pageSize ?? 20,
            },
        });
        return {
            items: res.data,
            pagination: parsePagination(res.headers['x-pagination']),
        };
    },

    resolveReport: (reportId: string, data: IResolveReportRequest) =>
        axiosPrivate.post(`${PATH}/reports/${reportId}/resolve`, data),
};
