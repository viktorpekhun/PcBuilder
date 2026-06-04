import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { authService } from '../../api/auth.service';
import { Button } from '../../components/Button/Button';
import useAuth from '../../hooks/useAuth';
import styles from './VerifyEmailPage.module.css';

type Status = 'loading' | 'success' | 'already-verified' | 'expired' | 'error' | 'no-token';

const VerifyEmailPage = () => {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<Status>('loading');
    const { auth, setAuth } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        const token = searchParams.get('token');

        if (!token) {
            setStatus('no-token');
            return;
        }

        authService.verifyEmail(token)
            .then(() => {
                setStatus('success');
                if (auth?.accessToken) {
                    setAuth(prev => ({ ...prev, emailVerified: true }));
                }
            })
            .catch((err) => {
                const code = err?.response?.data?.message as string | undefined;
                if (code?.includes('already been used') || code?.includes('AlreadyVerified')) {
                    setStatus('already-verified');
                } else if (code?.includes('expired') || code?.includes('ExpiredToken')) {
                    setStatus('expired');
                } else {
                    setStatus('error');
                }
            });
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const content: Record<Status, { icon: 'success' | 'info' | 'error'; title: string; text: string; action: string }> = {
        loading: { icon: 'info', title: '', text: '', action: '' },
        success: {
            icon: 'success',
            title: 'Електронну пошту підтверджено!',
            text: 'Дякуємо! Ваш акаунт успішно верифіковано.',
            action: 'На головну',
        },
        'already-verified': {
            icon: 'info',
            title: 'Пошту вже підтверджено',
            text: 'Це посилання вже було використано. Ваша адреса електронної пошти підтверджена.',
            action: 'На головну',
        },
        expired: {
            icon: 'error',
            title: 'Посилання застаріло',
            text: 'Термін дії посилання закінчився. Увійдіть в акаунт та надішліть новий лист підтвердження.',
            action: 'Увійти',
        },
        error: {
            icon: 'error',
            title: 'Помилка підтвердження',
            text: 'Недійсне посилання підтвердження.',
            action: 'На головну',
        },
        'no-token': {
            icon: 'error',
            title: 'Помилка підтвердження',
            text: 'Токен підтвердження відсутній.',
            action: 'На головну',
        },
    };

    const handleAction = () => {
        navigate(status === 'expired' ? '/login' : '/');
    };

    return (
        <section className={styles['verify-page']}>
            <div className={styles['verify-container']}>
                {status === 'loading' ? (
                    <div className={styles['loading']}>
                        <div className={styles['spinner']} />
                        <p>Підтвердження електронної пошти...</p>
                    </div>
                ) : (
                    <>
                        <div className={styles[`icon-${content[status].icon}`]}>
                            {content[status].icon === 'success' && (
                                <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
                                </svg>
                            )}
                            {content[status].icon === 'info' && (
                                <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>
                                </svg>
                            )}
                            {content[status].icon === 'error' && (
                                <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>
                                </svg>
                            )}
                        </div>
                        <h1>{content[status].title}</h1>
                        <p className={styles['subtitle']}>{content[status].text}</p>
                        <Button variant='primary' size='md' onClick={handleAction}>
                            {content[status].action}
                        </Button>
                    </>
                )}
            </div>
        </section>
    );
};

export default VerifyEmailPage;
