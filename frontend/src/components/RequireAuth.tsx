import { useLocation, Navigate, Outlet } from "react-router-dom";
import useAuth from "../hooks/useAuth";

interface RequireAuthProps {
    allowedRoles?: string[];
}

const RequireAuth = ({ allowedRoles }: RequireAuthProps) => {
    const { auth } = useAuth();
    const location = useLocation();

    if (!auth?.accessToken) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (allowedRoles && !auth.roles?.some(role => allowedRoles.includes(role))) {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
}

export default RequireAuth;
