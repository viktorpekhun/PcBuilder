import axios, { axiosPrivate } from "./axios";
import type { IComment, IAddCommentRequest } from "../types/comment.types";
import type { IApiResponse } from "../types/build.types";

const PATH = "/PcBuild";

export const commentService = {
    getComments: (buildId: string, pageNumber = 1, pageSize = 10) =>
        axios.get<IComment[]>(`${PATH}/${buildId}/comments`, {
            params: { pageNumber, pageSize }
        }),
    addComment: (buildId: string, data: IAddCommentRequest) =>
        axiosPrivate.post<IApiResponse & { commentId: string }>(`${PATH}/${buildId}/comments`, data),
    deleteComment: (commentId: string) =>
        axiosPrivate.delete<IApiResponse>(`${PATH}/comments/${commentId}`),
};
