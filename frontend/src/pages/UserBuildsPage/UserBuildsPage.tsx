import { useState, useEffect, useMemo, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import styles from './UserBuildsPage.module.css';
import useAuth from '../../hooks/useAuth';
import { BuildCover } from '../../components/BuildCover/BuildCover';
import { buildService } from '../../api/build.service';
import { profileService } from '../../api/profile.service';
import type { IPcBuildList, IPcBuildRequest, IComponentPreview, IMultiComponentPreview } from '../../types/build.types';
import type { SelectedComponents, SingleKey, MultiKey } from '../PcBuildPage/types';

// --- Component type configs ---

interface SingleConfig { key: keyof Pick<IPcBuildRequest, SingleKey>; slot: string; label: string; urlType: string }
interface MultiConfig  { key: keyof Pick<IPcBuildRequest, MultiKey>;  slot: string; label: string; urlType: string }

const SINGLE_COMPONENTS: SingleConfig[] = [
    { key: 'cpu',          slot: 'CPU', label: 'buildDetail.slotLabels.cpu',         urlType: 'cpu'          },
    { key: 'gpu',          slot: 'GPU', label: 'buildDetail.slotLabels.gpu',         urlType: 'gpu'          },
    { key: 'motherboard',  slot: 'M/B', label: 'buildDetail.slotLabels.motherboard', urlType: 'motherboard'  },
    { key: 'powerSupply',  slot: 'PSU', label: 'buildDetail.slotLabels.powerSupply', urlType: 'powerSupply'  },
    { key: 'cpuCooler',    slot: 'CLR', label: 'buildDetail.slotLabels.cpuCooler',   urlType: 'cpuCooler'    },
    { key: 'pcCase',       slot: 'CSE', label: 'buildDetail.slotLabels.pcCase',      urlType: 'pcCase'       },
];

const MULTI_COMPONENTS: MultiConfig[] = [
    { key: 'rams', slot: 'RAM', label: 'buildDetail.slotLabels.rams', urlType: 'ram' },
    { key: 'ssds', slot: 'SSD', label: 'buildDetail.slotLabels.ssds', urlType: 'ssd' },
    { key: 'hdds', slot: 'HDD', label: 'buildDetail.slotLabels.hdds', urlType: 'hdd' },
    { key: 'fans', slot: 'FAN', label: 'buildDetail.slotLabels.fans', urlType: 'fan' },
];

// --- Helpers ---

function fmt(n: number) {
    return n.toLocaleString('uk-UA');
}

function countParts(build: IPcBuildRequest): number {
    return SINGLE_COMPONENTS.filter(({ key }) => build[key] != null).length
        + MULTI_COMPONENTS.reduce((sum, { key }) => sum + (build[key] as IMultiComponentPreview[]).length, 0);
}

function shortDate(iso?: string) {
    if (!iso) return '—';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso;
    return d.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' }).toUpperCase();
}

// --- State types ---

interface DeleteModalState { isOpen: boolean; buildId: string | null; buildName: string }
interface DeleteStatus     { loading: boolean; error: string | null }

// --- Component photo placeholder ---

function ComponentPhoto({ name, slot, imageUrl }: { name: string; slot: string; imageUrl?: string }) {
    return (
        <div className={styles['thumb-mat']}>
            {imageUrl
                ? <img src={imageUrl} alt={name} className={styles['thumb-img']} />
                : <span className={styles['thumb-slot']}>{slot}</span>
            }
        </div>
    );
}

// --- BuildList ---

function BuildList({
    builds, selectedId, setSelectedId, query, setQuery,
}: {
    builds: IPcBuildList[];
    selectedId: string | null;
    setSelectedId: (id: string) => void;
    query: string;
    setQuery: (q: string) => void;
}) {
    const { t } = useTranslation();
    return (
        <div className={styles['ub-list']}>
            <div className={styles['ub-list-head']}>
                <span className={styles['lbl']}>{t('savedBuilds.yourBuilds')}</span>
                <span className={styles['count']}>{t('savedBuilds.items', { count: builds.length })}</span>
            </div>
            <div className={styles['list-search']}>
                <span className={styles['search-ic']}>⌕</span>
                <input
                    className={styles['search-input']}
                    placeholder={t('savedBuilds.filterPlaceholder')}
                    value={query}
                    onChange={e => setQuery(e.target.value)}
                />
            </div>
            <div className={styles['ub-list-body']}>
                {builds.map(b => {
                    const sel = b.id === selectedId;
                    return (
                        <div
                            key={b.id}
                            className={`${styles['ub-list-item']} ${sel ? styles['sel'] : ''}`}
                            onClick={() => setSelectedId(b.id)}
                        >
                            <span className={styles['item-name']}>{b.name}</span>
                            <span className={styles['item-price']}>
                                <span className={styles['ccy']}>₴</span>{fmt(b.price)}
                            </span>
                            <span className={styles['item-row2']}>
                                <span className={b.isPublished ? styles['status-pub'] : styles['status-priv']}>
                                    <span className={styles['status-dot']} />
                                    {b.isPublished ? t('savedBuilds.buildDetail.published') : t('savedBuilds.buildDetail.private')}
                                </span>
                                <span className={styles['item-sep']}>·</span>
                                <span>{b.componentCount} {t('savedBuilds.buildDetail.slot')}</span>
                                <span className={styles['item-sep']}>·</span>
                                <span>{shortDate(b.updatedAt)}</span>
                            </span>
                        </div>
                    );
                })}
                {builds.length === 0 && (
                    <div className={styles['list-empty']}>
                        {t('savedBuilds.noMatchBuilds')}<br />
                        <span>{t('savedBuilds.clearFilter')}</span>
                    </div>
                )}
            </div>
            <div className={styles['ub-list-foot']}>
                <button className={styles['list-foot-btn']} onClick={() => window.location.assign('/')}>
                    {t('savedBuilds.createNewBuild')}
                </button>
            </div>
        </div>
    );
}

// --- Cover ---

function Cover({
    build,
    onPhotoUploaded,
    onPhotoDeleted,
}: {
    build: IPcBuildRequest;
    onPhotoUploaded: (url: string) => void;
    onPhotoDeleted: () => void;
}) {
    const [popOpen, setPopOpen] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [uploadError, setUploadError] = useState<string | null>(null);
    const fileRef = useRef<HTMLInputElement>(null);

    const { t } = useTranslation();

    const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        if (file.size > 10 * 1024 * 1024) { setUploadError(t('savedBuilds.buildDetail.fileTooLarge')); return; }
        try {
            setUploading(true);
            setUploadError(null);
            const { data } = await buildService.uploadBuildPhoto(build.id, file);
            onPhotoUploaded(data.photoUrl);
            setPopOpen(false);
        } catch {
            setUploadError(t('savedBuilds.buildDetail.uploadFailed'));
        } finally {
            setUploading(false);
            if (fileRef.current) fileRef.current.value = '';
        }
    };

    const handleRemove = async () => {
        try {
            setUploading(true);
            setUploadError(null);
            await buildService.deleteBuildPhoto(build.id);
            onPhotoDeleted();
            setPopOpen(false);
        } catch {
            setUploadError(t('savedBuilds.buildDetail.removeFailed'));
        } finally {
            setUploading(false);
        }
    };

    return (
        <div className={styles['ub-cover-wrap']}>
            {build.photoUrl
                ? <img src={build.photoUrl} alt={build.name} className={styles['user-cover']} />
                : <BuildCover title={build.name} stamp={false} className={styles['build-cover-fill'] || ''} />
            }

            <span className={`${styles['cover-pin']} ${build.isPublished ? styles['pub'] : ''}`}>
                <span className={styles['dot']} />
                {build.isPublished ? t('savedBuilds.buildDetail.published') : t('savedBuilds.buildDetail.private')}
            </span>

            <div className={styles['cover-actions']}>
                <button
                    className={`${styles['btn']} ${styles['btn-cover']}`}
                    onClick={() => { setUploadError(null); setPopOpen(p => !p); }}
                    disabled={uploading}
                >
                    {uploading ? '…' : build.photoUrl ? t('savedBuilds.buildDetail.changeCover') : t('savedBuilds.buildDetail.uploadCover')}
                </button>
            </div>

            {popOpen && (
                <div className={styles['cover-pop']} onClick={e => e.stopPropagation()}>
                    <div className={styles['cover-pop-ttl']}>
                        <span>{t('savedBuilds.buildDetail.coverTitle', { name: build.name.slice(0, 18).toUpperCase() })}</span>
                        <span className={styles['cover-pop-x']} onClick={() => setPopOpen(false)}>×</span>
                    </div>
                    <div className={styles['cover-pop-opt']} onClick={() => fileRef.current?.click()}>
                        <span className={styles['cover-pop-gly']}>↑</span>
                        <div className={styles['cover-pop-col']}>
                            <span>{t('savedBuilds.buildDetail.uploadImage')}</span>
                            <span className={styles['cover-pop-desc']}>{t('savedBuilds.buildDetail.uploadDesc')}</span>
                        </div>
                    </div>
                    {build.photoUrl && (
                        <div className={styles['cover-pop-opt']} onClick={handleRemove}>
                            <span className={`${styles['cover-pop-gly']} ${styles['cover-pop-gly-err']}`}>×</span>
                            <div className={styles['cover-pop-col']}>
                                <span>{t('savedBuilds.buildDetail.removeCover')}</span>
                                <span className={styles['cover-pop-desc']}>{t('savedBuilds.buildDetail.removeCoverDesc')}</span>
                            </div>
                        </div>
                    )}
                    {uploadError && <div className={styles['cover-pop-err']}>{uploadError}</div>}
                    <input
                        ref={fileRef}
                        type="file"
                        accept="image/jpeg,image/png,image/webp"
                        className={styles['cover-file-input']}
                        onChange={handleFile}
                    />
                </div>
            )}
        </div>
    );
}

// --- Parts table ---

function PartsTable({ build }: { build: IPcBuildRequest }) {
    const { t } = useTranslation();
    const rows: { slot: string; label: string; comp: IComponentPreview | IMultiComponentPreview; urlType: string; isMulti?: boolean }[] = [];

    for (const { key, slot, label, urlType } of SINGLE_COMPONENTS) {
        const c = build[key] as IComponentPreview | undefined;
        if (c) rows.push({ slot, label: t(label), comp: c, urlType });
    }
    for (const { key, slot, label, urlType } of MULTI_COMPONENTS) {
        const items = build[key] as IMultiComponentPreview[];
        for (const c of items) rows.push({ slot, label: t(label), comp: c, urlType, isMulti: true });
    }

    return (
        <div className={styles['ub-parts']}>
            <div className={styles['parts-head']}>
                <span>{t('savedBuilds.buildDetail.slot')}</span>
                <span />
                <span>{t('savedBuilds.buildDetail.component')}</span>
                <span>{t('savedBuilds.buildDetail.store')}</span>
                <span className={styles['r']}>{t('savedBuilds.buildDetail.price')}</span>
                <span className={styles['r']}>{t('savedBuilds.buildDetail.offer')}</span>
            </div>
            {rows.map((r, i) => {
                const multi = r.comp as IMultiComponentPreview;
                const single = r.comp as IComponentPreview;
                const price = r.isMulti ? (multi.totalPrice ?? 0) : (single.price ?? 0);
                const qty = r.isMulti ? (multi.quantity ?? 1) : 1;
                const store = r.comp.storeName;
                const offerUrl = r.comp.productOfferUrl;
                const isCarried = price === 0;

                return (
                    <div className={styles['ub-row']} key={i}>
                        <span className={styles['row-gly']}>{r.slot}</span>
                        <ComponentPhoto name={r.comp.name} slot={r.slot} {...(r.comp.imageUrl ? { imageUrl: r.comp.imageUrl } : {})} />
                        <div className={styles['row-nm']}>
                            <div className={styles['row-name']}>
                                <Link to={`/components/${r.urlType}/${r.comp.id}`} className={styles['row-link']}>
                                    {r.comp.name}
                                </Link>
                                {qty > 1 && <span className={styles['row-qty']}>× {qty}</span>}
                            </div>
                            <div className={styles['row-meta']}>{r.label}{isCarried ? ` · ${t('savedBuilds.buildDetail.carried')}` : ''}</div>
                        </div>
                        <div className={styles['row-store']}>
                            {isCarried ? (
                                <span className={styles['carried-dash']}>—</span>
                            ) : (
                                <>
                                    <span>{store}</span>
                                </>
                            )}
                        </div>
                        {isCarried ? (
                            <div className={`${styles['row-price']} ${styles['carried']}`}>{t('savedBuilds.buildDetail.carried')}</div>
                        ) : (
                            <div className={styles['row-price']}>
                                <span className={styles['ccy']}>₴</span>{fmt(price)}
                            </div>
                        )}
                        {isCarried ? (
                            <span className={styles['row-ghost']}>—</span>
                        ) : offerUrl ? (
                            <a href={offerUrl} target="_blank" rel="noopener noreferrer" className={styles['row-open']}>
                                {t('savedBuilds.buildDetail.buy')}
                            </a>
                        ) : (
                            <span className={styles['row-ghost']}>—</span>
                        )}
                    </div>
                );
            })}
        </div>
    );
}

// --- Build detail ---

function BuildDetail({
    build, onPublishToggle, onDelete, onEdit,
    onPhotoUploaded, onPhotoDeleted, onDescriptionSaved,
    publishLoading, postBanUntil, deleteError,
}: {
    build: IPcBuildRequest;
    onPublishToggle: () => void;
    onDelete: () => void;
    onEdit: () => void;
    onPhotoUploaded: (url: string) => void;
    onPhotoDeleted: () => void;
    onDescriptionSaved: (desc: string) => void;
    publishLoading: boolean;
    postBanUntil: string | null;
    deleteError: string | null;
}) {
    const { t } = useTranslation();
    const [editingDesc, setEditingDesc] = useState(false);
    const [descDraft, setDescDraft] = useState('');
    const [descSaving, setDescSaving] = useState(false);
    const [descError, setDescError] = useState<string | null>(null);

    const startDescEdit = () => {
        setDescDraft(build.description ?? '');
        setDescError(null);
        setEditingDesc(true);
    };

    const saveDesc = async () => {
        try {
            setDescSaving(true);
            setDescError(null);
            const trimmed = descDraft.trim();
            const input = {
                name: build.name,
                ...(trimmed ? { description: trimmed } : {}),
                isPublished: build.isPublished,
                ...(build.cpu          ? { cpuId: build.cpu.id,           cpuOfferId: build.cpu.offerId }           : {}),
                ...(build.gpu          ? { gpuId: build.gpu.id,           gpuOfferId: build.gpu.offerId }           : {}),
                ...(build.motherboard  ? { motherboardId: build.motherboard.id,  motherboardOfferId: build.motherboard.offerId }  : {}),
                ...(build.cpuCooler    ? { cpuCoolerId: build.cpuCooler.id,    cpuCoolerOfferId: build.cpuCooler.offerId }    : {}),
                ...(build.powerSupply  ? { powerSupplyId: build.powerSupply.id,  powerSupplyOfferId: build.powerSupply.offerId }  : {}),
                ...(build.pcCase       ? { pcCaseId: build.pcCase.id,       pcCaseOfferId: build.pcCase.offerId }       : {}),
                rams: build.rams.map(r => ({ componentId: r.id, offerId: r.offerId, quantity: r.quantity })),
                ssds: build.ssds.map(s => ({ componentId: s.id, offerId: s.offerId, quantity: s.quantity })),
                hdds: build.hdds.map(h => ({ componentId: h.id, offerId: h.offerId, quantity: h.quantity })),
                fans: build.fans.map(f => ({ componentId: f.id, offerId: f.offerId, quantity: f.quantity })),
            };
            await buildService.updateBuild(build.id, input);
            onDescriptionSaved(descDraft.trim());
            setEditingDesc(false);
        } catch {
            setDescError(t('savedBuilds.buildDetail.descSaveFailed'));
        } finally {
            setDescSaving(false);
        }
    };

    return (
        <div className={styles['ub-detail']}>
            {/* Hero */}
            <div className={styles['ub-hero']}>
                <Cover build={build} onPhotoUploaded={onPhotoUploaded} onPhotoDeleted={onPhotoDeleted} />
                <div className={styles['ub-hero-info']}>
                    <div className={styles['ub-hero-body']}>
                        <h2 className={styles['hero-title']}>{build.name}</h2>
                        <div className={styles['hero-meta']}>
                            <span>{t('savedBuilds.buildDetail.createdDate', { date: shortDate(build.createdAt) })}</span>
                            <span className={styles['sep']}>·</span>
                            <span>{t('savedBuilds.buildDetail.updatedDate', { date: shortDate(build.updatedAt) })}</span>
                            {build.isPublished && build.publishedAt && (
                                <>
                                    <span className={styles['sep']}>·</span>
                                    <span className={styles['pub-label']}>{t('savedBuilds.buildDetail.publishedDate', { date: shortDate(build.publishedAt) })}</span>
                                </>
                            )}
                        </div>
                    </div>
                    <div className={styles['hero-actions']}>
                        {deleteError && <span className={styles['err-inline']}>{deleteError}</span>}
                        {postBanUntil && !build.isPublished && (
                            <span className={styles['ban-chip']} title={`Banned until ${new Date(postBanUntil).toLocaleString()}`}>
                                ⚿ BANNED
                            </span>
                        )}
                        <button
                            className={`${styles['btn']} ${build.isPublished ? styles['btn-sec'] : styles['btn-pri']}`}
                            onClick={onPublishToggle}
                            disabled={publishLoading || (!build.isPublished && !!postBanUntil)}
                        >
                            {publishLoading ? '…' : build.isPublished ? t('savedBuilds.buildDetail.unpublish') : t('savedBuilds.buildDetail.publish')}
                        </button>
                        <button className={`${styles['btn']} ${styles['btn-sec']}`} onClick={onEdit}>
                            {t('savedBuilds.buildDetail.editBuild')}
                        </button>
                        <button className={`${styles['btn']} ${styles['btn-dng']}`} onClick={onDelete}>
                            {t('savedBuilds.buildDetail.deleteBuild')}
                        </button>
                    </div>
                </div>
            </div>

            {/* Stats strip */}
            <div className={styles['ub-stats']}>
                <div className={styles['stat-cell']}>
                    <span className={styles['stat-lbl']}>{t('savedBuilds.buildDetail.total')}</span>
                    <span className={styles['stat-val']}>
                        <span style={{ color: 'var(--ub-fg3)', marginRight: 4 }}>₴</span>
                        {fmt(build.price)}
                    </span>
                </div>
                {build.estimatedPower != null && (
                    <div className={styles['stat-cell']}>
                        <span className={styles['stat-lbl']}>{t('savedBuilds.buildDetail.estPower')}</span>
                        <span className={styles['stat-val']}>
                            {fmt(build.estimatedPower)}
                            <span style={{ color: 'var(--ub-fg3)', marginLeft: 4, fontSize: 11 }}>W</span>
                        </span>
                        {build.psuWattage && (
                            <span className={styles['stat-sub']}>
                                {t('savedBuilds.buildDetail.psuLoad', { psu: build.psuWattage, load: Math.round((build.estimatedPower / build.psuWattage) * 100) })}
                            </span>
                        )}
                    </div>
                )}
                <div className={styles['stat-cell']}>
                    <span className={styles['stat-lbl']}>{t('savedBuilds.buildDetail.status')}</span>
                    <span className={`${styles['stat-val']} ${build.isPublished ? styles['ok'] : styles['warn']}`}>
                        {build.isPublished ? t('savedBuilds.buildDetail.published') : t('savedBuilds.buildDetail.private')}
                    </span>
                    <span className={styles['stat-sub']}>
                        {build.isPublished ? t('savedBuilds.buildDetail.visibleInGallery') : t('savedBuilds.buildDetail.onlyYouCanSee')}
                    </span>
                </div>
            </div>

            {/* Description */}
            <div className={styles['ub-desc']}>
                <div className={styles['desc-head']}>
                    <span className={styles['eyebrow']}>{t('savedBuilds.buildDetail.description')}</span>
                    {!editingDesc && (
                        <button className={styles['desc-edit-btn']} onClick={startDescEdit}>
                            ✎ {build.description ? t('savedBuilds.buildDetail.edit') : t('savedBuilds.buildDetail.add')}
                        </button>
                    )}
                </div>
                {editingDesc ? (
                    <div className={styles['desc-edit-wrap']}>
                        <textarea
                            className={styles['desc-textarea']}
                            value={descDraft}
                            onChange={e => setDescDraft(e.target.value)}
                            placeholder={t('savedBuilds.buildDetail.addDescPlaceholder')}
                            rows={4}
                            autoFocus
                        />
                        {descError && <div className={styles['desc-err']}>{descError}</div>}
                        <div className={styles['desc-edit-actions']}>
                            <button
                                className={`${styles['btn']} ${styles['btn-sec']}`}
                                onClick={() => setEditingDesc(false)}
                                disabled={descSaving}
                            >
                                {t('savedBuilds.buildDetail.cancel')}
                            </button>
                            <button
                                className={`${styles['btn']} ${styles['btn-pri']}`}
                                onClick={saveDesc}
                                disabled={descSaving}
                            >
                                {descSaving ? '…' : t('savedBuilds.buildDetail.save')}
                            </button>
                        </div>
                    </div>
                ) : build.description ? (
                    <p className={styles['desc-text']}>{build.description}</p>
                ) : (
                    <p className={styles['desc-empty']}>{t('savedBuilds.buildDetail.noDescription')}</p>
                )}
            </div>

            {/* Parts table */}
            <div className={styles['ub-parts-wrap']}>
                <div className={styles['parts-wrap-head']}>
                    <span className={styles['eyebrow']}>{t('savedBuilds.buildDetail.components', { count: countParts(build) })}</span>
                    <span className={styles['parts-subtotal']}>
                        {t('savedBuilds.buildDetail.subtotal')} <strong>₴ {fmt(build.price)}</strong>
                    </span>
                </div>
                <PartsTable build={build} />
                <div className={styles['parts-foot']}>
                    <span className={styles['parts-foot-ttl']}>{t('savedBuilds.buildDetail.finalPrice')}</span>
                    <span className={styles['parts-foot-num']}>
                        <span className={styles['ccy']}>₴</span>{fmt(build.price)}
                    </span>
                </div>
            </div>
        </div>
    );
}

// --- Delete modal ---

function DeleteModal({
    build, onCancel, onConfirm, loading,
}: {
    build: IPcBuildList;
    onCancel: () => void;
    onConfirm: () => void;
    loading: boolean;
}) {
    const { t } = useTranslation();
    return (
        <>
            <div className={styles['del-overlay']} onClick={onCancel} />
            <div className={styles['del-modal']} role="dialog" aria-modal="true">
                <div className={styles['dm-head']}>
                    <span className={styles['dm-ttl']}>{t('savedBuilds.deleteModal.title')}</span>
                    <span className={styles['dm-x']} onClick={onCancel}>×</span>
                </div>
                <div className={styles['dm-body']}>
                    <p>{t('savedBuilds.deleteModal.confirmPrefix')} <strong>{build.name}</strong>?</p>
                    <p className={styles['dm-sub']}>
                        {t('savedBuilds.deleteModal.subText')}
                    </p>
                </div>
                <div className={styles['dm-foot']}>
                    <button className={`${styles['btn']} ${styles['btn-sec']}`} onClick={onCancel}>
                        {t('savedBuilds.deleteModal.cancel')}
                    </button>
                    <button className={`${styles['btn']} ${styles['btn-dng']}`} onClick={onConfirm} disabled={loading}>
                        {loading ? '…' : t('savedBuilds.deleteModal.confirm')}
                    </button>
                </div>
            </div>
        </>
    );
}

// --- Main page ---

function UserBuildsPage() {
    const { t } = useTranslation();
    const [builds, setBuilds] = useState<IPcBuildList[]>([]);
    const [selectedBuild, setSelectedBuild] = useState<IPcBuildRequest | null>(null);
    const [selectedBuildId, setSelectedBuildId] = useState<string | null>(null);
    const [query, setQuery] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deleteStatus, setDeleteStatus] = useState<DeleteStatus>({ loading: false, error: null });
    const [deleteModal, setDeleteModal] = useState<DeleteModalState>({ isOpen: false, buildId: null, buildName: '' });
    const [publishLoading, setPublishLoading] = useState(false);
    const [postBanUntil, setPostBanUntil] = useState<string | null>(null);

    const navigate = useNavigate();
    const { auth } = useAuth();

    const filtered = useMemo(() => {
        if (!query.trim()) return builds;
        const q = query.toLowerCase();
        return builds.filter(b => b.name.toLowerCase().includes(q));
    }, [builds, query]);

    // keep selection valid as list mutates
    useEffect(() => {
        if (!selectedBuildId && filtered.length > 0) setSelectedBuildId(filtered[0]!.id);
    }, [filtered, selectedBuildId]);

    useEffect(() => {
        if (!auth?.accessToken) { setLoading(false); return; }
        const fetch = async () => {
            try {
                setLoading(true);
                const { data } = await buildService.getUserBuilds();
                setBuilds(data);
                if (data.length > 0) setSelectedBuildId(data[0]!.id);
                setError(null);
            } catch {
                setError(t('savedBuilds.loadFailed'));
            } finally {
                setLoading(false);
            }
        };
        fetch();
        profileService.getBans().then(res => {
            setPostBanUntil(res.data.isPostBanned ? res.data.postBanUntil : null);
        }).catch(() => {});
    }, [auth?.accessToken, t]);

    useEffect(() => {
        if (!selectedBuildId) return;
        buildService.getBuildById(selectedBuildId)
            .then(({ data }) => setSelectedBuild(data))
            .catch(err => console.error(`Error fetching build ${selectedBuildId}:`, err));
    }, [selectedBuildId]);

    const handleDeleteBuild = async () => {
        if (!deleteModal.buildId) return;
        try {
            setDeleteStatus({ loading: true, error: null });
            await buildService.deleteBuild(deleteModal.buildId);
            setBuilds(prev => prev.filter(b => b.id !== deleteModal.buildId));
            if (selectedBuildId === deleteModal.buildId) {
                const remaining = builds.filter(b => b.id !== deleteModal.buildId);
                if (remaining.length > 0) setSelectedBuildId(remaining[0]!.id);
                else { setSelectedBuildId(null); setSelectedBuild(null); }
            }
            setDeleteStatus({ loading: false, error: null });
            setDeleteModal({ isOpen: false, buildId: null, buildName: '' });
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message || t('savedBuilds.deleteModal.deleteFailed');
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
            const comp = selectedBuild[key] as IComponentPreview | undefined;
            if (comp) {
                transformed[key as SingleKey] = {
                    componentId: comp.id, offerId: comp.offerId,
                    price: comp.price ?? 0, storeName: comp.storeName ?? '',
                    storeLogoUrl: comp.imageUrl ?? null, productOfferUrl: comp.productOfferUrl ?? null,
                };
            }
        }
        for (const { key } of MULTI_COMPONENTS) {
            const items = selectedBuild[key] as IMultiComponentPreview[];
            if (items.length > 0) {
                transformed[key as MultiKey] = items.map(item => ({
                    componentId: item.id, offerId: item.offerId,
                    quantity: item.quantity || 1,
                    price: item.totalPrice ? item.totalPrice / (item.quantity || 1) : 0,
                    storeName: item.storeName ?? '', storeLogoUrl: item.imageUrl ?? null,
                    productOfferUrl: item.productOfferUrl ?? null,
                }));
            }
        }
        localStorage.setItem('selectedComponents', JSON.stringify(transformed));
        localStorage.setItem('editingBuild', JSON.stringify({
            id: selectedBuild.id, name: selectedBuild.name,
            isPublished: selectedBuild.isPublished,
            ...(selectedBuild.description && { description: selectedBuild.description }),
        }));
        navigate('/');
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
            setBuilds(prev => prev.map(b =>
                b.id === selectedBuild.id ? { ...b, isPublished: newState } : b
            ));
        } catch (err) {
            console.error('Failed to toggle publish:', err);
        } finally {
            setPublishLoading(false);
        }
    };

    const publishedCount = builds.filter(b => b.isPublished).length;

    const deletingBuild = deleteModal.buildId
        ? builds.find(b => b.id === deleteModal.buildId) ?? null
        : null;

    if (loading) return <div className={styles['loading']}>{t('savedBuilds.loading')}</div>;

    return (
        <div className={styles['ub-page']}>
            {deleteModal.isOpen && deletingBuild && (
                <DeleteModal
                    build={deletingBuild}
                    onCancel={() => setDeleteModal({ isOpen: false, buildId: null, buildName: '' })}
                    onConfirm={handleDeleteBuild}
                    loading={deleteStatus.loading}
                />
            )}

            <div className={styles['ub-shell']}>
                <div className={styles['ub-head']}>
                    <div>
                        <span className={styles['eyebrow']}>
                            {t('savedBuilds.eyebrow', { count: builds.length })}
                        </span>
                        <h1 className={styles['ub-h1']}>{t('savedBuilds.heading')}</h1>
                        <div className={styles['head-meta']}>
                            <span>{t('savedBuilds.published', { count: publishedCount })}</span>
                            <span className={styles['sep']}>·</span>
                            <span>{t('savedBuilds.private', { count: builds.length - publishedCount })}</span>
                        </div>
                    </div>
                    <div className={styles['head-actions']}>
                        <Link to="/gallery">
                            <button className={`${styles['btn']} ${styles['btn-ghost']}`}>{t('savedBuilds.browseGallery')}</button>
                        </Link>
                        <button
                            className={`${styles['btn']} ${styles['btn-pri']}`}
                            onClick={() => { localStorage.removeItem('selectedComponents'); localStorage.removeItem('editingBuild'); navigate('/'); }}
                        >
                            {t('savedBuilds.newBuild')}
                        </button>
                    </div>
                </div>

                {error && <div className={styles['error']}>{error}</div>}

                {builds.length === 0 && !error ? (
                    <div className={styles['ub-empty']}>
                        <span className={styles['eyebrow']}>{t('savedBuilds.noBuilds.eyebrow')}</span>
                        <h2>{t('savedBuilds.noBuilds.title')}</h2>
                        <p>{t('savedBuilds.noBuilds.body')}</p>
                        <button
                            className={`${styles['btn']} ${styles['btn-pri']}`}
                            onClick={() => { localStorage.removeItem('selectedComponents'); localStorage.removeItem('editingBuild'); navigate('/'); }}
                        >
                            {t('savedBuilds.noBuilds.cta')}
                        </button>
                    </div>
                ) : (
                    <div className={styles['ub-pane']}>
                        <BuildList
                            builds={filtered}
                            selectedId={selectedBuildId}
                            setSelectedId={setSelectedBuildId}
                            query={query}
                            setQuery={setQuery}
                        />
                        {selectedBuild ? (
                            <BuildDetail
                                build={selectedBuild}
                                onPublishToggle={handleTogglePublish}
                                onDelete={() => setDeleteModal({ isOpen: true, buildId: selectedBuild.id, buildName: selectedBuild.name })}
                                onEdit={handleEditBuild}
                                onPhotoUploaded={url => setSelectedBuild(prev => prev ? { ...prev, photoUrl: url } : prev)}
                                onPhotoDeleted={() => setSelectedBuild(prev => { if (!prev) return prev; const { photoUrl: _, ...rest } = prev; return rest as IPcBuildRequest; })}
                                onDescriptionSaved={desc => setSelectedBuild(prev => prev ? { ...prev, ...(desc ? { description: desc } : {}) } : prev)}
                                publishLoading={publishLoading}
                                postBanUntil={postBanUntil}
                                deleteError={deleteStatus.error}
                            />
                        ) : (
                            <div className={styles['ub-detail']}>
                                <div className={styles['ub-empty']}>
                                    <span className={styles['eyebrow']}>{t('savedBuilds.selectBuild.eyebrow')}</span>
                                    <h2>{t('savedBuilds.selectBuild.title')}</h2>
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}

export default UserBuildsPage;
