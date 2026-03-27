import { useEffect, useState } from 'react';
import styles from './SaveBuildModal.module.css';
import { Button } from '../Button/Button';

interface SaveBuildData {
    name: string;
    description: string;
}

interface SaveBuildModalProps {
    isOpen: boolean;
    onCancel: () => void;
    onSave: (data: SaveBuildData) => void;
    isSaving: boolean;
    initialName?: string;
    initialDescription?: string;
    isEditing?: boolean;
}

function SaveBuildModal({
    isOpen,
    onCancel,
    onSave,
    isSaving,
    initialName = '',
    initialDescription = '',
    isEditing = false
}: SaveBuildModalProps) {
    const [buildName, setBuildName] = useState(initialName);
    const [description, setDescription] = useState(initialDescription);
    const [error, setError] = useState('');
    const [nameFieldTouched, setNameFieldTouched] = useState(false);

    useEffect(() => {
        if (isOpen) {
            setBuildName(initialName);
            setDescription(initialDescription);
            setError('');
            setNameFieldTouched(false);
        }
    }, [isOpen, initialName, initialDescription]);

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setBuildName(value);

        // Clear error message when user types in the field
        if (value.trim()) {
            setError('');
        }
    };

    const handleNameBlur = () => {
        setNameFieldTouched(true);
        // Validate on blur
        if (!buildName.trim()) {
            setError('Введіть назву збірки');
        }
    };

    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        // Simple validation
        if (!buildName.trim()) {
            setError('Введіть назву збірки');
            setNameFieldTouched(true);
            return;
        }

        // Call the save function with the form data
        onSave({
            name: buildName.trim(),
            description: description.trim()
        });
    };

    // Determine if we should show error styling
    const showNameError = (nameFieldTouched && !buildName.trim()) || error;

    if (!isOpen) return null;

    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <h2>{isEditing ? 'Оновити Збірку' : 'Зберегти Збірку'}</h2>

                <form onSubmit={handleSubmit}>

                    {error && (
                        <div className={styles.errorMessage}>
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                viewBox="0 0 16 16" style={{ marginRight: '8px' }}>
                                <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
                                <path
                                    d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z" />
                            </svg>
                            {error}
                        </div>
                    )}
                    <div className={styles.formGroup}>
                        <label htmlFor="build-name">Назва збірки*</label>
                        <input
                            type="text"
                            id="build-name"
                            value={buildName}
                            onChange={handleNameChange}
                            onBlur={handleNameBlur}
                            placeholder="Введіть назву збірки"
                            required
                            className={showNameError ? styles.inputError : ''}
                        />
                    </div>

                    <div className={styles.formGroup}>
                        <label htmlFor="build-description">Опис (опціонально)</label>
                        <textarea
                            id="build-description"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            placeholder="Додайте опис до вашої збірки"
                            rows={4}
                        />
                    </div>


                    <div className={styles.buttonGroup}>
                        <Button
                            variant='outline-secondary'
                            size='md'
                            onClick={onCancel}
                            disabled={isSaving}
                        >
                            Відмінити
                        </Button>
                        <Button
                            variant='primary'
                            size='md'
                            disabled={isSaving}
                        >
                            {isSaving ? 'Збереження...' : isEditing ? 'Оновити' : 'Зберегти Збірку'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default SaveBuildModal;
