import { axiosPrivate } from "../api/axios";
import { useEffect, useRef } from "react";
import useRefreshToken from "./useRefreshToken";
import useAuth from "./useAuth";
import type { InternalAxiosRequestConfig } from "axios";
import type { AuthUser } from "../types/auth.types";

const useAxiosPrivate = () => {
    const refresh = useRefreshToken();
    const { auth, setAuth } = useAuth();

    const authRef = useRef<AuthUser>(auth);
    authRef.current = auth;

    const refreshRef = useRef(refresh);
    refreshRef.current = refresh;

    const setAuthRef = useRef(setAuth);
    setAuthRef.current = setAuth;

    useEffect(() => {
        const requestIntercept = axiosPrivate.interceptors.request.use(
            (config: InternalAxiosRequestConfig) => {
                const token = authRef.current?.accessToken;
                if (!config.headers['Authorization'] && token) {
                    config.headers['Authorization'] = `Bearer ${token}`;
                }
                return config;
            }, (error) => Promise.reject(error)
        );

        const responseIntercept = axiosPrivate.interceptors.response.use(
            response => response,
            async (error) => {
                const prevRequest = error?.config;
                const isRefreshRequest = prevRequest?.url?.includes('/refresh');
                if ((error?.response?.status === 401 || error?.response?.status === 403) && !prevRequest?.sent && !isRefreshRequest) {
                    prevRequest.sent = true;
                    try {
                        const newAccessToken = await refreshRef.current();
                        prevRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;
                        return axiosPrivate(prevRequest);
                    } catch (refreshError) {
                        setAuthRef.current({});
                        return Promise.reject(refreshError);
                    }
                }
                return Promise.reject(error);
            }
        );

        return () => {
            axiosPrivate.interceptors.request.eject(requestIntercept);
            axiosPrivate.interceptors.response.eject(responseIntercept);
        }
    }, []);

    return axiosPrivate;
}

export default useAxiosPrivate;
