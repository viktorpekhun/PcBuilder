import { useState } from 'react';
import type { IProfileResponse, IUserBanStatus } from '../../types/profile.types';
import { Badge } from './ProfileBadge';
import { shortDate, accountAge, banRemain } from './profileHelpers';
import styles from './ProfilePage.module.css';

export function CompletenessCard({ profile }: { profile: IProfileResponse }) {
    const checks = [
        { lbl: 'Profile photo uploaded', done: !!profile.avatarUrl },
        { lbl: 'E-mail verified',        done: profile.emailVerified },
        { lbl: 'Bio set',                done: !!(profile.bio?.trim()) },
        { lbl: 'Password set',           done: profile.hasPassword },
    ];
    const done = checks.filter(c => c.done).length;
    const pct = Math.round((done / checks.length) * 100);
    return (
        <div className={styles['card']}>
            <div className={styles['card-h']}>
                <span className={styles['card-ttl']}>Completeness</span>
                <span className={styles['card-aux']}>{done}/{checks.length}</span>
            </div>
            <div className={styles['card-b']}>
                <div className={styles['cmpl-head']}>
                    <span className={styles['cmpl-pct']}>{pct}<span className={styles['cmpl-pct-u']}>%</span></span>
                    <span className={styles['cmpl-count']}><b>{done}</b> OF {checks.length} DONE</span>
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
    const methods: string[] = [];
    if (profile.hasPassword) methods.push('E-mail');
    if (profile.googleLinked) methods.push('Google');
    return (
        <div className={styles['card']}>
            <div className={styles['card-h']}>
                <span className={styles['card-ttl']}>Account overview</span>
                <span className={styles['card-aux']}>SUMMARY</span>
            </div>
            <div className={`${styles['card-b']} ${styles['card-b-tight']}`}>
                <div className={styles['kv']}>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>Registered</span>
                        <span className={styles['kv-v']}>{shortDate(profile.createdAt)}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>Account age</span>
                        <span className={styles['kv-v']}>{accountAge(profile.createdAt)}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>Builds saved</span>
                        <span className={styles['kv-v']}>{profile.buildCount}</span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>Login methods</span>
                        <span className={`${styles['kv-v']} ${styles['kv-v-sans']}`}>
                            {methods.length ? methods.join(' + ') : <span style={{ color: 'var(--err)' }}>NONE</span>}
                        </span>
                    </div>
                    <div className={styles['kv-row']}>
                        <span className={styles['kv-k']}>E-mail status</span>
                        <span className={styles['kv-v']}>
                            {profile.emailVerified
                                ? <Badge variant="ok">VERIFIED</Badge>
                                : <Badge variant="warn">UNVERIFIED</Badge>}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}

export function BansCard({ banStatus }: { banStatus: IUserBanStatus }) {
    const [expanded, setExpanded] = useState(false);
    const VISIBLE = 3;

    const activeBans: { key: string; label: string; desc: string; until: string }[] = [];
    if (banStatus.isCommentBanned && banStatus.commentBanUntil)
        activeBans.push({ key: 'comment', label: 'Comment ban', desc: "Can't reply on builds", until: banStatus.commentBanUntil });
    if (banStatus.isPostBanned && banStatus.postBanUntil)
        activeBans.push({ key: 'post', label: 'Publish ban', desc: "Can't publish new builds", until: banStatus.postBanUntil });

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
                <span className={styles['card-ttl']}>Account restrictions</span>
                <span className={`${styles['card-aux']} ${sev === 'ban' ? styles['restrict-aux-ban'] : styles['restrict-aux-warn']}`}>
                    {activeBans.length
                        ? `${activeBans.length} ACTIVE`
                        : `${wcount} WARNING${wcount === 1 ? '' : 'S'}`}
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
                                        <span className={styles['rstr-ban-cd']}>{banRemain(b.until)} LEFT</span>
                                    </div>
                                    <div className={styles['rstr-ban-sub']}>
                                        {b.desc} · until <b>{shortDate(b.until)}</b>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {wcount > 0 && (
                    <div className={styles['rstr-warn']}>
                        <div className={styles['rstr-warn-head']}>
                            <span className={styles['rstr-warn-lbl']}>Warnings · 180 days</span>
                            <span className={styles['rstr-warn-brk']}>
                                <span className={styles['rstr-warn-chip']}><b>{cWarn}</b> CMT</span>
                                <span className={styles['rstr-warn-chip']}><b>{pWarn}</b> PUB</span>
                            </span>
                        </div>
                        <ul className={`${styles['rstr-warns']} ${scroll ? styles['rstr-warns-scroll'] : ''}`}>
                            {shown.map(w => (
                                <li key={w.id}>
                                    <span className={styles['rstr-warns-ico']}>{w.banType === 'Comment' ? '▢' : '△'}</span>
                                    <div className={styles['rstr-warns-body']}>
                                        <div className={styles['rstr-warns-top']}>
                                            <span className={styles['rstr-warns-type']}>{w.banType === 'Comment' ? 'COMMENT' : 'PUBLISH'}</span>
                                            <span className={styles['rstr-warns-date']}>{shortDate(w.issuedAt)}</span>
                                        </div>
                                        <span className={styles['rstr-warns-reason']}>{w.reason}</span>
                                    </div>
                                </li>
                            ))}
                        </ul>
                        {overflow > 0 && (
                            <button className={styles['rstr-more']} onClick={() => setExpanded(e => !e)}>
                                {expanded ? '↑ SHOW LESS' : `↓ SHOW ALL ${wcount} WARNINGS`}
                            </button>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
