import { useContext } from "react";
import AuthContext from "../context/AuthProvider";
import type { IAuthContextType } from "../types/auth.types";

const useAuth = (): IAuthContextType => {
    return useContext(AuthContext);
}

export default useAuth;
