import { useState } from "react";
import { Button } from "../Button/Button";
import { reportService } from "../../api/report.service";
import type { ReportTargetType } from "../../types/report.types";
import styles from "./ReportModal.module.css";

interface ReportModalProps {
    isOpen: boolean;
    targetType: ReportTargetType;
    targetId: string;
    onClose: () => void;
}

function ReportModal({ isOpen, targetType, targetId, onClose }: ReportModalProps) {
    const [reason, setReason] = useState("");
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!reason.trim() || submitting) return;

        try {
            setSubmitting(true);
            setError(null);

            if (targetType === "review") {
                await reportService.reportReview(targetId, { reason: reason.trim() });
            } else {
                await reportService.reportBuild(targetId, { reason: reason.trim() });
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
        setReason("");
        setError(null);
        setSuccess(false);
        onClose();
    };

    const title = targetType === "review" ? "Поскаржитись на відгук" : "Поскаржитись на збірку";

    return (
        <div className={styles.overlay} onMouseDown={handleClose}>
            <div className={styles.modal} onMouseDown={(e) => e.stopPropagation()}>
                <h2 className={styles.title}>{title}</h2>

                {success ? (
                    <div className={styles.success}>
                        Скаргу надіслано. Дякуємо за повідомлення.
                    </div>
                ) : (
                    <form onSubmit={handleSubmit}>
                        <p className={styles.description}>
                            Опишіть причину скарги. Модератори розглянуть ваше повідомлення.
                        </p>

                        <textarea
                            className={styles.textarea}
                            value={reason}
                            onChange={(e) => setReason(e.target.value)}
                            placeholder="Причина скарги..."
                            maxLength={500}
                            rows={4}
                            autoFocus
                        />

                        <div className={styles.charCount}>{reason.length}/500</div>

                        {error && <div className={styles.error}>{error}</div>}

                        <div className={styles.buttons}>
                            <Button
                                variant="outline-secondary"
                                size="md"
                                onClick={handleClose}
                                disabled={submitting}
                            >
                                Скасувати
                            </Button>
                            <Button
                                variant="danger"
                                size="md"
                                onClick={() => {}}
                                disabled={!reason.trim() || submitting}
                            >
                                {submitting ? "Надсилання..." : "Надіслати скаргу"}
                            </Button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
}

export default ReportModal;
