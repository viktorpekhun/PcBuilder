import { useCallback, useEffect, useState } from "react";
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

const banTypeLabel = (value: BanTypeValue) =>
    value === BanType.Comment ? "Comment" : "Post";

const initials = (username: string) =>
    username
        .split(/\s+/)
        .map(s => s[0])
        .filter(Boolean)
        .slice(0, 2)
        .join("")
        .toUpperCase();

const formatDate = (iso: string | null) =>
    iso ? new Date(iso).toLocaleDateString() : "—";

const AdminUsersPage = () => {
    const [users, setUsers] = useState<IAdminUser[]>([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState("");
    const [pageNumber, setPageNumber] = useState(1);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

    const fetchUsers = useCallback(async (query: string, page: number) => {
        setLoading(true);
        try {
            const { items, pagination } = await adminService.getUsers({
                searchQuery: query,
                pageNumber: page,
                pageSize: PAGE_SIZE,
            });
            setUsers(items);
            setPagination(pagination);
        } catch {
            setUsers([]);
            setPagination(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchUsers(search, pageNumber);
    }, [fetchUsers, search, pageNumber]);

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setPageNumber(1);
    };

    const handleBan = async (user: IAdminUser) => {
        const typeStr = window.prompt("Ban type? Enter 'comment' or 'post'", "comment");
        if (!typeStr) return;
        const banType = typeStr.toLowerCase().startsWith("p") ? BanType.Post : BanType.Comment;
        const daysStr = window.prompt("Duration in days (1-365)?", "7");
        if (!daysStr) return;
        const durationDays = parseInt(daysStr, 10);
        if (!Number.isFinite(durationDays) || durationDays < 1 || durationDays > 365) {
            window.alert("Invalid duration.");
            return;
        }
        const reason = window.prompt(`Ban ${user.username} — reason?`);
        if (!reason) return;
        try {
            await adminService.banUser(user.id, { banType, durationDays, reason });
            await fetchUsers(search, pageNumber);
        } catch {
            window.alert("Failed to ban user.");
        }
    };

    const handleUnban = async (user: IAdminUser) => {
        if (!user.isCommentBanned && !user.isPostBanned) {
            window.alert("User has no active bans.");
            return;
        }
        let banType: BanTypeValue;
        if (user.isCommentBanned && user.isPostBanned) {
            const typeStr = window.prompt("Unban type? Enter 'comment' or 'post'", "comment");
            if (!typeStr) return;
            banType = typeStr.toLowerCase().startsWith("p") ? BanType.Post : BanType.Comment;
        } else {
            banType = user.isCommentBanned ? BanType.Comment : BanType.Post;
        }
        if (!window.confirm(`Lift ${banTypeLabel(banType).toLowerCase()} ban for ${user.username}?`)) return;
        try {
            await adminService.unbanUser(user.id, { banType });
            await fetchUsers(search, pageNumber);
        } catch {
            window.alert("Failed to unban user.");
        }
    };

    const handleRoleToggle = async (user: IAdminUser) => {
        const isAdmin = user.roles.includes("Admin");
        const nextRole: "Admin" | "User" = isAdmin ? "User" : "Admin";
        if (!window.confirm(`${isAdmin ? "Demote" : "Promote"} ${user.username} to ${nextRole}?`)) return;
        try {
            await adminService.changeUserRole(user.id, { role: nextRole });
            await fetchUsers(search, pageNumber);
        } catch {
            window.alert("Failed to change role.");
        }
    };

    const handleDelete = async (user: IAdminUser) => {
        if (!window.confirm(`Delete account for ${user.username}? This cannot be undone.`)) return;
        try {
            await adminService.deleteUser(user.id);
            await fetchUsers(search, pageNumber);
        } catch {
            window.alert("Failed to delete user.");
        }
    };

    return (
        <div className={styles.page}>
            <div className={styles.header}>
                <h1 className={styles.title}>Users</h1>
                <form className={styles["search-box"]} onSubmit={handleSearchSubmit}>
                    <input
                        className={styles["search-input"]}
                        placeholder="Search username or email…"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                </form>
            </div>

            <div className={styles["table-wrap"]}>
                {loading ? (
                    <div className={styles.loading}>Loading users…</div>
                ) : users.length === 0 ? (
                    <div className={styles.empty}>No users found.</div>
                ) : (
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th></th>
                                <th>Username</th>
                                <th>Email</th>
                                <th>Roles</th>
                                <th>Registered</th>
                                <th>Bans</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map(user => (
                                <tr key={user.id}>
                                    <td>
                                        {user.avatarUrl ? (
                                            <img
                                                src={user.avatarUrl}
                                                alt={user.username}
                                                className={styles.avatar}
                                            />
                                        ) : (
                                            <div className={styles["avatar-placeholder"]}>
                                                {initials(user.username)}
                                            </div>
                                        )}
                                    </td>
                                    <td>
                                        <button
                                            type="button"
                                            className={styles["username-button"]}
                                            onClick={() => setSelectedUserId(user.id)}
                                        >
                                            {user.username}
                                        </button>
                                    </td>
                                    <td>{user.email}</td>
                                    <td>
                                        {user.roles.map(role => (
                                            <span
                                                key={role}
                                                className={`${styles["role-badge"]} ${role === "Admin" ? styles["role-badge-admin"] : ""}`}
                                            >
                                                {role}
                                            </span>
                                        ))}
                                    </td>
                                    <td>{formatDate(user.createdAt)}</td>
                                    <td>
                                        {user.isCommentBanned && (
                                            <span className={styles["ban-badge"]}>Comment</span>
                                        )}
                                        {user.isPostBanned && (
                                            <span className={styles["ban-badge"]}>Post</span>
                                        )}
                                        {!user.isCommentBanned && !user.isPostBanned && (
                                            <span className={styles["ban-none"]}>—</span>
                                        )}
                                    </td>
                                    <td>
                                        <div className={styles["actions-cell"]}>
                                            <button
                                                type="button"
                                                className={styles["action-btn"]}
                                                onClick={() => handleBan(user)}
                                            >
                                                Ban
                                            </button>
                                            {(user.isCommentBanned || user.isPostBanned) && (
                                                <button
                                                    type="button"
                                                    className={styles["action-btn"]}
                                                    onClick={() => handleUnban(user)}
                                                >
                                                    Unban
                                                </button>
                                            )}
                                            <button
                                                type="button"
                                                className={styles["action-btn"]}
                                                onClick={() => handleRoleToggle(user)}
                                            >
                                                {user.roles.includes("Admin") ? "Demote" : "Promote"}
                                            </button>
                                            <button
                                                type="button"
                                                className={`${styles["action-btn"]} ${styles["action-btn-danger"]}`}
                                                onClick={() => handleDelete(user)}
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
                {pagination && users.length > 0 && (
                    <div className={styles.pagination}>
                        <span>
                            Page {pagination.pageNumber} of {pagination.totalPages} — {pagination.totalCount} total
                        </span>
                        <div className={styles["pagination-buttons"]}>
                            <button
                                type="button"
                                className={styles["page-btn"]}
                                disabled={!pagination.hasPrevious}
                                onClick={() => setPageNumber(p => Math.max(1, p - 1))}
                            >
                                Prev
                            </button>
                            <button
                                type="button"
                                className={styles["page-btn"]}
                                disabled={!pagination.hasNext}
                                onClick={() => setPageNumber(p => p + 1)}
                            >
                                Next
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {selectedUserId && (
                <UserDetailPanel
                    userId={selectedUserId}
                    onClose={() => setSelectedUserId(null)}
                />
            )}
        </div>
    );
};

interface UserDetailPanelProps {
    userId: string;
    onClose: () => void;
}

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
        <div className={styles["panel-backdrop"]} onClick={onClose}>
            <div className={styles.panel} onClick={e => e.stopPropagation()}>
                <div className={styles["panel-header"]}>
                    <h2 className={styles["panel-title"]}>User Details</h2>
                    <button type="button" className={styles["panel-close"]} onClick={onClose}>
                        ×
                    </button>
                </div>

                <div className={styles["panel-body"]}>
                    {loading ? (
                        <div className={styles.loading}>Loading…</div>
                    ) : !detail ? (
                        <div className={styles["panel-empty"]}>Failed to load user.</div>
                    ) : (
                        <>
                            <section>
                                <h3 className={styles["panel-section-title"]}>Profile</h3>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Username</span>
                                    <span className={styles["field-value"]}>{detail.username}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Email</span>
                                    <span className={styles["field-value"]}>{detail.email}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Verified</span>
                                    <span className={styles["field-value"]}>{detail.isEmailVerified ? "Yes" : "No"}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Roles</span>
                                    <span className={styles["field-value"]}>{detail.roles.join(", ") || "—"}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Registered</span>
                                    <span className={styles["field-value"]}>{formatDate(detail.createdAt)}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Builds</span>
                                    <span className={styles["field-value"]}>{detail.buildCount}</span>
                                </div>
                                <div className={styles["field-row"]}>
                                    <span className={styles["field-label"]}>Reviews</span>
                                    <span className={styles["field-value"]}>{detail.reviewCount}</span>
                                </div>
                            </section>

                            <section>
                                <h3 className={styles["panel-section-title"]}>Active Bans</h3>
                                {detail.isCommentBanned && (
                                    <div className={styles["field-row"]}>
                                        <span className={styles["field-label"]}>Comment ban until</span>
                                        <span className={styles["field-value"]}>{formatDate(detail.commentBanUntil)}</span>
                                    </div>
                                )}
                                {detail.isPostBanned && (
                                    <div className={styles["field-row"]}>
                                        <span className={styles["field-label"]}>Post ban until</span>
                                        <span className={styles["field-value"]}>{formatDate(detail.postBanUntil)}</span>
                                    </div>
                                )}
                                {!detail.isCommentBanned && !detail.isPostBanned && (
                                    <div className={styles["panel-empty"]}>No active bans.</div>
                                )}
                            </section>

                            <section>
                                <h3 className={styles["panel-section-title"]}>
                                    Warning History ({detail.warnings.length})
                                </h3>
                                {detail.warnings.length === 0 ? (
                                    <div className={styles["panel-empty"]}>No warnings issued.</div>
                                ) : (
                                    detail.warnings.map(w => (
                                        <div key={w.id} className={styles["warning-item"]}>
                                            <div className={styles["warning-head"]}>
                                                <span>{banTypeLabel(w.banType)} · {formatDate(w.issuedAt)}</span>
                                                {w.issuedByAdminUsername && (
                                                    <span>by {w.issuedByAdminUsername}</span>
                                                )}
                                            </div>
                                            <div className={styles["warning-reason"]}>{w.reason}</div>
                                        </div>
                                    ))
                                )}
                            </section>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default AdminUsersPage;
