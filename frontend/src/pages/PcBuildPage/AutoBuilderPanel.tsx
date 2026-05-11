import { useState } from 'react';
import { buildService } from '../../api/build.service';
import type { IAutoBuildComponents, IAutoBuildResult, UsageScenario } from '../../types/build.types';
import { Button } from '../../components/Button/Button';
import styles from './AutoBuilderPanel.module.css';

// --- Icons ---

const CloseIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
        <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/>
    </svg>
);

const ChevronDown = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
        <path fillRule="evenodd" d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z"/>
    </svg>
);

const ChevronUp = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
        <path fillRule="evenodd" d="M7.646 4.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1-.708.708L8 5.707l-5.646 5.647a.5.5 0 0 1-.708-.708l6-6z"/>
    </svg>
);

// --- Scenario definitions ---

type ScenarioConfig = { value: UsageScenario; label: string; icon: JSX.Element };

const SCENARIOS: ScenarioConfig[] = [
    {
        value: 'Gaming',
        label: 'Ігровий',
        icon: (
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" viewBox="0 0 16 16">
                <path d="M11.5 6.027a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0zm-1.5 1.5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1zm2.5-.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0zm-1.5 1.5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1zm-6.5-3h1v1h1v1h-1v1h-1v-1h-1v-1h1v-1z"/>
                <path d="M3.051 3.26a.5.5 0 0 1 .354-.613l1.932-.518a.5.5 0 0 1 .62.39c.655-.079 1.35-.117 2.043-.117.72 0 1.443.041 2.12.126a.5.5 0 0 1 .622-.399l1.932.518a.5.5 0 0 1 .306.729c.14.09.266.19.373.297.408.408.78 1.05 1.095 1.772.32.733.599 1.591.805 2.466.206.875.34 1.78.364 2.606.024.816-.059 1.602-.328 2.21a1.42 1.42 0 0 1-1.445.83c-.636-.067-1.115-.394-1.513-.773-.245-.232-.496-.526-.739-.808-.126-.148-.25-.292-.368-.423-.728-.804-1.597-1.527-3.224-1.527-1.627 0-2.496.723-3.224 1.527-.119.131-.242.275-.368.423-.243.282-.494.575-.739.808-.398.38-.877.706-1.513.773a1.42 1.42 0 0 1-1.445-.83c-.27-.608-.352-1.395-.329-2.21.024-.826.16-1.73.365-2.606.206-.875.486-1.733.805-2.466.315-.722.687-1.364 1.094-1.772a2.34 2.34 0 0 1 .433-.335z"/>
            </svg>
        ),
    },
    {
        value: 'Office',
        label: 'Офісний',
        icon: (
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" viewBox="0 0 16 16">
                <path d="M0 1.5A1.5 1.5 0 0 1 1.5 0h13A1.5 1.5 0 0 1 16 1.5v13a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 0 14.5v-13zM1.5 1a.5.5 0 0 0-.5.5V5h1V1H1.5zM1 14.5a.5.5 0 0 0 .5.5H5v-4H1v3.5zm4 .5h6v-4H5v4zm7 0h.5a.5.5 0 0 0 .5-.5V11h-1v4zm1-5V6h-1v4h1zm0-5V1.5a.5.5 0 0 0-.5-.5H11v4h1zM10 1H6v4h4V1zM5 1H1v4h4V1zm0 5H1v4h4V6zm1 0v4h4V6H6zm5 0v4h4V6h-4z"/>
            </svg>
        ),
    },
    {
        value: 'ContentCreation',
        label: 'Контент',
        icon: (
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" viewBox="0 0 16 16">
                <path d="M0 5a2 2 0 0 1 2-2h7.5a2 2 0 0 1 1.983 1.738l3.11-1.382A1 1 0 0 1 16 4.269v7.462a1 1 0 0 1-1.406.913l-3.111-1.382A2 2 0 0 1 9.5 13H2a2 2 0 0 1-2-2V5z"/>
            </svg>
        ),
    },
    {
        value: 'Workstation',
        label: 'Робоча станція',
        icon: (
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" viewBox="0 0 16 16">
                <path d="M0 4s0-2 2-2h12s2 0 2 2v6s0 2-2 2h-4c0 .667.083 1.167.25 1.5H11a.5.5 0 0 1 0 1H5a.5.5 0 0 1 0-1h.75c.167-.333.25-.833.25-1.5H2s-2 0-2-2V4zm1.398-.855a.758.758 0 0 0-.254.302A1.46 1.46 0 0 0 1 4.01V10c0 .325.078.502.145.602.07.105.17.188.302.254a1.464 1.464 0 0 0 .538.143L2.01 11H14c.325 0 .502-.078.602-.145a.758.758 0 0 0 .254-.302 1.464 1.464 0 0 0 .143-.538L15 9.99V4c0-.325-.078-.502-.145-.602a.757.757 0 0 0-.302-.254A1.46 1.46 0 0 0 13.99 3H2c-.325 0-.502.078-.602.145z"/>
            </svg>
        ),
    },
    {
        value: 'Budget',
        label: 'Бюджетний',
        icon: (
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor" viewBox="0 0 16 16">
                <path d="M1 3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1H1zm7 8a2 2 0 1 0 0-4 2 2 0 0 0 0 4z"/>
                <path d="M0 5a1 1 0 0 1 1-1h14a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H1a1 1 0 0 1-1-1V5zm3 0a2 2 0 0 1-2 2v4a2 2 0 0 1 2 2h10a2 2 0 0 1 2-2V7a2 2 0 0 1-2-2H3z"/>
            </svg>
        ),
    },
];

const FORM_FACTORS = ['ATX', 'Micro-ATX', 'Mini-ITX', 'E-ATX'];

const COMPONENT_LABELS: { key: keyof IAutoBuildComponents; label: string }[] = [
    { key: 'cpu', label: 'Процесор' },
    { key: 'gpu', label: 'Відеокарта' },
    { key: 'motherboard', label: 'Материнська Плата' },
    { key: 'ram', label: "Оперативна Пам'ять" },
    { key: 'ssd', label: 'SSD Диск' },
    { key: 'hdd', label: 'HDD Диск' },
    { key: 'powerSupply', label: 'Блок живлення' },
    { key: 'cpuCooler', label: 'Кулер Процесора' },
    { key: 'pcCase', label: 'Корпус ПК' },
    { key: 'fan', label: 'Вентилятори' },
];

const quantityKey: Partial<Record<keyof IAutoBuildComponents, keyof IAutoBuildComponents>> = {
    ram: 'ramQuantity',
    ssd: 'ssdQuantity',
    hdd: 'hddQuantity',
    fan: 'fanQuantity',
};

interface AutoBuilderPanelProps {
    isOpen: boolean;
    hasExistingComponents: boolean;
    onClose: () => void;
    onApply: (components: IAutoBuildComponents) => void;
}

interface FormState {
    budget: string;
    scenario: UsageScenario;
    isStrictBudget: boolean;
    isFutureProof: boolean;
    formFactor: string | null;
}

const DEFAULT_FORM: FormState = {
    budget: '',
    scenario: 'Gaming',
    isStrictBudget: false,
    isFutureProof: false,
    formFactor: null,
};

export default function AutoBuilderPanel({ isOpen, hasExistingComponents, onClose, onApply }: AutoBuilderPanelProps) {
    const [form, setForm] = useState<FormState>(DEFAULT_FORM);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<IAutoBuildResult | null>(null);
    const [extraOpen, setExtraOpen] = useState(false);

    const handleGenerate = async () => {
        const budget = parseFloat(form.budget);
        if (!form.budget || isNaN(budget) || budget <= 0) {
            setError('Введіть коректний бюджет.');
            return;
        }
        setError(null);
        setIsLoading(true);
        try {
            const { data } = await buildService.autoBuild({
                budget,
                preferredUse: form.scenario,
                isStrictBudget: form.isStrictBudget,
                isFutureProof: form.isFutureProof,
                ...(form.formFactor ? { preferredFormFactor: form.formFactor } : {}),
            });
            setResult(data);
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message ?? 'Не вдалося підібрати компоненти. Спробуйте змінити параметри.';
            setError(msg);
        } finally {
            setIsLoading(false);
        }
    };

    const handleReset = () => {
        setResult(null);
        setError(null);
    };

    const handleApply = () => {
        if (result) onApply(result.components);
    };

    const scenarioLabel = SCENARIOS.find(s => s.value === form.scenario)?.label ?? '';

    return (
        <>
            {isOpen && <div className={styles.overlay} onClick={onClose} />}
            <div className={`${styles.panel} ${isOpen ? styles.open : ''}`}>
                <div className={styles.header}>
                    <span className={styles.title}>Автопідбір ПК</span>
                    <button className={styles.closeBtn} onClick={onClose} aria-label="Закрити">
                        <CloseIcon />
                    </button>
                </div>

                <div className={styles.body}>
                    {result ? (
                        // --- Result state ---
                        <>
                            <div className={styles.formSummaryChip}>
                                <span>{scenarioLabel}</span>
                                <span className={styles.chipDot}>·</span>
                                <span>{parseFloat(form.budget).toLocaleString('uk-UA')} грн</span>
                                {form.isStrictBudget && <><span className={styles.chipDot}>·</span><span>Строгий</span></>}
                                {form.isFutureProof && <><span className={styles.chipDot}>·</span><span>На майбутнє</span></>}
                                {form.formFactor && <><span className={styles.chipDot}>·</span><span>{form.formFactor}</span></>}
                            </div>

                            <div className={styles.resultList}>
                                {COMPONENT_LABELS.map(({ key, label }) => {
                                    const info = result.components[key];
                                    if (!info || typeof info !== 'object') return null;
                                    const qKey = quantityKey[key];
                                    const qty = qKey ? (result.components[qKey] as number) : 1;
                                    const price = (info.averagePrice ?? 0) * qty;
                                    return (
                                        <div key={key} className={styles.resultRow}>
                                            <span className={styles.resultType}>
                                                {label}{qty > 1 ? ` ×${qty}` : ''}
                                            </span>
                                            <span className={styles.resultName}>{info.name}</span>
                                            <span className={styles.resultPrice}>
                                                {price > 0 ? `${price.toLocaleString('uk-UA')} грн` : '—'}
                                            </span>
                                        </div>
                                    );
                                })}
                            </div>

                            <div className={styles.resultTotal}>
                                <span>Разом</span>
                                <span className={styles.totalPrice}>
                                    {result.totalPrice.toLocaleString('uk-UA')} грн
                                </span>
                            </div>

                            {hasExistingComponents && (
                                <div className={styles.overwriteWarning}>
                                    Поточні компоненти будуть замінені
                                </div>
                            )}

                            <div className={styles.resultActions}>
                                <Button variant="primary" size="md" onClick={handleApply}>
                                    Застосувати
                                </Button>
                                <Button variant="outline-secondary" size="md" onClick={handleReset}>
                                    Спробувати знову
                                </Button>
                            </div>
                        </>
                    ) : (
                        // --- Input state ---
                        <>
                            <label className={styles.fieldLabel}>Бюджет (грн)</label>
                            <input
                                className={styles.budgetInput}
                                type="number"
                                min={0}
                                placeholder="Наприклад: 30000"
                                value={form.budget}
                                disabled={isLoading}
                                onChange={e => setForm(f => ({ ...f, budget: e.target.value }))}
                            />

                            <label className={styles.fieldLabel}>Сценарій використання</label>
                            <div className={styles.scenarioGrid}>
                                {SCENARIOS.map(s => (
                                    <button
                                        key={s.value}
                                        className={`${styles.scenarioCard} ${form.scenario === s.value ? styles.scenarioSelected : ''}`}
                                        disabled={isLoading}
                                        onClick={() => setForm(f => ({ ...f, scenario: s.value }))}
                                    >
                                        <span className={styles.scenarioIcon}>{s.icon}</span>
                                        <span className={styles.scenarioLabel}>{s.label}</span>
                                    </button>
                                ))}
                            </div>

                            <div className={styles.toggleRow}>
                                <label className={styles.toggleLabel}>
                                    <input
                                        type="checkbox"
                                        checked={form.isStrictBudget}
                                        disabled={isLoading}
                                        onChange={e => setForm(f => ({ ...f, isStrictBudget: e.target.checked }))}
                                    />
                                    Строгий бюджет
                                </label>
                                <span className={styles.toggleHint}>Не перевищувати бюджет</span>
                            </div>

                            <div className={styles.toggleRow}>
                                <label className={styles.toggleLabel}>
                                    <input
                                        type="checkbox"
                                        checked={form.isFutureProof}
                                        disabled={isLoading}
                                        onChange={e => setForm(f => ({ ...f, isFutureProof: e.target.checked }))}
                                    />
                                    На майбутнє
                                </label>
                                <span className={styles.toggleHint}>Оптимізувати для апгрейду</span>
                            </div>

                            <button
                                className={styles.extraToggle}
                                onClick={() => setExtraOpen(o => !o)}
                                disabled={isLoading}
                            >
                                <span>Додаткові параметри</span>
                                {extraOpen ? <ChevronUp /> : <ChevronDown />}
                            </button>

                            {extraOpen && (
                                <div className={styles.extraSection}>
                                    <label className={styles.fieldLabel}>Форм-фактор</label>
                                    <div className={styles.chipRow}>
                                        <button
                                            className={`${styles.chip} ${form.formFactor === null ? styles.chipSelected : ''}`}
                                            disabled={isLoading}
                                            onClick={() => setForm(f => ({ ...f, formFactor: null }))}
                                        >
                                            Будь-який
                                        </button>
                                        {FORM_FACTORS.map(ff => (
                                            <button
                                                key={ff}
                                                className={`${styles.chip} ${form.formFactor === ff ? styles.chipSelected : ''}`}
                                                disabled={isLoading}
                                                onClick={() => setForm(f => ({ ...f, formFactor: ff }))}
                                            >
                                                {ff}
                                            </button>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {error && <div className={styles.errorMsg}>{error}</div>}

                            <Button
                                variant="primary"
                                size="md"
                                onClick={handleGenerate}
                                disabled={isLoading}
                            >
                                {isLoading ? 'Підбираємо компоненти...' : 'Згенерувати збірку'}
                            </Button>
                        </>
                    )}
                </div>
            </div>
        </>
    );
}
