import axios from "../api/axios.jsx";
import useAuth from "./useAuth";

const useLogout = () => {
    const { setAuth } = useAuth();


    const logout = async () => {
        try {

            await axios.post('/api/auth/logout', {}, { withCredentials: true });
            setAuth({});

        } catch (err) {
            console.error(err);
        }

    }

    return logout;
}

export default useLogout