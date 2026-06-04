import { useRef, useState } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { authService } from '../../api/auth.service';
import styles from './ForgotPasswordPage.module.css';

const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,50}$/;
const PWD_REGEX   = /^(?=.*[a-zA-Z])(?=.*[0-9]).{8,30}$/;

const pwdScore = (p: string) => {
    let s = 0;
    if (p.length >= 8) s++;
    if (/[a-zA-Z]/.test(p) && /[0-9]/.test(p)) s++;
    if (p.length >= 12) s++;
    if (/[^a-zA-Z0-9]/.test(p)) s++;
    return Math.min(s, 4);
};
const SCORE_LABEL = ['', 'Слабкий', 'Середній', 'Добрий', 'Надійний'];

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



const PageFoot = () => (
    <div className={styles['stage-foot']}>
        <span>
            <span className={styles['stage-foot-dot']}></span>
            <span className={styles['stage-foot-ok']}>all systems ok</span>
        </span>
        <span>v 0.42 · 2026.05.31</span>
    </div>
);

/* ── FORGOT PASSWORD view ────────────────────────────────── */
const ForgotView = () => {
    const emailRef = useRef<HTMLInputElement>(null);
    const [email, setEmail]       = useState('');
    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading]   = useState(false);
    const [errMsg, setErrMsg]     = useState('');

    const valid = EMAIL_REGEX.test(email);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!valid) return;
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
        <>
            <div className={styles['container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>Акаунт / Відновлення</span>
                    <h1>Скидання пароля</h1>
                    <p className={styles['subtitle']}>
                        Введіть ваш e-mail. Ми надішлемо одноразове посилання для скидання.
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {submitted ? (
                        <div className={styles['success-banner']}>
                            <span className={styles['success-glyph']}>✓</span>
                            <span className={styles['success-body']}>
                                Надіслано. Перевірте{' '}
                                <span className={styles['success-email']}>{email}</span>{' '}
                                для отримання посилання. Якщо не отримали — перевірте папку «Спам».
                            </span>
                        </div>
                    ) : (
                        <>
                            {errMsg && (
                                <div className={styles['errmsg']} role="alert">
                                    <span className={styles['errmsg-glyph']}>×</span>
                                    <div>
                                        <div className={styles['errmsg-title']}>Помилка</div>
                                        <div className={styles['errmsg-body']}>{errMsg}</div>
                                    </div>
                                </div>
                            )}
                            <form onSubmit={handleSubmit}>
                                <div className={styles['field']}>
                                    <label className={styles['field-label']} htmlFor="forgot-email">E-mail</label>
                                    <input
                                        type="email"
                                        id="forgot-email"
                                        ref={emailRef}
                                        autoComplete="email"
                                        placeholder="you@example.com"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        required
                                        autoFocus
                                    />
                                </div>
                                <button
                                    type="submit"
                                    className={styles['btn-primary']}
                                    disabled={!valid || loading}
                                >
                                    {loading ? 'Надсилання...' : <>Надіслати посилання <span className={styles['btn-kbd']}>↵</span></>}
                                </button>
                            </form>
                        </>
                    )}
                </div>

                <div className={styles['card-foot']}>
                    <Link to="/login" className={styles['back-link']}>← Повернутися до входу</Link>
                </div>
            </div>
            <PageFoot />
        </>
    );
};

/* ── RESET PASSWORD view ─────────────────────────────────── */
type ResetStatus = 'form' | 'loading' | 'success' | 'invalid' | 'expired';

const ResetView = ({ token }: { token: string }) => {
    const navigate = useNavigate();
    const [status, setStatus]           = useState<ResetStatus>('form');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPwd, setConfirmPwd]   = useState('');
    const [showNew, setShowNew]         = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const [errMsg, setErrMsg]           = useState('');

    const vPwd   = PWD_REGEX.test(newPassword);
    const vMatch = newPassword !== '' && newPassword === confirmPwd;
    const score  = pwdScore(newPassword);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!vPwd)   { setErrMsg('Пароль не відповідає вимогам.'); return; }
        if (!vMatch) { setErrMsg('Паролі не збігаються.'); return; }
        setStatus('loading');
        setErrMsg('');
        try {
            await authService.resetPassword({ token, newPassword });
            setStatus('success');
        } catch (err) {
            const code = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
            setStatus(code?.includes('expired') || code?.includes('ExpiredToken') ? 'expired' : 'invalid');
        }
    };

    if (status === 'invalid') {
        return (
            <>
                <div className={styles['container']}>
                    <div className={styles['card-head']}>
                        <span className={styles['eyebrow']}>Акаунт / Відновлення</span>
                        <h1>Недійсне посилання</h1>
                        <p className={styles['subtitle']}>
                            Посилання недійсне або вже було використано.
                        </p>
                    </div>
                    <div className={styles['card-body']}>
                        <div className={`${styles['state-banner']} ${styles['state-banner-err']}`}>
                            <span className={styles['state-glyph']}>×</span>
                            <span className={styles['state-body']}>Це посилання більше не діє. Запросіть нове.</span>
                        </div>
                        <button className={styles['btn-primary']} onClick={() => navigate('/forgot-password')}>
                            Надіслати новий лист <span className={styles['btn-kbd']}>↵</span>
                        </button>
                    </div>
                    <div className={styles['card-foot']}>
                        <Link to="/login" className={styles['back-link']}>← Повернутися до входу</Link>
                    </div>
                </div>
                <PageFoot />
            </>
        );
    }

    if (status === 'expired') {
        return (
            <>
                <div className={styles['container']}>
                    <div className={styles['card-head']}>
                        <span className={styles['eyebrow']}>Акаунт / Відновлення</span>
                        <h1>Посилання застаріло</h1>
                        <p className={styles['subtitle']}>
                            Термін дії посилання закінчився. Надішліть новий запит.
                        </p>
                    </div>
                    <div className={styles['card-body']}>
                        <div className={`${styles['state-banner']} ${styles['state-banner-err']}`}>
                            <span className={styles['state-glyph']}>×</span>
                            <span className={styles['state-body']}>Посилання дійсне 24 години. Цей термін минув.</span>
                        </div>
                        <button className={styles['btn-primary']} onClick={() => navigate('/forgot-password')}>
                            Надіслати новий лист <span className={styles['btn-kbd']}>↵</span>
                        </button>
                    </div>
                    <div className={styles['card-foot']}>
                        <Link to="/login" className={styles['back-link']}>← Повернутися до входу</Link>
                    </div>
                </div>
                <PageFoot />
            </>
        );
    }

    if (status === 'success') {
        return (
            <>
                <div className={styles['container']}>
                    <div className={styles['card-head']}>
                        <span className={styles['eyebrow']}>Акаунт / Відновлення</span>
                        <h1>Пароль змінено</h1>
                        <p className={styles['subtitle']}>
                            Ваш пароль успішно оновлено. Тепер ви можете увійти.
                        </p>
                    </div>
                    <div className={styles['card-body']}>
                        <div className={`${styles['state-banner']} ${styles['state-banner-ok']}`}>
                            <span className={styles['state-glyph']}>✓</span>
                            <span className={styles['state-body']}>
                                Новий пароль збережено. Використовуйте його при наступному вході.
                            </span>
                        </div>
                        <button className={styles['btn-primary']} onClick={() => navigate('/login')}>
                            Увійти <span className={styles['btn-kbd']}>↵</span>
                        </button>
                    </div>
                </div>
                <PageFoot />
            </>
        );
    }

    return (
        <>
            <div className={styles['container']}>
                <div className={styles['card-head']}>
                    <span className={styles['eyebrow']}>Акаунт / Відновлення</span>
                    <h1>Новий пароль</h1>
                    <p className={styles['subtitle']}>
                        Придумайте новий надійний пароль для вашого акаунту.
                    </p>
                </div>

                <div className={styles['card-body']}>
                    {errMsg && (
                        <div className={styles['errmsg']} role="alert">
                            <span className={styles['errmsg-glyph']}>×</span>
                            <div>
                                <div className={styles['errmsg-title']}>Помилка</div>
                                <div className={styles['errmsg-body']}>{errMsg}</div>
                            </div>
                        </div>
                    )}
                    <form onSubmit={handleSubmit}>
                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="new-password">Новий пароль</label>
                            <div className={styles['password-field']}>
                                <input
                                    type={showNew ? 'text' : 'password'}
                                    id="new-password"
                                    placeholder="••••••••"
                                    value={newPassword}
                                    onChange={(e) => { setNewPassword(e.target.value); setErrMsg(''); }}
                                    required autoFocus autoComplete="new-password"
                                />
                                <button type="button" className={styles['toggle']}
                                    onClick={() => setShowNew(p => !p)}
                                    aria-label={showNew ? 'Сховати пароль' : 'Показати пароль'}>
                                    <EyeIcon off={showNew} />
                                </button>
                            </div>
                        </div>
                        {newPassword && (
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

                        <div className={styles['field']}>
                            <label className={styles['field-label']} htmlFor="confirm-password">Підтвердіть пароль</label>
                            <div className={styles['password-field']}>
                                <input
                                    type={showConfirm ? 'text' : 'password'}
                                    id="confirm-password"
                                    placeholder="••••••••"
                                    value={confirmPwd}
                                    onChange={(e) => { setConfirmPwd(e.target.value); setErrMsg(''); }}
                                    required autoComplete="new-password"
                                />
                                <button type="button" className={styles['toggle']}
                                    onClick={() => setShowConfirm(p => !p)}
                                    aria-label={showConfirm ? 'Сховати пароль' : 'Показати пароль'}>
                                    <EyeIcon off={showConfirm} />
                                </button>
                            </div>
                        </div>

                        <button type="submit" className={styles['btn-primary']} disabled={status === 'loading'}>
                            {status === 'loading'
                                ? 'Збереження...'
                                : <>Зберегти пароль <span className={styles['btn-kbd']}>↵</span></>}
                        </button>
                    </form>
                </div>

                <div className={styles['card-foot']}>
                    <Link to="/login" className={styles['back-link']}>← Повернутися до входу</Link>
                </div>
            </div>
            <PageFoot />
        </>
    );
};

/* ── router: token present → reset, absent → forgot ─────── */
const ForgotPasswordPage = () => {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');

    return (
        <section className={styles['page']}>
            {token ? <ResetView token={token} /> : <ForgotView />}
        </section>
    );
};

export default ForgotPasswordPage;
