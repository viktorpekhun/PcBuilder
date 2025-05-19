import { useState, useEffect } from 'react';
import styles from './FilterPanel.module.css';

const FilterPanel = ({ config, onFilterChange }) => {
    const [filterValues, setFilterValues] = useState({});

    useEffect(() => {
        // Initialize filter values based on config
        const initialValues = {};
        config.filters.forEach(filter => {
            if (filter.type === 'checkbox') {
                initialValues[filter.id] = [];
            } else if (filter.type === 'range') {
                initialValues[filter.id] = {
                    min: filter.min,
                    max: filter.max
                };
            } else if (filter.type === 'dropdown') {
                initialValues[filter.id] = '';
            }
        });
        setFilterValues(initialValues);
    }, [config]);

    const handleCheckboxChange = (filterId, option, isChecked) => {
        setFilterValues(prev => {
            const updated = { ...prev };
            if (isChecked) {
                updated[filterId] = [...(prev[filterId] || []), option];
            } else {
                updated[filterId] = prev[filterId].filter(item => item !== option);
            }

            // Notify parent component about filter changes
            onFilterChange({ ...updated });
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

            onFilterChange({ ...updated });
            return updated;
        });
    };

    const handleDropdownChange = (filterId, value) => {
        setFilterValues(prev => {
            const updated = {
                ...prev,
                [filterId]: value
            };

            onFilterChange({ ...updated });
            return updated;
        });
    };

    const renderFilter = (filter) => {
        switch(filter.type) {
            case 'checkbox':
                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <div className={styles.checkboxGroup}>
                            {filter.options.map(option => (
                                <label key={option} className={styles.checkboxLabel}>
                                    <input
                                        type="checkbox"
                                        checked={filterValues[filter.id]?.includes(option) || false}
                                        onChange={(e) => handleCheckboxChange(filter.id, option, e.target.checked)}
                                    />
                                    {option}
                                </label>
                            ))}
                        </div>
                    </div>
                );

            case 'range':
                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <div className={styles.rangeGroup}>
                            <label>
                                Min:
                                <input
                                    type="number"
                                    min={filter.min}
                                    max={filter.max}
                                    step={filter.step}
                                    value={filterValues[filter.id]?.min || filter.min}
                                    onChange={(e) => handleRangeChange(filter.id, 'min', e.target.value)}
                                    className={styles.rangeInput}
                                />
                            </label>
                            <label>
                                Max:
                                <input
                                    type="number"
                                    min={filter.min}
                                    max={filter.max}
                                    step={filter.step}
                                    value={filterValues[filter.id]?.max || filter.max}
                                    onChange={(e) => handleRangeChange(filter.id, 'max', e.target.value)}
                                    className={styles.rangeInput}
                                />
                            </label>
                        </div>
                    </div>
                );

            case 'dropdown':
                return (
                    <div key={filter.id} className={styles.filterGroup}>
                        <h4>{filter.label}</h4>
                        <select
                            value={filterValues[filter.id] || ''}
                            onChange={(e) => handleDropdownChange(filter.id, e.target.value)}
                            className={styles.dropdown}
                        >
                            <option value="">All</option>
                            {filter.options.map(option => (
                                <option key={option} value={option}>{option}</option>
                            ))}
                        </select>
                    </div>
                );

            default:
                return null;
        }
    };

    return (
        <div className={styles.filterPanel}>
            <h3>{config.title}</h3>
            <div className={styles.filtersContainer}>
                {config.filters.map(filter => renderFilter(filter))}
            </div>
            <button
                className={styles.clearButton}
                onClick={() => {
                    // Reset all filters
                    const initialValues = {};
                    config.filters.forEach(filter => {
                        if (filter.type === 'checkbox') {
                            initialValues[filter.id] = [];
                        } else if (filter.type === 'range') {
                            initialValues[filter.id] = {
                                min: filter.min,
                                max: filter.max
                            };
                        } else if (filter.type === 'dropdown') {
                            initialValues[filter.id] = '';
                        }
                    });
                    setFilterValues(initialValues);
                    onFilterChange(initialValues);
                }}
            >
                Clear Filters
            </button>
        </div>
    );
};

export default FilterPanel;