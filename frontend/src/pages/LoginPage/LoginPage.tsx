import { useRef, useState, useEffect } from 'react';
import useAuth from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from 'react-i18next';
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
    const { t } = useTranslation();
    const { theme } = useTheme();
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

    // eslint-disable-next-line react-hooks/exhaustive-deps
    useEffect(() => { setPersist(true); }, []);

    return (
        <section className={styles['login-page']}>

            <div className={styles['seg']} role="tablist">
                <button
                    className={`${styles['seg-btn']} ${styles['seg-btn-active']}`}
                    role="tab" aria-selected="true"
                >
                    <span className={styles['seg-tick']}>●</span>{t('auth.tabs.signIn')}
                </button>
                <Link
                    to="/register"
                    className={styles['seg-btn']}
                    role="tab" aria-selected="false"
                >
                    {t('auth.tabs.register')}
                </Link>
            </div>

            <div className={styles['login-container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>{t('auth.login.eyebrow')}</span>
                    <h1>{t('auth.login.heading')}</h1>
                    <p className={styles['description']}>
                        {t('auth.login.description')}
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {err === 'empty' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.login.errors.emptyTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.login.errors.emptyBody')}</div>
                            </div>
                        </div>
                    )}
                    {(err === 'auth') && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.login.errors.authTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.login.errors.authBody')}</div>
                            </div>
                        </div>
                    )}
                    {err === 'server' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.login.errors.serverTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.login.errors.serverBody')}</div>
                            </div>
                        </div>
                    )}
                    {err === 'google' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.login.errors.googleTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.login.errors.googleBody')}</div>
                            </div>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="login-email">{t('auth.login.emailLabel')}</label>
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
                            <label className={styles['field-label']} htmlFor="login-pwd">{t('auth.login.passwordLabel')}</label>
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
                                    aria-label={showPassword ? t('auth.login.hidePassword') : t('auth.login.showPassword')}
                                >
                                    <EyeIcon off={showPassword} />
                                </button>
                            </div>
                        </div>

                        <button type="submit" className={styles['btn-primary']}>
                            {t('auth.login.submitBtn')} <span className={styles['btn-kbd']}>↵</span>
                        </button>
                        <Link to="/forgot-password" className={styles['forgot-link']}>
                            {t('auth.login.forgotPassword')}
                        </Link>
                    </form>

                    <div className={styles['divider']}><span>{t('auth.login.orDivider')}</span></div>

                    <div className={styles['google-login']}>
                        <GoogleLogin
                            onSuccess={handleGoogleLogin}
                            onError={() => setErr('google')}
                            text="signin_with"
                            theme={theme === 'light' ? 'outline' : 'filled_black'}
                            shape="square"
                            width={320}
                        />
                    </div>
                </div>
            </div>

        </section>
    );
};

export default LoginPage;
