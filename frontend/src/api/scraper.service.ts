import { axiosPrivate } from './axios';
import type { IScrapeJobStatus } from '../types/admin.types';

const PATH = '/scraper';

export const scraperService = {
    getAllJobs: () =>
        axiosPrivate.get<IScrapeJobStatus[]>(`${PATH}/jobs`),

    getLatestJobs: () =>
        axiosPrivate.get<IScrapeJobStatus[]>(`${PATH}/jobs/latest`),

    cancelJob: (jobId: string) =>
        axiosPrivate.delete(`${PATH}/jobs/${jobId}`),

    startCategoryScrape: (routeSlug: string) =>
        axiosPrivate.post(`${PATH}/${routeSlug}`),

    startPriceUpdate: (componentType: string) =>
        axiosPrivate.post(`${PATH}/prices/${componentType}`),
};
