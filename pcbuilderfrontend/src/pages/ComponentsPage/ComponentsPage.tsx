import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { useNavigate, useParams } from "react-router-dom";
import { componentService, parsePagination } from "../../api/component.service";
import type { ComponentType, IComponentBase, IResourceParameters } from "../../types/component.types";
import type { FilterConfig } from '../../components/FilterPanel/filterConfigs';
import { filterConfigs } from '../../components/FilterPanel/filterConfigs';
import { componentSpecConfigs } from "./componentSpecsConfigs";
import FilterPanel from '../../components/FilterPanel/FilterPanel';
import { Pagination } from '../../components/Pagination/Pagination';
import { Button } from '../../components/Button/Button';
import styles from './ComponentsPage.module.css';

type RangeValue = { min: number; max: number };
type FilterValues = Record<string, string[] | RangeValue>;

function ComponentsPage() {
    const { type } = useParams<{ type: string }>();
    const navigate = useNavigate();
    const [components, setComponents] = useState<IComponentBase[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [filters, setFilters] = useState<FilterValues>({});
    const [searchQuery, setSearchQuery] = useState('');
    const firstLoadDone = useRef(false);

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

    const fetchComponents = useCallback(async (customFilters: FilterValues, page = 1, query: string | null = null) => {
        try {
            setLoading(true);

            const searchToUse = query !== null ? query : searchQuery;

            const params: IResourceParameters = {
                pageNumber: page,
                pageSize,
                orderBy: sortField,
                ascending: sortDirection === 'asc',
                searchQuery: searchToUse.trim(),
                filters: buildApiFilters(customFilters),
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
            setError(`Error loading ${type} components: ${(err as Error).message}`);
        } finally {
            setLoading(false);
            firstLoadDone.current = true;
        }
    }, [type, buildApiFilters, pageSize, sortField, sortDirection, searchQuery]);

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
    }, [type]);

    // Filters / sort change
    useEffect(() => {
        if (!firstLoadDone.current) return;
        setCurrentPage(1);
        fetchComponents(filters, 1, searchQuery);
    }, [filters, sortField, sortDirection]);

    // Page change
    useEffect(() => {
        if (!firstLoadDone.current) return;
        if (currentPage > 1) {
            fetchComponents(filters, currentPage, searchQuery);
        }
    }, [currentPage]);

    // Debounced search
    useEffect(() => {
        if (!firstLoadDone.current) return;
        const timer = setTimeout(() => {
            setCurrentPage(1);
            fetchComponents(filters, 1, searchQuery);
        }, 400);
        return () => clearTimeout(timer);
    }, [searchQuery]);

    const handleFilterChange = useCallback((newFilters: FilterValues) => {
        setFilters(newFilters);
    }, []);

    if (loading && !firstLoadDone.current) return <div>Loading {type} components...</div>;
    if (error) return <div>{error}</div>;

    return (
        <section className={styles['component-list']}>
            <div className={styles['top-bar-content']}>
                <div className={styles['search-container']}>
                    <input
                        type="text"
                        placeholder="Пошук..."
                        value={searchQuery}
                        onChange={e => setSearchQuery(e.target.value)}
                        className={styles['search-input']}
                    />
                </div>
            </div>
            <div className={styles['content-container']}>
                <FilterPanel
                    config={filterConfig}
                    onFilterChange={handleFilterChange}
                />

                <div className={styles['table-container']}>
                    <div className={styles['sorting-controls']}>
                        <span className={styles['sorting-label']}>Сортувати за:</span>
                        <div className={styles['sort-buttons']}>
                            {[
                                { field: 'name', label: 'Назвою' },
                                { field: 'averagePrice', label: 'Ціною' },
                                { field: 'offersCount', label: 'К-стю пропозицій' },
                            ].map(({ field, label }) => (
                                <button
                                    key={field}
                                    className={`${styles['sort-button']} ${sortField === field ? styles['active'] : ''}`}
                                    onClick={() => handleSortChange(field)}
                                >
                                    {label}
                                    {sortField === field && (
                                        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor"
                                             viewBox="0 0 16 16">
                                            <path d={sortDirection === 'asc'
                                                ? "M8 15a.5.5 0 0 0 .5-.5V2.707l3.146 3.147a.5.5 0 0 0 .708-.708l-4-4a.5.5 0 0 0-.708 0l-4 4a.5.5 0 1 0 .708.708L7.5 2.707V14.5a.5.5 0 0 0 .5.5"
                                                : "M8 1a.5.5 0 0 1 .5.5v11.793l3.146-3.147a.5.5 0 0 1 .708.708l-4 4a.5.5 0 0 1-.708 0l-4-4a.5.5 0 0 1 .708-.708L7.5 13.293V1.5A.5.5 0 0 1 8 1"}/>
                                        </svg>
                                    )}
                                </button>
                            ))}
                        </div>
                    </div>

                    <div className={styles['components-list']}>
                        {components.map(component => (
                            <div key={component.id} className={styles['component-card']}>
                                <div
                                    className={styles['component-image']}
                                    onClick={() => navigate(`/components/${type}/${component.id}`)}
                                >
                                    {component.photoUrl ? (
                                        <img
                                            src={component.photoUrl}
                                            alt={component.name}
                                            className={styles['componentImage']}
                                        />
                                    ) : (
                                        <div className={styles['no-image']}>No image</div>
                                    )}
                                </div>

                                <div className={styles['component-info']}>
                                    <div className={styles['component-title']}
                                         onClick={() => navigate(`/components/${type}/${component.id}`)}>
                                        {component.name}
                                    </div>
                                    <div className={styles['component-specs']}>
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
                                                                <span key={spec.key} className={styles['spec-item']}>
                                                                    {index > 0 && <span className={styles['spec-separator']}>&bull;</span>}
                                                                    {spec.label && <span className={styles['spec-label']}>{spec.label}:</span>}
                                                                    <span className={styles['spec-value']}>
                                                                        {formattedItems}{spec.unit && ` ${spec.unit}`}
                                                                    </span>
                                                                </span>
                                                            );
                                                        }
                                                        return null;
                                                    }

                                                    if (value == null || value === '') return null;

                                                    return (
                                                        <span key={spec.key} className={styles['spec-item']}>
                                                            {index > 0 && <span className={styles['spec-separator']}>&bull;</span>}
                                                            {spec.label && <span className={styles['spec-label']}>{spec.label}:</span>}
                                                            <span className={styles['spec-value']}>
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
                                                        <span key={spec.keys.join('_')} className={styles['spec-item']}>
                                                            {index > 0 && <span className={styles['spec-separator']}>&bull;</span>}
                                                            {spec.label && <span className={styles['spec-label']}>{spec.label}:</span>}
                                                            <span className={styles['spec-value']}>
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

                                <div className={styles['component-price']}>
                                    {component.averagePrice} грн
                                </div>

                                <div className={styles['component-actions']}>
                                    <Button
                                        variant='primary'
                                        size='md'
                                        onClick={() => navigate(`/components/${type}/${component.id}`)}
                                    >
                                        Деталі
                                    </Button>
                                    <p>Пропозицій: {component.offersCount}</p>
                                </div>
                            </div>
                        ))}
                    </div>

                    {components.length === 0 && !loading && (
                        <div className={styles['no-results']}>
                            No components match your filters. Try adjusting your criteria.
                        </div>
                    )}
                </div>
            </div>
            <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalResults={totalResults}
                pageSize={pageSize}
                onPageChange={(newPage) => setCurrentPage(newPage)}
            />
        </section>
    );
}

export default ComponentsPage;
