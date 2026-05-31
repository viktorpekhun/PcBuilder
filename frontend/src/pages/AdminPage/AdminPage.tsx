import { useLocation, useNavigate, Outlet } from "react-router-dom";
import { adminService } from "../../api/admin.service";
import { useEffect, useState } from "react";
import type { IAdminStats } from "../../types/admin.types";
import styles from "./AdminPage.module.css";

interface TabDef {
    to: string;
    exact?: boolean;
    icon: string;
    label: string;
    getCount?: (s: IAdminStats) => number | null;
}

const TABS: TabDef[] = [
    { to: "/admin",            exact: true, icon: "▦", label: "Dashboard" },
    { to: "/admin/users",                   icon: "@", label: "Users",      getCount: s => s.totalUsers },
    { to: "/admin/moderation",              icon: "⚑", label: "Moderation", getCount: s => s.pendingReports },
    { to: "/admin/scraping",                icon: "↻", label: "Scraping" },
];

const AdminPage = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const [stats, setStats] = useState<IAdminStats | null>(null);

    const fetchStats = () => {
        adminService.getStats()
            .then(r => setStats(r.data))
            .catch(() => {});
    };

    useEffect(() => {
        fetchStats();
        window.addEventListener("admin:statsChanged", fetchStats);
        return () => window.removeEventListener("admin:statsChanged", fetchStats);
    }, []);

    const isActive = (tab: TabDef) =>
        tab.exact ? location.pathname === tab.to : location.pathname.startsWith(tab.to);

    const activeTab = TABS.find(isActive) ?? TABS[0]!;

    return (
        <div className={styles.page}>
            {/* Header */}
            <div className={styles.head}>
                <div>
                    <span className={styles.eyebrow}>SYSTEM · ADMIN CONSOLE</span>
                    <h1 className={styles.h1}>{activeTab.label}</h1>
                    <div className={styles.headMeta}>
                        {stats ? (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${styles.dotAcc}`} />
                                    {stats.totalUsers.toLocaleString("en-US").replace(/,/g, " ")} USERS
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>
                                    <span className={`${styles.dot} ${stats.pendingReports > 0 ? styles.dotWarn : styles.dotDim}`} />
                                    {stats.pendingReports} PENDING REPORTS
                                </span>
                            </>
                        ) : (
                            <span className={styles.dotDim}>LOADING…</span>
                        )}
                    </div>
                </div>
            </div>

            {/* Tab bar */}
            <div className={styles.tabs}>
                {TABS.map(tab => {
                    const active = isActive(tab);
                    const count = stats && tab.getCount ? tab.getCount(stats) : null;
                    return (
                        <button
                            key={tab.to}
                            type="button"
                            className={`${styles.tabBtn} ${active ? styles.tabActive : ""}`}
                            onClick={() => navigate(tab.to)}
                        >
                            <span className={styles.tabIc}>{tab.icon}</span>
                            <span className={styles.tabLbl}>{tab.label}</span>
                            {count !== null && (
                                <span className={styles.tabCount}>
                                    {tab.to === "/admin/moderation" && count > 0
                                        ? <><b>{count}</b></>
                                        : count}
                                </span>
                            )}
                        </button>
                    );
                })}
                <div className={styles.tabSpacer} />
            </div>

            {/* Page content */}
            <div className={styles.panel}>
                <Outlet />
            </div>
        </div>
    );
};

export default AdminPage;
