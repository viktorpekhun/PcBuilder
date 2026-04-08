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
    roles?: string[];
    emailVerified?: boolean;
    avatarUrl?: string;
    accessToken?: string;
}

export interface IGoogleLoginRequest {
    idToken: string;
}

export interface IForgotPasswordRequest {
    email: string;
}

export interface IResetPasswordRequest {
    token: string;
    newPassword: string;
}

export interface IAuthContextType {
    auth: AuthUser;
    setAuth: Dispatch<SetStateAction<AuthUser>>;
    persist: boolean;
    setPersist: Dispatch<SetStateAction<boolean>>;
}