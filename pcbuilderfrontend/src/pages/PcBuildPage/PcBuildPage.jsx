import {Link, useLocation, useNavigate} from "react-router-dom";
import {useEffect, useRef, useState} from "react";
import axios from "../../api/axios.jsx";
import styles from './PcBuildPage.module.css';
import { getComponentById } from "../../services/componentService.js";
import useAuth from "../../hooks/useAuth.js";
import useAxiosPrivate from "../../hooks/useAxiosPrivate.js";
import SaveBuildModal from "../../components/SaveBuildModal/SaveBuildModal.jsx";
import CancelEditModal from "../../components/CanselEditModal/CanselEditModal.jsx";
import Toast from "../../components/Toast/Toast.jsx";


const CHECK_URL = '/api/pcBuild/check'
const SAVE_BUILD_URL = '/api/pcBuild/save';
const UPDATE_BUILD_URL = '/api/pcBuild/update'
function CompatibilityCheck({ selectedComponentIds, componentData }) {
    const [compatibilityResults, setCompatibilityResults] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [totalWattage, setTotalWattage] = useState(0);

    const hasComponents = Object.values(selectedComponentIds).some(
        value => value !== null && (Array.isArray(value) ? value.length > 0 : true)
    );
    useEffect(() => {
        if (!componentData) return;

        let wattage = 0;

        // Add CPU wattage
        if (componentData.cpu?.tdp) {
            wattage += parseInt(componentData.cpu.tdp) || 0;
        }

        // Add GPU wattage (often the highest consumer)
        if (componentData.gpu?.wattage) {
            wattage += parseInt(componentData.gpu.wattage) || 0;
        }

        // Add other components' wattage if available
        // Motherboard (usually small)
        if (componentData.motherboard?.wattage) {
            wattage += parseInt(componentData.motherboard.wattage) || 0;
        }

        // RAM modules (each stick typically uses 2-3W)
        if (componentData.rams && Array.isArray(componentData.rams)) {
            componentData.rams.forEach(ram => {
                if (ram.component?.wattage) {
                    wattage += (parseInt(ram.component.wattage) || 0) * (ram.quantity || 1) * parseInt(ram.component.moduleQuantity);
                } else {
                    // Estimate 3W per RAM stick if not specified
                    wattage += 0;
                }
            });
        }

        // Storage drives
        if (componentData.ssds && Array.isArray(componentData.ssds)) {
            componentData.ssds.forEach(ssd => {
                if (ssd.component?.wattage) {
                    wattage += (parseInt(ssd.component.wattage) || 0) * (ssd.quantity || 1);
                } else {
                    // Estimate 5W per SSD if not specified
                    wattage += 0;
                }
            });
        }

        if (componentData.hdds && Array.isArray(componentData.hdds)) {
            componentData.hdds.forEach(hdd => {
                if (hdd.component?.wattage) {
                    wattage += (parseInt(hdd.component.wattage) || 0) * (hdd.quantity || 1);
                } else {
                    // Estimate 8W per HDD if not specified
                    wattage += 0;
                }
            });
        }

        // Fans (typically 1-3W each)
        if (componentData.fans && Array.isArray(componentData.fans)) {
            componentData.fans.forEach(fan => {
                if (fan.component?.wattage) {
                    wattage += (parseInt(fan.component.wattage) || 0) * (fan.quantity || 1) * parseInt(fan.component.moduleCount);
                } else {
                    // Estimate 2W per fan if not specified
                    wattage += 0;
                }
            });
        }

        // CPU Cooler
        if (componentData.cpuCooler?.wattage) {
            wattage += parseInt(componentData.cpuCooler.wattage) || 0;
        }
        if (hasComponents) {
            wattage += 150;
        }

        setTotalWattage(wattage);
    }, [componentData, hasComponents]);

    useEffect(() => {
        // Only check compatibility if we have at least some components selected


        if (!hasComponents) {
            setCompatibilityResults(null);
            return;
        }

        const checkCompatibility = async () => {
            setLoading(true);
            try {
                // Transform the data structure for the API
                const requestData = {
                    cpuId: selectedComponentIds.cpu,
                    gpuId: selectedComponentIds.gpu,
                    motherboardId: selectedComponentIds.motherboard,
                    powerSupplyId: selectedComponentIds.powerSupply,
                    cpuCoolerId: selectedComponentIds.cpuCooler,
                    pcCaseId: selectedComponentIds.pcCase,
                    rams: selectedComponentIds.rams,
                    ssds: selectedComponentIds.ssds,
                    hdds: selectedComponentIds.hdds,
                    fans: selectedComponentIds.fans
                };

                const response = await axios.post(CHECK_URL, requestData);
                setCompatibilityResults(response.data);
                setError(null);
            } catch (err) {
                console.error("Error checking compatibility:", err);
                setError("Failed to check compatibility");
            } finally {
                setLoading(false);
            }
        };

        checkCompatibility();
    }, [selectedComponentIds, hasComponents]);


    console.log("Compatibility results:", compatibilityResults);

    const getMessageTypeInfo = (typeCode) => {
        // Визначаємо тип повідомлення (0 = Problem, 1 = Warning)
        if (typeCode === 0) {
            return {
                type: 'Problem',
                className: styles['message-error']
            };
        } else {
            return {
                type: 'Warning',
                className: styles['message-warning']
            };
        }
    };

    let warningCount = 0;
    let problemCount = 0;

    // Count warnings and problems using the correct data structure
    if (compatibilityResults && compatibilityResults.results) {
        // Loop through each result
        for (let i = 0; i < compatibilityResults.results.length; i++) {
            const result = compatibilityResults.results[i];

            // If this result has messages, loop through them
            if (result.messages && Array.isArray(result.messages)) {
                for (let j = 0; j < result.messages.length; j++) {
                    const message = result.messages[j];

                    // Count based on message type
                    if (message.type === 1) {
                        warningCount++;
                    } else if (message.type === 0) { // Assuming type 0 is for problems/errors
                        problemCount++;
                    }
                }
            }
        }
    }
    return (
        <div className={styles['compatibility-container']}>
            <h2>Перевірка сумісності</h2>

            <div className={styles['basic-info-display']}>
                <div className={`${styles['basic-info']} ${styles['wattage-info']}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                         className={styles['icon']}
                         viewBox="0 0 16 16">
                        <path
                            d="M5.52.359A.5.5 0 0 1 6 0h4a.5.5 0 0 1 .474.658L8.694 6H12.5a.5.5 0 0 1 .395.807l-7 9a.5.5 0 0 1-.873-.454L6.823 9.5H3.5a.5.5 0 0 1-.48-.641z"/>
                    </svg>
                    <div className={styles['wattage-text']}>
                        Необхідна потужність(із запасом): {totalWattage} Вт
                    </div>
                </div>
                <div className={`${styles['basic-info']} ${styles['wattage-info']}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                         className={styles['icon']}
                         viewBox="0 0 16 16">
                        <path
                            d="M5.52.359A.5.5 0 0 1 6 0h4a.5.5 0 0 1 .474.658L8.694 6H12.5a.5.5 0 0 1 .395.807l-7 9a.5.5 0 0 1-.873-.454L6.823 9.5H3.5a.5.5 0 0 1-.48-.641z"/>
                    </svg>
                    {componentData.powerSupply ? (
                        <div className={styles['wattage-text']}>
                            Потужність БЖ: {componentData.powerSupply.wattage} Вт
                        </div>

                    ) : (
                        <div className={styles['wattage-text']}>
                            Потужність БЖ: 0 Вт
                        </div>
                    )}
                </div>

                <div className={`${styles['basic-info']} ${styles['problems-info']}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                         className={styles['icon']} viewBox="0 0 16 16">
                        <path
                            d="M4.54.146A.5.5 0 0 1 4.893 0h6.214a.5.5 0 0 1 .353.146l4.394 4.394a.5.5 0 0 1 .146.353v6.214a.5.5 0 0 1-.146.353l-4.394 4.394a.5.5 0 0 1-.353.146H4.893a.5.5 0 0 1-.353-.146L.146 11.46A.5.5 0 0 1 0 11.107V4.893a.5.5 0 0 1 .146-.353zM5.1 1 1 5.1v5.8L5.1 15h5.8l4.1-4.1V5.1L10.9 1z"/>
                        <path
                            d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
                    </svg>
                    <div className={styles['wattage-text']}>
                        Кількість критичних проблем у збірці: {problemCount}
                    </div>
                </div>
                <div className={`${styles['basic-info']} ${styles['warnings-info']}`}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                         className={styles['icon']} viewBox="0 0 16 16">
                        <path
                            d="M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z"/>
                        <path
                            d="M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
                    </svg>
                    <div className={styles['wattage-text']}>
                        Кількість потенційних проблем: {warningCount}
                    </div>
                </div>
            </div>


            {/* Overall status */}
            <div className={`${styles['compatibility-status']} ${
                !hasComponents
                    ? styles['status-warning']
                    : !compatibilityResults?.compatible
                        ? styles['status-error']
                        : compatibilityResults?.hasWarnings
                            ? styles['status-warning']
                            : styles['status-success']
            }`}>
                {!hasComponents ? (
                    <>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                             className={styles['icon']} viewBox="0 0 16 16">
                            <path
                                d="M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z"/>
                            <path
                                d="M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
                        </svg>
                        <p>Компоненти не вибрано</p>
                    </>
                ) : !compatibilityResults?.compatible ? (
                    <>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                             className={styles['icon']} viewBox="0 0 16 16">
                            <path
                                d="M4.54.146A.5.5 0 0 1 4.893 0h6.214a.5.5 0 0 1 .353.146l4.394 4.394a.5.5 0 0 1 .146.353v6.214a.5.5 0 0 1-.146.353l-4.394 4.394a.5.5 0 0 1-.353.146H4.893a.5.5 0 0 1-.353-.146L.146 11.46A.5.5 0 0 1 0 11.107V4.893a.5.5 0 0 1 .146-.353zM5.1 1 1 5.1v5.8L5.1 15h5.8l4.1-4.1V5.1L10.9 1z"/>
                            <path
                                d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
                        </svg>
                        <p>Виявлено несумісні комплектуючі</p>
                    </>
                ) : compatibilityResults?.hasWarnings ? (
                    <>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                             className={styles['icon']} viewBox="0 0 16 16">
                            <path
                                d="M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z"/>
                            <path
                                d="M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z"/>
                        </svg>
                        <p>Виявлено потенційні проблеми</p>
                    </>
                ) : loading ? (
                    <p>Перевірка сумісності...</p>
                ) : (
                    <>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                             className={styles['icon']} viewBox="0 0 16 16">
                            <path
                                d="M3 14.5A1.5 1.5 0 0 1 1.5 13V3A1.5 1.5 0 0 1 3 1.5h8a.5.5 0 0 1 0 1H3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V8a.5.5 0 0 1 1 0v5a1.5 1.5 0 0 1-1.5 1.5z"/>
                            <path
                                d="m8.354 10.354 7-7a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0"/>
                        </svg>
                        <p>Всі компоненти сумісні</p>
                    </>
                )}
            </div>

            {/* All messages in a single list - WITH NULL CHECKS */}
            {hasComponents && compatibilityResults && compatibilityResults.results && (
                <ul className={styles['compatibility-messages']}>
                    {compatibilityResults.results.flatMap((result, resultIndex) =>
                            result.messages && result.messages.map((message, messageIndex) => {
                                const messageInfo = getMessageTypeInfo(message.type);
                                return (
                                    <li
                                        key={`${resultIndex}-${messageIndex}`}
                                        className={`${styles['compatibility-message']} ${messageInfo.className}`}
                                    >
                                        {message.message}
                                    </li>
                                );
                            })
                    )}
                </ul>
            )}

            {/* Show a message when no components are selected */}
            {!hasComponents && (
                <ul className={styles['compatibility-messages']}>
                    <li className={`${styles['compatibility-message']} ${styles['message-warning']}`}>
                        Додайте компоненти до збірки для перевірки сумісності
                    </li>
                </ul>
            )}
        </div>
    );
}

function PcBuildPage() {
    // Store component IDs - arrays for multi-components
    const [selectedComponents, setSelectedComponents] = useState({
        cpu: null,
        gpu: null,
        motherboard: null,
        rams: [],
        ssds: [],
        hdds: [],
        powerSupply: null,
        cpuCooler: null,
        pcCase: null,
        fans: []
    });

    // Store full component data separately
    const [componentData, setComponentData] = useState({
        cpu: null,
        gpu: null,
        motherboard: null,
        rams: [],
        ssds: [],
        hdds: [],
        powerSupply: null,
        cpuCooler: null,
        pcCase: null,
        fans: []
    });
    const [cancelEditModal, setCancelEditModal] = useState({
        isOpen: false
    });
    const [initialLoadComplete, setInitialLoadComplete] = useState(false);
    const [loading, setLoading] = useState(true);
    const [editingBuild, setEditingBuild] = useState(null);
    const [saveModal, setSaveModal] = useState({
        isOpen: false
    });
    const [saveStatus, setSaveStatus] = useState({
        loading: false,
        error: null,
        success: false
    });
    const [toast, setToast] = useState({
        visible: false,
        message: '',
        type: 'success'
    });
    const navigate = useNavigate();
    const location = useLocation();
    const isInitialMount = useRef(true);
    const {auth} = useAuth();
    const axiosPrivate = useAxiosPrivate();


    // Configuration for component types - single vs multi
    const componentTypes = [
        {key: 'cpu', label: 'Процесор', buttonLabel: 'Процесор', isMulti: false},
        {key: 'gpu', label: 'Відеокарта', buttonLabel: 'Відеокарту', isMulti: false},
        {key: 'motherboard', label: 'Материнська Плата', buttonLabel: 'Материнську Плату', isMulti: false},
        {key: 'rams', label: "Оперативна Пам'ять", buttonLabel: "Оперативну Пам'ять", isMulti: true, singleType: 'ram'},
        {key: 'ssds', label: 'SSD Диски', buttonLabel: 'SSD Диск', isMulti: true, singleType: 'ssd'},
        {key: 'hdds', label: 'HDD Диски', buttonLabel: 'HDD Диск', isMulti: true, singleType: 'hdd'},
        {key: 'powerSupply', label: 'Блок живлення', buttonLabel: 'Блок живлення', isMulti: false},
        {key: 'cpuCooler', label: 'Кулер Процесора', buttonLabel: 'Кулер Процесора', isMulti: false},
        {key: 'pcCase', label: 'Корпус ПК', buttonLabel: 'Корпус ПК', isMulti: false},
        {key: 'fans', label: 'Додаткові Вентилятори', buttonLabel: 'Вентилятор', isMulti: true, singleType: 'fan'}
    ];

    // Check if we are editing a build
    useEffect(() => {
        try {
            const editingBuildData = localStorage.getItem('editingBuild');
            if (editingBuildData) {
                const parsedData = JSON.parse(editingBuildData);
                setEditingBuild(parsedData);
                // Populate the save modal with existing data when opened
                if (parsedData.name) {
                    setSaveModal(prev => ({
                        ...prev,
                        buildName: parsedData.name,
                        buildDescription: parsedData.description
                    }));
                }
            }
        } catch (err) {
            console.error("Error loading editing build data:", err);
        }
    }, []);

    // Load saved component IDs from localStorage on initial render
    useEffect(() => {
        try {
            console.log("PcBuildPage mounted, checking localStorage");
            const savedComponents = localStorage.getItem('selectedComponents');

            if (savedComponents) {
                const parsedComponents = JSON.parse(savedComponents);
                console.log("Current parsed components:", parsedComponents);
                setSelectedComponents(parsedComponents);
                // We'll log the updated state in another useEffect
            }
        } catch (err) {
            console.error("Error loading components from localStorage:", err);
        } finally {
            setInitialLoadComplete(true);
        }
    }, []);

    useEffect(() => {
        console.log("selectedComponents updated:", selectedComponents);
    }, [selectedComponents]);

    // Fetch component data whenever IDs change
    useEffect(() => {
        if (!initialLoadComplete) {
            // Skip first render to avoid unnecessary fetching
            return;
        }

        const fetchComponentData = async () => {
            setLoading(true);
            try {
                const newComponentData = {
                    cpu: null,
                    gpu: null,
                    motherboard: null,
                    rams: [],
                    ssds: [],
                    hdds: [],
                    powerSupply: null,
                    cpuCooler: null,
                    pcCase: null,
                    fans: []
                };

                // Fetch data for single components
                for (const type of componentTypes) {
                    if (!type.isMulti) {
                        const component = selectedComponents[type.key];
                        if (component && component.componentId) {
                            try {
                                const data = await getComponentById(type.key, component.componentId);
                                newComponentData[type.key] = {
                                    ...data,
                                    selectedOffer: {
                                        price: component.price,
                                        storeName: component.storeName,
                                        storeLogoUrl: component.storeLogoUrl,
                                        productOfferUrl: component.productOfferUrl,
                                        id: component.offerId || component.componentId
                                    }
                                };
                            } catch (error) {
                                console.error(`Error fetching ${type.key} component:`, error);
                            }
                        }
                    }
                }

                // Fetch data for multi-components
                for (const type of componentTypes) {
                    if (type.isMulti && selectedComponents[type.key]?.length > 0) {
                        const componentItems = [];

                        // Process each item in the array
                        for (const item of selectedComponents[type.key]) {
                            try {
                                const data = await getComponentById(type.singleType, item.componentId);
                                if (data) {
                                    componentItems.push({
                                        component: data,
                                        componentId: item.componentId,
                                        quantity: item.quantity,
                                        price: item.price,
                                        storeName: item.storeName,
                                        storeLogoUrl: item.storeLogoUrl,
                                        productOfferUrl: item.productOfferUrl,
                                        offerId: item.offerId || item.componentId,
                                        // Also create this property for consistency with your save function
                                        selectedOfferId: item.offerId || item.componentId
                                    });
                                }
                            } catch (error) {
                                console.error(`Error fetching ${type.singleType} component:`, error);
                            }
                        }

                        newComponentData[type.key] = componentItems;
                    }
                }

                setComponentData(newComponentData);
            } catch (err) {
                console.error("Error fetching component data:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchComponentData();

        // Mark initial mount as done (still needed for other effects)
        if (isInitialMount.current) {
            isInitialMount.current = false;
        }
    }, [selectedComponents, initialLoadComplete]);

    // Save component IDs to localStorage when they change
    useEffect(() => {
        if (!initialLoadComplete) {
            return;
        }

        try {
            console.log("Saving to localStorage:", selectedComponents);
            localStorage.setItem('selectedComponents', JSON.stringify(selectedComponents));
        } catch (err) {
            console.error("Error saving components to localStorage:", err);
        }
    }, [selectedComponents, initialLoadComplete]);

    useEffect(() => {
        // This effect runs when auth state changes
        if (!auth?.accessToken && editingBuild) {
            // User logged out while in edit mode
            // Clear editing state but keep selected components
            localStorage.removeItem('editingBuild');
            setEditingBuild(null);

        }
    }, [auth, editingBuild]);

    if (loading && !initialLoadComplete) {
        return <div className={styles['loading-indicator']}>Loading your build...</div>;
    }

    const removeComponent = (type) => {
        console.log("Removing component type:", type);
        setSelectedComponents(prev => ({
            ...prev,
            [type]: null
        }));
    };

    const removeMultiComponent = (type, componentId) => {
        console.log(`Removing ${type} with ID: ${componentId}`);
        setSelectedComponents(prev => ({
            ...prev,
            [type]: prev[type].filter(item => item.componentId !== componentId)
        }));
    };

    // Adjust quantity of a multi-component
    const adjustQuantity = (type, componentId, change) => {
        console.log(`Adjusting quantity for ${type}, componentId: ${componentId}, change: ${change}`);
        setSelectedComponents(prev => {
            const updatedComponents = [...prev[type]];
            const index = updatedComponents.findIndex(item => item.componentId === componentId);

            if (index !== -1) {
                const newQuantity = updatedComponents[index].quantity + change;

                if (newQuantity <= 0) {
                    return {
                        ...prev,
                        [type]: updatedComponents.filter(item => item.componentId !== componentId)
                    };
                } else {
                    updatedComponents[index] = {
                        ...updatedComponents[index],
                        quantity: newQuantity
                    };

                    return {
                        ...prev,
                        [type]: updatedComponents
                    };
                }
            }

            return prev;
        });
    };

    const renderMultiComponents = (key, buttonLabel, singleType) => {
        return (
            <div className={styles['multi-components']}>
                {componentData[key].length > 0 && (
                    <>
                        {componentData[key].map((item, index) => (
                            <div key={index} className={styles['multi-component-item']}>
                                <div className={styles['selected-component']}>
                                    {item.component.photoUrl && (
                                        <img
                                            src={item.component.photoUrl}
                                            alt={item.component.name}
                                            width="50"
                                        />
                                    )}
                                    <div className={styles['component-name']}>
                                        {item.component.name}
                                    </div>
                                </div>
                                <div className={styles['quantity-control']}>
                                    <button
                                        className={styles['quantity-btn']}
                                        onClick={() => adjustQuantity(key, item.componentId, 1)}
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg"
                                             fill="currentColor" className="bi bi-caret-up-fill" viewBox="0 0 16 16">
                                            <path
                                                d="m7.247 4.86-4.796 5.481c-.566.647-.106 1.659.753 1.659h9.592a1 1 0 0 0 .753-1.659l-4.796-5.48a1 1 0 0 0-1.506 0z"/>
                                        </svg>
                                    </button>
                                    <span className={styles['quantity-display']}>
                                            {item.quantity}
                                    </span>
                                    <button
                                        className={styles['quantity-btn']}
                                        onClick={() => adjustQuantity(key, item.componentId, -1)}
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                             fill="currentColor" className="bi bi-caret-down-fill" viewBox="0 0 16 16">
                                            <path
                                                d="M7.247 11.14 2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z"/>
                                        </svg>
                                    </button>
                                </div>
                            </div>
                        ))}
                    </>
                )}
                <button className={'button-primary'}>
                    <Link to={`/components/${singleType}`}>Додати {buttonLabel}</Link>
                </button>
            </div>
        );
    };

    // Calculate price for multi-components
    const renderMultiComponentStores = (components) => {
        if (components.length === 0) return "";

        return (
            <div className={styles['multi-info-container']}>
                {components.map((item, index) => (
                    <div key={index} className={styles['component-price-item']}>
                        <div className={styles['store-info']}>
                            <span>{item.storeName}</span>
                        </div>
                    </div>
                ))}
            </div>
        );
    };

    const calculateComponentPrice = (component) => {
        return component.price * component.quantity;
    };
    const renderMultiComponentPrices = (components) => {
        if (components.length === 0) return "";

        return (
            <div className={styles['multi-info-container']}>
                {components.map((item, index) => (
                    <div key={index} className={`${styles['component-price-item']} ${styles['price']}`}>
                        {calculateComponentPrice(item)}
                    </div>
                ))}
            </div>
        );
    };

    const renderMultiComponentButtons = (key, components) => {
        if (components.length === 0) return "";

        return (
            <div className={styles['multi-info-container']}>
                {components.map((item, index) => (
                    <div key={index} className={styles['component-price-item']}>
                        <button
                            className={`button-secondary button-with-icon`}
                            onClick={() => removeMultiComponent(key, item.componentId)}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                                 fill="currentColor" className="bi bi-x" viewBox="0 0 16 16">
                                <path
                                    d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
                            </svg>
                            Прибрати
                        </button>
                    </div>
                ))}
            </div>
        );
    };

    // Calculate total price
    const calculateTotalPrice = () => {
        let total = 0;

        // Add single component prices
        for (const type of componentTypes) {
            if (!type.isMulti && selectedComponents[type.key]) {
                total += Number(selectedComponents[type.key].price) || 0;
            }
        }

        // Add multi-component prices
        for (const type of componentTypes) {
            if (type.isMulti && selectedComponents[type.key]?.length > 0) {
                for (const item of selectedComponents[type.key]) {
                    total += Number(item.price) * item.quantity || 0;
                }
            }
        }

        return total.toFixed(2);
    };

    const convertToCompatibilityFormat = () => {
        const result = {
            cpu: null,
            gpu: null,
            motherboard: null,
            rams: [],
            ssds: [],
            hdds: [],
            powerSupply: null,
            cpuCooler: null,
            pcCase: null,
            fans: []
        };

        // Handle single components
        for (const type of componentTypes) {
            if (!type.isMulti && selectedComponents[type.key]) {
                result[type.key] = selectedComponents[type.key].componentId;
            }
        }

        // Handle multi-components
        for (const type of componentTypes) {
            if (type.isMulti && selectedComponents[type.key]?.length > 0) {
                result[type.key] = selectedComponents[type.key].map(item => ({
                    componentId: item.componentId,
                    quantity: item.quantity
                }));
            }
        }

        return result;
    };

    const openSaveModal = () => {
        if (!auth?.accessToken) {
            navigate('/login', {
                state: {
                    from: location.pathname,
                    message: 'Please log in to save your build.'
                }
            });
            return;
        }

        // If editing, pre-populate the modal with build name and description
        setSaveModal({
            isOpen: true,
            buildName: editingBuild?.name || '',
            buildDescription: editingBuild?.description || ''
        });
    };

    const openSaveAsNewModal = () => {
        if (!auth?.accessToken) {
            navigate('/login', {
                state: {
                    from: location.pathname,
                    message: 'Увійдіть щоб зберегти збірку.'
                }
            });
            return;
        }

        // Open the modal but don't pre-fill with the editing build data
        setSaveModal({
            isOpen: true,
            buildName: '',  // Start with empty name
            buildDescription: '',  // Start with empty description
            saveAsNew: true  // Flag to indicate we're saving as new
        });
    };

    // Function to handle saving the build
    const handleSaveBuild = async (buildData) => {
        // Check if there's at least one component selected
        const hasComponents = Object.values(selectedComponents).some(
            value => value !== null && (Array.isArray(value) ? value.length > 0 : true)
        );

        if (!hasComponents) {
            setToast({
                visible: true,
                message: 'В збірці повинен бути хоча б один компонент.',
                type: 'error'
            });
            return;
        }

        try {
            setSaveStatus({ loading: true, error: null, success: false });
            // Format data according to API requirements
            const buildPayload = {
                name: buildData.name,
                description: buildData.description,

                // Single components - include both componentId and offerId
                cpuId: selectedComponents.cpu?.componentId || null,
                cpuOfferId: componentData.cpu?.selectedOffer?.id || null,

                motherboardId: selectedComponents.motherboard?.componentId || null,
                motherboardOfferId: componentData.motherboard?.selectedOffer?.id || null,

                gpuId: selectedComponents.gpu?.componentId || null,
                gpuOfferId: componentData.gpu?.selectedOffer?.id || null,

                powerSupplyId: selectedComponents.powerSupply?.componentId || null,
                powerSupplyOfferId: componentData.powerSupply?.selectedOffer?.id || null,

                cpuCoolerId: selectedComponents.cpuCooler?.componentId || null,
                cpuCoolerOfferId: componentData.cpuCooler?.selectedOffer?.id || null,

                pcCaseId: selectedComponents.pcCase?.componentId || null,
                pcCaseOfferId: componentData.pcCase?.selectedOffer?.id || null,

                // Multi-components - format as arrays with componentId, offerId, quantity
                rams: componentData.rams?.map(item => ({
                    componentId: item.id || item.componentId,
                    offerId: item.offerId || item.componentId,
                    quantity: item.quantity || 1
                })) || [],

                ssds: componentData.ssds?.map(item => ({
                    componentId: item.id || item.componentId,
                    offerId: item.offerId || item.componentId,
                    quantity: item.quantity || 1
                })) || [],

                hdds: componentData.hdds?.map(item => ({
                    componentId: item.id || item.componentId,
                    offerId: item.offerId || item.componentId,
                    quantity: item.quantity || 1
                })) || [],

                fans: componentData.fans?.map(item => ({
                    componentId: item.id || item.componentId,
                    offerId: item.offerId || item.componentId,
                    quantity: item.quantity || 1
                })) || []
            };

            const saveAsNew = saveModal.saveAsNew;

            if (editingBuild?.id && !saveAsNew) {
                console.log('Editing build ID: ' + editingBuild.id)
                await axiosPrivate.put(`${UPDATE_BUILD_URL}/${editingBuild.id}`, buildPayload);
                localStorage.removeItem('editingBuild');
                setEditingBuild(null);
            } else {
                await axiosPrivate.post(SAVE_BUILD_URL, buildPayload);
                if (editingBuild) {
                    localStorage.removeItem('editingBuild');
                    setEditingBuild(null);
                }
            }

            // Handle success
            setSaveStatus({
                loading: false,
                error: null,
                success: true
            });

            setToast({
                visible: true,
                message: 'Збірку успішно збережено!',
                type: 'success'
            });

            // Close the modal
            setSaveModal({ isOpen: false });

            // Navigate to user builds page or show success message
            navigate('/user/builds', { state: { message: 'Збірку успішно збережено!' } });

        } catch (err) {
            console.error("Error saving build:", err);
            setSaveStatus({
                loading: false,
                error: err.response?.data?.message || 'Не вдалося зберегти збірку. Будь ласка, спробуйте ще раз.',
                success: false
            });

            setToast({
                visible: true,
                message: err.response?.data?.message || 'Не вдалося зберегти збірку. Будь ласка, спробуйте ще раз.',
                type: 'error'
            });
        }
    };

    const handleCancelEdit = () => {
        localStorage.removeItem('editingBuild');
        localStorage.removeItem('selectedComponents');
        localStorage.removeItem('editComponents');
        setEditingBuild(null);
        navigate('/user/builds');
    };

    return (
        <section className={styles['build-components-page']}>
            {toast.visible && (
                <Toast
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast({ ...toast, visible: false })}
                    duration={5000}
                />
            )}
            <SaveBuildModal
                isOpen={saveModal.isOpen}
                onCancel={() => setSaveModal({ isOpen: false })}
                onSave={handleSaveBuild}
                isSaving={saveStatus.loading}
                initialName={editingBuild?.name || ''}
                initialDescription={editingBuild?.description || ''}
                isEditing={!!editingBuild && !saveModal.saveAsNew} // Check if it's not saveAsNew
            />
            <CancelEditModal
                isOpen={cancelEditModal.isOpen}
                onCancel={() => setCancelEditModal({ isOpen: false })}
                onConfirm={handleCancelEdit}
            />
            <h1>
                Конфігуратор ПК
                <svg xmlns="http://www.w3.org/2000/svg" width="44" height="44" fill="currentColor"
                     className="bi bi-gear-wide" viewBox="0 0 16 16">
                    <path
                        d="M8.932.727c-.243-.97-1.62-.97-1.864 0l-.071.286a.96.96 0 0 1-1.622.434l-.205-.211c-.695-.719-1.888-.03-1.613.931l.08.284a.96.96 0 0 1-1.186 1.187l-.284-.081c-.96-.275-1.65.918-.931 1.613l.211.205a.96.96 0 0 1-.434 1.622l-.286.071c-.97.243-.97 1.62 0 1.864l.286.071a.96.96 0 0 1 .434 1.622l-.211.205c-.719.695-.03 1.888.931 1.613l.284-.08a.96.96 0 0 1 1.187 1.187l-.081.283c-.275.96.918 1.65 1.613.931l.205-.211a.96.96 0 0 1 1.622.434l.071.286c.243.97 1.62.97 1.864 0l.071-.286a.96.96 0 0 1 1.622-.434l.205.211c.695.719 1.888.03 1.613-.931l-.08-.284a.96.96 0 0 1 1.187-1.187l.283.081c.96.275 1.65-.918.931-1.613l-.211-.205a.96.96 0 0 1 .434-1.622l.286-.071c.97-.243.97-1.62 0-1.864l-.286-.071a.96.96 0 0 1-.434-1.622l.211-.205c.719-.695.03-1.888-.931-1.613l-.284.08a.96.96 0 0 1-1.187-1.186l.081-.284c.275-.96-.918-1.65-1.613-.931l-.205.211a.96.96 0 0 1-1.622-.434zM8 12.997a4.998 4.998 0 1 1 0-9.995 4.998 4.998 0 0 1 0 9.996z"/>
                </svg>
            </h1>
            {editingBuild && (
                <div className={styles['edit-mode-banner']}>
                    <div className={styles['banner-content']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor"
                             className="bi bi-pencil-square" viewBox="0 0 16 16">
                            <path
                                d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/>
                            <path
                                  d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z"/>
                        </svg>
                        <span>Редагування збірки: <strong>{editingBuild.name}</strong></span>
                    </div>
                    <button
                        className={'button-secondary'}
                        onClick={() => setCancelEditModal({isOpen: true})}
                    >
                        Скасувати Редагування
                    </button>
                </div>
            )}
            <div className={styles['build-components-container']}>
                <div className={styles['build-components-section']}>
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
                        {componentTypes.filter(type => !type.isMulti).map(({key, label, buttonLabel}) => (
                            <tr key={key}>
                                <td>{label}</td>
                                <td>
                                    {loading && selectedComponents[key] ? (
                                        <div className={styles['loading-component']}>Завантаження компонента...</div>
                                    ) : componentData[key] ? (
                                        <div className={styles['selected-component']}>
                                            {componentData[key].photoUrl && (
                                                <img
                                                    src={componentData[key].photoUrl}
                                                    alt={componentData[key].name}
                                                    width="50"
                                                />
                                            )}
                                            <div className={styles['component-name']}>
                                                {componentData[key].name}
                                            </div>
                                        </div>
                                    ) : (
                                        <button className={'button-primary'}>
                                            <Link to={`/components/${key}`}>Виберіть {buttonLabel}</Link>
                                        </button>
                                    )}
                                </td>
                                <td>
                                    {componentData[key]?.selectedOffer && (
                                        <div className={styles['store-info']}>
                                            <span>{componentData[key].selectedOffer.storeName}</span>
                                        </div>
                                    )}
                                </td>
                                <td className={styles['price']}>
                                    {componentData[key]?.selectedOffer?.price ||
                                        componentData[key]?.averagePrice || ""}
                                </td>
                                <td>
                                    {componentData[key] && (
                                        <div className={styles['action-buttons']}>
                                            <button
                                                className={`button-secondary button-with-icon`}
                                                onClick={() => removeComponent(key)}
                                            >
                                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                                                     fill="currentColor" className="bi bi-x" viewBox="0 0 16 16">
                                                    <path
                                                        d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708"/>
                                                </svg>
                                                Прибрати
                                            </button>
                                        </div>
                                    )}
                                </td>
                            </tr>
                        ))}

                        {/* Multi components */}
                        {componentTypes.filter(type => type.isMulti).map(({key, label, buttonLabel, singleType}) => (
                            <tr key={key}>
                                <td>{label}</td>
                                <td className={styles['multi-components-info-container']}>
                                    {renderMultiComponents(key, buttonLabel, singleType)}
                                </td>
                                <td className={styles['multi-components-info-container']}>
                                    {renderMultiComponentStores(componentData[key])}
                                </td>
                                <td className={styles['multi-components-info-container']}>
                                    {renderMultiComponentPrices(componentData[key])}
                                </td>
                                <td className={styles['multi-components-info-container']}>
                                    {componentData[key].length > 0 ? (
                                        renderMultiComponentButtons(key, componentData[key])
                                    ) : (
                                        ""
                                    )}
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    {/* Total Price Display */}
                    <div className={styles['total-price']}>
                        <h3>Остаточна ціна: <span>{calculateTotalPrice()} грн</span></h3>
                    </div>
                    <div className={styles['save-buttons-container']}>
                        {editingBuild ? (
                            <>
                                <button
                                    className={'button-primary'}
                                    onClick={openSaveModal}
                                    disabled={saveStatus.loading}
                                >
                                    {saveStatus.loading ? 'Збереження...' : 'Оновити Збірку'}
                                </button>

                                <button
                                    className={'button-secondary'}
                                    onClick={openSaveAsNewModal}
                                    disabled={saveStatus.loading}
                                >
                                    Зберегти Як Нову
                                </button>
                            </>
                        ) : (
                            <button
                                className={'button-primary'}
                                onClick={openSaveModal}
                                disabled={saveStatus.loading}
                            >
                                {saveStatus.loading ? 'Збереження...' : 'Зберегти Збірку'}
                            </button>
                        )}

                    </div>
                    {/*{saveStatus.error && (*/}
                    {/*    <div className={styles['error-message']}>*/}
                    {/*        {saveStatus.error}*/}
                    {/*    </div>*/}
                    {/*)}*/}
                    {/*{saveStatus.success && (*/}
                    {/*    <div className={styles['success-message']}>*/}
                    {/*        Збірка успішно збережена!*/}
                    {/*    </div>*/}
                    {/*)}*/}
                </div>

                <CompatibilityCheck
                    selectedComponentIds={convertToCompatibilityFormat()}
                    componentData={componentData}
                />
            </div>
        </section>
    );
}

export default PcBuildPage;