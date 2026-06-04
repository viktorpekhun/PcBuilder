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
    ratingCount: number;
    socket?: string;
    photoUrl?: string;
    tags: string[];
}
