import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import styles from './UserBuildsPage.module.css';
import useAuth from '../../hooks/useAuth';
import useAxiosPrivate from '../../hooks/useAxiosPrivate';
import DeleteModal from "../../components/DeleteModal/DeleteModal.jsx";

const USER_BUILDS = '/api/pcBuild/user-builds';
const USER_BUILD = '/api/pcBuild';

// Component type configurations
const COMPONENT_TYPES = {
  // Single components
  singleComponents: [
    { key: 'cpu', label: 'Процесор' },
    { key: 'gpu', label: 'Відеокарта' },
    { key: 'motherboard', label: 'Материнська плата' },
    { key: 'powerSupply', label: 'Блок живлення' },
    { key: 'cpuCooler', label: 'Процесорний кулер' },
    { key: 'pcCase', label: 'Корпус' },
  ],
  // Multi-components (arrays with quantities)
  multiComponents: [
    { key: 'rams', label: "Оперативна пам'ять", singular: 'ram' },
    { key: 'ssds', label: 'SSD Диск', singular: 'ssd' },
    { key: 'hdds', label: 'HDD Диск', singular: 'hdd' },
    { key: 'fans', label: 'Вентилятор', singular: 'fan' },
  ]
};

function UserBuildsPage() {
    const [builds, setBuilds] = useState([]);
    const [selectedBuild, setSelectedBuild] = useState(null);
    const [selectedBuildId, setSelectedBuildId] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    const [error, setError] = useState(null);
    const [deleteStatus, setDeleteStatus] = useState({ loading: false, error: null });
    const { auth } = useAuth();
    const axiosPrivate = useAxiosPrivate();
    const [deleteModal, setDeleteModal] = useState({
        isOpen: false,
        buildId: null,
        buildName: ''
    });

    const handleDeleteBuild = async () => {
        if (!deleteModal.buildId) return;

        try {
            setDeleteStatus({ loading: true, error: null });
            await axiosPrivate.delete(`${USER_BUILD}/${deleteModal.buildId}`);

            setBuilds(prevBuilds => prevBuilds.filter(build => build.id !== deleteModal.buildId));

            if (selectedBuildId === deleteModal.buildId) {
                const remainingBuilds = builds.filter(build => build.id !== deleteModal.buildId);
                if (remainingBuilds.length > 0) {
                    setSelectedBuildId(remainingBuilds[0].id);
                } else {
                    setSelectedBuildId(null);
                    setSelectedBuild(null);
                }
            }

            setDeleteStatus({ loading: false, error: null });
            setDeleteModal({ isOpen: false, buildId: null, buildName: '' });
        } catch (err) {
            console.error('Error deleting build:', err);
            setDeleteStatus({
                loading: false,
                error: err.response?.data?.message || 'Failed to delete build. Please try again.'
            });
        }
    };

    const handleEditBuild = () => {
        if (!selectedBuild) return;
        
        // Transform the selected build data to the format expected by PcBuildPage
        const transformedComponents = {};

        // Transform single components
        COMPONENT_TYPES.singleComponents.forEach(({ key }) => {
            if (selectedBuild[key]) {
                transformedComponents[key] = {
                    componentId: selectedBuild[key].id,
                    price: selectedBuild[key].price,
                    storeName: selectedBuild[key].storeName,
                    storeLogoUrl: selectedBuild[key].storeLogoUrl || selectedBuild[key].imageUrl,
                    productOfferUrl: selectedBuild[key].productOfferUrl,
                    offerId: selectedBuild[key].offerId
                };
            }
        });

        // Transform multi-components
        COMPONENT_TYPES.multiComponents.forEach(({ key }) => {
            if (selectedBuild[key] && selectedBuild[key].length > 0) {
                transformedComponents[key] = selectedBuild[key].map(item => ({
                    componentId: item.id,
                    quantity: item.quantity || 1,
                    price: item.price || (item.totalPrice / (item.quantity || 1)),
                    storeName: item.storeName,
                    storeLogoUrl: item.storeLogoUrl || item.imageUrl,
                    productOfferUrl: item.productOfferUrl,
                    offerId: item.offerId
                }));
            } else {
                transformedComponents[key] = [];
            }
        });

        // Store data for PcBuildPage
        localStorage.setItem('selectedComponents', JSON.stringify(transformedComponents));
        localStorage.setItem('editingBuild', JSON.stringify({
            id: selectedBuild.id,
            name: selectedBuild.name,
            description: selectedBuild.description
        }));

        // Navigate to the PC Builder page
        navigate('/');
    };

    const openDeleteModal = (buildId) => {
        const buildToDelete = builds.find(build => build.id === buildId);
        if (buildToDelete) {
            setDeleteModal({
                isOpen: true,
                buildId: buildId,
                buildName: buildToDelete.name
            });
        }
    };

    const handleCreateNewBuild = () => {
        localStorage.removeItem('selectedComponents');
        localStorage.removeItem('editingBuild');
        navigate('/');
    };

    // Fetch all user builds
    useEffect(() => {
        const fetchUserBuilds = async () => {
            try {
                setLoading(true);
                const response = await axiosPrivate.get(USER_BUILDS);
                const data = response.data;
                setBuilds(data);

                if (data.length > 0) {
                    setSelectedBuildId(data[0].id);
                }
                setError(null);
            } catch (err) {
                setError('Failed to load your builds. Please try again later.');
                console.error('Error fetching user builds:', err);
            } finally {
                setLoading(false);
            }
        };

        if (auth?.accessToken) {
            fetchUserBuilds();
        } else {
            setError('You need to be logged in to view your builds.');
            setLoading(false);
        }
    }, [auth?.accessToken]);

    // Fetch selected build details
    useEffect(() => {
        if (!selectedBuildId) return;

        const fetchSelectedBuild = async () => {
            try {
                const response = await axiosPrivate.get(`${USER_BUILD}/${selectedBuildId}`);
                setSelectedBuild(response.data);
            } catch (err) {
                console.error(`Error fetching build ${selectedBuildId}:`, err);
            }
        };
        fetchSelectedBuild();
    }, [selectedBuildId]);

    // Handle build selection
    const handleSelectBuild = (buildId) => {
        setSelectedBuildId(buildId);
    };

    // Helper function to render a component row
    const renderComponentRow = (component, type, index = 0, isMulti = false) => {
        if (!component) return null;
        
        // Determine the component type for URL
        const getComponentType = () => {
            if (isMulti) {
                // Fix: Use a simpler approach to map labels to keys
                let key = '';
                if (type === "Оперативна пам'ять") key = 'rams';
                else if (type === 'SSD Диск') key = 'ssds';
                else if (type === 'HDD Диск') key = 'hdds';
                else if (type === 'Вентилятор') key = 'fans';
                
                // Find the mapping using the determined key
                const multiType = COMPONENT_TYPES.multiComponents.find(t => t.key === key);
                return multiType?.singular || '';
            } else {
                // For single components, find the mapping
                const singleType = COMPONENT_TYPES.singleComponents.find(t => t.label === type);
                return singleType?.key || '';
            }
        };
        
        const componentType = getComponentType();
        
        return (
            <tr key={`${type}-${component.id || index}`}>
                <td>
                    {type}
                </td>
                <td className={styles['component-cell']}>
                    <div className={styles['component-info']}>
                        {component.imageUrl && (
                            <img
                                src={component.imageUrl}
                                alt={component.name}
                            />
                        )}
                        <div>
                            <div className={styles['component-name']}>
                                {componentType ? (
                                    <Link 
                                        to={`/components/${componentType}/${component.id}`}
                                        className={styles['component-link']}
                                    >
                                        {component.name}
                                    </Link>
                                ) : (
                                    component.name
                                )}
                            </div>
                            {isMulti && component.quantity > 1 && (
                                <div className={styles['quantity']}>
                                    Кількість: {component.quantity}
                                </div>
                            )}
                        </div>
                    </div>
                </td>
                <td>
                    <div className={styles['store-info']}>
                        <span>{component.storeName || 'Середня ціна'}</span>
                    </div>
                </td>
                <td>
                    <span className={styles['price-info']}>{isMulti ? component.totalPrice : component.price}</span>
                </td>
                <td className={styles['offer-link-cell']}>
                    {component.productOfferUrl ? (
                        <a 
                            href={component.productOfferUrl} 
                            className={styles['offer-link-button']}
                            target="_blank" 
                            rel="noopener noreferrer"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                <path fillRule="evenodd" d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z"/>
                                <path fillRule="evenodd" d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z"/>
                            </svg>
                            Купити
                        </a>
                    ) : (
                        <span className={styles['no-offer']}>-</span>
                    )}
                </td>
            </tr>
        );
    };

    if (loading) {
        return <div className={styles['loading']}>Loading your builds...</div>;
    }

    if (error) {
        return (
            <div className={styles['error-container']}>
                <div className={styles['error']}>{error}</div>
                {!auth?.accessToken && (
                    <div className={styles['login-prompt']}>
                        <Link to="/login" className={styles['login-link']}>Log in</Link> to see your saved builds.
                    </div>
                )}
            </div>
        );
    }

    if (builds.length === 0) {
        return (
            <div className={styles['no-builds']}>
                <h2>You don't have any saved builds yet</h2>
                <p>Go to the <Link to="/">PC Builder</Link> to create your first build!</p>
            </div>
        );
    }

    return (
        <div className={styles['user-builds-page']}>
            <DeleteModal
                isOpen={deleteModal.isOpen}
                buildName={deleteModal.buildName}
                isDeleting={deleteStatus.loading}
                onCancel={() => setDeleteModal({ isOpen: false, buildId: null, buildName: '' })}
                onConfirm={handleDeleteBuild}
            />
            <div className={styles['builds-container']}>
                {/* Left sidebar - builds list */}
                <div className={styles['builds-list']}>
                    <h2>Мої Збірки</h2>
                    <ul>
                        {builds.map(build => (
                            <li
                                key={build.id}
                                className={`${styles['build-item']} ${selectedBuildId === build.id ? styles['selected'] : ''}`}
                                onClick={() => handleSelectBuild(build.id)}
                            >
                                <div className={styles['name-date-row']}>
                                    <div className={styles['build-name']}>{build.name}</div>
                                    <div className={styles['build-date']}>
                                        {new Date(build.updatedAt).toLocaleDateString()}
                                    </div>
                                </div>
                                <div className={styles['build-price']}>{build.price} грн</div>
                            </li>
                        ))}
                    </ul>
                    <button
                        onClick={handleCreateNewBuild}
                        className={`button-primary ${styles['new-build-button']}`}
                    >
                        Створити Нову Збірку
                    </button>
                </div>

                {/* Right content - selected build details */}
                <div className={styles['build-details']}>
                    {selectedBuild ? (
                        <>
                            <div className={styles['build-header']}>
                                <div className={styles['build-title-area']}>
                                    <div className={styles['build-dates']}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                             fill="currentColor" className="bi bi-clock" viewBox="0 0 16 16">
                                            <path
                                                d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71z"/>
                                            <path
                                                d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16m7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0"/>
                                        </svg>
                                        <span>Створено {new Date(selectedBuild.createdAt).toLocaleDateString()}</span>
                                        {selectedBuild.updatedAt && (
                                            <>
                                                <span className={styles['date-separator']}>•</span>
                                                <span>Оновлено {new Date(selectedBuild.updatedAt).toLocaleDateString()}</span>
                                            </>
                                        )}
                                    </div>
                                    <h2>{selectedBuild.name}</h2>
                                </div>
                                <div className={styles['build-actions']}>
                                    <button
                                        className={'button-secondary'}
                                        onClick={handleEditBuild}
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
                                             fill="currentColor" className="bi bi-pencil-square" viewBox="0 0 16 16">
                                            <path
                                                d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/>
                                            <path fill-rule="evenodd"
                                                  d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z"/>
                                        </svg>

                                    </button>
                                    <button
                                        className={styles['delete-button']}
                                        onClick={() => openDeleteModal(selectedBuildId)}
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
                                             fill="currentColor" className="bi bi-trash3" viewBox="0 0 16 16">
                                            <path
                                                d="M6.5 1h3a.5.5 0 0 1 .5.5v1H6v-1a.5.5 0 0 1 .5-.5M11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47M8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5"/>
                                        </svg>
                                    </button>
                                </div>
                            </div>

                            {deleteStatus.error && (
                                <div className={styles['error-message']}>
                                    {deleteStatus.error}
                                </div>
                            )}

                            {selectedBuild.description && (
                                <div className={styles['build-description']}>
                                    <h3>Опис</h3>
                                    <p>{selectedBuild.description}</p>
                                </div>
                            )}

                            <div className={styles['build-components']}>
                                <table className={styles['components-table']}>
                                    <thead>
                                    <tr>
                                        <th>Тип</th>
                                        <th>Компонент</th>
                                        <th>Магазин</th>
                                        <th>Ціна, грн</th>
                                        <th></th>
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {/* Dynamically render single components */}
                                    {COMPONENT_TYPES.singleComponents.map(({key, label}) =>
                                        selectedBuild[key] ? renderComponentRow(selectedBuild[key], label) : null
                                    )}

                                    {/* Dynamically render multi-components */}
                                    {COMPONENT_TYPES.multiComponents.map(({key, label}) =>
                                        selectedBuild[key]?.length > 0
                                            ? selectedBuild[key].map((item, idx) =>
                                                renderComponentRow(item, label, idx, true)
                                            )
                                            : null
                                    )}
                                    </tbody>
                                </table>
                            </div>
                            <div className={styles['build-total']}>
                                Остаточна ціна: <span>{selectedBuild.price} грн</span>
                        </div>

                        </>
                    ) : (
                        <div className={styles['no-build-selected']}>
                            Виберіть збірку із списку щоб побачити деталі
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default UserBuildsPage;