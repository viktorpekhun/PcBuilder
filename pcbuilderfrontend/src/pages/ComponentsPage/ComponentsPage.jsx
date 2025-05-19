import { useState, useEffect } from 'react';
import { getComponents } from "../../services/componentService.js";
import { useNavigate, useParams } from "react-router-dom";
import styles from './ComponentsPage.module.css';
import FilterPanel from '../../components/FilterPanel/FilterPanel.jsx';
import { filterConfigs } from '../../components/FilterPanel/filterConfigs.js';

function ComponentsPage() {
    const { type } = useParams();
    const navigate = useNavigate();
    const [components, setComponents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filters, setFilters] = useState({});

    const filterConfig = filterConfigs[type] || {
        title: `${type.toUpperCase()} Filters`,
        filters: []
    };

    useEffect(() => {
        fetchComponents();
    }, [type]);

    const fetchComponents = async (customFilters = filters) => {
        try {
            setLoading(true);

            // Convert filters to API format
            const apiFilters = {};

            // Process the filters
            Object.keys(customFilters).forEach(filterId => {
                // Find the filter config
                const filterDef = filterConfig.filters.find(f => f.id === filterId);
                if (!filterDef) return;

                const value = customFilters[filterId];

                // Handle different filter types
                if (filterDef.type === 'checkbox' && value.length > 0) {
                    apiFilters[filterDef.property] = value;
                } else if (filterDef.type === 'range') {
                    apiFilters[`${filterDef.property}Min`] = value.min;
                    apiFilters[`${filterDef.property}Max`] = value.max;
                } else if (filterDef.type === 'dropdown' && value) {
                    apiFilters[filterDef.property] = value;
                }
            });

            const data = await getComponents(type, {
                pageNumber: 1,
                pageSize: 20,
                orderBy: 'OffersCount',
                ascending: 'false',
                filters: apiFilters
            });

            setComponents(data);
            setError(null);
        } catch (err) {
            setError(`Error loading ${type} components: ${err.message}`);
        } finally {
            setLoading(false);
        }
    };

    const handleFilterChange = (newFilters) => {
        setFilters(newFilters);
        fetchComponents(newFilters);
    };

    if (loading && !components.length) return <div>Loading {type} components...</div>;
    if (error) return <div>{error}</div>;

    return (
        <section className={styles['component-list']}>
            <h2>{type.toUpperCase()} Components</h2>
            <button className={styles['back-button']} onClick={() => navigate('/')}>
                Back to PC Build
            </button>

            <div className={styles['content-container']}>
                {/* Filter panel */}
                <FilterPanel
                    config={filterConfig}
                    onFilterChange={handleFilterChange}
                />

                {/* Components table */}
                <div className={styles['table-container']}>
                    {loading && <div className={styles['loading-overlay']}>Applying filters...</div>}

                    <table className={styles['components-table']}>
                        <thead>
                        <tr>
                            <th>Image</th>
                            <th>Name</th>
                            <th>Price</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        {components.map(component => (
                            <tr key={component.id} className={styles['component-row']}>
                                <td className={styles['component-image']}
                                    onClick={() => navigate(`/components/${type}/${component.id}`)}>
                                    {component.photoUrl ? (
                                        <img
                                            src={component.photoUrl}
                                            alt={component.name}
                                            className={styles['componentImage']}
                                        />
                                    ) : (
                                        <div className={styles['no-image']}>No image</div>
                                    )}
                                </td>
                                <td
                                    className={styles['component-name']}
                                    onClick={() => navigate(`/components/${type}/${component.id}`)}
                                >
                                    {component.name}
                                </td>
                                <td className={styles['component-price']}>
                                    {component.averagePrice} uah
                                </td>
                                <td className={styles['component-actions']}>
                                    <button
                                        onClick={() => navigate(`/components/${type}/${component.id}`)}
                                        className={styles['select-button']}
                                    >
                                        View Details
                                    </button>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    {components.length === 0 && !loading && (
                        <div className={styles['no-results']}>
                            No components match your filters. Try adjusting your criteria.
                        </div>
                    )}
                </div>
            </div>
        </section>
    );
}

export default ComponentsPage;