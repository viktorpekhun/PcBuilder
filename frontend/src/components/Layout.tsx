import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar/Sidebar";
import Topbar from "./Topbar/Topbar";
import EmailVerificationBanner from "./EmailVerificationBanner/EmailVerificationBanner";
import useAuth from "../hooks/useAuth";
import styles from "./Layout.module.css";

const Layout = () => {
    const { auth } = useAuth();

    return (
        <div className={styles.app}>
            <Topbar />
            <Sidebar />
            <main className={styles.main}>
                {auth?.accessToken && auth?.emailVerified === false && (
                    <EmailVerificationBanner />
                )}
                <Outlet />
            </main>
        </div>
    );
};

export default Layout;
