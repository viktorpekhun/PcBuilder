import { jwtDecode } from 'jwt-decode';
import type { AuthUser } from '../types/auth.types';

interface CustomJwtPayload {
    sub: string;
    name: string;
    email: string;
    exp: number;
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string | string[];
    email_verified?: string;
}

export const decodeToken = (token: string): AuthUser | null => {
    if (!token) return null;

    try {
        const decoded = jwtDecode<CustomJwtPayload>(token);
        const roles = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

        return {
            userId: decoded.sub,
            username: decoded.name,
            email: decoded.email,
            roles: roles ? (Array.isArray(roles) ? roles : [roles]) : [],
            emailVerified: decoded.email_verified === "true"
        };
    } catch (error) {
        console.error("Failed to decode token:", error);
        return null;
    }
};
