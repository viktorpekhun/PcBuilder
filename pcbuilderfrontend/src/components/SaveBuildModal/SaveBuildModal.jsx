import React, { useState } from 'react';
import styles from './SaveBuildModal.module.css';

function SaveBuildModal({ isOpen, onCancel, onSave, isSaving }) {
    const [buildName, setBuildName] = useState('');
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();

        // Simple validation
        if (!buildName.trim()) {
            setError('Please enter a name for your build');
            return;
        }

        // Call the save function with the form data
        onSave({
            name: buildName.trim(),
            description: description.trim()
        });
    };

    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>Save Your Build</h2>

                <form onSubmit={handleSubmit}>
                    <div className={styles.formGroup}>
                        <label htmlFor="build-name">Build Name *</label>
                        <input
                            type="text"
                            id="build-name"
                            value={buildName}
                            onChange={(e) => setBuildName(e.target.value)}
                            placeholder="Enter a name for your build"
                            required
                        />
                    </div>

                    <div className={styles.formGroup}>
                        <label htmlFor="build-description">Description (Optional)</label>
                        <textarea
                            id="build-description"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            placeholder="Add some details about your build"
                            rows="4"
                        />
                    </div>

                    {error && <div className={styles.errorMessage}>{error}</div>}

                    <div className={styles.buttonGroup}>
                        <button
                            type="button"
                            className={styles.cancelButton}
                            onClick={onCancel}
                            disabled={isSaving}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className={styles.saveButton}
                            disabled={isSaving}
                        >
                            {isSaving ? 'Saving...' : 'Save Build'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default SaveBuildModal;