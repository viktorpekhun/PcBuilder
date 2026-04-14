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
import VerifyEmailPage from "./pages/VerifyEmailPage/VerifyEmailPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage/ResetPasswordPage";
import ProfilePage from "./pages/ProfilePage/ProfilePage";
import GalleryPage from "./pages/GalleryPage/GalleryPage";
import BuildDetailPage from "./pages/BuildDetailPage/BuildDetailPage";
import AdminLayout from "./components/AdminLayout/AdminLayout";
import AdminDashboardPage from "./pages/AdminPage/AdminDashboardPage";
import AdminUsersPage from "./pages/AdminPage/AdminUsersPage";
import AdminModerationPage from "./pages/AdminPage/AdminModerationPage";
import AdminScrapingPage from "./pages/AdminPage/AdminScrapingPage";
import NotificationsPage from "./pages/NotificationsPage/NotificationsPage";

function App() {

    return(
        <Routes>
            <Route element={<PersistLogin />}>
                <Route path="/" element={<Layout />}>
                    <Route index element={<PcBuildPage />} />

                    <Route path="login" element={<LoginPage />} />
                    <Route path="register" element={<RegisterPage />} />

                    <Route path="gallery" element={<GalleryPage />} />
                    <Route path="builds/:id" element={<BuildDetailPage />} />
                    <Route path="components/:type" element={<ComponentsPage />} />
                    <Route path="components/:type/:id" element={<ComponentPage />} />
                    <Route path="verify-email" element={<VerifyEmailPage />} />
                    <Route path="forgot-password" element={<ForgotPasswordPage />} />
                    <Route path="reset-password" element={<ResetPasswordPage />} />

                    
                    <Route element={<RequireAuth />}>
                        <Route path="user/builds" element={<UserBuildsPage />} />
                        <Route path="profile" element={<ProfilePage />} />
                        <Route path="notifications" element={<NotificationsPage />} />
                    </Route>

                    <Route element={<RequireAuth allowedRoles={['Admin']} />}>
                        <Route path="admin" element={<AdminLayout />}>
                            <Route index element={<AdminDashboardPage />} />
                            <Route path="users" element={<AdminUsersPage />} />
                            <Route path="moderation" element={<AdminModerationPage />} />
                            <Route path="scraping" element={<AdminScrapingPage />} />
                        </Route>
                    </Route>
                    
                </Route>
            </Route>
        </Routes>
    );
}

export default App
