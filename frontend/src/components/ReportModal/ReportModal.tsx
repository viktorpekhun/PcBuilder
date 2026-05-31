import { useState } from "react";
import { reportService } from "../../api/report.service";
import type { ReportTargetType } from "../../types/report.types";
import styles from "./ReportModal.module.css";

interface ReportModalProps {
    isOpen: boolean;
    targetType: ReportTargetType;
    targetId: string;
    onClose: () => void;
}

const REASON_OPTIONS: Record<ReportTargetType, string[]> = {
    build: [
        "Спам або реклама",
        "Неприйнятний вміст",
        "Шахрайство або введення в оману",
        "Порушення авторських прав",
        "Дублікат збірки",
        "Інше",
    ],
    review: [
        "Спам або реклама",
        "Образлива поведінка",
        "Неприйнятний вміст",
        "Недостовірна інформація",
        "Інше",
    ],
};

const MAX_REASON = 500;

function ReportModal({ isOpen, targetType, targetId, onClose }: ReportModalProps) {
    const [selected, setSelected] = useState<string | null>(null);
    const [details, setDetails] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    if (!isOpen) return null;

    const options = REASON_OPTIONS[targetType];
    // "Інше" requires the free-text field; for everything else it's optional.
    const requiresDetails = selected === "Інше";
    const canSubmit = !!selected && (!requiresDetails || details.trim().length > 0) && !submitting;

    const buildReason = () => {
        const detail = details.trim();
        if (!selected) return "";
        // For "Інше" the detail IS the reason; otherwise append it as extra context.
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
                    ?.data?.message || "Не вдалося надіслати скаргу.";
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

    const title = targetType === "review" ? "Поскаржитись на відгук" : "Поскаржитись на збірку";

    return (
        <div className={styles.overlay} onMouseDown={handleClose}>
            <div className={styles.modal} onMouseDown={(e) => e.stopPropagation()} role="dialog" aria-modal="true">
                <div className={styles.head}>
                    <span className={styles.eyebrow}>СКАРГА · МОДЕРАЦІЯ</span>
                    <h2 className={styles.title}>{title}</h2>
                </div>

                {success ? (
                    <div className={styles.success}>
                        <span className={styles.successGlyph}>✓</span>
                        Скаргу надіслано. Дякуємо за повідомлення.
                    </div>
                ) : (
                    <form onSubmit={handleSubmit}>
                        <p className={styles.description}>
                            Оберіть причину скарги. Модератори розглянуть ваше повідомлення.
                        </p>

                        <div className={styles.options} role="radiogroup" aria-label="Причина скарги">
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
                            {requiresDetails ? "ОПИШІТЬ ПРИЧИНУ" : "ДЕТАЛІ (НЕОБОВ’ЯЗКОВО)"}
                        </label>
                        <textarea
                            className={styles.textarea}
                            value={details}
                            onChange={(e) => setDetails(e.target.value)}
                            placeholder={requiresDetails ? "Опишіть причину скарги…" : "Додаткові деталі для адміністраторів…"}
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
                                Скасувати
                            </button>
                            <button
                                type="submit"
                                className={`${styles.btn} ${styles.btnDanger}`}
                                disabled={!canSubmit}
                            >
                                {submitting ? "Надсилання…" : "Надіслати скаргу"}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
}

export default ReportModal;
