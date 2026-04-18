/**
 * RiskLifecycle – مخطط دورة حياة المخاطرة
 * 4 مراحل: اكتشاف → تقييم → تخفيف → إغلاق
 */

const fmt = (d) => d ? new Date(d).toLocaleDateString('ar-AE', { year: 'numeric', month: 'short', day: 'numeric' }) : null

// ── RiskStatus enum (mirrors backend) ────────────────────────────────────────
// 0 = Open, 1 = Mitigated, 2 = Closed

function StageNode({ icon, label, date, sublabel, state, isLast }) {
  const dotCls = {
    done:    'bg-emerald-500 border-emerald-500 text-white',
    current: 'bg-blue-600 border-blue-600 text-white ring-4 ring-blue-100',
    warning: 'bg-amber-500 border-amber-500 text-white ring-4 ring-amber-100',
    future:  'bg-white border-gray-300 text-gray-400',
  }[state]

  const labelCls = {
    done:    'text-gray-700',
    current: 'text-blue-700 font-bold',
    warning: 'text-amber-700 font-bold',
    future:  'text-gray-400',
  }[state]

  const dateCls = {
    done:    'text-emerald-600',
    current: 'text-blue-600',
    warning: 'text-amber-600',
    future:  'text-gray-300',
  }[state]

  const lineCls = state === 'done' ? 'bg-emerald-400' : 'bg-gray-200'

  return (
    <div className="flex items-start flex-1 min-w-0">
      <div className="flex flex-col items-center flex-1 min-w-0">
        <div className={`w-10 h-10 rounded-full border-2 flex items-center justify-center
          text-sm flex-shrink-0 transition-all ${dotCls}`}>
          {state === 'done'
            ? <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M20 6L9 17l-5-5"/></svg>
            : <span>{icon}</span>}
        </div>
        <p className={`text-xs font-semibold mt-2 text-center leading-tight ${labelCls}`}>{label}</p>
        {date
          ? <p className={`text-xs mt-1 text-center ${dateCls}`}>{date}</p>
          : <p className="text-xs mt-1 text-gray-300">—</p>}
        {sublabel && <p className="text-xs text-gray-400 mt-0.5 text-center leading-tight">{sublabel}</p>}
      </div>
      {!isLast && (
        <div className="flex-shrink-0 w-8 mt-5">
          <div className={`h-0.5 w-full ${lineCls}`} />
        </div>
      )}
    </div>
  )
}

export default function RiskLifecycle({ risk }) {
  const isOpen      = risk.status === 0
  const isMitigated = risk.status === 1
  const isClosed    = risk.status === 2

  const score = (risk.likelihood ?? 0) * (risk.impact ?? 0)
  const isHighRisk = score >= 15

  // Stage states
  const s1 = 'done'                                           // اكتشاف – always done
  const s2 = isClosed || isMitigated ? 'done' : 'current'    // تقييم
  const s3 = isClosed    ? 'done'
           : isMitigated ? (isHighRisk ? 'warning' : 'current')
           : 'future'                                         // تخفيف
  const s4 = isClosed ? 'done' : 'future'                    // إغلاق

  const stages = [
    {
      icon: '🔍',
      label: 'اكتشاف',
      date: fmt(risk.createdAt),
      sublabel: risk.code,
      state: s1,
    },
    {
      icon: '📊',
      label: 'تقييم',
      date: null,
      sublabel: score > 0 ? `${risk.likelihood}×${risk.impact} = ${score}` : '—',
      state: s2,
    },
    {
      icon: '🛡️',
      label: 'تخفيف',
      date: null,
      sublabel: isMitigated ? 'جارٍ التخفيف' : isClosed ? 'مكتمل' : null,
      state: s3,
    },
    {
      icon: '✅',
      label: 'إغلاق',
      date: null,
      sublabel: isClosed ? 'تم الإغلاق' : null,
      state: s4,
    },
  ]

  const stepsCompleted = [s1, s2, s3, s4].filter(s => s === 'done').length
  const pct = (stepsCompleted / 4) * 100
  const barColor = isClosed    ? 'bg-emerald-500'
                 : isMitigated ? (isHighRisk ? 'bg-amber-500' : 'bg-blue-500')
                 : isHighRisk  ? 'bg-red-500'
                 : 'bg-blue-500'

  const STATUS_LABEL = { 0: 'مفتوحة', 1: 'مُخففة', 2: 'مغلقة' }
  const STATUS_CLS   = {
    0: isHighRisk ? 'bg-red-100 text-red-700' : 'bg-amber-100 text-amber-700',
    1: 'bg-blue-100 text-blue-700',
    2: 'bg-emerald-100 text-emerald-700',
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-2">
          <h2 className="text-sm font-bold text-gray-700">دورة حياة المخاطرة</h2>
          <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${STATUS_CLS[risk.status]}`}>
            {STATUS_LABEL[risk.status]}
          </span>
        </div>
        <div className="flex items-center gap-3">
          <div className="w-28 bg-gray-100 rounded-full h-1.5">
            <div className={`h-1.5 rounded-full transition-all duration-700 ${barColor}`}
              style={{ width: `${pct}%` }} />
          </div>
          <span className="text-xs text-gray-400">{stepsCompleted}/4 مراحل</span>
        </div>
      </div>

      {/* Stages */}
      <div className="flex items-start">
        {stages.map((s, i) => (
          <StageNode key={i} {...s} isLast={i === stages.length - 1} />
        ))}
      </div>

      {/* High risk warning */}
      {isOpen && isHighRisk && (
        <div className="mt-4 flex items-center gap-2 bg-red-50 border border-red-200 rounded-lg px-4 py-2.5 text-sm">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#dc2626" strokeWidth="2">
            <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z M12 9v4 M12 17h.01"/>
          </svg>
          <span className="text-red-700 font-medium">
            مخاطرة حرجة (درجة {score}) – يجب اتخاذ إجراء تخفيف عاجل.
          </span>
        </div>
      )}
      {isMitigated && isHighRisk && (
        <div className="mt-4 flex items-center gap-2 bg-amber-50 border border-amber-200 rounded-lg px-4 py-2.5 text-sm">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#d97706" strokeWidth="2">
            <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <span className="text-amber-700 font-medium">
            التخفيف جارٍ – تأكد من اكتمال خطة التخفيف قبل الإغلاق.
          </span>
        </div>
      )}
    </div>
  )
}
