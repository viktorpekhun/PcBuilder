import axios, { axiosPrivate } from "./axios";
import type {
    ILoginRequest,
    IRegisterRequest,
    ITokenResponse,
    IMessageResponse,
} from "../types/auth.types";

const PATH = "/Auth";

export const authService = {
    register: (data: IRegisterRequest) =>
        axiosPrivate.post<ITokenResponse>(`${PATH}/register`, data),

    login: (data: ILoginRequest) =>
        axiosPrivate.post<ITokenResponse>(`${PATH}/login`, data),

    logout: () =>
        axiosPrivate.post<IMessageResponse>(`${PATH}/logout`),

    refresh: () =>
        axiosPrivate.get<ITokenResponse>(`${PATH}/refresh`),
};
