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

const PAGE_SIZE = 30;

const fmtDateTime = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString("en-GB", { day: "2-digit", month: "short" }) + " " +
        d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
};

const reportTypeLabel = (value: ReportTypeValue) => value === ReportType.Review ? "REVIEW" : "BUILD";

const statusLabel = (status: ReportStatusValue) =>
    status === ReportStatus.Pending ? "Pending" : status === ReportStatus.Resolved ? "Resolved" : "Dismissed";

const ACTION_OPTS: { value: ReportResolutionActionValue; label: string; hint: string }[] = [
    { value: ReportResolutionAction.Dismiss,              label: "Dismiss",             hint: "No violation — close the report, no action." },
    { value: ReportResolutionAction.DeleteContent,        label: "Delete content",       hint: "Remove the content. No warning or ban." },
    { value: ReportResolutionAction.DeleteContentAndWarn, label: "Delete + warn user",   hint: "Remove content and log a formal warning." },
    { value: ReportResolutionAction.DeleteContentAndBan,  label: "Delete + ban user",    hint: "Remove content and issue a temporary ban." },
];

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

// ── Resolve modal ─────────────────────────────────────────────────────────────
interface ResolveModalProps { report: IReport; onClose: () => void; onResolved: () => void; }
const ResolveModal = ({ report, onClose, onResolved }: ResolveModalProps) => {
    const [action, setAction] = useState<ReportResolutionActionValue>(ReportResolutionAction.Dismiss);
    const [banType, setBanType] = useState<BanTypeValue>(BanType.Comment);
    const [days, setDays] = useState(7);
    const [reason, setReason] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [err, setErr] = useState<string | null>(null);

    useEffect(() => {
        const h = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
        window.addEventListener("keydown", h);
        return () => window.removeEventListener("keydown", h);
    }, [onClose]);

    const needsReason = action === ReportResolutionAction.DeleteContentAndWarn || action === ReportResolutionAction.DeleteContentAndBan;
    const needsDuration = action === ReportResolutionAction.DeleteContentAndBan;

    const submit = async () => {
        if (needsReason && !reason.trim()) { setErr("Reason is required for this action."); return; }
        if (needsDuration && (days < 1 || days > 365)) { setErr("Duration must be 1–365 days."); return; }
        setSubmitting(true);
        setErr(null);
        try {
            const payload: IResolveReportRequest = { action };
            if (needsReason) payload.reason = reason;
            if (needsReason) payload.banType = banType;
            if (needsDuration) payload.banDurationDays = days;
            await adminService.resolveReport(report.id, payload);
            onResolved();
        } catch (e: unknown) {
            const message = (e as { response?: { data?: { message?: string } } })?.response?.data?.message;
            setErr(message || "Failed to resolve report. The content may no longer exist.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={styles.scrim} onClick={onClose}>
            <div className={styles.modal} onClick={e => e.stopPropagation()}>
                <div className={styles.modalHead}>
                    <div>
                        <div className={styles.modalTitle}>Resolve report</div>
                        <div className={styles.modalSub}>{reportTypeLabel(report.reportType)} · reported by @{report.reporterUsername}</div>
                    </div>
                    <button type="button" className={styles.modalClose} onClick={onClose}>×</button>
                </div>

                <div className={styles.modalBody}>
                    <div className={styles.field}>
                        <label className={styles.fieldLabel}>Resolution</label>
                        <div className={styles.actionOptions}>
                            {ACTION_OPTS.map(o => (
                                <button
                                    key={o.value}
                                    type="button"
                                    className={`${styles.actionOption} ${action === o.value ? styles.actionOptionActive : ""}`}
                                    onClick={() => { setAction(o.value); setErr(null); }}
                                >
                                    <span className={`${styles.actionOptionRadio} ${action === o.value ? styles.actionOptionRadioActive : ""}`}>
                                        {action === o.value && <span className={styles.actionOptionRadioDot} />}
                                    </span>
                                    <span>
                                        <span className={`${styles.actionOptionLabel} ${action === o.value ? styles.actionOptionLabelActive : ""}`}>{o.label}</span>
                                        <span className={styles.actionOptionHint}>{o.hint}</span>
                                    </span>
                                </button>
                            ))}
                        </div>
                    </div>

                    {needsReason && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>Ban scope</label>
                            <Seg fill value={banType} onChange={setBanType}
                                options={[{ value: BanType.Comment, label: "Comment" }, { value: BanType.Post, label: "Post" }]} />
                        </div>
                    )}

                    {needsDuration && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>Ban duration</label>
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
                                    options={[{ value: 7, label: "7d" }, { value: 30, label: "30d" }, { value: 90, label: "90d" }]} />
                            </div>
                        </div>
                    )}

                    {needsReason && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>Reason (shown to user)</label>
                            <textarea className={styles.fieldTextarea} maxLength={500} value={reason}
                                onChange={e => setReason(e.target.value)}
                                placeholder="Explain the decision…" />
                        </div>
                    )}

                    {err && <div className={styles.fieldError}>{err}</div>}
                </div>

                <div className={styles.modalFoot}>
                    <button type="button" className={styles.btnGhost} onClick={onClose} disabled={submitting}>Cancel</button>
                    <button
                        type="button"
                        className={action === ReportResolutionAction.Dismiss ? styles.btnSec : styles.btnPrimary}
                        onClick={submit}
                        disabled={submitting}
                    >
                        {submitting ? "Resolving…" : action === ReportResolutionAction.Dismiss ? "Dismiss report" : "Confirm action"}
                    </button>
                </div>
            </div>
        </div>
    );
};

// ── Tab bar ───────────────────────────────────────────────────────────────────
const STATUS_TABS: { value: ReportStatusValue; label: string }[] = [
    { value: ReportStatus.Pending,   label: "PENDING" },
    { value: ReportStatus.Resolved,  label: "RESOLVED" },
    { value: ReportStatus.Dismissed, label: "DISMISSED" },
];


// ── Main page ─────────────────────────────────────────────────────────────────
const AdminModerationPage = () => {
    const [tab, setTab] = useState<ReportStatusValue>(ReportStatus.Pending);
    const [reports, setReports] = useState<IReport[]>([]);
    const [loading, setLoading] = useState(true);
    const [pageNumber, setPageNumber] = useState(1);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [selected, setSelected] = useState<IReport | null>(null);
    const [resolvingReport, setResolvingReport] = useState<IReport | null>(null);
    const [counts, setCounts] = useState<Partial<Record<ReportStatusValue, number>>>({});

    const fetchReports = useCallback(async (status: ReportStatusValue, page: number) => {
        setLoading(true);
        try {
            const { items, pagination: p } = await adminService.getReports({ status, pageNumber: page, pageSize: PAGE_SIZE });
            setReports(items);
            setPagination(p);
            if (p) setCounts(prev => ({ ...prev, [status]: p.totalCount }));
            setSelected(prev => {
                if (!prev) return items[0] ?? null;
                const still = items.find(r => r.id === prev.id);
                return still ?? items[0] ?? null;
            });
        } catch {
            setReports([]);
            setPagination(null);
            setSelected(null);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { fetchReports(tab, pageNumber); }, [fetchReports, tab, pageNumber]);

    useEffect(() => {
        const statuses = [ReportStatus.Pending, ReportStatus.Resolved, ReportStatus.Dismissed] as const;
        Promise.all(statuses.map(s => adminService.getReports({ status: s, pageNumber: 1, pageSize: 1 }))).then(results => {
            setCounts(prev => {
                const next = { ...prev };
                statuses.forEach((s, i) => { const p = results[i]?.pagination; if (p) next[s] = p.totalCount; });
                return next;
            });
        }).catch(() => {});
    }, []);

    const handleTabChange = (next: ReportStatusValue) => { setTab(next); setPageNumber(1); };

    const afterResolve = async () => {
        setResolvingReport(null);
        await fetchReports(tab, pageNumber);
    };

    return (
        <div className={styles.page}>
            {/* Top tab bar */}
            <div className={styles.tabBar}>
                {STATUS_TABS.map(t => {
                    const cnt = counts[t.value];
                    const active = tab === t.value;
                    return (
                        <button
                            key={t.value}
                            type="button"
                            className={`${styles.tabBtn} ${active ? styles.tabBtnActive : ""}`}
                            onClick={() => handleTabChange(t.value)}
                        >
                            {t.label}
                            {cnt !== undefined && (
                                <span className={styles.tabCount}>{cnt}</span>
                            )}
                        </button>
                    );
                })}
                <span className={styles.tabMeta}>
                    {tab === ReportStatus.Pending && pagination
                        ? `${pagination.totalCount} awaiting review`
                        : statusLabel(tab).toLowerCase()}
                </span>
            </div>

            {/* Split layout */}
            <div className={styles.split}>
                {/* Left — report list */}
                <div className={styles.listPane}>
                    {loading ? (
                        <div className={styles.loading}>LOADING…</div>
                    ) : reports.length === 0 ? (
                        <div className={styles.empty}>
                            <span className={styles.emptyGlyph}>✓</span>
                            <span className={styles.emptyMsg}>No {statusLabel(tab).toLowerCase()} reports.</span>
                        </div>
                    ) : (
                        reports.map((report, idx) => {
                            const isSelected = selected?.id === report.id;
                            const hasDot = report.status === ReportStatus.Pending;
                            return (
                                <div
                                    key={report.id}
                                    className={`${styles.listRow} ${isSelected ? styles.listRowSelected : ""}`}
                                    onClick={() => setSelected(report)}
                                >
                                    <span className={styles.listRowDot}>
                                        {hasDot && <span className={styles.dot} style={{ background: "#FF5C5C" }} />}
                                    </span>
                                    <span className={styles.listRowTag}>{reportTypeLabel(report.reportType)}</span>
                                    <span className={styles.listRowInfo}>
                                        <span className={styles.listRowName}>{report.reportedEntityId ?? `#${idx + 1}`}</span>
                                        <span className={styles.listRowWho}>
                                            <span className={styles.listRowReporter}>@{report.reporterUsername}</span>
                                            {" → "}
                                            <span className={styles.listRowReported}>@{report.reportedUsername}</span>
                                        </span>
                                    </span>
                                    <span className={styles.listRowTime}>{fmtDateTime(report.createdAt).split(" ")[1]}</span>
                                </div>
                            );
                        })
                    )}
                </div>

                {/* Right — detail panel */}
                <div className={styles.detailPane}>
                    {selected ? (
                        <>
                            <div className={styles.detailBreadcrumb}>/ REPORT / R{reports.indexOf(selected) + 1 + (pageNumber - 1) * PAGE_SIZE}</div>
                            <div className={styles.detailTitle}>{selected.reportedEntityId ?? "—"}</div>

                            <div className={styles.detailMeta}>
                                <span className={styles.detailTag}>{reportTypeLabel(selected.reportType)}</span>
                                {selected.status === ReportStatus.Pending && (
                                    <span className={styles.detailSev}>
                                        <span className={styles.dot} style={{ background: "#FF5C5C" }} />
                                        HIGH
                                    </span>
                                )}
                                <span className={styles.detailMetaText}>
                                    @{selected.reporterUsername} · {fmtDateTime(selected.createdAt)}
                                </span>
                            </div>

                            <div className={styles.detailSection}>
                                <div className={styles.detailSectionLabel}>REASON</div>
                                <div className={styles.detailReason}>{selected.reason}</div>
                            </div>

                            {selected.adminResolutionNote && (
                                <div className={styles.detailSection}>
                                    <div className={styles.detailSectionLabel}>RESOLUTION NOTE</div>
                                    <div className={styles.detailResNote}>{selected.adminResolutionNote}</div>
                                </div>
                            )}

                            <div className={styles.detailFoot}>
                                <Link to={`/builds/${selected.reportedEntityId}`} className={styles.btnGhost}>
                                    View content →
                                </Link>
                                {selected.status === ReportStatus.Pending && (
                                    <button type="button" className={styles.btnPrimary} onClick={() => setResolvingReport(selected)}>
                                        Resolve
                                    </button>
                                )}
                            </div>
                        </>
                    ) : !loading ? (
                        <div className={styles.detailEmpty}>Select a report to review.</div>
                    ) : null}
                </div>
            </div>

            {/* Resolve modal */}
            {resolvingReport && (
                <ResolveModal
                    report={resolvingReport}
                    onClose={() => setResolvingReport(null)}
                    onResolved={afterResolve}
                />
            )}
        </div>
    );
};

export default AdminModerationPage;
