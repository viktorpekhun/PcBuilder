import React from 'react';
import styles from './CanselEditModal.module.css'
import { Button } from '../Button/Button';

function CancelEditModal({ isOpen, onCancel, onConfirm }) {
    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
<h2>Скасувати редагування?</h2>
                <p>Ви впевнені що хочете скасувати редагування?</p>
                <p className={styles.warningText}>Будь які не збережені зміни буде втрачено.</p>

                <div className={styles.buttonGroup}>
                    <Button
                        variant='outline-secondary'
                        size='md'
                        onClick={onCancel}
                    >
                        Продовжити Редагування
                    </Button>
                    <Button
                        variant='danger'
                        size='md'
                        onClick={onConfirm}
                    >
                        Вийти Без Збереження
                    </Button>
                </div>
            </div>
        </div>
    );
}

export default CancelEditModal;