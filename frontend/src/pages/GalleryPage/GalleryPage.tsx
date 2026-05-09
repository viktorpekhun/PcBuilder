import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './GalleryPage.module.css';
import { galleryService } from '../../api/gallery.service';
import { Pagination } from '../../components/Pagination/Pagination';
import type { IPcBuildGallery } from '../../types/gallery.types';

interface PaginationMeta {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
}

const PAGE_SIZE = 12;

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

    const fetchBuilds = useCallback(async () => {
        try {
            setLoading(true);
            const response = await galleryService.getPublicBuilds({
                pageNumber: currentPage,
                pageSize: PAGE_SIZE,
                orderBy: sortField,
                ascending: sortAscending,
                searchQuery: searchQuery || undefined,
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

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        setCurrentPage(1);
    };

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

    const truncate = (text: string | undefined, maxLen: number) => {
        if (!text) return '';
        return text.length > maxLen ? text.slice(0, maxLen) + '...' : text;
    };

    const renderStars = (rating: number) => {
        const full = Math.floor(rating);
        const stars = [];
        for (let i = 0; i < 5; i++) {
            stars.push(
                <span key={i} className={i < full ? styles['star-filled'] : styles['star-empty']}>
                    &#9733;
                </span>
            );
        }
        return <div className={styles['stars']}>{stars}</div>;
    };

    return (
        <div className={styles['gallery-page']}>
            <h1>Галерея Збірок</h1>

            <div className={styles['controls']}>
                <form className={styles['search-form']} onSubmit={handleSearch}>
                    <input
                        type="text"
                        className={styles['search-input']}
                        placeholder="Пошук збірок..."
                        value={searchQuery}
                        onChange={e => { setSearchQuery(e.target.value); setCurrentPage(1); }}
                    />
                </form>

                <div className={styles['sort-controls']}>
                    <span className={styles['sort-label']}>Сортувати:</span>
                    {[
                        { field: 'publishedAt', label: 'Нові' },
                        { field: 'price', label: 'Ціна' },
                        { field: 'averageRating', label: 'Рейтинг' },
                        { field: 'name', label: 'Назва' },
                    ].map(({ field, label }) => (
                        <button
                            key={field}
                            className={`${styles['sort-btn']} ${sortField === field ? styles['sort-active'] : ''}`}
                            onClick={() => handleSortChange(field)}
                        >
                            {label}
                            {sortField === field && (
                                <span className={styles['sort-arrow']}>{sortAscending ? ' ▲' : ' ▼'}</span>
                            )}
                        </button>
                    ))}
                </div>
            </div>

            {error && <div className={styles['error']}>{error}</div>}

            {loading ? (
                <div className={styles['loading']}>Завантаження...</div>
            ) : builds.length === 0 ? (
                <div className={styles['no-results']}>Збірки не знайдено.</div>
            ) : (
                <>
                    <div className={styles['builds-grid']}>
                        {builds.map(build => (
                            <div
                                key={build.id}
                                className={styles['build-card']}
                                onClick={() => navigate(`/builds/${build.id}`)}
                            >
                                <div className={styles['card-header']}>
                                    <h3 className={styles['card-title']}>{build.name}</h3>
                                    <span className={styles['card-price']}>{build.price} грн</span>
                                </div>

                                {build.description && (
                                    <p className={styles['card-description']}>
                                        {truncate(build.description, 100)}
                                    </p>
                                )}

                                <div className={styles['card-stats']}>
                                    {renderStars(build.averageRating)}
                                    <span className={styles['stat-item']}>
                                        {build.componentCount} компонентів
                                    </span>
                                    <span className={styles['stat-item']}>
                                        {build.commentCount} коментарів
                                    </span>
                                </div>

                                <div className={styles['card-footer']}>
                                    <div className={styles['card-author']}>
                                        {build.avatarUrl ? (
                                            <img src={build.avatarUrl} alt={build.username} className={styles['author-avatar']} loading="lazy" />
                                        ) : (
                                            <div className={styles['author-avatar-placeholder']}>
                                                {build.username.charAt(0).toUpperCase()}
                                            </div>
                                        )}
                                        <span className={styles['author-name']}>{build.username}</span>
                                    </div>
                                    <span className={styles['card-date']}>{formatDate(build.publishedAt)}</span>
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