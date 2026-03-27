import { useRef, useState, useEffect } from "react";
import { faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { authService } from '../../api/auth.service';
import useAuth from "../../hooks/useAuth";
import { Link, useNavigate } from "react-router-dom";
import styles from './RegisterPage.module.css';
import { Button } from "../../components/Button/Button";
import { AxiosError } from "axios";
import { decodeToken } from "../../utils/decodeToken";

const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,50}$/;
const USER_REGEX = /^[a-zA-Z][a-zA-Z0-9-_]{3,30}$/;
const PWD_REGEX = /^(?=.*[a-zA-Z])(?=.*[0-9]).{8,30}$/;

const RegisterPage = () => {
    const { setAuth, persist, setPersist } = useAuth();
    const navigate = useNavigate();
    const emailRef = useRef<HTMLInputElement>(null);
    const errRef = useRef<HTMLParagraphElement>(null);

    const [email, setEmail] = useState('');
    const [validEmail, setValidEmail] = useState(false);
    const [emailFocus, setEmailFocus] = useState(false);

    const [user, setUser] = useState('');
    const [validName, setValidName] = useState(false);
    const [userFocus, setUserFocus] = useState(false);

    const [pwd, setPwd] = useState('');
    const [validPwd, setValidPwd] = useState(false);
    const [pwdFocus, setPwdFocus] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const [matchPwd, setMatchPwd] = useState('');
    const [validMatch, setValidMatch] = useState(false);
    const [matchFocus, setMatchFocus] = useState(false);
    const [showMatchPassword, setShowMatchPassword] = useState(false);

    const [errMsg, setErrMsg] = useState('');

    useEffect(() => {
        emailRef.current?.focus();
    }, [])

    useEffect(() => {
        setValidEmail(EMAIL_REGEX.test(email));
    }, [email]);

    useEffect(() => {
        setValidName(USER_REGEX.test(user));
    }, [user]);

    useEffect(() => {
        setValidPwd(PWD_REGEX.test(pwd));
        setValidMatch(pwd === matchPwd);
    }, [pwd, matchPwd]);

    useEffect(() => {
        setErrMsg('');
    }, [email, user, pwd, matchPwd]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!EMAIL_REGEX.test(email) || !USER_REGEX.test(user) || !PWD_REGEX.test(pwd)) {
            setErrMsg("Помилка валідації даних");
            return;
        }
        try {
            const response = await authService.register({ email, username: user, password: pwd });
            const accessToken = response.data.accessToken;
            const userData = decodeToken(accessToken);
            setAuth({ ...userData, accessToken });
            setEmail('');
            setPwd('');
            navigate("/");
        } catch (err) {
            const error = err as AxiosError;
            if (!error?.response) {
                setErrMsg('Сервер не відповідає');
            } else if (error.response.status === 409) {
                setErrMsg("Користувач з таким e-mail вже зареєстрований");
            } else {
                setErrMsg('Помилка реєстрації');
            }
            errRef.current?.focus();
        }
    }

    useEffect(() => {
        setPersist(true);
    }, []);

    useEffect(() => {
        localStorage.setItem("persist", JSON.stringify(persist));
    }, [persist]);

    const togglePasswordVisibility = () => {
        setShowPassword(prev => !prev);
    };

    const toggleMatchPasswordVisibility = () => {
        setShowMatchPassword(prev => !prev);
    };

    return (
        <section className={styles['register-page']}>
            <div className={styles['register-container']}>
                <h1>Реєстрація</h1>
                <p className={styles['description']}>Створіть обліковий запис, щоб отримати доступ до усіх функцій</p>

                <p
                    ref={errRef}
                    className={errMsg ? styles['errmsg'] : styles['offscreen']}
                    aria-live="assertive"
                >
                    {errMsg && (
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16" style={{ marginRight: '8px' }}>
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
                            <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z" />
                        </svg>
                    )}
                    {errMsg}
                </p>

                <form onSubmit={handleSubmit}>
                    <label htmlFor="email">
                        E-mail:
                        {email && (
                            <FontAwesomeIcon
                                icon={validEmail ? faCheck : faTimes}
                                className={(validEmail ? styles.valid : styles.invalid) ?? ''}
                            />
                        )}
                    </label>
                    <input
                        type="text"
                        id="email"
                        ref={emailRef}
                        autoComplete="off"
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        aria-invalid={validEmail ? "false" : "true"}
                        aria-describedby="emailnote"
                        onFocus={() => setEmailFocus(true)}
                        onBlur={() => setEmailFocus(false)}
                    />
                    <p id="emailnote" className={emailFocus && email && !validEmail ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        Введіть дійсну адресу електронної пошти.
                    </p>

                    <label htmlFor="username">
                        Ім'я користувача:
                        {user && (
                            <FontAwesomeIcon
                                icon={validName ? faCheck : faTimes}
                                className={(validName ? styles.valid : styles.invalid) ?? ''}
                            />
                        )}
                    </label>
                    <input
                        type="text"
                        id="username"
                        autoComplete="off"
                        onChange={(e) => setUser(e.target.value)}
                        required
                        aria-invalid={validName ? "false" : "true"}
                        aria-describedby="uidnote"
                        onFocus={() => setUserFocus(true)}
                        onBlur={() => setUserFocus(false)}
                    />
                    <p id="uidnote" className={userFocus && user && !validName ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        4-30 символів.<br />
                        Має починатися з літери.<br />
                        Дозволені літери, цифри, підкреслення та дефіси.
                    </p>

                    <label htmlFor="password">
                        Пароль:
                        {pwd && (
                            <FontAwesomeIcon
                                icon={validPwd ? faCheck : faTimes}
                                className={(validPwd ? styles.valid : styles.invalid) ?? ''}
                            />
                        )}
                    </label>
                    <div className={styles['password-field']}>
                        <input
                            type={showPassword ? "text" : "password"}
                            id="password"
                            onChange={(e) => setPwd(e.target.value)}
                            required
                            aria-invalid={validPwd ? "false" : "true"}
                            aria-describedby="pwdnote"
                            onFocus={() => setPwdFocus(true)}
                            onBlur={() => setPwdFocus(false)}
                        />
                        <button
                            type="button"
                            className={styles['password-toggle']}
                            onClick={togglePasswordVisibility}
                            aria-label={showPassword ? "Сховати пароль" : "Показати пароль"}
                        >
                            {showPassword ? (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z" />
                                    <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z" />
                                </svg>
                            ) : (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z" />
                                    <path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z" />
                                    <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z" />
                                </svg>
                            )}
                        </button>
                    </div>
                    <p id="pwdnote" className={pwdFocus && !validPwd ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        8-30 символів.<br />
                        Має містити літери та цифри.
                    </p>

                    <label htmlFor="confirm_pwd">
                        Підтвердіть пароль:
                        {matchPwd && (
                            <FontAwesomeIcon
                                icon={validMatch && matchPwd ? faCheck : faTimes}
                                className={(validMatch ? styles.valid : styles.invalid) ?? ''}
                            />
                        )}
                    </label>
                    <div className={styles['password-field']}>
                        <input
                            type={showMatchPassword ? "text" : "password"}
                            id="confirm_pwd"
                            onChange={(e) => setMatchPwd(e.target.value)}
                            required
                            aria-invalid={validMatch ? "false" : "true"}
                            aria-describedby="confirmnote"
                            onFocus={() => setMatchFocus(true)}
                            onBlur={() => setMatchFocus(false)}
                        />
                        <button
                            type="button"
                            className={styles['password-toggle']}
                            onClick={toggleMatchPasswordVisibility}
                            aria-label={showMatchPassword ? "Сховати пароль" : "Показати пароль"}
                        >
                            {showMatchPassword ? (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z" />
                                    <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z" />
                                </svg>
                            ) : (
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z" />
                                    <path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z" />
                                    <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z" />
                                </svg>
                            )}
                        </button>
                    </div>
                    <p id="confirmnote" className={matchFocus && !validMatch ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        Має співпадати з полем пароля.
                    </p>

                    <Button
                        variant="primary"
                        size="md"
                        disabled={!validEmail || !validName || !validPwd || !validMatch}
                    >
                        Зареєструватись
                    </Button>
                </form>

                <p className={styles['login-link']}>
                    Вже маєте акаунт?
                    <span className={styles['line']}>
                        <Link to="/login">Увійти</Link>
                    </span>
                </p>
            </div>
        </section>
    );
}

export default RegisterPage;
