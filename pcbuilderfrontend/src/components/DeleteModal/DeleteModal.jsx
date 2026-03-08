import React from 'react';
import styles from './DeleteModal.module.css';
import { Button } from '../Button/Button';

function DeleteModal({ isOpen, buildName, onCancel, onConfirm, isDeleting }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Підтвердження Видалення</h2>
                <p>Ви впевнені що хочете видалити <strong>{buildName}</strong>?</p>
                <p className={styles.warningText}>Ця дія є незворотною.</p>

                <div className={styles.buttonGroup}>
                    <Button
                        variant='outline-secondary'
                        size='md'
                        onClick={onCancel}
                        disabled={isDeleting}
                    >
                        Відмінити
                    </Button>
                    <Button
                        variant='danger'
                        size='md'
                        onClick={onConfirm}
                        disabled={isDeleting}
                    >
                        {isDeleting ? 'Видалення...' : 'Видалити Збірку'}
                    </Button>
                </div>
            </div>
        </div>
    );
}

export default DeleteModal;
