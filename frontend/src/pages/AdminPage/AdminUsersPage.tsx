import { useCallback, useEffect, useRef, useState } from "react";
import { Pagination } from "../../components/Pagination/Pagination";
import { adminService } from "../../api/admin.service";
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

const banTypeLabel = (value: BanTypeValue) => value === BanType.Comment ? "Comment" : "Post";

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
    const [banType, setBanType] = useState<BanTypeValue>(BanType.Comment);
    const [days, setDays] = useState(7);
    const [reason, setReason] = useState("");
    const [err, setErr] = useState<string | null>(null);

    const submit = () => {
        if (!reason.trim()) { setErr("Reason is required — it's shown to the user."); return; }
        if (days < 1 || days > 365) { setErr("Duration must be between 1 and 365 days."); return; }
        onConfirm({ banType, durationDays: days, reason });
    };

    return (
        <Modal title="Ban user" sub={`@${user.username} · ${user.email}`} onClose={onClose}
            footer={<>
                <button type="button" className={styles.btnGhost} onClick={onClose}>Cancel</button>
                <button type="button" className={styles.btnDanger} onClick={submit}>Issue ban</button>
            </>}>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>Ban type</label>
                <Seg fill value={banType} onChange={setBanType}
                    options={[{ value: BanType.Comment, label: "Comment" }, { value: BanType.Post, label: "Post" }]} />
                <span className={styles.fieldHint}>
                    {banType === BanType.Comment ? "Blocks commenting and reviews." : "Blocks publishing builds."}
                </span>
            </div>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>Duration</label>
                <div className={styles.fieldRow}>
                    <input
                        type="number" min={1} max={365}
                        className={`${styles.fieldInput} ${styles.fieldInputNum}`}
                        value={days}
                        onChange={e => setDays(Number(e.target.value))}
                    />
                    <span className={styles.fieldDays}>days</span>
                    <div className={styles.fieldGrow} />
                    <Seg value={days} onChange={setDays}
                        options={[{ value: 1, label: "1d" }, { value: 7, label: "7d" }, { value: 30, label: "30d" }, { value: 365, label: "1y" }]} />
                </div>
            </div>
            <div className={styles.field}>
                <label className={styles.fieldLabel}>Reason (shown to user)</label>
                <textarea className={styles.fieldTextarea} maxLength={500} value={reason}
                    onChange={e => setReason(e.target.value)}
                    placeholder="Explain the reason for this ban…" />
            </div>
            {err && <div className={styles.fieldError}>{err}</div>}
        </Modal>
    );
};

// ── Unban modal ──────────────────────────────────────────────────────────────
interface UnbanModalProps { user: IAdminUser; onClose: () => void; onConfirm: (p: { banType: BanTypeValue }) => void; }
const UnbanModal = ({ user, onClose, onConfirm }: UnbanModalProps) => {
    const both = user.isCommentBanned && user.isPostBanned;
    const [banType, setBanType] = useState<BanTypeValue>(user.isCommentBanned ? BanType.Comment : BanType.Post);
    return (
        <Modal title="Lift ban" sub={`@${user.username}`} onClose={onClose}
            footer={<>
                <button type="button" className={styles.btnGhost} onClick={onClose}>Cancel</button>
                <button type="button" className={styles.btnPrimary} onClick={() => onConfirm({ banType })}>Lift ban</button>
            </>}>
            {both ? (
                <div className={styles.field}>
                    <label className={styles.fieldLabel}>Which ban to lift?</label>
                    <Seg fill value={banType} onChange={setBanType}
                        options={[{ value: BanType.Comment, label: "Comment" }, { value: BanType.Post, label: "Post" }]} />
                </div>
            ) : (
                <p className={styles.modalBodyText}>
                    Lift the {user.isCommentBanned ? "comment" : "post"} ban for <strong style={{ color: "#ECECEE" }}>@{user.username}</strong>? They'll regain access immediately.
                </p>
            )}
        </Modal>
    );
};

// ── Confirm modal ─────────────────────────────────────────────────────────────
interface ConfirmModalProps { title: string; sub?: string; body: string; confirmLabel: string; danger?: boolean; onClose: () => void; onConfirm: () => void; }
const ConfirmModal = ({ title, sub, body, confirmLabel, danger, onClose, onConfirm }: ConfirmModalProps) => (
    <Modal title={title} {...(sub !== undefined && { sub })} onClose={onClose}
        footer={<>
            <button type="button" className={styles.btnGhost} onClick={onClose}>Cancel</button>
            <button type="button" className={danger ? styles.btnDanger : styles.btnPrimary} onClick={onConfirm}>{confirmLabel}</button>
        </>}>
        <p className={styles.modalBodyText}>{body}</p>
    </Modal>
);

// ── Row menu popover ──────────────────────────────────────────────────────────
interface RowMenuProps { user: IAdminUser; anchor: { top: number; right: number }; onClose: () => void; onPick: (action: string) => void; }
const RowMenu = ({ user, anchor, onClose, onPick }: RowMenuProps) => {
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
                <span className={styles.menuIcon}>◰</span> View details
            </button>
            <div className={styles.menuSep} />
            <button type="button" className={styles.menuItem} onClick={() => onPick("ban")}>
                <span className={styles.menuIcon}>⊘</span> Ban user…
            </button>
            {banned && (
                <button type="button" className={styles.menuItem} onClick={() => onPick("unban")}>
                    <span className={styles.menuIcon}>✓</span> Lift ban…
                </button>
            )}
            <button type="button" className={styles.menuItem} onClick={() => onPick("role")}>
                <span className={styles.menuIcon}>{isAdmin ? "▼" : "▲"}</span> {isAdmin ? "Demote to user" : "Promote to admin"}
            </button>
            <div className={styles.menuSep} />
            <button type="button" className={`${styles.menuItem} ${styles.menuItemDanger}`} onClick={() => onPick("delete")}>
                <span className={styles.menuIcon}>×</span> Delete account
            </button>
        </div>
    );
};

// ── User detail panel ─────────────────────────────────────────────────────────
interface UserDetailPanelProps { userId: string; onClose: () => void; }
const UserDetailPanel = ({ userId, onClose }: UserDetailPanelProps) => {
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
                        <div className={styles.panelEmpty}>Loading…</div>
                    ) : !detail ? (
                        <div className={styles.panelEmpty}>Failed to load user.</div>
                    ) : (
                        <>
                            <section>
                                <div className={styles.psecTitle}>Profile</div>
                                {([
                                    ["Username", detail.username],
                                    ["Email", detail.email],
                                    ["Verified", detail.isEmailVerified ? "Yes" : "No"],
                                    ["Roles", detail.roles.join(", ") || "—"],
                                    ["Registered", fmtDate(detail.createdAt)],
                                    ["Builds", String(detail.buildCount)],
                                    ["Reviews", String(detail.reviewCount)],
                                ] as [string, string][]).map(([k, v]) => (
                                    <div className={styles.frow} key={k}>
                                        <span className={styles.fk}>{k}</span>
                                        <span className={styles.fv}>{v}</span>
                                    </div>
                                ))}
                            </section>

                            <section>
                                <div className={styles.psecTitle}>Active bans</div>
                                {!detail.isCommentBanned && !detail.isPostBanned ? (
                                    <div className={styles.panelEmpty}>No active bans.</div>
                                ) : (
                                    <>
                                        {detail.isCommentBanned && (
                                            <div className={styles.frow}>
                                                <span className={styles.fk}>Comment ban until</span>
                                                <span className={`${styles.fv} ${styles.fvErr}`}>{fmtDate(detail.commentBanUntil ?? null)}</span>
                                            </div>
                                        )}
                                        {detail.isPostBanned && (
                                            <div className={styles.frow}>
                                                <span className={styles.fk}>Post ban until</span>
                                                <span className={`${styles.fv} ${styles.fvErr}`}>{fmtDate(detail.postBanUntil ?? null)}</span>
                                            </div>
                                        )}
                                    </>
                                )}
                            </section>

                            <section>
                                <div className={styles.psecTitle}>Warning history ({detail.warnings.length})</div>
                                {detail.warnings.length === 0 ? (
                                    <div className={styles.panelEmpty}>No warnings issued.</div>
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

    return (
        <div className={styles.page}>
            {/* Content bar */}
            <div className={styles.contentBar}>
                <span className={styles.contentMeta}>
                    {pagination?.totalCount ?? users.length} account{(pagination?.totalCount ?? users.length) === 1 ? "" : "s"}
                    {search && " matching"} · click a name for full detail
                </span>
                <form className={styles.searchForm} onSubmit={doSearch}>
                    <span className={styles.searchIcon}>⌕</span>
                    <input
                        className={styles.searchInput}
                        placeholder="Search username or email…"
                        value={searchInput}
                        onChange={e => setSearchInput(e.target.value)}
                    />
                </form>
            </div>

            {/* Table */}
            <div className={styles.tableWrap}>
                {loading ? (
                    <div className={styles.loading}>LOADING…</div>
                ) : users.length === 0 ? (
                    <div className={styles.empty}>
                        <span className={styles.emptyGlyph}>∅</span>
                        <span className={styles.emptyMsg}>No users match "{search}".</span>
                        <button type="button" className={styles.emptyLink} onClick={clearSearch}>Clear search →</button>
                    </div>
                ) : (
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th style={{ width: 44 }}></th>
                                <th>Username</th>
                                <th>Email</th>
                                <th style={{ width: 120 }}>Roles</th>
                                <th style={{ width: 120 }}>Registered</th>
                                <th style={{ width: 130 }}>Bans</th>
                                <th className={styles.thRight} style={{ width: 60 }}>Actions</th>
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
                                            {u.isCommentBanned && <span className={styles.banBadge}>Comment</span>}
                                            {u.isPostBanned && <span className={styles.banBadge}>Post</span>}
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
                    title={action.isAdmin ? "Demote to user" : "Promote to admin"}
                    sub={`@${actionUser.username}`}
                    body={action.isAdmin
                        ? `Remove admin privileges from @${actionUser.username}? They'll keep their standard user account.`
                        : `Grant @${actionUser.username} full admin access — including user management, moderation, and scraping control?`}
                    confirmLabel={action.isAdmin ? "Demote" : "Promote"}
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
                    title="Delete account"
                    sub={`@${actionUser.username}`}
                    body={`Permanently delete @${actionUser.username}? This cannot be undone.`}
                    confirmLabel="Delete permanently"
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
