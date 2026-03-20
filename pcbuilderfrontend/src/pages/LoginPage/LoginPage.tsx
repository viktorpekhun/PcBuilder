import { useRef, useState, useEffect } from 'react';
import useAuth from '../../hooks/useAuth';
import { Link, useNavigate, useLocation } from "react-router-dom";
import styles from './LoginPage.module.css';

import { authService } from '../../api/auth.service';
import { Button } from '../../components/Button/Button';
import { AxiosError } from 'axios';
import { decodeToken } from '../../utils/decodeToken';

const LoginPage = () => {
    const { setAuth, persist, setPersist } = useAuth();

    const navigate = useNavigate();
    const location = useLocation();
    const from = (location.state as { from?: { pathname: string } })?.from?.pathname || "/";

    const emailRef = useRef<HTMLInputElement>(null);
    const errRef = useRef<HTMLParagraphElement>(null);

    const [email, setEmail] = useState('');
    const [pwd, setPwd] = useState('');
    const [errMsg, setErrMsg] = useState('');
    const [showPassword, setShowPassword] = useState(false);

    useEffect(() => {
        emailRef.current?.focus();
    }, []);

    useEffect(() => {
        setErrMsg('');
    }, [email, pwd]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        try {
            const response = await authService.login({ email, password: pwd });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            setEmail('');
            setPwd('');
            navigate(from, { replace: true });
        } catch (err) {
            const error = err as AxiosError;
            if (!error?.response) {
                setErrMsg('Сервер не відповідає');
            } else if (error.response?.status === 400) {
                setErrMsg('Будь ласка, введіть e-mail та пароль');
            } else if (error.response?.status === 401) {
                setErrMsg('Невірні автентифікаційні дані');
            } else {
                setErrMsg('Не вдалося виконати вхід в акаунт');
            }
            errRef.current?.focus();
        }
    };

    useEffect(() => {
        setPersist(true);
    }, []);

    useEffect(() => {
        localStorage.setItem("persist", JSON.stringify(persist));
    }, [persist]);

    const togglePasswordVisibility = () => {
        setShowPassword(prev => !prev);
    };

    return (
        <section className={styles['login-page']}>
            <div className={styles['login-container']}>
                <h1>Вхід</h1>
                <p className={styles['description']}>Увійдіть в акаунт щоб отримати доступ до усіх функцій</p>
                <p
                    ref={errRef}
                    className={errMsg ? styles['errmsg'] : styles['offscreen']}
                    aria-live="assertive"
                >
                    {errMsg && (
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                             viewBox="0 0 16 16" style={{marginRight: '8px'}}>
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                            <path
                                d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>
                        </svg>
                    )}
                    {errMsg}
                </p>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="email">E-mail:</label>
                    <input
                        type="text"
                        id="email"
                        ref={emailRef}
                        autoComplete="off"
                        onChange={(e) => setEmail(e.target.value)}
                        value={email}
                        required
                        className={errMsg ? styles['input-error'] : ''}
                    />

                    <label htmlFor="password">Пароль:</label>
                    <div className={styles['password-field']}>
                        <input
                            type={showPassword ? "text" : "password"}
                            id="password"
                            onChange={(e) => setPwd(e.target.value)}
                            value={pwd}
                            required
                            className={errMsg ? styles['input-error'] : ''}
                        />
                        <button
                            type="button"
                            className={styles['password-toggle']}
                            onClick={togglePasswordVisibility}
                            aria-label={showPassword ? "Сховати пароль" : "Показати пароль"}
                        >
                            {showPassword ? (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    viewBox="0 0 16 16">
                                    <path
                                        d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z" />
                                    <path
                                        d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z" />
                                </svg>
                            ) : (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                                    viewBox="0 0 16 16">
                                    <path
                                        d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z" />
                                    <path
                                        d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z" />
                                    <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z" />
                                </svg>
                            )}
                        </button>
                    </div>

                    <Button type='submit' variant='primary' size='md'>
                        Увійти
                    </Button>
                </form>
                <p className={styles['register-link']}>
                    Немає акаунту?
                    <span className={styles['line']}>
                        <Link to="/register">Зареєструватись</Link>
                    </span>
                </p>
            </div>
        </section>
    );
};

export default LoginPage;
