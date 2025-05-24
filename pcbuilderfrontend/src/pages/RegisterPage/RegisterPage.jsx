import { useRef, useState, useEffect } from "react";
import { faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import axios from '../../api/axios.jsx';
import useAuth from "../../hooks/useAuth.js";
import { Link, useNavigate } from "react-router-dom";
import styles from './RegisterPage.module.css';

const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,50}$/;
const USER_REGEX = /^[a-zA-Z][a-zA-Z0-9-_]{3,30}$/;
const PWD_REGEX = /^(?=.*[a-zA-Z])(?=.*[0-9]).{8,30}$/;
const REGISTER_URL = '/api/auth/register';

const RegisterPage = () => {
    const { setAuth, persist, setPersist } = useAuth();
    const navigate = useNavigate();
    const emailRef = useRef();
    const errRef = useRef();

    const [email, setEmail] = useState('');
    const [validEmail, setValidEmail] = useState(false);
    const [emailFocus, setEmailFocus] = useState(false);

    const [user, setUser] = useState('');
    const [validName, setValidName] = useState(false);
    const [userFocus, setUserFocus] = useState(false);

    const [pwd, setPwd] = useState('');
    const [validPwd, setValidPwd] = useState(false);
    const [pwdFocus, setPwdFocus] = useState(false);

    const [matchPwd, setMatchPwd] = useState('');
    const [validMatch, setValidMatch] = useState(false);
    const [matchFocus, setMatchFocus] = useState(false);

    const [errMsg, setErrMsg] = useState('');

    useEffect(() => {
        emailRef.current.focus();
    }, [])

    useEffect(() => {
        const result = EMAIL_REGEX.test(email);
        setValidEmail(result);
    }, [email]);

    useEffect(() => {
        const result = USER_REGEX.test(user);
        setValidName(result);
    }, [user]);

    useEffect(() => {
        const result = PWD_REGEX.test(pwd);
        setValidPwd(result);
        const match = pwd === matchPwd;
        setValidMatch(match);
    }, [pwd, matchPwd]);

    useEffect(() => {
        setErrMsg('');
    }, [email, user, pwd, matchPwd]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        const v1 = EMAIL_REGEX.test(email);
        const v2 = USER_REGEX.test(user);
        const v3 = PWD_REGEX.test(pwd);
        if (!v1 || !v2 || !v3) {
            setErrMsg("Invalid Entry");
            return;
        }
        try {
            const response = await axios.post(REGISTER_URL,
                JSON.stringify({ Email: email, Username: user, Password: pwd }),
                {
                    headers: { 'Content-Type': 'application/json' },
                    withCredentials: true
                }
            );
            const accessToken = response?.data?.accessToken;
            setAuth({ email, pwd, accessToken });
            setEmail('');
            setPwd('');
            navigate("/");
        } catch (err) {
            if (!err?.response) {
                setErrMsg('No Server Response');
            } else if (err.response.status === 409) {
                setErrMsg('This Email Already Taken');
            } else {
                setErrMsg('Registration Failed');
            }
            errRef.current.focus();
        }
    }

    useEffect(() => {
        setPersist(true);
    }, []);


    useEffect(() => {
        localStorage.setItem("persist", persist);
    }, [persist]);

    return (
        <div className={styles['register-container']}>
            <section>
                <p ref={errRef} className={errMsg ? styles['errmsg'] : styles['offscreen']} aria-live="assertive">{errMsg}</p>
                <h1>Register</h1>
                <form onSubmit={handleSubmit}>
                    <label htmlFor="email">
                        Email:
                        <span className={validEmail ? styles['valid'] : styles['hide']}><FontAwesomeIcon icon={faCheck} /></span>
                        <span className={validEmail || !email ? styles['hide'] : styles['invalid']}><FontAwesomeIcon icon={faTimes} /></span>
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
                        Provide a valid email address.
                    </p>

                    <label htmlFor="username">
                        Username:
                        <span className={validName ? styles['valid'] : styles['hide']}><FontAwesomeIcon icon={faCheck} /></span>
                        <span className={validName || !user ? styles['hide'] : styles['invalid']}><FontAwesomeIcon icon={faTimes} /></span>
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
                        4 to 24 characters.<br />
                        Must begin with a letter.<br />
                    </p>

                    <label htmlFor="password">
                        Password:
                        <span className={validPwd ? styles['valid'] : styles['hide']}><FontAwesomeIcon icon={faCheck} /></span>
                        <span className={validPwd || !pwd ? styles['hide'] : styles['invalid']}><FontAwesomeIcon icon={faTimes} /></span>
                    </label>
                    <input
                        type="password"
                        id="password"
                        onChange={(e) => setPwd(e.target.value)}
                        required
                        aria-invalid={validPwd ? "false" : "true"}
                        aria-describedby="pwdnote"
                        onFocus={() => setPwdFocus(true)}
                        onBlur={() => setPwdFocus(false)}
                    />
                    <p id="pwdnote" className={pwdFocus && !validPwd ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        8 to 24 characters.<br />
                        Must include letters and numbers.
                    </p>

                    <label htmlFor="confirm_pwd">
                        Confirm Password:
                        <span className={validMatch && matchPwd ? styles['valid'] : styles['hide']}><FontAwesomeIcon icon={faCheck} /></span>
                        <span className={validMatch || !matchPwd ? styles['hide'] : styles['invalid']}><FontAwesomeIcon icon={faTimes} /></span>
                    </label>
                    <input
                        type="password"
                        id="confirm_pwd"
                        onChange={(e) => setMatchPwd(e.target.value)}
                        required
                        aria-invalid={validMatch ? "false" : "true"}
                        aria-describedby="confirmnote"
                        onFocus={() => setMatchFocus(true)}
                        onBlur={() => setMatchFocus(false)}
                    />
                    <p id="confirmnote" className={matchFocus && !validMatch ? styles['instructions'] : styles['offscreen']}>
                        <FontAwesomeIcon icon={faInfoCircle} />
                        Must match the password field.
                    </p>

                    <button disabled={!validName || !validPwd || !validMatch ? true : false}>
                        Sign Up
                    </button>

                </form>

                <p>
                    Already registered?<br />
                    <span className={styles['line']}>
                        <Link to="/login">Sign In</Link>
                    </span>
                </p>
            </section>
        </div>
    );
}

export default RegisterPage;
