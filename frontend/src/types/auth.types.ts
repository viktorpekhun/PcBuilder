import type { Dispatch, SetStateAction } from "react";

// --- Request types ---

export interface ILoginRequest {
    email: string;
    password: string;
}

export interface IRegisterRequest {
    username: string;
    email: string;
    password: string;
}

// --- Response types ---

export interface ITokenResponse {
    accessToken: string;
}

export interface IMessageResponse {
    message: string;
}

// --- Context types ---

export interface AuthUser {
    userId?: string;
    username?: string;
    email?: string;
    accessToken?: string;
}

export interface IAuthContextType {
    auth: AuthUser;
    setAuth: Dispatch<SetStateAction<AuthUser>>;
    persist: boolean;
    setPersist: Dispatch<SetStateAction<boolean>>;
}