import React from 'react';
import styles from './DeleteModal.module.css';

function DeleteModal({ isOpen, buildName, onCancel, onConfirm, isDeleting }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Delete Confirmation</h2>
                <p>Are you sure you want to delete <strong>{buildName}</strong>?</p>
                <p className={styles.warningText}>This action cannot be undone.</p>

                <div className={styles.buttonGroup}>
                    <button
                        className={styles.cancelButton}
                        onClick={onCancel}
                        disabled={isDeleting}
                    >
                        Cancel
                    </button>
                    <button
                        className={styles.deleteButton}
                        onClick={onConfirm}
                        disabled={isDeleting}
                    >
                        {isDeleting ? 'Deleting...' : 'Delete Build'}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default DeleteModal;