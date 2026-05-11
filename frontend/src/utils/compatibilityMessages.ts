import type { ICompatibilityIssue } from "../types/build.types";

type Formatter = (params: Record<string, string>) => string;

const messages: Record<string, Formatter> = {
    SOCKET_MISMATCH: (p) =>
        `Сокет процесора (${p.CpuSocket}) не збігається з сокетом материнської плати (${p.MbSocket}).`,
    SOCKET_DATA_MISSING: () =>
        "Неможливо перевірити сумісність процесора та материнської плати — недостатньо даних.",

    COOLER_TDP_INSUFFICIENT: (p) =>
        `Розсіювання кулера ${p.CoolerName} (${p.CoolerTdp} Вт) менше за TDP процесора ${p.CpuName} (${p.CpuTdp} Вт).`,
    COOLER_TDP_DATA_MISSING: () =>
        "Неможливо перевірити сумісність кулера та процесора — недостатньо даних.",
    COOLER_SOCKET_MISMATCH: (p) =>
        `Кулер ${p.CoolerName} не сумісний із сокетом материнської плати (${p.MbSocket}).`,
    COOLER_SOCKET_DATA_MISSING: () =>
        "Неможливо перевірити сумісність кулера та материнської плати — недостатньо даних.",
    COOLER_CASE_FIT: (p) =>
        `Кулер ${p.CoolerName} не поміщається в корпус ${p.CaseName}.`,
    COOLER_CASE_DATA_MISSING: () =>
        "Неможливо перевірити сумісність кулера та корпусу — недостатньо даних.",

    GPU_PCIE_VERSION: (p) =>
        `Слоти PCIe материнської плати мають старішу версію, ніж потрібно для відеокарти ${p.GpuName}. Можлива знижена швидкість.`,
    GPU_PCIE_LANE: (p) =>
        `Слоти PCIe материнської плати мають меншу пропускну здатність, ніж потрібно для відеокарти ${p.GpuName}.`,
    GPU_LENGTH_EXCEEDS_CASE: (p) =>
        `Довжина відеокарти ${p.GpuName} (${p.GpuLength} мм) перевищує максимум корпуса ${p.CaseName} (${p.MaxLength} мм).`,
    GPU_LENGTH_DATA_MISSING: () =>
        "Неможливо перевірити сумісність відеокарти та корпусу — недостатньо даних.",

    RAM_TYPE_MISMATCH: (p) =>
        `Материнська плата не підтримує тип пам'яті ${p.RamType ?? ""}.`,
    RAM_FREQUENCY_EXCEEDED: (p) =>
        `Частота ОЗП ${p.RamName} (${p.RamFrequency} МГц) перевищує максимум материнської плати (${p.MbMaxFrequency} МГц).`,
    RAM_SLOTS_EXCEEDED: (p) =>
        `Материнська плата має лише ${p.MbSlots} слотів DIMM, потрібно ${p.ModuleCount}.`,
    RAM_CAPACITY_EXCEEDED: (p) =>
        `Загальний обсяг ОЗП (${p.TotalCapacity} ГБ) перевищує максимум (${p.MaxCapacity} ГБ).`,
    RAM_MIXED_FREQUENCIES: (p) =>
        `Модулі ОЗП мають різні частоти. Працюватимуть на ${p.MinFrequency} МГц замість ${p.MaxFrequency} МГц.`,

    SATA_SLOTS_MISSING: () => "Материнська плата не має SATA-слотів.",
    SATA_SLOTS_EXCEEDED: (p) =>
        `Кількість SATA-накопичувачів перевищує доступні слоти (${p.FreeSataSlots ?? p.MbSataSlots}).`,
    M2_SLOTS_MISSING: (p) =>
        `Неможливо перевірити SSD ${p.SsdName} — материнська плата не має M.2 слотів.`,
    M2_SLOTS_EXCEEDED: (p) =>
        `Кількість SSD M.2 (${p.SsdCount}) перевищує кількість M.2 слотів (${p.M2SlotCount}).`,
    NVME_VERSION_MISMATCH: (p) =>
        `Слоти M.2 материнської плати мають старішу версію, ніж SSD ${p.SsdName}. Можлива знижена швидкість.`,

    FAN_HEADERS_INSUFFICIENT: (p) =>
        `Материнська плата має лише ${p.MbHeaders ?? "0"} конекторів для вентиляторів, потрібно ${p.FanCount ?? "більше"}.`,
    FAN_CASE_SLOTS_INSUFFICIENT: (p) =>
        `Корпус ${p.CaseName} підтримує лише ${p.MaxSlots} вентиляторів розміру ${p.FanSize} мм, обрано ${p.FanCount}.`,
    FAN_CASE_DATA_MISSING: () =>
        "Неможливо перевірити сумісність вентиляторів та корпусу — недостатньо даних.",

    PSU_GPU_CONNECTOR_MISSING: (p) =>
        `Блок живлення ${p.PsuName} не має достатньо конекторів для відеокарти${p.MissingConnectors ? `: ${p.MissingConnectors}` : ""}.`,
    PSU_CPU_CONNECTOR_MISSING: (p) =>
        `Блок живлення ${p.PsuName} не має достатньо конекторів живлення процесора${p.MissingConnectors ? `: ${p.MissingConnectors}` : ""}.`,
    PSU_INSUFFICIENT: (p) =>
        `Потужність блоку живлення (${p.PsuWattage} Вт) недостатня для системи (${p.SystemWattage} Вт, ${p.LoadPercentage}% навантаження).`,
    PSU_OVERKILL: (p) =>
        `Блок живлення (${p.PsuWattage} Вт) значно потужніший, ніж потрібно (${p.SystemWattage} Вт, ${p.LoadPercentage}% навантаження).`,

    CPU_BOTTLENECK: (p) =>
        `Процесор ${p.CpuName} може обмежувати продуктивність відеокарти ${p.GpuName} (співвідношення ${p.Ratio}).`,
    GPU_BOTTLENECK: (p) =>
        `Відеокарта ${p.GpuName} надмірно потужна для процесора ${p.CpuName} (співвідношення ${p.Ratio}).`,
};

export function formatIssue(issue: ICompatibilityIssue): string {
    const formatter = messages[issue.code];
    return formatter ? formatter(issue.parameters) : issue.code;
}
