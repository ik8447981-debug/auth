import { useState } from 'react';
import { ChevronUp, ChevronDown, ChevronLeft, ChevronRight } from 'lucide-react';
import clsx from 'clsx';
import LoadingSpinner from './LoadingSpinner';

export interface Column<T> {
  key: string;
  header: string;
  sortable?: boolean;
  render?: (item: T) => React.ReactNode;
  className?: string;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  emptyMessage?: string;
  emptyIcon?: React.ReactNode;
  pageSize?: number;
  currentPage?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  onSort?: (key: string, direction: 'asc' | 'desc') => void;
  keyExtractor: (item: T) => string;
}

export default function DataTable<T>({
  columns,
  data,
  loading = false,
  emptyMessage = 'No data found',
  emptyIcon,
  pageSize = 10,
  currentPage = 1,
  totalPages = 1,
  onPageChange,
  onSort,
  keyExtractor,
}: DataTableProps<T>) {
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  const handleSort = (key: string) => {
    const newDirection = sortKey === key && sortDirection === 'asc' ? 'desc' : 'asc';
    setSortKey(key);
    setSortDirection(newDirection);
    onSort?.(key, newDirection);
  };

  if (loading) {
    return (
      <div className="card overflow-hidden">
        <div className="p-8">
          <LoadingSpinner />
        </div>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="card overflow-hidden">
        <div className="flex flex-col items-center justify-center py-16">
          {emptyIcon || (
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-surface-800">
              <svg className="h-8 w-8 text-surface-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
              </svg>
            </div>
          )}
          <p className="mt-4 text-sm text-surface-400">{emptyMessage}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="card overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-surface-700 bg-surface-800/50">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={clsx(
                    'px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-surface-400',
                    col.sortable && 'cursor-pointer hover:text-surface-200 select-none',
                    col.className
                  )}
                  onClick={() => col.sortable && handleSort(col.key)}
                >
                  <div className="flex items-center gap-1">
                    {col.header}
                    {col.sortable && sortKey === col.key && (
                      sortDirection === 'asc' ? (
                        <ChevronUp className="h-3 w-3" />
                      ) : (
                        <ChevronDown className="h-3 w-3" />
                      )
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((item) => (
              <tr key={keyExtractor(item)} className="table-row">
                {columns.map((col) => (
                  <td key={col.key} className={clsx('px-4 py-3 text-sm', col.className)}>
                    {col.render
                      ? col.render(item)
                      : String((item as Record<string, unknown>)[col.key] ?? '')}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && onPageChange && (
        <div className="flex items-center justify-between border-t border-surface-700 px-4 py-3">
          <p className="text-sm text-surface-400">
            Page {currentPage} of {totalPages}
          </p>
          <div className="flex items-center gap-2">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage <= 1}
              className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200 disabled:opacity-30 disabled:cursor-not-allowed"
            >
              <ChevronLeft className="h-4 w-4" />
            </button>
            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
              let page: number;
              if (totalPages <= 5) {
                page = i + 1;
              } else if (currentPage <= 3) {
                page = i + 1;
              } else if (currentPage >= totalPages - 2) {
                page = totalPages - 4 + i;
              } else {
                page = currentPage - 2 + i;
              }
              return (
                <button
                  key={page}
                  onClick={() => onPageChange(page)}
                  className={clsx(
                    'h-8 w-8 rounded-lg text-sm font-medium transition-colors',
                    page === currentPage
                      ? 'bg-brand-600 text-white'
                      : 'text-surface-400 hover:bg-surface-800 hover:text-surface-200'
                  )}
                >
                  {page}
                </button>
              );
            })}
            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage >= totalPages}
              className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200 disabled:opacity-30 disabled:cursor-not-allowed"
            >
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
