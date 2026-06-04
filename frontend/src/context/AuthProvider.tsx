import { createContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import type { AuthUser, IAuthContextType } from "../types/auth.types";
import { authService } from "../api/auth.service";
import { axiosPrivate } from "../api/axios";
import { decodeToken } from "../utils/decodeToken";
import type { IProfileResponse } from "../types/profile.types";
import i18n from "../i18n";

const AuthContext = createContext<IAuthContextType>({} as IAuthContextType);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [auth, setAuth] = useState<AuthUser>({});
    const [persist, setPersist] = useState<boolean>(
        JSON.parse(localStorage.getItem("persist") || "false")
    );
    const [isAuthInitializing, setIsAuthInitializing] = useState<boolean>(true);

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const { data } = await authService.refresh();
                if (cancelled) return;
                const userData = decodeToken(data.accessToken);
                const profileRes = await axiosPrivate.get<IProfileResponse>("/Profile", {
                    headers: { Authorization: `Bearer ${data.accessToken}` },
                });
                if (cancelled) return;
                setAuth({ ...userData, accessToken: data.accessToken, avatarUrl: profileRes.data.avatarUrl });
                if (profileRes.data.preferredLanguage) {
                    i18n.changeLanguage(profileRes.data.preferredLanguage);
                }
            } catch {
                // No valid refresh cookie — stay unauthenticated.
            } finally {
                if (!cancelled) setIsAuthInitializing(false);
            }
        })();
        return () => { cancelled = true; };
    }, []);

    return (
        <AuthContext.Provider value={{ auth, setAuth, persist, setPersist, isAuthInitializing }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthContext;
