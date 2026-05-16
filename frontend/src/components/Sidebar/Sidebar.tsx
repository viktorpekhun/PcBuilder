import { useLocation, useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";
import useLogout from "../../hooks/useLogout";
import useNotifications from "../../hooks/useNotifications";
import styles from "./Sidebar.module.css";

interface NavItem {
    id: string;
    ic: string;
    label: string;
    path: string;
    count?: number;
}

interface NavGroup {
    grp: string;
    items: NavItem[];
}

export default function Sidebar() {
    const navigate = useNavigate();
    const location = useLocation();
    const { auth } = useAuth();
    const logout = useLogout();
    const { unreadCount } = useNotifications();

    const isActive = (path: string) => {
        if (path === "/") return location.pathname === "/";
        return location.pathname.startsWith(path);
    };

    const groups: NavGroup[] = [
        {
            grp: "BUILD",
            items: [
                { id: "build", ic: "▣", label: "Composer", path: "/" },
                { id: "components", ic: "≡", label: "Components", path: "/components/cpu" },
            ],
        },
        {
            grp: "COMMUNITY",
            items: [
                { id: "gallery", ic: "⌘", label: "Gallery", path: "/gallery" },
            ],
        },
    ];

    if (auth?.username) {
        groups.push({
            grp: "YOU",
            items: [
                { id: "saved", ic: "↓", label: "Saved builds", path: "/user/builds" },
                { id: "profile", ic: "@", label: "Profile", path: "/profile" },
                {
                    id: "notifs", ic: "●", label: "Notifications", path: "/notifications",
                    count: unreadCount > 0 ? unreadCount : undefined,
                },
            ],
        });
    }

    if (auth?.roles?.includes("Admin")) {
        groups.push({
            grp: "ADMIN",
            items: [
                { id: "admin", ic: "⚙", label: "Admin", path: "/admin" },
            ],
        });
    }

    const handleSignOut = async () => {
        await logout();
        navigate("/");
    };

    return (
        <nav className={styles.rail}>
            {groups.map((g) => (
                <div key={g.grp}>
                    <div className={styles.grp}>{g.grp}</div>
                    {g.items.map((it) => (
                        <button
                            key={it.id}
                            className={`${styles.item} ${isActive(it.path) ? styles.active : ""}`}
                            onClick={() => navigate(it.path)}
                        >
                            <span className={styles.ic}>{it.ic}</span>
                            <span className={styles.label}>{it.label}</span>
                            {it.count != null && <span className={styles.count}>{it.count}</span>}
                        </button>
                    ))}
                </div>
            ))}

            <div className={styles.grp} style={{ marginTop: 24 }}>SYSTEM</div>
            {auth?.username ? (
                <button className={styles.item} onClick={handleSignOut}>
                    <span className={styles.ic}>↗</span>
                    <span className={styles.label}>Sign out</span>
                </button>
            ) : (
                <>
                    <button className={styles.item} onClick={() => navigate("/login")}>
                        <span className={styles.ic}>→</span>
                        <span className={styles.label}>Sign in</span>
                    </button>
                    <button className={styles.item} onClick={() => navigate("/register")}>
                        <span className={styles.ic}>+</span>
                        <span className={styles.label}>Register</span>
                    </button>
                </>
            )}

            <div className={styles.footer}>
                v 0.42 · build 2026.05.16<br />
                ● <span className={styles.ok}>all systems ok</span>
            </div>
        </nav>
    );
}
