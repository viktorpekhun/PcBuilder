import type { ComponentType, IComponentBase } from "../../types/component.types";

// --- State keys ---

export type SingleKey = 'cpu' | 'gpu' | 'motherboard' | 'powerSupply' | 'cpuCooler' | 'pcCase';
export type MultiKey = 'rams' | 'ssds' | 'hdds' | 'fans';

// --- Component type config (discriminated union) ---

interface SingleComponentTypeConfig {
    key: SingleKey;
    apiType: ComponentType;
    urlType: string;
    label: string;
    buttonLabel: string;
    isMulti: false;
}

interface MultiComponentTypeConfig {
    key: MultiKey;
    apiType: ComponentType;
    urlType: string;
    label: string;
    buttonLabel: string;
    isMulti: true;
}

export type ComponentTypeConfig = SingleComponentTypeConfig | MultiComponentTypeConfig;

// --- Selected component data (persisted in localStorage) ---

export interface SelectedOffer {
    componentId: string;
    offerId: string;
    price: number;
    storeName: string;
    storeLogoUrl: string | null;
    productOfferUrl: string | null;
}

export interface SelectedMultiOffer extends SelectedOffer {
    quantity: number;
}

export interface SelectedComponents {
    cpu: SelectedOffer | null;
    gpu: SelectedOffer | null;
    motherboard: SelectedOffer | null;
    powerSupply: SelectedOffer | null;
    cpuCooler: SelectedOffer | null;
    pcCase: SelectedOffer | null;
    rams: SelectedMultiOffer[];
    ssds: SelectedMultiOffer[];
    hdds: SelectedMultiOffer[];
    fans: SelectedMultiOffer[];
}

// --- Fetched component data ---

export interface SelectedOfferData {
    id: string;
    price: number;
    storeName: string;
    storeLogoUrl: string | null;
    productOfferUrl: string | null;
}

export interface SingleComponentData extends IComponentBase {
    selectedOffer: SelectedOfferData;
}

export interface MultiComponentData {
    component: IComponentBase;
    componentId: string;
    quantity: number;
    price: number;
    storeName: string;
    storeLogoUrl: string | null;
    productOfferUrl: string | null;
    offerId: string;
}

export interface ComponentDataState {
    cpu: SingleComponentData | null;
    gpu: SingleComponentData | null;
    motherboard: SingleComponentData | null;
    powerSupply: SingleComponentData | null;
    cpuCooler: SingleComponentData | null;
    pcCase: SingleComponentData | null;
    rams: MultiComponentData[];
    ssds: MultiComponentData[];
    hdds: MultiComponentData[];
    fans: MultiComponentData[];
}

// --- Editing build (from localStorage) ---

export interface EditingBuild {
    id: string;
    name: string;
    description?: string;
}

// --- Save modal state ---

export interface SaveModalState {
    isOpen: boolean;
    saveAsNew?: boolean;
}

// --- Toast state ---

export interface ToastState {
    visible: boolean;
    message: string;
    type: 'success' | 'error';
}

// --- Constants ---

export const EMPTY_SELECTED: SelectedComponents = {
    cpu: null, gpu: null, motherboard: null,
    powerSupply: null, cpuCooler: null, pcCase: null,
    rams: [], ssds: [], hdds: [], fans: [],
};

export const EMPTY_DATA: ComponentDataState = {
    cpu: null, gpu: null, motherboard: null,
    powerSupply: null, cpuCooler: null, pcCase: null,
    rams: [], ssds: [], hdds: [], fans: [],
};

export const COMPONENT_TYPES: ComponentTypeConfig[] = [
    { key: 'cpu', apiType: 'Cpu', urlType: 'cpu', label: 'Процесор', buttonLabel: 'Процесор', isMulti: false },
    { key: 'gpu', apiType: 'Gpu', urlType: 'gpu', label: 'Відеокарта', buttonLabel: 'Відеокарту', isMulti: false },
    { key: 'motherboard', apiType: 'Motherboard', urlType: 'motherboard', label: 'Материнська Плата', buttonLabel: 'Материнську Плату', isMulti: false },
    { key: 'rams', apiType: 'Ram', urlType: 'ram', label: "Оперативна Пам'ять", buttonLabel: "Оперативну Пам'ять", isMulti: true },
    { key: 'ssds', apiType: 'Ssd', urlType: 'ssd', label: 'SSD Диски', buttonLabel: 'SSD Диск', isMulti: true },
    { key: 'hdds', apiType: 'Hdd', urlType: 'hdd', label: 'HDD Диски', buttonLabel: 'HDD Диск', isMulti: true },
    { key: 'powerSupply', apiType: 'PowerSupply', urlType: 'powerSupply', label: 'Блок живлення', buttonLabel: 'Блок живлення', isMulti: false },
    { key: 'cpuCooler', apiType: 'CpuCooler', urlType: 'cpuCooler', label: 'Кулер Процесора', buttonLabel: 'Кулер Процесора', isMulti: false },
    { key: 'pcCase', apiType: 'PcCase', urlType: 'pcCase', label: 'Корпус ПК', buttonLabel: 'Корпус ПК', isMulti: false },
    { key: 'fans', apiType: 'Fan', urlType: 'fan', label: 'Додаткові Вентилятори', buttonLabel: 'Вентилятор', isMulti: true },
];

export const SINGLE_TYPES = COMPONENT_TYPES.filter((t): t is ComponentTypeConfig & { isMulti: false } => !t.isMulti);
export const MULTI_TYPES = COMPONENT_TYPES.filter((t): t is ComponentTypeConfig & { isMulti: true } => t.isMulti);
