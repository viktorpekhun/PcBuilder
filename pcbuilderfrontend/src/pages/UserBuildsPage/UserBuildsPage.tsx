import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import styles from './UserBuildsPage.module.css';
import useAuth from '../../hooks/useAuth';
import DeleteModal from "../../components/DeleteModal/DeleteModal";
import { Button } from '../../components/Button/Button';
import { buildService } from '../../api/build.service';
import type { IPcBuildList, IPcBuildRequest, IComponentPreview, IMultiComponentPreview } from '../../types/build.types';
import type { SelectedComponents, SingleKey, MultiKey} from '../PcBuildPage/types';

// --- Component type configs ---

interface SingleConfig { key: keyof Pick<IPcBuildRequest, SingleKey>; label: string; urlType: string }
interface MultiConfig { key: keyof Pick<IPcBuildRequest, MultiKey>; label: string; urlType: string }

const SINGLE_COMPONENTS: SingleConfig[] = [
    { key: 'cpu', label: 'Процесор', urlType: 'cpu' },
    { key: 'gpu', label: 'Відеокарта', urlType: 'gpu' },
    { key: 'motherboard', label: 'Материнська плата', urlType: 'motherboard' },
    { key: 'powerSupply', label: 'Блок живлення', urlType: 'powerSupply' },
    { key: 'cpuCooler', label: 'Процесорний кулер', urlType: 'cpuCooler' },
    { key: 'pcCase', label: 'Корпус', urlType: 'pcCase' },
];

const MULTI_COMPONENTS: MultiConfig[] = [
    { key: 'rams', label: "Оперативна пам'ять", urlType: 'ram' },
    { key: 'ssds', label: 'SSD Диск', urlType: 'ssd' },
    { key: 'hdds', label: 'HDD Диск', urlType: 'hdd' },
    { key: 'fans', label: 'Вентилятор', urlType: 'fan' },
];

// --- State types ---

interface DeleteModalState {
    isOpen: boolean;
    buildId: string | null;
    buildName: string;
}

interface DeleteStatus {
    loading: boolean;
    error: string | null;
}

// --- SVG icons ---

const ExternalLinkIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path fillRule="evenodd" d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z"/>
        <path fillRule="evenodd" d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z"/>
    </svg>
);

const PencilIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
         fill="currentColor" className="bi bi-pencil-square" viewBox="0 0 16 16">
        <path d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/>
        <path fillRule="evenodd" d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z"/>
    </svg>
);

const TrashIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
         fill="currentColor" className="bi bi-trash3" viewBox="0 0 16 16">
        <path d="M6.5 1h3a.5.5 0 0 1 .5.5v1H6v-1a.5.5 0 0 1 .5-.5M11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47M8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5"/>
    </svg>
);

const ClockIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
         fill="currentColor" className="bi bi-clock" viewBox="0 0 16 16">
        <path d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71z"/>
        <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16m7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0"/>
    </svg>
);

// --- Component ---

function UserBuildsPage() {
    const [builds, setBuilds] = useState<IPcBuildList[]>([]);
    const [selectedBuild, setSelectedBuild] = useState<IPcBuildRequest | null>(null);
    const [selectedBuildId, setSelectedBuildId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deleteStatus, setDeleteStatus] = useState<DeleteStatus>({ loading: false, error: null });
    const [deleteModal, setDeleteModal] = useState<DeleteModalState>({ isOpen: false, buildId: null, buildName: '' });

    const navigate = useNavigate();
    const { auth } = useAuth();

    // --- Fetch builds ---

    useEffect(() => {
        const fetchUserBuilds = async () => {
            try {
                setLoading(true);
                const { data } = await buildService.getUserBuilds();
                setBuilds(data);
                if (data.length > 0) setSelectedBuildId(data[0]!.id);
                setError(null);
            } catch {
                setError('Failed to load your builds. Please try again later.');
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

    useEffect(() => {
        if (!selectedBuildId) return;
        const fetchBuild = async () => {
            try {
                const { data } = await buildService.getBuildById(selectedBuildId);
                setSelectedBuild(data);
            } catch (err) {
                console.error(`Error fetching build ${selectedBuildId}:`, err);
            }
        };
        fetchBuild();
    }, [selectedBuildId]);

    // --- Handlers ---

    const handleDeleteBuild = async () => {
        if (!deleteModal.buildId) return;
        try {
            setDeleteStatus({ loading: true, error: null });
            await buildService.deleteBuild(deleteModal.buildId);

            setBuilds(prev => prev.filter(b => b.id !== deleteModal.buildId));

            if (selectedBuildId === deleteModal.buildId) {
                const remaining = builds.filter(b => b.id !== deleteModal.buildId);
                if (remaining.length > 0) {
                    setSelectedBuildId(remaining[0]!.id);
                } else {
                    setSelectedBuildId(null);
                    setSelectedBuild(null);
                }
            }

            setDeleteStatus({ loading: false, error: null });
            setDeleteModal({ isOpen: false, buildId: null, buildName: '' });
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message || 'Failed to delete build. Please try again.';
            setDeleteStatus({ loading: false, error: msg });
        }
    };

    const handleEditBuild = () => {
        if (!selectedBuild) return;

        const transformed: SelectedComponents = {
            cpu: null, gpu: null, motherboard: null,
            powerSupply: null, cpuCooler: null, pcCase: null,
            rams: [], ssds: [], hdds: [], fans: [],
        };

        for (const { key } of SINGLE_COMPONENTS) {
            const comp = selectedBuild[key];
            if (comp) {
                transformed[key] = {
                    componentId: comp.id,
                    offerId: comp.offerId,
                    price: comp.price ?? 0,
                    storeName: comp.storeName ?? '',
                    storeLogoUrl: comp.imageUrl ?? null,
                    productOfferUrl: comp.productOfferUrl ?? null,
                };
            }
        }

        for (const { key } of MULTI_COMPONENTS) {
            const items = selectedBuild[key];
            if (items.length > 0) {
                transformed[key] = items.map(item => ({
                    componentId: item.id,
                    offerId: item.offerId,
                    quantity: item.quantity || 1,
                    price: item.totalPrice ? item.totalPrice / (item.quantity || 1) : 0,
                    storeName: item.storeName ?? '',
                    storeLogoUrl: item.imageUrl ?? null,
                    productOfferUrl: item.productOfferUrl ?? null,
                }));
            }
        }

        localStorage.setItem('selectedComponents', JSON.stringify(transformed));
        localStorage.setItem('editingBuild', JSON.stringify({
            id: selectedBuild.id,
            name: selectedBuild.name,
            ...(selectedBuild.description && { description: selectedBuild.description }),
        }));
        navigate('/');
    };

    const openDeleteModal = (buildId: string | null) => {
        if (!buildId) return;
        const build = builds.find(b => b.id === buildId);
        if (build) {
            setDeleteModal({ isOpen: true, buildId, buildName: build.name });
        }
    };

    const handleCreateNewBuild = () => {
        localStorage.removeItem('selectedComponents');
        localStorage.removeItem('editingBuild');
        navigate('/');
    };

    // --- Render helpers ---

    const renderSingleRow = (comp: IComponentPreview, label: string, urlType: string) => (
        <tr key={`${label}-${comp.id}`}>
            <td>{label}</td>
            <td className={styles['component-cell']}>
                <div className={styles['component-info']}>
                    {comp.imageUrl && <img src={comp.imageUrl} alt={comp.name} />}
                    <div>
                        <div className={styles['component-name']}>
                            <Link to={`/components/${urlType}/${comp.id}`} className={styles['component-link']}>
                                {comp.name}
                            </Link>
                        </div>
                    </div>
                </div>
            </td>
            <td>
                <div className={styles['store-info']}>
                    <span>{comp.storeName || 'Середня ціна'}</span>
                </div>
            </td>
            <td><span className={styles['price-info']}>{comp.price}</span></td>
            <td className={styles['offer-link-cell']}>
                {comp.productOfferUrl ? (
                    <Button variant="secondary" size="sm" href={comp.productOfferUrl} target="_blank" rel="noopener noreferrer">
                        <ExternalLinkIcon /> Купити
                    </Button>
                ) : (
                    <span className={styles['no-offer']}>-</span>
                )}
            </td>
        </tr>
    );

    const renderMultiRow = (comp: IMultiComponentPreview, label: string, urlType: string, index: number) => (
        <tr key={`${label}-${comp.id || index}`}>
            <td>{label}</td>
            <td className={styles['component-cell']}>
                <div className={styles['component-info']}>
                    {comp.imageUrl && <img src={comp.imageUrl} alt={comp.name} />}
                    <div>
                        <div className={styles['component-name']}>
                            <Link to={`/components/${urlType}/${comp.id}`} className={styles['component-link']}>
                                {comp.name}
                            </Link>
                        </div>
                        {comp.quantity > 1 && (
                            <div className={styles['quantity']}>Кількість: {comp.quantity}</div>
                        )}
                    </div>
                </div>
            </td>
            <td>
                <div className={styles['store-info']}>
                    <span>{comp.storeName || 'Середня ціна'}</span>
                </div>
            </td>
            <td><span className={styles['price-info']}>{comp.totalPrice}</span></td>
            <td className={styles['offer-link-cell']}>
                {comp.productOfferUrl ? (
                    <Button variant="secondary" size="sm" href={comp.productOfferUrl} target="_blank" rel="noopener noreferrer">
                        <ExternalLinkIcon /> Купити
                    </Button>
                ) : (
                    <span className={styles['no-offer']}>-</span>
                )}
            </td>
        </tr>
    );

    // --- Early returns ---

    if (loading) return <div className={styles['loading']}>Loading your builds...</div>;

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
                                onClick={() => setSelectedBuildId(build.id)}
                            >
                                <div className={styles['name-date-row']}>
                                    <div className={styles['build-name']}>{build.name}</div>
                                    <div className={styles['build-date']}>
                                        {build.updatedAt && new Date(build.updatedAt).toLocaleDateString()}
                                    </div>
                                </div>
                                <div className={styles['build-price']}>{build.price} грн</div>
                            </li>
                        ))}
                    </ul>
                    <div className={styles['builds-list-footer']}>
                        <Button variant='primary' size='lg' onClick={handleCreateNewBuild}>
                            Створити Нову Збірку
                        </Button>
                    </div>
                </div>

                {/* Right content - selected build details */}
                <div className={styles['build-details']}>
                    {selectedBuild ? (
                        <>
                            <div className={styles['build-header']}>
                                <div className={styles['build-title-area']}>
                                    <div className={styles['build-dates']}>
                                        <ClockIcon />
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
                                    <Button variant='outline-secondary' size='sm' onClick={handleEditBuild}>
                                        <PencilIcon />
                                    </Button>
                                    <Button variant='danger' size='sm' onClick={() => openDeleteModal(selectedBuildId)}>
                                        <TrashIcon />
                                    </Button>
                                </div>
                            </div>

                            {deleteStatus.error && (
                                <div className={styles['error-message']}>{deleteStatus.error}</div>
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
                                    {SINGLE_COMPONENTS.map(({ key, label, urlType }) => {
                                        const comp = selectedBuild[key];
                                        return comp ? renderSingleRow(comp, label, urlType) : null;
                                    })}
                                    {MULTI_COMPONENTS.map(({ key, label, urlType }) =>
                                        selectedBuild[key].map((item, idx) =>
                                            renderMultiRow(item, label, urlType, idx)
                                        )
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
