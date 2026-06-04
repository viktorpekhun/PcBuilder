import { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import styles from './BuildDetailPage.module.css';
import useAuth from '../../hooks/useAuth';
import { buildService } from '../../api/build.service';
import { commentService } from '../../api/comment.service';
import { profileService } from '../../api/profile.service';
import ReportModal from '../../components/ReportModal/ReportModal';
import { BuildCover } from '../../components/BuildCover/BuildCover';
import { Pagination } from '../../components/Pagination/Pagination';
import type {
    IPcBuildRequest,
    IComponentPreview,
    IMultiComponentPreview,
    IComponentsCompatibility,
    IBuildCompatibilityReport,
} from '../../types/build.types';
import type { IComment } from '../../types/comment.types';

// --- Part-row model: a flattened, slot-tagged view of build components ---

interface PartRow {
    slot: string;
    labelKey: string;
    name: string;
    qty: number;
    componentId: string;
    urlType: string;
    storeName?: string;
    domain?: string;
    imageUrl?: string;
    subtotal?: number;
    offerUrl?: string;
}

const SINGLE_COMPONENTS: {
    key: keyof Pick<IPcBuildRequest, 'cpu' | 'gpu' | 'motherboard' | 'powerSupply' | 'cpuCooler' | 'pcCase'>;
    slot: string; labelKey: string; urlType: string;
}[] = [
    { key: 'cpu', slot: 'CPU', labelKey: 'cpu', urlType: 'cpu' },
    { key: 'gpu', slot: 'GPU', labelKey: 'gpu', urlType: 'gpu' },
    { key: 'motherboard', slot: 'M/B', labelKey: 'motherboard', urlType: 'motherboard' },
    { key: 'cpuCooler', slot: 'CLR', labelKey: 'cpuCooler', urlType: 'cpuCooler' },
    { key: 'powerSupply', slot: 'PSU', labelKey: 'powerSupply', urlType: 'powerSupply' },
    { key: 'pcCase', slot: 'CSE', labelKey: 'pcCase', urlType: 'pcCase' },
];

const MULTI_COMPONENTS: {
    key: keyof Pick<IPcBuildRequest, 'rams' | 'ssds' | 'hdds' | 'fans'>;
    slot: string; labelKey: string; urlType: string;
}[] = [
    { key: 'rams', slot: 'RAM', labelKey: 'rams', urlType: 'ram' },
    { key: 'ssds', slot: 'SSD', labelKey: 'ssds', urlType: 'ssd' },
    { key: 'hdds', slot: 'HDD', labelKey: 'hdds', urlType: 'hdd' },
    { key: 'fans', slot: 'FAN', labelKey: 'fans', urlType: 'fan' },
];

const COMMENTS_PAGE_SIZE = 10;

/** "12 345" — whole-number, space-grouped, matching the mono tabular number style. */
const fmt = (n?: number) =>
    n == null ? '—' : Math.round(n).toLocaleString('uk-UA', { maximumFractionDigits: 0 }).replace(/[\s,]/g, ' ');

function hash(seed: string): number {
    return Math.abs(seed.split('').reduce((a, c) => (a * 31 + c.charCodeAt(0)) | 0, 0));
}

// --- Per-component thumbnail: real image, or a hash-seeded dark mat fallback ---

function ComponentThumb({ name, imageUrl }: { name: string; imageUrl?: string }) {
    if (imageUrl) {
        return <img src={imageUrl} alt={name} className={styles['thumb-img']} loading="lazy" />;
    }
    const h = hash(name);
    const hue = h % 360;
    const angle = (h >> 4) % 180;
    return (
        <div
            className={styles['thumb-mat']}
            style={{
                backgroundImage: `linear-gradient(${angle}deg, hsl(${hue} 10% 24%), hsl(${(hue + 50) % 360} 12% 12%))`,
            }}
        >
            <span
                className={styles['thumb-glint']}
                style={{ background: `linear-gradient(${(angle + 90) % 360}deg, transparent 60%, rgba(255,255,255,0.06))` }}
            />
        </div>
    );
}

// --- Star rating (read-only) ---

function Stars({ value }: { value: number }) {
    const full = Math.round(value);
    return (
        <span className={styles['stars']}>
            {Array.from({ length: 5 }).map((_, i) => (
                <span key={i} className={i < full ? styles['star'] : styles['star-empty']}>★</span>
            ))}
        </span>
    );
}

function BuildDetailPage() {
    const { id } = useParams<{ id: string }>();
    const { auth } = useAuth();
    const navigate = useNavigate();
    const { t } = useTranslation();

    const [build, setBuild] = useState<IPcBuildRequest | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [cloning, setCloning] = useState(false);
    const [reportBuildOpen, setReportBuildOpen] = useState(false);
    const [compat, setCompat] = useState<IBuildCompatibilityReport | null>(null);

    // comments state
    const [comments, setComments] = useState<IComment[]>([]);
    const [commentsMeta, setCommentsMeta] = useState({ totalCount: 0, pageNumber: 1, totalPages: 0, pageSize: COMMENTS_PAGE_SIZE });
    const [commentsLoading, setCommentsLoading] = useState(true);
    const [commentsError, setCommentsError] = useState<string | null>(null);
    const [reportComment, setReportComment] = useState<string | null>(null);

    // compose state
    const [text, setText] = useState('');
    const [rating, setRating] = useState(0);
    const [hover, setHover] = useState(0);
    const [submitting, setSubmitting] = useState(false);
    const [formError, setFormError] = useState<string | null>(null);
    const [commentBanUntil, setCommentBanUntil] = useState<string | null>(null);

    const isLoggedIn = !!auth?.accessToken;
    const isOwner = !!build && auth?.userId === build.userId;

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
                setError(t('buildDetail.loadFailed'));
            } finally {
                setLoading(false);
            }
        };
        fetchBuild();
    }, [id, auth?.accessToken, t]);

    // Run the compatibility checker against the loaded build's components for the stats strip.
    useEffect(() => {
        if (!build) return;
        const payload: IComponentsCompatibility = {
            ...(build.cpu ? { cpuId: build.cpu.id } : {}),
            ...(build.gpu ? { gpuId: build.gpu.id } : {}),
            ...(build.motherboard ? { motherboardId: build.motherboard.id } : {}),
            ...(build.cpuCooler ? { cpuCoolerId: build.cpuCooler.id } : {}),
            ...(build.powerSupply ? { powerSupplyId: build.powerSupply.id } : {}),
            ...(build.pcCase ? { pcCaseId: build.pcCase.id } : {}),
            rams: build.rams.map(r => ({ componentId: r.id, quantity: r.quantity })),
            ssds: build.ssds.map(s => ({ componentId: s.id, quantity: s.quantity })),
            hdds: build.hdds.map(h => ({ componentId: h.id, quantity: h.quantity })),
            fans: build.fans.map(f => ({ componentId: f.id, quantity: f.quantity })),
        };
        let cancelled = false;
        buildService.checkCompatibility(payload)
            .then(res => { if (!cancelled) setCompat(res.data); })
            .catch(() => { if (!cancelled) setCompat(null); });
        return () => { cancelled = true; };
    }, [build]);

    useEffect(() => {
        if (!auth?.userId) return;
        profileService.getBans()
            .then(res => setCommentBanUntil(res.data.isCommentBanned ? res.data.commentBanUntil : null))
            .catch(() => {});
    }, [auth?.userId]);

    const fetchComments = useCallback(async (page: number) => {
        if (!id) return;
        try {
            setCommentsLoading(true);
            const response = await commentService.getComments(id, page, COMMENTS_PAGE_SIZE);
            setComments(response.data);
            const header = response.headers['x-pagination'];
            if (header) setCommentsMeta(JSON.parse(header));
            setCommentsError(null);
        } catch {
            setCommentsError(t('buildDetail.reviewsSection.loadFailed'));
        } finally {
            setCommentsLoading(false);
        }
    }, [id, t]);

    useEffect(() => {
        fetchComments(1);
    }, [fetchComments]);

    const handleClone = async () => {
        if (!id || cloning) return;
        try {
            setCloning(true);
            await buildService.cloneBuild(id);
            navigate('/user/builds');
        } catch {
            setError(t('buildDetail.cloneFailed'));
        } finally {
            setCloning(false);
        }
    };

    const handleSubmitComment = async () => {
        const trimmed = text.trim();
        if (!id || !trimmed || rating === 0 || submitting || !auth?.userId || commentBanUntil) return;
        try {
            setSubmitting(true);
            setFormError(null);
            await commentService.addComment(id, { text: trimmed, rating });
            setText('');
            setRating(0);
            await fetchComments(1);
        } catch (err) {
            const resp = (err as { response?: { status?: number; data?: { message?: string; code?: string } } })?.response;
            if (resp?.status === 400 && resp?.data?.code === 'Toxic') return; // surfaced via notifications
            setFormError(resp?.data?.message || t('buildDetail.submitFailed'));
        } finally {
            setSubmitting(false);
        }
    };

    const handleDeleteComment = async (commentId: string) => {
        try {
            await commentService.deleteComment(commentId);
            await fetchComments(commentsMeta.pageNumber);
        } catch {
            setCommentsError(t('buildDetail.reviewsSection.deleteFailed'));
        }
    };

    const handleCompose = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            handleSubmitComment();
        }
    };

    const formatDate = (dateStr?: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' }).toUpperCase();
    };

    const formatWhen = (dateStr: string) => {
        const diffMs = Date.now() - new Date(dateStr).getTime();
        const mins = Math.floor(diffMs / 60000);
        if (mins < 1) return t('notifications.row.justNow');
        if (mins < 60) return t('notifications.row.minutesAgo', { count: mins });
        const hours = Math.floor(mins / 60);
        if (hours < 24) return t('notifications.row.hoursAgo', { count: hours });
        const days = Math.floor(hours / 24);
        if (days < 30) return t('notifications.row.daysAgo', { count: days });
        return new Date(dateStr).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short' });
    };

    const formatRemaining = (dateStr: string) => {
        const diffMs = new Date(dateStr).getTime() - Date.now();
        if (diffMs <= 0) return null;
        const mins = Math.floor(diffMs / 60000);
        const days = Math.floor(mins / 1440);
        const hours = Math.floor((mins % 1440) / 60);
        const minutes = mins % 60;
        if (days > 0) return `${days}d ${hours}h`;
        if (hours > 0) return `${hours}h ${minutes}m`;
        return `${minutes}m`;
    };

    // Flatten the build into slot-tagged part rows in canonical assembly order.
    const parts = useMemo<PartRow[]>(() => {
        if (!build) return [];
        const domainOf = (url?: string) => {
            if (!url) return undefined;
            try { return new URL(url).hostname.replace(/^www\./, ''); } catch { return undefined; }
        };
        const rows: PartRow[] = [];
        for (const { key, slot, labelKey, urlType } of SINGLE_COMPONENTS) {
            const c = build[key] as IComponentPreview | undefined;
            if (!c) continue;
            rows.push({
                slot, labelKey, urlType, name: c.name, qty: 1, componentId: c.id,
                ...(c.storeName ? { storeName: c.storeName } : {}),
                ...(domainOf(c.productOfferUrl) ? { domain: domainOf(c.productOfferUrl)! } : {}),
                ...(c.imageUrl ? { imageUrl: c.imageUrl } : {}),
                ...(c.price != null ? { subtotal: c.price } : {}),
                ...(c.productOfferUrl ? { offerUrl: c.productOfferUrl } : {}),
            });
        }
        for (const { key, slot, labelKey, urlType } of MULTI_COMPONENTS) {
            for (const c of build[key] as IMultiComponentPreview[]) {
                rows.push({
                    slot, labelKey, urlType, name: c.name, qty: c.quantity, componentId: c.id,
                    ...(c.storeName ? { storeName: c.storeName } : {}),
                    ...(domainOf(c.productOfferUrl) ? { domain: domainOf(c.productOfferUrl)! } : {}),
                    ...(c.imageUrl ? { imageUrl: c.imageUrl } : {}),
                    ...(c.totalPrice != null ? { subtotal: c.totalPrice } : {}),
                    ...(c.productOfferUrl ? { offerUrl: c.productOfferUrl } : {}),
                });
            }
        }
        return rows;
    }, [build]);

    const slotsUsed = useMemo(() => new Set(parts.map(p => p.slot)).size, [parts]);

    // PSU load % and a human label, when both estimated draw and PSU wattage are known.
    const psuLoad = useMemo(() => {
        if (!build?.psuWattage || build.psuWattage <= 0) return null;
        const pct = Math.round((build.estimatedPower / build.psuWattage) * 100);
        const labelKey = pct > 100 ? 'psuOver' : pct < 40 ? 'psuLow' : 'psuOk';
        return { pct, labelKey, over: pct > 100 };
    }, [build]);

    // Compatibility summary derived from the rule report (only failing rules are returned).
    const compatInfo = useMemo(() => {
        if (!compat) return null;
        const issues = compat.ruleResults.flatMap(r => r.issues);
        const critical = issues.filter(i => i.severity === 'Critical').length;
        const warnings = issues.filter(i => i.severity === 'Warning').length;
        return { pass: compat.isStrictlyCompatible, critical, warnings, issues: issues.length };
    }, [compat]);

    if (loading) return <div className={styles['state']}>{t('buildDetail.loading')}</div>;
    if (error || !build) return <div className={`${styles['state']} ${styles['state-error']}`}>{error || t('buildDetail.loadFailed')}</div>;

    return (
        <div className={styles['shell']}>

            {/* ── header ──────────────────────────────────── */}
            <div className={styles['head']}>
                <div>
                    <h1>{build.name}</h1>
                    <div className={styles['meta']}>
                        <span className={styles['author']}>
                            {build.avatarUrl ? (
                                <img src={build.avatarUrl} alt={build.username} className={styles['author-av']} />
                            ) : (
                                <span className={styles['author-av-ph']}>
                                    {(build.username || '?').charAt(0).toUpperCase()}
                                </span>
                            )}
                            <strong>@{build.username}</strong>
                        </span>
                        {build.publishedAt && (
                            <>
                                <span className={styles['sep']}>·</span>
                                <span>{t('buildDetail.publishedOn')} {formatDate(build.publishedAt)}</span>
                            </>
                        )}
                        {(build.averageRating ?? 0) > 0 && (
                            <>
                                <span className={styles['sep']}>·</span>
                                <Stars value={build.averageRating!} />
                                <span className={styles['rating-val']}>{build.averageRating!.toFixed(1)}</span>
                            </>
                        )}
                    </div>
                </div>
                <div className={styles['head-right']}>
                    <div className={styles['price-wrap']}>
                        <div className={styles['price']}><span className={styles['ccy']}>₴</span>{fmt(build.price)}</div>
                    </div>
                    <div className={styles['actions']}>
                        {isLoggedIn && !isOwner && (
                            <button
                                className={`${styles['btn']} ${styles['btn-ghost']}`}
                                onClick={() => setReportBuildOpen(true)}
                            >
                                {t('buildDetail.reportBtn')}
                            </button>
                        )}
                        {isOwner ? (
                            <button
                                className={`${styles['btn']} ${styles['btn-sec']}`}
                                onClick={() => navigate('/user/builds')}
                            >
                                {t('buildDetail.editBtn')}
                            </button>
                        ) : isLoggedIn ? (
                            <button
                                className={`${styles['btn']} ${styles['btn-pri']}`}
                                onClick={handleClone}
                                disabled={cloning}
                            >
                                {cloning ? t('buildDetail.cloning') : t('buildDetail.cloneBuild')}
                            </button>
                        ) : null}
                    </div>
                </div>
            </div>

            {/* ── overview: cover left · description + stats right ── */}
            <div className={styles['overview']}>
                {/* cover */}
                <div className={styles['cover']}>
                    <BuildCover title={build.name} coverUrl={build.photoUrl} />
                </div>

                <div className={styles['overview-body']}>
                    {/* description */}
                    {build.description && (
                        <div className={styles['desc']}>
                            <p>{build.description}</p>
                        </div>
                    )}

                    {/* spec tags */}
                    {build.tags.length > 0 && (
                        <div className={styles['tags']}>
                            {build.tags.map(tag => (
                                <span key={tag} className={styles['tag']}>{tag}</span>
                            ))}
                        </div>
                    )}

                    {/* stats strip — compact label : value rows */}
                    <div className={styles['stats']}>
                        <div className={styles['stat']}>
                            <span className={styles['lbl']}>{t('buildDetail.stats.total')}</span>
                            <span className={styles['val']}>{fmt(build.price)}</span>
                        </div>
                        <div className={styles['stat']}>
                            <span className={styles['lbl']}>{t('buildDetail.stats.power')}</span>
                            <span className={styles['val']}>
                                {build.estimatedPower}
                                {build.psuWattage ? (
                                    <span className={styles['val-of']}> / {build.psuWattage} W</span>
                                ) : <span className={styles['val-of']}> W</span>}
                                {psuLoad && <span className={styles['val-note']}> · {psuLoad.pct}% {t(`buildDetail.stats.${psuLoad.labelKey}`)}</span>}
                            </span>
                        </div>
                        <div className={styles['stat']}>
                            <span className={styles['lbl']}>{t('buildDetail.stats.compatibility')}</span>
                            <span className={styles['val']}>
                                {compatInfo ? (
                                    <span className={compatInfo.pass ? styles['ok'] : styles['err']}>
                                        {compatInfo.pass ? t('buildDetail.stats.compatPass') : t('buildDetail.stats.compatFail')}
                                    </span>
                                ) : '…'}
                                {compatInfo && (
                                    <span className={styles['val-note']}>
                                        {compatInfo.pass
                                            ? (compatInfo.warnings > 0 ? ` · ${t('buildDetail.stats.warnings', { count: compatInfo.warnings })}` : '')
                                            : ` · ${t('buildDetail.stats.critical', { count: compatInfo.critical })}`}
                                    </span>
                                )}
                            </span>
                        </div>
                        <div className={styles['stat']}>
                            <span className={styles['lbl']}>{t('buildDetail.stats.components')}</span>
                            <span className={styles['val']}>
                                {parts.length}
                                <span className={styles['val-note']}> · {t('buildDetail.stats.slots', { count: slotsUsed })}</span>
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {/* ── parts table ─────────────────────────────── */}
            <div className={styles['parts']}>
                <div className={styles['ph']}>
                    <span className={styles['eyebrow']}>{t('buildDetail.components')} · {parts.length}</span>
                </div>
                <div className={styles['table-head']}>
                    <span>{t('buildDetail.table.slot')}</span>
                    <span />
                    <span>{t('buildDetail.table.component')}</span>
                    <span>{t('buildDetail.table.store')}</span>
                    <span className={styles['r']}>{t('buildDetail.table.subtotal')}</span>
                    <span className={styles['r']}>{t('buildDetail.table.link')}</span>
                </div>
                {parts.map((p, i) => (
                    <div key={`${p.slot}-${p.componentId}-${i}`} className={styles['row']}>
                        <div className={styles['slot-gly']}>[{p.slot}]</div>
                        <div className={styles['thumb']}>
                            <ComponentThumb name={p.name} {...(p.imageUrl ? { imageUrl: p.imageUrl } : {})} />
                        </div>
                        <div className={styles['name-col']}>
                            <Link to={`/components/${p.urlType}/${p.componentId}`} className={styles['name']} title={p.name}>
                                {p.name}
                            </Link>
                            {p.qty > 1 && <span className={styles['qty']}>× {p.qty}</span>}
                            <div className={styles['part-sub']}>
                                {t(`buildDetail.slotLabels.${p.labelKey}`).toUpperCase()}
                                {p.qty > 1 ? ` · ${t('buildDetail.table.units', { count: p.qty })}` : ''}
                            </div>
                        </div>
                        <div className={styles['store']}>
                            <div>{p.storeName || t('buildDetail.table.avgPrice')}</div>
                            {p.domain && <div className={styles['dom']}>{p.domain}</div>}
                        </div>
                        <div className={styles['part-price']}>
                            {p.subtotal != null ? <><span className={styles['ccy']}>₴</span>{fmt(p.subtotal)}</> : '—'}
                        </div>
                        <div className={styles['buy-col']}>
                            {p.offerUrl ? (
                                <a className={styles['buy']} href={p.offerUrl} target="_blank" rel="noopener noreferrer">{t('buildDetail.table.buy')}</a>
                            ) : (
                                <span className={styles['no-offer']}>—</span>
                            )}
                        </div>
                    </div>
                ))}
                <div className={styles['total']}>
                    <span className={styles['total-lbl']}>{t('buildDetail.table.finalPrice')}</span>
                    <span className={styles['total-v']}><span className={styles['ccy']}>₴</span>{fmt(build.price)}</span>
                </div>
            </div>

            {/* ── comments ────────────────────────────────── */}
            <div className={styles['comments']}>
                <div className={styles['ch']}>
                    <span className={styles['eyebrow']}>{t('buildDetail.reviewsSection.heading', { count: commentsMeta.totalCount })}</span>
                </div>

                {commentsError && (
                    <div className={styles['list-error']} role="alert">
                        <span>{commentsError}</span>
                        <button className={styles['list-error-x']} onClick={() => setCommentsError(null)} aria-label="Close">{t('buildDetail.reviewsSection.closeError')}</button>
                    </div>
                )}

                {commentsLoading && comments.length === 0 ? (
                    <div className={styles['comments-state']}>{t('buildDetail.reviewsSection.loading')}</div>
                ) : comments.length === 0 ? (
                    <div className={styles['comments-state']}>{t('buildDetail.reviewsSection.empty')}</div>
                ) : (
                    <>
                        {comments.map(c => (
                            <div key={c.id} className={styles['comment']}>
                                {c.avatarUrl ? (
                                    <img src={c.avatarUrl} alt={c.username} className={styles['c-av']} />
                                ) : (
                                    <span className={styles['c-av-ph']}>{(c.username || '?').charAt(0).toUpperCase()}</span>
                                )}
                                <div>
                                    <div className={styles['c-top']}>
                                        <span className={styles['c-author']}>@{c.username}</span>
                                        <span className={styles['c-when']}>· {formatWhen(c.createdAt)}</span>
                                        <span className={styles['c-stars']}>
                                            {Array.from({ length: 5 }).map((_, i) => (
                                                <span key={i} className={i < c.rating ? '' : styles['c-star-empty']}>★</span>
                                            ))}
                                        </span>
                                        <span className={styles['grow']} />
                                        {auth?.userId === c.userId ? (
                                            <span className={styles['c-act']} onClick={() => handleDeleteComment(c.id)}>{t('buildDetail.reviewsSection.deleteComment')}</span>
                                        ) : isLoggedIn ? (
                                            <span className={styles['c-act']} onClick={() => setReportComment(c.id)}>{t('buildDetail.reviewsSection.reportComment')}</span>
                                        ) : null}
                                    </div>
                                    <p className={styles['c-text']}>{c.text}</p>
                                </div>
                            </div>
                        ))}

                        {commentsMeta.totalPages > 1 && (
                            <Pagination
                                currentPage={commentsMeta.pageNumber}
                                totalPages={commentsMeta.totalPages}
                                totalResults={commentsMeta.totalCount}
                                pageSize={commentsMeta.pageSize}
                                onPageChange={page => fetchComments(page)}
                            />
                        )}
                    </>
                )}

                {/* compose / sign-in prompt */}
                {!isLoggedIn ? (
                    <div className={styles['signin-prompt']}>
                        <div className={styles['eyebrow']}>{t('buildDetail.signInPrompt.eyebrow')}</div>
                        <div className={styles['signin-links']}>
                            <Link to="/login" className={styles['lnk']}>{t('buildDetail.signInPrompt.signIn')}</Link>
                            <span className={styles['sep']}>·</span>
                            <Link to="/register" className={styles['lnk']}>{t('buildDetail.signInPrompt.register')}</Link>
                        </div>
                    </div>
                ) : commentBanUntil ? (
                    <div className={styles['ban']} role="status">
                        {t('buildDetail.ban.message')} <strong>{new Date(commentBanUntil).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</strong>
                        {formatRemaining(commentBanUntil) && <> · {formatRemaining(commentBanUntil)}</>}
                    </div>
                ) : (
                    <div className={styles['compose']}>
                        <div className={`${styles['field']} ${formError ? styles['field-error'] : ''}`}>
                            <textarea
                                placeholder={t('buildDetail.reviewsSection.placeholder')}
                                value={text}
                                maxLength={500}
                                onChange={e => { setText(e.target.value); if (formError) setFormError(null); }}
                                onKeyDown={handleCompose}
                            />
                            <div className={styles['ratebar']}>
                                <span className={styles['rate-lbl']}>{t('buildDetail.reviewsSection.ratingLabel')}</span>
                                {[1, 2, 3, 4, 5].map(n => (
                                    <span
                                        key={n}
                                        className={`${styles['rate-star']} ${(hover || rating) >= n ? styles['on'] : ''}`}
                                        onMouseEnter={() => setHover(n)}
                                        onMouseLeave={() => setHover(0)}
                                        onClick={() => setRating(n)}
                                    >★</span>
                                ))}
                                <span className={styles['grow']} />
                                <span className={styles['hint']}>{text.length}/500</span>
                                <button
                                    className={`${styles['btn']} ${styles['btn-pri']}`}
                                    disabled={!text.trim() || rating === 0 || submitting}
                                    onClick={handleSubmitComment}
                                >
                                    {submitting ? t('buildDetail.reviewsSection.sending') : t('buildDetail.reviewsSection.send')}
                                </button>
                            </div>
                        </div>
                        {formError && <div className={styles['field-msg']} role="alert">{formError}</div>}
                    </div>
                )}
            </div>

            {/* ── modals ──────────────────────────────────── */}
            {id && (
                <ReportModal
                    isOpen={reportBuildOpen}
                    targetType="build"
                    targetId={id}
                    onClose={() => setReportBuildOpen(false)}
                />
            )}
            <ReportModal
                isOpen={reportComment !== null}
                targetType="review"
                targetId={reportComment ?? ''}
                onClose={() => setReportComment(null)}
            />
        </div>
    );
}

export default BuildDetailPage;
