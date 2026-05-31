import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './GalleryPage.module.css';
import { galleryService } from '../../api/gallery.service';
import { Pagination } from '../../components/Pagination/Pagination';
import { BuildCover } from '../../components/BuildCover/BuildCover';
import useAuth from '../../hooks/useAuth';
import type { IPcBuildGallery } from '../../types/gallery.types';

interface PaginationMeta {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
}

const PAGE_SIZE = 12;

const SORTS = [
    { field: 'publishedAt', label: 'Нові' },
    { field: 'price', label: 'Ціна' },
    { field: 'averageRating', label: 'Рейтинг' },
    { field: 'name', label: 'Назва' },
];

/** "12 345" — whole-number, space-grouped, matching the mono tabular number style.
   Rounds to whole UAH so decimal prices don't break grouping (uk-UA uses a comma decimal). */
const fmt = (n: number) =>
    Math.round(n).toLocaleString('uk-UA', { maximumFractionDigits: 0 }).replace(/[\s,]/g, ' ');

function GalleryPage() {
    const [builds, setBuilds] = useState<IPcBuildGallery[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta>({
        totalCount: 0, pageSize: PAGE_SIZE, pageNumber: 1, totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [searchQuery, setSearchQuery] = useState('');
    const [sortField, setSortField] = useState('publishedAt');
    const [sortAscending, setSortAscending] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);

    const navigate = useNavigate();
    const { auth } = useAuth();
    const isLoggedIn = !!auth?.accessToken;

    const fetchBuilds = useCallback(async () => {
        try {
            setLoading(true);
            const response = await galleryService.getPublicBuilds({
                pageNumber: currentPage,
                pageSize: PAGE_SIZE,
                orderBy: sortField,
                ascending: sortAscending,
                ...(searchQuery ? { searchQuery } : {}),
            });
            setBuilds(response.data);

            const paginationHeader = response.headers['x-pagination'];
            if (paginationHeader) {
                setPagination(JSON.parse(paginationHeader));
            }
            setError(null);
        } catch {
            setError('Не вдалося завантажити збірки.');
        } finally {
            setLoading(false);
        }
    }, [currentPage, sortField, sortAscending, searchQuery]);

    useEffect(() => {
        fetchBuilds();
    }, [fetchBuilds]);

    const handleSortChange = (field: string) => {
        if (field === sortField) {
            setSortAscending(prev => !prev);
        } else {
            setSortField(field);
            setSortAscending(field === 'name');
        }
        setCurrentPage(1);
    };

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('uk-UA', {
            day: 'numeric', month: 'short', year: 'numeric'
        });
    };

    return (
        <div className={styles['page-shell']}>
            <div className={styles['head']}>
                <div>
                    <span className={styles['eyebrow']}>
                        ГАЛЕРЕЯ · ПУБЛІЧНІ ЗБІРКИ
                    </span>
                    <h1>Галерея збірок</h1>
                    <div className={styles['meta']}>
                        <span>СПІЛЬНОТА · СОРТУВАННЯ ТА ПОШУК</span>
                    </div>
                </div>
                {isLoggedIn && (
                    <div className={styles['actions']}>
                        <button
                            className={`${styles['btn']} ${styles['btn-ghost']}`}
                            onClick={() => navigate('/user/builds')}
                        >
                            ↗ Мої збірки
                        </button>
                        <button
                            className={`${styles['btn']} ${styles['btn-pri']}`}
                            onClick={() => navigate('/')}
                        >
                            ＋ Опублікувати збірку
                        </button>
                    </div>
                )}
            </div>

            <div className={styles['subbar']}>
                <div className={styles['input']}>
                    <span className={styles['ic']}>⌕</span>
                    <input
                        placeholder="Пошук збірок…"
                        value={searchQuery}
                        onChange={e => { setSearchQuery(e.target.value); setCurrentPage(1); }}
                    />
                </div>
                <span className={styles['stat']}>
                    <strong>{fmt(pagination.totalCount)}</strong> збірок знайдено
                </span>
                <span className={styles['grow']} />
                <span className={styles['toolbarLabel']}>SORT</span>
                <div className={styles['sortGroup']}>
                    {SORTS.map(s => {
                        const isActive = sortField === s.field;
                        return (
                            <span
                                key={s.field}
                                className={`${styles['sortSeg']} ${isActive ? styles['sortSegOn'] : ''}`}
                                onClick={() => handleSortChange(s.field)}
                            >
                                {s.label}
                                {isActive && (
                                    <span className={styles['sortArrow']}>
                                        {sortAscending ? '▲' : '▼'}
                                    </span>
                                )}
                            </span>
                        );
                    })}
                </div>
            </div>

            {error && <div className={styles['error']}>{error}</div>}

            {loading ? (
                <div className={styles['loading']}>Завантаження…</div>
            ) : builds.length === 0 ? (
                <div className={styles['empty']}>
                    <div className={styles['eyebrow']}>НЕМАЄ ЗБІГІВ</div>
                    <div className={styles['empty-title']}>Жодна збірка не відповідає фільтрам.</div>
                    <div className={styles['empty-hint']}>
                        Спробуйте змінити запит пошуку, сокет або сортування.
                    </div>
                </div>
            ) : (
                <>
                    <div className={styles['grid']}>
                        {builds.map(build => (
                            <div
                                key={build.id}
                                className={styles['card']}
                                onClick={() => navigate(`/builds/${build.id}`)}
                            >
                                <div className={styles['cover-wrap']}>
                                    <BuildCover title={build.name} />
                                    <span className={styles['price-badge']}>₴ {fmt(build.price)}</span>
                                </div>

                                <div className={styles['body']}>
                                    <div className={styles['title']}>{build.name}</div>
                                    {build.description && (
                                        <div className={styles['description']}>{build.description}</div>
                                    )}
                                    <div className={styles['author-row']}>
                                        {build.avatarUrl ? (
                                            <img
                                                src={build.avatarUrl}
                                                alt={build.username}
                                                className={styles['author-avatar']}
                                                loading="lazy"
                                            />
                                        ) : (
                                            <span className={styles['author-avatar-ph']}>
                                                {build.username.charAt(0).toUpperCase()}
                                            </span>
                                        )}
                                        <span>@{build.username}</span>
                                        {build.publishedAt && (
                                            <>
                                                <span className={styles['sep']}>·</span>
                                                <span>{formatDate(build.publishedAt)}</span>
                                            </>
                                        )}
                                    </div>
                                    {build.tags && build.tags.length > 0 && (
                                        <div className={styles['tags']}>
                                            {build.tags.slice(0, 3).map(tag => (
                                                <span key={tag} className={styles['tag-mini']}>{tag}</span>
                                            ))}
                                        </div>
                                    )}
                                </div>

                                <div className={styles['foot']}>
                                    <span className={styles['rating']}>
                                        <span className={styles['star']}>★</span>
                                        <span>{build.averageRating.toFixed(1)}</span>
                                        <span className={styles['rating-count']}>({build.ratingCount})</span>
                                    </span>
                                    <span className={styles['parts']}>{build.componentCount} КОМП.</span>
                                    <span className={styles['comments']}>
                                        <span>✎ {build.commentCount}</span>
                                    </span>
                                </div>
                            </div>
                        ))}
                    </div>

                    <Pagination
                        currentPage={pagination.pageNumber}
                        totalPages={pagination.totalPages}
                        totalResults={pagination.totalCount}
                        pageSize={pagination.pageSize}
                        onPageChange={page => setCurrentPage(page)}
                    />
                </>
            )}
        </div>
    );
}

export default GalleryPage;
