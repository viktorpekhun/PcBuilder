import { useEffect, useState } from "react";
import {
    CartesianGrid,
    Line,
    LineChart,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from "recharts";
import type { TooltipProps } from "recharts";
import { componentService } from "../../api/component.service";
import type { ComponentType, IPriceHistoryPoint } from "../../types/component.types";
import styles from "./PriceHistoryChart.module.css";

interface PriceHistoryChartProps {
    type: ComponentType;
    id: string;
}

type Range = 7 | 30 | 90 | 0;

const ranges: { label: string; value: Range }[] = [
    { label: "7 днів", value: 7 },
    { label: "30 днів", value: 30 },
    { label: "90 днів", value: 90 },
    { label: "Весь час", value: 0 },
];

function formatDateShort(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleDateString("uk-UA", { day: "2-digit", month: "2-digit" });
}

function formatDateFull(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleDateString("uk-UA", { day: "2-digit", month: "2-digit", year: "numeric" });
}

function CustomTooltip({ active, payload }: TooltipProps<number, string>) {
    if (!active || !payload || payload.length === 0) return null;
    const point = payload[0].payload as IPriceHistoryPoint;
    return (
        <div className={styles.tooltip}>
            <div className={styles["tooltip-date"]}>{formatDateFull(point.date)}</div>
            <p className={styles["tooltip-price"]}>{point.averagePrice.toLocaleString("uk-UA")} грн</p>
            <p className={styles["tooltip-offers"]}>Пропозицій: {point.offersCount}</p>
        </div>
    );
}

export default function PriceHistoryChart({ type, id }: PriceHistoryChartProps) {
    const [range, setRange] = useState<Range>(30);
    const [data, setData] = useState<IPriceHistoryPoint[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        setLoading(true);
        setError(null);

        componentService
            .getPriceHistory(type, id, range)
            .then((res) => {
                if (!cancelled) setData(res.data);
            })
            .catch(() => {
                if (!cancelled) setError("Не вдалося завантажити історію цін");
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, [type, id, range]);

    return (
        <div className={styles["chart-container"]}>
            <div className={styles.header}>
                <h2 className={styles.title}>Історія цін</h2>
                <div className={styles["range-toggle"]}>
                    {ranges.map((r) => (
                        <button
                            key={r.value}
                            type="button"
                            className={`${styles["range-button"]} ${range === r.value ? styles.active : ""}`}
                            onClick={() => setRange(r.value)}
                        >
                            {r.label}
                        </button>
                    ))}
                </div>
            </div>

            {loading && <div className={styles.status}>Завантаження...</div>}
            {!loading && error && <div className={`${styles.status} ${styles.error}`}>{error}</div>}
            {!loading && !error && data.length === 0 && (
                <div className={styles.status}>Дані про історію цін відсутні</div>
            )}
            {!loading && !error && data.length > 0 && (
                <div className={styles["chart-wrapper"]}>
                    <ResponsiveContainer width="100%" height="100%">
                        <LineChart data={data} margin={{ top: 10, right: 20, bottom: 10, left: 10 }}>
                            <CartesianGrid strokeDasharray="3 3" stroke="#e4e7ea" />
                            <XAxis
                                dataKey="date"
                                tickFormatter={formatDateShort}
                                tick={{ fontSize: 12 }}
                            />
                            <YAxis
                                tick={{ fontSize: 12 }}
                                tickFormatter={(v: number) => v.toLocaleString("uk-UA")}
                            />
                            <Tooltip content={<CustomTooltip />} />
                            <Line
                                type="monotone"
                                dataKey="averagePrice"
                                stroke="#1971c2"
                                strokeWidth={2}
                                dot={{ r: 3 }}
                                activeDot={{ r: 5 }}
                            />
                        </LineChart>
                    </ResponsiveContainer>
                </div>
            )}
        </div>
    );
}
