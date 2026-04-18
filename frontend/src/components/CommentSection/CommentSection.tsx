import { useState, useEffect, useCallback } from 'react';
import styles from './CommentSection.module.css';
import { commentService } from '../../api/comment.service';
import { Pagination } from '../Pagination/Pagination';
import ReportModal from '../ReportModal/ReportModal';
import useAuth from '../../hooks/useAuth';
import type { IComment } from '../../types/comment.types';

type PendingComment = IComment & { isPending: true };
type DisplayComment = IComment | PendingComment;
const isPending = (c: DisplayComment): c is PendingComment =>
    (c as PendingComment).isPending === true;

interface PaginationMeta {
    totalCount: number;
    pageSize: number;
    pageNumber: number;
    totalPages: number;
}

interface CommentSectionProps {
    buildId: string;
    currentUserId?: string;
}

const DEFAULT_PAGE_SIZE = 10;

function StarRating({ rating, interactive, onRate }: { rating: number; interactive?: boolean; onRate?: (r: number) => void }) {
    const [hovered, setHovered] = useState(0);
    const display = interactive ? (hovered || rating) : rating;

    return (
        <div className={styles['comment-rating']}>
            {[1, 2, 3, 4, 5].map(star => (
                interactive ? (
                    <button
                        key={star}
                        type="button"
                        className={`${styles['star-btn']} ${star <= display ? styles['active'] : ''}`}
                        onMouseEnter={() => setHovered(star)}
                        onMouseLeave={() => setHovered(0)}
                        onClick={() => onRate?.(star)}
                        title={`${star} зірок`}
                    >
                        ★
                    </button>
                ) : (
                    <span key={star} className={star <= rating ? styles['star-filled'] : styles['star-empty']}>★</span>
                )
            ))}
        </div>
    );
}

function CommentSection({ buildId, currentUserId }: CommentSectionProps) {
    const { auth } = useAuth();
    const [comments, setComments] = useState<DisplayComment[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta>({
        totalCount: 0, pageSize: DEFAULT_PAGE_SIZE, pageNumber: 1, totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [newComment, setNewComment] = useState('');
    const [newRating, setNewRating] = useState(0);
    const [submitting, setSubmitting] = useState(false);
    const [formError, setFormError] = useState<string | null>(null);
    const [listError, setListError] = useState<string | null>(null);
    const [reportTarget, setReportTarget] = useState<string | null>(null);

    const fetchComments = useCallback(async (page: number) => {
        try {
            setLoading(true);
            const response = await commentService.getComments(buildId, page, DEFAULT_PAGE_SIZE);
            setComments(response.data);

            const paginationHeader = response.headers['x-pagination'];
            if (paginationHeader) {
                const meta = JSON.parse(paginationHeader);
                setPagination(meta);
            }
            setListError(null);
        } catch {
            setListError('Не вдалося завантажити відгуки.');
        } finally {
            setLoading(false);
        }
    }, [buildId]);

    useEffect(() => {
        fetchComments(1);
    }, [fetchComments]);

    const isToxicError = (status?: number, code?: string) =>
        status === 400 && code === 'Toxic';

    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        const trimmed = newComment.trim();
        if (!trimmed || newRating === 0 || submitting || !currentUserId) return;

        const tempId = `temp-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
        const optimistic: PendingComment = {
            id: tempId,
            text: trimmed,
            rating: newRating,
            createdAt: new Date().toISOString(),
            userId: currentUserId,
            username: auth.username ?? 'Ви',
            ...(auth.avatarUrl ? { avatarUrl: auth.avatarUrl } : {}),
            isPending: true,
        };

        const snapshotText = newComment;
        const snapshotRating = newRating;

        setComments(prev => [optimistic, ...prev]);
        setNewComment('');
        setNewRating(0);
        setFormError(null);
        setSubmitting(true);

        try {
            await commentService.addComment(buildId, { text: trimmed, rating: newRating });
            await fetchComments(1);
        } catch (err) {
            setComments(prev => prev.filter(c => c.id !== tempId));
            const resp = (err as { response?: { status?: number; data?: { message?: string; code?: string } } })?.response;

            if (isToxicError(resp?.status, resp?.data?.code)) {
                // Silent — notification bell will surface it shortly.
                return;
            }

            setNewComment(snapshotText);
            setNewRating(snapshotRating);
            setFormError(resp?.data?.message || 'Не вдалося надіслати відгук. Спробуйте ще раз.');
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (commentId: string) => {
        try {
            await commentService.deleteComment(commentId);
            await fetchComments(pagination.pageNumber);
        } catch {
            setListError('Не вдалося видалити відгук.');
        }
    };

    const handleCommentChange = (value: string) => {
        setNewComment(value);
        if (formError) setFormError(null);
    };

    const formatDate = (dateStr: string) => {
        const date = new Date(dateStr);
        return date.toLocaleDateString('uk-UA', {
            day: 'numeric', month: 'short', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    };

    return (
        <div className={styles['comment-section']}>
            <h3 className={styles['section-title']}>
                Відгуки {pagination.totalCount > 0 && `(${pagination.totalCount})`}
            </h3>

            {currentUserId && (
                <form className={styles['comment-form']} onSubmit={handleSubmit} noValidate>
                    <div className={styles['rating-picker']}>
                        <span className={styles['rating-label']}>Ваша оцінка:</span>
                        <StarRating rating={newRating} interactive onRate={setNewRating} />
                    </div>
                    <textarea
                        className={`${styles['comment-input']} ${formError ? styles['comment-input--error'] : ''}`}
                        value={newComment}
                        onChange={e => handleCommentChange(e.target.value)}
                        placeholder="Напишіть відгук..."
                        maxLength={500}
                        rows={3}
                        aria-invalid={!!formError}
                        aria-describedby={formError ? 'comment-form-error' : undefined}
                    />
                    {formError && (
                        <div id="comment-form-error" className={styles['field-error']} role="alert">
                            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                                <path d="M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm0 3a.9.9 0 0 1 .9.9v3.6a.9.9 0 0 1-1.8 0V4.9A.9.9 0 0 1 8 4zm0 8.2a1.1 1.1 0 1 1 0-2.2 1.1 1.1 0 0 1 0 2.2z" />
                            </svg>
                            <span>{formError}</span>
                        </div>
                    )}
                    <div className={styles['form-footer']}>
                        <span className={styles['char-count']}>{newComment.length}/500</span>
                        <button
                            type="submit"
                            className={styles['submit-btn']}
                            disabled={!newComment.trim() || newRating === 0 || submitting}
                        >
                            {submitting ? 'Надсилання...' : 'Надіслати'}
                        </button>
                    </div>
                </form>
            )}

            {listError && (
                <div className={styles['list-error']} role="alert">
                    <span>{listError}</span>
                    <button
                        type="button"
                        className={styles['list-error-dismiss']}
                        onClick={() => setListError(null)}
                        aria-label="Закрити"
                    >
                        &times;
                    </button>
                </div>
            )}

            {loading && comments.length === 0 ? (
                <div className={styles['loading']}>Завантаження відгуків...</div>
            ) : comments.length === 0 ? (
                <div className={styles['no-comments']}>Поки немає відгуків.</div>
            ) : (
                <>
                    <div className={styles['comments-list']}>
                        {comments.map(comment => {
                            const pending = isPending(comment);
                            return (
                                <div
                                    key={comment.id}
                                    className={`${styles['comment']} ${pending ? styles['comment--pending'] : ''}`}
                                    aria-busy={pending}
                                >
                                    <div className={styles['comment-header']}>
                                        <div className={styles['comment-author']}>
                                            {comment.avatarUrl ? (
                                                <img
                                                    src={comment.avatarUrl}
                                                    alt={comment.username}
                                                    className={styles['avatar']}
                                                />
                                            ) : (
                                                <div className={styles['avatar-placeholder']}>
                                                    {comment.username.charAt(0).toUpperCase()}
                                                </div>
                                            )}
                                            <span className={styles['username']}>{comment.username}</span>
                                        </div>
                                        <div className={styles['comment-meta']}>
                                            {pending ? (
                                                <span className={styles['pending-label']}>Надсилається...</span>
                                            ) : (
                                                <span className={styles['comment-date']}>{formatDate(comment.createdAt)}</span>
                                            )}
                                            {!pending && currentUserId && currentUserId !== comment.userId && (
                                                <button
                                                    className={styles['report-btn']}
                                                    onClick={() => setReportTarget(comment.id)}
                                                    title="Поскаржитись"
                                                >
                                                    ⚑
                                                </button>
                                            )}
                                            {!pending && currentUserId === comment.userId && (
                                                <button
                                                    className={styles['delete-btn']}
                                                    onClick={() => handleDelete(comment.id)}
                                                    title="Видалити відгук"
                                                >
                                                    &times;
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                    <StarRating rating={comment.rating} />
                                    <p className={styles['comment-text']}>{comment.text}</p>
                                </div>
                            );
                        })}
                    </div>

                    <Pagination
                        currentPage={pagination.pageNumber}
                        totalPages={pagination.totalPages}
                        totalResults={pagination.totalCount}
                        pageSize={pagination.pageSize}
                        onPageChange={(page) => fetchComments(page)}
                    />
                </>
            )}

            <ReportModal
                isOpen={reportTarget !== null}
                targetType="review"
                targetId={reportTarget ?? ""}
                onClose={() => setReportTarget(null)}
            />
        </div>
    );
}

export default CommentSection;
