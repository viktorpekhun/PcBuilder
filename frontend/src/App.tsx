import './App.css'
import {Routes, Route} from "react-router-dom";
import Layout from "./components/Layout";
import PcBuildPage from "./pages/PcBuildPage/PcBuildPage";
import ComponentsPage from "./pages/ComponentsPage/ComponentsPage";
import ComponentPage from "./pages/ComponentPage/ComponentPage";
import PersistLogin from "./components/PersistLogin";
import RequireAuth from "./components/RequireAuth";
import LoginPage from "./pages/LoginPage/LoginPage";
import RegisterPage from "./pages/RegisterPage/RegisterPage";
import UserBuildsPage from "./pages/UserBuildsPage/UserBuildsPage";

function App() {

    return(
        <Routes>
            <Route element={<PersistLogin />}>
                <Route path="/" element={<Layout />}>
                    <Route index element={<PcBuildPage />} />

                    <Route path="login" element={<LoginPage />} />
                    <Route path="register" element={<RegisterPage />} />

                    <Route path="components/:type" element={<ComponentsPage />} />
                    <Route path="components/:type/:id" element={<ComponentPage />} />

                    
                    <Route element={<RequireAuth />}>
                        <Route path="user/builds" element={<UserBuildsPage />} />
                    </Route>
                    
                </Route>
            </Route>
        </Routes>
    );
}

export default App
