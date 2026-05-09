import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import styles from './UserBuildsPage.module.css';
import useAuth from '../../hooks/useAuth';
import DeleteModal from "../../components/DeleteModal/DeleteModal";
import { Button } from '../../components/Button/Button';
import { buildService } from '../../api/build.service';
import { profileService } from '../../api/profile.service';
import { priceAlertService } from '../../api/priceAlert.service';
import type { IPcBuildList, IPcBuildRequest, IComponentPreview, IMultiComponentPreview } from '../../types/build.types';
import type { IUserPriceAlert } from '../../types/priceAlert.types';
import type { SelectedComponents, SingleKey, MultiKey } from '../PcBuildPage/types';

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

const COMPONENT_TYPE_LABEL: Record<string, string> = {
    Cpu: 'Процесор',
    Gpu: 'Відеокарта',
    Ram: "Оперативна пам'ять",
    Motherboard: 'Материнська плата',
    CpuCooler: 'Процесорний кулер',
    PcCase: 'Корпус',
    PowerSupply: 'Блок живлення',
    Ssd: 'SSD Диск',
    Hdd: 'HDD Диск',
    Fan: 'Вентилятор',
};

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

type ActiveTab = 'builds' | 'subscriptions';

// --- SVG icons ---

const ExternalLinkIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path fillRule="evenodd" d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z" />
        <path fillRule="evenodd" d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z" />
    </svg>
);

const PencilIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
        fill="currentColor" className="bi bi-pencil-square" viewBox="0 0 16 16">
        <path d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z" />
        <path fillRule="evenodd" d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5z" />
    </svg>
);

const TrashIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20"
        fill="currentColor" className="bi bi-trash3" viewBox="0 0 16 16">
        <path d="M6.5 1h3a.5.5 0 0 1 .5.5v1H6v-1a.5.5 0 0 1 .5-.5M11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47M8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5" />
    </svg>
);

const ClockIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"
        fill="currentColor" className="bi bi-clock" viewBox="0 0 16 16">
        <path d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71z" />
        <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16m7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0" />
    </svg>
);

const BellIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2M8 1.918l-.797.161A4 4 0 0 0 4 6c0 .628-.134 2.197-.459 3.742-.16.767-.376 1.566-.663 2.258h10.244c-.287-.692-.502-1.49-.663-2.258C12.134 8.197 12 6.628 12 6a4 4 0 0 0-3.203-3.92zM14.22 12c.223.447.481.801.78 1H1c.299-.199.557-.553.78-1C2.68 10.2 3 6.88 3 6c0-2.42 1.72-4.44 4.005-4.901a1 1 0 1 1 1.99 0A5 5 0 0 1 13 6c0 .88.32 4.2 1.22 6" />
    </svg>
);

// --- Component ---

function UserBuildsPage() {
    const [activeTab, setActiveTab] = useState<ActiveTab>('builds');

    const [builds, setBuilds] = useState<IPcBuildList[]>([]);
    const [selectedBuild, setSelectedBuild] = useState<IPcBuildRequest | null>(null);
    const [selectedBuildId, setSelectedBuildId] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deleteStatus, setDeleteStatus] = useState<DeleteStatus>({ loading: false, error: null });
    const [deleteModal, setDeleteModal] = useState<DeleteModalState>({ isOpen: false, buildId: null, buildName: '' });
    const [publishLoading, setPublishLoading] = useState(false);
    const [postBanUntil, setPostBanUntil] = useState<string | null>(null);

    const [subscriptions, setSubscriptions] = useState<IUserPriceAlert[]>([]);
    const [subscriptionsLoading, setSubscriptionsLoading] = useState(false);
    const [subscriptionsError, setSubscriptionsError] = useState<string | null>(null);
    const [unsubscribingId, setUnsubscribingId] = useState<string | null>(null);

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

        if (!auth?.accessToken) {
            setLoading(false);
            return;
        }

        fetchUserBuilds();
        profileService.getBans().then(res => {
            setPostBanUntil(res.data.isPostBanned ? res.data.postBanUntil : null);
        }).catch(() => { });
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

    // --- Fetch subscriptions ---

    useEffect(() => {
        if (!auth?.accessToken) return;
        const fetchSubscriptions = async () => {
            try {
                setSubscriptionsLoading(true);
                setSubscriptionsError(null);
                const { data } = await priceAlertService.getMine();
                setSubscriptions(data);
            } catch {
                setSubscriptionsError('Не вдалося завантажити підписки. Спробуйте пізніше.');
            } finally {
                setSubscriptionsLoading(false);
            }
        };
        fetchSubscriptions();
    }, [auth?.accessToken]);

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

    const formatBanUntil = (dateStr: string) => new Date(dateStr).toLocaleString('uk-UA', {
        day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
    });

    const formatBanRemaining = (dateStr: string) => {
        const diffMs = new Date(dateStr).getTime() - Date.now();
        if (diffMs <= 0) return null;
        const mins = Math.floor(diffMs / 60000);
        const days = Math.floor(mins / 1440);
        const hours = Math.floor((mins % 1440) / 60);
        const minutes = mins % 60;
        if (days > 0) return `${days} д ${hours} год`;
        if (hours > 0) return `${hours} год ${minutes} хв`;
        return `${minutes} хв`;
    };

    const handleTogglePublish = async () => {
        if (!selectedBuild || publishLoading) return;
        try {
            setPublishLoading(true);
            const newState = !selectedBuild.isPublished;
            await buildService.publishBuild(selectedBuild.id, newState);
            setSelectedBuild(prev => {
                if (!prev) return null;
                const updated = { ...prev, isPublished: newState };
                if (newState) updated.publishedAt = new Date().toISOString();
                else delete updated.publishedAt;
                return updated;
            });
        } catch (err) {
            console.error('Failed to toggle publish:', err);
        } finally {
            setPublishLoading(false);
        }
    };

    const handleUnsubscribe = async (id: string) => {
        if (unsubscribingId) return;
        try {
            setUnsubscribingId(id);
            await priceAlertService.unsubscribe(id);
            setSubscriptions(prev => prev.filter(s => s.id !== id));
        } catch {
            setSubscriptionsError('Не вдалося скасувати підписку.');
        } finally {
            setUnsubscribingId(null);
        }
    };

    // --- Render helpers ---

    const renderSingleRow = (comp: IComponentPreview, label: string, urlType: string) => (
        <tr key={`${label}-${comp.id}`}>
            <td>{label}</td>
            <td className={styles['component-cell']}>
                <div className={styles['component-info']}>
                    {comp.imageUrl && <img src={comp.imageUrl} alt={comp.name} loading="lazy" />}
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
                    {comp.imageUrl && <img src={comp.imageUrl} alt={comp.name} loading="lazy" />}
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

    const renderSubscriptionCard = (sub: IUserPriceAlert) => {
        const urlType = sub.componentType.charAt(0).toLowerCase() + sub.componentType.slice(1);
        const typeLabel = COMPONENT_TYPE_LABEL[sub.componentType] ?? sub.componentType;
        const current = sub.currentAveragePrice;
        const baseline = sub.initialPrice;
        const deltaPercent = current != null && baseline > 0
            ? ((current - baseline) / baseline) * 100
            : null;
        const deltaClass =
            deltaPercent == null ? '' :
                deltaPercent > 0.1 ? styles['delta-up'] :
                    deltaPercent < -0.1 ? styles['delta-down'] :
                        styles['delta-flat'];

        return (
            <div key={sub.id} className={styles['sub-card']}>
                <Link to={`/components/${urlType}/${sub.componentId}`} className={styles['sub-image-link']}>
                    {sub.componentImageUrl ? (
                        <img src={sub.componentImageUrl} alt={sub.componentName ?? ''} className={styles['sub-image']} loading="lazy" />
                    ) : (
                        <div className={styles['sub-image-placeholder']} />
                    )}
                </Link>
                <div className={styles['sub-main']}>
                    <div className={styles['sub-type']}>{typeLabel}</div>
                    <Link to={`/components/${urlType}/${sub.componentId}`} className={styles['sub-name-link']}>
                        {sub.componentName ?? 'Компонент недоступний'}
                    </Link>
                    <div className={styles['sub-meta']}>
                        <span className={styles['sub-threshold']}>
                            <BellIcon /> Поріг ±{sub.thresholdPercent}%
                        </span>
                        <span className={styles['sub-created']}>
                            Створено {new Date(sub.createdAt).toLocaleDateString('uk-UA')}
                        </span>
                    </div>
                </div>
                <div className={styles['sub-prices']}>
                    <div className={styles['sub-price-row']}>
                        <span className={styles['sub-price-label']}>Ціна при підписці</span>
                        <span className={styles['sub-price-value']}>{Math.round(baseline)} грн</span>
                    </div>
                    <div className={styles['sub-price-row']}>
                        <span className={styles['sub-price-label']}>Поточна</span>
                        <span className={styles['sub-price-value']}>
                            {current != null ? `${Math.round(current)} грн` : '—'}
                        </span>
                    </div>
                    {deltaPercent != null && (
                        <div className={`${styles['sub-delta']} ${deltaClass}`}>
                            {deltaPercent > 0 ? '▲' : deltaPercent < 0 ? '▼' : '•'} {Math.abs(deltaPercent).toFixed(1)}%
                        </div>
                    )}
                </div>
                <div className={styles['sub-actions']}>
                    <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleUnsubscribe(sub.id)}
                        disabled={unsubscribingId === sub.id}
                    >
                        {unsubscribingId === sub.id ? '...' : 'Відписатися'}
                    </Button>
                </div>
            </div>
        );
    };

    // --- Early returns ---

    if (loading) return <div className={styles['loading']}>Loading your builds...</div>;

    const renderBuildsTab = () => {
        if (error) return <div className={styles['error']}>{error}</div>;

        if (builds.length === 0) {
            return (
                <div className={styles['no-builds']}>
                    <h2>You don't have any saved builds yet</h2>
                    <p>Go to the <Link to="/">PC Builder</Link> to create your first build!</p>
                </div>
            );
        }

        return (
            <div className={styles['builds-container']}>
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
                                    <Button
                                        variant={selectedBuild.isPublished ? 'outline-primary' : 'primary'}
                                        size='sm'
                                        onClick={handleTogglePublish}
                                        disabled={publishLoading || (!!postBanUntil && !selectedBuild.isPublished)}
                                        title={postBanUntil && !selectedBuild.isPublished ? `Заблоковано до ${new Date(postBanUntil).toLocaleString('uk-UA')}` : undefined}
                                    >
                                        {publishLoading ? '...' : selectedBuild.isPublished ? 'Зняти з публікації' : 'Опублікувати'}
                                    </Button>
                                    <Button variant='outline-secondary' size='sm' onClick={handleEditBuild}>
                                        <PencilIcon />
                                    </Button>
                                    <Button variant='danger' size='sm' onClick={() => openDeleteModal(selectedBuildId)}>
                                        <TrashIcon />
                                    </Button>
                                </div>
                            </div>

                            {postBanUntil && (
                                <div className={styles['ban-inline']} role="status">
                                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                                        <rect x="3" y="11" width="18" height="11" rx="2" />
                                        <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                                    </svg>
                                    <span>Публікація заблокована до <strong>{formatBanUntil(postBanUntil)}</strong>{formatBanRemaining(postBanUntil) && <> · {formatBanRemaining(postBanUntil)}</>}</span>
                                </div>
                            )}

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
        );
    };

    const renderSubscriptionsTab = () => {
        if (subscriptionsLoading) {
            return <div className={styles['loading']}>Завантаження підписок...</div>;
        }

        return (
            <div className={styles['subscriptions-panel']}>
                <div className={styles['subscriptions-header']}>
                    <h2><BellIcon /> Мої Підписки на Ціни</h2>
                    <p className={styles['subscriptions-subtitle']}>
                        Ви отримаєте сповіщення, коли середня ціна компонента зміниться більше, ніж на заданий поріг.
                    </p>
                </div>

                {subscriptionsError && (
                    <div className={styles['error-message']}>{subscriptionsError}</div>
                )}

                {subscriptions.length === 0 ? (
                    <div className={styles['no-subscriptions']}>
                        <h3>У вас ще немає підписок</h3>
                        <p>
                            Перейдіть на сторінку будь-якого{' '}
                            <Link to="/components/cpu" className={styles['login-link']}>компонента</Link>
                            {' '}і натисніть "Сповіщати про зміну ціни", щоб підписатися.
                        </p>
                    </div>
                ) : (
                    <div className={styles['subscriptions-list']}>
                        {subscriptions.map(renderSubscriptionCard)}
                    </div>
                )}
            </div>
        );
    };

    return (
        <div className={styles['user-builds-page']}>
            <DeleteModal
                isOpen={deleteModal.isOpen}
                buildName={deleteModal.buildName}
                isDeleting={deleteStatus.loading}
                onCancel={() => setDeleteModal({ isOpen: false, buildId: null, buildName: '' })}
                onConfirm={handleDeleteBuild}
            />

            <div className={styles['tabs']}>
                <button
                    type="button"
                    className={`${styles['tab']} ${activeTab === 'builds' ? styles['tab-active'] : ''}`}
                    onClick={() => setActiveTab('builds')}
                >
                    Мої Збірки <span className={styles['tab-count']}>{builds.length}</span>
                </button>
                <button
                    type="button"
                    className={`${styles['tab']} ${activeTab === 'subscriptions' ? styles['tab-active'] : ''}`}
                    onClick={() => setActiveTab('subscriptions')}
                >
                    <BellIcon /> Підписки на Ціни <span className={styles['tab-count']}>{subscriptions.length}</span>
                </button>
            </div>

            {activeTab === 'builds' ? renderBuildsTab() : renderSubscriptionsTab()}
        </div>
    );
}

export default UserBuildsPage;
