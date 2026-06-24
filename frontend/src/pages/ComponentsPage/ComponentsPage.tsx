import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from 'react-i18next';
import { componentService, parsePagination } from "../../api/component.service";
import type { ComponentType, ComponentListMap, IPartialBuildIds, IResourceParameters } from "../../types/component.types";
import type { FilterConfig } from '../../components/FilterPanel/filterConfigs';
import { filterConfigs } from '../../components/FilterPanel/filterConfigs';
import { componentSpecConfigs } from "./componentSpecsConfigs";
import FilterPanel from '../../components/FilterPanel/FilterPanel';
import { Pagination } from '../../components/Pagination/Pagination';
import styles from './ComponentsPage.module.css';

type RangeValue = { min: number; max: number };
type FilterValues = Record<string, string[] | RangeValue>;

const PREFILTER_KEY = 'compat_prefilter_enabled';

function readPartialBuildIds(): IPartialBuildIds {
    try {
        const raw = localStorage.getItem('selectedComponents');
        if (!raw) return {};
        const sel = JSON.parse(raw) as {
            cpu?: { componentId?: string } | null;
            gpu?: { componentId?: string } | null;
            motherboard?: { componentId?: string } | null;
            powerSupply?: { componentId?: string } | null;
            cpuCooler?: { componentId?: string } | null;
            pcCase?: { componentId?: string } | null;
            rams?: { componentId?: string }[];
        };
        const ids: IPartialBuildIds = {};
        if (sel.cpu?.componentId)         ids.cpuId         = sel.cpu.componentId;
        if (sel.gpu?.componentId)         ids.gpuId         = sel.gpu.componentId;
        if (sel.motherboard?.componentId) ids.motherboardId = sel.motherboard.componentId;
        if (sel.powerSupply?.componentId) ids.powerSupplyId = sel.powerSupply.componentId;
        if (sel.cpuCooler?.componentId)   ids.cpuCoolerId   = sel.cpuCooler.componentId;
        if (sel.pcCase?.componentId)      ids.pcCaseId      = sel.pcCase.componentId;
        if (sel.rams?.[0]?.componentId)   ids.ramId         = sel.rams[0].componentId;
        return ids;
    } catch {
        return {};
    }
}

function ComponentsPage() {
    const { type } = useParams<{ type: string }>();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const [components, setComponents] = useState<ComponentListMap[ComponentType][]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [filters, setFilters] = useState<FilterValues>({});
    const [searchQuery, setSearchQuery] = useState('');
    const firstLoadDone = useRef(false);
    const skipNextPageEffect = useRef(false);
    const currentPageRef = useRef(1);
    const [prefilterEnabled, setPrefilterEnabled] = useState<boolean>(() => {
        const stored = localStorage.getItem(PREFILTER_KEY);
        return stored === null ? true : stored === 'true';
    });

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalResults, setTotalResults] = useState(1);
    const pageSize = 20;
    const [sortField, setSortField] = useState('offersCount');
    const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');

    const filterConfig = useMemo<FilterConfig>(() => ({
        ...filterConfigs[type!] || {
            title: `${type!.toUpperCase()} Filters`,
            filters: []
        },
        componentType: type!
    }), [type]);

    const buildApiFilters = useCallback((rawFilters: FilterValues): Record<string, string[]> => {
        const apiFilters: Record<string, string[]> = {};

        for (const [filterId, value] of Object.entries(rawFilters)) {
            const filterDef = filterConfig.filters.find(f => f.id === filterId);
            if (!filterDef) continue;

            if (filterDef.type === 'checkbox') {
                const arr = value as string[];
                if (arr.length > 0) {
                    const existing = apiFilters[filterDef.property] ?? [];
                    apiFilters[filterDef.property] = [...new Set([...existing, ...arr])];
                }
            } else if (filterDef.type === 'range') {
                const range = value as RangeValue;
                apiFilters[filterDef.property] = [
                    String(range.min).replace('.', ','),
                    String(range.max).replace('.', ',')
                ];
            }
        }

        return apiFilters;
    }, [filterConfig.filters]);

    const fetchComponents = useCallback(async (customFilters: FilterValues, page = 1, query: string | null = null, prefilter = prefilterEnabled) => {
        try {
            setLoading(true);

            const searchToUse = query !== null ? query : searchQuery;
            const ids = readPartialBuildIds();
            const hasPartialBuild = Object.keys(ids).length > 0;

            const params: IResourceParameters = {
                pageNumber: page,
                pageSize,
                orderBy: sortField,
                ascending: sortDirection === 'asc',
                searchQuery: searchToUse.trim(),
                filters: buildApiFilters(customFilters),
                ...(prefilter && hasPartialBuild ? { prefilter: true, partialBuild: ids } : {}),
            };

            const response = await componentService.getAll(type as ComponentType, params);
            const pagination = parsePagination(response.headers as Record<string, string>);

            if (pagination) {
                setComponents(response.data);
                setTotalPages(pagination.totalPages);
                setCurrentPage(pagination.pageNumber);
                setTotalResults(pagination.totalCount);
            } else {
                setComponents(response.data);
                setTotalPages(Math.max(1, Math.ceil(response.data.length / pageSize)));
            }

            setError(null);
        } catch (err) {
            setError(t('componentsPage.errorLoading', { type, message: (err as Error).message }));
        } finally {
            setLoading(false);
            firstLoadDone.current = true;
        }
    }, [type, buildApiFilters, pageSize, sortField, sortDirection, searchQuery, prefilterEnabled, t]);

    const handleSortChange = (field: string) => {
        if (field === sortField) {
            setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDirection('asc');
        }
    };

    // Initial load
    useEffect(() => {
        firstLoadDone.current = false;
        setFilters({});
        setSearchQuery('');
        setCurrentPage(1);
        fetchComponents({}, 1, '');
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [type]);

    // Keep currentPageRef in sync
    useEffect(() => { currentPageRef.current = currentPage; }, [currentPage]);

    // Filters / sort change
    useEffect(() => {
        if (!firstLoadDone.current) return;
        skipNextPageEffect.current = currentPageRef.current !== 1;
        setCurrentPage(1);
        fetchComponents(filters, 1, searchQuery);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [filters, sortField, sortDirection]);

    // Page change
    useEffect(() => {
        if (!firstLoadDone.current) return;
        if (skipNextPageEffect.current) {
            skipNextPageEffect.current = false;
            return;
        }
        fetchComponents(filters, currentPage, searchQuery);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentPage]);

    // Debounced search
    useEffect(() => {
        if (!firstLoadDone.current) return;
        const timer = setTimeout(() => {
            skipNextPageEffect.current = currentPageRef.current !== 1;
            setCurrentPage(1);
            fetchComponents(filters, 1, searchQuery);
        }, 400);
        return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [searchQuery]);

    const handleFilterChange = useCallback((newFilters: FilterValues) => {
        setFilters(newFilters);
    }, []);

    const handlePrefilterChange = useCallback((enabled: boolean) => {
        setPrefilterEnabled(enabled);
        localStorage.setItem(PREFILTER_KEY, String(enabled));
        skipNextPageEffect.current = currentPageRef.current !== 1;
        setCurrentPage(1);
        fetchComponents(filters, 1, searchQuery, enabled);
    }, [filters, searchQuery, fetchComponents]);

    const sortOptions = [
        { field: 'name', label: t('componentsPage.sortFields.name') },
        { field: 'averagePrice', label: t('componentsPage.sortFields.averagePrice') },
        { field: 'offersCount', label: t('componentsPage.sortFields.offersCount') },
    ];

    const typeKey = type ?? '';
    const typeLabel = t(`componentTypes.${typeKey}`, { defaultValue: typeKey.toUpperCase() });

    if (loading && !firstLoadDone.current) {
        return <div className={styles.pageLoading}>{t('componentsPage.loading', { type: typeLabel.toUpperCase() })}</div>;
    }
    if (error) return <div className={styles.pageError}>{error}</div>;

    return (
        <section className={styles.page}>
            <div className={styles.titleRow}>
                <div>
                    <h1 className={styles.title}>{typeLabel}</h1>
                    <div className={styles.meta}>
                        <span>{t('componentsPage.catalog')} · {t('componentsPage.results', { count: totalResults })}</span>
                    </div>
                </div>
                <div className={styles.metaRight}>
                    <span>{t('componentsPage.type')} · {typeKey.toUpperCase()}</span>
                    <span className={styles.metaSep}>·</span>
                    <span>{t('componentsPage.page', { current: currentPage, total: totalPages })}</span>
                </div>
            </div>

            <div className={styles.grid}>
                <FilterPanel
                    config={filterConfig}
                    onFilterChange={handleFilterChange}
                    showPrefilterToggle={true}
                    onPrefilterChange={handlePrefilterChange}
                />

                <div className={styles.results}>
                    <div className={styles.toolbar}>
                        <div className={styles.searchBar}>
                            <span className={styles.searchIcon}>⌕</span>
                            <input
                                type="text"
                                placeholder={t('componentsPage.searchPlaceholder', { type: typeLabel.toLowerCase() })}
                                value={searchQuery}
                                onChange={e => setSearchQuery(e.target.value)}
                                className={styles.searchInput}
                            />
                        </div>
                        <span className={styles.toolbarLabel}>{t('componentsPage.sort')}</span>
                        <div className={styles.sortGroup}>
                            {sortOptions.map(({ field, label }) => {
                                const isActive = sortField === field;
                                return (
                                    <span
                                        key={field}
                                        className={`${styles.sortSeg} ${isActive ? styles.sortSegOn : ''}`}
                                        onClick={() => handleSortChange(field)}
                                    >
                                        {label}
                                        {isActive && (
                                            <span className={styles.sortArrow}>
                                                {sortDirection === 'asc' ? '▲' : '▼'}
                                            </span>
                                        )}
                                    </span>
                                );
                            })}
                        </div>
                    </div>

                    {components.length === 0 && !loading ? (
                        <div className={styles.empty}>
                            {t('componentsPage.empty')}
                        </div>
                    ) : (
                        <div className={styles.list}>
                            {components.map(component => (
                                <div
                                    key={component.id}
                                    className={styles.item}
                                    onClick={() => navigate(`/components/${type}/${component.id}`)}
                                >
                                    <div className={styles.itemImg}>
                                        {component.photoUrl ? (
                                            <img
                                                src={component.photoUrl}
                                                alt={component.name}
                                                loading="lazy"
                                            />
                                        ) : (
                                            <span className={styles.itemImgPh}>—</span>
                                        )}
                                    </div>

                                    <div className={styles.itemInfo}>
                                        <div className={styles.itemName}>{component.name}</div>
                                        <div className={styles.itemSpec}>
                                            {(componentSpecConfigs[type ?? ''] ?? componentSpecConfigs['default'] ?? [])
                                                .map((spec, index) => {
                                                    const comp = component;

                                                    if ('key' in spec) {
                                                        const value = comp[spec.key];

                                                        if (spec.isList) {
                                                            if (Array.isArray(value) && value.length > 0) {
                                                                const formattedItems = spec.formatList(
                                                                    value.map((item: Record<string, unknown>) => spec.formatItem(item))
                                                                );
                                                                return (
                                                                    <span key={spec.key} className={styles.specItem}>
                                                                        {index > 0 && <span className={styles.specDot}>/</span>}
                                                                        <span>
                                                                            {spec.labelKey && <span className={styles.specLabel}>{t(spec.labelKey)}: </span>}
                                                                            {formattedItems}{spec.unit && ` ${spec.unit}`}
                                                                        </span>
                                                                    </span>
                                                                );
                                                            }
                                                            return null;
                                                        }

                                                        if (value == null || value === '') return null;

                                                        return (
                                                            <span key={spec.key} className={styles.specItem}>
                                                                {index > 0 && <span className={styles.specDot}>/</span>}
                                                                <span>
                                                                    {spec.labelKey && <span className={styles.specLabel}>{t(spec.labelKey)}: </span>}
                                                                    {String(value)}{spec.unit && ` ${spec.unit}`}
                                                                </span>
                                                            </span>
                                                        );
                                                    }

                                                    if ('keys' in spec) {
                                                        const values: Record<string, unknown> = {};
                                                        let hasValue = false;
                                                        spec.keys.forEach((key) => {
                                                            values[key] = comp[key];
                                                            if (comp[key] != null && comp[key] !== '') hasValue = true;
                                                        });
                                                        if (!hasValue) return null;

                                                        return (
                                                            <span key={spec.keys.join('_')} className={styles.specItem}>
                                                                {index > 0 && <span className={styles.specDot}>/</span>}
                                                                <span>
                                                                    {spec.labelKey && <span className={styles.specLabel}>{t(spec.labelKey)}: </span>}
                                                                    {spec.format(values)}{spec.unit && ` ${spec.unit}`}
                                                                </span>
                                                            </span>
                                                        );
                                                    }
                                                    return null;
                                                })
                                                .filter(Boolean)
                                            }
                                        </div>
                                    </div>

                                    <div className={styles.itemPrice}>
                                        <span className={styles.priceCcy}>₴</span>{component.averagePrice}
                                        <div className={styles.offersLine}>{t('componentsPage.offers', { count: component.offersCount })}</div>
                                    </div>

                                    <div
                                        className={styles.itemAct}
                                        onClick={(e) => { e.stopPropagation(); navigate(`/components/${type}/${component.id}`); }}
                                    >
                                        {t('componentsPage.details')}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        totalResults={totalResults}
                        pageSize={pageSize}
                        onPageChange={(newPage) => setCurrentPage(newPage)}
                    />
                </div>
            </div>
        </section>
    );
}

export default ComponentsPage;
