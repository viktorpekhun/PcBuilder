import { useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { authService } from '../../api/auth.service';
import { Button } from '../../components/Button/Button';
import styles from './ForgotPasswordPage.module.css';

const ForgotPasswordPage = () => {
    const emailRef = useRef<HTMLInputElement>(null);
    const [email, setEmail] = useState('');
    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [errMsg, setErrMsg] = useState('');

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setLoading(true);
        setErrMsg('');
        try {
            await authService.forgotPassword({ email });
            setSubmitted(true);
        } catch {
            setErrMsg('Не вдалося надіслати лист. Спробуйте ще раз.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <section className={styles['page']}>
            <div className={styles['container']}>
                {submitted ? (
                    <>
                        <div className={styles['icon-success']}>
                            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                                <path d="M2 2a2 2 0 0 0-2 2v8.01A2 2 0 0 0 2 14h5.5a.5.5 0 0 0 0-1H2a1 1 0 0 1-.966-.741l5.64-3.471L8 9.583l7-4.2V8.5a.5.5 0 0 0 1 0V4a2 2 0 0 0-2-2zm3.708 6.208L1 11.105V5.383zM1 4.217V4a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v.217l-7 4.2z"/>
                                <path d="M16 12.5a3.5 3.5 0 1 1-7 0 3.5 3.5 0 0 1 7 0m-1.993-1.679a.5.5 0 0 0-.686.172l-1.17 1.95-.547-.547a.5.5 0 0 0-.708.708l.774.773a.75.75 0 0 0 1.174-.144l1.335-2.226a.5.5 0 0 0-.172-.686"/>
                            </svg>
                        </div>
                        <h1>Перевірте пошту</h1>
                        <p className={styles['subtitle']}>
                            Якщо акаунт з адресою <strong>{email}</strong> існує, ми надіслали лист із посиланням для скидання пароля.
                        </p>
                        <p className={styles['hint']}>Не отримали? Перевірте папку «Спам».</p>
                        <Link to="/login" className={styles['back-link']}>Повернутися до входу</Link>
                    </>
                ) : (
                    <>
                        <h1>Скидання пароля</h1>
                        <p className={styles['subtitle']}>
                            Введіть вашу адресу електронної пошти. Ми надішлемо посилання для скидання пароля.
                        </p>
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
                            <label htmlFor="email">E-mail:</label>
                            <input
                                type="email"
                                id="email"
                                ref={emailRef}
                                autoComplete="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                                autoFocus
                            />
                            <Button type="submit" variant="primary" size="md" disabled={loading}>
                                {loading ? 'Надсилання...' : 'Надіслати посилання'}
                            </Button>
                        </form>
                        <Link to="/login" className={styles['back-link']}>Повернутися до входу</Link>
                    </>
                )}
            </div>
        </section>
    );
};

export default ForgotPasswordPage;
