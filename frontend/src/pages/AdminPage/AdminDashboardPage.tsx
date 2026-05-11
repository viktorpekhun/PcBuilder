import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adminService } from "../../api/admin.service";
import type { IAdminStats } from "../../types/admin.types";
import styles from "./AdminDashboardPage.module.css";

const componentLabels: Record<string, string> = {
    cpu: "CPU",
    gpu: "GPU",
    motherboard: "Motherboard",
    ram: "RAM",
    ssd: "SSD",
    hdd: "HDD",
    cpuCooler: "CPU Cooler",
    fan: "Fan",
    pcCase: "PC Case",
    powerSupply: "Power Supply",
};

const AdminDashboardPage = () => {
    const [stats, setStats] = useState<IAdminStats | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        adminService.getStats()
            .then(res => {
                if (!cancelled) setStats(res.data);
            })
            .catch(() => {
                if (!cancelled) setError("Failed to load stats.");
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });
        return () => { cancelled = true; };
    }, []);

    if (loading) {
        return (
            <div className={styles.page}>
                <h1 className={styles.title}>Dashboard</h1>
                <div className={styles.loading}>Loading stats…</div>
            </div>
        );
    }

    if (error || !stats) {
        return (
            <div className={styles.page}>
                <h1 className={styles.title}>Dashboard</h1>
                <div className={styles.error}>{error ?? "No data."}</div>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <div>
                <h1 className={styles.title}>Dashboard</h1>
                <p className={styles.subtitle}>Site overview and quick actions.</p>
            </div>

            <div className={styles["cards-row"]}>
                <div className={`${styles.card} ${styles["card-accent-blue"]}`}>
                    <span className={styles["card-label"]}>Total Users</span>
                    <span className={styles["card-value"]}>{stats.totalUsers}</span>
                    <span className={styles["card-hint"]}>+{stats.newUsersLast7Days} in last 7 days</span>
                </div>

                <div className={`${styles.card} ${styles["card-accent-green"]}`}>
                    <span className={styles["card-label"]}>Total Builds</span>
                    <span className={styles["card-value"]}>{stats.totalBuilds}</span>
                    <span className={styles["card-hint"]}>{stats.publishedBuilds} published</span>
                </div>

                <div className={`${styles.card} ${styles["card-accent-amber"]}`}>
                    <span className={styles["card-label"]}>Total Reviews</span>
                    <span className={styles["card-value"]}>{stats.totalReviews}</span>
                </div>

                <div className={`${styles.card} ${styles["card-accent-red"]}`}>
                    <span className={styles["card-label"]}>Pending Reports</span>
                    <span className={styles["card-value"]}>{stats.pendingReports}</span>
                    <span className={styles["card-hint"]}>
                        {stats.activeBannedUsers} active ban{stats.activeBannedUsers === 1 ? "" : "s"}
                    </span>
                </div>
            </div>

            <section className={styles.section}>
                <h2 className={styles["section-title"]}>Component Catalog</h2>
                <div className={styles["components-grid"]}>
                    {Object.entries(stats.componentCounts).map(([key, count]) => (
                        <div key={key} className={styles["component-tile"]}>
                            <span className={styles["component-label"]}>
                                {componentLabels[key] ?? key}
                            </span>
                            <span className={styles["component-value"]}>{count}</span>
                        </div>
                    ))}
                </div>
            </section>

            <section className={styles.section}>
                <h2 className={styles["section-title"]}>Quick Actions</h2>
                <div className={styles["quick-actions"]}>
                    <Link to="/admin/users" className={styles["action-link"]}>Manage Users</Link>
                    <Link to="/admin/moderation" className={styles["action-link"]}>
                        Moderation Queue ({stats.pendingReports})
                    </Link>
                    <Link
                        to="/admin/scraping"
                        className={`${styles["action-link"]} ${styles["action-link-secondary"]}`}
                    >
                        Scraping Control
                    </Link>
                </div>
            </section>
        </div>
    );
};

export default AdminDashboardPage;
