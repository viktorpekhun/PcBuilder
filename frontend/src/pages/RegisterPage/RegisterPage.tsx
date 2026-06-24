import { useRef, useState, useEffect } from "react";
import { authService } from '../../api/auth.service';
import useAuth from "../../hooks/useAuth";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import styles from './RegisterPage.module.css';
import { AxiosError } from "axios";
import { decodeToken } from "../../utils/decodeToken";
import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';

const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,50}$/;
const USER_REGEX = /^[a-zA-Z][a-zA-Z0-9-_]{3,30}$/;
const PWD_REGEX = /^(?=.*[a-zA-Z])(?=.*[0-9]).{8,30}$/;

const pwdScore = (p: string) => {
    let s = 0;
    if (p.length >= 8) s++;
    if (/[a-zA-Z]/.test(p) && /[0-9]/.test(p)) s++;
    if (p.length >= 12) s++;
    if (/[^a-zA-Z0-9]/.test(p)) s++;
    return Math.min(s, 4);
};

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

type ErrType = '' | 'validate' | 'conflict' | 'server' | 'google';

const RegisterPage = () => {
    const { t } = useTranslation();
    const { setAuth, setPersist } = useAuth();
    const navigate = useNavigate();
    const emailRef = useRef<HTMLInputElement>(null);

    const [email, setEmail] = useState('');
    const [user, setUser] = useState('');
    const [pwd, setPwd] = useState('');
    const [matchPwd, setMatchPwd] = useState('');
    const [focus, setFocus] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [showMatchPassword, setShowMatchPassword] = useState(false);
    const [err, setErr] = useState<ErrType>('');

    useEffect(() => { emailRef.current?.focus(); }, []);
    useEffect(() => { setErr(''); }, [email, user, pwd, matchPwd]);

    const vEmail = EMAIL_REGEX.test(email);
    const vUser  = USER_REGEX.test(user);
    const vPwd   = PWD_REGEX.test(pwd);
    const vMatch = pwd !== '' && pwd === matchPwd;
    const allValid = vEmail && vUser && vPwd && vMatch;
    const score = pwdScore(pwd);

    const SCORE_LABEL = [
        "",
        t('auth.register.strengthLabels.weak'),
        t('auth.register.strengthLabels.fair'),
        t('auth.register.strengthLabels.good'),
        t('auth.register.strengthLabels.strong'),
    ];

    const handleGoogleLogin = async (credentialResponse: CredentialResponse) => {
        try {
            const response = await authService.googleLogin({
                idToken: credentialResponse.credential!
            });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            navigate("/");
        } catch {
            setErr('google');
        }
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!allValid) { setErr('validate'); return; }
        try {
            const response = await authService.register({ email, username: user, password: pwd });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            setEmail('');
            setPwd('');
            navigate("/");
        } catch (error) {
            const axiosErr = error as AxiosError;
            if (!axiosErr?.response) {
                setErr('server');
            } else if (axiosErr.response.status === 409) {
                setErr('conflict');
            } else {
                setErr('validate');
            }
        }
    };

    // eslint-disable-next-line react-hooks/exhaustive-deps
    useEffect(() => { setPersist(true); }, []);

    return (
        <section className={styles['register-page']}>

            <div className={styles['seg']} role="tablist">
                <Link
                    to="/login"
                    className={styles['seg-btn']}
                    role="tab" aria-selected="false"
                >
                    {t('auth.tabs.signIn')}
                </Link>
                <button
                    className={`${styles['seg-btn']} ${styles['seg-btn-active']}`}
                    role="tab" aria-selected="true"
                >
                    <span className={styles['seg-tick']}>●</span>{t('auth.register.submitBtn')}
                </button>
            </div>

            <div className={styles['register-container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>{t('auth.register.eyebrow')}</span>
                    <h1>{t('auth.register.heading')}</h1>
                    <p className={styles['description']}>
                        {t('auth.register.description')}
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {err === 'validate' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.register.errors.validateTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.register.errors.validateBody')}</div>
                            </div>
                        </div>
                    )}
                    {err === 'conflict' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.register.errors.conflictTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.register.errors.conflictBody')}</div>
                            </div>
                        </div>
                    )}
                    {err === 'server' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.register.errors.serverTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.register.errors.serverBody')}</div>
                            </div>
                        </div>
                    )}
                    {err === 'google' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>{t('auth.register.errors.googleTitle')}</div>
                                <div className={styles['errmsg-body']}>{t('auth.register.errors.googleBody')}</div>
                            </div>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        {/* E-mail */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-email">
                                {t('auth.register.emailLabel')}
                                {email && (
                                    <span className={`${styles['vmark']} ${vEmail ? styles['valid'] : styles['invalid']}`}>
                                        {vEmail ? '✓' : '×'}
                                    </span>
                                )}
                            </label>
                            <div className={styles['control']}>
                                <input
                                    type="text"
                                    id="reg-email"
                                    ref={emailRef}
                                    autoComplete="off"
                                    placeholder="you@example.com"
                                    onChange={(e) => setEmail(e.target.value)}
                                    value={email}
                                    required
                                    aria-invalid={vEmail ? "false" : "true"}
                                    onFocus={() => setFocus('email')}
                                    onBlur={() => setFocus('')}
                                />
                            </div>
                        </div>
                        {focus === 'email' && email && !vEmail && (
                            <div className={styles['instructions']}>
                                <span className={styles['instructions-icon']}>i</span>
                                <span className={styles['instructions-text']}>{t('auth.register.hints.email')}</span>
                            </div>
                        )}

                        {/* Username */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-user">
                                {t('auth.register.usernameLabel')}
                                {user && (
                                    <span className={`${styles['vmark']} ${vUser ? styles['valid'] : styles['invalid']}`}>
                                        {vUser ? '✓' : '×'}
                                    </span>
                                )}
                            </label>
                            <div className={styles['control']}>
                                <input
                                    type="text"
                                    id="reg-user"
                                    autoComplete="off"
                                    placeholder="handle"
                                    onChange={(e) => setUser(e.target.value)}
                                    value={user}
                                    required
                                    aria-invalid={vUser ? "false" : "true"}
                                    onFocus={() => setFocus('user')}
                                    onBlur={() => setFocus('')}
                                />
                            </div>
                        </div>
                        {focus === 'user' && user && !vUser && (
                            <div className={styles['instructions']}>
                                <span className={styles['instructions-icon']}>i</span>
                                <span className={styles['instructions-text']}>
                                    {t('auth.register.hints.username').split('\n').map((line, i) => (
                                        <span key={i}>{line}{i === 0 ? <br /> : null}</span>
                                    ))}
                                </span>
                            </div>
                        )}

                        {/* Password */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-pwd">
                                {t('auth.register.passwordLabel')}
                                {pwd && (
                                    <span className={`${styles['vmark']} ${vPwd ? styles['valid'] : styles['invalid']}`}>
                                        {vPwd ? '✓' : '×'}
                                    </span>
                                )}
                            </label>
                            <div className={styles['password-field']}>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="reg-pwd"
                                    placeholder="••••••••"
                                    onChange={(e) => setPwd(e.target.value)}
                                    value={pwd}
                                    required
                                    aria-invalid={vPwd ? "false" : "true"}
                                    onFocus={() => setFocus('pwd')}
                                    onBlur={() => setFocus('')}
                                />
                                <button
                                    type="button"
                                    className={styles['password-toggle']}
                                    onClick={() => setShowPassword(p => !p)}
                                    aria-label={showPassword ? t('auth.register.hidePassword') : t('auth.register.showPassword')}
                                >
                                    <EyeIcon off={showPassword} />
                                </button>
                            </div>
                        </div>
                        {pwd && (
                            <>
                                <div className={styles['meter']}>
                                    {[0,1,2,3].map(i => (
                                        <div key={i} className={`${styles['meter-bar']}${i < score ? ` ${styles['meter-bar-on']}` : ''}`} />
                                    ))}
                                </div>
                                <div className={styles['meter-label']}>
                                    {t('auth.register.passwordStrength', { label: SCORE_LABEL[score] || '—' })}
                                </div>
                            </>
                        )}

                        {/* Confirm password */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-match">
                                {t('auth.register.confirmPasswordLabel')}
                                {matchPwd && (
                                    <span className={`${styles['vmark']} ${vMatch ? styles['valid'] : styles['invalid']}`}>
                                        {vMatch ? '✓' : '×'}
                                    </span>
                                )}
                            </label>
                            <div className={styles['password-field']}>
                                <input
                                    type={showMatchPassword ? "text" : "password"}
                                    id="reg-match"
                                    placeholder="••••••••"
                                    onChange={(e) => setMatchPwd(e.target.value)}
                                    value={matchPwd}
                                    required
                                    aria-invalid={vMatch ? "false" : "true"}
                                    onFocus={() => setFocus('match')}
                                    onBlur={() => setFocus('')}
                                />
                                <button
                                    type="button"
                                    className={styles['password-toggle']}
                                    onClick={() => setShowMatchPassword(p => !p)}
                                    aria-label={showMatchPassword ? t('auth.register.hidePassword') : t('auth.register.showPassword')}
                                >
                                    <EyeIcon off={showMatchPassword} />
                                </button>
                            </div>
                        </div>
                        {focus === 'match' && matchPwd && !vMatch && (
                            <div className={styles['instructions']}>
                                <span className={styles['instructions-icon']}>i</span>
                                <span className={styles['instructions-text']}>{t('auth.register.hints.passwordMatch')}</span>
                            </div>
                        )}

                        <button
                            type="submit"
                            className={styles['btn-primary']}
                            disabled={!allValid}
                        >
                            {t('auth.register.submitBtn')} <span className={styles['btn-kbd']}>↵</span>
                        </button>
                    </form>

                    <div className={styles['divider']}><span>{t('auth.register.orDivider')}</span></div>

                    <div className={styles['google-login']}>
                        <GoogleLogin
                            onSuccess={handleGoogleLogin}
                            onError={() => setErr('google')}
                            text="signup_with"
                            theme="filled_black"
                            shape="square"
                            width={320}
                        />
                    </div>
                </div>
            </div>
        </section>
    );
};

export default RegisterPage;
