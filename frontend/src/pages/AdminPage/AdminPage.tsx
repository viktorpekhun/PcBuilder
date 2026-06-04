import { useLocation, useNavigate, Outlet } from "react-router-dom";
import { adminService } from "../../api/admin.service";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { IAdminStats } from "../../types/admin.types";
import styles from "./AdminPage.module.css";

interface TabDef {
    to: string;
    exact?: boolean;
    icon: string;
    labelKey: string;
    getCount?: (s: IAdminStats) => number | null;
}

const TABS: TabDef[] = [
    { to: "/admin",            exact: true, icon: "▦", labelKey: "admin.tabs.dashboard" },
    { to: "/admin/users",                   icon: "@", labelKey: "admin.tabs.users",      getCount: s => s.totalUsers },
    { to: "/admin/moderation",              icon: "⚑", labelKey: "admin.tabs.moderation", getCount: s => s.pendingReports },
    { to: "/admin/scraping",                icon: "↻", labelKey: "admin.tabs.scraping" },
];

const AdminPage = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { t } = useTranslation();
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
                    <span className={styles.eyebrow}>{t("admin.eyebrow")}</span>
                    <h1 className={styles.h1}>{t(activeTab.labelKey)}</h1>
                    <div className={styles.headMeta}>
                        {stats ? (
                            <>
                                <span>
                                    <span className={`${styles.dot} ${styles.dotAcc}`} />
                                    {t("admin.header.users_other", { count: stats.totalUsers })}
                                </span>
                                <span className={styles.metaSep}>·</span>
                                <span>
                                    <span className={`${styles.dot} ${stats.pendingReports > 0 ? styles.dotWarn : styles.dotDim}`} />
                                    {t("admin.header.pendingReports_other", { count: stats.pendingReports })}
                                </span>
                            </>
                        ) : (
                            <span className={styles.dotDim}>{t("admin.header.loading")}</span>
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
                            <span className={styles.tabLbl}>{t(tab.labelKey)}</span>
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
