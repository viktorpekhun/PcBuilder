export interface IPcBuildGallery {
    id: string;
    name: string;
    description?: string;
    price: number;
    averageRating: number;
    publishedAt?: string;
    username: string;
    avatarUrl?: string;
    componentCount: number;
    commentCount: number;
}
