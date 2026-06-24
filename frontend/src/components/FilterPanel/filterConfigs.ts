export interface CheckboxFilter {
    id: string;
    type: 'checkbox';
    label: string;
    options: string[];
    dynamic: boolean;
    property: string;
    formatOptionLabel?: (value: string) => string;
    displayLabelKeys?: Record<string, string>;
}

export interface RangeFilter {
    id: string;
    type: 'range';
    label: string;
    min: number;
    max: number;
    step: number;
    dynamic: boolean;
    property: string;
}

export type Filter = CheckboxFilter | RangeFilter;

export interface FilterConfig {
    componentType: string;
    filters: Filter[];
}

export const filterConfigs: Record<string, FilterConfig> = {
    cpu: {
        componentType: "cpu",
        filters: [
            {
                id: "price",
                label: "filters.cpu.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "manufacturer",
                label: "filters.cpu.manufacturer",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "socketType",
                label: "filters.cpu.socketType",
                type: "checkbox",
                options: [],
                property: "socket",
                dynamic: true
            },
            {
                id: "coreCount",
                label: "filters.cpu.coreCount",
                type: "range",
                min: 2,
                max: 16,
                step: 2,
                property: "cores_range",
                dynamic: true
            },
            {
                id: "threadCount",
                label: "filters.cpu.threadCount",
                type: "range",
                min: 2,
                max: 32,
                step: 2,
                property: "threads_range",
                dynamic: true
            },
            {
                id: "basicFrequency",
                label: "filters.cpu.basicFrequency",
                type: "range",
                min: 1.0,
                max: 5.0,
                step: 0.1,
                property: "basicFrequency_range",
                dynamic: true
            },
            {
                id: "maxFrequency",
                label: "filters.cpu.maxFrequency",
                type: "range",
                min: 1.0,
                max: 6.0,
                step: 0.1,
                property: "maxFrequency_range",
                dynamic: true
            },
            {
                id: "cache",
                label: "filters.cpu.cache",
                type: "range",
                min: 2,
                max: 256,
                step: 2,
                property: "cache_range",
                dynamic: true
            },
            {
                id: "integratedGraphics",
                label: "filters.cpu.integratedGraphics",
                type: "checkbox",
                options: ['false', 'true'],
                displayLabelKeys: {
                    'false': 'filters.boolNo',
                    'true': 'filters.boolYes'
                },
                property: "integratedGraphics",
                dynamic: false
            },
            {
                id: "complectation",
                label: "filters.cpu.complectation",
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
                label: "filters.gpu.price",
                type: "range",
                min: 0,
                max: 100000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.gpu.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "gpuManufacturer",
                label: "filters.gpu.gpuManufacturer",
                type: "checkbox",
                options: [],
                property: "gpuManufacturer",
                dynamic: true
            },
            {
                id: "gpuModel",
                label: "filters.gpu.gpuModel",
                type: "checkbox",
                options: [],
                property: "gpuModel",
                dynamic: true
            },
            {
                id: "memory",
                label: "filters.gpu.memory",
                type: "range",
                min: 2,
                max: 48,
                step: 2,
                property: "memory_range",
                dynamic: true
            },
            {
                id: "maxFrequency",
                label: "filters.gpu.maxFrequency",
                type: "range",
                min: 100,
                max: 6000,
                step: 100,
                property: "maxFrequency_range",
                dynamic: true
            },
            {
                id: "cudaCores",
                label: "filters.gpu.cudaCores",
                type: "range",
                min: 100,
                max: 8000,
                step: 100,
                property: "cudaCores_range",
                dynamic: true
            },
            {
                id: "memoryBus",
                label: "filters.gpu.memoryBus",
                type: "checkbox",
                options: [],
                property: "memoryBus",
                dynamic: true
            },
            {
                id: "pcieVersion",
                label: "filters.gpu.pcieVersion",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value: string) => `PCI-Express ${value.replace(',', '.')}`,
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
                label: "filters.motherboard.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.motherboard.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "socketType",
                label: "filters.motherboard.socketType",
                type: "checkbox",
                options: [],
                property: "socket",
                dynamic: true
            },
            {
                id: "chipset",
                label: "filters.motherboard.chipset",
                type: "checkbox",
                options: [],
                property: "chipset",
                dynamic: true
            },
            {
                id: "dimmSlots",
                label: "filters.motherboard.dimmSlots",
                type: "checkbox",
                options: [],
                property: "dimmSlots",
                dynamic: true
            },
            {
                id: "dimmType",
                label: "filters.motherboard.dimmType",
                type: "checkbox",
                options: [],
                property: "dimmType",
                dynamic: true
            },
            {
                id: "dimmCapacity",
                label: "filters.motherboard.dimmCapacity",
                type: "checkbox",
                options: [],
                property: "dimmCapacity",
                dynamic: true
            },
            {
                id: "sata3Count",
                label: "filters.motherboard.sata3Count",
                type: "range",
                min: 0,
                max: 10,
                step: 1,
                property: "sata3Count_range",
                dynamic: true
            },
            {
                id: "formFactor",
                label: "filters.motherboard.formFactor",
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
                label: "filters.cpuCooler.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.cpuCooler.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "type",
                label: "filters.cpuCooler.type",
                type: "checkbox",
                options: ['Air', 'Water'],
                displayLabelKeys: {
                    'Air': 'filters.cpuCooler.typeAir',
                    'Water': 'filters.cpuCooler.typeWater'
                },
                property: "type",
                dynamic: false
            },
            {
                id: "fanSize",
                label: "filters.cpuCooler.fanSize",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value: string) => `${value.replace(',0', '')} мм`,
                property: "fanSize",
                dynamic: true
            },
            {
                id: "fanCount",
                label: "filters.cpuCooler.fanCount",
                type: "checkbox",
                options: [],
                property: "fanCount",
                dynamic: true
            },
            {
                id: "height",
                label: "filters.cpuCooler.height",
                type: "range",
                min: 20,
                max: 200,
                step: 10,
                property: "height_range",
                dynamic: true
            },
            {
                id: "maxPowerDissipation",
                label: "filters.cpuCooler.maxPowerDissipation",
                type: "range",
                min: 20,
                max: 400,
                step: 20,
                property: "maxPowerDissipation_range",
                dynamic: true
            },
            {
                id: "maxSpeed",
                label: "filters.cpuCooler.maxSpeed",
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
                label: "filters.powerSupply.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.powerSupply.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "wattage",
                label: "filters.powerSupply.wattage",
                type: "range",
                min: 100,
                max: 3000,
                step: 50,
                property: "wattage_range",
                dynamic: true
            },
            {
                id: "efficiencyStandart",
                label: "filters.powerSupply.efficiencyStandart",
                type: "checkbox",
                options: [],
                property: "efficiencyStandart",
                dynamic: true
            },
            {
                id: "efficiencyPercent",
                label: "filters.powerSupply.efficiencyPercent",
                type: "range",
                min: 70,
                max: 100,
                step: 2,
                property: "efficiencyPercent_range",
                dynamic: true
            },
            {
                id: "molexCount",
                label: "filters.powerSupply.molexCount",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "molexCount_range",
                dynamic: true
            },
            {
                id: "sataCount",
                label: "filters.powerSupply.sataCount",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "sataCount_range",
                dynamic: true
            },
            {
                id: "isModular",
                label: "filters.powerSupply.isModular",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolYes',
                    'false': 'filters.boolNo'
                },
                property: "isModular",
                dynamic: false
            },
            {
                id: "cpuConnectorPins",
                label: "filters.powerSupply.cpuConnectorPins",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value: string) => `${value} pin`,
                property: "cpuConnectorPins",
                dynamic: true
            },
            {
                id: "gpuConnectorPins",
                label: "filters.powerSupply.gpuConnectorPins",
                type: "checkbox",
                options: [],
                formatOptionLabel: (value: string) => `${value} pin`,
                property: "gpuConnectorPins",
                dynamic: true
            }
        ]
    },
    pcCase: {
        componentType: "pcCase",
        filters: [
            {
                id: "price",
                label: "filters.pcCase.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.pcCase.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "weight",
                label: "filters.pcCase.weight",
                type: "range",
                min: 1,
                max: 40,
                step: 1,
                property: "weight_range",
                dynamic: true
            },
            {
                id: "maxGpuLength",
                label: "filters.pcCase.maxGpuLength",
                type: "range",
                min: 100,
                max: 400,
                step: 20,
                property: "maxGpuLength_range",
                dynamic: true
            },
            {
                id: "maxCpuCoolerHeight",
                label: "filters.pcCase.maxCpuCoolerHeight",
                type: "range",
                min: 60,
                max: 200,
                step: 20,
                property: "maxCpuCoolerHeight_range",
                dynamic: true
            },
            {
                id: "hasDustFilters",
                label: "filters.pcCase.hasDustFilters",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
                },
                property: "hasDustFilters",
                dynamic: false
            },
            {
                id: "slot25Quant",
                label: "filters.pcCase.slot25Quant",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "slot25Quant_range",
                dynamic: true
            },
            {
                id: "slot35Quant",
                label: "filters.pcCase.slot35Quant",
                type: "range",
                min: 1,
                max: 10,
                step: 1,
                property: "slot35Quant_range",
                dynamic: true
            },
            {
                id: "hasHeadphones",
                label: "filters.pcCase.hasHeadphones",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
                },
                property: "hasHeadphones",
                dynamic: false
            },
            {
                id: "hasMicrophone",
                label: "filters.pcCase.hasMicrophone",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
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
                label: "filters.ram.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.ram.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "type",
                label: "filters.ram.type",
                type: "checkbox",
                options: [],
                property: "type",
                dynamic: true
            },
            {
                id: "capacity",
                label: "filters.ram.capacity",
                type: "range",
                min: 2000,
                max: 10000,
                step: 2000,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "frequency",
                label: "filters.ram.frequency",
                type: "range",
                min: 2000,
                max: 10000,
                step: 2000,
                property: "frequency_range",
                dynamic: true
            },
            {
                id: "moduleQuantity",
                label: "filters.ram.moduleQuantity",
                type: "checkbox",
                options: [],
                property: "moduleQuantity",
                dynamic: true
            },
            {
                id: "bufferization",
                label: "filters.ram.bufferization",
                type: "checkbox",
                options: [],
                property: "bufferization",
                dynamic: true
            },
            {
                id: "xmp",
                label: "filters.ram.xmp",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
                },
                property: "xmp",
                dynamic: false
            },
            {
                id: "ecc",
                label: "filters.ram.ecc",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
                },
                property: "ecc",
                dynamic: false
            },
            {
                id: "expo",
                label: "filters.ram.expo",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
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
                label: "filters.ssd.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.ssd.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "formFactor",
                label: "filters.ssd.formFactor",
                type: "checkbox",
                options: [],
                property: "formFactor",
                dynamic: true
            },
            {
                id: "capacity",
                label: "filters.ssd.capacity",
                type: "range",
                min: 120,
                max: 10000,
                step: 100,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "maxReadSpeed",
                label: "filters.ssd.maxReadSpeed",
                type: "range",
                min: 2000,
                max: 10000,
                step: 200,
                property: "maxReadSpeed_range",
                dynamic: true
            },
            {
                id: "maxWriteSpeed",
                label: "filters.ssd.maxWriteSpeed",
                type: "range",
                min: 2000,
                max: 10000,
                step: 200,
                property: "maxWriteSpeed_range",
                dynamic: true
            },
            {
                id: "isTrimmSupported",
                label: "filters.ssd.isTrimmSupported",
                type: "checkbox",
                options: ['true', 'false'],
                displayLabelKeys: {
                    'true': 'filters.boolPresent',
                    'false': 'filters.boolAbsent'
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
                label: "filters.hdd.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.hdd.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "capacity",
                label: "filters.hdd.capacity",
                type: "range",
                min: 120,
                max: 10000,
                step: 100,
                property: "capacity_range",
                dynamic: true
            },
            {
                id: "spindleSpeed",
                label: "filters.hdd.spindleSpeed",
                type: "checkbox",
                options: [],
                property: "spindleSpeed",
                dynamic: true
            },
            {
                id: "cache",
                label: "filters.hdd.cache",
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
                label: "filters.fan.price",
                type: "range",
                min: 0,
                max: 50000,
                step: 100,
                property: "averagePrice_range",
                dynamic: true
            },
            {
                id: "brand",
                label: "filters.fan.brand",
                type: "checkbox",
                options: [],
                property: "brand",
                dynamic: true
            },
            {
                id: "moduleCount",
                label: "filters.fan.moduleCount",
                type: "checkbox",
                options: [],
                property: "moduleCount",
                dynamic: true
            },
            {
                id: "sizeLength",
                label: "filters.fan.sizeLength",
                type: "range",
                min: 50,
                max: 200,
                step: 10,
                property: "sizeLength_range",
                dynamic: true
            },
            {
                id: "maxSpeed",
                label: "filters.fan.maxSpeed",
                type: "range",
                min: 1000,
                max: 10000,
                step: 100,
                property: "maxSpeed_range",
                dynamic: true
            },
            {
                id: "airflowCfm",
                label: "filters.fan.airflowCfm",
                type: "range",
                min: 10,
                max: 100,
                step: 5,
                property: "airflowCfm_range",
                dynamic: true
            },
            {
                id: "noiseLevelDb",
                label: "filters.fan.noiseLevelDb",
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
