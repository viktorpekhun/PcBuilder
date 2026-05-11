export interface IComment {
    id: string;
    text: string;
    rating: number;
    createdAt: string;
    userId: string;
    username: string;
    avatarUrl?: string;
}

export interface IAddCommentRequest {
    text: string;
    rating: number;
}
