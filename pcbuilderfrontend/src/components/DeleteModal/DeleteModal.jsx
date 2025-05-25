import React from 'react';
import styles from './DeleteModal.module.css';

function DeleteModal({ isOpen, buildName, onCancel, onConfirm, isDeleting }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Підтвердження Видалення</h2>
                <p>Ви впевнені що хочете видалити <strong>{buildName}</strong>?</p>
                <p className={styles.warningText}>Ця дія є незворотною.</p>

                <div className={styles.buttonGroup}>
                    <button
                        className={styles.cancelButton}
                        onClick={onCancel}
                        disabled={isDeleting}
                    >
                        Відмінити
                    </button>
                    <button
                        className={styles.deleteButton}
                        onClick={onConfirm}
                        disabled={isDeleting}
                    >
                        {isDeleting ? 'Видалення...' : 'Видалити Збірку'}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default DeleteModal;