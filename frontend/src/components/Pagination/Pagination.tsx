import React from "react";
import styles from "./Pagination.module.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faChevronLeft,
    faChevronRight
} from "@fortawesome/free-solid-svg-icons";

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
    totalResults: number;
    pageSize: number;
    siblingCount?: number;
    disabled?: boolean;
}

const range = (start: number, end: number): number[] =>
    Array.from({ length: end - start + 1 }, (_, i) => start + i);

const buildPages = (currentPage: number, totalPages: number, siblingCount: number): (number | "...")[] => {
    const totalPageNumbers = siblingCount * 2 + 5;

    if (totalPages <= totalPageNumbers) {
        return range(1, totalPages);
    }

    const leftSibling = Math.max(currentPage - siblingCount, 1);
    const rightSibling = Math.min(currentPage + siblingCount, totalPages);

    const showLeftEllipsis = leftSibling > 2;
    const showRightEllipsis = rightSibling < totalPages - 1;

    if (!showLeftEllipsis && showRightEllipsis) {
        return [...range(1, 3 + siblingCount * 2), "...", totalPages];
    }

    if (showLeftEllipsis && !showRightEllipsis) {
        return [1, "...", ...range(totalPages - (2 + siblingCount * 2), totalPages)];
    }

    return [1, "...", ...range(leftSibling, rightSibling), "...", totalPages];
};

export const Pagination: React.FC<PaginationProps> = ({
    currentPage,
    totalPages,
    onPageChange,
    totalResults,
    pageSize,
    siblingCount = 1,
    disabled = false,
}) => {
    if (totalPages <= 1) return null;

    const pages = buildPages(currentPage, totalPages, siblingCount);
    const isFirst = currentPage === 1;
    const isLast = currentPage === totalPages;

    const firstResult = (currentPage - 1) * pageSize + 1;
    const lastResult = Math.min(currentPage * pageSize, totalResults);

    return (
        <div className={styles.paginationWrapper}>
            <span className={styles.resultsInfo}>
                Showing <strong>{firstResult}–{lastResult}</strong> out of <strong>{totalResults}</strong> results
            </span>

            <nav aria-label="Pagination" className={styles.pagination}>
                <button
                    className={`${styles.paginationButton} ${styles.navButton}`}
                    onClick={() => onPageChange(currentPage - 1)}
                    disabled={isFirst || disabled}
                    title="Previous page"
                    aria-label="Previous page"
                >
                    <FontAwesomeIcon icon={faChevronLeft} />
                </button>

                {pages.map((page, idx) =>
                    page === "..." ? (
                        <span
                            key={`ellipsis-${idx}`}
                            className={`${styles.paginationButton} ${styles.ellipsis}`}
                            aria-hidden="true"
                        >
                            &hellip;
                        </span>
                    ) : (
                        <button
                            key={page}
                            className={`${styles.paginationButton} ${page === currentPage ? styles.activePagination : ""}`}
                            onClick={() => onPageChange(page)}
                            disabled={disabled}
                            aria-label={`Page ${page}`}
                            aria-current={page === currentPage ? "page" : undefined}
                        >
                            {page}
                        </button>
                    )
                )}

                <button
                    className={`${styles.paginationButton} ${styles.navButton}`}
                    onClick={() => onPageChange(currentPage + 1)}
                    disabled={isLast || disabled}
                    title="Next page"
                    aria-label="Next page"
                >
                    <FontAwesomeIcon icon={faChevronRight} />
                </button>
            </nav>
        </div>
    );
};
