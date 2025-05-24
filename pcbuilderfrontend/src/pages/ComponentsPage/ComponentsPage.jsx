import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import { getComponents } from "../../services/componentService.js";
import { useNavigate, useParams } from "react-router-dom";
import styles from './ComponentsPage.module.css';
import FilterPanel from '../../components/FilterPanel/FilterPanel.jsx';
import { filterConfigs } from '../../components/FilterPanel/filterConfigs.js';
import {componentSpecConfigs} from "./componentSpecsConfigs.js";

function ComponentsPage() {
    const { type } = useParams();
    const navigate = useNavigate();
    const [components, setComponents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({});
    const [searchQuery, setSearchQuery] = useState('');
    const [activeSearchQuery, setActiveSearchQuery] = useState('');
    const firstLoadDone = useRef(false);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
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
            const searchToUse = query !== null ? query : activeSearchQuery;

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
        setFilters({});
        setSearchQuery('');
        setActiveSearchQuery('');
        setCurrentPage(1);
        fetchComponents({}, 1, '');
    }, [type, fetchComponents]);

    // Combined effect to handle filters and search changes
    useEffect(() => {
        if (!firstLoadDone.current) return;
        setCurrentPage(1);
        fetchComponents(filters, 1, activeSearchQuery);
    }, [filters, activeSearchQuery, fetchComponents]);

    // Handle page changes
    useEffect(() => {
        if (!firstLoadDone.current) return;
        if (currentPage > 1) {
            fetchComponents(filters, currentPage, activeSearchQuery);
        }
    }, [currentPage, fetchComponents]);

    // Handle search input change - just update the local state
    const handleSearchChange = (e) => {
        setSearchQuery(e.target.value);
    };

    // Handle search submission (button click or Enter key)
    const handleSearchSubmit = (e) => {
        e.preventDefault();
        setActiveSearchQuery(searchQuery); // This will trigger the filters/search effect
    };

    const handleFilterChange = useCallback((newFilters) => {
        setFilters(newFilters);
    }, []);

    const handlePageChange = (page) => {
        if (page < 1 || page > totalPages || page === currentPage) return;
        setCurrentPage(page);
        window.scrollTo(0, 0); // Scroll to top for better UX
    };

    const Pagination = () => {
        console.log(`Rendering pagination: currentPage=${currentPage}, totalPages=${totalPages}`);

        // Don't render pagination if there's only one page
        if (totalPages <= 1) {
            console.log("Not showing pagination: totalPages <= 1");
            return null;
        }

        // Generate array of page numbers to show
        const getPageNumbers = () => {
            let pages = [];
            // Always show first 5 pages or fewer if totalPages < 5
            const maxInitialPages = Math.min(5, totalPages);

            for (let i = 1; i <= maxInitialPages; i++) {
                pages.push(i);
            }

            // Add ellipsis and last page if totalPages > 5
            if (totalPages > 5) {
                if (currentPage > 5 && currentPage < totalPages) {
                    // If current page is beyond first 5, show it too
                    pages = [1, '...', currentPage - 1, currentPage, currentPage + 1, '...', totalPages];
                } else if (currentPage >= totalPages - 2) {
                    // If we're near the end, show last 5 pages
                    pages = [1, '...', totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
                } else {
                    pages.push('...');
                    pages.push(totalPages);
                }
            }

            return pages;
        };

        const pageNumbers = getPageNumbers();

        return (
            <div className={styles.pagination}>
                <button
                    className={`${styles.paginationButton} ${styles.navButton}`}
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                         stroke="currentColor" strokeWidth="1" className="bi bi-chevron-left" viewBox="0 0 16 16">
                        <path fill-rule="evenodd"
                              d="M11.354 1.646a.5.5 0 0 1 0 .708L5.707 8l5.647 5.646a.5.5 0 0 1-.708.708l-6-6a.5.5 0 0 1 0-.708l6-6a.5.5 0 0 1 .708 0"/>
                    </svg>
                </button>

                {pageNumbers.map((page, index) => (
                    <button
                        key={index}
                        className={`${styles.paginationButton} ${page === currentPage ? styles.activePagination : ''} ${page === '...' ? styles.ellipsis : ''}`}
                        onClick={() => page !== '...' && handlePageChange(page)}
                        disabled={page === '...'}
                    >
                        {page}
                    </button>
                ))}

                <button
                    className={`${styles.paginationButton} ${styles.navButton}`}
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                         class="bi bi-chevron-right" viewBox="0 0 16 16" stroke="currentColor" strokeWidth="1">
                        <path fill-rule="evenodd" d="M4.646 1.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 0 .708l-6 6a.5.5 0 0 1-.708-.708L10.293 8 4.646 2.354a.5.5 0 0 1 0-.708"/>
                    </svg>
                </button>
            </div>
        );
    };


    if (loading && !components.length) return <div>Loading {type} components...</div>;
    if (error) return <div>{error}</div>;

    return (
        <section className={styles['component-list']}>
            <div className={styles['top-bar-content']}>
                <button className={`button-secondary ${styles['back-button']}`} onClick={() => navigate('/')}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" stroke="currentColor" strokeWidth="1"
                         className="bi bi-arrow-90deg-left" viewBox="0 0 16 16">
                        <path fill-rule="evenodd"
                              d="M1.146 4.854a.5.5 0 0 1 0-.708l4-4a.5.5 0 1 1 .708.708L2.707 4H12.5A2.5 2.5 0 0 1 15 6.5v8a.5.5 0 0 1-1 0v-8A1.5 1.5 0 0 0 12.5 5H2.707l3.147 3.146a.5.5 0 1 1-.708.708z"/>
                    </svg>
                    До Конфігуратора
                </button>
                <form onSubmit={handleSearchSubmit} className={styles['search-container']}>
                    <input
                        type="text"
                        placeholder="Пошук..."
                        value={searchQuery}
                        onChange={handleSearchChange}
                        className={styles['search-input']}
                    />
                    <button
                        type="submit"
                        className={styles['search-button-field']}
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                             className={`bi bi-search ${styles['search-button']}`} viewBox="0 0 16 16">
                            <path
                                d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001q.044.06.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1 1 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0"/>
                        </svg>
                    </button>
                </form>
            </div>
            <div className={styles['content-container']}>
                {/* Filter panel */}
                <FilterPanel
                    config={filterConfig}
                    onFilterChange={handleFilterChange}
                />

                {/* Components table */}
                <div className={styles['table-container']}>
                    {loading && <div className={styles['loading-overlay']}>Applying filters...</div>}

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
                                    <button
                                        onClick={() => navigate(`/components/${type}/${component.id}`)}
                                        className={`button-primary ${styles['details-button']}`}
                                    >
                                        Деталі
                                    </button>
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
            <Pagination/>
        </section>
    );
}

export default ComponentsPage;