import { useState, useEffect, useCallback } from 'react';
import styles from './CommentSection.module.css';
import { commentService } from '../../api/comment.service';
import { Pagination } from '../Pagination/Pagination';
import ReportModal from '../ReportModal/ReportModal';
import type { IComment } from '../../types/comment.types';

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
    const [comments, setComments] = useState<IComment[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta>({
        totalCount: 0, pageSize: DEFAULT_PAGE_SIZE, pageNumber: 1, totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [newComment, setNewComment] = useState('');
    const [newRating, setNewRating] = useState(0);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
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
            setError(null);
        } catch {
            setError('Failed to load comments.');
        } finally {
            setLoading(false);
        }
    }, [buildId]);

    useEffect(() => {
        fetchComments(1);
    }, [fetchComments]);

    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!newComment.trim() || newRating === 0 || submitting) return;

        try {
            setSubmitting(true);
            setError(null);
            await commentService.addComment(buildId, { text: newComment.trim(), rating: newRating });
            setNewComment('');
            setNewRating(0);
            await fetchComments(1);
        } catch (err) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message || 'Failed to add comment.';
            setError(msg);
        } finally {
            setSubmitting(false);
        }
    };

    const handleDelete = async (commentId: string) => {
        try {
            await commentService.deleteComment(commentId);
            await fetchComments(pagination.pageNumber);
        } catch {
            setError('Failed to delete comment.');
        }
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
                <form className={styles['comment-form']} onSubmit={handleSubmit}>
                    <div className={styles['rating-picker']}>
                        <span className={styles['rating-label']}>Ваша оцінка:</span>
                        <StarRating rating={newRating} interactive onRate={setNewRating} />
                    </div>
                    <textarea
                        className={styles['comment-input']}
                        value={newComment}
                        onChange={e => setNewComment(e.target.value)}
                        placeholder="Напишіть відгук..."
                        maxLength={500}
                        rows={3}
                    />
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

            {error && <div className={styles['error']}>{error}</div>}

            {loading ? (
                <div className={styles['loading']}>Завантаження відгуків...</div>
            ) : comments.length === 0 ? (
                <div className={styles['no-comments']}>Поки немає відгуків.</div>
            ) : (
                <>
                    <div className={styles['comments-list']}>
                        {comments.map(comment => (
                            <div key={comment.id} className={styles['comment']}>
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
                                        <span className={styles['comment-date']}>{formatDate(comment.createdAt)}</span>
                                        {currentUserId && currentUserId !== comment.userId && (
                                            <button
                                                className={styles['report-btn']}
                                                onClick={() => setReportTarget(comment.id)}
                                                title="Поскаржитись"
                                            >
                                                ⚑
                                            </button>
                                        )}
                                        {currentUserId === comment.userId && (
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
                        ))}
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
