import axios, { axiosPrivate } from "./axios";
import type {
    ILoginRequest,
    IRegisterRequest,
    IGoogleLoginRequest,
    IForgotPasswordRequest,
    IResetPasswordRequest,
    ITokenResponse,
    IMessageResponse,
} from "../types/auth.types";

const PATH = "/Auth";

export const authService = {
    register: (data: IRegisterRequest) =>
        axiosPrivate.post<ITokenResponse>(`${PATH}/register`, data),

    login: (data: ILoginRequest) =>
        axiosPrivate.post<ITokenResponse>(`${PATH}/login`, data),

    googleLogin: (data: IGoogleLoginRequest) =>
        axiosPrivate.post<ITokenResponse>(`${PATH}/google-login`, data),

    logout: () =>
        axiosPrivate.post<IMessageResponse>(`${PATH}/logout`),

    refresh: () =>
        axiosPrivate.get<ITokenResponse>(`${PATH}/refresh`),

    verifyEmail: (token: string) =>
        axios.get<IMessageResponse>(`${PATH}/verify-email`, { params: { token } }),

    resendVerification: () =>
        axiosPrivate.post<IMessageResponse>(`${PATH}/resend-verification`),

    forgotPassword: (data: IForgotPasswordRequest) =>
        axios.post<IMessageResponse>(`${PATH}/forgot-password`, data),

    resetPassword: (data: IResetPasswordRequest) =>
        axios.post<IMessageResponse>(`${PATH}/reset-password`, data),
};
