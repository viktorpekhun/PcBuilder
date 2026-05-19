import axios from "./axios";

export interface IGlobalSearchItem {
    id: string;
    name: string;
    category: string;
    navigateTo: string;
}

export const searchService = {
    search: (q: string, limit = 5) =>
        axios.get<IGlobalSearchItem[]>(`/Search?q=${encodeURIComponent(q)}&limit=${limit}`),
};
