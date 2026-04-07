import { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { authService } from '../../api/auth.service';
import { Button } from '../../components/Button/Button';
import styles from './ResetPasswordPage.module.css';

type Status = 'form' | 'loading' | 'success' | 'invalid' | 'expired' | 'no-token';

const EyeOpen = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
        <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
    </svg>
);

const EyeClosed = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
        <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"/>
        <path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"/>
        <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"/>
    </svg>
);

const ResetPasswordPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const token = searchParams.get('token');

    const [status, setStatus] = useState<Status>(token ? 'form' : 'no-token');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showNew, setShowNew] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const [errMsg, setErrMsg] = useState('');

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (newPassword !== confirmPassword) {
            setErrMsg('Паролі не збігаються');
            return;
        }
        setStatus('loading');
        setErrMsg('');
        try {
            await authService.resetPassword({ token: token!, newPassword });
            setStatus('success');
        } catch (err: any) {
            const code = err?.response?.data?.message as string | undefined;
            if (code?.includes('expired') || code?.includes('ExpiredToken')) {
                setStatus('expired');
            } else {
                setStatus('invalid');
            }
        }
    };

    if (status === 'no-token' || status === 'invalid') {
        return (
            <section className={styles['page']}>
                <div className={styles['container']}>
                    <div className={styles['icon-error']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>
                        </svg>
                    </div>
                    <h1>Недійсне посилання</h1>
                    <p className={styles['subtitle']}>Посилання для скидання пароля недійсне або вже було використано.</p>
                    <Button variant="primary" size="md" onClick={() => navigate('/forgot-password')}>
                        Надіслати новий лист
                    </Button>
                </div>
            </section>
        );
    }

    if (status === 'expired') {
        return (
            <section className={styles['page']}>
                <div className={styles['container']}>
                    <div className={styles['icon-error']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>
                        </svg>
                    </div>
                    <h1>Посилання застаріло</h1>
                    <p className={styles['subtitle']}>Термін дії посилання закінчився. Надішліть новий запит на скидання пароля.</p>
                    <Button variant="primary" size="md" onClick={() => navigate('/forgot-password')}>
                        Надіслати новий лист
                    </Button>
                </div>
            </section>
        );
    }

    if (status === 'success') {
        return (
            <section className={styles['page']}>
                <div className={styles['container']}>
                    <div className={styles['icon-success']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
                        </svg>
                    </div>
                    <h1>Пароль змінено!</h1>
                    <p className={styles['subtitle']}>Ваш пароль успішно оновлено. Тепер ви можете увійти з новим паролем.</p>
                    <Button variant="primary" size="md" onClick={() => navigate('/login')}>
                        Увійти
                    </Button>
                </div>
            </section>
        );
    }

    return (
        <section className={styles['page']}>
            <div className={styles['container']}>
                <h1>Новий пароль</h1>
                <p className={styles['subtitle']}>Придумайте новий надійний пароль для вашого акаунта.</p>
                {errMsg && (
                    <p className={styles['errmsg']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16" style={{ marginRight: '8px', flexShrink: 0 }}>
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                            <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>
                        </svg>
                        {errMsg}
                    </p>
                )}
                <form onSubmit={handleSubmit}>
                    <label htmlFor="new-password">Новий пароль:</label>
                    <div className={styles['password-field']}>
                        <input
                            type={showNew ? 'text' : 'password'}
                            id="new-password"
                            value={newPassword}
                            onChange={(e) => { setNewPassword(e.target.value); setErrMsg(''); }}
                            required
                            autoFocus
                            autoComplete="new-password"
                        />
                        <button type="button" className={styles['toggle']} onClick={() => setShowNew(p => !p)} aria-label="Показати пароль">
                            {showNew ? <EyeOpen /> : <EyeClosed />}
                        </button>
                    </div>

                    <label htmlFor="confirm-password">Підтвердіть пароль:</label>
                    <div className={styles['password-field']}>
                        <input
                            type={showConfirm ? 'text' : 'password'}
                            id="confirm-password"
                            value={confirmPassword}
                            onChange={(e) => { setConfirmPassword(e.target.value); setErrMsg(''); }}
                            required
                            autoComplete="new-password"
                        />
                        <button type="button" className={styles['toggle']} onClick={() => setShowConfirm(p => !p)} aria-label="Показати пароль">
                            {showConfirm ? <EyeOpen /> : <EyeClosed />}
                        </button>
                    </div>

                    <Button type="submit" variant="primary" size="md" disabled={status === 'loading'}>
                        {status === 'loading' ? 'Збереження...' : 'Зберегти пароль'}
                    </Button>
                </form>
            </div>
        </section>
    );
};

export default ResetPasswordPage;
