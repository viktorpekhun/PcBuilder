import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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

const fmt = (n: number) =>
    Math.round(n).toLocaleString('uk-UA', { maximumFractionDigits: 0 }).replace(/[\s,]/g, ' ');

function GalleryPage() {
    const { t, i18n } = useTranslation();
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

    const SORTS = [
        { field: 'publishedAt', label: t('gallery.sorts.publishedAt') },
        { field: 'price', label: t('gallery.sorts.price') },
        { field: 'averageRating', label: t('gallery.sorts.averageRating') },
        { field: 'name', label: t('gallery.sorts.name') },
    ];

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
            setError(t('gallery.loadFailed'));
        } finally {
            setLoading(false);
        }
    }, [currentPage, sortField, sortAscending, searchQuery, t]);

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
        const locale = i18n.language === 'uk' ? 'uk-UA' : 'en-GB';
        return new Date(dateStr).toLocaleDateString(locale, {
            day: 'numeric', month: 'short', year: 'numeric'
        });
    };

    return (
        <div className={styles['page-shell']}>
            <div className={styles['head']}>
                <div>
                    <span className={styles['eyebrow']}>
                        {t('gallery.eyebrow')}
                    </span>
                    <h1>{t('gallery.heading')}</h1>
                    <div className={styles['meta']}>
                        <span>{t('gallery.meta')}</span>
                    </div>
                </div>
                {isLoggedIn && (
                    <div className={styles['actions']}>
                        <button
                            className={`${styles['btn']} ${styles['btn-ghost']}`}
                            onClick={() => navigate('/user/builds')}
                        >
                            {t('gallery.myBuilds')}
                        </button>
                        <button
                            className={`${styles['btn']} ${styles['btn-pri']}`}
                            onClick={() => navigate('/')}
                        >
                            {t('gallery.publishBuild')}
                        </button>
                    </div>
                )}
            </div>

            <div className={styles['subbar']}>
                <div className={styles['input']}>
                    <span className={styles['ic']}>⌕</span>
                    <input
                        placeholder={t('gallery.searchPlaceholder')}
                        value={searchQuery}
                        onChange={e => { setSearchQuery(e.target.value); setCurrentPage(1); }}
                    />
                </div>
                <span className={styles['stat']}>
                    <strong>{fmt(pagination.totalCount)}</strong> {t('gallery.buildsFound', { count: pagination.totalCount })}
                </span>
                <span className={styles['grow']} />
                <span className={styles['toolbarLabel']}>{t('gallery.sortLabel')}</span>
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
                <div className={styles['loading']}>{t('gallery.loading')}</div>
            ) : builds.length === 0 ? (
                <div className={styles['empty']}>
                    <div className={styles['eyebrow']}>{t('gallery.empty.eyebrow')}</div>
                    <div className={styles['empty-title']}>{t('gallery.empty.title')}</div>
                    <div className={styles['empty-hint']}>
                        {t('gallery.empty.hint')}
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
                                    <BuildCover title={build.name} coverUrl={build.photoUrl} />
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
                                    <span className={styles['parts']}>{build.componentCount} {t('gallery.partCount', { count: build.componentCount })}</span>
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
