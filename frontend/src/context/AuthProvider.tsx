import { createContext, useState } from "react";
import type { ReactNode } from "react";
import type { AuthUser, IAuthContextType } from "../types/auth.types";

const AuthContext = createContext<IAuthContextType>({} as IAuthContextType);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [auth, setAuth] = useState<AuthUser>({});
    const [persist, setPersist] = useState<boolean>(
        JSON.parse(localStorage.getItem("persist") || "false")
    );

    return (
        <AuthContext.Provider value={{ auth, setAuth, persist, setPersist }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthContext;
