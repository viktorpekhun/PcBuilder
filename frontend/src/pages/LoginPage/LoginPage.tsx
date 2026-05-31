import { useRef, useState, useEffect } from 'react';
import useAuth from '../../hooks/useAuth';
import { Link, useNavigate, useLocation } from "react-router-dom";
import styles from './LoginPage.module.css';

import { authService } from '../../api/auth.service';
import { AxiosError } from 'axios';
import { decodeToken } from '../../utils/decodeToken';
import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';

const EyeIcon = ({ off }: { off: boolean }) => (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="1.6" strokeLinecap="square" strokeLinejoin="miter">
        {off ? (
            <>
                <path d="M10.7 5.1A9.9 9.9 0 0 1 12 5c5.5 0 9 7 9 7a14.8 14.8 0 0 1-2.6 3.3" />
                <path d="M6.6 6.6A14.6 14.6 0 0 0 3 12s3.5 7 9 7a9.6 9.6 0 0 0 4.5-1.1" />
                <path d="M9.9 9.9a3 3 0 0 0 4.2 4.2" />
                <line x1="3" y1="3" x2="21" y2="21" />
            </>
        ) : (
            <>
                <path d="M3 12s3.5-7 9-7 9 7 9 7-3.5 7-9 7-9-7-9-7Z" />
                <circle cx="12" cy="12" r="2.6" />
            </>
        )}
    </svg>
);

type ErrType = '' | 'empty' | 'server' | 'auth' | 'google';

const LoginPage = () => {
    const { setAuth, setPersist } = useAuth();

    const navigate = useNavigate();
    const location = useLocation();
    const from = (location.state as { from?: { pathname: string } })?.from?.pathname || "/";

    const emailRef = useRef<HTMLInputElement>(null);

    const [email, setEmail] = useState('');
    const [pwd, setPwd] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [err, setErr] = useState<ErrType>('');

    useEffect(() => { emailRef.current?.focus(); }, []);
    useEffect(() => { setErr(''); }, [email, pwd]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!email || !pwd) { setErr('empty'); return; }

        try {
            const response = await authService.login({ email, password: pwd });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            setEmail('');
            setPwd('');
            navigate(from, { replace: true });
        } catch (error) {
            const axiosErr = error as AxiosError;
            if (!axiosErr?.response) {
                setErr('server');
            } else if (axiosErr.response?.status === 400) {
                setErr('empty');
            } else if (axiosErr.response?.status === 401) {
                setErr('auth');
            } else {
                setErr('auth');
            }
        }
    };

    const handleGoogleLogin = async (credentialResponse: CredentialResponse) => {
        try {
            const response = await authService.googleLogin({
                idToken: credentialResponse.credential!
            });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            navigate(from, { replace: true });
        } catch {
            setErr('google');
        }
    };

    useEffect(() => { setPersist(true); }, []);

    return (
        <section className={styles['login-page']}>

            <div className={styles['seg']} role="tablist">
                <button
                    className={`${styles['seg-btn']} ${styles['seg-btn-active']}`}
                    role="tab" aria-selected="true"
                >
                    <span className={styles['seg-tick']}>●</span>Увійти
                </button>
                <Link
                    to="/register"
                    className={styles['seg-btn']}
                    role="tab" aria-selected="false"
                >
                    Зареєструватись
                </Link>
            </div>

            <div className={styles['login-container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>Акаунт / Доступ</span>
                    <h1>Вхід</h1>
                    <p className={styles['description']}>
                        Увійдіть в свій акаунт щоб отримати доступ до всіх функцій.
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {err === 'empty' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Порожні поля</div>
                                <div className={styles['errmsg-body']}>Введіть e-mail та пароль щоб продовжити.</div>
                            </div>
                        </div>
                    )}
                    {(err === 'auth') && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Невірні дані</div>
                                <div className={styles['errmsg-body']}>E-mail або пароль неправильні. Спробуйте ще раз.</div>
                            </div>
                        </div>
                    )}
                    {err === 'server' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Сервер не відповідає</div>
                                <div className={styles['errmsg-body']}>Спробуйте пізніше.</div>
                            </div>
                        </div>
                    )}
                    {err === 'google' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Помилка Google</div>
                                <div className={styles['errmsg-body']}>Не вдалося увійти через Google. Спробуйте ще раз.</div>
                            </div>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="login-email">E-mail</label>
                            <div className={styles['control']}>
                                <input
                                    type="text"
                                    id="login-email"
                                    ref={emailRef}
                                    autoComplete="off"
                                    placeholder="you@example.com"
                                    onChange={(e) => setEmail(e.target.value)}
                                    value={email}
                                    required
                                />
                            </div>
                        </div>

                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="login-pwd">Пароль</label>
                            <div className={styles['password-field']}>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="login-pwd"
                                    placeholder="••••••••"
                                    onChange={(e) => setPwd(e.target.value)}
                                    value={pwd}
                                    required
                                />
                                <button
                                    type="button"
                                    className={styles['password-toggle']}
                                    onClick={() => setShowPassword(p => !p)}
                                    aria-label={showPassword ? "Сховати пароль" : "Показати пароль"}
                                >
                                    <EyeIcon off={showPassword} />
                                </button>
                            </div>
                        </div>

                        <button type="submit" className={styles['btn-primary']}>
                            Увійти <span className={styles['btn-kbd']}>↵</span>
                        </button>
                        <Link to="/forgot-password" className={styles['forgot-link']}>
                            Забули пароль? →
                        </Link>
                    </form>

                    <div className={styles['divider']}><span>або</span></div>

                    <div className={styles['google-login']}>
                        <div className={styles['google-btn-wrap']}>
                            <div className={styles['google-btn-face']} aria-hidden="true">
                                <span className={styles['google-mark']}>G</span>
                                Увійти через Google
                            </div>
                            <div className={styles['google-btn-real']}>
                                <GoogleLogin
                                    onSuccess={handleGoogleLogin}
                                    onError={() => setErr('google')}
                                    text="signin_with"
                                    width="100%"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        </section>
    );
};

export default LoginPage;
