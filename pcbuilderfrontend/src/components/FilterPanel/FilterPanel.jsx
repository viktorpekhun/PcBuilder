import {useState, useEffect, useRef} from 'react';
import axios from "../../api/axios.jsx";
import styles from './FilterPanel.module.css';

const FilterPanel = ({ config, onFilterChange }) => {
    const [filterValues, setFilterValues] = useState({});
    const [dynamicOptions, setDynamicOptions] = useState({});
    const [isLoading, setIsLoading] = useState(false);
    const isInitialMount = useRef(true);
    const previousConfigType = useRef(null);
    const prevFiltersRef = useRef(null);
    const [expandedFilters, setExpandedFilters] = useState({});

    useEffect(() => {
        const fetchFilterOptions = async () => {
            try {
                setIsLoading(true);
                const componentType = config.componentType;
                if (!componentType) return;

                const response = await axios.get(`/api/Component/${componentType}/filter-options`);
                setDynamicOptions(response.data);
            } catch (error) {
                console.error('Error fetching filter options:', error);
            } finally {
                setIsLoading(false);
            }
        };

        // Only fetch if component type changes
        if (config.componentType && config.componentType !== previousConfigType.current) {
            previousConfigType.current = config.componentType;
            fetchFilterOptions();
        }
    }, [config.componentType]);

    // Initialize filter values based on config and dynamic options
    useEffect(() => {
        // Exit if still loading options
        if (isLoading) return;

        // Skip if we don't have options yet
        if (Object.keys(dynamicOptions).length === 0) return;

        // Reset the initial mount flag when component type changes
        if (config.componentType !== previousConfigType.current) {
            isInitialMount.current = true;
            previousConfigType.current = config.componentType;
        }

        const initialValues = {};
        config.filters.forEach(filter => {
            if (filter.type === 'checkbox') {
                initialValues[filter.id] = [];
            } else if (filter.type === 'range') {
                let min = filter.min;
                let max = filter.max;

                if (filter.dynamic && dynamicOptions[`${filter.property}`]) {
                    const rangeValues = dynamicOptions[`${filter.property}`];
                    if (rangeValues.length === 2) {
                        min = parseFloat(rangeValues[0].replace(',', '.'));
                        max = parseFloat(rangeValues[1].replace(',', '.'));
                    }
                }

                initialValues[filter.id] = { min, max };
            }
        });

        setFilterValues(initialValues);

        // Don't notify parent on initial setup
        isInitialMount.current = false;
    }, [dynamicOptions, isLoading, config]);

    // Notify parent component when filters chang

    useEffect(() => {
        // Skip empty filter values
        if (Object.keys(filterValues).length === 0) return;

        // Skip the initial mount
        if (isInitialMount.current) {
            isInitialMount.current = false;
            return;
        }

        // Check if values actually changed
        const filtersStr = JSON.stringify(filterValues);
        if (prevFiltersRef.current === filtersStr) return;

        prevFiltersRef.current = filtersStr;
        onFilterChange(filterValues);
    }, [filterValues, onFilterChange]);

    const toggleFilterExpand = (filterId) => {
        setExpandedFilters(prev => ({
            ...prev,
            [filterId]: !prev[filterId]
        }));
    };

    const handleCheckboxChange = (filterId, option, isChecked) => {
        setFilterValues(prev => {
            const updated = { ...prev };
            if (isChecked) {
                updated[filterId] = [...(prev[filterId] || []), option];
            } else {
                updated[filterId] = prev[filterId].filter(item => item !== option);
            }
            return updated;
        });
    };

    const handleRangeChange = (filterId, minOrMax, value) => {
        setFilterValues(prev => {
            const updated = {
                ...prev,
                [filterId]: {
                    ...prev[filterId],
                    [minOrMax]: parseFloat(value)
                }
            };
            return updated;
        });
    };


    const renderFilter = (filter) => {
        switch(filter.type) {
            case 'checkbox': {
                const options = filter.dynamic && dynamicOptions[filter.property]
                    ? dynamicOptions[filter.property]
                    : filter.options;

                // Determine if we need to show the "Show more" button
                const showMoreButton = options.length > 6;
                const isExpanded = expandedFilters[filter.id] || false;

                // Decide which options to display
                const visibleOptions = showMoreButton && !isExpanded
                    ? options.slice(0, 6)
                    : options;

                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <div className={styles.checkboxGroup}>
                            {visibleOptions.map(option => (
                                <label key={option} className={styles.checkboxLabel}>
                                    <input
                                        type="checkbox"
                                        checked={filterValues[filter.id]?.includes(option) || false}
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
                                    {isExpanded ? 'Приховати ▲' : `Показати ще (${options.length - 6}) ▼`}
                                </button>
                            )}
                        </div>
                    </div>
                );
            }

            case 'range': {
                // Use dynamic min/max if available
                let minValue = filter.min;
                let maxValue = filter.max;
                let step = filter.step || 0.1;

                if (filter.dynamic && dynamicOptions[`${filter.property}`]) {
                    const rangeValues = dynamicOptions[`${filter.property}`];
                    if (rangeValues.length === 2) {
                        // Parse the comma-formatted numbers to floats
                        minValue = parseFloat(rangeValues[0].replace(',', '.'));
                        maxValue = parseFloat(rangeValues[1].replace(',', '.'));
                    }
                }

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
                                    value={filterValues[filter.id]?.min || minValue}
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
                                    value={filterValues[filter.id]?.max || maxValue}
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
            <button
                className={`button-primary ${styles.clearButton}`}
                onClick={() => {
                    // Reset all filters
                    const initialValues = {};
                    config.filters.forEach(filter => {
                        if (filter.type === 'checkbox') {
                            initialValues[filter.id] = [];
                        } else if (filter.type === 'range') {
                            let min = filter.min;
                            let max = filter.max;

                            if (filter.dynamic && dynamicOptions[`${filter.property}`]) {
                                const rangeValues = dynamicOptions[`${filter.property}`];
                                if (rangeValues.length === 2) {
                                    min = parseFloat(rangeValues[0].replace(',', '.'));
                                    max = parseFloat(rangeValues[1].replace(',', '.'));
                                }
                            }

                            initialValues[filter.id] = { min, max };
                        }
                    });
                    setFilterValues(initialValues);
                    onFilterChange(initialValues);
                }}
            >
                Очистити фільтри
            </button>
        </div>
    );
};

export default FilterPanel;