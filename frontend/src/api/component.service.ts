import axios from "./axios";
import type {
    ComponentType,
    ComponentListMap,
    ComponentDetailMap,
    IResourceParameters,
    IFilterOptions,
    IPagination,
    IPriceHistoryPoint,
} from "../types/component.types";

const PATH = "/Component";

function buildQuery(params?: IResourceParameters): string {
    if (!params) return "";

    const query = new URLSearchParams();

    if (params.pageNumber != null) query.set("pageNumber", String(params.pageNumber));
    if (params.pageSize != null) query.set("pageSize", String(params.pageSize));
    if (params.orderBy) query.set("orderBy", params.orderBy);
    if (params.ascending != null) query.set("ascending", String(params.ascending));
    if (params.searchQuery) query.set("searchQuery", params.searchQuery);

    if (params.filters) {
        for (const [key, values] of Object.entries(params.filters)) {
            if (values.length > 0) {
                query.set(key, JSON.stringify(values));
            }
        }
    }

    const str = query.toString();
    return str ? `?${str}` : "";
}

export function parsePagination(headers: Record<string, string>): IPagination | null {
    const raw = headers["x-pagination"];
    return raw ? JSON.parse(raw) : null;
}

export const componentService = {
    getAll: <T extends ComponentType>(type: T, params?: IResourceParameters) =>
        axios.get<ComponentListMap[T][]>(`${PATH}/${type}${buildQuery(params)}`),

    getById: <T extends ComponentType>(type: T, id: string) =>
        axios.get<ComponentDetailMap[T]>(`${PATH}/${type}/${id}`),

    getFilterOptions: (type: ComponentType) =>
        axios.get<IFilterOptions>(`${PATH}/${type}/filter-options`),

    getPriceHistory: (type: ComponentType, id: string, days: number) =>
        axios.get<IPriceHistoryPoint[]>(`${PATH}/${type}/${id}/price-history?days=${days}`),
};
