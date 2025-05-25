
export const componentSpecConfigs = {
    cpu: [
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'socket', label: 'Сокет', unit: '' },
        { key: 'cores', label: 'Ядра', unit: '' },
        { key: 'threads', label: 'Потоки', unit: '' },
        {
            keys: ['basicFrequency','maxFrequency'],
            label: 'Частота',
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
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'threads', label: 'Потоки', unit: '' },
        {
            keys: ['gpuManufacturer','gpuModel'],
            label: '',
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
        { key: 'memory', label: "Пам'ять", unit: 'Гб' },
        { key: 'memoryType', label: '', unit: '' },
        { key: 'maxFrequency', label: 'Максимальна частота', unit: 'МГц' },
        { key: 'memoryBus', label: 'Шина', unit: 'біт' },
        {
            keys: ['pcleVersion','PcleLane'],
            label: '',
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
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'socket', label: '', unit: '' },
        { key: 'chipset', label: 'Чіпсет', unit: '' },
        {
            keys: ['dimmSlots','dimmType','dimmFrequency','dimmCapacity'],
            label: '',
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
            label: '',
            isList: true,
            formatItem: (item) => `PCI-E ${item.version}.0 (x${item.lane})`,
            formatList: (items) => items.join(', '),
            unit: ''
        },
        {
            keys: ['formFactor','sizeDimentions'],
            label: 'Форм-фактор',
            format: (values) => {
                if (values.formFactor && values.sizeDimentions) {
                    let newSizeDim = values.sizeDimentions.replace(' мм', '').replace('мм','');
                    return `${values.formFactor}, ${newSizeDim.replaceAll(',','.')} мм`;
                } else if (values.formFactor) {
                    return `${values.formFactor}`;
                } else if (values.sizeDimentions) {
                    let newSizeDim = values.sizeDimentions.replace(' мм', '').replace('мм','');
                    return `${newSizeDim.replaceAll(',','.')} мм`;
                }
                return '';
            },
            unit: ''
        }
    ],
    pcCase: [
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'sizeStandard', label: '', unit: '' },
        {
            key: 'pcCaseFormFactors',
            label: '',
            isList: true,
            formatItem: (item) => `${item.name}`,
            formatList: (items) => items.join('/ '),
            unit: ''
        },
        { key: 'psuLocation', label: 'Місце для блоку живлення', unit: '' },
        { key: 'usb', label: 'USB на передній панелі', unit: '' },
        {
            keys: ['sizeDimentions'],
            label: '',
            format: (values) => {
                if (values.sizeDimentions) {
                    let newSizeDim = values.sizeDimentions.replace(' мм', '').replace('мм','');
                    return `${newSizeDim}`;
                }
                return '';
            },
            unit: 'мм'
        }
    ],
    cpuCooler: [
        { key: 'brand', label: 'Бренд', unit: '' },
        {
            keys: ['type'],
            label: 'Тип',
            format: (values) => {
                if (values.type === 'Water') {
                    return `Водяне`;
                } else {
                    return `Повітряне`;
                }
            },
            unit: ''
        },
        { key: 'fanSize', label: 'Розмір вентиляторів', unit: 'мм' },
        { key: 'radiatorMaterial', label: 'Матеріал радіатора', unit: '' },
        { key: 'maxSpeed', label: 'Максимальна швидкість обертання', unit: '' },
        { key: 'maxPowerDissipation', label: 'Розсіювана потужність', unit: 'Вт' },
        {
            keys: ['noiseLevelDb'],
            label: 'Рівень шуму',
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
            label: '',
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
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'formFactor', label: '', unit: '' },
        { key: 'wattage', label: 'Потужність', unit: 'Вт' },
        { key: 'efficiencyStandart', label: 'Стандарт', unit: '' },
        {
            keys: ['efficiencyPercent'],
            label: 'ККД',
            format: (values) => {
                if (values.efficiencyPercent) {
                    return `${values.efficiencyPercent}%`;
                }
                return '';
            },
            unit: ''
        },
        { key: 'noiseLevelMaxDb', label: 'Максимальний рівень шуму', unit: 'Дб' },
        {
            key: 'powerSupplyCpuPowerConnectors',
            label: 'Підключення CPU',
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
            label: 'Підключення GPU',
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
        { key: 'capacity', label: 'Capacity', unit: 'GB' },
        { key: 'speed', label: 'Speed', unit: 'MHz' },
        { key: 'type', label: 'Type', unit: '' }
    ],
    ssd: [
        { key: 'capacity', label: 'Capacity', unit: 'GB' },
        { key: 'speed', label: 'Speed', unit: 'MHz' },
        { key: 'type', label: 'Type', unit: '' }
    ],
    hdd: [
        { key: 'capacity', label: 'Capacity', unit: 'GB' },
        { key: 'speed', label: 'Speed', unit: 'MHz' },
        { key: 'type', label: 'Type', unit: '' }
    ],
    fan: [
        { key: 'capacity', label: 'Capacity', unit: 'GB' },
        { key: 'speed', label: 'Speed', unit: 'MHz' },
        { key: 'type', label: 'Type', unit: '' }
    ],

    // Add default for any unspecified component type
    default: [
        { key: 'spec1', label: 'Spec 1', unit: '' },
        { key: 'spec2', label: 'Spec 2', unit: '' }
    ]
};