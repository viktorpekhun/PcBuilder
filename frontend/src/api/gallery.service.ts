import axios from "./axios";
import type { IPcBuildGallery } from "../types/gallery.types";

const PATH = "/PcBuild";

interface GalleryParams {
    pageNumber?: number;
    pageSize?: number;
    orderBy?: string;
    ascending?: boolean;
    searchQuery?: string;
    filters?: Record<string, string[]>;
}

function buildQuery(params?: GalleryParams): string {
    if (!params) return '';
    const parts: string[] = [];

    if (params.pageNumber) parts.push(`pageNumber=${params.pageNumber}`);
    if (params.pageSize) parts.push(`pageSize=${params.pageSize}`);
    if (params.orderBy) parts.push(`orderBy=${params.orderBy}`);
    if (params.ascending !== undefined) parts.push(`ascending=${params.ascending}`);
    if (params.searchQuery) parts.push(`searchQuery=${encodeURIComponent(params.searchQuery)}`);

    if (params.filters) {
        for (const [key, values] of Object.entries(params.filters)) {
            parts.push(`${key}=${encodeURIComponent(JSON.stringify(values))}`);
        }
    }

    return parts.length > 0 ? `?${parts.join('&')}` : '';
}

export const galleryService = {
    getPublicBuilds: (params?: GalleryParams) =>
        axios.get<IPcBuildGallery[]>(`${PATH}/gallery${buildQuery(params)}`),
};
