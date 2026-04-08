import { Outlet } from "react-router-dom";
import Navbar from "./Navbar/Navbar";
import Footer from "./Footer/Footer";
import EmailVerificationBanner from "./EmailVerificationBanner/EmailVerificationBanner";
import useAuth from "../hooks/useAuth";

const Layout = () => {
    const { auth } = useAuth();

    return (
        <div style={{ display: 'flex', flexDirection: 'column', width: '100%', minHeight: '100vh' }}>
            <Navbar/>
            {auth?.accessToken && auth?.emailVerified === false && (
                <EmailVerificationBanner />
            )}
            <div className="content-wrapper">
                <Outlet />
            </div>
            <Footer/>
        </div>
    )
}

export default Layout;
