import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import styles from './ComponentPage.module.css';
import { getComponentById } from '../../services/componentService';

function ComponentPage() {
    const { type, id } = useParams();
    const navigate = useNavigate();
    const [component, setComponent] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

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
    const handleAddWithAveragePrice = () => {
        handleAddToBuild(null);
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
            <button
                className={styles['back-button']}
                onClick={() => navigate(`/components/${type}`)}
            >
                Back to {type.toUpperCase()} list
            </button>

            <div className={styles['component-details']}>
                <div className={styles['component-header']}>
                    <h1>{component.name}</h1>
                    {(!component.productOffers || component.productOffers.length === 0) && (
                        <button
                            className={styles['add-button']}
                            onClick={handleAddWithAveragePrice}
                        >
                            Add to Build ({component.averagePrice} UAH)
                        </button>
                    )}
                </div>

                <div className={styles['component-main']}>
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

                    <div className={styles['component-info']}>
                        <div className={styles['component-price']}>
                            <h2>Average Price: {component.averagePrice} UAH</h2>
                        </div>

                        <div className={styles['component-specs']}>
                            <h3>Specifications</h3>
                            <table className={styles['specs-table']}>
                                <tbody>
                                {/* Render specs based on component type */}
                                {type === 'cpu' && (
                                    <>
                                        <tr>
                                            <td>Base Clock</td>
                                            <td>{component.basicFrequency} GHz</td>
                                        </tr>
                                        <tr>
                                            <td>Max Clock</td>
                                            <td>{component.maxFrequency} GHz</td>
                                        </tr>
                                        <tr>
                                            <td>Cores</td>
                                            <td>{component.cores}</td>
                                        </tr>
                                        <tr>
                                            <td>Threads</td>
                                            <td>{component.threads}</td>
                                        </tr>
                                        <tr>
                                            <td>Socket</td>
                                            <td>{component.socket}</td>
                                        </tr>
                                        <tr>
                                            <td>TDP</td>
                                            <td>{component.tdp}W</td>
                                        </tr>
                                    </>
                                )}
                                {type === 'gpu' && (
                                    <>
                                        <tr>
                                            <td>Memory</td>
                                            <td>{component.memory} GB</td>
                                        </tr>
                                        <tr>
                                            <td>Memory Type</td>
                                            <td>{component.memoryType}</td>
                                        </tr>
                                        <tr>
                                            <td>Core Clock</td>
                                            <td>{component.coreClock} MHz</td>
                                        </tr>
                                    </>
                                )}
                                {/* Add more specs for other component types */}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Description Section */}
                {component.description && (
                    <div className={styles['component-description']}>
                        <h3>Description</h3>
                        <p>{component.description}</p>
                    </div>
                )}

                {/* Offers Section - With individual add buttons */}
                {component.productOffers && component.productOffers.length > 0 && (
                    <div className={styles['offers-section']}>
                        <h3>Available Offers ({component.offersCount})</h3>
                        <table className={styles['offers-table']}>
                            <thead>
                            <tr>
                                <th>Store</th>
                                <th>Rating</th>
                                <th>Price</th>
                                <th>External Link</th>
                                <th>Add to Build</th>
                            </tr>
                            </thead>
                            <tbody>
                            {component.productOffers.map((offer, index) => (
                                <tr key={index}>
                                    <td className={styles['store-info']}>
                                        {offer.store.logoUrl && (
                                            <img
                                                src={offer.store.logoUrl}
                                                alt={offer.store.name}
                                                className={styles['store-logo']}
                                            />
                                        )}
                                        <span>{offer.store.name}</span>
                                    </td>
                                    <td className={styles['store-rating']}>
                                        <span className={styles['likes']}>👍 {offer.store.likes}</span>
                                        <span className={styles['dislikes']}>👎 {offer.store.dislikes}</span>
                                    </td>
                                    <td className={styles['offer-price']}>
                                        {offer.price} UAH
                                    </td>
                                    <td>
                                        <a
                                            href={offer.productOfferUrl}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className={styles['view-offer-btn']}
                                        >
                                            View Offer
                                        </a>
                                    </td>
                                    <td>
                                        <button
                                            className={styles['add-offer-btn']}
                                            onClick={() => handleAddToBuild(offer)}
                                        >
                                            Add to Build
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
}

export default ComponentPage;