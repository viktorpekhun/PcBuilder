import { Fragment, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
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
    labelKey: string;
}

const COMPONENT_TYPES: ComponentTypeEntry[] = [
    { id: "cpu",         api: "Cpu",         gly: "CPU", labelKey: "componentTypes.cpu" },
    { id: "gpu",         api: "Gpu",         gly: "GPU", labelKey: "componentTypes.gpu" },
    { id: "motherboard", api: "Motherboard", gly: "M/B", labelKey: "componentTypes.motherboard" },
    { id: "ram",         api: "Ram",         gly: "RAM", labelKey: "componentTypes.ram" },
    { id: "ssd",         api: "Ssd",         gly: "SSD", labelKey: "componentTypes.ssd" },
    { id: "hdd",         api: "Hdd",         gly: "HDD", labelKey: "componentTypes.hdd" },
    { id: "powerSupply", api: "PowerSupply", gly: "PSU", labelKey: "componentTypes.powerSupply" },
    { id: "cpuCooler",   api: "CpuCooler",   gly: "CLR", labelKey: "componentTypes.cpuCooler" },
    { id: "pcCase",      api: "PcCase",      gly: "CSE", labelKey: "componentTypes.pcCase" },
    { id: "fan",         api: "Fan",         gly: "FAN", labelKey: "componentTypes.fan" },
];

export default function Sidebar() {
    const navigate = useNavigate();
    const location = useLocation();
    const { auth } = useAuth();
    const logout = useLogout();
    const { unreadCount } = useNotifications();
    const { t } = useTranslation();

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
            grp: t("nav.build"),
            items: [
                { id: "build", ic: "▣", label: t("nav.composer"), path: "/" },
                { id: "components", ic: "≡", label: t("nav.components"), path: "/components/cpu" },
            ],
        },
        {
            grp: t("nav.community"),
            items: [
                { id: "gallery", ic: "⌘", label: t("nav.gallery"), path: "/gallery" },
            ],
        },
    ];

    if (auth?.username) {
        groups.push({
            grp: t("nav.you"),
            items: [
                { id: "saved", ic: "↓", label: t("nav.savedBuilds"), path: "/user/builds" },
                { id: "profile", ic: "@", label: t("nav.profile"), path: "/profile" },
                {
                    id: "notifs", ic: "●", label: t("nav.notifications"), path: "/notifications",
                    ...(unreadCount > 0 ? { count: unreadCount } : {}),
                },
            ],
        });
    }

    if (auth?.roles?.includes("Admin")) {
        groups.push({
            grp: t("nav.admin"),
            items: [
                { id: "admin", ic: "⚙", label: t("nav.adminPanel"), path: "/admin" },
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
                                            <span className={styles.label}>{t(ct.labelKey)}</span>
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

            <div className={styles.grp} style={{ marginTop: 24 }}>{t("nav.system")}</div>
            {auth?.username ? (
                <button className={styles.item} onClick={handleSignOut}>
                    <span className={styles.ic}>↗</span>
                    <span className={styles.label}>{t("nav.signOut")}</span>
                </button>
            ) : (
                <>
                    <button className={styles.item} onClick={() => navigate("/login")}>
                        <span className={styles.ic}>→</span>
                        <span className={styles.label}>{t("nav.signIn")}</span>
                    </button>
                    <button className={styles.item} onClick={() => navigate("/register")}>
                        <span className={styles.ic}>+</span>
                        <span className={styles.label}>{t("nav.register")}</span>
                    </button>
                </>
            )}

            <div className={styles.footer}>
                v 0.42 · build 2026.05.16<br />
                ● <span className={styles.ok}>{t("nav.allSystemsOk")}</span>
            </div>
        </nav>
    );
}
