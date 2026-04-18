/**
 * Spinner – حلقة دوّارة بسيطة.
 * size: 'sm' | 'md' | 'lg'
 */
export function Spinner({ size = 'md', className = '' }) {
  const dim = { sm: 'w-4 h-4', md: 'w-6 h-6', lg: 'w-10 h-10' }[size]

  return (
    <svg
      className={`animate-spin text-green-600 ${dim} ${className}`}
      viewBox="0 0 24 24" fill="none"
    >
      <circle className="opacity-25" cx="12" cy="12" r="10"
        stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor"
        d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z" />
    </svg>
  )
}

/**
 * PageLoader – spinner يملأ المنطقة الرئيسية عند تحميل صفحة كاملة.
 */
export function PageLoader() {
  return (
    <div className="flex flex-col items-center justify-center h-64 gap-3 text-gray-400">
      <Spinner size="lg" />
      <p className="text-sm">جارٍ التحميل...</p>
    </div>
  )
}
