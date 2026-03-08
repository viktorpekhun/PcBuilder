import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { getComponents } from "../../services/componentService.js";
import { useNavigate, useParams } from "react-router-dom";
import styles from './ComponentsPage.module.css';
import FilterPanel from '../../components/FilterPanel/FilterPanel.jsx';
import { Pagination } from '../../components/Pagination/Pagination.tsx';
import { filterConfigs } from '../../components/FilterPanel/filterConfigs.js';
import {componentSpecConfigs} from "./componentSpecsConfigs.js";
import { Button } from '../../components/Button/Button.js';

function ComponentsPage() {
    const { type } = useParams();
    const navigate = useNavigate();
    const [components, setComponents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({});
    const [searchQuery, setSearchQuery] = useState('');
    const firstLoadDone = useRef(false);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalResults, setTotalResults] = useState(1);
    const pageSize = 20;
    const [sortField, setSortField] = useState('offersCount');
    const [sortDirection, setSortDirection] = useState('desc');

    const filterConfig = useMemo(() => ({
        ...filterConfigs[type] || {
            title: `${type.toUpperCase()} Filters`,
            filters: []
        },
        componentType: type
    }), [type]);


    const fetchComponents = useCallback(async (customFilters, page = 1, query = null) => {
        try {
            setLoading(true);

            // Use provided customFilters parameter instead of filters state
            const filtersToUse = customFilters || {};
            // Use provided query or fall back to activeSearchQuery
            const searchToUse = query !== null ? query : searchQuery;

            // Convert filters to API format
            const apiFilters = {};

            // Process the filters
            Object.keys(filtersToUse).forEach(filterId => {
                // Find the filter config
                const filterDef = filterConfig.filters.find(f => f.id === filterId);
                if (!filterDef) return;

                const value = filtersToUse[filterId];

                // Handle different filter types
                if (filterDef.type === 'checkbox' && value.length > 0) {
                    // Initialize the property array if it doesn't exist
                    if (!apiFilters[filterDef.property]) {
                        apiFilters[filterDef.property] = [];
                    }

                    // Make sure we don't add duplicate values
                    value.forEach(item => {
                        if (!apiFilters[filterDef.property].includes(item)) {
                            apiFilters[filterDef.property].push(item);
                        }
                    });

                    // JSON stringify the array
                    apiFilters[filterDef.property] = JSON.stringify(apiFilters[filterDef.property]);
                } else if (filterDef.type === 'range') {
                    // Format range values according to API requirements
                    const rangeProperty = `${filterDef.property}`;
                    const rangeArray = [
                        `${value.min}`.toString().replace('.', ','),
                        `${value.max}`.toString().replace('.', ',')
                    ];

                    // JSON stringify the array
                    apiFilters[rangeProperty] = JSON.stringify(rangeArray);
                }
            });

            const response = await getComponents(type, {
                pageNumber: page,
                pageSize: pageSize,
                orderBy: sortField,  // Use the current sort field
                ascending: sortDirection === 'asc' ? 'true' : 'false',  // Convert direction to string
                filters: apiFilters,
                searchQuery: searchToUse.trim()
            });

            console.log("API Response:", response);

            // Check for pagination header in the response
            const paginationHeader = response.headers?.get('x-pagination');

            if (paginationHeader) {
                try {
                    const paginationData = JSON.parse(paginationHeader);
                    console.log("Pagination header:", paginationData);

                    setComponents(response.data || response);
                    setTotalPages(paginationData.TotalPages);
                    setCurrentPage(paginationData.PageNumber);
                    setTotalResults(paginationData.TotalCount);

                    console.log(`Using header pagination: Total pages = ${paginationData.TotalPages}`);
                } catch (parseError) {
                    console.error("Failed to parse pagination header:", parseError);
                    // Fallback to old method
                    handleResponseWithoutHeader(response);
                }
            } else {
                // Fallback to old method if header is not available
                handleResponseWithoutHeader(response);
            }

            setError(null);
        } catch (err) {
            setError(`Error loading ${type} components: ${err.message}`);
        } finally {
            setLoading(false);
            firstLoadDone.current = true;
        }
    }, [type, filterConfig.filters, pageSize, sortField, sortDirection]); // Remove activeSearchQuery from dependencies

    // Helper function to handle responses without pagination header
    const handleResponseWithoutHeader = (response) => {
        if (response.items && response.totalPages) {
            setComponents(response.items);
            setTotalPages(response.totalPages);
            setCurrentPage(response.currentPage || 1);
            console.log(`Using API response pagination: Total pages = ${response.totalPages}`);
        } else {
            // If not, assume it's just an array of items
            const data = Array.isArray(response) ? response : response.data || [];
            setComponents(data);
            const calculatedPages = Math.max(1, Math.ceil(data.length / pageSize));
            console.log(`Calculating pagination: Items = ${data.length}, Pages = ${calculatedPages}`);
            setTotalPages(calculatedPages);
        }
    };

    const handleSortChange = (field) => {
        if (field === sortField) {
            // Toggle direction if clicking the same field
            setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
        } else {
            // Set new field and default direction
            setSortField(field);
            setSortDirection('asc');
        }
    };
    // Initial load effect
    useEffect(() => {
        firstLoadDone.current = false;
        setFilters({});
        setSearchQuery('');
        setCurrentPage(1);
        fetchComponents({}, 1, '');
    }, [type]);

    // Combined effect to handle filters and search changes
    useEffect(() => {
        if (!firstLoadDone.current) return;
        setCurrentPage(1);
        fetchComponents(filters, 1, searchQuery); // use searchQuery directly
    }, [filters, sortField, sortDirection]); // Remove activeSearchQuery

    // Handle page changes
    useEffect(() => {
        if (!firstLoadDone.current) return;
        if (currentPage > 1) {
            fetchComponents(filters, currentPage, searchQuery);
        }
    }, [currentPage]); // Remove fetchComponents from dependencies

    // Handle search input change - update state, debounce will handle the rest
    const handleSearchChange = (e) => {
        setSearchQuery(e.target.value);
    };

    // Debounced search effect
    useEffect(() => {
        if (!firstLoadDone.current) return;
        const debounceTimer = setTimeout(() => {
            setCurrentPage(1);
            fetchComponents(filters, 1, searchQuery);
        }, 400);

        return () => clearTimeout(debounceTimer); // Cleanup on every keystroke
    }, [searchQuery]);

    const handleFilterChange = useCallback((newFilters) => {
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
                        onChange={handleSearchChange}
                        className={styles['search-input']}
                    />
                </div>
            </div>
            <div className={styles['content-container']}>
                {/* Filter panel */}
                <FilterPanel
                    config={filterConfig}
                    onFilterChange={handleFilterChange}
                />

                {/* Components table */}
                <div className={styles['table-container']}>
                    {/* {loading && <div className={styles['loading-overlay']}>Applying filters...</div>} */}

                    <div className={styles['sorting-controls']}>
                        <span className={styles['sorting-label']}>Сортувати за:</span>
                        <div className={styles['sort-buttons']}>
                            <button
                                className={`${styles['sort-button']} ${sortField === 'name' ? styles['active'] : ''}`}
                                onClick={() => handleSortChange('name')}
                            >
                                Назвою
                                {sortField === 'name' && (
                                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor"
                                         viewBox="0 0 16 16">
                                        <path d={sortDirection === 'asc'
                                            ? "M8 15a.5.5 0 0 0 .5-.5V2.707l3.146 3.147a.5.5 0 0 0 .708-.708l-4-4a.5.5 0 0 0-.708 0l-4 4a.5.5 0 1 0 .708.708L7.5 2.707V14.5a.5.5 0 0 0 .5.5"
                                            : "M8 1a.5.5 0 0 1 .5.5v11.793l3.146-3.147a.5.5 0 0 1 .708.708l-4 4a.5.5 0 0 1-.708 0l-4-4a.5.5 0 0 1 .708-.708L7.5 13.293V1.5A.5.5 0 0 1 8 1"}/>
                                    </svg>
                                )}
                            </button>
                            <button
                                className={`${styles['sort-button']} ${sortField === 'averagePrice' ? styles['active'] : ''}`}
                                onClick={() => handleSortChange('averagePrice')}
                            >
                                Ціною
                                {sortField === 'averagePrice' && (
                                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor"
                                         viewBox="0 0 16 16">
                                        <path d={sortDirection === 'asc'
                                            ? "M8 15a.5.5 0 0 0 .5-.5V2.707l3.146 3.147a.5.5 0 0 0 .708-.708l-4-4a.5.5 0 0 0-.708 0l-4 4a.5.5 0 1 0 .708.708L7.5 2.707V14.5a.5.5 0 0 0 .5.5"
                                            : "M8 1a.5.5 0 0 1 .5.5v11.793l3.146-3.147a.5.5 0 0 1 .708.708l-4 4a.5.5 0 0 1-.708 0l-4-4a.5.5 0 0 1 .708-.708L7.5 13.293V1.5A.5.5 0 0 1 8 1"}/>
                                    </svg>
                                )}
                            </button>
                            <button
                                className={`${styles['sort-button']} ${sortField === 'offersCount' ? styles['active'] : ''}`}
                                onClick={() => handleSortChange('offersCount')}
                            >
                                К-стю пропозицій
                                {sortField === 'offersCount' && (
                                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor"
                                         viewBox="0 0 16 16">
                                        <path d={sortDirection === 'asc'
                                            ? "M8 15a.5.5 0 0 0 .5-.5V2.707l3.146 3.147a.5.5 0 0 0 .708-.708l-4-4a.5.5 0 0 0-.708 0l-4 4a.5.5 0 1 0 .708.708L7.5 2.707V14.5a.5.5 0 0 0 .5.5"
                                            : "M8 1a.5.5 0 0 1 .5.5v11.793l3.146-3.147a.5.5 0 0 1 .708.708l-4 4a.5.5 0 0 1-.708 0l-4-4a.5.5 0 0 1 .708-.708L7.5 13.293V1.5A.5.5 0 0 1 8 1"}/>
                                    </svg>
                                )}
                            </button>
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

                                <div
                                    className={styles['component-info']}
                                >
                                    <div className={styles['component-title']}
                                         onClick={() => navigate(`/components/${type}/${component.id}`)}>
                                        {component.name}
                                    </div>
                                    <div className={styles['component-specs']}>
                                        {(componentSpecConfigs[type] || componentSpecConfigs.default)
                                            .map((spec, index) => {
                                                // Handle single-key specs (original way)
                                                if (spec.key) {

                                                    if (spec.isList) {
                                                        // Check if the property exists and is an array
                                                        if (Array.isArray(component[spec.key]) && component[spec.key].length > 0) {
                                                            const formattedItems = spec.formatList(
                                                                component[spec.key].map(item => spec.formatItem(item))
                                                            );

                                                            return (
                                                                <span key={spec.key} className={styles['spec-item']}>
                                                                    {index > 0 && <span className={styles['spec-separator']}>•</span>}
                                                                    {spec.label && (
                                                                        <span className={styles['spec-label']}>{spec.label}:</span>
                                                                    )}
                                                                    <span className={styles['spec-value']}>
                                                                        {formattedItems}
                                                                        {spec.unit && ` ${spec.unit}`}
                                                                    </span>
                                                                </span>
                                                            );
                                                        }
                                                        return null;
                                                    }

                                                    if (component[spec.key] == null || component[spec.key] === '') {
                                                        return null; // Skip if value is null or empty
                                                    }

                                                    return (
                                                        <span key={spec.key} className={styles['spec-item']}>
                                                            {index > 0 && <span className={styles['spec-separator']}>•</span>}

                                                            {spec.label && (
                                                                <span className={styles['spec-label']}>{spec.label}:</span>
                                                            )}

                                                            <span className={styles['spec-value']}>
                                                                {component[spec.key]}
                                                                {spec.unit && ` ${spec.unit}`}
                                                            </span>
                                                        </span>
                                                    );

                                                }

                                                // Handle multi-key specs (combined values)
                                                if (spec.keys) {
                                                    const values = {};
                                                    let hasValue = false;

                                                    spec.keys.forEach(key => {
                                                        values[key] = component[key];
                                                        if (component[key] != null && component[key] !== '') {
                                                            hasValue = true;
                                                        }
                                                    });

                                                    if (!hasValue) return null;

                                                    return (
                                                        <span key={spec.keys.join('_')} className={styles['spec-item']}>
                                                            {index > 0 && <span className={styles['spec-separator']}>•</span>}

                                                            {spec.label && (
                                                                <span className={styles['spec-label']}>{spec.label}:</span>
                                                            )}

                                                            <span className={styles['spec-value']}>
                                                                {spec.format(values)}
                                                                {spec.unit && ` ${spec.unit}`}
                                                            </span>
                                                        </span>
                                                    );
                                                }
                                                return null;
                                            })
                                            .filter(Boolean) // Remove null values
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