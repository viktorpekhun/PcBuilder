import { Outlet } from "react-router-dom";
import Navbar from "./Navbar/Navbar";
import Footer from "./Footer/Footer";

const Layout = () => {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', width: '100%', minHeight: '100vh' }}>
            <Navbar/>
            <div className="content-wrapper">
                <Outlet />
            </div>
            <Footer/>
        </div>
    )
}

export default Layout;
