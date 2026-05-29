// --- Spec config types ---

interface SimpleSpec {
    key: string;
    label: string;
    unit: string;
    isList?: false;
    fullWidth?: boolean;
}

interface ListSpec {
    key: string;
    label: string;
    unit: string;
    isList: true;
    renderAsSeparateRows?: false;
    formatItem: (item: Record<string, unknown>) => string;
    formatList: (items: string[]) => string;
    fullWidth?: boolean;
}

interface SeparateRowsListSpec {
    key: string;
    label: string;
    unit: string;
    isList: true;
    renderAsSeparateRows: true;
    getLabelFromItem: (item: Record<string, unknown>) => string;
    getValueFromItem: (item: Record<string, unknown>) => unknown;
    valueUnit?: string;
    fullWidth?: boolean;
}

interface MultiKeySpec {
    keys: string[];
    label: string;
    unit: string;
    format: (values: Record<string, unknown>) => string;
    fullWidth?: boolean;
}

interface SectionHeaderSpec {
    type: 'sectionHeader';
    label: string;
}

export type FullSpecConfig = SimpleSpec | ListSpec | SeparateRowsListSpec | MultiKeySpec | SectionHeaderSpec;

// LocalizedString from backend: { uk: string, en?: string }
function loc(v: unknown): string {
    if (!v || typeof v !== 'object') return '';
    const o = v as Record<string, unknown>;
    return String(o['en'] ?? o['uk'] ?? '');
}

export const componentSpecFullConfigs: Record<string, FullSpecConfig[]> = {

    // ── CPU ─────────────────────────────────────────────────────
    Cpu: [
        { type: 'sectionHeader', label: 'Core architecture' },
        { key: 'brand',          label: 'Brand',          unit: '' },
        { key: 'socket',         label: 'Socket',         unit: '' },
        { key: 'basicFrequency', label: 'Base frequency', unit: 'GHz' },
        { key: 'maxFrequency',   label: 'Max boost',      unit: 'GHz' },
        { key: 'cores',          label: 'Cores',          unit: '' },
        { key: 'threads',        label: 'Threads',        unit: '' },
        { key: 'cache',          label: 'L3 cache',       unit: 'MB' },
        { key: 'techprocess',    label: 'Process node',   unit: 'nm' },

        { type: 'sectionHeader', label: 'Other' },
        { key: 'dimmType',       label: 'Memory type',    unit: '' },
        { key: 'tdp',            label: 'TDP',            unit: 'W' },
        { key: 'integratedGraphics', label: 'Has iGPU',   unit: '' },
        { key: 'complectation',  label: 'Packaging',      unit: '' },

    ],

    // ── GPU ─────────────────────────────────────────────────────
    Gpu: [
        { type: 'sectionHeader', label: 'Chip' },
        { key: 'brand',           label: 'Brand',          unit: '' },
        { key: 'gpuManufacturer', label: 'Chip vendor',    unit: '' },
        { key: 'gpuModel',        label: 'GPU model',      unit: '' },
        { key: 'maxFrequency',    label: 'Boost clock',    unit: 'MHz' },
        { key: 'cudaCores',       label: 'Shader units',   unit: '' },

        { type: 'sectionHeader', label: 'Memory' },
        { key: 'memory',          label: 'VRAM',           unit: 'GB' },
        { key: 'memoryType',      label: 'Memory type',    unit: '' },
        { key: 'memoryBus',       label: 'Memory bus',     unit: 'bit' },
        { key: 'memorySpeed',     label: 'Memory speed',   unit: 'GB/s' },

        { type: 'sectionHeader', label: 'Interface' },
        {
            keys: ['pcleVersion', 'pcleLane'],
            label: 'PCIe interface', unit: '',
            format: (v) => {
                if (v['pcleVersion'] && v['pcleLane']) return `PCIe ${v['pcleVersion']}.0 x${v['pcleLane']}`;
                if (v['pcleVersion']) return `PCIe ${v['pcleVersion']}.0`;
                return '';
            },
        },
        {
            key: 'gpuPowerConnectors', label: 'Power connectors', unit: '', isList: true,
            formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
            formatList: (items) => items.join(', '),
        },

        { type: 'sectionHeader', label: 'Power & size' },
        { key: 'wattage',         label: 'TDP',            unit: 'W' },
        { key: 'psuReccomended',  label: 'Recommended PSU', unit: 'W' },
        {
            keys: ['sizeLength', 'sizeWidth', 'sizeHeight'],
            label: 'Dimensions', unit: 'mm',
            format: (v) => {
                if (v['sizeLength'] && v['sizeWidth'] && v['sizeHeight']) return `${v['sizeLength']} × ${v['sizeWidth']} × ${v['sizeHeight']}`;
                if (v['sizeLength'] && v['sizeWidth']) return `${v['sizeLength']} × ${v['sizeWidth']}`;
                return '';
            },
        },
    ],

    // ── RAM ─────────────────────────────────────────────────────
    Ram: [
        { type: 'sectionHeader', label: 'Specifications' },
        { key: 'brand',          label: 'Brand',          unit: '' },
        { key: 'type',           label: 'Memory type',    unit: '' },
        { key: 'frequency',      label: 'Speed',          unit: 'MHz' },
        {
            keys: ['capacity', 'moduleQuantity'],
            label: 'Capacity', unit: '',
            format: (v) => {
                if (v['capacity'] && v['moduleQuantity']) return `${v['moduleQuantity']}× ${v['capacity']} GB`;
                if (v['capacity']) return `${v['capacity']} GB`;
                return '';
            },
        },
        { key: 'timings',        label: 'Timings',        unit: '' },
        { key: 'voltage',        label: 'Voltage',        unit: 'V' },

        { type: 'sectionHeader', label: 'Features' },
        { key: 'xmp',            label: 'XMP',            unit: '' },
        { key: 'expo',           label: 'EXPO',           unit: '' },
        { key: 'ecc',            label: 'ECC',            unit: '' },
        { key: 'bufferization',  label: 'Buffering',      unit: '' },
        {
            keys: ['color'],
            label: 'Color', unit: '',
            format: (v) => loc(v['color']),
        },

        { type: 'sectionHeader', label: 'Power' },
        { key: 'wattage',        label: 'Power draw',     unit: 'W' },
    ],

    // ── Motherboard ─────────────────────────────────────────────
    Motherboard: [
        { type: 'sectionHeader', label: 'Platform' },
        { key: 'brand',          label: 'Brand',          unit: '' },
        { key: 'socket',         label: 'Socket',         unit: '' },
        { key: 'chipset',        label: 'Chipset',        unit: '' },
        { key: 'formFactor',     label: 'Form factor',    unit: '' },
        {
            keys: ['sizeDimentions'],
            label: 'Size', unit: 'mm',
            format: (v) => v['sizeDimentions']
                ? String(v['sizeDimentions']).replace(/\s*мм\s*/gi, '').replace(/[хx]/gi, '×')
                : '',
        },

        { type: 'sectionHeader', label: 'Memory' },
        { key: 'dimmType',       label: 'RAM type',       unit: '' },
        { key: 'dimmSlots',      label: 'DIMM slots',     unit: '' },
        { key: 'dimmFrequency',  label: 'Max frequency',  unit: 'MHz' },
        { key: 'dimmCapacity',   label: 'Max capacity',   unit: 'GB' },

        { type: 'sectionHeader', label: 'Expansion slots' },
        {
            key: 'pcleSlots', label: 'PCIe x16 slots', unit: '', isList: true,
            formatItem: (item) => `PCIe ${item['version']}.0 x${item['lane']}`,
            formatList: (items) => items.join(', '),
        },
        { key: 'pcleX1Quantity', label: 'PCIe x1 slots',  unit: '' },
        {
            key: 'm2Slots', label: 'M.2 slots', unit: '', isList: true,
            formatItem: (item) => `PCIe ${item['version']}.0 x${item['lane']}`,
            formatList: (items) => items.join(', '),
        },
        { key: 'sata3Count',     label: 'SATA ports',     unit: '' },

        { type: 'sectionHeader', label: 'Power' },
        {
            keys: ['powerMotherboard'],
            label: 'ATX connector', unit: '',
            format: (v) => v['powerMotherboard'] ? `${v['powerMotherboard']}-pin` : '',
        },
        {
            key: 'cpuPowerConnectors', label: 'CPU power', unit: '', isList: true,
            formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
            formatList: (items) => items.join(', '),
        },

        { type: 'sectionHeader', label: 'Connectivity' },
        { key: 'ethernet',       label: 'Ethernet',       unit: '' },
        { key: 'wifi',           label: 'Wi-Fi',          unit: '' },
        { key: 'bluetooth',      label: 'Bluetooth',      unit: '' },
        { key: 'audio',          label: 'Audio codec',    unit: '' },
        { key: 'videoPorts',     label: 'Video outputs',  unit: '' },

        { type: 'sectionHeader', label: 'Rear ports' },
        {
            key: 'rearPorts', label: '', unit: '', isList: true, renderAsSeparateRows: true,
            getLabelFromItem: (item) => String(item['type']),
            getValueFromItem: (item) => item['quantity'],
        },

        { type: 'sectionHeader', label: 'Internal connectors' },
        {
            key: 'innerPorts', label: '', unit: '', isList: true, renderAsSeparateRows: true,
            getLabelFromItem: (item) => String(item['type']),
            getValueFromItem: (item) => item['value'],
        },
    ],

    // ── CPU Cooler ───────────────────────────────────────────────
    CpuCooler: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',              label: 'Brand',           unit: '' },
        { key: 'type',               label: 'Cooling type',    unit: '' },
        {
            keys: ['radiatorMaterial'],
            label: 'Radiator', unit: '',
            format: (v) => loc(v['radiatorMaterial']),
        },
        { key: 'fanCount',           label: 'Fan count',       unit: '' },
        { key: 'fanSize',            label: 'Fan size',        unit: 'mm' },
        { key: 'powerConnector',     label: 'Connector',       unit: '' },
        { key: 'speedControl',       label: 'Speed control',   unit: '' },

        { type: 'sectionHeader', label: 'Performance' },
        { key: 'maxPowerDissipation', label: 'TDP rating',     unit: 'W' },
        { key: 'maxSpeed',           label: 'Max fan speed',   unit: 'RPM' },
        { key: 'minSpeed',           label: 'Min fan speed',   unit: 'RPM' },
        { key: 'noiseLevelDb',       label: 'Noise level',     unit: 'dB' },
        { key: 'airflowCfm',         label: 'Airflow',         unit: 'CFM' },

        { type: 'sectionHeader', label: 'Compatibility' },
        {
            key: 'cpuCoolerSockets', label: 'Supported sockets', unit: '', isList: true,
            formatItem: (item) => String(item['socketType']),
            formatList: (items) => items.join(', '),
        },

        { type: 'sectionHeader', label: 'Dimensions & power' },
        {
            keys: ['length', 'width', 'height'],
            label: 'Dimensions', unit: 'mm',
            format: (v) => {
                if (v['length'] && v['width'] && v['height']) return `${v['length']} × ${v['width']} × ${v['height']}`;
                if (v['length'] && v['width']) return `${v['length']} × ${v['width']}`;
                return '';
            },
        },
        { key: 'weight',             label: 'Weight',          unit: 'g' },
        { key: 'voltage',            label: 'Voltage',         unit: 'V' },
        { key: 'wattage',            label: 'Power draw',      unit: 'W' },
        { key: 'lifespan',           label: 'Lifespan',        unit: 'h' },
    ],

    // ── PC Case ──────────────────────────────────────────────────
    PcCase: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',          label: 'Brand',           unit: '' },
        { key: 'sizeStandard',   label: 'Case class',      unit: '' },
        {
            key: 'pcCaseFormFactors', label: 'Supported form factors', unit: '', isList: true,
            formatItem: (item) => String(item['name']),
            formatList: (items) => items.join(', '),
        },
        {
            keys: ['psuLocation'],
            label: 'PSU location', unit: '',
            format: (v) => loc(v['psuLocation']),
        },
        { key: 'hasDustFilters', label: 'Dust filters',    unit: '' },

        { type: 'sectionHeader', label: 'Clearances' },
        { key: 'maxGpuLength',       label: 'Max GPU length',     unit: 'mm' },
        { key: 'maxCpuCoolerHeight', label: 'Max cooler height',  unit: 'mm' },

        { type: 'sectionHeader', label: 'Bays' },
        { key: 'slot25Quant',        label: '2.5″ bays',          unit: '' },
        { key: 'slot35Quant',        label: '3.5″ bays',          unit: '' },
        { key: 'slot525Quant',       label: '5.25″ bays',         unit: '' },
        { key: 'expansionSlotQuant', label: 'Expansion slots',    unit: '' },

        { type: 'sectionHeader', label: 'Fans' },
        {
            keys: ['builtInFans'],
            label: 'Included fans', unit: '',
            format: (v) => loc(v['builtInFans']),
            fullWidth: true,
        },
        {
            keys: ['additionalFanPlaces'],
            label: 'Fan mounts', unit: '',
            format: (v) => loc(v['additionalFanPlaces']),
            fullWidth: true,
        },
        {
            key: 'pcCaseFanLocations', label: 'Fan locations', unit: '', isList: true,
            formatItem: (item) => {
                const name = typeof item['name'] === 'object' ? loc(item['name']) : String(item['name'] ?? '');
                return `${name} ${item['fanSize']}mm ×${item['maxFans']}`;
            },
            formatList: (items) => items.join(', '),
            fullWidth: true,
        },

        { type: 'sectionHeader', label: 'I/O panel' },
        { key: 'usb',            label: 'Front USB',       unit: '' },
        { key: 'hasHeadphones',  label: 'Headphone jack',  unit: '' },
        { key: 'hasMicrophone',  label: 'Mic jack',        unit: '' },

        { type: 'sectionHeader', label: 'Physical' },
        {
            keys: ['sizeDimentions'],
            label: 'Dimensions', unit: 'mm',
            format: (v) => v['sizeDimentions']
                ? String(v['sizeDimentions']).replace(/\s*мм\s*/gi, '')
                : '',
        },
        { key: 'weight',         label: 'Weight',          unit: 'kg' },
    ],

    // ── Power Supply ─────────────────────────────────────────────
    PowerSupply: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',              label: 'Brand',          unit: '' },
        { key: 'formFactor',         label: 'Form factor',    unit: '' },
        { key: 'wattage',            label: 'Wattage',        unit: 'W' },
        {
            keys: ['modularity'],
            label: 'Modularity', unit: '',
            format: (v) => loc(v['modularity']),
        },

        { type: 'sectionHeader', label: 'Efficiency' },
        { key: 'efficiencyStandart', label: '80 PLUS rating', unit: '' },
        {
            keys: ['efficiencyPercent'],
            label: 'Efficiency', unit: '',
            format: (v) => v['efficiencyPercent'] ? `${v['efficiencyPercent']}%` : '',
        },
        { key: 'hasApcf',            label: 'Active PFC',     unit: '' },

        { type: 'sectionHeader', label: 'Connectors' },
        {
            key: 'powerSupplyMotherboardPowerConnectors', label: 'Motherboard', unit: '', isList: true,
            formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
            formatList: (items) => items.join(', '),
        },
        {
            key: 'powerSupplyCpuPowerConnectors', label: 'CPU', unit: '', isList: true,
            formatItem: (item) => {
                const base = `${item['quantity']}× ${item['pins']}`;
                return item['additionalPins'] != null ? `${base}+${item['additionalPins']}-pin` : `${base}-pin`;
            },
            formatList: (items) => items.join(', '),
        },
        {
            key: 'powerSupplyGpuPowerConnectors', label: 'GPU', unit: '', isList: true,
            formatItem: (item) => {
                const base = `${item['quantity']}× ${item['pins']}`;
                return item['additionalPins'] != null ? `${base}+${item['additionalPins']}-pin` : `${base}-pin`;
            },
            formatList: (items) => items.join(', '),
        },
        { key: 'molexCount',         label: 'Molex',          unit: '' },
        { key: 'sataCount',          label: 'SATA',           unit: '' },
        { key: 'fddCount',           label: 'FDD',            unit: '' },

        { type: 'sectionHeader', label: 'Input' },
        {
            keys: ['inputMinVoltage', 'inputMaxVoltage'],
            label: 'Input voltage', unit: 'V',
            format: (v) => {
                if (v['inputMinVoltage'] && v['inputMaxVoltage']) return `${v['inputMinVoltage']} – ${v['inputMaxVoltage']}`;
                if (v['inputMinVoltage']) return String(v['inputMinVoltage']);
                return '';
            },
        },

        { type: 'sectionHeader', label: 'Physical' },
        { key: 'noiseLevelMaxDb',    label: 'Max noise',      unit: 'dB' },
        { key: 'size',               label: 'Dimensions',     unit: '' },
    ],

    // ── SSD ──────────────────────────────────────────────────────
    Ssd: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',          label: 'Brand',           unit: '' },
        { key: 'capacity',       label: 'Capacity',        unit: 'GB' },
        { key: 'interface',      label: 'Interface',       unit: '' },
        { key: 'formFactor',     label: 'Form factor',     unit: '' },
        { key: 'nandType',       label: 'NAND type',       unit: '' },
        { key: 'isTrimmSupported', label: 'TRIM',          unit: '' },

        { type: 'sectionHeader', label: 'Performance' },
        {
            keys: ['maxReadSpeed', 'maxWriteSpeed'],
            label: 'Sequential read / write', unit: 'MB/s',
            format: (v) => {
                if (v['maxReadSpeed'] && v['maxWriteSpeed']) return `${v['maxReadSpeed']} / ${v['maxWriteSpeed']}`;
                if (v['maxReadSpeed']) return String(v['maxReadSpeed']);
                return '';
            },
        },
        {
            keys: ['randomReadSpeed', 'randomWriteSpeed'],
            label: 'Random read / write', unit: 'IOPS',
            format: (v) => {
                if (v['randomReadSpeed'] && v['randomWriteSpeed']) return `${v['randomReadSpeed']} / ${v['randomWriteSpeed']}`;
                if (v['randomReadSpeed']) return String(v['randomReadSpeed']);
                return '';
            },
        },
        { key: 'writingRecource', label: 'Write endurance', unit: 'TBW' },
        { key: 'averageLifeTime', label: 'MTBF',           unit: 'h' },

        { type: 'sectionHeader', label: 'Physical' },
        { key: 'size',           label: 'Dimensions',      unit: '' },
        { key: 'weight',         label: 'Weight',          unit: 'g' },
        { key: 'wattage',        label: 'Power draw',      unit: 'W' },
    ],

    // ── HDD ──────────────────────────────────────────────────────
    Hdd: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',          label: 'Brand',           unit: '' },
        { key: 'capacity',       label: 'Capacity',        unit: 'GB' },
        { key: 'interface',      label: 'Interface',       unit: '' },
        { key: 'formFactor',     label: 'Form factor',     unit: '' },

        { type: 'sectionHeader', label: 'Performance' },
        { key: 'spindleSpeed',   label: 'Spindle speed',   unit: 'RPM' },
        { key: 'cache',          label: 'Cache',           unit: 'MB' },
        { key: 'speed',          label: 'Transfer speed',  unit: 'MB/s' },
        { key: 'writingTechnology', label: 'Write technology', unit: '' },

        { type: 'sectionHeader', label: 'Physical' },
        { key: 'noiceDb',        label: 'Noise level',     unit: 'dB' },
        { key: 'wattage',        label: 'Power draw',      unit: 'W' },
    ],

    // ── Fan ──────────────────────────────────────────────────────
    Fan: [
        { type: 'sectionHeader', label: 'General' },
        { key: 'brand',          label: 'Brand',           unit: '' },
        { key: 'moduleCount',    label: 'Pack count',      unit: '' },
        {
            keys: ['bearingType'],
            label: 'Bearing type', unit: '',
            format: (v) => loc(v['bearingType']),
        },
        { key: 'connector',      label: 'Connector',       unit: '' },
        { key: 'speedControl',   label: 'Speed control',   unit: '' },
        {
            keys: ['color'],
            label: 'Color', unit: '',
            format: (v) => loc(v['color']),
        },

        { type: 'sectionHeader', label: 'Performance' },
        {
            keys: ['minSpeed', 'maxSpeed'],
            label: 'Fan speed', unit: 'RPM',
            format: (v) => {
                if (v['minSpeed'] && v['maxSpeed']) return `${v['minSpeed']} – ${v['maxSpeed']}`;
                if (v['maxSpeed']) return String(v['maxSpeed']);
                return '';
            },
        },
        { key: 'airflowCfm',     label: 'Airflow',         unit: 'CFM' },
        { key: 'noiseLevelDb',   label: 'Noise level',     unit: 'dB' },
        { key: 'voltage',        label: 'Voltage',         unit: 'V' },

        { type: 'sectionHeader', label: 'Physical' },
        {
            keys: ['sizeLength', 'sizeWidth', 'sizeHeight'],
            label: 'Dimensions', unit: 'mm',
            format: (v) => {
                if (v['sizeLength'] && v['sizeWidth'] && v['sizeHeight']) return `${v['sizeLength']} × ${v['sizeWidth']} × ${v['sizeHeight']}`;
                if (v['sizeLength'] && v['sizeWidth']) return `${v['sizeLength']} × ${v['sizeWidth']}`;
                return '';
            },
        },
        { key: 'weight',         label: 'Weight',          unit: 'g' },
        { key: 'wattage',        label: 'Power draw',      unit: 'W' },
    ],

    default: [],
};
