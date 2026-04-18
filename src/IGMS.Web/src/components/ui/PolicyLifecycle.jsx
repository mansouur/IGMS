/**
 * PolicyLifecycle – مخطط دورة حياة السياسة
 * يعرض المراحل الخمس أفقياً مع التواريخ والحالة الحالية.
 */

const fmt = (d) => d ? new Date(d).toLocaleDateString('ar-AE', { year: 'numeric', month: 'short', day: 'numeric' }) : null
const fmtLong = (d) => d ? new Date(d).toLocaleDateString('ar-AE', { year: 'numeric', month: 'long', day: 'numeric' }) : null

// ── Single stage node ─────────────────────────────────────────────────────────
function StageNode({ icon, label, date, sublabel, state, isLast, warning }) {
  // state: 'done' | 'current' | 'future' | 'warning'
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
        {/* Node circle */}
        <div className={`w-10 h-10 rounded-full border-2 flex items-center justify-center
          text-sm flex-shrink-0 transition-all ${dotCls}`}>
          {state === 'done'
            ? <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M20 6L9 17l-5-5"/></svg>
            : <span>{icon}</span>}
        </div>
        {/* Label */}
        <p className={`text-xs font-semibold mt-2 text-center leading-tight ${labelCls}`}>{label}</p>
        {/* Date */}
        {date
          ? <p className={`text-xs mt-1 text-center ${dateCls}`}>{date}</p>
          : <p className="text-xs mt-1 text-gray-300">—</p>}
        {/* Sub-label */}
        {sublabel && <p className="text-xs text-gray-400 mt-0.5 text-center leading-tight">{sublabel}</p>}
        {/* Warning badge */}
        {warning && (
          <span className="mt-1.5 text-xs bg-amber-100 text-amber-700 border border-amber-200 px-2 py-0.5 rounded-full">
            {warning}
          </span>
        )}
      </div>
      {/* Connector line */}
      {!isLast && (
        <div className="flex-shrink-0 w-8 mt-5">
          <div className={`h-0.5 w-full ${lineCls}`} />
        </div>
      )}
    </div>
  )
}

// ── Main component ────────────────────────────────────────────────────────────
export default function PolicyLifecycle({ policy }) {
  const now      = new Date()
  const expiry   = policy.expiryDate   ? new Date(policy.expiryDate)   : null
  const effective= policy.effectiveDate? new Date(policy.effectiveDate) : null

  const isExpired       = expiry && expiry < now
  const isExpiringSoon  = expiry && !isExpired && (expiry - now) / 86400000 <= 30
  const isEffective     = effective && effective <= now
  const isActive        = policy.status === 1
  const isArchived      = policy.status === 2
  const isDraft         = policy.status === 0

  // ── Stage states ──────────────────────────────────────────────────────────

  // 1. إنشاء – always done
  const s1 = 'done'

  // 2. مسودة – done when not draft anymore; current when still draft
  const s2 = isDraft ? 'current' : 'done'

  // 3. اعتماد ونشر – done/current when active or archived
  const s3 = isArchived ? 'done' : isActive ? 'current' : 'future'

  // 4. سريان فعلي – done when effective date passed; warning if expiring soon
  const s4 = isArchived
    ? 'done'
    : isActive && isEffective
      ? (isExpiringSoon ? 'warning' : 'done')
      : isActive && !isEffective
        ? 'current'
        : 'future'

  // 5. انتهاء / تجديد – done/warning when archived; warning when expiring soon; future otherwise
  const s5 = isArchived
    ? 'done'
    : isExpired
      ? 'warning'
      : isExpiringSoon
        ? 'warning'
        : 'future'

  const stages = [
    {
      icon: '✏️',
      label: 'إنشاء',
      date: fmt(policy.createdAt),
      state: s1,
    },
    {
      icon: '📋',
      label: 'مراجعة',
      date: isDraft ? 'الحالة الراهنة' : null,
      sublabel: isDraft ? null : 'مكتملة',
      state: s2,
    },
    {
      icon: '✅',
      label: 'اعتماد ونشر',
      date: policy.approvedAt ? fmt(policy.approvedAt) : null,
      sublabel: policy.approverNameAr ? `المعتمد: ${policy.approverNameAr}` : null,
      state: s3,
    },
    {
      icon: '📅',
      label: 'سريان فعلي',
      date: policy.effectiveDate ? fmt(policy.effectiveDate) : null,
      state: s4,
      warning: isExpiringSoon && isActive
        ? `تنتهي خلال ${Math.ceil((expiry - now) / 86400000)} يوم`
        : null,
    },
    {
      icon: isArchived ? '🔄' : '⏳',
      label: isArchived ? 'مؤرشفة / مجدَّدة' : 'انتهاء الصلاحية',
      date: policy.expiryDate ? fmt(policy.expiryDate) : null,
      sublabel: isExpired && !isArchived ? 'انتهت الصلاحية' : isArchived ? 'تم الأرشفة' : null,
      state: s5,
    },
  ]

  // ── Health bar ──────────────────────────────────────────────────────────────
  const stepsCompleted = [s1, s2, s3, s4, s5].filter(s => s === 'done').length
  const pct = (stepsCompleted / 5) * 100
  const barColor = isArchived
    ? 'bg-gray-400'
    : isExpired ? 'bg-red-500'
    : isExpiringSoon ? 'bg-amber-500'
    : 'bg-emerald-500'

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      {/* Title + progress bar */}
      <div className="flex items-center justify-between mb-5">
        <h2 className="text-sm font-bold text-gray-700">دورة حياة السياسة</h2>
        <div className="flex items-center gap-3">
          <div className="w-32 bg-gray-100 rounded-full h-1.5">
            <div className={`h-1.5 rounded-full transition-all duration-700 ${barColor}`}
              style={{ width: `${pct}%` }} />
          </div>
          <span className="text-xs text-gray-400">{stepsCompleted}/5 مراحل</span>
        </div>
      </div>

      {/* Stages */}
      <div className="flex items-start">
        {stages.map((s, i) => (
          <StageNode key={i} {...s} isLast={i === stages.length - 1} />
        ))}
      </div>

      {/* Expiry warning banner */}
      {isActive && isExpiringSoon && !isExpired && (
        <div className="mt-4 flex items-center gap-2 bg-amber-50 border border-amber-200 rounded-lg px-4 py-2.5 text-sm">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#d97706" strokeWidth="2">
            <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z M12 9v4 M12 17h.01"/>
          </svg>
          <span className="text-amber-700 font-medium">
            تنتهي صلاحية هذه السياسة في {fmtLong(policy.expiryDate)} –
            يُنصح بالتجديد قبل انتهاء الموعد.
          </span>
        </div>
      )}
      {isActive && isExpired && (
        <div className="mt-4 flex items-center gap-2 bg-red-50 border border-red-200 rounded-lg px-4 py-2.5 text-sm">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#dc2626" strokeWidth="2">
            <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <span className="text-red-700 font-medium">
            انتهت صلاحية هذه السياسة في {fmtLong(policy.expiryDate)} – يجب تجديدها أو أرشفتها فوراً.
          </span>
        </div>
      )}
    </div>
  )
}
