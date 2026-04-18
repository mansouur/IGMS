/**
 * Skeleton – مستطيل رمادي متحرك يحاكي شكل المحتوى أثناء التحميل.
 */
export function Skeleton({ className = '' }) {
  return (
    <div className={`bg-gray-200 rounded animate-pulse ${className}`} />
  )
}

/**
 * SkeletonTable – صفوف وهمية لجدول بيانات.
 */
export function SkeletonTable({ rows = 5, cols = 4 }) {
  return (
    <div className="space-y-2 p-4">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex gap-4">
          {Array.from({ length: cols }).map((_, j) => (
            <Skeleton
              key={j}
              className={`h-5 ${j === 0 ? 'flex-[2]' : 'flex-1'}`}
            />
          ))}
        </div>
      ))}
    </div>
  )
}

/**
 * SkeletonCard – بطاقة وهمية.
 */
export function SkeletonCard() {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-3">
      <Skeleton className="h-4 w-1/3" />
      <Skeleton className="h-6 w-2/3" />
      <Skeleton className="h-3 w-1/2" />
    </div>
  )
}
