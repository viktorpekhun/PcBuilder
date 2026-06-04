import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { authService } from '../../api/auth.service';
import styles from './EmailVerificationBanner.module.css';

const EmailVerificationBanner = () => {
    const { t } = useTranslation();
    const [dismissed, setDismissed] = useState(false);
    const [sending, setSending] = useState(false);
    const [sent, setSent] = useState(false);

    const handleResend = async () => {
        setSending(true);
        try {
            await authService.resendVerification();
            setSent(true);
        } catch {
            // keep banner open, user can try again
        } finally {
            setSending(false);
        }
    };

    if (dismissed) return null;

    return (
        <div className={styles['notification']}>
            <div className={styles['icon-wrap']}>
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M2 2a2 2 0 0 0-2 2v8.01A2 2 0 0 0 2 14h5.5a.5.5 0 0 0 0-1H2a1 1 0 0 1-.966-.741l5.64-3.471L8 9.583l7-4.2V8.5a.5.5 0 0 0 1 0V4a2 2 0 0 0-2-2zm3.708 6.208L1 11.105V5.383zM1 4.217V4a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v.217l-7 4.2z"/>
                    <path d="M16 12.5a3.5 3.5 0 1 1-7 0 3.5 3.5 0 0 1 7 0m-1.993-1.679a.5.5 0 0 0-.686.172l-1.17 1.95-.547-.547a.5.5 0 0 0-.708.708l.774.773a.75.75 0 0 0 1.174-.144l1.335-2.226a.5.5 0 0 0-.172-.686"/>
                </svg>
            </div>

            <div className={styles['body']}>
                {sent ? (
                    <p className={styles['text']}>{t('emailVerification.sent')}</p>
                ) : (
                    <p className={styles['title']}>{t('emailVerification.banner')}</p>
                )}
                {!sent && (
                    <button className={styles['action-btn']} onClick={handleResend} disabled={sending}>
                        {sending ? t('emailVerification.resending') : t('emailVerification.resend')}
                    </button>
                )}
            </div>

            <button className={styles['close-btn']} onClick={() => setDismissed(true)} aria-label={t('common.close')}>
                <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/>
                </svg>
            </button>
        </div>
    );
};

export default EmailVerificationBanner;
