import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { searchService, type IGlobalSearchItem } from "../../api/search.service";
import styles from "./GlobalSearch.module.css";

export default function GlobalSearch() {
    const { t } = useTranslation();
    const [query, setQuery] = useState("");
    const [results, setResults] = useState<IGlobalSearchItem[]>([]);
    const [loading, setLoading] = useState(false);
    const [open, setOpen] = useState(false);
    const wrapperRef = useRef<HTMLDivElement>(null);
    const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        if (debounceRef.current) clearTimeout(debounceRef.current);

        if (query.trim().length < 2) {
            setResults([]);
            setOpen(false);
            return;
        }

        debounceRef.current = setTimeout(async () => {
            setLoading(true);
            try {
                const res = await searchService.search(query.trim());
                setResults(res.data);
                setOpen(true);
            } catch {
                setResults([]);
            } finally {
                setLoading(false);
            }
        }, 300);

        return () => {
            if (debounceRef.current) clearTimeout(debounceRef.current);
        };
    }, [query]);

    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
                setOpen(false);
            }
        };
        const keyHandler = (e: KeyboardEvent) => {
            if (e.key === "Escape") setOpen(false);
        };
        document.addEventListener("mousedown", handler);
        document.addEventListener("keydown", keyHandler);
        return () => {
            document.removeEventListener("mousedown", handler);
            document.removeEventListener("keydown", keyHandler);
        };
    }, []);

    const grouped = results.reduce<Record<string, IGlobalSearchItem[]>>((acc, item) => {
        const group = item.category === "Build" ? t("globalSearch.groupBuilds") : t("globalSearch.groupComponents");
        (acc[group] ??= []).push(item);
        return acc;
    }, {});

    const handleSelect = (item: IGlobalSearchItem) => {
        setOpen(false);
        setQuery("");
        navigate(item.navigateTo);
    };

    return (
        <div className={styles.wrapper} ref={wrapperRef}>
            <div className={styles.inputRow}>
                <span className={styles.icon}>⌕</span>
                <input
                    className={styles.searchInput}
                    type="text"
                    placeholder={t("globalSearch.placeholder")}
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    onFocus={() => results.length > 0 && setOpen(true)}
                />
                {loading && <span className={styles.spinner} />}
            </div>

            {open && (
                <div className={styles.dropdown}>
                    {results.length === 0 && !loading ? (
                        <span className={styles.empty}>{t("globalSearch.noResults")}</span>
                    ) : (
                        Object.entries(grouped).map(([group, items]) => (
                            <div key={group} className={styles.group}>
                                <span className={styles.groupLabel}>{group}</span>
                                {items.map((item) => (
                                    <button
                                        key={item.id}
                                        className={styles.item}
                                        onClick={() => handleSelect(item)}
                                    >
                                        <span className={styles.itemName}>{item.name}</span>
                                        <span className={styles.chip}>{item.category}</span>
                                    </button>
                                ))}
                            </div>
                        ))
                    )}
                </div>
            )}
        </div>
    );
}
