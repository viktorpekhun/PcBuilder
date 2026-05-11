import { NavLink, Outlet } from "react-router-dom";
import styles from "./AdminLayout.module.css";

const navItems = [
    { to: "/admin", label: "Dashboard", end: true },
    { to: "/admin/users", label: "Users" },
    { to: "/admin/moderation", label: "Moderation" },
    { to: "/admin/scraping", label: "Scraping" },
];

const AdminLayout = () => {
    return (
        <div className={styles.page}>
            <aside className={styles.sidebar}>
                <div className={styles["sidebar-title"]}>Admin Panel</div>
                <nav className={styles.nav}>
                    {navItems.map(item => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.end}
                            className={({ isActive }) =>
                                isActive
                                    ? `${styles["nav-link"]} ${styles["nav-link-active"]}`
                                    : styles["nav-link"]
                            }
                        >
                            {item.label}
                        </NavLink>
                    ))}
                </nav>
            </aside>
            <main className={styles.content}>
                <Outlet />
            </main>
        </div>
    );
};

export default AdminLayout;
