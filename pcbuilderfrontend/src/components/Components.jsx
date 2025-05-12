import { useState, useEffect } from 'react';
import axios from "../api/axios.jsx";

const PRODUCT_URL = '/api/component'

export const getComponents = async (type, params = {}) => {
    const queryParams = new URLSearchParams();

    // Add pagination parameters
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);

    // Add any filters
    Object.entries(params.filters || {}).forEach(([key, value]) => {
        if (Array.isArray(value)) {
            queryParams.append(key, JSON.stringify(value));
        } else {
            queryParams.append(key, value);
        }
    });

    try {
        const response = await axios.get(`${PRODUCT_URL}/${type}?${queryParams}`);
        return response.data; // axios автоматично розпарсить JSON
    } catch (error) {
        console.error("Failed to fetch components:", error);
        throw new Error(`Failed to fetch ${type} components`);
    }
};

function Components( {type}) {
    const [components, setComponents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchComponents = async () => {
            try {
                setLoading(true);
                const data = await getComponents(type, {
                    pageNumber: 1,
                    pageSize: 10,
                    filters: {
                        orderBy: 'Cores'
                    }
                });
                setComponents(data);
                setError(null);
            } catch (err) {
                setError(`Error loading ${type} components: ${err.message}`);
            } finally {
                setLoading(false);
            }
        };

        fetchComponents();
    }, [type]);

    if (loading) return <div>Loading {type} components...</div>;
    if (error) return <div>{error}</div>;

    return(
        <div className="component-list">
            <h2>{type} Components</h2>
            <ul>
                {components.map(component => (
                    <li key={component.id}>
                        {component.name}
                        {component.photoUrl && <img src={component.photoUrl} alt={component.name} width="50"/>}
                    </li>
                ))}
            </ul>
        </div>
    );

}

export default Components