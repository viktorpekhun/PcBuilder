import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import styles from './ComponentPage.module.css';
import { getComponentById } from '../../services/componentService';
import { componentSpecFullConfigs } from './componentSpecsFullConfigs.js';

function ComponentPage() {
    const { type, id } = useParams();
    const navigate = useNavigate();
    const [component, setComponent] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [isDescriptionExpanded, setIsDescriptionExpanded] = useState(false);

    useEffect(() => {
        const fetchComponentData = async () => {
            try {
                setLoading(true);
                const componentData = await getComponentById(type, id);
                setComponent(componentData);
                setError(null);
            } catch (err) {
                console.error("Error fetching component:", err);
                setError(`Failed to load ${type} details`);
            } finally {
                setLoading(false);
            }
        };

        fetchComponentData();
    }, [type, id]);

    const toggleDescription = () => {
        setIsDescriptionExpanded(!isDescriptionExpanded);
    };
    const handleAddToBuild = (offer) => {
        try {
            let selectedComponents = {};
            const savedComponents = localStorage.getItem('selectedComponents');

            if (savedComponents) {
                selectedComponents = JSON.parse(savedComponents);
            }

            // Create component with offer data
            const componentWithOffer = {
                componentId: component.id,
                offerId: offer ? offer.id || 'default-offer' : null,
                price: offer ? offer.price : component.averagePrice,
                storeName: offer ? offer.store.name : 'Average Price',
                storeLogoUrl: offer ? offer.store.logoUrl : null,
                productOfferUrl: offer ? offer.productOfferUrl : null
            };

            // Handle multi-components vs single components
            const multiComponentTypes = ['ram', 'ssd', 'hdd', 'fan'];
            if (multiComponentTypes.includes(type)) {
                const arrayName = `${type}s`; // rams, ssds, hdds, fans

                if (!selectedComponents[arrayName]) {
                    selectedComponents[arrayName] = [];
                }

                const existingIndex = selectedComponents[arrayName].findIndex(
                    item => item.componentId === component.id
                );

                if (existingIndex >= 0) {
                    // Update existing entry with new offer data but keep quantity
                    const currentQuantity = selectedComponents[arrayName][existingIndex].quantity;
                    selectedComponents[arrayName][existingIndex] = {
                        ...componentWithOffer,
                        quantity: currentQuantity + 1
                    };
                } else {
                    // Add new entry with quantity 1
                    selectedComponents[arrayName].push({
                        ...componentWithOffer,
                        quantity: 1
                    });
                }
            } else {
                // For single components
                selectedComponents[type] = componentWithOffer;
            }

            localStorage.setItem('selectedComponents', JSON.stringify(selectedComponents));
            navigate('/');
        } catch (err) {
            console.error("Error adding component to build:", err);
        }
    };

    // Handle adding with average price if no offers are available

    // Function to render specs dynamically from config
    const renderSpecsFromConfig = () => {
        if (!component) return null;

        const specs = componentSpecFullConfigs[type] || componentSpecFullConfigs.default;

        return specs.map((spec, index) => {
            // Handle simple key specs

            if (spec.type === 'sectionHeader') {
                return (
                    <tr key={`section-${index}`} className={styles['spec-section-header']}>
                        <td colSpan={2}>{spec.label}</td>
                    </tr>
                );
            }

            if (spec.key) {
                if (spec.isList) {
                    // Handle list-type specs (like PCIe slots)
                    if (Array.isArray(component[spec.key]) && component[spec.key].length > 0) {
                        // Special handling for ports and other items that need separate rows
                        if (spec.renderAsSeparateRows) {
                            return (
                                <React.Fragment key={`${spec.key}-${index}`}>
                                    {/* Optional section header */}
                                    {spec.label && (
                                        <tr className={styles['spec-section-header']}>
                                            <td colSpan={2}>{spec.label}</td>
                                        </tr>
                                    )}

                                    {/* Individual rows for each item, with flexible key/value extraction */}
                                    {component[spec.key].map((item, idx) => {
                                        // Get label and value using formatter functions or fallbacks
                                        const label = spec.getLabelFromItem ? spec.getLabelFromItem(item) : Object.keys(item)[0];
                                        const value = spec.getValueFromItem ? spec.getValueFromItem(item) : Object.values(item)[1];

                                        return (
                                            <tr key={`${spec.key}-item-${idx}`}>
                                                <td>{label}:</td>
                                                <td>{value}{spec.valueUnit && ` ${spec.valueUnit}`}</td>
                                            </tr>
                                        );
                                    })}
                                </React.Fragment>
                            );
                        }

                        // Regular list rendering (existing code)
                        const formattedItems = spec.formatList(
                            component[spec.key].map(item => spec.formatItem(item))
                        );

                        return (
                            <tr key={`${spec.key}-${index}`}>
                                <td>{spec.label || spec.key}:</td>
                                <td>{formattedItems}{spec.unit && ` ${spec.unit}`}</td>
                            </tr>
                        );
                    }
                    return null;
                }

                // Handle regular key specs
                if (component[spec.key] != null && component[spec.key] !== '') {
                    return (
                        <tr key={`${spec.key}-${index}`}>
                            <td>{spec.label || spec.key}:</td>
                            <td>{component[spec.key]}{spec.unit && ` ${spec.unit}`}</td>
                        </tr>
                    );
                }
                return null;
            }

            // Handle combined keys specs
            if (spec.keys) {
                const values = {};
                let hasValue = false;

                spec.keys.forEach(key => {
                    values[key] = component[key];
                    if (component[key] != null && component[key] !== '') {
                        hasValue = true;
                    }
                });

                if (!hasValue) return null;

                // Generate the formatted value
                const formattedValue = spec.format(values);

                // Only render if we have something to show
                if (!formattedValue) return null;

                return (
                    <tr key={`${spec.keys.join('-')}-${index}`}>
                        <td>{spec.label || spec.keys[0]}:</td>
                        <td>{formattedValue}{spec.unit && ` ${spec.unit}`}</td>
                    </tr>
                );
            }

            return null;
        }).filter(Boolean); // Filter out null values
    };

    if (loading) {
        return <div className={styles['loading']}>Loading component details...</div>;
    }

    if (error) {
        return <div className={styles['error']}>{error}</div>;
    }

    if (!component) {
        return <div className={styles['error']}>Component not found</div>;
    }

    return (
        <div className={styles['component-page']}>

            <div className={styles['component-details']}>
                <div className={styles['component-header']}>
                    <div className={styles['header-left']}>
                        <button
                            className={'button-secondary'}
                            onClick={() => navigate(`/components/${type}`)}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" stroke="currentColor" strokeWidth="1"
                                 className="bi bi-arrow-90deg-left" viewBox="0 0 16 16">
                                <path fill-rule="evenodd"
                                      d="M1.146 4.854a.5.5 0 0 1 0-.708l4-4a.5.5 0 1 1 .708.708L2.707 4H12.5A2.5 2.5 0 0 1 15 6.5v8a.5.5 0 0 1-1 0v-8A1.5 1.5 0 0 0 12.5 5H2.707l3.147 3.146a.5.5 0 1 1-.708.708z"/>
                            </svg>
                        </button>
                        <div className={styles['header-title']}>
                            <h1>{component.name}</h1>
                        </div>
                    </div>


                    {component.factoryLink && (
                        <div className={styles['header-right']}>
                            <a
                                href={component.factoryLink}
                                target="_blank"
                                rel="noopener noreferrer"
                                className={styles['factory-link-button']}
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                     viewBox="0 0 16 16">
                                    <path fillRule="evenodd"
                                          d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z"/>
                                    <path fillRule="evenodd"
                                          d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z"/>
                                </svg>
                                На сайт виробника
                            </a>
                        </div>
                    )}
                </div>

                <div className={styles['component-main']}>
                    <div className={styles['image-container']}>
                        <div className={styles['component-image']}>
                            {component.photoUrl ? (
                                <img
                                    src={component.photoUrl}
                                    alt={component.name}
                                />
                            ) : (
                                <div className={styles['no-image']}>No image available</div>
                            )}
                        </div>
                        {!component.description && (
                            <div className={styles['component-price']}>
                                <h2>Середня ціна: <span className={styles['price']}>{component.averagePrice} грн</span></h2>
                            </div>
                        )}
                    </div>

                    <div className={styles['component-info']}>
                        {component.description && (
                            <div className={styles['component-price']}>
                                <h2>Середня ціна: <span className={styles['price']}>{component.averagePrice} грн</span></h2>
                            </div>
                        )}
                        {component.description ? (
                            <div className={styles['component-description']}>
                                <h3>Опис</h3>
                                <div className={`${styles['description-content']} ${
                                    !isDescriptionExpanded && component.description.length > 500
                                        ? styles['collapsed']
                                        : ''
                                }`}>
                                    <p>{component.description}</p>
                                </div>
                                {component.description.length > 500 && (
                                    <button
                                        className={styles['description-toggle']}
                                        onClick={toggleDescription}
                                    >
                                        {isDescriptionExpanded ? 'Приховати ↑' : 'Розгорнути ↓'}
                                    </button>
                                )}
                            </div>
                        ) : (
                            <div className={`${styles['component-specs']}`}>
                                <h3>Характеристики</h3>
                                <table className={styles['specs-table']}>
                                    <tbody>
                                    {renderSpecsFromConfig()}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                </div>
                {component.description && (
                    <div className={styles['component-specs']}>
                        <h3>Характеристики</h3>
                        <table className={styles['specs-table']}>
                            <tbody>
                            {renderSpecsFromConfig()}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Offers Section - With individual add buttons */}
                {component.productOffers && component.productOffers.length > 0 && (
                    <div className={styles['offers-section']}>
                        <h3>Наявні Пропозиції ({component.offersCount})</h3>
                        <div className={styles['offers-table-container']}>
                            <div className={styles['offers-table-body-container']}>
                                <table className={styles['offers-table']}>
                                    <thead>
                                    <tr>
                                        <th>Магазин</th>
                                        <th>Ціна</th>
                                        <th></th>
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {component.productOffers.map((offer, index) => (
                                        <tr key={index}>
                                            <td>
                                                <div className={styles['store-info']}>
                                                    {offer.store.logoUrl && (
                                                        <img
                                                            src={offer.store.logoUrl}
                                                            alt={offer.store.name}
                                                            className={styles['store-logo']}
                                                        />
                                                    )}
                                                    <span>{offer.store.name}</span>
                                                    <span className={`${styles['rating-basic']} ${styles['likes']}`}>
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                                             fill="currentColor" className="bi bi-hand-thumbs-up-fill"
                                                             viewBox="0 0 16 16">
                                                            <path
                                                                d="M6.956 1.745C7.021.81 7.908.087 8.864.325l.261.066c.463.116.874.456 1.012.965.22.816.533 2.511.062 4.51a10 10 0 0 1 .443-.051c.713-.065 1.669-.072 2.516.21.518.173.994.681 1.2 1.273.184.532.16 1.162-.234 1.733q.086.18.138.363c.077.27.113.567.113.856s-.036.586-.113.856c-.039.135-.09.273-.16.404.169.387.107.819-.003 1.148a3.2 3.2 0 0 1-.488.901c.054.152.076.312.076.465 0 .305-.089.625-.253.912C13.1 15.522 12.437 16 11.5 16H8c-.605 0-1.07-.081-1.466-.218a4.8 4.8 0 0 1-.97-.484l-.048-.03c-.504-.307-.999-.609-2.068-.722C2.682 14.464 2 13.846 2 13V9c0-.85.685-1.432 1.357-1.615.849-.232 1.574-.787 2.132-1.41.56-.627.914-1.28 1.039-1.639.199-.575.356-1.539.428-2.59z"/>
                                                        </svg>
                                                        {offer.store.likes}
                                                    </span>
                                                    <span className={`${styles['rating-basic']} ${styles['dislikes']}`}>
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                                             fill="currentColor" className="bi bi-hand-thumbs-down-fill"
                                                             viewBox="0 0 16 16">
                                                            <path
                                                                d="M6.956 14.534c.065.936.952 1.659 1.908 1.42l.261-.065a1.38 1.38 0 0 0 1.012-.965c.22-.816.533-2.512.062-4.51q.205.03.443.051c.713.065 1.669.071 2.516-.211.518-.173.994-.68 1.2-1.272a1.9 1.9 0 0 0-.234-1.734c.058-.118.103-.242.138-.362.077-.27.113-.568.113-.856 0-.29-.036-.586-.113-.857a2 2 0 0 0-.16-.403c.169-.387.107-.82-.003-1.149a3.2 3.2 0 0 0-.488-.9c.054-.153.076-.313.076-.465a1.86 1.86 0 0 0-.253-.912C13.1.757 12.437.28 11.5.28H8c-.605 0-1.07.08-1.466.217a4.8 4.8 0 0 0-.97.485l-.048.029c-.504.308-.999.61-2.068.723C2.682 1.815 2 2.434 2 3.279v4c0 .851.685 1.433 1.357 1.616.849.232 1.574.787 2.132 1.41.56.626.914 1.28 1.039 1.638.199.575.356 1.54.428 2.591"/>
                                                        </svg>
                                                        {offer.store.dislikes}
                                                    </span>
                                                </div>
                                            </td>
                                            <td>
                                                <p className={styles['offer-price']}>{offer.price} грн</p>
                                            </td>
                                            <td>
                                                <div className={styles['offer-buttons']}>
                                                    <button
                                                        className={`button-primary ${styles['offers-button']}`}
                                                        onClick={() => handleAddToBuild(offer)}
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                                             fill="currentColor" className="bi bi-plus-square"
                                                             viewBox="0 0 16 16" stroke="currentColor" strokeWidth="0.7">
                                                            <path
                                                                d="M14 1a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1zM2 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2z"/>
                                                            <path
                                                                d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                                                        </svg>
                                                        Додати до Збірки
                                                    </button>
                                                    <a
                                                        href={offer.productOfferUrl}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className={`button-secondary ${styles['offers-button']}`}
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                                             fill="currentColor"
                                                             viewBox="0 0 16 16">
                                                            <path fillRule="evenodd"
                                                                  d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z"/>
                                                            <path fillRule="evenodd"
                                                                  d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z"/>
                                                        </svg>
                                                        Купити
                                                    </a>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

export default ComponentPage;