import { useEffect, useState } from "react";

type Theme = "dark" | "light";

const STORAGE_KEY = "pcbuilder-theme";

function getInitial(): Theme {
    try {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (saved === "light" || saved === "dark") return saved;
    } catch {}
    return "dark";
}

function applyTheme(theme: Theme) {
    if (theme === "light") {
        document.documentElement.setAttribute("data-theme", "light");
    } else {
        document.documentElement.removeAttribute("data-theme");
    }
}

export function useTheme() {
    const [theme, setTheme] = useState<Theme>(getInitial);

    useEffect(() => {
        applyTheme(theme);
        try { localStorage.setItem(STORAGE_KEY, theme); } catch {}
    }, [theme]);

    const toggle = () => setTheme(t => (t === "dark" ? "light" : "dark"));

    return { theme, toggle };
}
