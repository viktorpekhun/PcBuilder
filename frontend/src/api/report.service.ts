import { axiosPrivate } from "./axios";
import type { IReportRequest } from "../types/report.types";

const PATH = "/Reports";

export const reportService = {
    reportReview: (reviewId: string, data: IReportRequest) =>
        axiosPrivate.post<{ reportId: string }>(`${PATH}/review/${reviewId}`, data),

    reportBuild: (buildId: string, data: IReportRequest) =>
        axiosPrivate.post<{ reportId: string }>(`${PATH}/build/${buildId}`, data),
};
