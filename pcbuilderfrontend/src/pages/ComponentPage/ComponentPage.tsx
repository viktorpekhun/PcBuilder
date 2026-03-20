import { useState, useEffect, Fragment } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import styles from './ComponentPage.module.css';
import { componentService } from '../../api/component.service';
import type { ComponentType, IComponentBase, IProductOffer } from '../../types/component.types';
import { componentSpecFullConfigs } from './componentSpecsFullConfigs';
import type { FullSpecConfig } from './componentSpecsFullConfigs';
import { Button } from '../../components/Button/Button';

interface SelectedComponentOffer {
    componentId: string;
    offerId: string;
    price: number;
    storeName: string;
    storeLogoUrl: string | null | undefined;
    productOfferUrl: string;
}

interface SelectedMultiComponent extends SelectedComponentOffer {
    quantity: number;
}

type SelectedComponents = Record<string, SelectedComponentOffer | SelectedMultiComponent[]>;

const MULTI_COMPONENT_TYPES = ['ram', 'ssd', 'hdd', 'fan'];

function ComponentPage() {
    const { type, id } = useParams<{ type: string; id: string }>();
    const navigate = useNavigate();
    const [component, setComponent] = useState<IComponentBase | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isDescriptionExpanded, setIsDescriptionExpanded] = useState(false);

    useEffect(() => {
        const fetchComponentData = async () => {
            try {
                setLoading(true);
                const response = await componentService.getById(type as ComponentType, id!);
                setComponent(response.data);
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

    const handleAddToBuild = (offer: IProductOffer) => {
        try {
            let selectedComponents: SelectedComponents = {};
            const savedComponents = localStorage.getItem('selectedComponents');

            if (savedComponents) {
                selectedComponents = JSON.parse(savedComponents);
            }

            const componentWithOffer: SelectedComponentOffer = {
                componentId: component!.id,
                offerId: offer.id,
                price: offer.price,
                storeName: offer.store.name,
                storeLogoUrl: offer.store.logoUrl,
                productOfferUrl: offer.productOfferUrl
            };

            if (MULTI_COMPONENT_TYPES.includes(type!)) {
                const arrayName = `${type}s`;

                if (!selectedComponents[arrayName]) {
                    selectedComponents[arrayName] = [];
                }

                const arr = selectedComponents[arrayName] as SelectedMultiComponent[];
                const existingIndex = arr.findIndex(
                    item => item.componentId === component!.id
                );

                if (existingIndex >= 0) {
                    const currentQuantity = arr[existingIndex]!.quantity;
                    arr[existingIndex] = {
                        ...componentWithOffer,
                        quantity: currentQuantity + 1
                    };
                } else {
                    arr.push({ ...componentWithOffer, quantity: 1 });
                }
            } else {
                selectedComponents[type!] = componentWithOffer;
            }

            localStorage.setItem('selectedComponents', JSON.stringify(selectedComponents));
            navigate('/');
        } catch (err) {
            console.error("Error adding component to build:", err);
        }
    };

    const renderSpecsFromConfig = () => {
        if (!component) return null;

        const specs: FullSpecConfig[] = componentSpecFullConfigs[type!]
            ?? componentSpecFullConfigs['default']
            ?? [];

        return specs.map((spec, index) => {
            if ('type' in spec && spec.type === 'sectionHeader') {
                return (
                    <tr key={`section-${index}`} className={styles['spec-section-header']}>
                        <td colSpan={2}>{spec.label}</td>
                    </tr>
                );
            }

            if ('key' in spec) {
                if (spec.isList) {
                    const arr = component[spec.key];
                    if (!Array.isArray(arr) || arr.length === 0) return null;

                    if ('renderAsSeparateRows' in spec && spec.renderAsSeparateRows) {
                        return (
                            <Fragment key={`${spec.key}-${index}`}>
                                {spec.label && (
                                    <tr className={styles['spec-section-header']}>
                                        <td colSpan={2}>{spec.label}</td>
                                    </tr>
                                )}
                                {arr.map((item: Record<string, unknown>, idx: number) => {
                                    const label = spec.getLabelFromItem(item);
                                    const value = spec.getValueFromItem(item);
                                    return (
                                        <tr key={`${spec.key}-item-${idx}`}>
                                            <td>{label}:</td>
                                            <td>{String(value)}{spec.valueUnit && ` ${spec.valueUnit}`}</td>
                                        </tr>
                                    );
                                })}
                            </Fragment>
                        );
                    }

                    const formattedItems = spec.formatList(
                        arr.map((item: Record<string, unknown>) => spec.formatItem(item))
                    );

                    return (
                        <tr key={`${spec.key}-${index}`}>
                            <td>{spec.label || spec.key}:</td>
                            <td>{formattedItems}{spec.unit && ` ${spec.unit}`}</td>
                        </tr>
                    );
                }

                const value = component[spec.key];
                if (value == null || value === '') return null;

                return (
                    <tr key={`${spec.key}-${index}`}>
                        <td>{spec.label || spec.key}:</td>
                        <td>{String(value)}{spec.unit && ` ${spec.unit}`}</td>
                    </tr>
                );
            }

            if ('keys' in spec) {
                const values: Record<string, unknown> = {};
                let hasValue = false;

                spec.keys.forEach(key => {
                    values[key] = component[key];
                    if (component[key] != null && component[key] !== '') hasValue = true;
                });

                if (!hasValue) return null;

                const formattedValue = spec.format(values);
                if (!formattedValue) return null;

                return (
                    <tr key={`${spec.keys.join('-')}-${index}`}>
                        <td>{spec.label || spec.keys[0]}:</td>
                        <td>{formattedValue}{spec.unit && ` ${spec.unit}`}</td>
                    </tr>
                );
            }

            return null;
        }).filter(Boolean);
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

    const description = component.description as string | undefined;
    const factoryLink = component.factoryLink as string | undefined;
    const productOffers = component.productOffers as IProductOffer[] | undefined;

    return (
        <div className={styles['component-page']}>
            <div className={styles['component-details']}>
                <div className={styles['component-header']}>
                    <div className={styles['header-left']}>
                        <div className={styles['header-title']}>
                            <h1>{component.name}</h1>
                        </div>
                    </div>

                    {factoryLink && (
                        <div className={styles['header-right']}>
                            <a
                                href={factoryLink}
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
                        {!description && (
                            <div className={styles['component-price']}>
                                <h2>Середня ціна: <span className={styles['price']}>{component.averagePrice} грн</span></h2>
                            </div>
                        )}
                    </div>

                    <div className={styles['component-info']}>
                        {description && (
                            <div className={styles['component-price']}>
                                <h2>Середня ціна: <span className={styles['price']}>{component.averagePrice} грн</span></h2>
                            </div>
                        )}
                        {description ? (
                            <div className={styles['component-description']}>
                                <h3>Опис</h3>
                                <div className={`${styles['description-content']} ${
                                    !isDescriptionExpanded && description.length > 500
                                        ? styles['collapsed']
                                        : ''
                                }`}>
                                    <p>{description}</p>
                                </div>
                                {description.length > 500 && (
                                    <button
                                        className={styles['description-toggle']}
                                        onClick={toggleDescription}
                                    >
                                        {isDescriptionExpanded ? 'Приховати ↑' : 'Розгорнути ↓'}
                                    </button>
                                )}
                            </div>
                        ) : (
                            <div className={styles['component-specs']}>
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
                {description && (
                    <div className={styles['component-specs']}>
                        <h3>Характеристики</h3>
                        <table className={styles['specs-table']}>
                            <tbody>
                            {renderSpecsFromConfig()}
                            </tbody>
                        </table>
                    </div>
                )}

                {productOffers && productOffers.length > 0 && (
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
                                    {productOffers.map((offer, index) => (
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
                                                    <Button
                                                        variant='primary'
                                                        size='sm'
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
                                                    </Button>
                                                    <Button
                                                        variant="secondary"
                                                        size="sm"
                                                        href={offer.productOfferUrl}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
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
                                                    </Button>
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
