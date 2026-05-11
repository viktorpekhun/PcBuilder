import axios, { axiosPrivate } from "./axios";
import type {
    IApiResponse,
    IAutoBuildRequest,
    IAutoBuildResult,
    IBuildCompatibilityReport,
    IComponentsCompatibility,
    IPcBuildInput,
    IPcBuildList,
    IPcBuildRequest
} from "../types/build.types";

const PATH: string = "/PcBuild";

export const buildService = {
    checkCompatibility: (data: IComponentsCompatibility) => axios.post<IBuildCompatibilityReport>(`${PATH}/check`, data),
    saveBuild: (data: IPcBuildInput) => axiosPrivate.post<IApiResponse>(`${PATH}/save`, data),
    updateBuild: (id: string, data: IPcBuildInput) => axiosPrivate.put<IApiResponse>(`${PATH}/update/${id}`, data),
    getUserBuilds: () => axiosPrivate.get<IPcBuildList[]>(`${PATH}/user-builds`),
    getBuildById: (id: string) => axiosPrivate.get<IPcBuildRequest>(`${PATH}/${id}`),
    deleteBuild: (id: string) => axiosPrivate.delete<IApiResponse>(`${PATH}/${id}`),
    publishBuild: (id: string, isPublished: boolean) => axiosPrivate.put<IApiResponse>(`${PATH}/${id}/publish`, { isPublished }),
    getPublicBuildById: (id: string) => axios.get<IPcBuildRequest>(`${PATH}/${id}`),
    cloneBuild: (id: string) => axiosPrivate.post<{ success: boolean; buildId: string }>(`${PATH}/${id}/clone`),
    autoBuild: (data: IAutoBuildRequest) => axios.post<IAutoBuildResult>(`${PATH}/auto`, data),
}