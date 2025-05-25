import React from 'react';
import styles from './CanselEditModal.module.css'

function CancelEditModal({ isOpen, onCancel, onConfirm }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Скасувати редагування?</h2>
                <p>Ви впевнені що хочете скасувати редагування?</p>
                <p className={styles.warningText}>Будь які не збережені зміни буде втрачено.</p>

                <div className={styles.buttonGroup}>
                    <button
                        className={styles.continueButton}
                        onClick={onCancel}
                    >
                        Продовжити Редагування
                    </button>
                    <button
                        className={styles.exitButton}
                        onClick={onConfirm}
                    >
                        Вийти Без Збереження
                    </button>
                </div>
            </div>
        </div>
    );
}

export default CancelEditModal;