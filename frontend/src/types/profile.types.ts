export interface IProfileResponse {
    id: string;
    username: string;
    email: string;
    emailVerified: boolean;
    avatarUrl?: string;
    bio?: string;
    hasPassword: boolean;
    googleLinked: boolean;
    roles: string[];
    createdAt: string;
    buildCount: number;
}

export interface IUpdateProfileRequest {
    username: string;
    bio?: string;
}

export interface IChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface ISetPasswordRequest {
    newPassword: string;
}
