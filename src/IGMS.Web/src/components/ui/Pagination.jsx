import { useTranslation } from 'react-i18next'

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100]

/**
 * Pagination – شريط تصفح الصفحات.
 *
 * Props:
 *   currentPage      : رقم الصفحة الحالية (1-based)
 *   totalPages       : إجمالي عدد الصفحات
 *   totalCount       : إجمالي السجلات
 *   pageSize         : حجم الصفحة الحالي
 *   onPageChange     : (page: number) => void
 *   onPageSizeChange : (size: number) => void  (اختياري)
 */
export default function Pagination({ currentPage, totalPages, totalCount, pageSize, onPageChange, onPageSizeChange }) {
  const { t } = useTranslation()
  if (!totalCount) return null

  const getPages = () => {
    const pages = []
    const delta = 2

    for (let i = 1; i <= totalPages; i++) {
      if (
        i === 1 ||
        i === totalPages ||
        (i >= currentPage - delta && i <= currentPage + delta)
      ) {
        pages.push(i)
      } else if (
        i === currentPage - delta - 1 ||
        i === currentPage + delta + 1
      ) {
        pages.push('…')
      }
    }
    return pages
  }

  const from = (currentPage - 1) * pageSize + 1
  const to   = Math.min(currentPage * pageSize, totalCount)

  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-3 px-1 py-3">

      {/* Info + page size selector */}
      <div className="flex items-center gap-3">
        <p className="text-sm text-gray-500 select-none">
          {t('pagination.showing')} <span className="font-semibold text-gray-700">{from}–{to}</span>{' '}
          {t('pagination.of')}{' '}
          <span className="font-semibold text-gray-700">{totalCount}</span>{' '}
          {t('pagination.records')}
        </p>
        {onPageSizeChange && (
          <select
            value={pageSize}
            onChange={(e) => { onPageSizeChange(Number(e.target.value)) }}
            className="border border-gray-200 rounded-md px-2 py-1 text-xs text-gray-600
              focus:outline-none focus:ring-2 focus:ring-green-600 bg-white"
          >
            {PAGE_SIZE_OPTIONS.map((s) => (
              <option key={s} value={s}>{s} {t('pagination.perPage')}</option>
            ))}
          </select>
        )}
      </div>

      {/* Page buttons – تظهر فقط إذا كان هناك أكثر من صفحة */}
      {totalPages > 1 && (
        <div className="flex items-center gap-1">
          <PageBtn
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            label="السابق"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </PageBtn>

          {getPages().map((page, idx) =>
            page === '…' ? (
              <span key={`ellipsis-${idx}`} className="px-2 text-gray-400 select-none">…</span>
            ) : (
              <PageBtn
                key={page}
                onClick={() => onPageChange(page)}
                active={page === currentPage}
                label={`صفحة ${page}`}
              >
                {page}
              </PageBtn>
            )
          )}

          <PageBtn
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            label="التالي"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <path d="M15 18l-6-6 6-6" />
            </svg>
          </PageBtn>
        </div>
      )}
    </div>
  )
}

function PageBtn({ children, onClick, disabled, active, label }) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      aria-current={active ? 'page' : undefined}
      className={[
        'min-w-[34px] h-8 px-2 rounded-lg text-sm font-medium transition-colors select-none',
        active
          ? 'bg-green-700 text-white'
          : 'text-gray-600 hover:bg-gray-100',
        disabled ? 'opacity-40 cursor-not-allowed' : 'cursor-pointer',
      ].join(' ')}
    >
      {children}
    </button>
  )
}
