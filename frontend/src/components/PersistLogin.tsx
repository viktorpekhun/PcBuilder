import { Outlet } from "react-router-dom";
import useAuth from "../hooks/useAuth";
import useAxiosPrivate from "../hooks/useAxiosPrivate";

const PersistLogin = () => {
    const { isAuthInitializing } = useAuth();
    useAxiosPrivate();

    if (isAuthInitializing) return <p>Loading...</p>;
    return <Outlet />;
}

export default PersistLogin;
