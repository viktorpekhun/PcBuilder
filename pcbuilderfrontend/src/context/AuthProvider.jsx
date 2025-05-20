import { createContext, useState, useEffect } from "react";
import { decodeToken } from "../utils/decodeToken.js";

const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {
    const [auth, setAuth] = useState({});
    const [persist, setPersist] = useState(JSON.parse(localStorage.getItem("persist")) || false);

    useEffect(() => {
        if (auth?.accessToken) {
            const userData = decodeToken(auth.accessToken);
            setAuth(prev => ({ ...prev, ...userData }));
        }
    }, [auth?.accessToken]);

    return (
        <AuthContext.Provider value={{ auth, setAuth, persist, setPersist }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthContext;