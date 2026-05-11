import { Link, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import styles from './PcBuildPage.module.css';
import buttonStyles from '../../components/Button/Button.module.css';
import { componentService } from "../../api/component.service";
import useAuth from "../../hooks/useAuth";
import { buildService } from "../../api/build.service";
import SaveBuildModal from "../../components/SaveBuildModal/SaveBuildModal";
import CancelEditModal from "../../components/CanselEditModal/CanselEditModal";
import { Button } from "../../components/Button/Button";
import Toast from "../../components/Toast/Toast";
import CompatibilityCheck from "./CompatibilityCheck";
import AutoBuilderPanel from "./AutoBuilderPanel";
import type { IAutoBuildComponents, IComponentsCompatibility, IPcBuildInput } from "../../types/build.types";
import type {
    SelectedComponents, ComponentDataState, EditingBuild,
    SaveModalState, ToastState, SingleComponentData, MultiComponentData,
    MultiKey, SelectedOffer, SelectedMultiOffer,
} from "./types";
import {
    EMPTY_SELECTED, EMPTY_DATA,
    COMPONENT_TYPES, SINGLE_TYPES, MULTI_TYPES,
} from "./types";

// --- SVG icons ---

const XIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
         fill="currentColor" className="bi bi-x" viewBox="0 0 16 16">
        <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
    </svg>
);

const CaretUp = () => (
    <svg xmlns="http://www.w3.org/2000/svg"
         fill="currentColor" className="bi bi-caret-up-fill" viewBox="0 0 16 16">
        <path d="m7.247 4.86-4.796 5.481c-.566.647-.106 1.659.753 1.659h9.592a1 1 0 0 0 .753-1.659l-4.796-5.48a1 1 0 0 0-1.506 0z"/>
    </svg>
);

const CaretDown = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
         fill="currentColor" className="bi bi-caret-down-fill" viewBox="0 0 16 16">
        <path d="M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z"/>
    </svg>
);

const WandIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" viewBox="0 0 16 16">
        <path d="M9.5 2.672a.5.5 0 1 0 1 0V.843a.5.5 0 0 0-1 0zm4.5.035A.5.5 0 0 0 13.293 2L12 3.293a.5.5 0 1 0 .707.707zM7.293 4A.5.5 0 1 0 8 3.293L6.707 2A.5.5 0 0 0 6 2.707zm-.621 2.5a.5.5 0 1 0 0-1H4.843a.5.5 0 1 0 0 1zm8.485 0a.5.5 0 1 0 0-1h-1.829a.5.5 0 0 0 0 1zM13.293 10A.5.5 0 1 0 14 9.293L12.707 8a.5.5 0 1 0-.707.707zM9.5 11.157a.5.5 0 0 0 1 0V9.328a.5.5 0 0 0-1 0zm1.854-5.097a.5.5 0 0 0 0-.706l-.708-.708a.5.5 0 0 0-.707 0L8.646 5.94a.5.5 0 0 0 0 .707l.708.708a.5.5 0 0 0 .707 0l1.293-1.293zM3 7.5l-.364-.364a.5.5 0 0 0-.707.707l7.5 7.5a.5.5 0 0 0 .707-.707L5.175 9.68 3 7.5z"/>
    </svg>
);

const PencilIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor"
         className="bi bi-pencil-square" viewBox="0 0 16 16">
        <path d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/>
        <path d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z"/>
    </svg>
);

function PcBuildPage() {
    const [selectedComponents, setSelectedComponents] = useState<SelectedComponents>({ ...EMPTY_SELECTED });
    const [componentData, setComponentData] = useState<ComponentDataState>({ ...EMPTY_DATA });
    const [cancelEditModal, setCancelEditModal] = useState(false);
    const [initialLoadComplete, setInitialLoadComplete] = useState(false);
    const [loading, setLoading] = useState(true);
    const [editingBuild, setEditingBuild] = useState<EditingBuild | null>(null);
    const [saveModal, setSaveModal] = useState<SaveModalState>({ isOpen: false });
    const [saveLoading, setSaveLoading] = useState(false);
    const [toast, setToast] = useState<ToastState>({ visible: false, message: '', type: 'success' });
    const [autoPanelOpen, setAutoPanelOpen] = useState(false);

    const navigate = useNavigate();
    const location = useLocation();
    const isInitialMount = useRef(true);
    const { auth } = useAuth();

    // Load editing build from localStorage
    useEffect(() => {
        try {
            const raw = localStorage.getItem('editingBuild');
            if (raw) {
                const parsed = JSON.parse(raw) as EditingBuild;
                setEditingBuild(parsed);
            }
        } catch (err) {
            console.error("Error loading editing build data:", err);
        }
    }, []);

    // Load saved component IDs from localStorage
    useEffect(() => {
        try {
            const raw = localStorage.getItem('selectedComponents');
            if (raw) {
                setSelectedComponents(JSON.parse(raw) as SelectedComponents);
            }
        } catch (err) {
            console.error("Error loading components from localStorage:", err);
        } finally {
            setInitialLoadComplete(true);
        }
    }, []);

    // Fetch component data whenever IDs change
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

        if (isInitialMount.current) {
            isInitialMount.current = false;
        }
    }, [selectedComponents, initialLoadComplete]);

    // Persist selectedComponents to localStorage
    useEffect(() => {
        if (!initialLoadComplete) return;
        try {
            localStorage.setItem('selectedComponents', JSON.stringify(selectedComponents));
        } catch (err) {
            console.error("Error saving components to localStorage:", err);
        }
    }, [selectedComponents, initialLoadComplete]);

    // Clear editing state on logout
    useEffect(() => {
        if (!auth?.accessToken && editingBuild) {
            localStorage.removeItem('editingBuild');
            setEditingBuild(null);
        }
    }, [auth, editingBuild]);

    if (loading && !initialLoadComplete) {
        return <div className={styles['loading-component']}>Loading your build...</div>;
    }

    // --- Handlers ---

    const removeComponent = (key: string) => {
        setSelectedComponents(prev => ({ ...prev, [key]: null }));
    };

    const removeMultiComponent = (key: string, componentId: string) => {
        setSelectedComponents(prev => ({
            ...prev,
            [key]: (prev[key as keyof SelectedComponents] as typeof prev.rams).filter(
                item => item.componentId !== componentId
            ),
        }));
    };

    const adjustQuantity = (key: string, componentId: string, change: number) => {
        setSelectedComponents(prev => {
            const arr = [...(prev[key as keyof SelectedComponents] as typeof prev.rams)];
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

    const calculateTotalPrice = (): string => {
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
        return total.toFixed(2);
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
            navigate('/login', {
                state: { from: location.pathname, message: 'Увійдіть щоб зберегти збірку.' },
            });
            return;
        }
        callback();
    };

    const openSaveModal = () => requireAuth(() => {
        setSaveModal({ isOpen: true });
    });

    const openSaveAsNewModal = () => requireAuth(() => {
        setSaveModal({ isOpen: true, saveAsNew: true });
    });

    const handleSaveBuild = async (buildData: { name: string; description: string }) => {
        const hasAny = Object.values(selectedComponents).some(
            v => v !== null && (Array.isArray(v) ? v.length > 0 : true)
        );
        if (!hasAny) {
            setToast({ visible: true, message: 'В збірці повинен бути хоча б один компонент.', type: 'error' });
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

            const buildPayload: IPcBuildInput = {
                name: buildData.name,
                isPublished: false,
                ...(buildData.description && { description: buildData.description }),
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
                rams: multiPayload('rams'),
                ssds: multiPayload('ssds'),
                hdds: multiPayload('hdds'),
                fans: multiPayload('fans'),
            };

            if (editingBuild?.id && !saveModal.saveAsNew) {
                await buildService.updateBuild(editingBuild.id, buildPayload);
            } else {
                await buildService.saveBuild(buildPayload);
            }

            if (editingBuild) {
                localStorage.removeItem('editingBuild');
                setEditingBuild(null);
            }

            setSaveLoading(false);
            setToast({ visible: true, message: 'Збірку успішно збережено!', type: 'success' });
            setSaveModal({ isOpen: false });
            navigate('/user/builds', { state: { message: 'Збірку успішно збережено!' } });
        } catch (err: unknown) {
            setSaveLoading(false);
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message || 'Не вдалося зберегти збірку. Будь ласка, спробуйте ще раз.';
            setToast({ visible: true, message: msg, type: 'error' });
        }
    };

    const handleCancelEdit = () => {
        localStorage.removeItem('editingBuild');
        localStorage.removeItem('selectedComponents');
        localStorage.removeItem('editComponents');
        setEditingBuild(null);
        navigate('/user/builds');
    };

    const handleApplyAutoBuild = (components: IAutoBuildComponents) => {
        const makeOffer = (id: string, price: number): SelectedOffer => ({
            componentId: id, offerId: '', price, storeName: '', storeLogoUrl: null, productOfferUrl: null,
        });
        const makeMultiOffer = (id: string, price: number, qty: number): SelectedMultiOffer => ({
            componentId: id, offerId: '', price, quantity: qty, storeName: '', storeLogoUrl: null, productOfferUrl: null,
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

    // --- Render helpers ---

    const renderMultiRow = (key: MultiKey, buttonLabel: string, urlType: string) => {
        const items = componentData[key];
        return (
            <tr key={key}>
                <td>{COMPONENT_TYPES.find(t => t.key === key)!.label}</td>
                <td className={styles['multi-components-info-container']}>
                    <div className={styles['multi-components']}>
                        {items.map((item, i) => (
                            <div key={i} className={styles['multi-component-item']}>
                                <Link to={`/components/${urlType}/${item.componentId}`} className={styles['selected-component']}>
                                    {item.component.photoUrl && (
                                        <img src={item.component.photoUrl} alt={item.component.name} width="50" />
                                    )}
                                    <div className={styles['component-name']}>{item.component.name}</div>
                                </Link>
                                <div className={styles['quantity-control']}>
                                    <button className={styles['quantity-btn']}
                                            onClick={() => adjustQuantity(key, item.componentId, 1)}>
                                        <CaretUp />
                                    </button>
                                    <span className={styles['quantity-display']}>{item.quantity}</span>
                                    <button className={styles['quantity-btn']}
                                            onClick={() => adjustQuantity(key, item.componentId, -1)}>
                                        <CaretDown />
                                    </button>
                                </div>
                            </div>
                        ))}
                        <Link
                            to={`/components/${urlType}`}
                            className={`${buttonStyles.button} ${buttonStyles['outline-primary']} ${buttonStyles.sm}`}
                        >
                            Додати {buttonLabel}
                        </Link>
                    </div>
                </td>
                <td className={styles['multi-components-info-container']}>
                    {items.length > 0 && (
                        <div className={styles['multi-info-container']}>
                            {items.map((item, i) => (
                                <div key={i} className={styles['component-price-item']}>
                                    <div className={styles['store-info']}>
                                        <span>{item.storeName}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </td>
                <td className={styles['multi-components-info-container']}>
                    {items.length > 0 && (
                        <div className={styles['multi-info-container']}>
                            {items.map((item, i) => (
                                <div key={i} className={`${styles['component-price-item']} ${styles['price']}`}>
                                    {item.price * item.quantity}
                                </div>
                            ))}
                        </div>
                    )}
                </td>
                <td className={styles['multi-components-info-container']}>
                    {items.length > 0 && (
                        <div className={styles['multi-info-container']}>
                            {items.map((item, i) => (
                                <div key={i} className={styles['component-price-item']}>
                                    <Button onClick={() => removeMultiComponent(key, item.componentId)}
                                            variant="outline-secondary" size="md">
                                        <XIcon /> Прибрати
                                    </Button>
                                </div>
                            ))}
                        </div>
                    )}
                </td>
            </tr>
        );
    };

    return (
        <section className={styles['build-components-page']}>
            <AutoBuilderPanel
                isOpen={autoPanelOpen}
                hasExistingComponents={Object.values(selectedComponents).some(
                    v => v !== null && (Array.isArray(v) ? v.length > 0 : true)
                )}
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
            <SaveBuildModal
                isOpen={saveModal.isOpen}
                onCancel={() => setSaveModal({ isOpen: false })}
                onSave={handleSaveBuild}
                isSaving={saveLoading}
                initialName={editingBuild?.name || ''}
                initialDescription={editingBuild?.description || ''}
                isEditing={!!editingBuild && !saveModal.saveAsNew}
            />
            <CancelEditModal
                isOpen={cancelEditModal}
                onCancel={() => setCancelEditModal(false)}
                onConfirm={handleCancelEdit}
            />

            {editingBuild && (
                <div className={styles['edit-mode-banner']}>
                    <div className={styles['banner-content']}>
                        <PencilIcon />
                        <span>Редагування збірки: <strong>{editingBuild.name}</strong></span>
                    </div>
                    <Button variant="outline-secondary" size="md"
                            onClick={() => setCancelEditModal(true)}>
                        Скасувати Редагування
                    </Button>
                </div>
            )}

            <div className={styles['build-components-container']}>
                <div className={styles['build-components-section']}>
                    {!Object.values(selectedComponents).some(v => v !== null && (Array.isArray(v) ? v.length > 0 : true)) && (
                        <div className={styles['autobuilder-empty-cta']}>
                            <div className={styles['autobuilder-empty-text']}>
                                <strong>Починіть збирати свій ПК</strong>
                                <span>Додайте компоненти вручну або дозвольте автопідбору зібрати оптимальну збірку за вашим бюджетом</span>
                            </div>
                            <Button variant="primary" size="md" onClick={() => setAutoPanelOpen(true)}>
                                Автопідбір ПК
                            </Button>
                        </div>
                    )}
                    <table className={styles['build-components-table']}>
                        <thead>
                        <tr>
                            <th>Тип</th>
                            <th>Компонент</th>
                            <th>Продавець</th>
                            <th>Ціна, грн</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        {/* Single components */}
                        {SINGLE_TYPES.map(({ key, label, buttonLabel, urlType }) => {
                            const data = componentData[key];
                            return (
                                <tr key={key}>
                                    <td>{label}</td>
                                    <td>
                                        {loading && selectedComponents[key] ? (
                                            <div className={styles['loading-component']}>Завантаження компонента...</div>
                                        ) : data ? (
                                            <Link to={`/components/${urlType}/${data.id}`} className={styles['selected-component']}>
                                                {data.photoUrl && (
                                                    <img src={data.photoUrl} alt={data.name} width="50" />
                                                )}
                                                <div className={styles['component-name']}>{data.name}</div>
                                            </Link>
                                        ) : (
                                            <Link
                                                to={`/components/${urlType}`}
                                                className={`${buttonStyles.button} ${buttonStyles['outline-primary']} ${buttonStyles.sm}`}
                                            >
                                                Додати {buttonLabel}
                                            </Link>
                                        )}
                                    </td>
                                    <td>
                                        {data?.selectedOffer && (
                                            <div className={styles['store-info']}>
                                                <span>{data.selectedOffer.storeName}</span>
                                            </div>
                                        )}
                                    </td>
                                    <td className={styles['price']}>
                                        {data?.selectedOffer?.price || data?.averagePrice || ""}
                                    </td>
                                    <td>
                                        {data && (
                                            <div className={styles['action-buttons']}>
                                                <Button onClick={() => removeComponent(key)}
                                                        variant="outline-secondary" size="sm">
                                                    <XIcon /> Прибрати
                                                </Button>
                                            </div>
                                        )}
                                    </td>
                                </tr>
                            );
                        })}

                        {/* Multi components */}
                        {MULTI_TYPES.map(({ key, buttonLabel, urlType }) =>
                            renderMultiRow(key, buttonLabel, urlType)
                        )}
                        </tbody>
                    </table>

                    <div className={styles['total-price']}>
                        <h3>Остаточна ціна: <span>{calculateTotalPrice()} грн</span></h3>
                    </div>
                    <div className={styles['save-buttons-container']}>
                        {editingBuild ? (
                            <>
                                <Button variant="primary" size="md" onClick={openSaveModal} disabled={saveLoading}>
                                    {saveLoading ? 'Збереження...' : 'Оновити Збірку'}
                                </Button>
                                <Button variant="outline-secondary" size="md" onClick={openSaveAsNewModal} disabled={saveLoading}>
                                    Зберегти Як Нову
                                </Button>
                            </>
                        ) : (
                            <Button variant="primary" size="md" onClick={openSaveModal} disabled={saveLoading}>
                                {saveLoading ? 'Збереження...' : 'Зберегти Збірку'}
                            </Button>
                        )}
                    </div>
                </div>

                <CompatibilityCheck
                    selectedComponentIds={convertToCompatibilityFormat()}
                    componentData={componentData}
                />
            </div>

            {Object.values(selectedComponents).some(v => v !== null && (Array.isArray(v) ? v.length > 0 : true)) && (
                <button className={styles['autobuilder-fab']} onClick={() => setAutoPanelOpen(true)}>
                    <WandIcon />
                    <span>Автопідбір</span>
                </button>
            )}
        </section>
    );
}

export default PcBuildPage;
