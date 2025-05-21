import {Link, useNavigate} from "react-router-dom";
import {useEffect, useRef, useState} from "react";
import axios from "../../api/axios.jsx";
import styles from './PcBuildPage.module.css';
import { getComponentById } from "../../services/componentService.js";


const CHECK_URL = '/api/pcBuild/check'
function CompatibilityCheck({ selectedComponentIds }) {
    const [compatibilityResults, setCompatibilityResults] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        // Only check compatibility if we have at least some components selected
        const hasComponents = Object.values(selectedComponentIds).some(
            value => value !== null && (Array.isArray(value) ? value.length > 0 : true)
        );

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
    }, [selectedComponentIds]);

    if (loading) {
        return <div className={styles['compatibility-loading']}>Перевірка сумісності...</div>;
    }

    if (error) {
        return <div className={styles['compatibility-error']}>{error}</div>;
    }

    if (!compatibilityResults) {
        return null;
    }

    console.log("Compatibility results:", compatibilityResults);

    // Функція для визначення типу повідомлення за числовим кодом
    const getMessageTypeInfo = (typeCode) => {
        // Визначаємо тип повідомлення (0 = Problem, 1 = Warning)
        if (typeCode === 0) {
            return {
                type: 'Problem',
                icon: '❌',
                className: styles['message-error']
            };
        } else {
            return {
                type: 'Warning',
                icon: '⚠️',
                className: styles['message-warning']
            };
        }
    };

    return (
        <div className={styles['compatibility-container']}>
            <h2>Перевірка сумісності</h2>

            {/* Overall status */}
            <div className={`${styles['compatibility-status']} ${
                !compatibilityResults.compatible
                    ? styles['status-error']
                    : compatibilityResults.hasWarnings
                        ? styles['status-warning']
                        : styles['status-success']
            }`}>
                {!compatibilityResults.compatible
                    ? '❌ Виявлено несумісні компоненти'
                    : compatibilityResults.hasWarnings
                        ? '⚠️ Сумісність із застереженнями'
                        : '✅ Всі компоненти сумісні'}
            </div>

            {/* All messages in a single list - WITH NULL CHECKS */}
            <ul className={styles['compatibility-messages']}>
                {compatibilityResults.results && compatibilityResults.results.flatMap((result, resultIndex) =>
                        result.messages && result.messages.map((message, messageIndex) => {
                            const messageInfo = getMessageTypeInfo(message.type);
                            return (
                                <li
                                    key={`${resultIndex}-${messageIndex}`}
                                    className={`${styles['compatibility-message']} ${messageInfo.className}`}
                                >
                                    {messageInfo.icon} {message.message}
                                </li>
                            );
                        })
                )}
            </ul>
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

    const [initialLoadComplete, setInitialLoadComplete] = useState(false);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    const isInitialMount = useRef(true);

    // Configuration for component types - single vs multi
    const componentTypes = [
        {key: 'cpu', label: 'CPU', isMulti: false},
        {key: 'gpu', label: 'GPU', isMulti: false},
        {key: 'motherboard', label: 'Motherboard', isMulti: false},
        {key: 'rams', label: 'RAM', isMulti: true, singleType: 'ram'},
        {key: 'ssds', label: 'SSD', isMulti: true, singleType: 'ssd'},
        {key: 'hdds', label: 'HDD', isMulti: true, singleType: 'hdd'},
        {key: 'powerSupply', label: 'Power Supply', isMulti: false},
        {key: 'cpuCooler', label: 'CPU Cooler', isMulti: false},
        {key: 'pcCase', label: 'PC Case', isMulti: false},
        {key: 'fans', label: 'Fan', isMulti: true, singleType: 'fan'}
    ];

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
                                        productOfferUrl: component.productOfferUrl
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
                                        productOfferUrl: item.productOfferUrl
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

    // Remove a single component
    const removeComponent = (type) => {
        console.log("Removing component type:", type);
        setSelectedComponents(prev => ({
            ...prev,
            [type]: null
        }));
    };

    // Remove a multi-component by ID
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

    const renderMultiComponents = (key, label, singleType) => {
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
                    <Link to={`/components/${singleType}`}>Add {label}</Link>
                </button>
            </div>
        );
    };

    if (loading && !initialLoadComplete) {
        return <div className={styles['loading-indicator']}>Loading your build...</div>;
    }

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
                        {calculateComponentPrice(item).toFixed(2)}
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
                            Remove
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

    return (
        <section className={styles['build-components-page']}>
            <h1>Build Your PC</h1>
            <div className={styles['build-components-container']}>
                <div className={styles['build-components-section']}>
                    <table className={styles['build-components-table']}>
                        {/* Table content remains the same, but using renderMultiComponents */}
                        <thead>
                            <tr>
                                <th>Type</th>
                                <th>Component</th>
                                <th>Store</th>
                                <th>Price</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                        {/* Single components */}
                        {componentTypes.filter(type => !type.isMulti).map(({key, label}) => (
                            <tr key={key}>
                                <td>{label}</td>
                                <td>
                                    {loading && selectedComponents[key] ? (
                                        <div className={styles['loading-component']}>Loading component...</div>
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
                                            <Link to={`/components/${key}`}>Choose A {label}</Link>
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
                                                Remove
                                            </button>
                                        </div>
                                    )}
                                </td>
                            </tr>
                        ))}

                        {/* Multi components */}
                        {componentTypes.filter(type => type.isMulti).map(({key, label, singleType}) => (
                            <tr key={key}>
                                <td>{label}</td>
                                <td className={styles['multi-components-info-container']}>
                                    {renderMultiComponents(key, label, singleType)}
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
                        <h3>Total Price: {calculateTotalPrice()} UAH</h3>
                    </div>
                    <div>
                        <button className={'button-primary'}>
                            <Link to={'#'}>Зберегти</Link>
                        </button>
                    </div>
                </div>
                <CompatibilityCheck selectedComponentIds={convertToCompatibilityFormat()}/>
            </div>
        </section>
    );
}

export default PcBuildPage;