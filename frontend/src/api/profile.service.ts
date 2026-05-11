import { axiosPrivate } from './axios';
import type { IProfileResponse, IUpdateProfileRequest, IChangePasswordRequest, ISetPasswordRequest, IUserBanStatus } from '../types/profile.types';

const PATH = '/Profile';

export const profileService = {
    getProfile: () =>
        axiosPrivate.get<IProfileResponse>(PATH),

    updateProfile: (data: IUpdateProfileRequest) =>
        axiosPrivate.put<IProfileResponse>(PATH, data),

    uploadAvatar: (formData: FormData) =>
        axiosPrivate.post<{ avatarUrl: string }>(`${PATH}/avatar`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        }),

    deleteAvatar: () =>
        axiosPrivate.delete(`${PATH}/avatar`),

    changePassword: (data: IChangePasswordRequest) =>
        axiosPrivate.post(`${PATH}/change-password`, data),

    setPassword: (data: ISetPasswordRequest) =>
        axiosPrivate.post(`${PATH}/set-password`, data),

    deleteAccount: () =>
        axiosPrivate.delete(PATH),

    getBans: () =>
        axiosPrivate.get<IUserBanStatus>(`${PATH}/bans`),
};
