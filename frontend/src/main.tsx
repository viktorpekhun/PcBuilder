import { createRoot } from 'react-dom/client'
import './i18n'
import './index.css'
import App from './App'
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "./context/AuthProvider";
import { NotificationProvider } from "./context/NotificationContext";
import { GoogleOAuthProvider } from '@react-oauth/google';

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID || "";

// Apply saved theme before first paint to prevent flash
// eslint-disable-next-line no-empty
try {
    const saved = localStorage.getItem("pcbuilder-theme");
    if (saved === "light") document.documentElement.setAttribute("data-theme", "light");
} catch {}

createRoot(document.getElementById('root')!).render(
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
        <BrowserRouter>
            <AuthProvider>
                <NotificationProvider>
                    <Routes>
                        <Route path="/*" element={<App />} />
                    </Routes>
                </NotificationProvider>
            </AuthProvider>
        </BrowserRouter>
    </GoogleOAuthProvider>
)
