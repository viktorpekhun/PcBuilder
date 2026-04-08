// --- Shared ---

export interface IComponentQuantity {
    componentId: string;
    offerId?: string;
    quantity: number;
}

// --- Input (save/update) ---

export interface IPcBuildInput {
    name: string;
    description?: string;
    isPublished: boolean;
    cpuId?: string;
    gpuId?: string;
    motherboardId?: string;
    cpuCoolerId?: string;
    powerSupplyId?: string;
    pcCaseId?: string;
    cpuOfferId?: string;
    gpuOfferId?: string;
    motherboardOfferId?: string;
    cpuCoolerOfferId?: string;
    powerSupplyOfferId?: string;
    pcCaseOfferId?: string;
    rams: IComponentQuantity[];
    ssds: IComponentQuantity[];
    hdds: IComponentQuantity[];
    fans: IComponentQuantity[];
}

// --- Compatibility check ---

export type IComponentsCompatibility = Pick<
    IPcBuildInput,
    "cpuId" | "gpuId" | "motherboardId" | "cpuCoolerId" | "powerSupplyId" | "pcCaseId" | "rams" | "ssds" | "hdds" | "fans"
>;

export type CompatibilityMessageType = "Problem" | "Warning";

export interface ICompatibilityMessage {
    type: CompatibilityMessageType;
    message: string;
}

export interface ICompatibilityResult {
    isCompatible: boolean;
    messages: ICompatibilityMessage[];
}

export interface ICompatibilityResponse {
    compatible: boolean;
    hasWarnings: boolean;
    results: ICompatibilityResult[];
}

// --- Build list (user-builds) ---

export interface IPcBuildList {
    id: string;
    name: string;
    price: number;
    updatedAt?: string;
}

// --- Build detail (get by id) ---

export interface IComponentPreview {
    id: string;
    offerId: string;
    name: string;
    imageUrl?: string;
    storeName?: string;
    price?: number;
    productOfferUrl?: string;
}

export interface IMultiComponentPreview {
    id: string;
    offerId: string;
    name: string;
    imageUrl?: string;
    quantity: number;
    storeName?: string;
    totalPrice?: number;
    productOfferUrl?: string;
}

export interface IPcBuildRequest {
    id: string;
    name: string;
    description?: string;
    isPublished: boolean;
    publishedAt?: string;
    price: number;
    userId: string;
    username?: string;
    avatarUrl?: string;
    createdAt: string;
    updatedAt?: string;
    cpu?: IComponentPreview;
    gpu?: IComponentPreview;
    motherboard?: IComponentPreview;
    cpuCooler?: IComponentPreview;
    powerSupply?: IComponentPreview;
    pcCase?: IComponentPreview;
    rams: IMultiComponentPreview[];
    ssds: IMultiComponentPreview[];
    hdds: IMultiComponentPreview[];
    fans: IMultiComponentPreview[];
}

// --- Generic API response ---

export interface IApiResponse {
    success: boolean;
    message: string;
}
