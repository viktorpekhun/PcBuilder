import { jwtDecode } from 'jwt-decode';
import type { AuthUser } from '../types/auth.types';

interface CustomJwtPayload {
    sub: string;
    name: string;
    email: string;
    exp: number;
}

export const decodeToken = (token: string): AuthUser | null => {
    if (!token) return null;

    try {
        const decoded = jwtDecode<CustomJwtPayload>(token);
        return {
            userId: decoded.sub,
            username: decoded.name,
            email: decoded.email
        };
    } catch (error) {
        console.error("Failed to decode token:", error);
        return null;
    }
};