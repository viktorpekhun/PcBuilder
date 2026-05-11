// --- Component type ---

export type ComponentType =
    | "Cpu" | "Gpu" | "Ram" | "Motherboard" | "CpuCooler"
    | "PcCase" | "PowerSupply" | "Ssd" | "Hdd" | "Fan";

// --- Pagination & query ---

export interface IPagination {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
}

export interface IPartialBuildIds {
    cpuId?: string;
    gpuId?: string;
    motherboardId?: string;
    ramId?: string;
    cpuCoolerId?: string;
    pcCaseId?: string;
    powerSupplyId?: string;
}

export interface IResourceParameters {
    pageNumber?: number;
    pageSize?: number;
    orderBy?: string;
    ascending?: boolean;
    searchQuery?: string;
    filters?: Record<string, string[]>;
    prefilter?: boolean;
    partialBuild?: IPartialBuildIds;
}

export type IFilterOptions = Record<string, string[]>;

// --- Shared nested types ---

export interface IStore {
    name: string;
    logoUrl?: string;
    likes: number;
    dislikes: number;
}

export interface IProductOffer {
    id: string;
    price: number;
    productOfferUrl: string;
    store: IStore;
}

// --- Base component interfaces ---

export interface IComponentBase {
    id: string;
    name: string;
    photoUrl?: string;
    brand: string;
    averagePrice?: number;
    offersCount?: number;
    [key: string]: unknown;
}

// =====================
// List types (per-component)
// =====================

export interface ICpuList extends IComponentBase {
    socket: string;
    basicFrequency?: number;
    maxFrequency?: number;
    cores?: number;
    threads?: number;
}

export interface IGpuList extends IComponentBase {
    gpuManufacturer: string;
    gpuModel: string;
    memory?: number;
    memoryType?: string;
    memoryBus?: number;
    pcleVersion?: number;
    pcleLane?: number;
    maxFrequency?: number;
}

export interface IRamList extends IComponentBase {
    type: string;
    frequency: number;
    capacity?: number;
    timings?: string;
    voltage?: number;
    xmp: boolean;
}

export interface IPcleSlot {
    version: number;
    lane?: number;
    quantity?: number;
}

export interface IMotherboardList extends IComponentBase {
    socket: string;
    chipset: string;
    dimmSlots?: number;
    dimmType?: string;
    dimmFrequency?: number;
    dimmCapacity?: number;
    formFactor?: string;
    sizeDimentions?: string;
    pcleSlots: IPcleSlot[];
}

export interface ICpuCoolerSocket {
    socketType: string;
}

export interface ICpuCoolerList extends IComponentBase {
    type: string;
    fanSize?: number;
    radiatorMaterial?: string;
    speedControl?: string;
    powerConnector?: string;
    maxPowerDissipation?: number;
    maxSpeed?: number;
    noiseLevelDb?: number;
    length?: number;
    width?: number;
    height?: number;
    cpuCoolerSockets: ICpuCoolerSocket[];
}

export interface IPcCaseFormFactor {
    name: string;
}

export interface IPcCaseList extends IComponentBase {
    sizeStandard?: string;
    sizeDimentions?: string;
    psuLocation?: string;
    usb?: string;
    pcCaseFormFactors: IPcCaseFormFactor[];
}

export interface IPowerSupplyCpuConnector {
    pins: number;
    additionalPins?: number;
    quantity: number;
}

export interface IPowerSupplyGpuConnector {
    pins: number;
    additionalPins?: number;
    quantity: number;
}

export interface IPowerSupplyList extends IComponentBase {
    formFactor: string;
    wattage: number;
    efficiencyStandart?: string;
    efficiencyPercent?: number;
    noiseLevelMaxDb?: number;
    powerSupplyCpuPowerConnectors: IPowerSupplyCpuConnector[];
    powerSupplyGpuPowerConnectors: IPowerSupplyGpuConnector[];
}

export interface ISsdList extends IComponentBase {
    capacity: number;
    interface?: string;
    nandType?: string;
    isTrimmSupported: boolean;
    formFactor?: string;
    maxReadSpeed?: number;
    maxWriteSpeed?: number;
}

export interface IHddList extends IComponentBase {
    capacity: number;
    interface?: string;
    formFactor?: string;
    spindleSpeed?: number;
    cache?: number;
}

export interface IFanList extends IComponentBase {
    moduleCount?: number;
    bearingType?: string;
    speedControl?: string;
    connector?: string;
    color?: string;
    maxSpeed?: number;
    noiseLevelDb?: number;
    sizeLength?: number;
    sizeWidth?: number;
    sizeHeight?: number;
}

// =====================
// Detail types (extend list with extra fields)
// =====================

export interface ICpuDetail extends ICpuList {
    cache?: number;
    dimmType?: string;
    techprocess?: string;
    tdp?: number;
    integratedGraphics: boolean;
    complectation?: string;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

export interface IGpuPowerConnector {
    pins: number;
    quantity: number;
}

export interface IGpuDetail extends IGpuList {
    cudaCores?: number;
    memorySpeed?: number;
    sizeLength?: number;
    sizeWidth?: number;
    sizeHeight?: number;
    wattage?: number;
    psuReccomended?: number;
    factoryLink?: string;
    gpuPowerConnectors: IGpuPowerConnector[];
    productOffers: IProductOffer[];
}

export interface IRamDetail extends IRamList {
    moduleQuantity?: number;
    ecc: boolean;
    expo: boolean;
    bufferization?: string;
    color?: string;
    wattage?: number;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

export interface ICpuPowerConnector {
    pins: number;
    quantity: number;
}

export interface IM2Slot {
    version: number;
    lane?: number;
    quantity?: number;
}

export interface IRearPort {
    type: string;
    quantity?: number;
}

export interface IInnerPort {
    type: string;
    value: string;
}

export interface IMotherboardDetail extends IMotherboardList {
    sata3Count?: number;
    powerMotherboard?: number;
    fanQuantity?: number;
    pcleX1Quantity?: number;
    ethernet?: string;
    audio?: string;
    wifi?: string;
    bluetooth?: string;
    videoPorts?: string;
    wattage?: number;
    factoryLink?: string;
    cpuPowerConnectors: ICpuPowerConnector[];
    m2Slots: IM2Slot[];
    rearPorts: IRearPort[];
    innerPorts: IInnerPort[];
    productOffers: IProductOffer[];
}

export interface ICpuCoolerDetail extends ICpuCoolerList {
    fanCount?: number;
    minSpeed?: number;
    airflowCfm?: number;
    voltage?: number;
    lifespan?: number;
    weight?: number;
    wattage?: number;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

export interface IPcCaseFanLocation {
    name: string;
    fanSize: number;
    maxFans: number;
}

export interface IPcCaseDetail extends IPcCaseList {
    weight?: number;
    maxGpuLength?: number;
    maxCpuCoolerHeight?: number;
    hasDustFilters: boolean;
    builtInFans?: string;
    additionalFanPlaces?: string;
    slot25Quant?: number;
    slot35Quant?: number;
    slot525Quant?: number;
    expansionSlotQuant?: number;
    hasHeadphones: boolean;
    hasMicrophone: boolean;
    factoryLink?: string;
    pcCaseFanLocations: IPcCaseFanLocation[];
    productOffers: IProductOffer[];
}

export interface IPowerSupplyMotherboardConnector {
    pins: number;
    quantity: number
}

export interface IPowerSupplyCpuConnector {
    pins: number;
    additionalPins?: number;
    quantity: number
}

export interface IPowerSupplyGpuConnector {
    pins: number;
    additionalPins?: number;
    quantity: number
}

export interface IPowerSupplyDetail extends IPowerSupplyList {
    molexCount?: number;
    sataCount?: number;
    fddCount?: number;
    inputMinVoltage?: number;
    inputMaxVoltage?: number;
    hasApcf: boolean;
    Modularity?: string;
    size?: string;
    factoryLink?: string;
    powerSupplyMotherboardPowerConnectors: IPowerSupplyMotherboardConnector[];
    powerSupplyCpuPowerConnectors: IPowerSupplyCpuConnector[];
    powerSupplyGpuPowerConnectors: IPowerSupplyGpuConnector[];
    productOffers: IProductOffer[];
}

export interface ISsdDetail extends ISsdList {
    description?: string;
    size?: string;
    weight?: number;
    randomReadSpeed?: number;
    randomWriteSpeed?: number;
    writingRecource?: number;
    averageLifeTime?: number;
    wattage: number;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

export interface IHddDetail extends IHddList {
    speed?: number;
    writingTechnology?: string;
    noiceDb?: number;
    wattage?: number;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

export interface IFanDetail extends IFanList {
    minSpeed?: number;
    airflowCfm?: number;
    voltage?: number;
    weight?: number;
    wattage?: number;
    factoryLink?: string;
    productOffers: IProductOffer[];
}

// =====================
// Type maps (for generic service methods)
// =====================

export interface ComponentListMap {
    Cpu: ICpuList;
    Gpu: IGpuList;
    Ram: IRamList;
    Motherboard: IMotherboardList;
    CpuCooler: ICpuCoolerList;
    PcCase: IPcCaseList;
    PowerSupply: IPowerSupplyList;
    Ssd: ISsdList;
    Hdd: IHddList;
    Fan: IFanList;
}

export interface IPriceHistoryPoint {
    date: string;
    averagePrice: number;
    offersCount: number;
}

export interface ComponentDetailMap {
    Cpu: ICpuDetail;
    Gpu: IGpuDetail;
    Ram: IRamDetail;
    Motherboard: IMotherboardDetail;
    CpuCooler: ICpuCoolerDetail;
    PcCase: IPcCaseDetail;
    PowerSupply: IPowerSupplyDetail;
    Ssd: ISsdDetail;
    Hdd: IHddDetail;
    Fan: IFanDetail;
}
