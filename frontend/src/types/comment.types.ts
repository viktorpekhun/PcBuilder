export interface IComment {
    id: string;
    text: string;
    createdAt: string;
    userId: string;
    username: string;
    avatarUrl?: string;
}

export interface IAddCommentRequest {
    text: string;
}
