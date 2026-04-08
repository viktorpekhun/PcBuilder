import { useState, useEffect, useCallback } from 'react';
import styles from './CommentSection.module.css';
import { commentService } from '../../api/comment.service';
import { Pagination } from '../Pagination/Pagination';
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

function CommentSection({ buildId, currentUserId }: CommentSectionProps) {
    const [comments, setComments] = useState<IComment[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta>({
        totalCount: 0, pageSize: DEFAULT_PAGE_SIZE, pageNumber: 1, totalPages: 0
    });
    const [loading, setLoading] = useState(true);
    const [newComment, setNewComment] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newComment.trim() || submitting) return;

        try {
            setSubmitting(true);
            setError(null);
            await commentService.addComment(buildId, { text: newComment.trim() });
            setNewComment('');
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
                Коментарі {pagination.totalCount > 0 && `(${pagination.totalCount})`}
            </h3>

            {currentUserId && (
                <form className={styles['comment-form']} onSubmit={handleSubmit}>
                    <textarea
                        className={styles['comment-input']}
                        value={newComment}
                        onChange={e => setNewComment(e.target.value)}
                        placeholder="Напишіть коментар..."
                        maxLength={500}
                        rows={3}
                    />
                    <div className={styles['form-footer']}>
                        <span className={styles['char-count']}>{newComment.length}/500</span>
                        <button
                            type="submit"
                            className={styles['submit-btn']}
                            disabled={!newComment.trim() || submitting}
                        >
                            {submitting ? 'Надсилання...' : 'Надіслати'}
                        </button>
                    </div>
                </form>
            )}

            {error && <div className={styles['error']}>{error}</div>}

            {loading ? (
                <div className={styles['loading']}>Завантаження коментарів...</div>
            ) : comments.length === 0 ? (
                <div className={styles['no-comments']}>Поки немає коментарів.</div>
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
                                        {currentUserId === comment.userId && (
                                            <button
                                                className={styles['delete-btn']}
                                                onClick={() => handleDelete(comment.id)}
                                                title="Видалити коментар"
                                            >
                                                &times;
                                            </button>
                                        )}
                                    </div>
                                </div>
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
        </div>
    );
}

export default CommentSection;