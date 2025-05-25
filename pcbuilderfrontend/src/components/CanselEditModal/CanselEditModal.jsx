import React from 'react';
import styles from './CanselEditModal.module.css'

function CancelEditModal({ isOpen, onCancel, onConfirm }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Cancel Editing?</h2>
                <p>Are you sure you want to cancel editing this build?</p>
                <p className={styles.warningText}>Any unsaved changes will be lost.</p>

                <div className={styles.buttonGroup}>
                    <button
                        className={styles.continueButton}
                        onClick={onCancel}
                    >
                        Continue Editing
                    </button>
                    <button
                        className={styles.exitButton}
                        onClick={onConfirm}
                    >
                        Exit Without Saving
                    </button>
                </div>
            </div>
        </div>
    );
}

export default CancelEditModal;