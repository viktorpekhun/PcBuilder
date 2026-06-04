import { useState, useEffect, useCallback, Fragment } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { TFunction } from 'i18next';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faShoppingBag } from '@fortawesome/free-solid-svg-icons';
import styles from './ComponentPage.module.css';
import { componentService } from '../../api/component.service';
import { buildService } from '../../api/build.service';
import type { ComponentType, IComponentBase, IProductOffer, IPriceHistoryPoint } from '../../types/component.types';
import type { IBuildCompatibilityReport, IComponentsCompatibility } from '../../types/build.types';
import { getComponentSpecFullConfigs } from './componentSpecsFullConfigs';
import type { FullSpecConfig } from './componentSpecsFullConfigs';
import PriceAlertButton from '../../components/PriceAlertButton/PriceAlertButton';

// ── constants ────────────────────────────────────────────────

const TYPE_META: Record<string, { gly: string; labelKey: string }> = {
    Cpu:         { gly: 'CPU', labelKey: 'componentTypes.cpu' },
    Gpu:         { gly: 'GPU', labelKey: 'componentTypes.gpu' },
    Motherboard: { gly: 'M/B', labelKey: 'componentTypes.motherboard' },
    Ram:         { gly: 'RAM', labelKey: 'componentTypes.ram' },
    Ssd:         { gly: 'SSD', labelKey: 'componentTypes.ssd' },
    Hdd:         { gly: 'HDD', labelKey: 'componentTypes.hdd' },
    PowerSupply: { gly: 'PSU', labelKey: 'componentTypes.powerSupply' },
    CpuCooler:  { gly: 'CLR', labelKey: 'componentTypes.cpuCooler' },
    PcCase:      { gly: 'CSE', labelKey: 'componentTypes.pcCase' },
    Fan:         { gly: 'FAN', labelKey: 'componentTypes.fan' },
};

const MULTI_COMPONENT_TYPES = ['ram', 'ssd', 'hdd', 'fan'];

// normalises lowercase URL param → PascalCase key used in TYPE_META and componentSpecFullConfigs
const TYPE_KEY_MAP: Record<string, string> = {
    cpu: 'Cpu', gpu: 'Gpu', ram: 'Ram', motherboard: 'Motherboard',
    cpucooler: 'CpuCooler', pccase: 'PcCase', powersupply: 'PowerSupply',
    ssd: 'Ssd', hdd: 'Hdd', fan: 'Fan',
};

// maps lowercase URL param → camelCase localStorage key (matching PcBuildPage/types.ts SelectedComponents)
const TYPE_TO_STORAGE_KEY: Record<string, string> = {
    cpu: 'cpu', gpu: 'gpu', motherboard: 'motherboard',
    cpucooler: 'cpuCooler', powersupply: 'powerSupply', pccase: 'pcCase',
};

// ── localStorage types ───────────────────────────────────────

interface SelectedOffer {
    componentId: string;
    offerId: string;
    price: number;
    storeName: string;
    storeLogoUrl?: string | null;
    productOfferUrl: string;
}
interface SelectedMulti extends SelectedOffer { quantity: number; }
type SelectedComponents = Record<string, SelectedOffer | SelectedMulti[]>;

// ── helper: build compatibility payload ─────────────────────

function buildCompatPayload(
    selected: SelectedComponents,
    type: string,
    id: string,
): IComponentsCompatibility {
    const getSingle = (key: string) =>
        (selected[key] as SelectedOffer | undefined)?.componentId;
    const getMulti = (key: string) => {
        const arr = selected[key];
        if (!Array.isArray(arr)) return [];
        return (arr as SelectedMulti[]).map(i => ({
            componentId: i.componentId,
            quantity: i.quantity ?? 1,
        }));
    };

    const data = {
        ...(getSingle('cpu')         ? { cpuId:         getSingle('cpu')!         } : {}),
        ...(getSingle('gpu')         ? { gpuId:         getSingle('gpu')!         } : {}),
        ...(getSingle('motherboard') ? { motherboardId: getSingle('motherboard')! } : {}),
        ...(getSingle('cpuCooler')   ? { cpuCoolerId:   getSingle('cpuCooler')!   } : {}),
        ...(getSingle('powerSupply') ? { powerSupplyId: getSingle('powerSupply')! } : {}),
        ...(getSingle('pcCase')      ? { pcCaseId:      getSingle('pcCase')!      } : {}),
        rams:  getMulti('rams'),
        ssds:  getMulti('ssds'),
        hdds:  getMulti('hdds'),
        fans:  getMulti('fans'),
    } as IComponentsCompatibility;

    const lc = type.toLowerCase();
    if      (lc === 'cpu')         data.cpuId = id;
    else if (lc === 'gpu')         data.gpuId = id;
    else if (lc === 'motherboard') data.motherboardId = id;
    else if (lc === 'cpucooler')   data.cpuCoolerId = id;
    else if (lc === 'powersupply') data.powerSupplyId = id;
    else if (lc === 'pccase')      data.pcCaseId = id;
    else if (lc === 'ram')  data.rams  = [...(data.rams  ?? []), { componentId: id, quantity: 1 }];
    else if (lc === 'ssd')  data.ssds  = [...(data.ssds  ?? []), { componentId: id, quantity: 1 }];
    else if (lc === 'hdd')  data.hdds  = [...(data.hdds  ?? []), { componentId: id, quantity: 1 }];
    else if (lc === 'fan')  data.fans  = [...(data.fans  ?? []), { componentId: id, quantity: 1 }];

    return data;
}

// ── inline sub-components ────────────────────────────────────

function ComponentPhotoPlaceholder({ gly }: { gly: string }) {
    const fans = (cx: number, cy: number, r1: number, r2: number, count = 7) =>
        Array.from({ length: count }, (_, i) => {
            const a = (i * Math.PI * 2) / count;
            const a3 = a + 0.85;
            return <path key={i} d={`M${cx+Math.cos(a)*r1} ${cy+Math.sin(a)*r1} Q${cx+Math.cos(a+0.4)*(r1+r2)/2} ${cy+Math.sin(a+0.4)*(r1+r2)/2} ${cx+Math.cos(a3)*r2} ${cy+Math.sin(a3)*r2}`} stroke="#44444E" strokeWidth="1.4" fill="none" />;
        });

    const psuBlades = () => Array.from({ length: 7 }, (_, i) => {
        const a = (i * Math.PI * 2) / 7;
        const x1 = 100+Math.cos(a)*8, y1 = 94+Math.sin(a)*8;
        const x2 = 100+Math.cos(a+0.5)*30, y2 = 94+Math.sin(a+0.5)*30;
        return <path key={i} d={`M${x1} ${y1} Q${100+Math.cos(a+0.2)*22} ${94+Math.sin(a+0.2)*22} ${x2} ${y2}`} stroke="#44444E" strokeWidth="1.4" fill="none" />;
    });

    const label = (txt: string, y = 170) => (
        <text x="100" y={y} textAnchor="middle" fontFamily="IBM Plex Mono, monospace" fontSize="11" letterSpacing="3" fontWeight="600" fill="#A6A6AE">{txt}</text>
    );

    const mono = (txt: string, x: number, y: number, size = 7) => (
        <text x={x} y={y} textAnchor="middle" fontFamily="IBM Plex Mono, monospace" fontSize={size} letterSpacing="1.6" fill="#70707A">{txt}</text>
    );

    const inner = (() => {
        switch (gly) {
            case 'GPU': return (
                <g transform="translate(7, 0)">
                <rect x="14" y="60" width="172" height="86" stroke="#44444E" strokeWidth="1.2" fill="#1E1E23"/>
                <rect x="14" y="60" width="172" height="14" fill="#2A2A30"/>
                <circle cx="44" cy="103" r="14" stroke="#6B6B76" strokeWidth="1" fill="none"/>
                <circle cx="100" cy="103" r="14" stroke="#6B6B76" strokeWidth="1" fill="none"/>
                <circle cx="156" cy="103" r="14" stroke="#6B6B76" strokeWidth="1" fill="none"/>
                <rect x="14" y="146" width="172" height="12" fill="#16161A" stroke="#44444E" strokeWidth="0.8"/>
                <rect x="0" y="60" width="14" height="86" fill="#16161A" stroke="#44444E" strokeWidth="0.8"/>
                {label('GPU')}
                </g>
            );
            case 'M/B': return (<>
                <rect x="20" y="20" width="160" height="160" fill="#16161A" stroke="#44444E" strokeWidth="1.2"/>
                <rect x="74" y="74" width="32" height="32" fill="#2A2A30" stroke="#6B6B76"/>
                {mono('CPU', 90, 95, 9)}
                <rect x="118" y="34" width="8" height="64" fill="#2A2A30"/>
                <rect x="130" y="34" width="8" height="64" fill="#2A2A30"/>
                <rect x="142" y="34" width="8" height="64" fill="#2A2A30"/>
                <rect x="154" y="34" width="8" height="64" fill="#2A2A30"/>
                <rect x="34" y="124" width="118" height="10" fill="#1E1E23" stroke="#44444E"/>
                <rect x="34" y="144" width="74" height="10" fill="#1E1E23" stroke="#44444E"/>
                {label('M/B')}
            </>);
            case 'RAM': return (<>
                <rect x="22" y="56" width="156" height="74" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <rect x="30" y="64" width="140" height="44" fill="#2A2A30" stroke="#44444E" strokeWidth="0.8"/>
                {[0,1,2,3,4,5].map(i => <rect key={i} x={36+i*22} y="70" width="14" height="32" fill="#16161A" stroke="#44444E" strokeWidth="0.6"/>)}
                {Array.from({length:36},(_,i) => <rect key={i} x={24+i*4.3} y="128" width="2" height="6" fill="#44444E"/>)}
                <rect x="86" y="128" width="28" height="6" fill="#16161A"/>
                {label('RAM')}
            </>);
            case 'SSD': return (<>
                <rect x="42" y="76" width="116" height="48" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <rect x="50" y="84" width="44" height="32" fill="#2A2A30" stroke="#44444E"/>
                <rect x="100" y="84" width="22" height="32" fill="#2A2A30" stroke="#44444E"/>
                <rect x="126" y="84" width="26" height="14" fill="#2A2A30" stroke="#44444E"/>
                <rect x="126" y="102" width="26" height="14" fill="#2A2A30" stroke="#44444E"/>
                <circle cx="156" cy="80" r="2" fill="#44444E"/>
                <rect x="32" y="124" width="136" height="6" fill="#16161A"/>
                {[0,1,2,3,4,5,6,7,8].map(i => <rect key={i} x={36+i*15} y="124" width="6" height="6" fill="#44444E"/>)}
                {label('SSD')}
            </>);
            case 'HDD': return (<>
                <rect x="30" y="30" width="140" height="140" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <circle cx="100" cy="92" r="46" fill="#16161A" stroke="#44444E" strokeWidth="1"/>
                <circle cx="100" cy="92" r="32" fill="#0F0F12" stroke="#44444E" strokeWidth="0.6"/>
                <circle cx="100" cy="92" r="10" fill="#2A2A30" stroke="#6B6B76" strokeWidth="0.8"/>
                <circle cx="100" cy="92" r="3" fill="#44444E"/>
                <line x1="100" y1="92" x2="146" y2="76" stroke="#6B6B76" strokeWidth="1.5"/>
                <circle cx="146" cy="76" r="4" fill="#2A2A30" stroke="#6B6B76"/>
                <rect x="38" y="146" width="124" height="14" fill="#16161A" stroke="#44444E" strokeWidth="0.8"/>
                {label('HDD')}
            </>);
            case 'PSU': return (<>
                <rect x="20" y="44" width="160" height="100" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <circle cx="100" cy="94" r="34" fill="#16161A" stroke="#44444E"/>
                <circle cx="100" cy="94" r="6" fill="#2A2A30" stroke="#6B6B76"/>
                {psuBlades()}
                <rect x="20" y="140" width="160" height="6" fill="#16161A"/>
                {[0,1,2,3,4,5,6,7,8,9,10,11].map(i => <rect key={i} x={28+i*12} y="140" width="6" height="6" fill="#44444E"/>)}
                {label('PSU')}
            </>);
            case 'CLR': return (<>
                <rect x="48" y="30" width="104" height="72" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                {[0,1,2,3,4,5,6,7,8,9,10,11].map(i => <line key={i} x1={52+i*8.5} y1="34" x2={52+i*8.5} y2="98" stroke="#44444E" strokeWidth="0.7"/>)}
                <circle cx="100" cy="130" r="32" fill="#16161A" stroke="#6B6B76" strokeWidth="1.2"/>
                <circle cx="100" cy="130" r="22" fill="#1E1E23" stroke="#44444E"/>
                <circle cx="100" cy="130" r="5" fill="#2A2A30" stroke="#6B6B76"/>
                {fans(100, 130, 6, 21)}
                {label('CLR', 178)}
            </>);
            case 'CSE': return (<>
                <rect x="56" y="20" width="88" height="156" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <rect x="64" y="28" width="72" height="40" fill="#16161A" stroke="#44444E"/>
                <circle cx="76" cy="38" r="2" fill="#44444E"/>
                <circle cx="84" cy="38" r="2" fill="#44444E"/>
                <rect x="68" y="46" width="64" height="16" fill="#2A2A30"/>
                {[0,1,2,3,4,5,6,7,8,9].map(i => <rect key={i} x="64" y={76+i*9} width="72" height="4" fill="#16161A" stroke="#44444E" strokeWidth="0.5"/>)}
                <circle cx="100" cy="166" r="6" fill="#16161A" stroke="#44444E"/>
                {label('CSE', 194)}
            </>);
            case 'FAN': return (<>
                <rect x="22" y="22" width="156" height="156" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                {([[22,22],[166,22],[22,166],[166,166]] as [number,number][]).map(([x,y],i) => <circle key={i} cx={x+12} cy={y+12} r="6" fill="#16161A" stroke="#44444E"/>)}
                <circle cx="100" cy="100" r="64" fill="#16161A" stroke="#6B6B76" strokeWidth="1"/>
                <circle cx="100" cy="100" r="10" fill="#2A2A30" stroke="#6B6B76"/>
                {Array.from({length:7},(_,i) => {
                    const a = (i * Math.PI * 2) / 7;
                    const a3 = a + 0.85;
                    return <path key={i} d={`M${100+Math.cos(a)*11} ${100+Math.sin(a)*11} Q${100+Math.cos(a+0.45)*35} ${100+Math.sin(a+0.45)*35} ${100+Math.cos(a3)*60} ${100+Math.sin(a3)*60}`} stroke="#44444E" strokeWidth="2" fill="none"/>;
                })}
                {label('FAN', 170)}
            </>);
            default: return (<> {/* CPU */}
                <rect x="40" y="40" width="120" height="120" fill="#1E1E23" stroke="#6B6B76" strokeWidth="1.2"/>
                <rect x="60" y="60" width="80" height="80" fill="#2A2A30" stroke="#44444E" strokeWidth="0.8"/>
                {mono('CPU', 100, 106, 11)}
                {[0,1,2,3,4,5,6,7].map(i => (<g key={i}>
                    <rect x={48+i*12.5} y="35" width="6" height="5" fill="#44444E"/>
                    <rect x={48+i*12.5} y="160" width="6" height="5" fill="#44444E"/>
                    <rect x="35" y={48+i*12.5} width="5" height="6" fill="#44444E"/>
                    <rect x="160" y={48+i*12.5} width="5" height="6" fill="#44444E"/>
                </g>))}
            </>);
        }
    })();

    return (
        <svg viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg" fill="none" style={{ width: '100%', height: '100%' }}>
            {inner}
        </svg>
    );
}

const MONTH_SHORT = ['JAN','FEB','MAR','APR','MAY','JUN','JUL','AUG','SEP','OCT','NOV','DEC'];

function fmtPrice(p: number) {
    if (p >= 1000) return `₴ ${(p / 1000).toFixed(1)}K`;
    return `₴ ${Math.round(p)}`;
}

function PriceChart({ data, avgPrice, range, t }: { data: IPriceHistoryPoint[]; avgPrice: number; range: string; t: TFunction }) {
    if (data.length === 0) {
        return <div className={styles.chartEmpty}>{t('components.componentPage.noHistoryData')}</div>;
    }

    const W = 760, H = 200, PX = 56, PY = 16, PB = 28;
    const innerH = H - PY - PB;
    const prices = data.map(d => d.averagePrice);
    const minP = Math.min(...prices);
    const maxP = Math.max(...prices);
    const pRange = maxP - minP || maxP * 0.1 || 1;
    const count = data.length;

    const toX = (i: number) => count === 1 ? PX + (W - PX * 2) / 2 : PX + (i / (count - 1)) * (W - PX * 2);
    const toY = (p: number) => PY + (1 - (p - minP) / pRange) * innerH;

    const pts = data.map((d, i) => `${toX(i)},${toY(d.averagePrice)}`).join(' ');
    const firstX = toX(0);
    const lastX = toX(count - 1);
    const lastY = toY(data[count - 1]!.averagePrice);
    const avgY = toY(avgPrice);
    const area = `${firstX},${PY + innerH} ${pts} ${lastX},${PY + innerH}`;

    // y-axis: 3 ticks — min, mid, max
    const yTicks = [minP, (minP + maxP) / 2, maxP].map((p, i) => ({ p, i, y: toY(p) }));

    // x-axis: up to 6 evenly spaced month labels from data dates
    const xTicks: { label: string; x: number }[] = [];
    if (count >= 2) {
        const step = Math.max(1, Math.floor(count / 6));
        for (let i = 0; i < count; i += step) {
            const d = data[i]!;
            const mo = new Date(d.date).getMonth();
            xTicks.push({ label: MONTH_SHORT[mo] ?? '', x: toX(i) });
        }
        // always include last
        const last = data[count - 1]!;
        const lastLabel = MONTH_SHORT[new Date(last.date).getMonth()] ?? '';
        if (!xTicks.length || xTicks[xTicks.length - 1]!.x < lastX - 20) {
            xTicks.push({ label: lastLabel, x: lastX });
        }
    }

    const startP = data[0]!.averagePrice;
    const deltaPct = startP > 0 ? ((data[count - 1]!.averagePrice - startP) / startP) * 100 : 0;
    const deltaStr = `${deltaPct >= 0 ? '+' : ''}${deltaPct.toFixed(1)}%`;

    return (
        <div className={styles.chartOuter}>
            {/* meta subtext */}
            <div className={styles.chartMeta}>
                <span>{t('components.componentPage.chartRange', { range, count })}</span>
            </div>

            {/* svg chart */}
            <svg width="100%" viewBox={`0 0 ${W} ${H}`} style={{ display: 'block' }}>
                <defs>
                    <linearGradient id="pcAreaGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="#AECF55" stopOpacity="0.18" />
                        <stop offset="100%" stopColor="#AECF55" stopOpacity="0.01" />
                    </linearGradient>
                </defs>

                {/* grid lines + y labels */}
                {yTicks.map(t => (
                    <g key={t.i}>
                        <line x1={PX} y1={t.y} x2={W - 8} y2={t.y} stroke="#1F1F24" strokeWidth="1" />
                        <text x={PX - 6} y={t.y + 4} textAnchor="end" fill="#4A4A52" fontSize="10" fontFamily="'IBM Plex Mono',monospace">
                            {fmtPrice(t.p)}
                        </text>
                    </g>
                ))}

                {/* area fill */}
                <polygon points={area} fill="url(#pcAreaGrad)" />

                {/* avg dashed line + label */}
                <line x1={PX} y1={avgY} x2={W - 8} y2={avgY} stroke="#44444E" strokeWidth="1" strokeDasharray="5 3" />
                <text x={W - 10} y={avgY - 4} textAnchor="end" fill="#6B6B76" fontSize="9" fontFamily="'IBM Plex Mono',monospace">
                    AVG · {fmtPrice(avgPrice)}
                </text>

                {/* price polyline */}
                <polyline points={pts} fill="none" stroke="#AECF55" strokeWidth="1.5" strokeLinejoin="round" strokeLinecap="round" />

                {/* vertical dashed line at last point */}
                <line x1={lastX} y1={lastY} x2={lastX} y2={PY + innerH} stroke="#44444E" strokeWidth="1" strokeDasharray="4 3" />

                {/* current price dot + label */}
                <circle cx={lastX} cy={lastY} r="5" fill="#AECF55" />
                <circle cx={lastX} cy={lastY} r="2.5" fill="#08080A" />
                <text x={lastX - 8} y={lastY - 9} textAnchor="end" fill="#AECF55" fontSize="11" fontWeight="600" fontFamily="'IBM Plex Mono',monospace">
                    {fmtPrice(data[count - 1]!.averagePrice)}
                </text>

                {/* x-axis labels */}
                {xTicks.map((t, i) => (
                    <text key={i} x={t.x} y={H - 4} textAnchor="middle" fill="#4A4A52" fontSize="10" fontFamily="'IBM Plex Mono',monospace">
                        {t.label}
                    </text>
                ))}
            </svg>

            {/* legend row */}
            <div className={styles.chartLegend}>
                <span className={styles.legendItem}>
                    <span className={styles.legendLineSolid} /> {t('components.componentPage.chartLegendPrice')}
                </span>
                <span className={styles.legendItem}>
                    <span className={styles.legendLineDash} /> {t('components.componentPage.chartLegendAvg')}
                </span>
                <span className={styles.legendSep}>·</span>
                <span className={styles.legendStat}>{t('components.componentPage.chartDeltaStart')} · <b className={deltaPct < 0 ? styles.deltaPos : styles.deltaNeg}>{deltaStr}</b></span>
                <span className={styles.legendSep}>·</span>
                <span className={styles.legendStat}>{t('components.componentPage.chartLow')} <b>{fmtPrice(minP)}</b></span>
                <span className={styles.legendSep}>·</span>
                <span className={styles.legendStat}>{t('components.componentPage.chartHigh')} <b>{fmtPrice(maxP)}</b></span>
            </div>
        </div>
    );
}

// ── constants ────────────────────────────────────────────────
const RANGE_DAYS = { '1M': 30, '3M': 90, '6M': 180, '1Y': 0 } as const;

// ── main component ───────────────────────────────────────────

function ComponentPage() {
    const { t, i18n } = useTranslation();
    const { type, id } = useParams<{ type: string; id: string }>();

    const [component, setComponent]         = useState<IComponentBase | null>(null);
    const [loading, setLoading]             = useState(true);
    const [error, setError]                 = useState<string | null>(null);
    const [selectedOffer, setSelectedOffer] = useState<IProductOffer | null>(null);
    const [offerSort, setOfferSort]         = useState<'price' | 'rating'>('price');
    const [offerSortDir, setOfferSortDir]   = useState<'asc' | 'desc'>('asc');
    const [history, setHistory]             = useState<IPriceHistoryPoint[]>([]);
    const [historyRange, setHistoryRange]   = useState<'1M' | '3M' | '6M' | '1Y'>('1M');
    const [similar, setSimilar]             = useState<IComponentBase[]>([]);
    const [compat, setCompat]               = useState<IBuildCompatibilityReport | null>(null);
    const [hasBuild, setHasBuild]           = useState(false);
    const [toast, setToast]                 = useState<{ visible: boolean; text: string }>({ visible: false, text: '' });

    const typeKey = TYPE_KEY_MAP[type?.toLowerCase() ?? ''] ?? type ?? '';
    const meta = TYPE_META[typeKey] ?? { gly: '???', labelKey: type ?? '' };

    // fetch main component data
    useEffect(() => {
        if (!type || !id) return;
        setLoading(true);
        componentService.getById(type as ComponentType, id)
            .then(r => {
                setComponent(r.data);
                const offers = (r.data as IComponentBase & { productOffers?: IProductOffer[] }).productOffers;
                if (offers && offers.length > 0) {
                    const cheapest = offers.reduce((a, b) => a.price < b.price ? a : b);
                    setSelectedOffer(cheapest);
                }
            })
            .catch(() => setError(t('components.componentPage.loadError', { type })))
            .finally(() => setLoading(false));
    }, [type, id, t]);

    // fetch price history
    useEffect(() => {
        if (!type || !id) return;
        componentService.getPriceHistory(type as ComponentType, id, RANGE_DAYS[historyRange])
            .then(r => setHistory(r.data))
            .catch(() => {});
    }, [type, id, historyRange]);

    // fetch similar
    useEffect(() => {
        if (!type || !id) return;
        componentService.getSimilar(type as ComponentType, id)
            .then(r => setSimilar(r.data))
            .catch(() => {});
    }, [type, id]);

    // build compatibility check
    useEffect(() => {
        if (!type || !id) return;
        try {
            const raw = localStorage.getItem('selectedComponents');
            if (!raw) return;
            const selected: SelectedComponents = JSON.parse(raw);
            const keys = Object.keys(selected);
            const hasComponents = keys.some(k => {
                const v = selected[k];
                return Array.isArray(v) ? v.length > 0 : v != null;
            });
            if (!hasComponents) return;
            setHasBuild(true);
            const payload = buildCompatPayload(selected, type, id);
            buildService.checkCompatibility(payload)
                .then(r => setCompat(r.data))
                .catch(() => {});
        } catch { /* ignore parse errors */ }
    }, [type, id]);

    // show toast and auto-dismiss
    const showToast = useCallback((text: string) => {
        setToast({ visible: true, text });
        setTimeout(() => setToast(t => ({ ...t, visible: false })), 3500);
    }, []);

    const handleAddToBuild = useCallback((offer: IProductOffer) => {
        if (!component || !type) return;
        try {
            let selected: SelectedComponents = {};
            const raw = localStorage.getItem('selectedComponents');
            if (raw) selected = JSON.parse(raw);

            const entry: SelectedOffer = {
                componentId: component.id,
                offerId: offer.id,
                price: offer.price,
                storeName: offer.store.name,
                storeLogoUrl: offer.store.logoUrl ?? null,
                productOfferUrl: offer.productOfferUrl,
            };

            const lc = type.toLowerCase();
            if (MULTI_COMPONENT_TYPES.includes(lc)) {
                const key = `${lc}s`;
                if (!Array.isArray(selected[key])) selected[key] = [];
                const arr = selected[key] as SelectedMulti[];
                const idx = arr.findIndex(i => i.componentId === component.id);
                if (idx >= 0) {
                    arr[idx] = { ...entry, quantity: arr[idx]!.quantity + 1 };
                } else {
                    arr.push({ ...entry, quantity: 1 });
                }
            } else {
                const storageKey = TYPE_TO_STORAGE_KEY[lc] ?? lc;
                selected[storageKey] = entry;
            }

            localStorage.setItem('selectedComponents', JSON.stringify(selected));
            showToast(`${component.name} · ₴${offer.price.toLocaleString()} at ${offer.store.name}`);
        } catch (e) {
            console.error('Error adding to build', e);
        }
    }, [component, type, showToast]);

    // ── renders ──────────────────────────────────────────────

    const renderCompatStrip = () => {
        if (!hasBuild || !compat) return null;
        let badge = '';
        let detail = '';
        let cls = '';

        if (compat.isStrictlyCompatible && compat.overallFitnessScore >= 0.8) {
            badge = t('components.componentPage.compatCompatible'); cls = styles.compatible ?? '';
        } else if (compat.isStrictlyCompatible) {
            badge = t('components.componentPage.compatWarning'); cls = styles.warning ?? '';
        } else {
            badge = t('components.componentPage.compatConflict'); cls = styles.conflict ?? '';
        }

        const issues = compat.ruleResults.flatMap(r => r.issues);
        const first = issues.find(i => i.severity === 'Critical') ?? issues.find(i => i.severity === 'Warning');
        if (first) {
            const params = Object.values(first.parameters).join(' · ');
            detail = params || first.code;
        }

        return (
            <div className={styles.buildStrip}>
                <span className={styles.buildStripLabel}>{t('components.componentPage.compatYourBuild')}</span>
                <span className={styles.buildStripDetail}>{detail || t('components.componentPage.compatNoIssues')}</span>
                <span className={`${styles.buildStripBadge} ${cls}`}>{badge}</span>
            </div>
        );
    };

    // Returns { label, value, unit, isBool, boolVal } or null
    type SpecCell = { label: string; value: string; unit: string; isBool: boolean; boolVal?: boolean };

    const resolveSpecCell = (spec: FullSpecConfig, comp: IComponentBase): SpecCell | null => {
        if ('type' in spec) return null; // section header handled separately

        if ('key' in spec) {
            if (spec.isList) {
                const arr = comp[spec.key];
                if (!Array.isArray(arr) || arr.length === 0) return null;
                if ('renderAsSeparateRows' in spec && spec.renderAsSeparateRows) return null; // handled separately
                const formatted = spec.formatList(arr.map((i: Record<string, unknown>) => spec.formatItem(i)));
                return { label: spec.label || spec.key, value: formatted, unit: spec.unit, isBool: false };
            }
            const raw = comp[spec.key];
            if (raw == null || raw === '') return null;
            if (typeof raw === 'boolean') return { label: spec.label || spec.key, value: '', unit: '', isBool: true, boolVal: raw };
            return { label: spec.label || spec.key, value: String(raw), unit: spec.unit, isBool: false };
        }

        if ('keys' in spec) {
            const values: Record<string, unknown> = {};
            let has = false;
            spec.keys.forEach(k => { values[k] = comp[k]; if (comp[k] != null && comp[k] !== '') has = true; });
            if (!has) return null;
            const formatted = spec.format(values);
            if (!formatted) return null;
            return { label: spec.label || (spec.keys[0] ?? ''), value: formatted, unit: spec.unit, isBool: false };
        }

        return null;
    };

    const renderSpecVal = (cell: SpecCell) => {
        if (cell.isBool) {
            return cell.boolVal
                ? <span className={styles.specValBoolYes}>{t('components.componentPage.specBoolYes')}</span>
                : <span className={styles.specValBoolNo}>—</span>;
        }
        return (
            <>
                <strong style={{ fontWeight: 500, color: 'var(--fg-0)' }}>{cell.value}</strong>
                {cell.unit && <span className={styles.specValUnit}>{cell.unit}</span>}
            </>
        );
    };

    const renderSpecs = () => {
        if (!component) return null;
        const configKey = TYPE_KEY_MAP[type?.toLowerCase() ?? ''] ?? type ?? '';
        const allSpecConfigs = getComponentSpecFullConfigs(t, i18n.language);
        const specs: FullSpecConfig[] = allSpecConfigs[configKey] ??
            allSpecConfigs['default'] ?? [];

        // pass 1: count non-null fields to decide layout
        let fieldCount = 0;
        specs.forEach(spec => {
            if ('type' in spec && spec.type === 'sectionHeader') return;
            if ('key' in spec && spec.isList && 'renderAsSeparateRows' in spec && spec.renderAsSeparateRows) {
                const arr = component[spec.key];
                if (Array.isArray(arr)) fieldCount += arr.length;
                return;
            }
            if (resolveSpecCell(spec, component)) fieldCount++;
        });

        // ≤ 8 fields → single pair per row (2-col), more → double pair (4-col)
        const wide = fieldCount > 8;

        // pass 2: build rows
        const rows: React.ReactNode[] = [];
        let pending: { spec: FullSpecConfig; cell: SpecCell } | null = null;
        // section headers are buffered so they emit just before the row that follows them,
        // allowing pending cells to pair across section boundaries in wide mode
        const pendingHeaders: React.ReactNode[] = [];

        const flushHeaders = () => {
            if (pendingHeaders.length) {
                rows.push(...pendingHeaders);
                pendingHeaders.length = 0;
            }
        };

        const flushPair = (right?: { spec: FullSpecConfig; cell: SpecCell }) => {
            if (!pending) return;
            flushHeaders();
            const leftKey = 'key' in pending.spec ? pending.spec.key : 'keys' in pending.spec ? pending.spec.keys.join('-') : String(rows.length);
            const rightKey = right ? ('key' in right.spec ? right.spec.key : 'keys' in right.spec ? right.spec.keys.join('-') : 'r') : 'empty';
            const rowClass = (wide && right) ? styles.specPairRow : styles.specPairRowSingle;
            rows.push(
                <div key={`pair-${leftKey}-${rightKey}`} className={rowClass}>
                    <div className={styles.specKey}>{pending.cell.label}</div>
                    <div className={styles.specVal}>{renderSpecVal(pending.cell)}</div>
                    {wide && right && (
                        <>
                            <div className={styles.specKey}>{right.cell.label}</div>
                            <div className={styles.specVal}>{renderSpecVal(right.cell)}</div>
                        </>
                    )}
                </div>
            );
            pending = null;
        };

        specs.forEach((spec, i) => {
            if ('type' in spec && spec.type === 'sectionHeader') {
                // in wide mode: don't flush — buffer the header so pending can pair across sections
                // in single mode: flush before the header (no cross-section pairing)
                if (!wide) flushPair();
                pendingHeaders.push(<div key={`sh-${i}`} className={styles.specSection}>{spec.label}</div>);
                return;
            }

            const isFullWidth = 'fullWidth' in spec && spec.fullWidth;

            if ('key' in spec && spec.isList && 'renderAsSeparateRows' in spec && spec.renderAsSeparateRows) {
                flushPair(); // always flush before a multi-row list
                flushHeaders();
                const arr = component[spec.key];
                if (Array.isArray(arr) && arr.length > 0) {
                    (arr as Record<string, unknown>[]).forEach((item) => {
                        const cell: SpecCell = {
                            label: spec.getLabelFromItem(item),
                            value: String(spec.getValueFromItem(item) ?? ''),
                            unit: spec.valueUnit ?? '',
                            isBool: false,
                        };
                        rows.push(
                            <div key={`list-${spec.key}-${cell.label}`} className={styles.specPairRowSingle}>
                                <div className={styles.specKey}>{cell.label}</div>
                                <div className={styles.specVal}>{renderSpecVal(cell)}</div>
                            </div>
                        );
                    });
                }
                return;
            }

            const cell = resolveSpecCell(spec, component);
            if (!cell) return;

            if (wide && !isFullWidth) {
                if (pending) {
                    flushPair({ spec, cell });
                } else {
                    pending = { spec, cell };
                }
            } else {
                flushPair(); // flush pending before a full-width field
                flushHeaders();
                rows.push(
                    <div key={`single-${'key' in spec ? spec.key : i}`} className={styles.specPairRowSingle}>
                        <div className={styles.specKey}>{cell.label}</div>
                        <div className={styles.specVal}>{renderSpecVal(cell)}</div>
                    </div>
                );
            }
        });

        flushPair();

        return { rows, fieldCount };
    };

    const specsResult = renderSpecs() ?? { rows: [], fieldCount: 0 };

    // ── offer list processing ─────────────────────────────────

    const productOffers = (component as (IComponentBase & { productOffers?: IProductOffer[] }) | null)?.productOffers ?? [];

    const filteredOffers = productOffers
        .sort((a, b) => {
            let cmp = 0;
            if (offerSort === 'price') {
                cmp = a.price - b.price;
            } else {
                const rA = a.store.likes - a.store.dislikes;
                const rB = b.store.likes - b.store.dislikes;
                cmp = rB - rA;
            }
            return offerSortDir === 'asc' ? cmp : -cmp;
        });

    const avgPrice = component?.averagePrice ?? 0;
    const minOffer = productOffers.length ? Math.min(...productOffers.map(o => o.price)) : 0;
    const maxOffer = productOffers.length ? Math.max(...productOffers.map(o => o.price)) : 0;

    const factoryLink = (component as IComponentBase & { factoryLink?: string } | null)?.factoryLink;


    // ── status / loading ──────────────────────────────────────

    if (loading) return <div className={styles.loading}>{t('components.componentPage.loading')}</div>;
    if (error)   return <div className={styles.errorState}>{error}</div>;
    if (!component) return <div className={styles.errorState}>{t('components.componentPage.notFound')}</div>;

    // ── render ────────────────────────────────────────────────

    return (
        <div className={styles.page}>

            {/* page header */}
            <div className={styles.pageHeader}>
                <div className={styles.headerLeft}>
                    <h1>{component.name}</h1>
                    <div className={styles.subRow}>
                        {component.offersCount != null && <span>{t('components.componentPage.offersCount', { count: component.offersCount })}</span>}
                        {avgPrice ? <span>{t('components.componentPage.avgPrice', { price: Number(avgPrice).toLocaleString() })}</span> : null}
                    </div>
                </div>
                <div className={styles.headerActions}>
                    {factoryLink && (
                        <a href={factoryLink} target="_blank" rel="noopener noreferrer" className={styles.btnGhost}>
                            {t('components.componentPage.manufacturer')}
                        </a>
                    )}
                    {selectedOffer && (
                        <button className={styles.btnPrimary} onClick={() => handleAddToBuild(selectedOffer)}>
                            {t('components.componentPage.addToBuild')}
                        </button>
                    )}
                </div>
            </div>

            {/* hero */}
            <div className={styles.hero}>
                {/* photo */}
                <div className={styles.photoCard}>
                    {component.photoUrl ? (
                        <img src={component.photoUrl} alt={component.name} loading="lazy" />
                    ) : (
                        <ComponentPhotoPlaceholder gly={meta.gly} />
                    )}
                </div>

                {/* pricing panel */}
                <div className={styles.pricingPanel}>
                    <div>
                        <div className={styles.priceLabel}>
                            <FontAwesomeIcon icon={faShoppingBag} style={{ marginRight: 6, fontSize: 10 }} />
                            {selectedOffer ? `${selectedOffer.store.name}` : t('components.componentPage.bestPrice')}
                        </div>
                        <div className={styles.priceHero}>
                            ₴{selectedOffer
                                ? Number(selectedOffer.price).toLocaleString()
                                : (avgPrice ? Number(avgPrice).toLocaleString() : '—')}
                            <span className={styles.priceHeroSub}>{t('components.componentPage.currency')}</span>
                        </div>
                    </div>

                    {productOffers.length > 0 && (
                        <div className={styles.priceStats}>
                            <div className={styles.priceStat}>
                                <span className={styles.priceStatLabel}>{t('components.componentPage.priceMin')}</span>
                                <span className={styles.priceStatVal}>₴{Number(minOffer).toLocaleString()}</span>
                            </div>
                            <div className={styles.priceStat}>
                                <span className={styles.priceStatLabel}>{t('components.componentPage.priceAvg')}</span>
                                <span className={styles.priceStatVal}>₴{Number(avgPrice).toLocaleString()}</span>
                            </div>
                            <div className={styles.priceStat}>
                                <span className={styles.priceStatLabel}>{t('components.componentPage.priceMax')}</span>
                                <span className={styles.priceStatVal}>₴{Number(maxOffer).toLocaleString()}</span>
                            </div>
                            <div className={styles.priceStat}>
                                <span className={styles.priceStatLabel}>{t('components.componentPage.priceOffers')}</span>
                                <span className={styles.priceStatVal}>{component.offersCount ?? productOffers.length}</span>
                            </div>
                        </div>
                    )}

                    {/* spec chips */}
                    {(() => {
                        const c = component as Record<string, unknown>;
                        const ck = (k: string) => t(`components.componentPage.chips.${k}`);
                        const chip = (key: string, lbl: string, val: unknown, unit = '') =>
                            val != null && val !== '' && val !== false
                                ? <span key={key} className={styles.chip}>
                                    <span className={styles.chipLabel}>{lbl}</span>
                                    <b>{String(val)}</b>
                                    {unit && <span className={styles.chipUnit}>{unit}</span>}
                                  </span>
                                : null;
                        const coolerType = (raw: unknown) => {
                            if (raw === 'Air') return ck('coolerTypeAir');
                            if (raw === 'Water') return ck('coolerTypeWater');
                            return raw;
                        };
                        const lk = typeKey.toLowerCase();
                        let chips: (React.ReactNode)[] = [];
                        if (lk === 'cpu') chips = [
                            chip('socket',   ck('socket'),  c['socket']),
                            chip('cores',    ck('cores'),   c['cores'] != null && c['threads'] != null ? `${c['cores']}C/${c['threads']}T` : c['cores']),
                            chip('freq',     ck('boost'),   c['maxFrequency'], 'GHz'),
                            chip('dimm',     ck('mem'),     c['dimmType']),
                            chip('tdp',      ck('tdp'),     c['tdp'], 'W'),
                        ];
                        else if (lk === 'gpu') chips = [
                            chip('mem',      ck('vram'),    c['memory'], 'GB'),
                            chip('memtype',  ck('type'),    c['memoryType']),
                            chip('freq',     ck('boost'),   c['maxFrequency'], 'MHz'),
                            chip('bus',      ck('bus'),     c['memoryBus'], 'bit'),
                            chip('tdp',      ck('tdp'),     c['wattage'], 'W'),
                        ];
                        else if (lk === 'ram') chips = [
                            chip('type',     ck('type'),    c['type']),
                            chip('freq',     ck('speed'),   c['frequency'], 'MHz'),
                            chip('cap',      ck('cap'),     c['capacity'] != null && c['moduleQuantity'] != null ? `${c['moduleQuantity']}×${c['capacity']} GB` : c['capacity'] != null ? `${c['capacity']} GB` : null),
                            chip('xmp',      '',            c['xmp'] ? 'XMP' : null),
                            chip('expo',     '',            c['expo'] ? 'EXPO' : null),
                            chip('ecc',      '',            c['ecc'] ? 'ECC' : null),
                        ];
                        else if (lk === 'motherboard') chips = [
                            chip('socket',   ck('socket'),  c['socket']),
                            chip('chipset',  ck('chipset'), c['chipset']),
                            chip('ff',       ck('form'),    c['formFactor']),
                            chip('dimm',     ck('mem'),     c['dimmType']),
                            chip('dimmfreq', ck('max'),     c['dimmFrequency'], 'MHz'),
                        ];
                        else if (lk === 'ssd') chips = [
                            chip('cap',      ck('cap'),     c['capacity'], 'GB'),
                            chip('iface',    ck('iface'),   c['interface']),
                            chip('ff',       ck('form'),    c['formFactor']),
                            chip('read',     ck('read'),    c['maxReadSpeed'], 'MB/s'),
                            chip('write',    ck('write'),   c['maxWriteSpeed'], 'MB/s'),
                        ];
                        else if (lk === 'hdd') chips = [
                            chip('cap',      ck('cap'),     c['capacity'], 'GB'),
                            chip('iface',    ck('iface'),   c['interface']),
                            chip('rpm',      ck('rpm'),     c['spindleSpeed']),
                            chip('cache',    ck('cache'),   c['cache'], 'MB'),
                        ];
                        else if (lk === 'powersupply') chips = [
                            chip('watt',     ck('power'),   c['wattage'], 'W'),
                            chip('ff',       ck('form'),    c['formFactor']),
                            chip('eff',      ck('eff'),     c['efficiencyStandart']),
                            chip('apcf',     '',            c['hasApcf'] ? 'APFC' : null),
                        ];
                        else if (lk === 'cpucooler') chips = [
                            chip('type',     ck('type'),    coolerType(c['type'])),
                            chip('tdp',      ck('tdp'),     c['maxPowerDissipation'], 'W'),
                            chip('fans',     ck('fans'),    c['fanCount'] != null ? `${c['fanCount']}×${c['fanSize']}mm` : null),
                            chip('noise',    ck('noise'),   c['noiseLevelDb'], 'dB'),
                        ];
                        else if (lk === 'pccase') chips = [
                            chip('std',      ck('class'),   c['sizeStandard']),
                            chip('maxgpu',   ck('gpu'),     c['maxGpuLength'], 'mm'),
                            chip('maxcpu',   ck('cooler'),  c['maxCpuCoolerHeight'], 'mm'),
                            chip('filters',  '',            c['hasDustFilters'] ? ck('dustFilters') : null),
                        ];
                        else if (lk === 'fan') chips = [
                            chip('size',     ck('size'),    c['sizeLength'] != null ? `${c['sizeLength']}mm` : null),
                            chip('speed',    ck('max'),     c['maxSpeed'], 'RPM'),
                            chip('noise',    ck('noise'),   c['noiseLevelDb'], 'dB'),
                            chip('airflow',  ck('flow'),    c['airflowCfm'], 'CFM'),
                        ];
                        const visible = chips.filter(Boolean);
                        if (!visible.length) return null;
                        return <div className={styles.specChips}>{visible}</div>;
                    })()}

                    {/* build compatibility strip */}
                    {renderCompatStrip()}

                </div>
            </div>

            {/* offers */}
            {productOffers.length > 0 && (
                <div className={styles.section}>
                    <div className={styles.sectionHead}>
                        <div>
                            <span className={styles.sectionTitle}>{t('components.componentPage.offersSection')}</span>
                            <span style={{ fontFamily: 'var(--font-mono)', fontSize: 11, color: 'var(--fg-3)', letterSpacing: '0.08em', marginLeft: 10 }}>
                                {t('components.componentPage.storesCount', { filtered: filteredOffers.length, total: productOffers.length, stores: new Set(productOffers.map(o => o.store.name)).size })}
                            </span>
                        </div>
                        <div className={styles.sectionControls}>
                            <span className={styles.ctrlLabel}>{t('components.componentPage.sortLabel')}</span>
                            <div className={styles.sortGroup}>
                                {(['price', 'rating'] as const).map(s => (
                                    <span
                                        key={s}
                                        className={`${styles.sortSeg} ${offerSort === s ? styles.sortSegOn : ''}`}
                                        onClick={() => {
                                            if (offerSort === s) setOfferSortDir(d => d === 'asc' ? 'desc' : 'asc');
                                            else { setOfferSort(s); setOfferSortDir('asc'); }
                                        }}
                                    >
                                        {s === 'price' ? t('components.componentPage.sortPrice') : t('components.componentPage.sortRating')}
                                        {offerSort === s && (
                                            <span className={styles.sortArrow}>{offerSortDir === 'asc' ? '▲' : '▼'}</span>
                                        )}
                                    </span>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div className={styles.offersWrap}>
                        {/* pinned header row */}
                        <div className={styles.offersHead}>
                            <span />
                            <span>{t('components.componentPage.offersHeadStore')}</span>
                            <span>{t('components.componentPage.offersHeadRating')}</span>
                            <span className={styles.r}>{t('components.componentPage.offersHeadPrice')}</span>
                            <span className={styles.r}>{t('components.componentPage.offersHeadDeltaAvg')}</span>
                            <span className={styles.r}>{t('components.componentPage.offersHeadActions')}</span>
                        </div>

                        {/* scrollable body */}
                        <div className={styles.offersBody}>
                            {filteredOffers.map(offer => {
                                const sel = selectedOffer?.id === offer.id;
                                const isBest = offer.price === minOffer;
                                const deltaPct = avgPrice
                                    ? ((offer.price - Number(avgPrice)) / Number(avgPrice)) * 100
                                    : 0;
                                const deltaStr = Math.abs(deltaPct) < 0.5
                                    ? '—'
                                    : `${deltaPct > 0 ? '+' : ''}${deltaPct.toFixed(1)}%`;
                                const deltaClass = deltaPct < -0.5
                                    ? styles.deltaPos
                                    : deltaPct > 0.5
                                        ? styles.deltaNeg
                                        : styles.deltaFlat;

                                const total = offer.store.likes + offer.store.dislikes;
                                const ratingPct = total > 0 ? Math.round((offer.store.likes / total) * 100) : null;

                                return (
                                    <div
                                        key={offer.id}
                                        className={`${styles.offerRow} ${sel ? styles.offerRowSel : ''}`}
                                        onClick={() => setSelectedOffer(offer)}
                                    >
                                        {/* pick dot */}
                                        <div className={styles.pickCell}>
                                            <span className={styles.pickDot} />
                                        </div>

                                        {/* store */}
                                        <div className={styles.storeCell}>
                                            <div className={styles.storeLogoBox}>
                                                {offer.store.logoUrl
                                                    ? <img src={offer.store.logoUrl} alt={offer.store.name} loading="lazy" />
                                                    : offer.store.name.substring(0, 3).toUpperCase()
                                                }
                                            </div>
                                            <div className={styles.storeMeta}>
                                                <div className={styles.storeName}>{offer.store.name}</div>
                                                <div className={styles.storeSub}>
                                                    {total > 0 ? `${total.toLocaleString()} ${t('components.componentPage.reviewsLabel')}` : ''}
                                                </div>
                                            </div>
                                        </div>

                                        {/* rating */}
                                        <div className={styles.ratingCell}>
                                            <span className={styles.ratingUp}>▲{offer.store.likes.toLocaleString()}</span>
                                            <span className={styles.ratingDn}>▼{offer.store.dislikes.toLocaleString()}</span>
                                            {ratingPct !== null && <span className={styles.ratingPct}>{ratingPct}%</span>}
                                        </div>

                                        {/* price */}
                                        <div className={`${styles.priceCell} ${isBest ? styles.priceBest : ''}`}>
                                            ₴{Number(offer.price).toLocaleString()}
                                        </div>

                                        {/* delta */}
                                        <div className={`${styles.deltaCell} ${deltaClass}`}>{deltaStr}</div>

                                        {/* actions */}
                                        <div className={styles.actsCell} onClick={e => e.stopPropagation()}>
                                            <a
                                                href={offer.productOfferUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className={styles.btnBuy}
                                            >
                                                {t('components.componentPage.buyBtn')}
                                            </a>
                                            <button
                                                className={styles.btnAdd}
                                                onClick={() => { setSelectedOffer(offer); handleAddToBuild(offer); }}
                                            >{t('components.componentPage.addBtn')}</button>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>

                        {/* footer */}
                        <div className={styles.offersFoot}>
                            <span>{t('components.componentPage.showingOffers', { count: filteredOffers.length })}</span>
                            <span>{t('components.componentPage.selectRowHint')}</span>
                        </div>
                    </div>
                </div>
            )}

            {/* history + alert */}
            <div className={styles.historyRow}>
                <div className={styles.historyCard}>
                    <div className={styles.cardHead}>
                        <span className={styles.cardTitle}>{t('components.componentPage.priceHistory')}</span>
                        <div className={styles.sortGroup}>
                            {(['1M', '3M', '6M', '1Y'] as const).map(r => (
                                <span
                                    key={r}
                                    className={`${styles.sortSeg} ${historyRange === r ? styles.sortSegOn : ''}`}
                                    onClick={() => setHistoryRange(r)}
                                >{r}</span>
                            ))}
                        </div>
                    </div>
                    <PriceChart data={history} avgPrice={Number(avgPrice)} range={historyRange} t={t} />
                </div>
                <div className={styles.alertCard}>
                    {id && type ? (
                        <PriceAlertButton componentId={id} componentType={type as ComponentType} />
                    ) : null}
                </div>
            </div>

            {/* specs */}
            <div className={styles.section}>
                <div className={styles.sectionHead}>
                    <span className={styles.sectionTitle}>{t('components.componentPage.specifications')}</span>
                    {specsResult.fieldCount > 0 && (
                        <span className={styles.sectionMeta}>{t('components.componentPage.specFields', { count: specsResult.fieldCount })}</span>
                    )}
                </div>
                <div className={styles.specsGrid}>
                    {specsResult.rows}
                </div>
            </div>

            {/* similar */}
            {similar.length > 0 && (
                <div className={styles.section}>
                    <div className={styles.similarHead}>
                        <span className={styles.similarTitle}>{t('components.componentPage.similarParts')}</span>
                        <span className={styles.similarMeta}>{t('components.componentPage.similarMeta', { count: similar.length, label: t(meta.labelKey).toLowerCase() })}</span>
                        <Link to={`/components/${type}`} className={styles.similarBrowse}>
                            {t('components.componentPage.browseAll', { label: t(meta.labelKey).toLowerCase() })}
                        </Link>
                    </div>
                    <div className={styles.similarGrid}>
                        {similar.map(c => {
                            const cp = c as IComponentBase & {
                                socket?: string; cores?: number; threads?: number;
                                maxFrequency?: number; memory?: number; capacity?: number;
                                wattage?: number; frequency?: number;
                            };
                            const specParts: string[] = [];
                            if (cp.socket) specParts.push(cp.socket);
                            if (cp.cores != null) specParts.push(cp.threads ? `${cp.cores}C/${cp.threads}T` : `${cp.cores}C`);
                            const freq = cp.maxFrequency ?? cp.frequency;
                            if (freq) specParts.push(`${freq} GHz`);
                            if (!specParts.length && cp.memory) specParts.push(`${cp.memory} GB`);
                            if (!specParts.length && cp.capacity) specParts.push(`${cp.capacity} GB`);
                            if (!specParts.length && cp.wattage) specParts.push(`${cp.wattage} W`);

                            const simPrice = c.averagePrice ? Number(c.averagePrice) : null;
                            const curPrice = avgPrice ? Number(avgPrice) : null;
                            let deltaEl: React.ReactNode = null;
                            if (simPrice && curPrice && curPrice > 0) {
                                const pct = ((simPrice - curPrice) / curPrice) * 100;
                                const absPct = Math.abs(pct).toFixed(0);
                                if (pct < -1) deltaEl = <span className={styles.similarDeltaDown}>{t('components.componentPage.vsCurrentDown', { pct: absPct })}</span>;
                                else if (pct > 1) deltaEl = <span className={styles.similarDeltaUp}>{t('components.componentPage.vsCurrentUp', { pct: absPct })}</span>;
                                else deltaEl = <span className={styles.similarDeltaFlat}>{t('components.componentPage.vsCurrentFlat')}</span>;
                            }

                            return (
                                <Link key={c.id} to={`/components/${type}/${c.id}`} className={styles.similarCard}>
                                    <div className={styles.similarTop}>
                                        <div className={styles.similarThumb}>
                                            {c.photoUrl ? (
                                                <img src={c.photoUrl} alt={c.name} loading="lazy" />
                                            ) : (
                                                <ComponentPhotoPlaceholder gly={meta.gly} />
                                            )}
                                        </div>
                                        <div className={styles.similarInfo}>
                                            <div className={styles.similarName}>{c.name}</div>
                                            {specParts.length > 0 && (
                                                <div className={styles.similarSpecs}>{specParts.join(' · ')}</div>
                                            )}
                                        </div>
                                    </div>
                                    <div className={styles.similarBottom}>
                                        {simPrice ? (
                                            <div className={styles.similarPrice}>
                                                <span className={styles.similarPriceCcy}>₴</span>
                                                {simPrice.toLocaleString()}
                                            </div>
                                        ) : <div className={styles.similarPrice}>—</div>}
                                        {deltaEl}
                                    </div>
                                </Link>
                            );
                        })}
                    </div>
                </div>
            )}

            {/* toast */}
            <div className={`${styles.toast} ${toast.visible ? styles.toastVisible : ''}`}>
                <div className={styles.toastDot} />
                <span className={styles.toastText}>{t('components.componentPage.toastAdded', { text: toast.text })}</span>
                <Link to="/" className={styles.toastLink}>{t('components.componentPage.viewBuild')}</Link>
            </div>
        </div>
    );
}

export default ComponentPage;
