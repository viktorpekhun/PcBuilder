export const filterConfigs = {
    cpu: {
        componentType: "cpu",  // Used for API calls
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "manufacturer",
                label: "Виробник",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "socketType",
                label: "Сокет",
                type: "checkbox",
                options: [],
                property: "socket",
                dynamic: true
            },
            {
                id: "coreCount",
                label: "Кількість ядер",
                type: "range",
                min: 2,
                max: 16,
                step: 2,  // Fallback options
                property: "cores_range",
                dynamic: true
            },
            {
                id: "threadCount",
                label: "Кількість потоків",
                type: "range",
                min: 2,
                max: 32,
                step: 2,
                property: "threads_range",
                dynamic: true
            },
            {
                id: "basicFrequency",
                label: "Базова частота, ГГц",
                type: "range",
                min: 1.0,
                max: 5.0,
                step: 0.1,
                property: "basicFrequency_range",
                dynamic: true
            },
            {
                id: "maxFrequency",
                label: "Максимальна частота, ГГц",
                type: "range",
                min: 1.0,
                max: 6.0,
                step: 0.1,
                property: "maxFrequency_range",
                dynamic: true
            },
            {
                id: "cache",
                label: "Кеш L3",
                type: "range",
                min: 2,
                max: 256,
                step: 2,
                property: "cache_range",
                dynamic: true
            },
            {
                id: "integratedGraphics",
                label: "Інтегрована графіка",
                type: "checkbox",
                options: ['false','true'],
                displayLabels: {
                    'false': 'Ні',
                    'true': 'Так'
                },
                property: "integratedGraphics",
                dynamic: false
            },
            {
                id: "complectation",
                label: "Комплектація",
                type: "checkbox",
                options: [],
                property: "complectation",
                dynamic: true
            }

        ]
    },
    gpu: {
        componentType: "gpu",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 100000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "gpuManufacturer",
                label: "Виробник GPU",
                type: "checkbox",
                options: [],
                property: "gpuManufacturer",
                dynamic: true
            },
            {
                id: "gpuModel",
                label: "Модель",
                type: "checkbox",
                options: [],
                property: "gpuModel",
                dynamic: true
            },
            {
                id: "memory",
                label: "Об'єм пам'яті, Гб",
                type: "range",
                min: 2,
                max: 48,
                step: 2,
                property: "memory_range",
                dynamic: true
            },
            {
                id: "maxFrequency",
                label: "Максимальна частота, МГц",
                type: "range",
                min: 100,
                max: 6000,
                step: 100,
                property: "maxFrequency_range",
                dynamic: true
            },
            {
                id: "cudaCores",
                label: "Кількість CUDA ядер",
                type: "range",
                min: 100,
                max: 8000,
                step: 100,
                property: "cudaCores_range",
                dynamic: true
            },
            {
                id: "memoryBus",
                label: "Шина пам'яті",
                type: "checkbox",
                options: [],
                property: "memoryBus",
                dynamic: true
            },
            {
                id: "pcieVersion",
                label: "Інтерфейс",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value) => `PCI-Express ${value.replace(',','.')}`,
                property: "pcleVersion",
                dynamic: true
            }

        ]
    },
    motherboard: {
        componentType: "motherboard",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "socketType",
                label: "Сокет",
                type: "checkbox",
                options: [],
                property: "socket",
                dynamic: true
            },
            {
                id: "chipset",
                label: "Чіпсет",
                type: "checkbox",
                options: [],
                property: "chipset",
                dynamic: true
            },
            {
                id: "dimmSlots",
                label: "Кількість слотів ОЗУ",
                type: "checkbox",
                options: [],
                property: "dimmSlots",
                dynamic: true
            },
            {
                id: "dimmType",
                label: "Тип підтримуваної ОЗУ",
                type: "checkbox",
                options: [],
                property: "dimmType",
                dynamic: true
            },
            {
                id: "dimmCapacity",
                label: "Максимальний об'єм ОЗУ",
                type: "checkbox",
                options: [],
                property: "dimmCapacity",
                dynamic: true
            },
            {
                id: "sata3Count",
                label: "Кількість роз'ємів SATA",
                type: "range",
                min: 0,
                max: 10,
                step: 1,
                property: "sata3Count_range",
                dynamic: true
            },
            {
                id: "formFactor",
                label: "Форм-фактор",
                type: "checkbox",
                options: [],
                property: "formFactor",
                dynamic: true
            }
        ]
    },
    cpuCooler: {
        componentType: "cpuCooler",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "type",
                label: "Тип охолодження",
                type: "checkbox",
                options: ['Air', 'Water'],
                displayLabels: {
                    'Air': 'Повітряне',
                    'Water': 'Водяне'
                },
                property: "type",
                dynamic: false
            },
            {
                id: "fanSize",
                label: "Розмір вентиляторів",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value) => `${value.replace(',0','')} мм`,
                property: "fanSize",
                dynamic: true
            },
            {
                id: "fanCount",
                label: "Кількість вентиляторів",
                type: "checkbox",
                options: [],
                property: "fanCount",
                dynamic: true
            },
            {
                id: "height",
                label: "Висота кулера, мм",
                type: "range",
                min: 20,
                max: 200,
                step: 10,
                property: "height_range",
                dynamic: true
            },
            {
                id: "maxPowerDissipation",
                label: "Розсіювана потужність, Вт",
                type: "range",
                min: 20,
                max: 400,
                step: 20,
                property: "maxPowerDissipation_range",
                dynamic: true
            },
            {
                id: "maxSpeed",
                label: "Максимальна швидкість обертання, Об/Хв",
                type: "range",
                min: 20,
                max: 400,
                step: 20,
                property: "maxSpeed_range",
                dynamic: true
            }
        ]
    },
    powerSupply: {
        componentType: "powerSupply",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "wattage",
                label: "Потужність, Вт",
                type: "range",
                min: 100,
                max: 3000,
                step: 50,
                property: "wattage_range",
                dynamic: true
            },
            {
                id: "efficiencyStandart",
                label: "Стандарт 80 PLUS",
                type: "checkbox",
                options: [],
                property: "efficiencyStandart",
                dynamic: true
            },
            {
                id: "efficiencyPercent",
                label: "ККД (%)",
                type: "range",
                min: 70,
                max: 100,
                step: 2,
                property: "efficiencyPercent_range",
                dynamic: true
            },
            {
                id: "molexCount",
                label: "Кількість підключень MOLEX",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "molexCount_range",
                dynamic: true
            },
            {
                id: "sataCount",
                label: "Кількість підключень SATA",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "sataCount_range",
                dynamic: true
            },
            {
                id: "isModular",
                label: "Модульність",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Так',
                    'false': 'Ні'
                },
                property: "isModular",
                dynamic: false
            }
        ]
    },
    pcCase: {
        componentType: "pcCase",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "weight",
                label: "Вага, кг",
                type: "range",
                min: 1,
                max: 40,
                step: 1,
                property: "weight_range",
                dynamic: true
            },
            {
                id: "maxGpuLength",
                label: "Максимальна довжина відеокарти, мм",
                type: "range",
                min: 100,
                max: 400,
                step: 20,
                property: "maxGpuLength_range",
                dynamic: true
            },
            {
                id: "maxCpuCoolerHeight",
                label: "Максимальна висота кулера, мм",
                type: "range",
                min: 60,
                max: 200,
                step: 20,
                property: "maxCpuCoolerHeight_range",
                dynamic: true
            },
            {
                id: "hasDustFilters",
                label: "Пилові фільтри",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "hasDustFilters",
                dynamic: false
            },
            {
                id: "slot25Quant",
                label: 'Кількість слотів 2.5"',
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "slot25Quant_range",
                dynamic: true
            },
            {
                id: "slot35Quant",
                label: 'Кількість слотів 3.5"',
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "slot35Quant_range",
                dynamic: true
            },
            {
                id: "hasHeadphones",
                label: "Роз'єм для гарнітури",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "hasHeadphones",
                dynamic: false
            },
            {
                id: "hasMicrophone",
                label: "Роз'єм для мікрофона",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "hasMicrophone",
                dynamic: false
            }
        ]
    },
    ram: {
        componentType: "ram",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "type",
                label: "Тип",
                type: "checkbox",
                options: [],
                property: "type",
                dynamic: true
            },
            {
                id: "capacity",
                label: "Об'єм(штука), Гб",
                type: "range",
                min: 2000,
                max: 10000,
                step: 2000,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "frequency",
                label: "Частота, МГц",
                type: "range",
                min: 2000,
                max: 10000,
                step: 2000,
                property: "frequency_range",
                dynamic: true
            },
            {
                id: "moduleQuantity",
                label: "Кількість планок в комлекті",
                type: "checkbox",
                options: [],
                property: "moduleQuantity",
                dynamic: true
            },
            {
                id: "bufferization",
                label: "Буферизація",
                type: "checkbox",
                options: [],
                property: "bufferization",
                dynamic: true
            },
            {
                id: "xmp",
                label: "Підтримка XMP",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "xmp",
                dynamic: false
            },
            {
                id: "ecc",
                label: "Підтримка ECC",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "ecc",
                dynamic: false
            },
            {
                id: "expo",
                label: "Підтримка EXPO",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "expo",
                dynamic: false
            }

        ]
    },
    ssd: {
        componentType: "ssd",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "formFactor",
                label: "Тип",
                type: "checkbox",
                options: [],
                property: "formFactor",
                dynamic: true
            },
            {
                id: "capacity",
                label: "Об'єм, Гб",
                type: "range",
                min: 120,
                max: 10000,
                step: 100,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "maxReadSpeed",
                label: "Максимальна швидкість читання, МБ/c",
                type: "range",
                min: 2000,
                max: 10000,
                step: 200,
                property: "maxReadSpeed_range",
                dynamic: true
            },
            {
                id: "maxWriteSpeed",
                label: "Максимальна швидкість запису, МБ/c",
                type: "range",
                min: 2000,
                max: 10000,
                step: 200,
                property: "maxWriteSpeed_range",
                dynamic: true
            },
            {
                id: "isTrimmSupported",
                label: "Підтримка Trimm",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabels: {
                    'true': 'Є',
                    'false': 'Немає'
                },
                property: "isTrimmSupported",
                dynamic: false
            }
        ]
    },
    hdd: {
        componentType: "hdd",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "capacity",
                label: "Об'єм, Гб",
                type: "range",
                min: 120,
                max: 10000,
                step: 100,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "spindleSpeed",
                label: "Швидкість обертання шпинделя, об/хв",
                type: "checkbox",
                options: [],
                property: "spindleSpeed",
                dynamic: true
            },
            {
                id: "cache",
                label: "Буфер, МБ",
                type: "checkbox",
                options: [],
                property: "cache",
                dynamic: true
            }
        ]
    },
    fan: {
        componentType: "fan",
        filters: [
            {
                id: "price",
                label: "Ціна, грн",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "Бренд",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "moduleCount",
                label: "Кількість в комплекті",
                type: "checkbox",
                options: [],
                property: "moduleCount",
                dynamic: true
            },
            {
                id: "sizeLength",
                label: "Розмір, мм",
                type: "range",
                min: 50,
                max: 200,
                step: 10,
                property: "sizeLength_range",
                dynamic: true
            },
            {
                id: "maxSpeed",
                label: "Максимальна швидкість обертання, об/хв",
                type: "range",
                min: 1000,
                max: 10000,
                step: 100,
                property: "maxSpeed_range",
                dynamic: true
            },
            {
                id: "airflowCfm",
                label: "Повітряний потік (CFM)",
                type: "range",
                min: 10,
                max: 100,
                step: 5,
                property: "airflowCfm_range",
                dynamic: true
            },
            {
                id: "noiseLevelDb",
                label: "Рівень шуму, Дб",
                type: "range",
                min: 10,
                max: 100,
                step: 5,
                property: "noiseLevelDb_range",
                dynamic: true
            }
        ]
    }
};