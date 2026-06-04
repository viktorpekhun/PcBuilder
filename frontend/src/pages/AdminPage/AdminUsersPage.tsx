import { useCallback, useEffect, useRef, useState } from "react";
import { Pagination } from "../../components/Pagination/Pagination";
import { adminService } from "../../api/admin.service";
import { useTranslation } from "react-i18next";
import {
    BanType,
    type BanTypeValue,
    type IAdminUser,
    type IAdminUserDetail,
    type IPaginationHeader,
} from "../../types/admin.types";
import styles from "./AdminUsersPage.module.css";

const PAGE_SIZE = 20;

const fmtDate = (iso: string | null) =>
    iso ? new Date(iso).toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" }) : "—";

const initials = (username: string) =>
    username.replace(/[^a-zA-Z]/g, "").slice(0, 2).toUpperCase();

// ── Segmented control ────────────────────────────────────────────────────────
interface SegOption<T> { value: T; label: string; }
function Seg<T extends number | string>({ value, onChange, options, fill }: { value: T; onChange: (v: T) => void; options: SegOption<T>[]; fill?: boolean }) {
    return (
        <div className={fill ? styles.segFill : styles.seg}>
            {options.map(o => (
                <button
                    key={String(o.value)}
                    type="button"
                    className={`${styles.segBtn} ${value === o.value ? styles.segBtnActive : ""}`}
                    onClick={() => onChange(o.value)}
                >
                    {o.label}
                </button>
            ))}
        </div>
    );
}

// ── Modal shell ──────────────────────────────────────────────────────────────
interface ModalProps { title: string; sub?: string; onClose: () => void; children: React.ReactNode; footer: React.ReactNode; }
const Modal = ({ title, sub, onClose, children, footer }: ModalProps) => {
    useEffect(() => {
        const h = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
        window.addEventListener("keydown", h);
        return () => window.removeEventListener("keydown", h);
    }, [onClose]);
    return (
        <div className={styles.scrim} onClick={onClose}>
            <div className={styles.modal} onClick={e => e.stopPropagation()}>
                <div className={styles.modalHead}>
                    <div>
                        <div className={styles.modalTitle}>{title}</div>
                        {sub && <div className={styles.modalSub}>{sub}</div>}
                    </div>
                    <button type="button" className={styles.modalClose} onClick={onClose}>×</button>
                </div>
                <div className={styles.modalBody}>{children}</div>
                <div className={styles.modalFoot}>{footer}</div>
            </div>
        </div>
    );
};

// ── Ban modal ────────────────────────────────────────────────────────────────
interface BanModalProps { user: IAdminUser; onClose: () => void; onConfirm: (p: { banType: BanTypeValue; durationDays: number; reason: string }) => void; }
const BanModal = ({ user, onClose, onConfirm }: BanModalProps) => {
    const { t } = useTranslation();
    const [banType, setBanType] = useState<BanTypeValue>(BanType.Comment);
    const [days, setDays] = useState(7);
    const [reason, setReason] = useState("");
    const [err, setErr] = useState<string | null>(null);

    const submit = () => {
        if (!reason.trim()) { setErr(t("admin.usersPage.banModal.reasonRequired")); return; }
        if (days < 1 || days > 365) { setErr(t("admin.usersPage.banModal.durationInvalid")); return; }
        onConfirm({ banType, durationDays: days, reason });
    };

    const banTypeOpts = [
        { value: BanType.Comment as BanTypeValue, label: t("admin.usersPage.banModal.commentLabel") },
        { value: BanType.Post as BanTypeValue,    label: t("admin.usersPage.banModal.postLabel") },
    ];

    return (
        <Modal title={t("admin.usersPage.banModal.title")} sub={`@${user.username} · ${user.email}`} onClose={onClose}
            footer={<>
                <button type="button" className={styles.btnGhost} onClick={onClose}>{t("admin.usersPage.banModal.cancel")}</button>
                <button type="button" className={styles.btnDanger} onClick={submit}>{t("admin.usersPage.banModal.confirm")}</button>
            </>}>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>{t("admin.usersPage.banModal.banType")}</label>
                <Seg fill value={banType} onChange={setBanType} options={banTypeOpts} />
                <span className={styles.fieldHint}>
                    {banType === BanType.Comment ? t("admin.usersPage.banModal.commentBanHint") : t("admin.usersPage.banModal.postBanHint")}
                </span>
            </div>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>{t("admin.usersPage.banModal.duration")}</label>
                <div className={styles.fieldRow}>
                    <input
                        type="number" min={1} max={365}
                        className={`${styles.fieldInput} ${styles.fieldInputNum}`}
                        value={days}
                        onChange={e => setDays(Number(e.target.value))}
                    />
                    <span className={styles.fieldDays}>{t("admin.usersPage.banModal.days")}</span>
                    <div className={styles.fieldGrow} />
                    <Seg value={days} onChange={setDays}
                        options={[{ value: 1, label: "1d" }, { value: 7, label: "7d" }, { value: 30, label: "30d" }, { value: 365, label: "1y" }]} />
                </div>
            </div>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>{t("admin.usersPage.banModal.reason")}</label>
                <textarea className={styles.fieldTextarea} maxLength={500} value={reason}
                    onChange={e => setReason(e.target.value)}
                    placeholder={t("admin.usersPage.banModal.reasonPlaceholder")} />
            </div>
            {err && <div className={styles.fieldError}>{err}</div>}
        </Modal>
    );
};

// ── Unban modal ──────────────────────────────────────────────────────────────
interface UnbanModalProps { user: IAdminUser; onClose: () => void; onConfirm: (p: { banType: BanTypeValue }) => void; }
const UnbanModal = ({ user, onClose, onConfirm }: UnbanModalProps) => {
    const { t } = useTranslation();
    const both = user.isCommentBanned && user.isPostBanned;
    const [banType, setBanType] = useState<BanTypeValue>(user.isCommentBanned ? BanType.Comment : BanType.Post);

    const banTypeOpts = [
        { value: BanType.Comment as BanTypeValue, label: t("admin.usersPage.unbanModal.commentLabel") },
        { value: BanType.Post as BanTypeValue,    label: t("admin.usersPage.unbanModal.postLabel") },
    ];

    return (
        <Modal title={t("admin.usersPage.unbanModal.title")} sub={`@${user.username}`} onClose={onClose}
            footer={<>
                <button type="button" className={styles.btnGhost} onClick={onClose}>{t("admin.usersPage.unbanModal.cancel")}</button>
                <button type="button" className={styles.btnPrimary} onClick={() => onConfirm({ banType })}>{t("admin.usersPage.unbanModal.confirm")}</button>
            </>}>
            {both ? (
                <div className={styles.field}>
                    <label className={styles.fieldLabel}>{t("admin.usersPage.unbanModal.whichBan")}</label>
                    <Seg fill value={banType} onChange={setBanType} options={banTypeOpts} />
                </div>
            ) : (
                <p className={styles.modalBodyText}>
                    {user.isCommentBanned
                        ? t("admin.usersPage.unbanModal.liftComment", { username: user.username })
                        : t("admin.usersPage.unbanModal.liftPost", { username: user.username })}
                </p>
            )}
        </Modal>
    );
};

// ── Confirm modal ─────────────────────────────────────────────────────────────
interface ConfirmModalProps { title: string; sub?: string; body: string; confirmLabel: string; danger?: boolean; onClose: () => void; onConfirm: () => void; cancelLabel: string; }
const ConfirmModal = ({ title, sub, body, confirmLabel, danger, onClose, onConfirm, cancelLabel }: ConfirmModalProps) => (
    <Modal title={title} {...(sub !== undefined && { sub })} onClose={onClose}
        footer={<>
            <button type="button" className={styles.btnGhost} onClick={onClose}>{cancelLabel}</button>
            <button type="button" className={danger ? styles.btnDanger : styles.btnPrimary} onClick={onConfirm}>{confirmLabel}</button>
        </>}>
        <p className={styles.modalBodyText}>{body}</p>
    </Modal>
);

// ── Row menu popover ──────────────────────────────────────────────────────────
interface RowMenuProps { user: IAdminUser; anchor: { top: number; right: number }; onClose: () => void; onPick: (action: string) => void; }
const RowMenu = ({ user, anchor, onClose, onPick }: RowMenuProps) => {
    const { t } = useTranslation();
    const ref = useRef<HTMLDivElement>(null);
    useEffect(() => {
        const h = (e: MouseEvent) => { if (ref.current && !ref.current.contains(e.target as Node)) onClose(); };
        const k = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
        document.addEventListener("mousedown", h);
        window.addEventListener("keydown", k);
        return () => { document.removeEventListener("mousedown", h); window.removeEventListener("keydown", k); };
    }, [onClose]);
    const isAdmin = user.roles.includes("Admin");
    const banned = user.isCommentBanned || user.isPostBanned;
    return (
        <div className={styles.menu} ref={ref} style={{ top: anchor.top, right: anchor.right }}>
            <button type="button" className={styles.menuItem} onClick={() => onPick("view")}>
                <span className={styles.menuIcon}>◰</span> {t("admin.usersPage.menu.viewDetails")}
            </button>
            <div className={styles.menuSep} />
            <button type="button" className={styles.menuItem} onClick={() => onPick("ban")}>
                <span className={styles.menuIcon}>⊘</span> {t("admin.usersPage.menu.banUser")}
            </button>
            {banned && (
                <button type="button" className={styles.menuItem} onClick={() => onPick("unban")}>
                    <span className={styles.menuIcon}>✓</span> {t("admin.usersPage.menu.liftBan")}
                </button>
            )}
            <button type="button" className={styles.menuItem} onClick={() => onPick("role")}>
                <span className={styles.menuIcon}>{isAdmin ? "▼" : "▲"}</span>{" "}
                {isAdmin ? t("admin.usersPage.menu.demote") : t("admin.usersPage.menu.promote")}
            </button>
            <div className={styles.menuSep} />
            <button type="button" className={`${styles.menuItem} ${styles.menuItemDanger}`} onClick={() => onPick("delete")}>
                <span className={styles.menuIcon}>×</span> {t("admin.usersPage.menu.deleteAccount")}
            </button>
        </div>
    );
};

// ── User detail panel ─────────────────────────────────────────────────────────
interface UserDetailPanelProps { userId: string; onClose: () => void; }
const UserDetailPanel = ({ userId, onClose }: UserDetailPanelProps) => {
    const { t } = useTranslation();
    const [detail, setDetail] = useState<IAdminUserDetail | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;
        adminService.getUserDetail(userId)
            .then(res => { if (!cancelled) setDetail(res.data); })
            .catch(() => { if (!cancelled) setDetail(null); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, [userId]);

    const banTypeLabel = (value: BanTypeValue) => value === BanType.Comment
        ? t("admin.usersPage.banModal.commentLabel")
        : t("admin.usersPage.banModal.postLabel");

    return (
        <>
            <div className={styles.panelScrim} onClick={onClose} />
            <div className={styles.panel}>
                <div className={styles.panelHead}>
                    <div className={styles.panelUserRow}>
                        {detail ? (
                            detail.avatarUrl
                                ? <div className={styles.avatarLg}><img src={detail.avatarUrl} alt={detail.username} className={styles.avatarImg} /></div>
                                : <div className={styles.avatarLg}>{initials(detail.username)}</div>
                        ) : <div className={styles.avatarLg}>…</div>}
                        <div>
                            <div className={styles.panelUsername}>@{detail?.username ?? "…"}</div>
                            <div className={styles.panelEmail}>{detail?.email ?? ""}</div>
                        </div>
                    </div>
                    <button type="button" className={styles.panelClose} onClick={onClose}>×</button>
                </div>

                <div className={styles.panelBody}>
                    {loading ? (
                        <div className={styles.panelEmpty}>{t("admin.usersPage.detail.loading")}</div>
                    ) : !detail ? (
                        <div className={styles.panelEmpty}>{t("admin.usersPage.detail.failed")}</div>
                    ) : (
                        <>
                            <section>
                                <div className={styles.psecTitle}>{t("admin.usersPage.detail.profile")}</div>
                                {([
                                    [t("admin.usersPage.detail.fields.username"),   detail.username],
                                    [t("admin.usersPage.detail.fields.email"),      detail.email],
                                    [t("admin.usersPage.detail.fields.verified"),   detail.isEmailVerified ? t("admin.usersPage.detail.fields.yes") : t("admin.usersPage.detail.fields.no")],
                                    [t("admin.usersPage.detail.fields.roles"),      detail.roles.join(", ") || "—"],
                                    [t("admin.usersPage.detail.fields.registered"), fmtDate(detail.createdAt)],
                                    [t("admin.usersPage.detail.fields.builds"),     String(detail.buildCount)],
                                    [t("admin.usersPage.detail.fields.reviews"),    String(detail.reviewCount)],
                                ] as [string, string][]).map(([k, v]) => (
                                    <div className={styles.frow} key={k}>
                                        <span className={styles.fk}>{k}</span>
                                        <span className={styles.fv}>{v}</span>
                                    </div>
                                ))}
                            </section>

                            <section>
                                <div className={styles.psecTitle}>{t("admin.usersPage.detail.activeBans")}</div>
                                {!detail.isCommentBanned && !detail.isPostBanned ? (
                                    <div className={styles.panelEmpty}>{t("admin.usersPage.detail.noBans")}</div>
                                ) : (
                                    <>
                                        {detail.isCommentBanned && (
                                            <div className={styles.frow}>
                                                <span className={styles.fk}>{t("admin.usersPage.detail.fields.commentBanUntil")}</span>
                                                <span className={`${styles.fv} ${styles.fvErr}`}>{fmtDate(detail.commentBanUntil ?? null)}</span>
                                            </div>
                                        )}
                                        {detail.isPostBanned && (
                                            <div className={styles.frow}>
                                                <span className={styles.fk}>{t("admin.usersPage.detail.fields.postBanUntil")}</span>
                                                <span className={`${styles.fv} ${styles.fvErr}`}>{fmtDate(detail.postBanUntil ?? null)}</span>
                                            </div>
                                        )}
                                    </>
                                )}
                            </section>

                            <section>
                                <div className={styles.psecTitle}>{t("admin.usersPage.detail.warningHistory", { count: detail.warnings.length })}</div>
                                {detail.warnings.length === 0 ? (
                                    <div className={styles.panelEmpty}>{t("admin.usersPage.detail.noWarnings")}</div>
                                ) : detail.warnings.map(w => (
                                    <div className={styles.warnItem} key={w.id}>
                                        <div className={styles.warnHead}>
                                            <span>{banTypeLabel(w.banType)} · {fmtDate(w.issuedAt)}</span>
                                            {w.issuedByAdminUsername && <span>by @{w.issuedByAdminUsername}</span>}
                                        </div>
                                        <div className={styles.warnReason}>{w.reason}</div>
                                    </div>
                                ))}
                            </section>
                        </>
                    )}
                </div>
            </div>
        </>
    );
};

// ── Main page ─────────────────────────────────────────────────────────────────
const AdminUsersPage = () => {
    const { t } = useTranslation();
    const [users, setUsers] = useState<IAdminUser[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchInput, setSearchInput] = useState("");
    const [search, setSearch] = useState("");
    const [pageNumber, setPageNumber] = useState(1);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [detailId, setDetailId] = useState<string | null>(null);
    const [menu, setMenu] = useState<{ userId: string; anchor: { top: number; right: number } } | null>(null);
    const [action, setAction] = useState<{ type: string; userId: string; isAdmin?: boolean } | null>(null);

    const fetchUsers = useCallback(async (query: string, page: number) => {
        setLoading(true);
        try {
            const { items, pagination } = await adminService.getUsers({ searchQuery: query, pageNumber: page, pageSize: PAGE_SIZE });
            setUsers(items);
            setPagination(pagination);
        } catch {
            setUsers([]);
            setPagination(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { fetchUsers(search, pageNumber); }, [fetchUsers, search, pageNumber]);

    const doSearch = (e: React.FormEvent) => { e.preventDefault(); setSearch(searchInput); setPageNumber(1); };
    const clearSearch = () => { setSearch(""); setSearchInput(""); setPageNumber(1); };

    const actionUser = action ? users.find(u => u.id === action.userId) ?? null : null;

    const onMenuPick = (type: string) => {
        if (!menu) return;
        const uid = menu.userId;
        setMenu(null);
        if (type === "view") { setDetailId(uid); return; }
        if (type === "role") {
            const u = users.find(x => x.id === uid);
            setAction({ type: "role", userId: uid, ...(u !== undefined && { isAdmin: u.roles.includes("Admin") }) });
            return;
        }
        setAction({ type, userId: uid });
    };

    const closeAction = () => setAction(null);

    const totalCount = pagination?.totalCount ?? users.length;

    return (
        <div className={styles.page}>
            {/* Content bar */}
            <div className={styles.contentBar}>
                <span className={styles.contentMeta}>
                    {search
                        ? t("admin.usersPage.accountsMatching_other", { count: totalCount })
                        : t("admin.usersPage.accounts_other", { count: totalCount })}
                    {t("admin.usersPage.clickForDetail")}
                </span>
                <form className={styles.searchForm} onSubmit={doSearch}>
                    <span className={styles.searchIcon}>⌕</span>
                    <input
                        className={styles.searchInput}
                        placeholder={t("admin.usersPage.searchPlaceholder")}
                        value={searchInput}
                        onChange={e => setSearchInput(e.target.value)}
                    />
                </form>
            </div>

            {/* Table */}
            <div className={styles.tableWrap}>
                {loading ? (
                    <div className={styles.loading}>{t("admin.usersPage.loading")}</div>
                ) : users.length === 0 ? (
                    <div className={styles.empty}>
                        <span className={styles.emptyGlyph}>∅</span>
                        <span className={styles.emptyMsg}>{t("admin.usersPage.noMatch", { query: search })}</span>
                        <button type="button" className={styles.emptyLink} onClick={clearSearch}>{t("admin.usersPage.clearSearch")}</button>
                    </div>
                ) : (
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th style={{ width: 44 }}></th>
                                <th>{t("admin.usersPage.table.username")}</th>
                                <th>{t("admin.usersPage.table.email")}</th>
                                <th style={{ width: 120 }}>{t("admin.usersPage.table.roles")}</th>
                                <th style={{ width: 120 }}>{t("admin.usersPage.table.registered")}</th>
                                <th style={{ width: 130 }}>{t("admin.usersPage.table.bans")}</th>
                                <th className={styles.thRight} style={{ width: 60 }}>{t("admin.usersPage.table.actions")}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map(u => (
                                <tr key={u.id}>
                                    <td>
                                        <div className={styles.avatar}>
                                            {u.avatarUrl
                                                ? <img src={u.avatarUrl} alt={u.username} className={styles.avatarImg} />
                                                : initials(u.username)}
                                        </div>
                                    </td>
                                    <td className={styles.tdName}>
                                        <button type="button" className={styles.usernameBtn} onClick={() => setDetailId(u.id)}>
                                            @{u.username}
                                        </button>
                                    </td>
                                    <td className={styles.tdEmail}>{u.email}</td>
                                    <td>
                                        <div style={{ display: "flex", gap: 4, flexWrap: "wrap" }}>
                                            {(u.roles.includes("Admin")
                                                ? u.roles.filter(r => r !== "User")
                                                : u.roles
                                            ).map(r => (
                                                <span key={r} className={`${styles.roleBadge} ${r === "Admin" ? styles.roleBadgeAdmin : ""}`}>{r}</span>
                                            ))}
                                        </div>
                                    </td>
                                    <td className={styles.tdDim}>{fmtDate(u.createdAt)}</td>
                                    <td>
                                        <div style={{ display: "flex", gap: 4, flexWrap: "wrap" }}>
                                            {u.isCommentBanned && <span className={styles.banBadge}>{t("admin.usersPage.banModal.commentLabel")}</span>}
                                            {u.isPostBanned && <span className={styles.banBadge}>{t("admin.usersPage.banModal.postLabel")}</span>}
                                            {!u.isCommentBanned && !u.isPostBanned && <span className={styles.tdDim}>—</span>}
                                        </div>
                                    </td>
                                    <td>
                                        <div className={styles.actionsCell}>
                                            <button
                                                type="button"
                                                className={styles.menuBtn}
                                                onClick={e => {
                                                    const r = e.currentTarget.getBoundingClientRect();
                                                    setMenu({ userId: u.id, anchor: { top: r.bottom + 4, right: window.innerWidth - r.right } });
                                                }}
                                            >⋯</button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}

                {pagination && (
                    <Pagination
                        currentPage={pagination.pageNumber}
                        totalPages={pagination.totalPages}
                        totalResults={pagination.totalCount}
                        pageSize={PAGE_SIZE}
                        onPageChange={setPageNumber}
                    />
                )}
            </div>

            {/* Popover menu */}
            {menu && (
                <RowMenu
                    user={users.find(u => u.id === menu.userId)!}
                    anchor={menu.anchor}
                    onClose={() => setMenu(null)}
                    onPick={onMenuPick}
                />
            )}

            {/* Action modals */}
            {action?.type === "ban" && actionUser && (
                <BanModal user={actionUser} onClose={closeAction} onConfirm={async ({ banType, durationDays, reason }) => {
                    try {
                        await adminService.banUser(actionUser.id, { banType, durationDays, reason });
                        closeAction();
                        await fetchUsers(search, pageNumber);
                    } catch { /* swallow */ }
                }} />
            )}

            {action?.type === "unban" && actionUser && (
                <UnbanModal user={actionUser} onClose={closeAction} onConfirm={async ({ banType }) => {
                    try {
                        await adminService.unbanUser(actionUser.id, { banType });
                        closeAction();
                        await fetchUsers(search, pageNumber);
                    } catch { /* swallow */ }
                }} />
            )}

            {action?.type === "role" && actionUser && (
                <ConfirmModal
                    title={action.isAdmin ? t("admin.usersPage.roleModal.demoteTitle") : t("admin.usersPage.roleModal.promoteTitle")}
                    sub={`@${actionUser.username}`}
                    body={action.isAdmin
                        ? t("admin.usersPage.roleModal.demoteBody", { username: actionUser.username })
                        : t("admin.usersPage.roleModal.promoteBody", { username: actionUser.username })}
                    confirmLabel={action.isAdmin ? t("admin.usersPage.roleModal.demoteConfirm") : t("admin.usersPage.roleModal.promoteConfirm")}
                    cancelLabel={t("admin.usersPage.roleModal.cancel")}
                    {...(action.isAdmin && { danger: true })}
                    onClose={closeAction}
                    onConfirm={async () => {
                        try {
                            const role: "Admin" | "User" = action.isAdmin ? "User" : "Admin";
                            await adminService.changeUserRole(actionUser.id, { role });
                            closeAction();
                            await fetchUsers(search, pageNumber);
                        } catch { /* swallow */ }
                    }}
                />
            )}

            {action?.type === "delete" && actionUser && (
                <ConfirmModal
                    title={t("admin.usersPage.deleteModal.title")}
                    sub={`@${actionUser.username}`}
                    body={t("admin.usersPage.deleteModal.body", { username: actionUser.username })}
                    confirmLabel={t("admin.usersPage.deleteModal.confirm")}
                    cancelLabel={t("admin.usersPage.deleteModal.cancel")}
                    danger
                    onClose={closeAction}
                    onConfirm={async () => {
                        try {
                            await adminService.deleteUser(actionUser.id);
                            closeAction();
                            await fetchUsers(search, pageNumber);
                            window.dispatchEvent(new Event("admin:statsChanged"));
                        } catch { /* swallow */ }
                    }}
                />
            )}

            {detailId && <UserDetailPanel userId={detailId} onClose={() => setDetailId(null)} />}
        </div>
    );
};

export default AdminUsersPage;
