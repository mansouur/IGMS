import useToastStore from '../../store/toastStore'

// ─── Icon per type ────────────────────────────────────────────────────────────

const ICONS = {
  success: (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <path d="M20 6L9 17l-5-5" />
    </svg>
  ),
  error: (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <path d="M18 6L6 18M6 6l12 12" />
    </svg>
  ),
  warning: (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0zM12 9v4M12 17h.01" />
    </svg>
  ),
  info: (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <circle cx="12" cy="12" r="10" />
      <path d="M12 16v-4M12 8h.01" />
    </svg>
  ),
}

const STYLES = {
  success: 'bg-emerald-50 border-emerald-300 text-emerald-800',
  error:   'bg-red-50    border-red-300    text-red-800',
  warning: 'bg-amber-50  border-amber-300  text-amber-800',
  info:    'bg-blue-50   border-blue-300   text-blue-800',
}

const ICON_STYLES = {
  success: 'text-emerald-600',
  error:   'text-red-600',
  warning: 'text-amber-600',
  info:    'text-blue-600',
}

// ─── Single Toast ─────────────────────────────────────────────────────────────

function ToastItem({ toast }) {
  const remove = useToastStore((s) => s.remove)

  return (
    <div
      role="alert"
      className={[
        'flex items-start gap-3 px-4 py-3 rounded-xl border shadow-md',
        'text-sm font-medium min-w-[260px] max-w-[360px]',
        'animate-[fadeIn_0.2s_ease]',
        STYLES[toast.type] ?? STYLES.info,
      ].join(' ')}
    >
      <span className={`flex-shrink-0 mt-0.5 ${ICON_STYLES[toast.type]}`}>
        {ICONS[toast.type]}
      </span>
      <span className="flex-1 leading-snug">{toast.message}</span>
      <button
        onClick={() => remove(toast.id)}
        className="flex-shrink-0 opacity-50 hover:opacity-100 transition-opacity mt-0.5"
        aria-label="إغلاق"
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
          <path d="M18 6L6 18M6 6l12 12" />
        </svg>
      </button>
    </div>
  )
}

// ─── Toast Container (fixed bottom-start) ─────────────────────────────────────

export default function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts)

  if (!toasts.length) return null

  return (
    <div
      aria-live="polite"
      className="fixed bottom-5 start-5 z-[100] flex flex-col gap-2"
    >
      {toasts.map((t) => (
        <ToastItem key={t.id} toast={t} />
      ))}
    </div>
  )
}
