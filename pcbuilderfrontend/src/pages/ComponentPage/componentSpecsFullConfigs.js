
export const componentSpecFullConfigs = {
    cpu: [
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'socket', label: 'Сокет', unit: '' },
        { key: 'basicFrequency', label: 'Базова частота, ГГц', unit: '' },
        { key: 'maxFrequency', label: 'Максимальна частота, ГГц', unit: '' },
        { key: 'сache', label: "Об'єм кеш-пам'яті третього рівня, МБ", unit: '' },
        { key: 'dimmType', label: "Підтримка оперативної пам'яті", unit: '' },
        { key: 'cores', label: 'Ядра', unit: '' },
        { key: 'threads', label: 'Потоки', unit: '' },
        { key: 'techprocess', label: 'Виробнича технологія, нм', unit: '' },
        { key: 'tpd', label: 'Максимальне тепловиділення, Вт', unit: '' },
        {
            keys: ['integratedGraphics'],
            label: 'Інтегрована графіка',
            format: (values) => {
                if (values.integratedGraphics === true) {
                    return `є`;
                } else if (values.integratedGraphics === false) {
                    return `немає`;
                }
                return '';
            },
            unit: ''
        },
        { key: 'complectation', label: 'Комплектація', unit: '' }
    ],
    gpu: [
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'gpuManufacturer', label: 'Виробник чіпа', unit: '' },
        { key: 'gpuModel', label: 'Модель', unit: '' },
        {
            keys: ['pcleVersion','PcleLane'],
            label: 'Інтерфейс',
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
        { key: 'memory', label: "Об'єм пам'яті, ГБ", unit: '' },
        { key: 'memoryType', label: "Тип пам'яті", unit: '' },
        { key: 'memorySpeed', label: "Швидкість пам'яті, ГБ/c", unit: '' },
        { key: 'memoryBus', label: 'Шина, біт', unit: '' },
        { key: 'maxFrequency', label: 'Максимальна частота, МГц', unit: '' },
        { key: 'cudaCores', label: 'Кількість CUDA ядер', unit: '' },
        { key: 'wattage', label: 'Енергоспоживання, Вт', unit: '' },
        {
            key: 'gpuPowerConnectors',
            label: "Роз'єми додаткового живлення",
            isList: true,
            formatItem: (item) => `${item.quantity}x${item.pins} pin`,
            formatList: (items) => items.join(', '),
            unit: ''
        },
        { key: 'psuReccomended', label: 'Рекомендована потужність БЖ, Вт', unit: '' },
        {
            keys: ['sizeLength','sizeWidth','sizeHeight'],
            label: 'Розміри, мм',
            format: (values) => {
                if (values.sizeLength && values.sizeWidth && values.sizeHeight) {
                    return `${values.sizeLength} x ${values.sizeWidth} x ${values.sizeHeight}`;
                } else if (values.sizeLength && values.sizeWidth) {
                    return `${values.sizeLength} x ${values.sizeWidth}`;
                }
                return '';
            },
            unit: ''
        }
    ],
    motherboard: [
        { key: 'brand', label: 'Бренд', unit: '' },
        { key: 'socket', label: 'Сокет', unit: '' },
        { key: 'chipset', label: 'Чіпсет', unit: '' },
        {
            keys: ['dimmSlots','dimmType','dimmFrequency','dimmCapacity'],
            label: "Підтримувана оперативна пам'ять",
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
            label: 'Слоти PCI-Express x16',
            isList: true,
            formatItem: (item) => `PCI-E ${item.version}.0 (x${item.lane})`,
            formatList: (items) => items.join(''),
            unit: ''
        },
        { key: 'pcleX1Quantity', label: 'Слоти PCI-Express x1', unit: '' },
        {
            keys: ['powerMotherboard'],
            label: 'Живлення материнської плати',
            format: (values) => {
                if (values.powerMotherboard) {
                    return `1x${values.powerMotherboard} pin`;
                }
                return '';
            },
            unit: ''
        },
        {
            key: 'cpuPowerConnectors',
            label: 'Живлення процесора',
            isList: true,
            formatItem: (item) => `${item.quantity}x${item.pins} pin`,
            formatList: (items) => items.join(', '),
            unit: ''
        },
        {
            key: 'm2Slots',
            label: "Типи М2 слотів",
            isList: true,
            formatItem: (item) => `${item.quantity}x PCI-E ${item.version}.0 (x${item.lane})`,
            formatList: (items) => items.join(', '),
            unit: ''
        },
        { key: 'sata3Count', label: "Кількість SATA роз'ємів", unit: '' },
        { key: 'formFactor', label: 'Форм фактор', unit: '' },
        {
            keys: ['sizeDimentions'],
            label: 'Розмір, мм',
            format: (values) => {
                if (values.sizeDimentions) {
                    let newSizeDim = values.sizeDimentions.replace(' мм', '').replace('мм','').replaceAll(' ', '')
                        .replaceAll(',', '.').replaceAll(/[xх]/gi, ' x ');
                    return `${newSizeDim}`;
                }
                return '';
            },
            unit: ''
        },
        { key: 'ethernet', label: 'Ethernet', unit: '' },
        { key: 'audio', label: 'Аудіокодек', unit: '' },
        { key: 'wifi', label: 'WiFi', unit: '' },
        { key: 'bluetooth', label: 'Bluetooth', unit: '' },
        { key: 'videoPorts', label: 'Виходи для монітора', unit: '' },
        {
            key: 'innerPorts',
            label: "Роз'єми на платі",
            isList: true,
            renderAsSeparateRows: true,
            getLabelFromItem: (item) => item.type,
            getValueFromItem: (item) => item.value,
            valueUnit: ''
        },
        {
            key: 'rearPorts',
            label: "Роз'єми на задній панелі",
            isList: true,
            renderAsSeparateRows: true,
            getLabelFromItem: (item) => item.type,
            getValueFromItem: (item) => item.quantity,
            valueUnit: ''
        },

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

    default: [
        { key: 'spec1', label: 'Spec 1', unit: '' },
        { key: 'spec2', label: 'Spec 2', unit: '' }
    ]
};