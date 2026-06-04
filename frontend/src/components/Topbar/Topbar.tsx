import { useEffect, useMemo, useRef, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import useAuth from "../../hooks/useAuth";
import useLogout from "../../hooks/useLogout";
import NotificationCenter from "../NotificationCenter/NotificationCenter";
import GlobalSearch from "../GlobalSearch/GlobalSearch";
import { buildService } from "../../api/build.service";
import { componentService } from "../../api/component.service";
import { profileService } from "../../api/profile.service";
import type { ComponentType } from "../../types/component.types";
import styles from "./Topbar.module.css";

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const nameCache = new Map<string, string>();
const inflight = new Map<string, Promise<string | null>>();

function cacheKey(kind: "build" | "component", id: string, type?: string) {
    return kind === "build" ? `build:${id}` : `component:${type}:${id}`;
}

async function fetchBuildName(id: string): Promise<string | null> {
    const key = cacheKey("build", id);
    if (nameCache.has(key)) return nameCache.get(key)!;
    if (inflight.has(key)) return inflight.get(key)!;
    const p = (async () => {
        try {
            const res = await buildService.getPublicBuildById(id);
            const name = res.data?.name ?? null;
            if (name) nameCache.set(key, name);
            return name;
        } catch {
            return null;
        } finally {
            inflight.delete(key);
        }
    })();
    inflight.set(key, p);
    return p;
}

async function fetchComponentName(type: string, id: string): Promise<string | null> {
    const key = cacheKey("component", id, type);
    if (nameCache.has(key)) return nameCache.get(key)!;
    if (inflight.has(key)) return inflight.get(key)!;
    const p = (async () => {
        try {
            const res = await componentService.getById(type as ComponentType, id);
            const name = (res.data as { name?: string })?.name ?? null;
            if (name) nameCache.set(key, name);
            return name;
        } catch {
            return null;
        } finally {
            inflight.delete(key);
        }
    })();
    inflight.set(key, p);
    return p;
}

interface Segment {
    label: string;
    href: string;
    clickable: boolean;
}

const NON_ROUTED_PREFIXES = new Set(["components", "builds", "user"]);

function buildSegments(pathname: string): Segment[] {
    if (pathname === "/") return [{ label: "composer", href: "/", clickable: true }];
    const parts = pathname.split("/").filter(Boolean);
    return parts.map((p, i) => {
        const isFirstAndNonRouted = i === 0 && NON_ROUTED_PREFIXES.has(p);
        return {
            label: p,
            href: "/" + parts.slice(0, i + 1).join("/"),
            clickable: !isFirstAndNonRouted,
        };
    });
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
    const { t, i18n } = useTranslation();
    const [menuOpen, setMenuOpen] = useState(false);
    const [langOpen, setLangOpen] = useState(false);
    const [resolved, setResolved] = useState<Record<string, string>>({});
    const menuRef = useRef<HTMLDivElement>(null);
    const langRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
                setMenuOpen(false);
            }
            if (langRef.current && !langRef.current.contains(e.target as Node)) {
                setLangOpen(false);
            }
        };
        document.addEventListener("mousedown", handler);
        return () => document.removeEventListener("mousedown", handler);
    }, []);

    const segs = useMemo(() => buildSegments(location.pathname), [location.pathname]);

    useEffect(() => {
        const parts = location.pathname.split("/").filter(Boolean);

        const tasks: Array<{ key: string; load: () => Promise<string | null> }> = [];

        if (parts[0] === "builds" && parts[1] && UUID_RE.test(parts[1])) {
            const id = parts[1];
            tasks.push({ key: cacheKey("build", id), load: () => fetchBuildName(id) });
        }

        if (parts[0] === "components" && parts[1] && parts[2] && UUID_RE.test(parts[2])) {
            const type = parts[1];
            const id = parts[2];
            tasks.push({ key: cacheKey("component", id, type), load: () => fetchComponentName(type, id) });
        }

        if (tasks.length === 0) return;

        let cancelled = false;
        const next: Record<string, string> = {};
        for (const t of tasks) {
            const cached = nameCache.get(t.key);
            if (cached) next[t.key] = cached;
        }
        if (Object.keys(next).length > 0) setResolved((prev) => ({ ...prev, ...next }));

        tasks.forEach((t) => {
            if (nameCache.has(t.key)) return;
            t.load().then((name) => {
                if (cancelled || !name) return;
                setResolved((prev) => ({ ...prev, [t.key]: name }));
            });
        });

        return () => {
            cancelled = true;
        };
    }, [location.pathname]);

    const displaySegs = useMemo(() => {
        const parts = location.pathname.split("/").filter(Boolean);
        return segs.map((s, i) => {
            if (parts[0] === "builds" && i === 1 && UUID_RE.test(s.label)) {
                const key = cacheKey("build", s.label);
                const name = resolved[key] ?? nameCache.get(key);
                if (name) return { ...s, label: name };
            }
            if (parts[0] === "components" && i === 2 && UUID_RE.test(s.label)) {
                const key = cacheKey("component", s.label, parts[1]);
                const name = resolved[key] ?? nameCache.get(key);
                if (name) return { ...s, label: name };
            }
            return s;
        });
    }, [segs, resolved, location.pathname]);

    const signOut = async () => {
        setMenuOpen(false);
        await logout();
        navigate("/");
    };

    const LANGUAGES = [
        { code: "en", label: "English", flagImg: "https://flagcdn.com/16x12/gb.png" },
        { code: "uk", label: "Українська", flagImg: "https://flagcdn.com/16x12/ua.png" },
    ];

    const selectLanguage = (code: string) => {
        setLangOpen(false);
        if (code === i18n.language) return;
        i18n.changeLanguage(code);
        if (auth?.username) {
            profileService.updateProfile({ username: auth.username, preferredLanguage: code })
                .catch(() => {});
        }
    };

    const currentLang = LANGUAGES.find(l => l.code === i18n.language) ?? LANGUAGES[0];

    return (
        <div className={styles.topbar}>
            <button className={styles.wordmark} onClick={() => navigate("/")}>
                <span className={styles.b}>[</span><span>pcbuilder</span><span className={styles.s}>/</span><span className={styles.b}>]</span>
            </button>

            <span className={styles.path}>
                {displaySegs.map((s, i) => (
                    <span key={i}>
                        {i > 0 && <span className={styles.sep}>/</span>}
                        {s.clickable ? (
                            <Link to={s.href} className={styles.seg}>{s.label}</Link>
                        ) : (
                            <span className={`${styles.seg} ${styles.segStatic}`}>{s.label}</span>
                        )}
                    </span>
                ))}
            </span>

            <div className={styles.grow} />
            <GlobalSearch />

            <div className={styles.langMenu} ref={langRef}>
                <button
                    className={`${styles.btn} ${styles.langBtn}`}
                    onClick={() => setLangOpen(p => !p)}
                    aria-expanded={langOpen}
                >
                    {currentLang && <img src={currentLang.flagImg} alt={currentLang.code} className={styles.flagImg} />}
                    <span>{currentLang?.code.toUpperCase()}</span>
                    <span className={styles.caret}>{langOpen ? "▾" : "▸"}</span>
                </button>
                {langOpen && (
                    <div className={styles.langDropdown}>
                        {LANGUAGES.map(l => (
                            <button
                                key={l.code}
                                className={`${styles.langOption} ${l.code === i18n.language ? styles.langOptionActive : ""}`}
                                onClick={() => selectLanguage(l.code)}
                            >
                                <img src={l.flagImg} alt={l.code} className={styles.flagImg} />
                                <span>{l.label}</span>
                            </button>
                        ))}
                    </div>
                )}
            </div>

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
                                    {t("topbar.profile")}
                                </button>
                                <div className={styles.dropdownDivider} />
                                <button
                                    className={`${styles.dropdownItem} ${styles.dropdownDanger}`}
                                    onClick={signOut}
                                >
                                    {t("topbar.signOut")}
                                </button>
                            </div>
                        )}
                    </div>
                </>
            ) : (
                <div className={styles.authButtons}>
                    <button className={styles.btn} onClick={() => navigate("/login")}>{t("topbar.signIn")}</button>
                    <button className={`${styles.btn} ${styles.btnPri}`} onClick={() => navigate("/register")}>{t("topbar.register")}</button>
                </div>
            )}
        </div>
    );
}
