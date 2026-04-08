import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import styles from './BuildDetailPage.module.css';
import useAuth from '../../hooks/useAuth';
import { buildService } from '../../api/build.service';
import CommentSection from '../../components/CommentSection/CommentSection';
import { Button } from '../../components/Button/Button';
import type { IPcBuildRequest, IComponentPreview, IMultiComponentPreview } from '../../types/build.types';

// --- Component configs (reused from UserBuildsPage) ---

const SINGLE_COMPONENTS: { key: keyof Pick<IPcBuildRequest, 'cpu' | 'gpu' | 'motherboard' | 'powerSupply' | 'cpuCooler' | 'pcCase'>; label: string; urlType: string }[] = [
    { key: 'cpu', label: 'Процесор', urlType: 'cpu' },
    { key: 'gpu', label: 'Відеокарта', urlType: 'gpu' },
    { key: 'motherboard', label: 'Материнська плата', urlType: 'motherboard' },
    { key: 'powerSupply', label: 'Блок живлення', urlType: 'powerSupply' },
    { key: 'cpuCooler', label: 'Процесорний кулер', urlType: 'cpuCooler' },
    { key: 'pcCase', label: 'Корпус', urlType: 'pcCase' },
];

const MULTI_COMPONENTS: { key: keyof Pick<IPcBuildRequest, 'rams' | 'ssds' | 'hdds' | 'fans'>; label: string; urlType: string }[] = [
    { key: 'rams', label: "Оперативна пам'ять", urlType: 'ram' },
    { key: 'ssds', label: 'SSD Диск', urlType: 'ssd' },
    { key: 'hdds', label: 'HDD Диск', urlType: 'hdd' },
    { key: 'fans', label: 'Вентилятор', urlType: 'fan' },
];

// --- SVG icons ---

const ExternalLinkIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path fillRule="evenodd" d="M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5z"/>
        <path fillRule="evenodd" d="M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0v-5z"/>
    </svg>
);

function BuildDetailPage() {
    const { id } = useParams<{ id: string }>();
    const { auth } = useAuth();
    const navigate = useNavigate();
    const [build, setBuild] = useState<IPcBuildRequest | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [cloning, setCloning] = useState(false);

    useEffect(() => {
        if (!id) return;
        const fetchBuild = async () => {
            try {
                setLoading(true);
                const { data } = auth?.accessToken
                    ? await buildService.getBuildById(id)
                    : await buildService.getPublicBuildById(id);
                setBuild(data);
                setError(null);
            } catch {
                setError('Не вдалося завантажити збірку.');
            } finally {
                setLoading(false);
            }
        };
        fetchBuild();
    }, [id, auth?.accessToken]);

    const handleClone = async () => {
        if (!id || cloning) return;
        try {
            setCloning(true);
            await buildService.cloneBuild(id);
            navigate('/user/builds');
        } catch {
            setError('Не вдалося клонувати збірку.');
        } finally {
            setCloning(false);
        }
    };

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('uk-UA', {
            day: 'numeric', month: 'long', year: 'numeric'
        });
    };

    const renderSingleRow = (comp: IComponentPreview, label: string, urlType: string) => (
        <tr key={`${label}-${comp.id}`}>
            <td>{label}</td>
            <td className={styles['component-cell']}>
                <div className={styles['component-info']}>
                    {comp.imageUrl && <img src={comp.imageUrl} alt={comp.name} />}
                    <div className={styles['component-name']}>
                        <Link to={`/components/${urlType}/${comp.id}`} className={styles['component-link']}>
                            {comp.name}
                        </Link>
                    </div>
                </div>
            </td>
            <td>
                <div className={styles['store-info']}>{comp.storeName || 'Середня ціна'}</div>
            </td>
            <td><span className={styles['price-info']}>{comp.price}</span></td>
            <td className={styles['offer-link-cell']}>
                {comp.productOfferUrl ? (
                    <a href={comp.productOfferUrl} target="_blank" rel="noopener noreferrer" className={styles['offer-link-button']}>
                        <ExternalLinkIcon /> Купити
                    </a>
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
                <div className={styles['store-info']}>{comp.storeName || 'Середня ціна'}</div>
            </td>
            <td><span className={styles['price-info']}>{comp.totalPrice}</span></td>
            <td className={styles['offer-link-cell']}>
                {comp.productOfferUrl ? (
                    <a href={comp.productOfferUrl} target="_blank" rel="noopener noreferrer" className={styles['offer-link-button']}>
                        <ExternalLinkIcon /> Купити
                    </a>
                ) : (
                    <span className={styles['no-offer']}>-</span>
                )}
            </td>
        </tr>
    );

    if (loading) return <div className={styles['loading']}>Завантаження...</div>;
    if (error || !build) return <div className={styles['error']}>{error || 'Збірку не знайдено.'}</div>;

    return (
        <div className={styles['detail-page']}>
            <div className={styles['build-header']}>
                <div className={styles['header-left']}>
                    <h1>{build.name}</h1>
                    <div className={styles['header-meta']}>
                        <div className={styles['author-info']}>
                            {build.avatarUrl ? (
                                <img src={build.avatarUrl} alt={build.username} className={styles['author-avatar']} />
                            ) : (
                                <div className={styles['author-avatar-placeholder']}>
                                    {(build.username || '?').charAt(0).toUpperCase()}
                                </div>
                            )}
                            <span className={styles['author-name']}>{build.username}</span>
                        </div>
                        {build.publishedAt && (
                            <span className={styles['published-date']}>
                                Опубліковано {formatDate(build.publishedAt)}
                            </span>
                        )}
                    </div>
                </div>
                <div className={styles['header-right']}>
                    <div className={styles['header-price']}>
                        {build.price} грн
                    </div>
                    {auth?.accessToken && (
                        <Button
                            variant='outline-secondary'
                            size='sm'
                            onClick={handleClone}
                            disabled={cloning}
                        >
                            {cloning ? 'Клонування...' : 'Клонувати збірку'}
                        </Button>
                    )}
                </div>
            </div>

            {build.description && (
                <div className={styles['description']}>
                    <p>{build.description}</p>
                </div>
            )}

            <div className={styles['components-section']}>
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
                        const comp = build[key];
                        return comp ? renderSingleRow(comp, label, urlType) : null;
                    })}
                    {MULTI_COMPONENTS.map(({ key, label, urlType }) =>
                        build[key].map((item, idx) =>
                            renderMultiRow(item, label, urlType, idx)
                        )
                    )}
                    </tbody>
                </table>
            </div>

            <div className={styles['total-price']}>
                Остаточна ціна: <span>{build.price} грн</span>
            </div>

            {id && (
                <CommentSection buildId={id} currentUserId={auth?.userId} />
            )}
        </div>
    );
}

export default BuildDetailPage;