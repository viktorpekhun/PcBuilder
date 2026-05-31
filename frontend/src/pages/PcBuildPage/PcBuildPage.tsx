import { useLocation, useNavigate } from "react-router-dom";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import styles from "./PcBuildPage.module.css";
import { componentService } from "../../api/component.service";
import useAuth from "../../hooks/useAuth";
import { buildService } from "../../api/build.service";
import Toast from "../../components/Toast/Toast";
import CompatibilityCheck from "./CompatibilityCheck";
import AutoBuilderPanel from "./AutoBuilderPanel";
import PartsTable from "./PartsTable";
import CardsView from "./CardsView";
import JsonView from "./JsonView";
import TotalCard from "./TotalCard";
import PowerCard from "./PowerCard";
import InlineBanners from "./InlineBanners";
import { usePowerStats } from "./hooks";
import type { IAutoBuildComponents, IComponentsCompatibility, IPcBuildInput } from "../../types/build.types";
import type {
    SelectedComponents, ComponentDataState, EditingBuild,
    ToastState, SingleComponentData, MultiComponentData,
    MultiKey, SingleKey, SelectedOffer, SelectedMultiOffer,
} from "./types";
import {
    EMPTY_SELECTED, EMPTY_DATA,
    COMPONENT_TYPES, SINGLE_TYPES, MULTI_TYPES,
} from "./types";

type ViewMode = "table" | "cards" | "json";

function PcBuildPage() {
    const [selectedComponents, setSelectedComponents] = useState<SelectedComponents>({ ...EMPTY_SELECTED });
    const [componentData, setComponentData] = useState<ComponentDataState>({ ...EMPTY_DATA });
    const [initialLoadComplete, setInitialLoadComplete] = useState(false);
    const [loading, setLoading] = useState(true);
    const [editingBuild, setEditingBuild] = useState<EditingBuild | null>(null);
    const [saveLoading, setSaveLoading] = useState(false);
    const [toast, setToast] = useState<ToastState>({ visible: false, message: "", type: "success" });
    const [autoPanelOpen, setAutoPanelOpen] = useState(false);
    const [viewMode, setViewMode] = useState<ViewMode>("table");
    const [draftName, setDraftName] = useState<string>("");
    const [publishLoading, setPublishLoading] = useState(false);
    const [criticalCount, setCriticalCount] = useState(0);
    const [firstCriticalMessage, setFirstCriticalMessage] = useState<string | null>(null);
    const [selectedRow, setSelectedRow] = useState<string | null>(null);

    const navigate = useNavigate();
    const location = useLocation();
    const isInitialMount = useRef(true);
    const { auth } = useAuth();

    const handleCompatCounts = useCallback((counts: { criticalCount: number; warningCount: number; firstCriticalMessage: string | null }) => {
        setCriticalCount(counts.criticalCount);
        setFirstCriticalMessage(counts.firstCriticalMessage);
    }, []);

    // Load editing build
    useEffect(() => {
        try {
            const raw = localStorage.getItem("editingBuild");
            if (raw) setEditingBuild(JSON.parse(raw) as EditingBuild);
        } catch (err) {
            console.error("Error loading editing build data:", err);
        }
    }, []);

    // Load saved component IDs
    useEffect(() => {
        try {
            const raw = localStorage.getItem("selectedComponents");
            if (raw) setSelectedComponents(JSON.parse(raw) as SelectedComponents);
        } catch (err) {
            console.error("Error loading components from localStorage:", err);
        } finally {
            setInitialLoadComplete(true);
        }
    }, []);

    // Load draft name
    useEffect(() => {
        try {
            const raw = localStorage.getItem("composerDraftName");
            if (raw) setDraftName(raw);
        } catch (err) {
            console.error("Error loading draft name:", err);
        }
    }, []);

    // Persist draft name (debounced)
    useEffect(() => {
        const t = setTimeout(() => {
            try {
                if (draftName) localStorage.setItem("composerDraftName", draftName);
                else localStorage.removeItem("composerDraftName");
            } catch (err) {
                console.error("Error saving draft name:", err);
            }
        }, 300);
        return () => clearTimeout(t);
    }, [draftName]);

    // Fetch component data
    useEffect(() => {
        if (!initialLoadComplete) return;

        const fetchComponentData = async () => {
            setLoading(true);
            try {
                const newData: ComponentDataState = { ...EMPTY_DATA };

                for (const type of SINGLE_TYPES) {
                    const sel = selectedComponents[type.key];
                    if (sel?.componentId) {
                        try {
                            const { data } = await componentService.getById(type.apiType, sel.componentId);
                            (newData[type.key] as SingleComponentData) = {
                                ...data,
                                selectedOffer: {
                                    price: sel.price,
                                    storeName: sel.storeName,
                                    storeLogoUrl: sel.storeLogoUrl,
                                    productOfferUrl: sel.productOfferUrl,
                                    id: sel.offerId,
                                },
                            };
                        } catch (err) {
                            console.error(`Error fetching ${type.key}:`, err);
                        }
                    }
                }

                for (const type of MULTI_TYPES) {
                    const items = selectedComponents[type.key];
                    if (items.length === 0) continue;
                    const fetched: MultiComponentData[] = [];
                    for (const item of items) {
                        try {
                            const { data } = await componentService.getById(type.apiType, item.componentId);
                            fetched.push({
                                component: data,
                                componentId: item.componentId,
                                quantity: item.quantity,
                                price: item.price,
                                storeName: item.storeName,
                                storeLogoUrl: item.storeLogoUrl,
                                productOfferUrl: item.productOfferUrl,
                                offerId: item.offerId,
                            });
                        } catch (err) {
                            console.error(`Error fetching ${type.apiType}:`, err);
                        }
                    }
                    (newData[type.key] as MultiComponentData[]) = fetched;
                }

                setComponentData(newData);
            } catch (err) {
                console.error("Error fetching component data:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchComponentData();

        if (isInitialMount.current) isInitialMount.current = false;
    }, [selectedComponents, initialLoadComplete]);

    // Persist
    useEffect(() => {
        if (!initialLoadComplete) return;
        try {
            localStorage.setItem("selectedComponents", JSON.stringify(selectedComponents));
        } catch (err) {
            console.error("Error saving components to localStorage:", err);
        }
    }, [selectedComponents, initialLoadComplete]);

    // Clear edit on logout
    useEffect(() => {
        if (!auth?.accessToken && editingBuild) {
            localStorage.removeItem("editingBuild");
            setEditingBuild(null);
        }
    }, [auth, editingBuild]);

    // Lazy-fetch isPublished if entering edit mode without it
    useEffect(() => {
        if (!editingBuild?.id || editingBuild.isPublished !== undefined) return;
        let cancelled = false;
        (async () => {
            try {
                const { data } = await buildService.getBuildById(editingBuild.id);
                if (cancelled) return;
                setEditingBuild(prev => {
                    if (!prev || prev.id !== editingBuild.id) return prev;
                    const updated = { ...prev, isPublished: data.isPublished };
                    try { localStorage.setItem("editingBuild", JSON.stringify(updated)); } catch { /* ignore */ }
                    return updated;
                });
            } catch (err) {
                console.error("Error fetching build isPublished:", err);
            }
        })();
        return () => { cancelled = true; };
    }, [editingBuild?.id, editingBuild?.isPublished]);

    // --- Derived ---

    const hasAnySelected = useMemo(() =>
        Object.values(selectedComponents).some(v => v !== null && (Array.isArray(v) ? v.length > 0 : true)),
        [selectedComponents]);

    const totalPrice = useMemo(() => {
        let total = 0;
        for (const type of COMPONENT_TYPES) {
            if (!type.isMulti) {
                const sel = selectedComponents[type.key];
                if (sel) total += Number(sel.price) || 0;
            } else {
                for (const item of selectedComponents[type.key]) {
                    total += (Number(item.price) * item.quantity) || 0;
                }
            }
        }
        return total;
    }, [selectedComponents]);

    const partCount = useMemo(() => {
        let n = 0;
        for (const type of COMPONENT_TYPES) {
            if (!type.isMulti) {
                if (selectedComponents[type.key]) n++;
            } else {
                n += selectedComponents[type.key].reduce((a, i) => a + i.quantity, 0);
            }
        }
        return n;
    }, [selectedComponents]);

    const filledSlots = useMemo(() => {
        let n = 0;
        for (const type of COMPONENT_TYPES) {
            if (!type.isMulti) {
                if (selectedComponents[type.key]) n++;
            } else {
                if (selectedComponents[type.key].length > 0) n++;
            }
        }
        return n;
    }, [selectedComponents]);

    const powerStats = usePowerStats(componentData);

    if (loading && !initialLoadComplete) {
        return <div className={styles.loading}>Завантаження збірки...</div>;
    }

    // --- Handlers ---

    const removeSingle = (key: SingleKey) => {
        setSelectedComponents(prev => ({ ...prev, [key]: null }));
        if (selectedRow === key) setSelectedRow(null);
    };

    const removeMulti = (key: MultiKey, componentId: string) => {
        setSelectedComponents(prev => ({
            ...prev,
            [key]: prev[key].filter(item => item.componentId !== componentId),
        }));
        if (selectedRow === `${key}:${componentId}`) setSelectedRow(null);
    };

    const adjustQty = (key: MultiKey, componentId: string, change: number) => {
        setSelectedComponents(prev => {
            const arr = [...prev[key]];
            const index = arr.findIndex(item => item.componentId === componentId);
            if (index === -1) return prev;
            const newQuantity = arr[index]!.quantity + change;
            if (newQuantity <= 0) {
                return { ...prev, [key]: arr.filter(item => item.componentId !== componentId) };
            }
            arr[index] = { ...arr[index]!, quantity: newQuantity };
            return { ...prev, [key]: arr };
        });
    };

    const convertToCompatibilityFormat = (): IComponentsCompatibility => {
        const result: IComponentsCompatibility = {
            rams: selectedComponents.rams.map(i => ({ componentId: i.componentId, quantity: i.quantity })),
            ssds: selectedComponents.ssds.map(i => ({ componentId: i.componentId, quantity: i.quantity })),
            hdds: selectedComponents.hdds.map(i => ({ componentId: i.componentId, quantity: i.quantity })),
            fans: selectedComponents.fans.map(i => ({ componentId: i.componentId, quantity: i.quantity })),
        };
        if (selectedComponents.cpu) result.cpuId = selectedComponents.cpu.componentId;
        if (selectedComponents.gpu) result.gpuId = selectedComponents.gpu.componentId;
        if (selectedComponents.motherboard) result.motherboardId = selectedComponents.motherboard.componentId;
        if (selectedComponents.powerSupply) result.powerSupplyId = selectedComponents.powerSupply.componentId;
        if (selectedComponents.cpuCooler) result.cpuCoolerId = selectedComponents.cpuCooler.componentId;
        if (selectedComponents.pcCase) result.pcCaseId = selectedComponents.pcCase.componentId;
        return result;
    };

    const requireAuth = (callback: () => void) => {
        if (!auth?.accessToken) {
            navigate("/login", {
                state: { from: location.pathname, message: "Увійдіть щоб зберегти збірку." },
            });
            return;
        }
        callback();
    };

    const handleSaveBuild = async (asNew = false) => {
        requireAuth(async () => {
            if (!hasAnySelected) {
                setToast({ visible: true, message: "В збірці повинен бути хоча б один компонент.", type: "error" });
                return;
            }

            try {
                setSaveLoading(true);

                const multiPayload = (key: MultiKey) =>
                    componentData[key].map(item => ({
                        componentId: item.componentId,
                        offerId: item.offerId,
                        quantity: item.quantity || 1,
                    }));

                const name = editingBuild?.name || draftName || "untitled-build";

                const buildPayload: IPcBuildInput = {
                    name,
                    isPublished: false,
                    ...(selectedComponents.cpu && { cpuId: selectedComponents.cpu.componentId }),
                    ...(componentData.cpu?.selectedOffer?.id && { cpuOfferId: componentData.cpu.selectedOffer.id }),
                    ...(selectedComponents.motherboard && { motherboardId: selectedComponents.motherboard.componentId }),
                    ...(componentData.motherboard?.selectedOffer?.id && { motherboardOfferId: componentData.motherboard.selectedOffer.id }),
                    ...(selectedComponents.gpu && { gpuId: selectedComponents.gpu.componentId }),
                    ...(componentData.gpu?.selectedOffer?.id && { gpuOfferId: componentData.gpu.selectedOffer.id }),
                    ...(selectedComponents.powerSupply && { powerSupplyId: selectedComponents.powerSupply.componentId }),
                    ...(componentData.powerSupply?.selectedOffer?.id && { powerSupplyOfferId: componentData.powerSupply.selectedOffer.id }),
                    ...(selectedComponents.cpuCooler && { cpuCoolerId: selectedComponents.cpuCooler.componentId }),
                    ...(componentData.cpuCooler?.selectedOffer?.id && { cpuCoolerOfferId: componentData.cpuCooler.selectedOffer.id }),
                    ...(selectedComponents.pcCase && { pcCaseId: selectedComponents.pcCase.componentId }),
                    ...(componentData.pcCase?.selectedOffer?.id && { pcCaseOfferId: componentData.pcCase.selectedOffer.id }),
                    rams: multiPayload("rams"),
                    ssds: multiPayload("ssds"),
                    hdds: multiPayload("hdds"),
                    fans: multiPayload("fans"),
                };

                if (editingBuild?.id && !asNew) {
                    await buildService.updateBuild(editingBuild.id, buildPayload);
                } else {
                    await buildService.saveBuild(buildPayload);
                }

                if (editingBuild) {
                    localStorage.removeItem("editingBuild");
                    setEditingBuild(null);
                }

                localStorage.removeItem("composerDraftName");
                setDraftName("");

                setSaveLoading(false);
                setToast({ visible: true, message: "Збірку успішно збережено!", type: "success" });
                navigate("/user/builds", { state: { message: "Збірку успішно збережено!" } });
            } catch (err: unknown) {
                setSaveLoading(false);
                const msg = (err as { response?: { data?: { message?: string } } })
                    ?.response?.data?.message || "Не вдалося зберегти збірку. Будь ласка, спробуйте ще раз.";
                setToast({ visible: true, message: msg, type: "error" });
            }
        });
    };

    const togglePublish = async () => {
        if (!editingBuild?.id || publishLoading) return;
        const newState = !(editingBuild.isPublished ?? false);
        try {
            setPublishLoading(true);
            await buildService.publishBuild(editingBuild.id, newState);
            setEditingBuild(prev => {
                if (!prev) return prev;
                const updated = { ...prev, isPublished: newState };
                try { localStorage.setItem("editingBuild", JSON.stringify(updated)); } catch { /* ignore */ }
                return updated;
            });
            setToast({
                visible: true,
                message: newState ? "Збірку опубліковано" : "Збірку знято з публікації",
                type: "success",
            });
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message || "Не вдалося змінити статус публікації.";
            setToast({ visible: true, message: msg, type: "error" });
        } finally {
            setPublishLoading(false);
        }
    };

    const handleCancelEdit = () => {
        localStorage.removeItem("editingBuild");
        localStorage.removeItem("selectedComponents");
        localStorage.removeItem("editComponents");
        setEditingBuild(null);
        navigate("/user/builds");
    };

    const handleDiscardDraft = () => {
        localStorage.removeItem("selectedComponents");
        localStorage.removeItem("composerDraftName");
        setSelectedComponents({ ...EMPTY_SELECTED });
        setComponentData({ ...EMPTY_DATA });
        setDraftName("");
        setSelectedRow(null);
    };

    const handleApplyAutoBuild = (components: IAutoBuildComponents) => {
        const makeOffer = (id: string, price: number): SelectedOffer => ({
            componentId: id, offerId: "", price, storeName: "", storeLogoUrl: null, productOfferUrl: null,
        });
        const makeMultiOffer = (id: string, price: number, qty: number): SelectedMultiOffer => ({
            componentId: id, offerId: "", price, quantity: qty, storeName: "", storeLogoUrl: null, productOfferUrl: null,
        });

        setSelectedComponents({
            cpu: components.cpu ? makeOffer(components.cpu.id, components.cpu.averagePrice ?? 0) : null,
            gpu: components.gpu ? makeOffer(components.gpu.id, components.gpu.averagePrice ?? 0) : null,
            motherboard: components.motherboard ? makeOffer(components.motherboard.id, components.motherboard.averagePrice ?? 0) : null,
            powerSupply: components.powerSupply ? makeOffer(components.powerSupply.id, components.powerSupply.averagePrice ?? 0) : null,
            cpuCooler: components.cpuCooler ? makeOffer(components.cpuCooler.id, components.cpuCooler.averagePrice ?? 0) : null,
            pcCase: components.pcCase ? makeOffer(components.pcCase.id, components.pcCase.averagePrice ?? 0) : null,
            rams: components.ram && components.ramQuantity > 0
                ? [makeMultiOffer(components.ram.id, components.ram.averagePrice ?? 0, components.ramQuantity)] : [],
            ssds: components.ssd && components.ssdQuantity > 0
                ? [makeMultiOffer(components.ssd.id, components.ssd.averagePrice ?? 0, components.ssdQuantity)] : [],
            hdds: components.hdd && components.hddQuantity > 0
                ? [makeMultiOffer(components.hdd.id, components.hdd.averagePrice ?? 0, components.hddQuantity)] : [],
            fans: components.fan && components.fanQuantity > 0
                ? [makeMultiOffer(components.fan.id, components.fan.averagePrice ?? 0, components.fanQuantity)] : [],
        });
        setAutoPanelOpen(false);
    };

    const TOTAL_SLOTS = COMPONENT_TYPES.length;

    return (
        <section className={styles.page}>
            <AutoBuilderPanel
                isOpen={autoPanelOpen}
                hasExistingComponents={hasAnySelected}
                onClose={() => setAutoPanelOpen(false)}
                onApply={handleApplyAutoBuild}
            />
            {toast.visible && (
                <Toast
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast(prev => ({ ...prev, visible: false }))}
                    duration={5000}
                />
            )}

            <div className={styles.colMain}>
                {editingBuild && (
                    <div className={styles.editBanner}>
                        <div>
                            <span className={styles.label}>EDITING · </span>
                            <span className={styles.name}>{editingBuild.name}</span>
                        </div>
                        <button className={styles.btn} onClick={handleCancelEdit}>
                            Скасувати редагування
                        </button>
                    </div>
                )}

                <div className={styles.titleRow}>
                    <div style={{ minWidth: 0 }}>
                        <div className={styles.eyebrow}>
                            <span>BUILD · {editingBuild ? "EDITING" : "DRAFT"}</span>
                            {auth?.username && (
                                <>
                                    <span className={styles.sep}>·</span>
                                    <span className={styles.dim}>OWNER · @{auth.username}</span>
                                </>
                            )}
                        </div>
                        {editingBuild ? (
                            <input
                                className={styles.titleInput}
                                value={editingBuild.name}
                                onChange={(e) => setEditingBuild(prev => prev ? { ...prev, name: e.target.value } : prev)}
                                spellCheck={false}
                                aria-label="Назва збірки"
                            />
                        ) : (
                            <input
                                className={styles.titleInput}
                                value={draftName}
                                placeholder="untitled-build"
                                onChange={(e) => setDraftName(e.target.value)}
                                spellCheck={false}
                                aria-label="Назва збірки"
                            />
                        )}
                        <div className={styles.metaStrip}>
                            <span>{filledSlots} / {TOTAL_SLOTS} SLOTS</span>
                            <span className={styles.sep}>·</span>
                            <span>{partCount} {partCount === 1 ? "PART" : "PARTS"}</span>
                            <span className={styles.sep}>·</span>
                            <span>{totalPrice.toLocaleString("uk-UA")} ₴</span>
                        </div>
                    </div>
                    <div className={styles.titleActions}>
                        <button className={styles.btn} onClick={() => setAutoPanelOpen(true)}>
                            ⚙ Автопідбір
                        </button>
                    </div>
                </div>

                <div className={styles.actbar}>
                    <div className={styles.viewGroup}>
                        <button
                            className={`${styles.viewSeg} ${viewMode === "table" ? styles.viewSegOn : ""}`}
                            onClick={() => setViewMode("table")}
                        >Table</button>
                        <button
                            className={`${styles.viewSeg} ${viewMode === "cards" ? styles.viewSegOn : ""}`}
                            onClick={() => setViewMode("cards")}
                        >Cards</button>
                        <button
                            className={`${styles.viewSeg} ${viewMode === "json" ? styles.viewSegOn : ""}`}
                            onClick={() => setViewMode("json")}
                        >JSON</button>
                    </div>
                    <div className={styles.grow} />
                    <span className={styles.stat}>
                        <strong>{filledSlots}</strong>&nbsp;/&nbsp;{TOTAL_SLOTS} slots filled
                    </span>
                    <span className={styles.statSep}>·</span>
                    <span className={styles.stat}>
                        {!hasAnySelected
                            ? <><span className={styles.statDim}>○</span>&nbsp;PENDING</>
                            : <><span className={styles.statOk}>✓</span>&nbsp;ACTIVE</>}
                    </span>
                </div>

                <InlineBanners
                    criticalCount={criticalCount}
                    loadPct={powerStats.loadPct}
                    psu={powerStats.psu}
                    firstCriticalMessage={firstCriticalMessage}
                />

                {!hasAnySelected && (
                    <div className={styles.emptyCTA}>
                        <div className={`${styles.emptyPanel} ${styles.emptyPanelFeatured}`}>
                            <div className={styles.eh}>
                                <span className={`${styles.emptyOrd} ${styles.emptyOrdFeatured}`}>01 ·</span>
                                РЕКОМЕНДОВАНО
                            </div>
                            <h2>Запустити Автопідбір</h2>
                            <p>Введіть бюджет та сценарій використання. Ми підберемо сумісні компоненти у межах вашого бюджету.</p>
                            <button className={`${styles.btn} ${styles.btnPri}`}
                                onClick={() => setAutoPanelOpen(true)}>
                                ⚙ Відкрити Автопідбір
                            </button>
                        </div>
                        <div className={styles.emptyPanel}>
                            <div className={styles.eh}>
                                <span className={styles.emptyOrd}>02 ·</span>
                                ВРУЧНУ
                            </div>
                            <h2>Підібрати по одному</h2>
                            <p>Почніть з процесора. Сумісність перевіряється на кожному кроці.</p>
                            <button className={styles.btn} onClick={() => navigate("/components/cpu")}>
                                ＋ Вибрати CPU →
                            </button>
                        </div>
                    </div>
                )}

                {viewMode === "table" && (
                    <PartsTable
                        componentData={componentData}
                        loading={loading}
                        selectedRow={selectedRow}
                        onSelectRow={setSelectedRow}
                        onRemoveSingle={removeSingle}
                        onRemoveMulti={removeMulti}
                        onAdjustQty={adjustQty}
                    />
                )}
                {viewMode === "cards" && (
                    <CardsView
                        componentData={componentData}
                        onRemoveSingle={removeSingle}
                        onRemoveMulti={removeMulti}
                    />
                )}
                {viewMode === "json" && (
                    <JsonView componentData={componentData} />
                )}

                <div className={styles.colMainSpacer} />

                <div className={styles.savebar}>
                    <div className={styles.meta}>
                        <span>BUILD&nbsp;&nbsp;<strong>{editingBuild?.name || draftName || "untitled"}</strong></span>
                        <span className={styles.statSep}>·</span>
                        <span>{filledSlots}/{TOTAL_SLOTS} SLOTS · {partCount} PARTS</span>
                        <span className={styles.statSep}>·</span>
                        <span>
                            {!hasAnySelected
                                ? <span className={styles.statDim}>○ PENDING</span>
                                : <span className={styles.statOk}>✓ ACTIVE</span>}
                        </span>
                    </div>
                    <div className={styles.grow} />
                    {editingBuild ? (
                        <>
                            <button
                                className={styles.btn}
                                onClick={togglePublish}
                                disabled={publishLoading || saveLoading || criticalCount > 0}
                                title={criticalCount > 0 ? "Виправте конфлікти сумісності, щоб опублікувати" : undefined}
                            >
                                {publishLoading
                                    ? "..."
                                    : editingBuild.isPublished
                                        ? "↩ Зняти з публікації"
                                        : "↗ Опублікувати"}
                            </button>
                            <button className={`${styles.btn} ${styles.btnPri}`}
                                onClick={() => handleSaveBuild()} disabled={saveLoading}>
                                {saveLoading ? "Збереження..." : "✓ Оновити"}
                            </button>
                            <button className={styles.btn}
                                onClick={() => handleSaveBuild(true)} disabled={saveLoading}>
                                Зберегти як нову
                            </button>
                        </>
                    ) : (
                        <>
                            {hasAnySelected && (
                                <button
                                    className={`${styles.btn} ${styles.btnDng}`}
                                    onClick={handleDiscardDraft}
                                    title="Скинути всі компоненти"
                                >
                                    ⌫ Скинути
                                </button>
                            )}
                            <button className={`${styles.btn} ${styles.btnPri}`}
                                onClick={() => handleSaveBuild()} disabled={saveLoading}>
                                {saveLoading ? "Збереження..." : "✓ Зберегти"}
                            </button>
                        </>
                    )}
                </div>
            </div>

            <aside className={styles.colSide}>
                <div className={styles.colSideScroll}>
                    <TotalCard total={totalPrice} partCount={partCount} />
                    <PowerCard componentData={componentData} />
                    <CompatibilityCheck
                        selectedComponentIds={convertToCompatibilityFormat()}
                        componentData={componentData}
                        onCounts={handleCompatCounts}
                    />
                </div>
                {(() => {
                    let cls = styles.sideFooterDim;
                    let glyph = "○";
                    let text = "Очікує компонентів";
                    if (hasAnySelected) {
                        if (criticalCount > 0) {
                            cls = styles.sideFooterErr;
                            glyph = "×";
                            text = `${criticalCount} ${criticalCount === 1 ? "конфлікт" : "конфлікти"} сумісності`;
                        } else if (firstCriticalMessage) {
                            cls = styles.sideFooterWarn;
                            glyph = "!";
                            text = "Перевірте попередження";
                        } else {
                            cls = styles.sideFooterOk;
                            glyph = "✓";
                            text = "All systems ok";
                        }
                    }
                    return (
                        <div className={`${styles.sideFooter} ${cls}`}>
                            <span className={styles.glyph}>{glyph}</span>
                            <span>{text}</span>
                        </div>
                    );
                })()}
            </aside>

        </section>
    );
}

export default PcBuildPage;
