import { useState, useEffect, useRef } from 'react';
import { componentService } from "../../api/component.service";
import type { ComponentType } from "../../types/component.types";
import styles from './FilterPanel.module.css';
import { Button } from '../Button/Button';
import type { Filter, FilterConfig } from './filterConfigs';

type RangeValue = { min: number; max: number };
type FilterValues = Record<string, string[] | RangeValue>;

interface FilterPanelProps {
    config: FilterConfig;
    onFilterChange: (values: FilterValues) => void;
}

const parseRangeValues = (values: string[] | undefined): [number, number] | null => {
    if (!values || values.length < 2) return null;
    const first = values[0];
    const second = values[1];
    if (first === undefined || second === undefined) return null;
    return [parseFloat(first.replace(',', '.')), parseFloat(second.replace(',', '.'))];
};

const FilterPanel = ({ config, onFilterChange }: FilterPanelProps) => {
    const [filterValues, setFilterValues] = useState<FilterValues>({});
    const [dynamicOptions, setDynamicOptions] = useState<Record<string, string[] | undefined>>({});
    const [isLoading, setIsLoading] = useState(false);
    const isInitialMount = useRef(true);
    const previousConfigType = useRef<string | null>(null);
    const prevFiltersRef = useRef<string | null>(null);
    const [expandedFilters, setExpandedFilters] = useState<Record<string, boolean>>({});

    useEffect(() => {
        const fetchFilterOptions = async () => {
            try {
                setIsLoading(true);
                const componentType = config.componentType;
                if (!componentType) return;

                const response = await componentService.getFilterOptions(componentType as ComponentType);
                setDynamicOptions(response.data);
            } catch (error) {
                console.error('Error fetching filter options:', error);
            } finally {
                setIsLoading(false);
            }
        };

        if (config.componentType && config.componentType !== previousConfigType.current) {
            previousConfigType.current = config.componentType;
            fetchFilterOptions();
        }
    }, [config.componentType]);

    useEffect(() => {
        if (isLoading) return;
        if (Object.keys(dynamicOptions).length === 0) return;

        if (config.componentType !== previousConfigType.current) {
            isInitialMount.current = true;
            previousConfigType.current = config.componentType;
        }

        const initialValues: FilterValues = {};
        config.filters.forEach(filter => {
            if (filter.type === 'checkbox') {
                initialValues[filter.id] = [];
            } else if (filter.type === 'range') {
                let min = filter.min;
                let max = filter.max;

                const parsed = filter.dynamic && filter.property
                    ? parseRangeValues(dynamicOptions[filter.property])
                    : null;
                if (parsed !== null) [min, max] = parsed;

                initialValues[filter.id] = { min, max };
            }
        });

        setFilterValues(initialValues);
        isInitialMount.current = false;
    }, [dynamicOptions, isLoading, config]);

    useEffect(() => {
        if (Object.keys(filterValues).length === 0) return;

        if (isInitialMount.current) {
            isInitialMount.current = false;
            return;
        }

        const filtersStr = JSON.stringify(filterValues);
        if (prevFiltersRef.current === filtersStr) return;

        prevFiltersRef.current = filtersStr;
        onFilterChange(filterValues);
    }, [filterValues, onFilterChange]);

    const toggleFilterExpand = (filterId: string) => {
        setExpandedFilters(prev => ({
            ...prev,
            [filterId]: !prev[filterId]
        }));
    };

    const handleCheckboxChange = (filterId: string, option: string, isChecked: boolean) => {
        setFilterValues(prev => {
            const updated = { ...prev };
            const current = (prev[filterId] as string[]) || [];
            if (isChecked) {
                updated[filterId] = [...current, option];
            } else {
                updated[filterId] = current.filter(item => item !== option);
            }
            return updated;
        });
    };

    const handleRangeChange = (filterId: string, minOrMax: 'min' | 'max', value: string) => {
        setFilterValues(prev => ({
            ...prev,
            [filterId]: {
                ...(prev[filterId] as RangeValue),
                [minOrMax]: parseFloat(value)
            }
        }));
    };

    const renderFilter = (filter: Filter) => {
        switch (filter.type) {
            case 'checkbox': {
                const options = filter.dynamic && filter.property && dynamicOptions[filter.property]
                    ? dynamicOptions[filter.property]
                    : filter.options;

                const showMoreButton = (options ?? []).length > 6;
                const isExpanded = expandedFilters[filter.id] || false;
                const visibleOptions = showMoreButton && !isExpanded ? (options ?? []).slice(0, 6) : (options ?? []);

                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <div className={styles.checkboxGroup}>
                            {visibleOptions.map(option => (
                                <label key={option} className={styles.checkboxLabel}>
                                    <input
                                        type="checkbox"
                                        checked={(filterValues[filter.id] as string[])?.includes(option) || false}
                                        onChange={(e) => handleCheckboxChange(filter.id, option, e.target.checked)}
                                    />
                                    {filter.formatOptionLabel ? filter.formatOptionLabel(option) : filter.displayLabels?.[option] || option}
                                </label>
                            ))}

                            {showMoreButton && (
                                <button
                                    className={styles.showMoreButton}
                                    onClick={() => toggleFilterExpand(filter.id)}
                                >
                                    {isExpanded ? 'Приховати ▲' : `Показати ще (${(options ?? []).length - 6}) ▼`}
                                </button>
                            )}
                        </div>
                    </div>
                );
            }

            case 'range': {
                let minValue = filter.min;
                let maxValue = filter.max;
                const step = filter.step || 0.1;

                const parsedRange = filter.dynamic && filter.property
                    ? parseRangeValues(dynamicOptions[filter.property])
                    : null;
                if (parsedRange !== null) [minValue, maxValue] = parsedRange;

                const rangeVal = filterValues[filter.id] as RangeValue | undefined;

                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <div className={styles.rangeGroup}>
                            <label>
                                Min:
                                <input
                                    type="number"
                                    min={minValue}
                                    max={maxValue}
                                    step={step}
                                    value={rangeVal?.min ?? minValue}
                                    onChange={(e) => handleRangeChange(filter.id, 'min', e.target.value)}
                                    className={styles.rangeInput}
                                />
                            </label>
                            <label>
                                Max:
                                <input
                                    type="number"
                                    min={minValue}
                                    max={maxValue}
                                    step={step}
                                    value={rangeVal?.max ?? maxValue}
                                    onChange={(e) => handleRangeChange(filter.id, 'max', e.target.value)}
                                    className={styles.rangeInput}
                                />
                            </label>
                        </div>
                    </div>
                );
            }

            default:
                return null;
        }
    };

    const handleClearFilters = () => {
        const initialValues: FilterValues = {};
        config.filters.forEach(filter => {
            if (filter.type === 'checkbox') {
                initialValues[filter.id] = [];
            } else if (filter.type === 'range') {
                let min = filter.min;
                let max = filter.max;

                const parsedClear = filter.dynamic && filter.property
                    ? parseRangeValues(dynamicOptions[filter.property])
                    : null;
                if (parsedClear !== null) [min, max] = parsedClear;

                initialValues[filter.id] = { min, max };
            }
        });
        setFilterValues(initialValues);
        onFilterChange(initialValues);
    };

    return (
        <div className={styles.filterPanel}>
            <div className={styles.filterHeader}>
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor"
                     className="bi bi-funnel" viewBox="0 0 16 16" stroke="currentColor" strokeWidth="0.7">
                    <path
                        d="M1.5 1.5A.5.5 0 0 1 2 1h12a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-.128.334L10 8.692V13.5a.5.5 0 0 1-.342.474l-3 1A.5.5 0 0 1 6 14.5V8.692L1.628 3.834A.5.5 0 0 1 1.5 3.5zm1 .5v1.308l4.372 4.858A.5.5 0 0 1 7 8.5v5.306l2-.666V8.5a.5.5 0 0 1 .128-.334L13.5 3.308V2z"/>
                </svg>
                <h3>Фільтри</h3>
            </div>
            <div className={styles.filtersContainer}>
                {isLoading ? (
                    <p>Loading filter options...</p>
                ) : (
                    config.filters.map(filter => renderFilter(filter))
                )}
            </div>
            <Button
                variant='primary'
                size='md'
                onClick={handleClearFilters}
                className={styles.clearButton}
            >
                Очистити фільтри
            </Button>
        </div>
    );
};

export default FilterPanel;
