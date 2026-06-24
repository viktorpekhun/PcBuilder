import { useState, useEffect, useRef, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { componentService } from "../../api/component.service";
import type { ComponentType } from "../../types/component.types";
import styles from './FilterPanel.module.css';
import type { Filter, FilterConfig } from './filterConfigs';

type RangeValue = { min: number; max: number };
type FilterValues = Record<string, string[] | RangeValue>;

interface FilterPanelProps {
    config: FilterConfig;
    onFilterChange: (values: FilterValues) => void;
    onPrefilterChange?: (enabled: boolean) => void;
    showPrefilterToggle?: boolean;
}

const parseRangeValues = (values: string[] | undefined): [number, number] | null => {
    if (!values || values.length < 2) return null;
    const first = values[0];
    const second = values[1];
    if (first === undefined || second === undefined) return null;
    return [parseFloat(first.replace(',', '.')), parseFloat(second.replace(',', '.'))];
};

const PREFILTER_KEY = 'compat_prefilter_enabled';

interface DoubleRangeSliderProps {
    min: number;
    max: number;
    step: number;
    value: RangeValue;
    onChange: (value: RangeValue) => void;
}

const DoubleRangeSlider = ({ min, max, step, value, onChange }: DoubleRangeSliderProps) => {
    const safeMin = Number.isFinite(min) ? min : 0;
    const safeMax = Number.isFinite(max) && max > safeMin ? max : safeMin + 1;
    const range = safeMax - safeMin;

    const curMin = Math.min(Math.max(value.min, safeMin), safeMax);
    const curMax = Math.min(Math.max(value.max, safeMin), safeMax);

    const leftPct = range === 0 ? 0 : ((curMin - safeMin) / range) * 100;
    const rightPct = range === 0 ? 100 : ((curMax - safeMin) / range) * 100;

    const handleMin = (raw: number) => {
        const newMin = Math.min(raw, curMax);
        onChange({ min: newMin, max: curMax });
    };
    const handleMax = (raw: number) => {
        const newMax = Math.max(raw, curMin);
        onChange({ min: curMin, max: newMax });
    };

    const fmt = (n: number) => {
        if (Number.isInteger(n)) return String(n);
        return n.toFixed(2).replace(/\.?0+$/, '');
    };

    return (
        <div className={styles.slider}>
            <div className={styles.sliderTrackWrap}>
                <div className={styles.sliderTrack} />
                <div
                    className={styles.sliderRange}
                    style={{ left: `${leftPct}%`, right: `${100 - rightPct}%` }}
                />
                <input
                    type="range"
                    min={safeMin}
                    max={safeMax}
                    step={step}
                    value={curMin}
                    onChange={(e) => handleMin(parseFloat(e.target.value))}
                    className={`${styles.sliderInput} ${styles.sliderInputMin}`}
                />
                <input
                    type="range"
                    min={safeMin}
                    max={safeMax}
                    step={step}
                    value={curMax}
                    onChange={(e) => handleMax(parseFloat(e.target.value))}
                    className={`${styles.sliderInput} ${styles.sliderInputMax}`}
                />
            </div>
            <div className={styles.sliderValues}>
                <span className={styles.sliderVal}>{fmt(curMin)}</span>
                <span className={styles.sliderDash}>—</span>
                <span className={styles.sliderVal}>{fmt(curMax)}</span>
            </div>
        </div>
    );
};

const FilterPanel = ({ config, onFilterChange, onPrefilterChange, showPrefilterToggle = false }: FilterPanelProps) => {
    const { t } = useTranslation();
    const [filterValues, setFilterValues] = useState<FilterValues>({});
    const [dynamicOptions, setDynamicOptions] = useState<Record<string, string[] | undefined>>({});
    const [isLoading, setIsLoading] = useState(false);
    const isInitialMount = useRef(true);
    const previousConfigType = useRef<string | null>(null);
    const prevFiltersRef = useRef<string>('{}');
    const initialRangeValuesRef = useRef<Record<string, RangeValue>>({});
    const [expandedFilters, setExpandedFilters] = useState<Record<string, boolean>>({});
    const [prefilterEnabled, setPrefilterEnabled] = useState<boolean>(() => {
        const stored = localStorage.getItem(PREFILTER_KEY);
        return stored === null ? true : stored === 'true';
    });

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
        const newInitialRangeValues: Record<string, RangeValue> = {};

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
                newInitialRangeValues[filter.id] = { min, max };
            }
        });

        initialRangeValuesRef.current = newInitialRangeValues;
        setFilterValues(initialValues);
    }, [dynamicOptions, isLoading, config]);

    useEffect(() => {
        if (Object.keys(filterValues).length === 0) return;

        if (isInitialMount.current) {
            isInitialMount.current = false;
            return;
        }

        const activeFilters: FilterValues = {};
        for (const [filterId, value] of Object.entries(filterValues)) {
            const filterDef = config.filters.find(f => f.id === filterId);
            if (!filterDef) continue;
            if (filterDef.type === 'checkbox') {
                if ((value as string[]).length > 0) activeFilters[filterId] = value;
            } else if (filterDef.type === 'range') {
                const initial = initialRangeValuesRef.current[filterId];
                const range = value as RangeValue;
                if (!initial || range.min !== initial.min || range.max !== initial.max) {
                    activeFilters[filterId] = value;
                }
            }
        }

        const filtersStr = JSON.stringify(activeFilters);
        if (prevFiltersRef.current === filtersStr) return;

        prevFiltersRef.current = filtersStr;
        onFilterChange(activeFilters);
    }, [filterValues, onFilterChange, config.filters]);

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

    const handleRangeFieldChange = (filterId: string, minOrMax: 'min' | 'max', value: string) => {
        setFilterValues(prev => ({
            ...prev,
            [filterId]: {
                ...(prev[filterId] as RangeValue),
                [minOrMax]: parseFloat(value)
            }
        }));
    };

    const handleSliderChange = useCallback((filterId: string, newRange: RangeValue) => {
        setFilterValues(prev => ({
            ...prev,
            [filterId]: newRange
        }));
    }, []);

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
                    <div key={filter.id} className={styles.section}>
                        <div className={styles.sectionLabel}>{t(filter.label)}</div>
                        <div className={styles.checks}>
                            {visibleOptions.map(option => {
                                const isOn = (filterValues[filter.id] as string[])?.includes(option) || false;
                                return (
                                    <label key={option} className={styles.check}>
                                        <input
                                            type="checkbox"
                                            checked={isOn}
                                            onChange={(e) => handleCheckboxChange(filter.id, option, e.target.checked)}
                                            className={styles.checkInput}
                                        />
                                        <span className={`${styles.checkBox} ${isOn ? styles.checkBoxOn : ''}`} />
                                        <span className={styles.checkLabel}>
                                            {filter.formatOptionLabel ? filter.formatOptionLabel(option) : filter.displayLabelKeys?.[option] ? t(filter.displayLabelKeys[option]) : option}
                                        </span>
                                    </label>
                                );
                            })}

                            {showMoreButton && (
                                <button
                                    className={styles.showMoreButton}
                                    onClick={() => toggleFilterExpand(filter.id)}
                                    type="button"
                                >
                                    {isExpanded ? t('components.filterPanel.showLess') : t('components.filterPanel.showMore', { count: (options ?? []).length - 6 })}
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
                const isPrice = filter.id === 'price';

                if (isPrice) {
                    return (
                        <div key={filter.id} className={styles.section}>
                            <div className={styles.sectionLabel}>{t(filter.label)}</div>
                            <div className={styles.priceRange}>
                                <div className={styles.priceInput}>
                                    <input
                                        type="number"
                                        min={minValue}
                                        max={maxValue}
                                        step={step}
                                        placeholder={String(minValue)}
                                        value={rangeVal?.min ?? minValue}
                                        onChange={(e) => handleRangeFieldChange(filter.id, 'min', e.target.value)}
                                    />
                                </div>
                                <span className={styles.priceDash}>—</span>
                                <div className={styles.priceInput}>
                                    <input
                                        type="number"
                                        min={minValue}
                                        max={maxValue}
                                        step={step}
                                        placeholder={String(maxValue)}
                                        value={rangeVal?.max ?? maxValue}
                                        onChange={(e) => handleRangeFieldChange(filter.id, 'max', e.target.value)}
                                    />
                                </div>
                            </div>
                        </div>
                    );
                }

                return (
                    <div key={filter.id} className={styles.section}>
                        <div className={styles.sectionLabel}>{t(filter.label)}</div>
                        <DoubleRangeSlider
                            min={minValue}
                            max={maxValue}
                            step={step}
                            value={rangeVal ?? { min: minValue, max: maxValue }}
                            onChange={(v) => handleSliderChange(filter.id, v)}
                        />
                    </div>
                );
            }

            default:
                return null;
        }
    };

    const handlePrefilterToggle = (enabled: boolean) => {
        setPrefilterEnabled(enabled);
        localStorage.setItem(PREFILTER_KEY, String(enabled));
        onPrefilterChange?.(enabled);
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
        prevFiltersRef.current = JSON.stringify({});
        setFilterValues(initialValues);
        onFilterChange({});
    };

    return (
        <aside className={styles.filterPanel}>
            <div className={styles.head}>
                <span className={styles.eyebrow}>{t('components.filterPanel.filters')}</span>
                <span className={styles.resetLink} onClick={handleClearFilters}>{t('components.filterPanel.reset')}</span>
            </div>

            {showPrefilterToggle && (
                <div
                    className={styles.compat}
                    onClick={() => handlePrefilterToggle(!prefilterEnabled)}
                    role="button"
                    tabIndex={0}
                >
                    <div className={`${styles.toggleTrack} ${prefilterEnabled ? styles.toggleTrackOn : ''}`}>
                        <div className={styles.toggleThumb} />
                    </div>
                    <div className={styles.toggleLabel}>
                        <strong>{t('components.filterPanel.onlyCompatible')}</strong>
                        {t('components.filterPanel.onlyCompatibleHint')}
                    </div>
                </div>
            )}

            <div className={styles.body}>
                {isLoading ? (
                    <div className={styles.loading}>{t('components.filterPanel.loading')}</div>
                ) : (
                    config.filters.map(filter => renderFilter(filter))
                )}
            </div>
        </aside>
    );
};

export default FilterPanel;
