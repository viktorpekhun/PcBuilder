import { useRef, useState, useEffect } from "react";
import { authService } from '../../api/auth.service';
import useAuth from "../../hooks/useAuth";
import { Link, useNavigate } from "react-router-dom";
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
const SCORE_LABEL = ["", "Слабкий", "Середній", "Добрий", "Надійний"];

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

    useEffect(() => { setPersist(true); }, []);

    return (
        <section className={styles['register-page']}>

            <div className={styles['seg']} role="tablist">
                <Link
                    to="/login"
                    className={styles['seg-btn']}
                    role="tab" aria-selected="false"
                >
                    Увійти
                </Link>
                <button
                    className={`${styles['seg-btn']} ${styles['seg-btn-active']}`}
                    role="tab" aria-selected="true"
                >
                    <span className={styles['seg-tick']}>●</span>Зареєструватись
                </button>
            </div>

            <div className={styles['register-container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>Акаунт / Реєстрація</span>
                    <h1>Створити акаунт</h1>
                    <p className={styles['description']}>
                        Створіть свій акаунт щоб отримати доступ до всіх функцій.
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {err === 'validate' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Помилка валідації</div>
                                <div className={styles['errmsg-body']}>Виправте виділені поля перед продовженням.</div>
                            </div>
                        </div>
                    )}
                    {err === 'conflict' && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>E-mail вже використовується</div>
                                <div className={styles['errmsg-body']}>Акаунт з таким e-mail вже існує.</div>
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
                                <div className={styles['errmsg-body']}>Не вдалося зареєструватись через Google.</div>
                            </div>
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>
                        {/* E-mail */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-email">
                                E-mail
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
                                <span className={styles['instructions-text']}>Введіть дійсну адресу електронної пошти.</span>
                            </div>
                        )}

                        {/* Username */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-user">
                                Ім'я користувача
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
                                    4–30 символів. Має починатися з літери.<br />
                                    Літери, цифри, підкреслення та дефіси.
                                </span>
                            </div>
                        )}

                        {/* Password */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-pwd">
                                Пароль
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
                                    aria-label={showPassword ? "Сховати пароль" : "Показати пароль"}
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
                                    Надійність: {SCORE_LABEL[score] || '—'} · 8–30 символів, літери + цифри
                                </div>
                            </>
                        )}

                        {/* Confirm password */}
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="reg-match">
                                Підтвердіть пароль
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
                                    aria-label={showMatchPassword ? "Сховати пароль" : "Показати пароль"}
                                >
                                    <EyeIcon off={showMatchPassword} />
                                </button>
                            </div>
                        </div>
                        {focus === 'match' && matchPwd && !vMatch && (
                            <div className={styles['instructions']}>
                                <span className={styles['instructions-icon']}>i</span>
                                <span className={styles['instructions-text']}>Має збігатися з полем пароля.</span>
                            </div>
                        )}

                        <button
                            type="submit"
                            className={styles['btn-primary']}
                            disabled={!allValid}
                        >
                            Зареєструватись <span className={styles['btn-kbd']}>↵</span>
                        </button>
                    </form>

                    <div className={styles['divider']}><span>або</span></div>

                    <div className={styles['google-login']}>
                        <div className={styles['google-btn-wrap']}>
                            <div className={styles['google-btn-face']} aria-hidden="true">
                                <span className={styles['google-mark']}>G</span>
                                Зареєструватись через Google
                            </div>
                            <div className={styles['google-btn-real']}>
                                <GoogleLogin
                                    onSuccess={handleGoogleLogin}
                                    onError={() => setErr('google')}
                                    text="signup_with"
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

export default RegisterPage;
