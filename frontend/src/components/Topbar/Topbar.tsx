import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";
import useLogout from "../../hooks/useLogout";
import NotificationCenter from "../NotificationCenter/NotificationCenter";
import GlobalSearch from "../GlobalSearch/GlobalSearch";
import styles from "./Topbar.module.css";

function pathSegments(pathname: string): string[] {
    if (pathname === "/") return ["composer"];
    return pathname.split("/").filter(Boolean);
}

function Avatar({ url, name }: { url?: string | undefined; name: string }) {
    if (url) {
        return (
            <span className={styles.avatar}>
                <img src={url} alt={name} />
            </span>
        );
    }
    return <span className={styles.avatar}>{name[0]?.toUpperCase() ?? "?"}</span>;
}

export default function Topbar() {
    const { auth } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const logout = useLogout();
    const [menuOpen, setMenuOpen] = useState(false);
    const menuRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
                setMenuOpen(false);
            }
        };
        document.addEventListener("mousedown", handler);
        return () => document.removeEventListener("mousedown", handler);
    }, []);

    const segs = pathSegments(location.pathname);

    const signOut = async () => {
        setMenuOpen(false);
        await logout();
        navigate("/");
    };

    return (
        <div className={styles.topbar}>
            <button className={styles.wordmark} onClick={() => navigate("/")}>
                pc<span className={styles.b}>[</span><span className={styles.s}>builder</span><span className={styles.b}>]</span>
            </button>

            <span className={styles.path}>
                {segs.map((s, i) => (
                    <span key={i}>
                        {i > 0 && <span className={styles.sep}>/</span>}
                        <span className={styles.seg}>{s}</span>
                    </span>
                ))}
            </span>

            <div className={styles.grow} />
            <GlobalSearch />

            {auth?.username ? (
                <>
                    <NotificationCenter />
                    <div className={styles.userMenu} ref={menuRef}>
                        <button
                            className={styles.userTrigger}
                            onClick={() => setMenuOpen((p) => !p)}
                            aria-expanded={menuOpen}
                        >
                            <Avatar url={auth.avatarUrl} name={auth.username} />
                            <span className={styles.triggerName}>{auth.username}</span>
                            <span className={styles.caret}>{menuOpen ? "▾" : "▸"}</span>
                        </button>
                        {menuOpen && (
                            <div className={styles.dropdown}>
                                <button
                                    className={styles.dropdownItem}
                                    onClick={() => {
                                        navigate("/profile");
                                        setMenuOpen(false);
                                    }}
                                >
                                    @ Profile
                                </button>
                                <div className={styles.dropdownDivider} />
                                <button
                                    className={`${styles.dropdownItem} ${styles.dropdownDanger}`}
                                    onClick={signOut}
                                >
                                    ↗ Sign out
                                </button>
                            </div>
                        )}
                    </div>
                </>
            ) : (
                <div className={styles.authButtons}>
                    <button className={styles.btn} onClick={() => navigate("/login")}>Sign in</button>
                    <button className={`${styles.btn} ${styles.btnPri}`} onClick={() => navigate("/register")}>Register</button>
                </div>
            )}
        </div>
    );
}
