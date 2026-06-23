import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { IProfileResponse, IUserBanStatus } from '../../types/profile.types';
import { Badge } from './ProfileBadge';
import { shortDate, accountAge, banRemain } from './profileHelpers';
import styles from './ProfilePage.module.css';

export function CompletenessCard({ profile }: { profile: IProfileResponse }) {
    const { t } = useTranslation();
    const checks = [
        { lbl: t('profile.sideCards.checkPhoto'),    done: !!profile.avatarUrl },
        { lbl: t('profile.sideCards.checkEmail'),    done: profile.emailVerified },
        { lbl: t('profile.sideCards.checkBio'),      done: !!(profile.bio?.trim()) },
        { lbl: t('profile.sideCards.checkPassword'), done: profile.hasPassword },
    ];
    const done = checks.filter(c => c.done).length;
    const pct = Math.round((done / checks.length) * 100);
    return (
        <div className={styles['card']}>
            <div className={styles['card-h']}>
                <span className={styles['card-ttl']}>{t('profile.sideCards.completenessTitle')}</span>
                <span className={styles['card-aux']}>{done}/{checks.length}</span>
            </div>
            <div className={styles['card-b']}>
                <div className={styles['cmpl-head']}>
                    <span className={styles['cmpl-pct']}>{pct}<span className={styles['cmpl-pct-u']}>%</span></span>
                    <span className={styles['cmpl-count']}><b>{done}</b> {t('profile.sideCards.ofDone', { total: checks.length })}</span>
                </div>
                <div className={styles['cmpl-track']}>
                    <span className={styles['cmpl-fill']} style={{ width: `${pct}%` }} />
                </div>
                <ul className={styles['cmpl-list']}>
                    {checks.map(c => (
                        <li key={c.lbl} className={c.done ? styles['cmpl-done'] : styles['cmpl-todo']}>
                            <span className={styles['cmpl-chk']}>{c.done ? '[✓]' : '[ ]'}</span>
                            <span>{c.lbl}</span>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
}

export function OverviewCard({ profile }: { profile: IProfileResponse }) {
    const { t, i18n } = useTranslation();
    const methods: string[] = [];
    if (profile.hasPassword) methods.push(t('profile.sideCards.emailMethodLabel'));
    if (profile.googleLinked) methods.push(t('profile.sideCards.googleMethodLabel'));
    return (
        <div className={styles['card']}>
            <div className={styles['card-h']}>
                <span className={styles['card-ttl']}>{t('profile.sideCards.overviewTitle')}</span>
                <span className={styles['card-aux']}>{t('profile.sideCards.overviewAux')}</span>
            </div>
            <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                <div className={styles['kv']}>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>{t('profile.sideCards.registered')}</span>
                        <span className={styles['kv-v']}>{shortDate(profile.createdAt, i18n.language)}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>{t('profile.sideCards.accountAge')}</span>
                        <span className={styles['kv-v']}>{accountAge(profile.createdAt, t)}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>{t('profile.sideCards.buildsSaved')}</span>
                        <span className={styles['kv-v']}>{profile.buildCount}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>{t('profile.sideCards.loginMethods')}</span>
                        <span className={`${styles['kv-v']} ${styles['kv-v-sans']}`}>
                            {methods.length ? methods.join(' + ') : <span style={{ color: 'var(--err)' }}>NONE</span>}
                        </span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>{t('profile.sideCards.emailStatus')}</span>
                        <span className={styles['kv-v']}>
                            {profile.emailVerified
                                ? <Badge variant="ok">{t('profile.sideCards.badgeVerified')}</Badge>
                                : <Badge variant="warn">{t('profile.sideCards.badgeUnverified')}</Badge>}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}

export function BansCard({ banStatus }: { banStatus: IUserBanStatus }) {
    const { t, i18n } = useTranslation();
    const [expanded, setExpanded] = useState(false);
    const VISIBLE = 3;

    const activeBans: { key: string; label: string; desc: string; until: string }[] = [];
    if (banStatus.isCommentBanned && banStatus.commentBanUntil)
        activeBans.push({ key: 'comment', label: t('profile.sideCards.commentBanLabel'), desc: t('profile.sideCards.commentBanDesc'), until: banStatus.commentBanUntil });
    if (banStatus.isPostBanned && banStatus.postBanUntil)
        activeBans.push({ key: 'post', label: t('profile.sideCards.publishBanLabel'), desc: t('profile.sideCards.publishBanDesc'), until: banStatus.postBanUntil });

    const warns = banStatus.recentWarnings;
    const wcount = warns.length;
    const cWarn = warns.filter(w => w.banType === 'Comment').length;
    const pWarn = wcount - cWarn;

    const shown = expanded ? warns : warns.slice(0, VISIBLE);
    const overflow = wcount - VISIBLE;
    const scroll = expanded && wcount > 6;
    const sev = activeBans.length ? 'ban' : 'warn';

    return (
        <div className={`${styles['card']} ${styles['restrict']} ${sev === 'ban' ? styles['restrict-ban'] : ''}`}>
            <div className={styles['card-h']}>
                <span className={styles['card-ttl']}>{t('profile.sideCards.restrictionsTitle')}</span>
                <span className={`${styles['card-aux']} ${sev === 'ban' ? styles['restrict-aux-ban'] : styles['restrict-aux-warn']}`}>
                    {activeBans.length
                        ? t('profile.sideCards.activeCount', { count: activeBans.length })
                        : t('profile.sideCards.warningCount', { count: wcount })}
                </span>
            </div>
            <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                {activeBans.length > 0 && (
                    <div className={styles['rstr-active']}>
                        {activeBans.map(b => (
                            <div key={b.key} className={styles['rstr-ban']}>
                                <span className={styles['rstr-ban-sev']} />
                                <div className={styles['rstr-ban-main']}>
                                    <div className={styles['rstr-ban-top']}>
                                        <span className={styles['rstr-ban-badge']}>{b.label.toUpperCase()}</span>
                                        <span className={styles['rstr-ban-cd']}>{banRemain(b.until)} {t('profile.sideCards.timeLeft')}</span>
                                    </div>
                                    <div className={styles['rstr-ban-sub']}>
                                        {b.desc} · {t('profile.sideCards.until')} <b>{shortDate(b.until, i18n.language)}</b>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {wcount > 0 && (
                    <div className={styles['rstr-warn']}>
                        <div className={styles['rstr-warn-head']}>
                            <span className={styles['rstr-warn-lbl']}>{t('profile.sideCards.warningsLabel')}</span>
                            <span className={styles['rstr-warn-brk']}>
                                <span className={styles['rstr-warn-chip']}><b>{cWarn}</b> {t('profile.sideCards.chipComment')}</span>
                                <span className={styles['rstr-warn-chip']}><b>{pWarn}</b> {t('profile.sideCards.chipPublish')}</span>
                            </span>
                        </div>
                        <ul className={`${styles['rstr-warns']} ${scroll ? styles['rstr-warns-scroll'] : ''}`}>
                            {shown.map(w => (
                                <li key={w.id}>
                                    <span className={styles['rstr-warns-ico']}>{w.banType === 'Comment' ? '▢' : '△'}</span>
                                    <div className={styles['rstr-warns-body']}>
                                        <div className={styles['rstr-warns-top']}>
                                            <span className={styles['rstr-warns-type']}>{w.banType === 'Comment' ? t('profile.sideCards.warnTypeComment') : t('profile.sideCards.warnTypePublish')}</span>
                                            <span className={styles['rstr-warns-date']}>{shortDate(w.issuedAt, i18n.language)}</span>
                                        </div>
                                        <span className={styles['rstr-warns-reason']}>
                                            {w.reasonCode
                                                ? t(`warnReasonCodes.${w.reasonCode}`, w.reason)
                                                : w.reason}
                                        </span>
                                    </div>
                                </li>
                            ))}
                        </ul>
                        {overflow > 0 && (
                            <button className={styles['rstr-more']} onClick={() => setExpanded(e => !e)}>
                                {expanded ? t('profile.sideCards.showLess') : t('profile.sideCards.showAll', { count: wcount })}
                            </button>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
