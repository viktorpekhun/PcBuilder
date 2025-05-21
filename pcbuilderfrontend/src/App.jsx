import './App.css'
import {Routes, Route} from "react-router-dom";
import Layout from "./components/Layout.jsx";
import PcBuildPage from "./pages/PcBuildPage/PcBuildPage.jsx";
import ComponentsPage from "./pages/ComponentsPage/ComponentsPage.jsx";
import ComponentPage from "./pages/ComponentPage/ComponentPage.jsx";
import Navbar from "./components/Navbar/Navbar.jsx";
import Footer from "./components/Footer/Footer.jsx";
import PersistLogin from "./components/PersistLogin.jsx";
import RequireAuth from "./components/RequireAuth.jsx";
import LoginPage from "./pages/LoginPage/LoginPage.jsx";
import RegisterPage from "./pages/RegisterPage/RegisterPage.jsx";
import UserBuildsPage from "./pages/UserBuildsPage/UserBuildsPage.jsx";

function App() {

    return(
        <>
            <Navbar/>
            <div>
                <Routes>
                    <Route path="/" element={<Layout />}>
                        <Route index element={<PcBuildPage />} />

                        <Route path="login" element={<LoginPage />} />
                        <Route path="register" element={<RegisterPage />} />

                        <Route path="components/:type" element={<ComponentsPage />} />
                        <Route path="components/:type/:id" element={<ComponentPage />} />

                        <Route element={<PersistLogin />}>
                            <Route element={<RequireAuth />}>
                                <Route path="user/builds" element={<UserBuildsPage />} />
                            </Route>
                        </Route>
                    </Route>
                </Routes>
            </div>
            <Footer/>
        </>
    );
}

export default App
