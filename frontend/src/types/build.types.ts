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

export type CompatibilitySeverity = "Critical" | "Warning" | "Info";

export interface ICompatibilityIssue {
    severity: CompatibilitySeverity;
    code: string;
    parameters: Record<string, string>;
}

export interface ICompatibilityResult {
    ruleName: string;
    fitnessScore: number;
    isStrictlyCompatible: boolean;
    issues: ICompatibilityIssue[];
}

export interface IBuildCompatibilityReport {
    isStrictlyCompatible: boolean;
    overallFitnessScore: number;
    ruleResults: ICompatibilityResult[];
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
    averageRating?: number;
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

// --- Auto-builder ---

export type UsageScenario = 'Gaming' | 'Office' | 'ContentCreation' | 'Workstation' | 'Budget';

export interface IAutoBuildRequest {
    budget: number;
    preferredUse: UsageScenario;
    isStrictBudget: boolean;
    isFutureProof?: boolean;
    preferredFormFactor?: string;
}

export interface ISelectedComponentInfo {
    id: string;
    name: string;
    averagePrice?: number;
}

export interface IAutoBuildComponents {
    cpu?: ISelectedComponentInfo;
    gpu?: ISelectedComponentInfo;
    motherboard?: ISelectedComponentInfo;
    cpuCooler?: ISelectedComponentInfo;
    powerSupply?: ISelectedComponentInfo;
    pcCase?: ISelectedComponentInfo;
    ram?: ISelectedComponentInfo;
    ramQuantity: number;
    ssd?: ISelectedComponentInfo;
    ssdQuantity: number;
    hdd?: ISelectedComponentInfo;
    hddQuantity: number;
    fan?: ISelectedComponentInfo;
    fanQuantity: number;
}

export interface IAutoBuildResult {
    totalPrice: number;
    components: IAutoBuildComponents;
}
