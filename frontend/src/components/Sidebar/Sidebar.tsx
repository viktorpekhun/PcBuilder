import { Fragment, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import useAuth from "../../hooks/useAuth";
import useLogout from "../../hooks/useLogout";
import useNotifications from "../../hooks/useNotifications";
import { componentService } from "../../api/component.service";
import type { ComponentType } from "../../types/component.types";
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

interface ComponentTypeEntry {
    id: string;
    api: ComponentType;
    gly: string;
    label: string;
}

const COMPONENT_TYPES: ComponentTypeEntry[] = [
    { id: "cpu",         api: "Cpu",         gly: "CPU", label: "Processors" },
    { id: "gpu",         api: "Gpu",         gly: "GPU", label: "Graphics cards" },
    { id: "motherboard", api: "Motherboard", gly: "M/B", label: "Motherboards" },
    { id: "ram",         api: "Ram",         gly: "RAM", label: "Memory" },
    { id: "ssd",         api: "Ssd",         gly: "SSD", label: "SSD storage" },
    { id: "hdd",         api: "Hdd",         gly: "HDD", label: "Hard drives" },
    { id: "powerSupply", api: "PowerSupply", gly: "PSU", label: "Power supplies" },
    { id: "cpuCooler",   api: "CpuCooler",   gly: "CLR", label: "CPU coolers" },
    { id: "pcCase",      api: "PcCase",      gly: "CSE", label: "Cases" },
    { id: "fan",         api: "Fan",         gly: "FAN", label: "Case fans" },
];

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

    const componentsActive = location.pathname.startsWith("/components");
    const activeTypeId = componentsActive
        ? (location.pathname.split("/")[2] ?? "")
        : "";

    const [typeCounts, setTypeCounts] = useState<Record<string, number>>({});

    useEffect(() => {
        let cancelled = false;
        (async () => {
            const results = await Promise.all(
                COMPONENT_TYPES.map(async (ct) => {
                    try {
                        const res = await componentService.getAll(ct.api, {
                            pageNumber: 1,
                            pageSize: 1,
                            orderBy: "offersCount",
                            ascending: false,
                        });
                        const headers = res.headers as unknown as {
                            get?: (k: string) => string | null;
                            [k: string]: unknown;
                        };
                        const raw =
                            (typeof headers.get === "function" ? headers.get("x-pagination") : null) ??
                            (headers["x-pagination"] as string | undefined) ??
                            (headers["X-Pagination"] as string | undefined);
                        if (!raw) {
                            console.warn(`[Sidebar] X-Pagination missing for ${ct.api}`);
                            return [ct.id, 0] as const;
                        }
                        const parsed = JSON.parse(raw) as { totalCount?: number };
                        return [ct.id, parsed.totalCount ?? 0] as const;
                    } catch (err) {
                        console.warn(`[Sidebar] count fetch failed for ${ct.api}`, err);
                        return [ct.id, 0] as const;
                    }
                })
            );
            if (cancelled) return;
            const counts: Record<string, number> = {};
            for (const [id, n] of results) counts[id] = n;
            setTypeCounts(counts);
        })();
        return () => { cancelled = true; };
    }, []);

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
                    ...(unreadCount > 0 ? { count: unreadCount } : {}),
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
                        <Fragment key={it.id}>
                            <button
                                className={`${styles.item} ${isActive(it.path) ? styles.active : ""}`}
                                onClick={() => navigate(it.path)}
                            >
                                <span className={styles.ic}>{it.ic}</span>
                                <span className={styles.label}>{it.label}</span>
                                {it.count != null && <span className={styles.count}>{it.count}</span>}
                            </button>
                            {it.id === "components" && componentsActive && (
                                <div className={styles.typeItems}>
                                    {COMPONENT_TYPES.map((ct) => (
                                        <button
                                            key={ct.id}
                                            className={`${styles.typeItem} ${activeTypeId === ct.id ? styles.active : ""}`}
                                            onClick={() => navigate(`/components/${ct.id}`)}
                                        >
                                            <span className={styles.typeGly}>{ct.gly}</span>
                                            <span className={styles.label}>{ct.label}</span>
                                            {typeCounts[ct.id] != null && (
                                                <span className={styles.typeCount}>{typeCounts[ct.id]}</span>
                                            )}
                                        </button>
                                    ))}
                                </div>
                            )}
                        </Fragment>
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
