import { useState } from "react";
import { useTranslation } from "react-i18next";
import { reportService } from "../../api/report.service";
import type { ReportTargetType } from "../../types/report.types";
import styles from "./ReportModal.module.css";

interface ReportModalProps {
    isOpen: boolean;
    targetType: ReportTargetType;
    targetId: string;
    onClose: () => void;
}

const MAX_REASON = 500;

function ReportModal({ isOpen, targetType, targetId, onClose }: ReportModalProps) {
    const { t } = useTranslation();
    const [selected, setSelected] = useState<string | null>(null);
    const [details, setDetails] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    if (!isOpen) return null;

    const other = t("report.other");
    const options: string[] = t(
        targetType === "review" ? "report.reasonsReview" : "report.reasonsBuild",
        { returnObjects: true }
    ) as string[];

    const requiresDetails = selected === other;
    const canSubmit = !!selected && (!requiresDetails || details.trim().length > 0) && !submitting;

    const buildReason = () => {
        const detail = details.trim();
        if (!selected) return "";
        const composed = requiresDetails
            ? detail
            : detail
                ? `${selected} — ${detail}`
                : selected;
        return composed.slice(0, MAX_REASON);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const reason = buildReason();
        if (!reason || submitting) return;

        try {
            setSubmitting(true);
            setError(null);

            if (targetType === "review") {
                await reportService.reportReview(targetId, { reason });
            } else {
                await reportService.reportBuild(targetId, { reason });
            }

            setSuccess(true);
            setTimeout(() => {
                handleClose();
            }, 1500);
        } catch (err) {
            const msg =
                (err as { response?: { data?: { message?: string } } })?.response
                    ?.data?.message || t("report.submitFailed");
            setError(msg);
        } finally {
            setSubmitting(false);
        }
    };

    const handleClose = () => {
        setSelected(null);
        setDetails("");
        setError(null);
        setSuccess(false);
        onClose();
    };

    const title = targetType === "review" ? t("report.titleReview") : t("report.titleBuild");

    return (
        <div className={styles.overlay} onMouseDown={handleClose}>
            <div className={styles.modal} onMouseDown={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
                <div className={styles.head}>
                    <span className={styles.eyebrow}>{t("report.eyebrow")}</span>
                    <h2 className={styles.title}>{title}</h2>
                </div>

                {success ? (
                    <div className={styles.success}>
                        <span className={styles.successGlyph}>{t("report.successGlyph")}</span>
                        {t("report.successMessage")}
                    </div>
                ) : (
                    <form onSubmit={handleSubmit}>
                        <p className={styles.description}>
                            {t("report.description")}
                        </p>

                        <div className={styles.options} role="radiogroup" aria-label={t("report.reasonsGroupLabel")}>
                            {options.map((opt) => {
                                const active = selected === opt;
                                return (
                                    <button
                                        type="button"
                                        key={opt}
                                        className={`${styles.option} ${active ? styles.optionOn : ""}`}
                                        onClick={() => setSelected(opt)}
                                        role="radio"
                                        aria-checked={active}
                                    >
                                        <span className={styles.radio} aria-hidden="true" />
                                        <span className={styles.optionLabel}>{opt}</span>
                                    </button>
                                );
                            })}
                        </div>

                        <label className={styles.fieldLabel}>
                            {requiresDetails ? t("report.fieldLabelRequired") : t("report.fieldLabelOptional")}
                        </label>
                        <textarea
                            className={styles.textarea}
                            value={details}
                            onChange={(e) => setDetails(e.target.value)}
                            placeholder={requiresDetails ? t("report.placeholderRequired") : t("report.placeholderOptional")}
                            maxLength={MAX_REASON}
                            rows={3}
                        />
                        <div className={styles.charCount}>{details.length}/{MAX_REASON}</div>

                        {error && <div className={styles.error}>{error}</div>}

                        <div className={styles.buttons}>
                            <button
                                type="button"
                                className={`${styles.btn} ${styles.btnGhost}`}
                                onClick={handleClose}
                                disabled={submitting}
                            >
                                {t("report.cancel")}
                            </button>
                            <button
                                type="submit"
                                className={`${styles.btn} ${styles.btnDanger}`}
                                disabled={!canSubmit}
                            >
                                {submitting ? t("report.submitting") : t("report.submit")}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
}

export default ReportModal;
