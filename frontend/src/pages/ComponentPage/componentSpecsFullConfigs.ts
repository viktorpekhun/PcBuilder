import type { TFunction } from 'i18next';

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
function makeLoc(lang: string) {
    return function loc(v: unknown): string {
        if (!v || typeof v !== 'object') return '';
        const o = v as Record<string, unknown>;
        const primary = lang.startsWith('uk') ? 'uk' : 'en';
        const fallback = primary === 'uk' ? 'en' : 'uk';
        return String(o[primary] ?? o[fallback] ?? '');
    };
}

export function getComponentSpecFullConfigs(t: TFunction, lang: string): Record<string, FullSpecConfig[]> {
    const s = (key: string) => t(`components.componentPage.specs.${key}`);
    const loc = makeLoc(lang);

    return {

        // ── CPU ─────────────────────────────────────────────────────
        Cpu: [
            { type: 'sectionHeader', label: s('coreArch') },
            { key: 'brand',          label: s('brand'),         unit: '' },
            { key: 'socket',         label: s('socket'),        unit: '' },
            { key: 'basicFrequency', label: s('baseFreq'),      unit: 'GHz' },
            { key: 'maxFrequency',   label: s('maxBoost'),      unit: 'GHz' },
            { key: 'cores',          label: s('cores'),         unit: '' },
            { key: 'threads',        label: s('threads'),       unit: '' },
            { key: 'cache',          label: s('l3Cache'),       unit: 'MB' },
            { key: 'techprocess',    label: s('processNode'),   unit: 'nm' },

            { type: 'sectionHeader', label: s('other') },
            { key: 'dimmType',       label: s('memType'),       unit: '' },
            { key: 'tdp',            label: s('tdp'),           unit: 'W' },
            { key: 'integratedGraphics', label: s('hasIgpu'),   unit: '' },
            { key: 'complectation',  label: s('packaging'),     unit: '' },
        ],

        // ── GPU ─────────────────────────────────────────────────────
        Gpu: [
            { type: 'sectionHeader', label: s('chip') },
            { key: 'brand',           label: s('brand'),        unit: '' },
            { key: 'gpuManufacturer', label: s('chipVendor'),   unit: '' },
            { key: 'gpuModel',        label: s('gpuModel'),     unit: '' },
            { key: 'maxFrequency',    label: s('boostClock'),   unit: 'MHz' },
            { key: 'cudaCores',       label: s('shaderUnits'),  unit: '' },

            { type: 'sectionHeader', label: s('memory') },
            { key: 'memory',          label: s('vram'),         unit: 'GB' },
            { key: 'memoryType',      label: s('memType'),      unit: '' },
            { key: 'memoryBus',       label: s('memBus'),       unit: 'bit' },
            { key: 'memorySpeed',     label: s('memSpeed'),     unit: 'GB/s' },

            { type: 'sectionHeader', label: s('interfaceSection') },
            {
                keys: ['pcleVersion', 'pcleLane'],
                label: s('pcieIface'), unit: '',
                format: (v) => {
                    if (v['pcleVersion'] && v['pcleLane']) return `PCIe ${v['pcleVersion']}.0 x${v['pcleLane']}`;
                    if (v['pcleVersion']) return `PCIe ${v['pcleVersion']}.0`;
                    return '';
                },
            },
            {
                key: 'gpuPowerConnectors', label: s('powerConn'), unit: '', isList: true,
                formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
                formatList: (items) => items.join(', '),
            },

            { type: 'sectionHeader', label: s('powerAndSize') },
            { key: 'wattage',         label: s('tdp'),          unit: 'W' },
            { key: 'psuReccomended',  label: s('recPsu'),       unit: 'W' },
            {
                keys: ['sizeLength', 'sizeWidth', 'sizeHeight'],
                label: s('dimensions'), unit: 'mm',
                format: (v) => {
                    if (v['sizeLength'] && v['sizeWidth'] && v['sizeHeight']) return `${v['sizeLength']} × ${v['sizeWidth']} × ${v['sizeHeight']}`;
                    if (v['sizeLength'] && v['sizeWidth']) return `${v['sizeLength']} × ${v['sizeWidth']}`;
                    return '';
                },
            },
        ],

        // ── RAM ─────────────────────────────────────────────────────
        Ram: [
            { type: 'sectionHeader', label: s('specifications') },
            { key: 'brand',          label: s('brand'),         unit: '' },
            { key: 'type',           label: s('memType'),       unit: '' },
            { key: 'frequency',      label: s('speed'),         unit: 'MHz' },
            {
                keys: ['capacity', 'moduleQuantity'],
                label: s('capacity'), unit: '',
                format: (v) => {
                    if (v['capacity'] && v['moduleQuantity']) return `${v['moduleQuantity']}× ${v['capacity']} GB`;
                    if (v['capacity']) return `${v['capacity']} GB`;
                    return '';
                },
            },
            { key: 'timings',        label: s('timings'),       unit: '' },
            { key: 'voltage',        label: s('voltage'),       unit: 'V' },

            { type: 'sectionHeader', label: s('features') },
            { key: 'xmp',            label: s('xmp'),           unit: '' },
            { key: 'expo',           label: s('expo'),          unit: '' },
            { key: 'ecc',            label: s('ecc'),           unit: '' },
            { key: 'bufferization',  label: s('buffering'),     unit: '' },
            {
                keys: ['color'],
                label: s('color'), unit: '',
                format: (v) => loc(v['color']),
            },

            { type: 'sectionHeader', label: s('power') },
            { key: 'wattage',        label: s('powerDraw'),     unit: 'W' },
        ],

        // ── Motherboard ─────────────────────────────────────────────
        Motherboard: [
            { type: 'sectionHeader', label: s('platform') },
            { key: 'brand',          label: s('brand'),         unit: '' },
            { key: 'socket',         label: s('socket'),        unit: '' },
            { key: 'chipset',        label: s('chipset'),       unit: '' },
            { key: 'formFactor',     label: s('formFactor'),    unit: '' },
            {
                keys: ['sizeDimentions'],
                label: s('size'), unit: 'mm',
                format: (v) => v['sizeDimentions']
                    ? String(v['sizeDimentions']).replace(/\s*мм\s*/gi, '').replace(/[хx]/gi, '×')
                    : '',
            },

            { type: 'sectionHeader', label: s('memory') },
            { key: 'dimmType',       label: s('ramType'),       unit: '' },
            { key: 'dimmSlots',      label: s('dimmSlots'),     unit: '' },
            { key: 'dimmFrequency',  label: s('maxFreq'),       unit: 'MHz' },
            { key: 'dimmCapacity',   label: s('maxCapacity'),   unit: 'GB' },

            { type: 'sectionHeader', label: s('expansionSlots') },
            {
                key: 'pcleSlots', label: s('pcieX16'), unit: '', isList: true,
                formatItem: (item) => `PCIe ${item['version']}.0 x${item['lane']}`,
                formatList: (items) => items.join(', '),
            },
            { key: 'pcleX1Quantity', label: s('pcieX1'),        unit: '' },
            {
                key: 'm2Slots', label: s('m2Slots'), unit: '', isList: true,
                formatItem: (item) => `PCIe ${item['version']}.0 x${item['lane']}`,
                formatList: (items) => items.join(', '),
            },
            { key: 'sata3Count',     label: s('sataPorts'),     unit: '' },

            { type: 'sectionHeader', label: s('power') },
            {
                keys: ['powerMotherboard'],
                label: s('atxConn'), unit: '',
                format: (v) => v['powerMotherboard'] ? `${v['powerMotherboard']}-pin` : '',
            },
            {
                key: 'cpuPowerConnectors', label: s('cpuPower'), unit: '', isList: true,
                formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
                formatList: (items) => items.join(', '),
            },

            { type: 'sectionHeader', label: s('connectivity') },
            { key: 'ethernet',       label: s('ethernet'),      unit: '' },
            { key: 'wifi',           label: s('wifi'),          unit: '' },
            { key: 'bluetooth',      label: s('bluetooth'),     unit: '' },
            { key: 'audio',          label: s('audioCodec'),    unit: '' },
            { key: 'videoPorts',     label: s('videoOutputs'),  unit: '' },

            { type: 'sectionHeader', label: s('rearPorts') },
            {
                key: 'rearPorts', label: '', unit: '', isList: true, renderAsSeparateRows: true,
                getLabelFromItem: (item) => String(item['type']),
                getValueFromItem: (item) => item['quantity'],
            },

            { type: 'sectionHeader', label: s('internalConn') },
            {
                key: 'innerPorts', label: '', unit: '', isList: true, renderAsSeparateRows: true,
                getLabelFromItem: (item) => String(item['type']),
                getValueFromItem: (item) => item['value'],
            },
        ],

        // ── CPU Cooler ───────────────────────────────────────────────
        CpuCooler: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',              label: s('brand'),          unit: '' },
            { key: 'type',               label: s('coolingType'),    unit: '' },
            {
                keys: ['radiatorMaterial'],
                label: s('radiator'), unit: '',
                format: (v) => loc(v['radiatorMaterial']),
            },
            { key: 'fanCount',           label: s('fanCount'),       unit: '' },
            { key: 'fanSize',            label: s('fanSize'),        unit: 'mm' },
            { key: 'powerConnector',     label: s('connector'),      unit: '' },
            { key: 'speedControl',       label: s('speedControl'),   unit: '' },

            { type: 'sectionHeader', label: s('performance') },
            { key: 'maxPowerDissipation', label: s('tdpRating'),     unit: 'W' },
            { key: 'maxSpeed',           label: s('maxFanSpeed'),    unit: 'RPM' },
            { key: 'minSpeed',           label: s('minFanSpeed'),    unit: 'RPM' },
            { key: 'noiseLevelDb',       label: s('noiseLevel'),     unit: 'dB' },
            { key: 'airflowCfm',         label: s('airflow'),        unit: 'CFM' },

            { type: 'sectionHeader', label: s('compatibility') },
            {
                key: 'cpuCoolerSockets', label: s('supportedSockets'), unit: '', isList: true,
                formatItem: (item) => String(item['socketType']),
                formatList: (items) => items.join(', '),
            },

            { type: 'sectionHeader', label: s('dimsAndPower') },
            {
                keys: ['length', 'width', 'height'],
                label: s('dimensions'), unit: 'mm',
                format: (v) => {
                    if (v['length'] && v['width'] && v['height']) return `${v['length']} × ${v['width']} × ${v['height']}`;
                    if (v['length'] && v['width']) return `${v['length']} × ${v['width']}`;
                    return '';
                },
            },
            { key: 'weight',             label: s('weight'),         unit: 'g' },
            { key: 'voltage',            label: s('voltage'),        unit: 'V' },
            { key: 'wattage',            label: s('powerDraw'),      unit: 'W' },
            { key: 'lifespan',           label: s('lifespan'),       unit: 'h' },
        ],

        // ── PC Case ──────────────────────────────────────────────────
        PcCase: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',          label: s('brand'),          unit: '' },
            { key: 'sizeStandard',   label: s('caseClass'),      unit: '' },
            {
                key: 'pcCaseFormFactors', label: s('supportedFF'), unit: '', isList: true,
                formatItem: (item) => String(item['name']),
                formatList: (items) => items.join(', '),
            },
            {
                keys: ['psuLocation'],
                label: s('psuLocation'), unit: '',
                format: (v) => loc(v['psuLocation']),
            },
            { key: 'hasDustFilters', label: s('dustFilters'),    unit: '' },

            { type: 'sectionHeader', label: s('clearances') },
            { key: 'maxGpuLength',       label: s('maxGpuLength'),   unit: 'mm' },
            { key: 'maxCpuCoolerHeight', label: s('maxCoolerH'),     unit: 'mm' },

            { type: 'sectionHeader', label: s('bays') },
            { key: 'slot25Quant',        label: s('bays25'),          unit: '' },
            { key: 'slot35Quant',        label: s('bays35'),          unit: '' },
            { key: 'slot525Quant',       label: s('bays525'),         unit: '' },
            { key: 'expansionSlotQuant', label: s('expansionSlots'),  unit: '' },

            { type: 'sectionHeader', label: s('fans') },
            {
                keys: ['builtInFans'],
                label: s('includedFans'), unit: '',
                format: (v) => loc(v['builtInFans']),
                fullWidth: true,
            },
            {
                keys: ['additionalFanPlaces'],
                label: s('fanMounts'), unit: '',
                format: (v) => loc(v['additionalFanPlaces']),
                fullWidth: true,
            },
            {
                key: 'pcCaseFanLocations', label: s('fanLocations'), unit: '', isList: true,
                formatItem: (item) => {
                    const name = typeof item['name'] === 'object' ? loc(item['name']) : String(item['name'] ?? '');
                    return `${name} ${item['fanSize']}mm ×${item['maxFans']}`;
                },
                formatList: (items) => items.join(', '),
                fullWidth: true,
            },

            { type: 'sectionHeader', label: s('ioPanel') },
            { key: 'usb',            label: s('frontUsb'),       unit: '' },
            { key: 'hasHeadphones',  label: s('headphoneJack'),  unit: '' },
            { key: 'hasMicrophone',  label: s('micJack'),        unit: '' },

            { type: 'sectionHeader', label: s('physical') },
            {
                keys: ['sizeDimentions'],
                label: s('dimensions'), unit: 'mm',
                format: (v) => v['sizeDimentions']
                    ? String(v['sizeDimentions']).replace(/\s*мм\s*/gi, '')
                    : '',
            },
            { key: 'weight',         label: s('weight'),         unit: 'kg' },
        ],

        // ── Power Supply ─────────────────────────────────────────────
        PowerSupply: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',              label: s('brand'),         unit: '' },
            { key: 'formFactor',         label: s('formFactor'),    unit: '' },
            { key: 'wattage',            label: s('wattage'),       unit: 'W' },
            {
                keys: ['modularity'],
                label: s('modularity'), unit: '',
                format: (v) => loc(v['modularity']),
            },

            { type: 'sectionHeader', label: s('efficiency') },
            { key: 'efficiencyStandart', label: s('80plusRating'),  unit: '' },
            {
                keys: ['efficiencyPercent'],
                label: s('efficiencyPct'), unit: '',
                format: (v) => v['efficiencyPercent'] ? `${v['efficiencyPercent']}%` : '',
            },
            { key: 'hasApcf',            label: s('activePfc'),     unit: '' },

            { type: 'sectionHeader', label: s('connectors') },
            {
                key: 'powerSupplyMotherboardPowerConnectors', label: s('motherboard'), unit: '', isList: true,
                formatItem: (item) => `${item['quantity']}× ${item['pins']}-pin`,
                formatList: (items) => items.join(', '),
            },
            {
                key: 'powerSupplyCpuPowerConnectors', label: s('cpu'), unit: '', isList: true,
                formatItem: (item) => {
                    const base = `${item['quantity']}× ${item['pins']}`;
                    return item['additionalPins'] != null ? `${base}+${item['additionalPins']}-pin` : `${base}-pin`;
                },
                formatList: (items) => items.join(', '),
            },
            {
                key: 'powerSupplyGpuPowerConnectors', label: s('gpu'), unit: '', isList: true,
                formatItem: (item) => {
                    const base = `${item['quantity']}× ${item['pins']}`;
                    return item['additionalPins'] != null ? `${base}+${item['additionalPins']}-pin` : `${base}-pin`;
                },
                formatList: (items) => items.join(', '),
            },
            { key: 'molexCount',         label: s('molex'),         unit: '' },
            { key: 'sataCount',          label: s('sata'),          unit: '' },
            { key: 'fddCount',           label: s('fdd'),           unit: '' },

            { type: 'sectionHeader', label: s('input') },
            {
                keys: ['inputMinVoltage', 'inputMaxVoltage'],
                label: s('inputVoltage'), unit: 'V',
                format: (v) => {
                    if (v['inputMinVoltage'] && v['inputMaxVoltage']) return `${v['inputMinVoltage']} – ${v['inputMaxVoltage']}`;
                    if (v['inputMinVoltage']) return String(v['inputMinVoltage']);
                    return '';
                },
            },

            { type: 'sectionHeader', label: s('physical') },
            { key: 'noiseLevelMaxDb',    label: s('maxNoise'),      unit: 'dB' },
            { key: 'size',               label: s('dimensions'),    unit: '' },
        ],

        // ── SSD ──────────────────────────────────────────────────────
        Ssd: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',          label: s('brand'),          unit: '' },
            { key: 'capacity',       label: s('capacity'),       unit: 'GB' },
            { key: 'interface',      label: s('interface'),      unit: '' },
            { key: 'formFactor',     label: s('formFactor'),     unit: '' },
            { key: 'nandType',       label: s('nandType'),       unit: '' },
            { key: 'isTrimmSupported', label: s('trim'),         unit: '' },

            { type: 'sectionHeader', label: s('performance') },
            {
                keys: ['maxReadSpeed', 'maxWriteSpeed'],
                label: s('seqReadWrite'), unit: 'MB/s',
                format: (v) => {
                    if (v['maxReadSpeed'] && v['maxWriteSpeed']) return `${v['maxReadSpeed']} / ${v['maxWriteSpeed']}`;
                    if (v['maxReadSpeed']) return String(v['maxReadSpeed']);
                    return '';
                },
            },
            {
                keys: ['randomReadSpeed', 'randomWriteSpeed'],
                label: s('randReadWrite'), unit: 'IOPS',
                format: (v) => {
                    if (v['randomReadSpeed'] && v['randomWriteSpeed']) return `${v['randomReadSpeed']} / ${v['randomWriteSpeed']}`;
                    if (v['randomReadSpeed']) return String(v['randomReadSpeed']);
                    return '';
                },
            },
            { key: 'writingRecource', label: s('writeEndurance'), unit: 'TBW' },
            { key: 'averageLifeTime', label: s('mtbf'),           unit: 'h' },

            { type: 'sectionHeader', label: s('physical') },
            { key: 'size',           label: s('dimensions'),     unit: '' },
            { key: 'weight',         label: s('weight'),         unit: 'g' },
            { key: 'wattage',        label: s('powerDraw'),      unit: 'W' },
        ],

        // ── HDD ──────────────────────────────────────────────────────
        Hdd: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',          label: s('brand'),          unit: '' },
            { key: 'capacity',       label: s('capacity'),       unit: 'GB' },
            { key: 'interface',      label: s('interface'),      unit: '' },
            { key: 'formFactor',     label: s('formFactor'),     unit: '' },

            { type: 'sectionHeader', label: s('performance') },
            { key: 'spindleSpeed',   label: s('spindleSpeed'),   unit: 'RPM' },
            { key: 'cache',          label: s('cache'),          unit: 'MB' },
            { key: 'speed',          label: s('transferSpeed'),  unit: 'MB/s' },
            { key: 'writingTechnology', label: s('writeTech'),   unit: '' },

            { type: 'sectionHeader', label: s('physical') },
            { key: 'noiceDb',        label: s('noiseLevel'),     unit: 'dB' },
            { key: 'wattage',        label: s('powerDraw'),      unit: 'W' },
        ],

        // ── Fan ──────────────────────────────────────────────────────
        Fan: [
            { type: 'sectionHeader', label: s('general') },
            { key: 'brand',          label: s('brand'),          unit: '' },
            { key: 'moduleCount',    label: s('packCount'),      unit: '' },
            {
                keys: ['bearingType'],
                label: s('bearingType'), unit: '',
                format: (v) => loc(v['bearingType']),
            },
            { key: 'connector',      label: s('connector'),      unit: '' },
            { key: 'speedControl',   label: s('speedControl'),   unit: '' },
            {
                keys: ['color'],
                label: s('color'), unit: '',
                format: (v) => loc(v['color']),
            },

            { type: 'sectionHeader', label: s('performance') },
            {
                keys: ['minSpeed', 'maxSpeed'],
                label: s('fanSpeed'), unit: 'RPM',
                format: (v) => {
                    if (v['minSpeed'] && v['maxSpeed']) return `${v['minSpeed']} – ${v['maxSpeed']}`;
                    if (v['maxSpeed']) return String(v['maxSpeed']);
                    return '';
                },
            },
            { key: 'airflowCfm',     label: s('airflow'),        unit: 'CFM' },
            { key: 'noiseLevelDb',   label: s('noiseLevel'),     unit: 'dB' },
            { key: 'voltage',        label: s('voltage'),        unit: 'V' },

            { type: 'sectionHeader', label: s('physical') },
            {
                keys: ['sizeLength', 'sizeWidth', 'sizeHeight'],
                label: s('dimensions'), unit: 'mm',
                format: (v) => {
                    if (v['sizeLength'] && v['sizeWidth'] && v['sizeHeight']) return `${v['sizeLength']} × ${v['sizeWidth']} × ${v['sizeHeight']}`;
                    if (v['sizeLength'] && v['sizeWidth']) return `${v['sizeLength']} × ${v['sizeWidth']}`;
                    return '';
                },
            },
            { key: 'weight',         label: s('weight'),         unit: 'g' },
            { key: 'wattage',        label: s('powerDraw'),      unit: 'W' },
        ],

        default: [],
    };
}

// Keep the old export for any code that still imports it (will be removed after migration)
export const componentSpecFullConfigs: Record<string, FullSpecConfig[]> = {};
