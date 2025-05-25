import axios from "../api/axios.jsx";

const PRODUCT_URL = '/api/component';
export const getComponents = async (type, params = {}) => {
    const queryParams = new URLSearchParams();

    // Add all params to query string
    Object.entries(params).forEach(([key, value]) => {
        if (key === 'filters' && typeof value === 'object') {
            // Handle filters object separately
            Object.entries(value).forEach(([filterKey, filterValue]) => {
                if (Array.isArray(filterValue)) {
                    filterValue.forEach(val => queryParams.append(`${filterKey}`, val));
                } else {
                    queryParams.append(`${filterKey}`, filterValue);
                }
            });
        } else {
            queryParams.append(key, value);
        }
    });

    const url = `${PRODUCT_URL}/${type}?${queryParams.toString()}`;

    try {
        const response = await axios.get(url);

        // Return both the data and headers
        return {
            data: response.data,
            headers: response.headers
        };
    } catch (error) {
        console.error("Failed to fetch components:", error);
        throw new Error(`Failed to fetch ${type} components`);
    }
};

export const getComponentById = async (type, id) => {
    try {
        const response = await axios.get(`${PRODUCT_URL}/${type}/${id}`);
        return response.data;
    } catch (error) {
        console.error(`Error fetching ${type} component by ID:`, error);
        throw error;
    }
};
