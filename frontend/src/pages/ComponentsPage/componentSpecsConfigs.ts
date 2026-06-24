// --- Spec config types ---

interface SingleKeySpec {
    key: string;
    labelKey: string;
    unit: string;
    isList?: false;
}

interface ListKeySpec {
    key: string;
    labelKey: string;
    unit: string;
    isList: true;
    formatItem: (item: Record<string, unknown>) => string;
    formatList: (items: string[]) => string;
}

interface MultiKeySpec {
    keys: string[];
    labelKey: string;
    unit: string;
    format: (values: Record<string, unknown>) => string;
}

export type SpecConfig = SingleKeySpec | ListKeySpec | MultiKeySpec;

// --- Configs ---

export const componentSpecConfigs: Record<string, SpecConfig[]> = {
    cpu: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'socket', labelKey: 'components.componentPage.specs.socket', unit: '' },
        { key: 'cores', labelKey: 'components.componentPage.specs.cores', unit: '' },
        { key: 'threads', labelKey: 'components.componentPage.specs.threads', unit: '' },
        {
            keys: ['basicFrequency', 'maxFrequency'],
            labelKey: 'components.componentPage.specs.baseFreq',
            format: (values) => {
                if (values.basicFrequency && values.maxFrequency) {
                    return `${values.basicFrequency}-${values.maxFrequency}`;
                } else if (values.basicFrequency) {
                    return `${values.basicFrequency}`;
                } else if (values.maxFrequency) {
                    return `до ${values.maxFrequency}`;
                }
                return '';
            },
            unit: 'ГГц'
        }
    ],
    gpu: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'threads', labelKey: 'components.componentPage.specs.threads', unit: '' },
        {
            keys: ['gpuManufacturer', 'gpuModel'],
            labelKey: '',
            format: (values) => {
                if (values.gpuManufacturer && values.gpuModel) {
                    return `${values.gpuManufacturer} ${values.gpuModel}`;
                } else if (values.gpuManufacturer) {
                    return `${values.gpuManufacturer}`;
                } else if (values.gpuModel) {
                    return `${values.gpuModel}`;
                }
                return '';
            },
            unit: ''
        },
        { key: 'memory', labelKey: 'components.componentPage.specs.vram', unit: 'Гб' },
        { key: 'memoryType', labelKey: '', unit: '' },
        { key: 'maxFrequency', labelKey: 'components.componentPage.specs.memSpeed', unit: 'МГц' },
        { key: 'memoryBus', labelKey: 'components.componentPage.specs.memBus', unit: 'біт' },
        {
            keys: ['pcleVersion', 'PcleLane'],
            labelKey: '',
            format: (values) => {
                if (values.pcleVersion && values.pcleLane) {
                    return `PCI-Express ${values.pcleVersion}.0 x${values.pcleLane}`;
                } else if (values.pcleVersion) {
                    return `PCI-Express ${values.pcleVersion}.0`;
                }
                return '';
            },
            unit: ''
        },
    ],
    motherboard: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'socket', labelKey: '', unit: '' },
        { key: 'chipset', labelKey: 'components.componentPage.specs.chipset', unit: '' },
        {
            keys: ['dimmSlots', 'dimmType', 'dimmFrequency', 'dimmCapacity'],
            labelKey: '',
            format: (values) => {
                if (values.dimmSlots && values.dimmType && values.dimmFrequency && values.dimmCapacity) {
                    return `${values.dimmSlots}x${values.dimmType} ${values.dimmFrequency} МГц, до ${values.dimmCapacity} Гб`;
                } else if (values.dimmType && values.dimmFrequency && values.dimmCapacity) {
                    return `${values.dimmType} ${values.dimmFrequency} МГц, до ${values.dimmCapacity} Гб`;
                } else if (values.dimmSlots && values.dimmType && values.dimmFrequency) {
                    return `${values.dimmSlots}x${values.dimmType} ${values.dimmFrequency} МГц`;
                } else if (values.dimmSlots && values.dimmType && values.dimmCapacity) {
                    return `${values.dimmSlots}x${values.dimmType}, до ${values.dimmCapacity} Гб`;
                } else if (values.dimmSlots && values.dimmType) {
                    return `${values.dimmSlots}x${values.dimmType}`;
                } else if (values.dimmType && values.dimmCapacity) {
                    return `${values.dimmType}, до ${values.dimmCapacity} Гб`;
                }
                return '';
            },
            unit: ''
        },
        {
            key: 'pcleSlots',
            labelKey: '',
            isList: true,
            formatItem: (item) => `PCI-E ${item.version}.0 (x${item.lane})`,
            formatList: (items) => items.join(', '),
            unit: ''
        },
        {
            keys: ['formFactor', 'sizeDimentions'],
            labelKey: 'components.componentPage.specs.formFactor',
            format: (values) => {
                if (values.formFactor && values.sizeDimentions) {
                    const newSizeDim = (values.sizeDimentions as string).replace(' мм', '').replace('мм', '');
                    return `${values.formFactor}, ${newSizeDim.replaceAll(',', '.')} мм`;
                } else if (values.formFactor) {
                    return `${values.formFactor}`;
                } else if (values.sizeDimentions) {
                    const newSizeDim = (values.sizeDimentions as string).replace(' мм', '').replace('мм', '');
                    return `${newSizeDim.replaceAll(',', '.')} мм`;
                }
                return '';
            },
            unit: ''
        }
    ],
    pcCase: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'sizeStandard', labelKey: '', unit: '' },
        {
            key: 'pcCaseFormFactors',
            labelKey: '',
            isList: true,
            formatItem: (item) => `${item.name}`,
            formatList: (items) => items.join('/ '),
            unit: ''
        },
        { key: 'psuLocation', labelKey: 'components.componentPage.specs.psuLocation', unit: '' },
        { key: 'usb', labelKey: 'components.componentPage.specs.frontUsb', unit: '' },
        {
            keys: ['sizeDimentions'],
            labelKey: '',
            format: (values) => {
                if (values.sizeDimentions) {
                    const newSizeDim = (values.sizeDimentions as string).replace(' мм', '').replace('мм', '');
                    return `${newSizeDim}`;
                }
                return '';
            },
            unit: 'мм'
        }
    ],
    cpuCooler: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        {
            keys: ['type'],
            labelKey: 'components.componentPage.specs.coolingType',
            format: (values) => {
                if (values.type === 'Water') {
                    return 'Водяне';
                } else {
                    return 'Повітряне';
                }
            },
            unit: ''
        },
        { key: 'fanSize', labelKey: 'components.componentPage.specs.fanSize', unit: 'мм' },
        { key: 'radiatorMaterial', labelKey: 'components.componentPage.specs.radiator', unit: '' },
        { key: 'maxSpeed', labelKey: 'components.componentPage.specs.maxFanSpeed', unit: '' },
        { key: 'maxPowerDissipation', labelKey: 'components.componentPage.specs.tdpRating', unit: 'Вт' },
        {
            keys: ['noiseLevelDb'],
            labelKey: 'components.componentPage.specs.noiseLevel',
            format: (values) => {
                if (values.noiseLevelDb) {
                    return `${values.noiseLevelDb}`;
                }
                return '';
            },
            unit: 'Дб'
        },
        {
            keys: ['length', 'width', 'height'],
            labelKey: '',
            format: (values) => {
                if (values.length && values.width && values.height) {
                    return `${values.length}x${values.width}x${values.height}`;
                } else if (values.length && values.width) {
                    return `${values.length}x${values.width}`;
                }
                return '';
            },
            unit: 'мм'
        },
    ],
    powerSupply: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'formFactor', labelKey: '', unit: '' },
        { key: 'wattage', labelKey: 'components.componentPage.specs.wattage', unit: 'Вт' },
        { key: 'efficiencyStandart', labelKey: 'components.componentPage.specs.80plusRating', unit: '' },
        {
            keys: ['efficiencyPercent'],
            labelKey: 'components.componentPage.specs.efficiencyPct',
            format: (values) => {
                if (values.efficiencyPercent) {
                    return `${values.efficiencyPercent}%`;
                }
                return '';
            },
            unit: ''
        },
        { key: 'noiseLevelMaxDb', labelKey: 'components.componentPage.specs.maxNoise', unit: 'Дб' },
        {
            key: 'powerSupplyCpuPowerConnectors',
            labelKey: 'components.componentPage.specs.cpuPower',
            isList: true,
            formatItem: (item) => {
                const base = `${item.quantity}x${item.pins}`;
                return item.additionalPins != null
                    ? `${base}+${item.additionalPins} pin`
                    : `${base} pin`;
            },
            formatList: (items) => items.join(', '),
            unit: ''
        },
        {
            key: 'powerSupplyGpuPowerConnectors',
            labelKey: 'components.componentPage.specs.gpuPowerConn',
            isList: true,
            formatItem: (item) => {
                const base = `${item.quantity}x${item.pins}`;
                return item.additionalPins != null
                    ? `${base}+${item.additionalPins} pin`
                    : `${base} pin`;
            },
            formatList: (items) => items.join(', '),
            unit: ''
        },
    ],
    ram: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'type', labelKey: '', unit: '' },
        {
            keys: ['moduleQuantity', 'capacity'],
            labelKey: 'components.componentPage.specs.capacity',
            format: (values) => {
                if (values.moduleQuantity && values.capacity) {
                    return `${values.moduleQuantity}x${values.capacity}`;
                } else if (values.capacity) {
                    return `${values.capacity}`;
                }
                return '';
            },
            unit: 'Гб'
        },
        { key: 'frequency', labelKey: 'components.componentPage.specs.speed', unit: 'МГц' },
        { key: 'timings', labelKey: 'components.componentPage.specs.timings', unit: '' },
        { key: 'voltage', labelKey: 'components.componentPage.specs.voltage', unit: 'В' },
    ],
    ssd: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'capacity', labelKey: 'components.componentPage.specs.capacity', unit: 'Гб' },
        { key: 'interface', labelKey: 'components.componentPage.specs.interface', unit: '' },
        { key: 'formFactor', labelKey: 'components.componentPage.specs.formFactor', unit: '' },
        { key: 'nandType', labelKey: 'components.componentPage.specs.nandType', unit: '' },
        {
            keys: ['maxReadSpeed', 'maxWriteSpeed'],
            labelKey: 'components.componentPage.specs.seqReadWrite',
            format: (values) => {
                if (values.maxReadSpeed && values.maxWriteSpeed) {
                    return `${values.maxReadSpeed} / ${values.maxWriteSpeed}`;
                } else if (values.maxReadSpeed) {
                    return `${values.maxReadSpeed}`;
                } else if (values.maxWriteSpeed) {
                    return `${values.maxWriteSpeed}`;
                }
                return '';
            },
            unit: 'Мб/с'
        },
    ],
    hdd: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'capacity', labelKey: 'components.componentPage.specs.capacity', unit: 'Гб' },
        { key: 'interface', labelKey: 'components.componentPage.specs.interface', unit: '' },
        { key: 'formFactor', labelKey: 'components.componentPage.specs.formFactor', unit: '' },
        { key: 'spindleSpeed', labelKey: 'components.componentPage.specs.spindleSpeed', unit: 'об/хв' },
        { key: 'cache', labelKey: 'components.componentPage.specs.cache', unit: 'Мб' },
        { key: 'writingTechnology', labelKey: 'components.componentPage.specs.writeTech', unit: '' },
    ],
    fan: [
        { key: 'brand', labelKey: 'components.componentPage.specs.brand', unit: '' },
        { key: 'moduleCount', labelKey: 'components.componentPage.specs.fanCount', unit: '' },
        { key: 'connector', labelKey: 'components.componentPage.specs.connector', unit: '' },
        {
            keys: ['minSpeed', 'maxSpeed'],
            labelKey: 'components.componentPage.specs.fanSpeed',
            format: (values) => {
                if (values.minSpeed && values.maxSpeed) {
                    return `${values.minSpeed}-${values.maxSpeed}`;
                } else if (values.maxSpeed) {
                    return `до ${values.maxSpeed}`;
                }
                return '';
            },
            unit: 'об/хв'
        },
        { key: 'noiseLevelDb', labelKey: 'components.componentPage.specs.noiseLevel', unit: 'дБ' },
        { key: 'sizeLength', labelKey: 'components.componentPage.specs.size', unit: 'мм' },
    ],
    default: [
        { key: 'spec1', labelKey: 'components.componentPage.specs.general', unit: '' },
        { key: 'spec2', labelKey: 'components.componentPage.specs.other', unit: '' }
    ]
};
