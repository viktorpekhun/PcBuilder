import axios from "axios";
import i18n from "../i18n";

const BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? '/api';

const langInterceptor = (config: import("axios").InternalAxiosRequestConfig) => {
    config.headers["Accept-Language"] = i18n.language || "en";
    return config;
};

const axiosDefault = axios.create({
    baseURL: BASE_URL,
    headers: { 'Content-Type': 'application/json' },
});
axiosDefault.interceptors.request.use(langInterceptor);

export default axiosDefault;

export const axiosPrivate = axios.create({
    baseURL: BASE_URL,
    headers: { 'Content-Type': 'application/json' },
    withCredentials: true,
});
axiosPrivate.interceptors.request.use(langInterceptor);
