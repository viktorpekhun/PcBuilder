import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adminService } from "../../api/admin.service";
import { useTranslation } from "react-i18next";
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
    const { t } = useTranslation();
    const [action, setAction] = useState<ReportResolutionActionValue>(ReportResolutionAction.Dismiss);
    const [banType, setBanType] = useState<BanTypeValue>(BanType.Comment);
    const [days, setDays] = useState(7);
    const [reason, setReason] = useState("");
    const [reasonCode, setReasonCode] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [err, setErr] = useState<string | null>(null);

    const REASON_CODE_OPTS = [
        "SPAM",
        "INAPPROPRIATE_CONTENT",
        "OFFENSIVE_BEHAVIOUR",
        "FALSE_INFORMATION",
        "COPYRIGHT_VIOLATION",
        "COMMUNITY_GUIDELINES",
    ] as const;

    useEffect(() => {
        const h = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
        window.addEventListener("keydown", h);
        return () => window.removeEventListener("keydown", h);
    }, [onClose]);

    const reportTypeLabel = (value: ReportTypeValue) =>
        value === ReportType.Review ? t("admin.moderationPage.reportTag.review") : t("admin.moderationPage.reportTag.build");

    const needsReasonCode = action === ReportResolutionAction.DeleteContentAndWarn;
    const needsReason = action === ReportResolutionAction.DeleteContentAndBan;
    const needsBanScope = needsReasonCode || needsReason;
    const needsDuration = action === ReportResolutionAction.DeleteContentAndBan;

    const ACTION_OPTS = [
        { value: ReportResolutionAction.Dismiss,              label: t("admin.moderationPage.resolveModal.actions.dismiss"),       hint: t("admin.moderationPage.resolveModal.actions.dismissHint") },
        { value: ReportResolutionAction.DeleteContent,        label: t("admin.moderationPage.resolveModal.actions.deleteContent"), hint: t("admin.moderationPage.resolveModal.actions.deleteContentHint") },
        { value: ReportResolutionAction.DeleteContentAndWarn, label: t("admin.moderationPage.resolveModal.actions.deleteWarn"),    hint: t("admin.moderationPage.resolveModal.actions.deleteWarnHint") },
        { value: ReportResolutionAction.DeleteContentAndBan,  label: t("admin.moderationPage.resolveModal.actions.deleteBan"),     hint: t("admin.moderationPage.resolveModal.actions.deleteBanHint") },
    ];

    const banTypeOpts = [
        { value: BanType.Comment as BanTypeValue, label: t("admin.moderationPage.resolveModal.commentLabel") },
        { value: BanType.Post as BanTypeValue,    label: t("admin.moderationPage.resolveModal.postLabel") },
    ];

    const submit = async () => {
        if (needsReasonCode && !reasonCode) { setErr(t("admin.moderationPage.resolveModal.reasonCodeRequired")); return; }
        if (needsReason && !reason.trim()) { setErr(t("admin.moderationPage.resolveModal.reasonRequired")); return; }
        if (needsDuration && (days < 1 || days > 365)) { setErr(t("admin.moderationPage.resolveModal.durationInvalid")); return; }
        setSubmitting(true);
        setErr(null);
        try {
            const payload: IResolveReportRequest = { action };
            if (needsReasonCode) { payload.reasonCode = reasonCode; payload.banType = banType; }
            if (needsReason) { payload.reason = reason; payload.banType = banType; }
            if (needsDuration) payload.banDurationDays = days;
            await adminService.resolveReport(report.id, payload);
            onResolved();
        } catch (e: unknown) {
            const message = (e as { response?: { data?: { message?: string } } })?.response?.data?.message;
            setErr(message || t("admin.moderationPage.resolveModal.resolveDefaultErr"));
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={styles.scrim} onClick={onClose}>
            <div className={styles.modal} onClick={e => e.stopPropagation()}>
                <div className={styles.modalHead}>
                    <div>
                        <div className={styles.modalTitle}>{t("admin.moderationPage.resolveModal.title")}</div>
                        <div className={styles.modalSub}>{reportTypeLabel(report.reportType)} · reported by @{report.reporterUsername}</div>
                    </div>
                    <button type="button" className={styles.modalClose} onClick={onClose}>×</button>
                </div>

                <div className={styles.modalBody}>
                    <div className={styles.field}>
                        <label className={styles.fieldLabel}>{t("admin.moderationPage.resolveModal.resolution")}</label>
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

                    {needsBanScope && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>{t("admin.moderationPage.resolveModal.banScope")}</label>
                            <Seg fill value={banType} onChange={setBanType} options={banTypeOpts} />
                        </div>
                    )}

                    {needsDuration && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>{t("admin.moderationPage.resolveModal.banDuration")}</label>
                            <div className={styles.fieldRow}>
                                <input
                                    type="number" min={1} max={365}
                                    className={`${styles.fieldInput} ${styles.fieldInputNum}`}
                                    value={days}
                                    onChange={e => setDays(Number(e.target.value))}
                                />
                                <span className={styles.fieldDays}>{t("admin.moderationPage.resolveModal.days")}</span>
                                <div className={styles.fieldGrow} />
                                <Seg value={days} onChange={setDays}
                                    options={[{ value: 7, label: "7d" }, { value: 30, label: "30d" }, { value: 90, label: "90d" }]} />
                            </div>
                        </div>
                    )}

                    {needsReasonCode && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>{t("admin.moderationPage.resolveModal.reasonCodeLabel")}</label>
                            <select
                                className={styles.fieldInput}
                                value={reasonCode}
                                onChange={e => { setReasonCode(e.target.value); setErr(null); }}
                            >
                                <option value="" disabled>— select —</option>
                                {REASON_CODE_OPTS.map(code => (
                                    <option key={code} value={code}>
                                        {t(`warnReasonCodes.${code}`)}
                                    </option>
                                ))}
                            </select>
                        </div>
                    )}

                    {needsReason && (
                        <div className={styles.field}>
                            <label className={styles.fieldLabel}>{t("admin.moderationPage.resolveModal.reasonLabel")}</label>
                            <textarea className={styles.fieldTextarea} maxLength={500} value={reason}
                                onChange={e => setReason(e.target.value)}
                                placeholder={t("admin.moderationPage.resolveModal.reasonPlaceholder")} />
                        </div>
                    )}

                    {err && <div className={styles.fieldError}>{err}</div>}
                </div>

                <div className={styles.modalFoot}>
                    <button type="button" className={styles.btnGhost} onClick={onClose} disabled={submitting}>
                        {t("admin.moderationPage.resolveModal.cancel")}
                    </button>
                    <button
                        type="button"
                        className={action === ReportResolutionAction.Dismiss ? styles.btnSec : styles.btnPrimary}
                        onClick={submit}
                        disabled={submitting}
                    >
                        {submitting
                            ? t("admin.moderationPage.resolveModal.resolving")
                            : action === ReportResolutionAction.Dismiss
                                ? t("admin.moderationPage.resolveModal.dismiss")
                                : t("admin.moderationPage.resolveModal.confirm")}
                    </button>
                </div>
            </div>
        </div>
    );
};

// ── Main page ─────────────────────────────────────────────────────────────────
const AdminModerationPage = () => {
    const { t } = useTranslation();
    const [tab, setTab] = useState<ReportStatusValue>(ReportStatus.Pending);
    const [reports, setReports] = useState<IReport[]>([]);
    const [loading, setLoading] = useState(true);
    const [pageNumber, setPageNumber] = useState(1);
    const [pagination, setPagination] = useState<IPaginationHeader | null>(null);
    const [selected, setSelected] = useState<IReport | null>(null);
    const [resolvingReport, setResolvingReport] = useState<IReport | null>(null);
    const [counts, setCounts] = useState<Partial<Record<ReportStatusValue, number>>>({});

    const reportTypeLabel = (value: ReportTypeValue) =>
        value === ReportType.Review ? t("admin.moderationPage.reportTag.review") : t("admin.moderationPage.reportTag.build");

    const statusLabel = (status: ReportStatusValue) =>
        status === ReportStatus.Pending
            ? t("admin.moderationPage.status.pending")
            : status === ReportStatus.Resolved
                ? t("admin.moderationPage.status.resolved")
                : t("admin.moderationPage.status.dismissed");

    const STATUS_TABS = [
        { value: ReportStatus.Pending,   label: t("admin.moderationPage.tabs.pending") },
        { value: ReportStatus.Resolved,  label: t("admin.moderationPage.tabs.resolved") },
        { value: ReportStatus.Dismissed, label: t("admin.moderationPage.tabs.dismissed") },
    ];

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
                {STATUS_TABS.map(tabDef => {
                    const cnt = counts[tabDef.value];
                    const active = tab === tabDef.value;
                    return (
                        <button
                            key={tabDef.value}
                            type="button"
                            className={`${styles.tabBtn} ${active ? styles.tabBtnActive : ""}`}
                            onClick={() => handleTabChange(tabDef.value)}
                        >
                            {tabDef.label}
                            {cnt !== undefined && (
                                <span className={styles.tabCount}>{cnt}</span>
                            )}
                        </button>
                    );
                })}
                <span className={styles.tabMeta}>
                    {tab === ReportStatus.Pending && pagination
                        ? t("admin.moderationPage.tabMeta.awaiting", { count: pagination.totalCount })
                        : statusLabel(tab).toLowerCase()}
                </span>
            </div>

            {/* Split layout */}
            <div className={styles.split}>
                {/* Left — report list */}
                <div className={styles.listPane}>
                    {loading ? (
                        <div className={styles.loading}>{t("admin.moderationPage.loading")}</div>
                    ) : reports.length === 0 ? (
                        <div className={styles.empty}>
                            <span className={styles.emptyGlyph}>✓</span>
                            <span className={styles.emptyMsg}>{t("admin.moderationPage.empty", { status: statusLabel(tab).toLowerCase() })}</span>
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
                            <div className={styles.detailBreadcrumb}>
                                {t("admin.moderationPage.detail.breadcrumb", { num: reports.indexOf(selected) + 1 + (pageNumber - 1) * PAGE_SIZE })}
                            </div>
                            <div className={styles.detailTitle}>{selected.reportedEntityId ?? "—"}</div>

                            <div className={styles.detailMeta}>
                                <span className={styles.detailTag}>{reportTypeLabel(selected.reportType)}</span>
                                {selected.status === ReportStatus.Pending && (
                                    <span className={styles.detailSev}>
                                        <span className={styles.dot} style={{ background: "#FF5C5C" }} />
                                        {t("admin.moderationPage.detail.severity")}
                                    </span>
                                )}
                                <span className={styles.detailMetaText}>
                                    @{selected.reporterUsername} · {fmtDateTime(selected.createdAt)}
                                </span>
                            </div>

                            <div className={styles.detailSection}>
                                <div className={styles.detailSectionLabel}>{t("admin.moderationPage.detail.reason")}</div>
                                <div className={styles.detailReason}>{selected.reason}</div>
                            </div>

                            {selected.adminResolutionNote && (
                                <div className={styles.detailSection}>
                                    <div className={styles.detailSectionLabel}>{t("admin.moderationPage.detail.resolutionNote")}</div>
                                    <div className={styles.detailResNote}>{selected.adminResolutionNote}</div>
                                </div>
                            )}

                            <div className={styles.detailFoot}>
                                <Link to={`/builds/${selected.reportedEntityId}`} className={styles.btnGhost}>
                                    {t("admin.moderationPage.detail.viewContent")}
                                </Link>
                                {selected.status === ReportStatus.Pending && (
                                    <button type="button" className={styles.btnPrimary} onClick={() => setResolvingReport(selected)}>
                                        {t("admin.moderationPage.detail.resolve")}
                                    </button>
                                )}
                            </div>
                        </>
                    ) : !loading ? (
                        <div className={styles.detailEmpty}>{t("admin.moderationPage.detail.noReport")}</div>
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
