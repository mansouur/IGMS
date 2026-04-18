import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import useConfirmStore from '../../store/confirmStore'

function DangerIcon() {
  return (
    <svg width="28" height="28" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="3 6 5 6 21 6" />
      <path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2" />
    </svg>
  )
}

function WarningIcon() {
  return (
    <svg width="28" height="28" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
  )
}

export default function ConfirmDialog() {
  const { t } = useTranslation()
  const { open, title, message, variant, confirm, cancel } = useConfirmStore()
  const cancelRef = useRef(null)

  useEffect(() => { if (open) cancelRef.current?.focus() }, [open])

  useEffect(() => {
    if (!open) return
    const onKey = (e) => { if (e.key === 'Escape') cancel() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open, cancel])

  if (!open) return null

  const VARIANT = {
    danger: {
      iconBg: 'bg-red-50', iconColor: 'text-red-600',
      btnCls: 'bg-red-600 hover:bg-red-700 focus:ring-red-500',
      btnLabel: t('confirm.danger.btnLabel'), icon: <DangerIcon />,
    },
    warning: {
      iconBg: 'bg-amber-50', iconColor: 'text-amber-600',
      btnCls: 'bg-amber-600 hover:bg-amber-700 focus:ring-amber-500',
      btnLabel: t('confirm.warning.btnLabel'), icon: <WarningIcon />,
    },
    info: {
      iconBg: 'bg-green-50', iconColor: 'text-green-700',
      btnCls: 'bg-green-700 hover:bg-green-800 focus:ring-green-500',
      btnLabel: t('confirm.info.btnLabel'), icon: <WarningIcon />,
    },
  }

  const v = VARIANT[variant] ?? VARIANT.danger

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4"
      aria-modal="true" role="dialog">
      <div className="absolute inset-0 bg-gray-900/40 backdrop-blur-[2px]" onClick={cancel} />

      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-sm
        border border-gray-100 overflow-hidden">
        <div className={`h-1 ${
          variant === 'danger'  ? 'bg-red-500'   :
          variant === 'warning' ? 'bg-amber-500' : 'bg-green-600'
        }`} />

        <div className="p-6">
          <div className="flex gap-4 items-start">
            <div className={`flex-shrink-0 w-12 h-12 rounded-full flex items-center
              justify-center ${v.iconBg} ${v.iconColor}`}>
              {v.icon}
            </div>
            <div className="flex-1 min-w-0">
              {title && <h2 className="text-base font-bold text-gray-800 mb-1">{title}</h2>}
              <p className="text-sm text-gray-500 leading-relaxed">{message}</p>
            </div>
          </div>

          <div className="h-px bg-gray-100 my-5" />

          <div className="flex items-center justify-end gap-2">
            <button ref={cancelRef} onClick={cancel}
              className="px-4 py-2 text-sm font-medium text-gray-600 bg-gray-100
                rounded-lg hover:bg-gray-200 transition-colors focus:outline-none focus:ring-2 focus:ring-gray-300">
              {t('confirm.cancel')}
            </button>
            <button onClick={confirm}
              className={`px-4 py-2 text-sm font-medium text-white rounded-lg
                transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1 ${v.btnCls}`}>
              {v.btnLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
