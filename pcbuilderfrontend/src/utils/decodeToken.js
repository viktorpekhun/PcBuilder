import { jwtDecode } from 'jwt-decode';

export const decodeToken = (token) => {
    if (!token) return null;
    try {
        const decoded = jwtDecode(token);
        return {
            userId: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
            username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        };
    } catch (error) {
        console.error("Failed to decode token:", error);
        return null;
    }
};