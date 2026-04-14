import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adminService } from "../../api/admin.service";
import {
    BanType,
    type BanTypeValue,
    type IPaginationHeader,
    type IReport,
    type IResolveReportRequest,
    ReportResolutionAction,
    type ReportResolutionActionValue,
    ReportStatus,
    type ReportStatusValue,
    ReportType,
    type ReportTypeValue,
} from "../../types/admin.types";
import styles from "./AdminModerationPage.module.css";

const PAGE_SIZE = 20;

const formatDate = (iso: string) => new Date(iso).toLocaleString();

const reportTypeLabel = (value: ReportTypeValue) =>
    value === ReportType.Review ? "Review" : "Build";

const reportTypeClass = (value: ReportTypeValue) =>
    value === ReportType.Review ? "badge-review" : "badge-build";

const contentLinkFor = (report: IReport): string =>
    report.reportType === ReportType.Build
        ? `/builds/${report.reportedEntityId}`
        : `/builds/${report.reportedEntityId}`;

const AdminModerationPage = () => {
    const [tab, setTab] = useState<ReportStatusValue>(ReportStatus.Pending);
    const [reports, setReports] = useState<IReport[]>([]);
    const [loading, setLoading] = useState(true);
    const [pageNumber, setPageNumber] = useState(1);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [resolvingReport, setResolvingReport] = useState<IReport | null>(null);

    const fetchReports = useCallback(async (status: ReportStatusValue, page: number) => {
        setLoading(true);
        try {
            const { items, pagination } = await adminService.getReports({
                status,
                pageNumber: page,
                pageSize: PAGE_SIZE,
            });
            setReports(items);
            setPagination(pagination);
        } catch {
            setReports([]);
            setPagination(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchReports(tab, pageNumber);
    }, [fetchReports, tab, pageNumber]);

    const handleTabChange = (next: ReportStatusValue) => {
        setTab(next);
        setPageNumber(1);
    };

    const afterResolve = async () => {
        setResolvingReport(null);
        await fetchReports(tab, pageNumber);
    };

    const statusBadgeClass = (status: ReportStatusValue) => {
        if (status === ReportStatus.Pending) return "badge-pending";
        if (status === ReportStatus.Resolved) return "badge-resolved";
        return "badge-dismissed";
    };

    const statusLabel = (status: ReportStatusValue) => {
        if (status === ReportStatus.Pending) return "Pending";
        if (status === ReportStatus.Resolved) return "Resolved";
        return "Dismissed";
    };

    return (
        <div className={styles.page}>
            <h1 className={styles.title}>Moderation</h1>

            <div className={styles.tabs}>
                <button
                    type="button"
                    className={`${styles.tab} ${tab === ReportStatus.Pending ? styles["tab-active"] : ""}`}
                    onClick={() => handleTabChange(ReportStatus.Pending)}
                >
                    Pending Reports
                </button>
                <button
                    type="button"
                    className={`${styles.tab} ${tab === ReportStatus.Resolved ? styles["tab-active"] : ""}`}
                    onClick={() => handleTabChange(ReportStatus.Resolved)}
                >
                    Resolved
                </button>
                <button
                    type="button"
                    className={`${styles.tab} ${tab === ReportStatus.Dismissed ? styles["tab-active"] : ""}`}
                    onClick={() => handleTabChange(ReportStatus.Dismissed)}
                >
                    Dismissed
                </button>
            </div>

            {loading ? (
                <div className={styles.empty}>Loading reports…</div>
            ) : reports.length === 0 ? (
                <div className={styles.empty}>No reports in this tab.</div>
            ) : (
                <div className={styles.list}>
                    {reports.map(report => (
                        <article key={report.id} className={styles.card}>
                            <div className={styles["card-header"]}>
                                <div className={styles["card-meta"]}>
                                    <h3 className={styles["card-title"]}>
                                        {report.reporterUsername} reported {report.reportedUsername}
                                    </h3>
                                    <span>{formatDate(report.createdAt)}</span>
                                </div>
                                <div style={{ display: "flex", gap: "6px" }}>
                                    <span className={`${styles.badge} ${styles[reportTypeClass(report.reportType)]}`}>
                                        {reportTypeLabel(report.reportType)}
                                    </span>
                                    <span className={`${styles.badge} ${styles[statusBadgeClass(report.status)]}`}>
                                        {statusLabel(report.status)}
                                    </span>
                                </div>
                            </div>

                            <div className={styles["reason-block"]}>{report.reason}</div>

                            {report.adminResolutionNote && (
                                <div className={styles["resolution-note"]}>
                                    Admin note: {report.adminResolutionNote}
                                </div>
                            )}

                            <div className={styles["card-footer"]}>
                                <Link to={contentLinkFor(report)} className={styles["action-btn"]}>
                                    View content →
                                </Link>
                                {report.status === ReportStatus.Pending && (
                                    <div className={styles.actions}>
                                        <button
                                            type="button"
                                            className={styles["action-btn-primary"] + " " + styles["action-btn"]}
                                            onClick={() => setResolvingReport(report)}
                                        >
                                            Resolve
                                        </button>
                                    </div>
                                )}
                            </div>
                        </article>
                    ))}
                </div>
            )}

            {pagination && reports.length > 0 && (
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

            {resolvingReport && (
                <ResolveReportModal
                    report={resolvingReport}
                    onClose={() => setResolvingReport(null)}
                    onResolved={afterResolve}
                />
            )}
        </div>
    );
};

interface ResolveReportModalProps {
    report: IReport;
    onClose: () => void;
    onResolved: () => void;
}

const ResolveReportModal = ({ report, onClose, onResolved }: ResolveReportModalProps) => {
    const [action, setAction] = useState<ReportResolutionActionValue>(ReportResolutionAction.Dismiss);
    const [reason, setReason] = useState("");
    const [banType, setBanType] = useState<BanTypeValue>(BanType.Comment);
    const [banDurationDays, setBanDurationDays] = useState(7);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const needsReason =
        action === ReportResolutionAction.DeleteContentAndWarn ||
        action === ReportResolutionAction.DeleteContentAndBan;

    const needsBanType = needsReason;
    const needsDuration = action === ReportResolutionAction.DeleteContentAndBan;

    const handleSubmit = async () => {
        if (needsReason && reason.trim().length === 0) {
            setError("Reason is required for this action.");
            return;
        }
        if (needsDuration && (banDurationDays < 1 || banDurationDays > 365)) {
            setError("Duration must be between 1 and 365 days.");
            return;
        }

        setSubmitting(true);
        setError(null);
        try {
            const payload: IResolveReportRequest = { action };
            if (needsReason) payload.reason = reason;
            if (needsBanType) payload.banType = banType;
            if (needsDuration) payload.banDurationDays = banDurationDays;

            await adminService.resolveReport(report.id, payload);
            onResolved();
        } catch (e: unknown) {
            const message = (e as { response?: { data?: { message?: string } } })
                ?.response?.data?.message;
            setError(message || "Failed to resolve report. The content may no longer exist.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={styles["modal-backdrop"]} onClick={onClose}>
            <div className={styles.modal} onClick={e => e.stopPropagation()}>
                <div className={styles["modal-header"]}>
                    <h2 className={styles["modal-title"]}>Resolve Report</h2>
                    <p className={styles["modal-subtitle"]}>
                        {reportTypeLabel(report.reportType)} by {report.reportedUsername}
                    </p>
                </div>

                <div className={styles["modal-body"]}>
                    <div className={styles.field}>
                        <label className={styles["field-label"]}>Action</label>
                        <select
                            className={styles.select}
                            value={action}
                            onChange={e => setAction(Number(e.target.value) as ReportResolutionActionValue)}
                        >
                            <option value={ReportResolutionAction.Dismiss}>Dismiss (no action)</option>
                            <option value={ReportResolutionAction.DeleteContent}>Delete content only</option>
                            <option value={ReportResolutionAction.DeleteContentAndWarn}>
                                Delete content + warn user
                            </option>
                            <option value={ReportResolutionAction.DeleteContentAndBan}>
                                Delete content + ban user
                            </option>
                        </select>
                    </div>

                    {needsBanType && (
                        <div className={styles.field}>
                            <label className={styles["field-label"]}>Ban type</label>
                            <select
                                className={styles.select}
                                value={banType}
                                onChange={e => setBanType(Number(e.target.value) as BanTypeValue)}
                            >
                                <option value={BanType.Comment}>Comment</option>
                                <option value={BanType.Post}>Post</option>
                            </select>
                        </div>
                    )}

                    {needsDuration && (
                        <div className={styles.field}>
                            <label className={styles["field-label"]}>Duration (days)</label>
                            <input
                                type="number"
                                min={1}
                                max={365}
                                className={styles.input}
                                value={banDurationDays}
                                onChange={e => setBanDurationDays(Number(e.target.value))}
                            />
                        </div>
                    )}

                    {needsReason && (
                        <div className={styles.field}>
                            <label className={styles["field-label"]}>Reason (shown to user)</label>
                            <textarea
                                className={styles.textarea}
                                maxLength={500}
                                value={reason}
                                onChange={e => setReason(e.target.value)}
                                placeholder="Explain the reason…"
                            />
                        </div>
                    )}

                    {error && <div className={styles["stale-notice"]}>{error}</div>}
                </div>

                <div className={styles["modal-footer"]}>
                    <button
                        type="button"
                        className={styles["action-btn"]}
                        onClick={onClose}
                        disabled={submitting}
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        className={`${styles["action-btn"]} ${styles["action-btn-primary"]}`}
                        onClick={handleSubmit}
                        disabled={submitting}
                    >
                        {submitting ? "Resolving…" : "Confirm"}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default AdminModerationPage;
