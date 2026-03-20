import { authService } from "../api/auth.service";
import { decodeToken } from "../utils/decodeToken";
import useAuth from "./useAuth";

const useRefreshToken = () => {
    const { setAuth } = useAuth();

    const refresh = async (): Promise<string> => {
        const response = await authService.refresh();
        const newAccessToken = response.data.accessToken;
        const userData = decodeToken(newAccessToken);

        setAuth(prev => ({
            ...prev,
            ...userData,
            accessToken: newAccessToken
        }));
        return newAccessToken;
    }
    return refresh;
};

export default useRefreshToken;
