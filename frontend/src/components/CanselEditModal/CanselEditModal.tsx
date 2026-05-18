import React from 'react';
import styles from './CanselEditModal.module.css';
import { Button } from '../Button/Button';

interface CancelEditModalProps {
    isOpen: boolean;
    onCancel: () => void;
    onConfirm: () => void;
    title?: string;
    body?: string;
    warning?: string;
    cancelLabel?: string;
    confirmLabel?: string;
}

function CancelEditModal({
    isOpen, onCancel, onConfirm,
    title = "Скасувати редагування?",
    body = "Ви впевнені що хочете скасувати редагування?",
    warning = "Будь які не збережені зміни буде втрачено.",
    cancelLabel = "Продовжити Редагування",
    confirmLabel = "Вийти Без Збереження",
}: CancelEditModalProps) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>{title}</h2>
                <p>{body}</p>
                <p className={styles.warningText}>{warning}</p>

                <div className={styles.buttonGroup}>
                    <Button
                        variant='outline-secondary'
                        size='md'
                        onClick={onCancel}
                    >
                        {cancelLabel}
                    </Button>
                    <Button
                        variant='danger'
                        size='md'
                        onClick={onConfirm}
                    >
                        {confirmLabel}
                    </Button>
                </div>
            </div>
        </div>
    );
}

export default CancelEditModal;
