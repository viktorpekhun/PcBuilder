import axios from "../api/axios.jsx";

const PRODUCT_URL = '/api/component';
export const getComponents = async (type, params = {}) => {
    const queryParams = new URLSearchParams();

    // Add pagination parameters
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.orderBy) queryParams.append('orderBy', params.orderBy);
    if (params.ascending) queryParams.append('ascending', params.ascending);


    // Add any filters
    Object.entries(params.filters || {}).forEach(([key, value]) => {
        if (Array.isArray(value)) {
            queryParams.append(key, JSON.stringify(value));
        } else {
            queryParams.append(key, value);
        }
    });

    if (params.searchQuery) queryParams.append('searchQuery', params.searchQuery);
    try {
        const response = await axios.get(`${PRODUCT_URL}/${type}?${queryParams}`);
        return response.data;
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
