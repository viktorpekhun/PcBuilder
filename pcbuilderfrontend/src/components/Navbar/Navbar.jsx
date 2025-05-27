import styles from "./Navbar.module.css";
import {Link, useNavigate} from "react-router-dom";
import useAuth from "../../hooks/useAuth.js";
import useLogout from "../../hooks/useLogout.js";

export default function Navbar() {
    const {auth} = useAuth();
    const navigate = useNavigate();
    const logout = useLogout();


    const signOut = async () => {
        await logout();
        navigate('/');
    }

    return (
        <nav className={styles['navbar']}>
            <div className={styles['navbar-items']}>
                <ul className={styles['navigation-items']}>
                    <li className={styles['nav-item']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor"
                             className="bi bi-gear-wide" viewBox="0 0 16 16">
                            <path
                                d="M8.932.727c-.243-.97-1.62-.97-1.864 0l-.071.286a.96.96 0 0 1-1.622.434l-.205-.211c-.695-.719-1.888-.03-1.613.931l.08.284a.96.96 0 0 1-1.186 1.187l-.284-.081c-.96-.275-1.65.918-.931 1.613l.211.205a.96.96 0 0 1-.434 1.622l-.286.071c-.97.243-.97 1.62 0 1.864l.286.071a.96.96 0 0 1 .434 1.622l-.211.205c-.719.695-.03 1.888.931 1.613l.284-.08a.96.96 0 0 1 1.187 1.187l-.081.283c-.275.96.918 1.65 1.613.931l.205-.211a.96.96 0 0 1 1.622.434l.071.286c.243.97 1.62.97 1.864 0l.071-.286a.96.96 0 0 1 1.622-.434l.205.211c.695.719 1.888.03 1.613-.931l-.08-.284a.96.96 0 0 1 1.187-1.187l.283.081c.96.275 1.65-.918.931-1.613l-.211-.205a.96.96 0 0 1 .434-1.622l.286-.071c.97-.243.97-1.62 0-1.864l-.286-.071a.96.96 0 0 1-.434-1.622l.211-.205c.719-.695.03-1.888-.931-1.613l-.284.08a.96.96 0 0 1-1.187-1.186l.081-.284c.275-.96-.918-1.65-1.613-.931l-.205.211a.96.96 0 0 1-1.622-.434zM8 12.997a4.998 4.998 0 1 1 0-9.995 4.998 4.998 0 0 1 0 9.996z"/>
                        </svg>
                        <Link to="/">Конфігуратор</Link>
                    </li>
                    <li className={styles['nav-item']}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor"
                             className="bi bi-pc-display" viewBox="0 0 16 16">
                            <path
                                d="M8 1a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1H9a1 1 0 0 1-1-1zm1 13.5a.5.5 0 1 0 1 0 .5.5 0 0 0-1 0m2 0a.5.5 0 1 0 1 0 .5.5 0 0 0-1 0M9.5 1a.5.5 0 0 0 0 1h5a.5.5 0 0 0 0-1zM9 3.5a.5.5 0 0 0 .5.5h5a.5.5 0 0 0 0-1h-5a.5.5 0 0 0-.5.5M1.5 2A1.5 1.5 0 0 0 0 3.5v7A1.5 1.5 0 0 0 1.5 12H6v2h-.5a.5.5 0 0 0 0 1H7v-4H1.5a.5.5 0 0 1-.5-.5v-7a.5.5 0 0 1 .5-.5H7V2z"/>
                        </svg>
                        <Link to="/user/builds">Мої Збірки</Link>
                    </li>
                </ul>
                <div className={styles['login-items']}>
                    {auth?.username ? (
                        <>
                            <p>Привіт, {auth.username}</p>
                            <button onClick={signOut} className={'button-secondary'}>Вихід</button>
                        </>
                    ) : (
                        <>
                            <button className={styles['login']}>
                                <Link to={`/login`} >Вхід</Link>
                            </button>
                            <button className={styles['register']}>
                                <Link to={`/register`}>Реєстрація</Link>
                            </button>
                        </>
                    )}
                </div>
            </div>
        </nav>
    );
}
