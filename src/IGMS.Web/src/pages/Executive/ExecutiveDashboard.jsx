import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader, Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ── Helpers ───────────────────────────────────────────────────────────────────
const scoreClr = (s) =>
  s >= 80 ? { text: 'text-emerald-600', bg: 'bg-emerald-500', light: 'bg-emerald-50', border: 'border-emerald-200', label: 'ممتاز' }
: s >= 60 ? { text: 'text-amber-600',   bg: 'bg-amber-400',   light: 'bg-amber-50',   border: 'border-amber-200',   label: 'مقبول' }
:           { text: 'text-red-600',     bg: 'bg-red-500',     light: 'bg-red-50',     border: 'border-red-200',     label: 'يحتاج تحسين' }

const today = () => new Date().toLocaleDateString('ar-AE', { year: 'numeric', month: 'long', day: 'numeric', weekday: 'long' })

// ── Governance Score Circle ───────────────────────────────────────────────────
function BigGauge({ score }) {
  const r   = 52
  const c   = 2 * Math.PI * r
  const pct = Math.min(score, 100) / 100
  const clr = score >= 80 ? '#10b981' : score >= 60 ? '#f59e0b' : '#ef4444'
  return (
    <svg width="136" height="136" viewBox="0 0 136 136">
      <circle cx="68" cy="68" r={r} fill="none" stroke="#e2e8f0" strokeWidth="10" />
      <circle cx="68" cy="68" r={r} fill="none" stroke={clr} strokeWidth="10"
        strokeDasharray={`${pct * c} ${c}`} strokeLinecap="round"
        transform="rotate(-90 68 68)" />
      <text x="50%" y="44%" textAnchor="middle" dominantBaseline="central"
        fontSize="26" fontWeight="800" fill={clr}>{score}</text>
      <text x="50%" y="62%" textAnchor="middle" dominantBaseline="central"
        fontSize="10" fill="#94a3b8">/ 100</text>
    </svg>
  )
}

// ── Pillar bar ────────────────────────────────────────────────────────────────
function PillarBar({ label, pct, sublabel }) {
  const clr = pct >= 80 ? 'bg-emerald-500' : pct >= 60 ? 'bg-amber-400' : 'bg-red-500'
  return (
    <div>
      <div className="flex justify-between items-baseline mb-1">
        <span className="text-xs text-slate-300">{label}</span>
        <span className="text-xs font-bold text-white">{pct.toFixed(0)}%</span>
      </div>
      <div className="w-full bg-slate-700 rounded-full h-2">
        <div className={`h-2 rounded-full ${clr} transition-all duration-700`} style={{ width: `${pct}%` }} />
      </div>
      {sublabel && <p className="text-xs text-slate-500 mt-0.5">{sublabel}</p>}
    </div>
  )
}

// ── Alert row ─────────────────────────────────────────────────────────────────
function AlertRow({ label, count, clr, onClick }) {
  if (!count) return null
  return (
    <button onClick={onClick}
      className={`flex items-center justify-between w-full px-4 py-2.5 rounded-lg border ${clr} hover:opacity-80 transition-opacity`}>
      <span className="text-sm font-medium">{label}</span>
      <span className="text-lg font-black">{count}</span>
    </button>
  )
}

// ── Risk row ──────────────────────────────────────────────────────────────────
function RiskRow({ risk, onClick }) {
  const clr = risk.score >= 15 ? 'bg-red-600' : risk.score >= 8 ? 'bg-amber-500' : 'bg-emerald-600'
  return (
    <button onClick={onClick}
      className="flex items-center gap-3 w-full py-2.5 border-b border-gray-100 last:border-0 hover:bg-gray-50 px-1 rounded transition-colors text-right">
      <span className={`w-8 h-8 rounded-lg flex items-center justify-center text-white text-xs font-black flex-shrink-0 ${clr}`}>
        {risk.score}
      </span>
      <div className="flex-1 min-w-0 text-right">
        <p className="text-sm font-medium text-gray-800 truncate">{risk.titleAr}</p>
        <p className="text-xs text-gray-400">{risk.code}</p>
      </div>
    </button>
  )
}

// ── Department scorecard mini-bar ─────────────────────────────────────────────
function DeptBar({ dept }) {
  const clr = scoreClr(dept.score)
  return (
    <div className="flex items-center gap-3">
      <span className="text-xs text-gray-600 w-28 truncate flex-shrink-0 text-right">{dept.departmentNameAr}</span>
      <div className="flex-1 bg-gray-100 rounded-full h-4 relative">
        <div className={`h-4 rounded-full ${clr.bg} transition-all duration-700 flex items-center justify-end pe-2`}
          style={{ width: `${dept.score}%` }}>
          {dept.score > 15 && <span className="text-white text-xs font-bold">{dept.score}%</span>}
        </div>
      </div>
    </div>
  )
}

// ── Main page ─────────────────────────────────────────────────────────────────
export default function ExecutiveDashboard() {
  const navigate      = useNavigate()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const [summary,      setSummary]    = useState(null)
  const [topRisks,     setTopRisks]   = useState([])
  const [scorecard,    setScorecard]  = useState([])
  const [loading,      setLoading]    = useState(true)
  const [pdfLoading,   setPdfLoading] = useState(false)
  const printRef = useRef(null)

  useEffect(() => {
    if (!hasPermission('REPORTS.READ')) { navigate('/dashboard', { replace: true }); return }
    Promise.all([
      api.get('/api/v1/reports/summary'),
      api.get('/api/v1/reports/top-risks'),
      api.get('/api/v1/reports/department-scorecard'),
    ]).then(([s, r, sc]) => {
      setSummary(s.data.data)
      setTopRisks(r.data.data ?? [])
      setScorecard(sc.data.data ?? [])
    }).finally(() => setLoading(false))
  }, [])

  if (loading) return <PageLoader />
  if (!summary) return null

  const { policies, risks, tasks, kpis, governanceScore } = summary
  const clr = scoreClr(governanceScore)

  // Pillar percentages
  const polPct = policies.total > 0 ? policies.active / policies.total * 100 : 0
  const rskPct = risks.total    > 0 ? (risks.mitigated + risks.closed) / risks.total * 100 : 100
  const tskPct = tasks.total    > 0 ? Math.max(0, tasks.done / tasks.total * 100 - tasks.overdue / tasks.total * 50) : 100
  const kpiPct = kpis.total     > 0 ? kpis.onTrack / kpis.total * 100 : 100

  const handlePrint = () => window.print()

  const handlePdf = async () => {
    setPdfLoading(true)
    try {
      const r = await api.get('/api/v1/reports/executive-pdf', { responseType: 'blob' })
      const url  = URL.createObjectURL(new Blob([r.data], { type: 'application/pdf' }))
      const link = document.createElement('a')
      link.href = url
      link.download = `executive_report_${new Date().toISOString().slice(0,10)}.pdf`
      document.body.appendChild(link); link.click()
      document.body.removeChild(link); URL.revokeObjectURL(url)
    } catch { /* silent */ } finally { setPdfLoading(false) }
  }

  return (
    <div className="space-y-5 max-w-6xl" ref={printRef}>

      {/* Header */}
      <div className="flex items-start justify-between flex-wrap gap-3">
        <div>
          <p className="text-xs text-gray-400 mb-1">{today()}</p>
          <h1 className="text-2xl font-black text-gray-900">لوحة المدير التنفيذي</h1>
          <p className="text-sm text-gray-500 mt-0.5">ملخص تنفيذي لصحة الحوكمة المؤسسية</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => navigate('/reports/department-scorecard')}
            className="flex items-center gap-2 px-3 py-2 text-sm border border-gray-200 text-gray-600 rounded-lg hover:bg-gray-50">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="2" y="3" width="6" height="18" rx="1"/><rect x="9" y="8" width="6" height="13" rx="1"/><rect x="16" y="5" width="6" height="16" rx="1"/>
            </svg>
            بطاقة الأقسام
          </button>
          <button onClick={handlePdf} disabled={pdfLoading}
            className="flex items-center gap-2 px-3 py-2 text-sm bg-red-700 text-white rounded-lg hover:bg-red-800 disabled:opacity-60">
            {pdfLoading
              ? <Spinner size="sm" />
              : <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/>
                  <polyline points="14 2 14 8 20 8"/>
                  <line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>
                  <polyline points="10 9 9 9 8 9"/>
                </svg>
            }
            تصدير PDF
          </button>
          <button onClick={handlePrint}
            className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-900 text-white rounded-lg hover:bg-gray-800">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 01-2-2v-5a2 2 0 012-2h16a2 2 0 012 2v5a2 2 0 01-2 2h-2"/>
              <rect x="6" y="14" width="12" height="8"/>
            </svg>
            طباعة
          </button>
        </div>
      </div>

      {/* Row 1: Score + Pillars + Alerts */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">

        {/* Governance Score */}
        <div className="bg-gradient-to-br from-slate-900 to-slate-700 rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <BigGauge score={governanceScore} />
          <div className="text-center">
            <p className="text-xs text-slate-400 font-medium uppercase tracking-wider">درجة الحوكمة</p>
            <span className={`text-sm font-bold px-3 py-0.5 rounded-full mt-1 inline-block
              ${governanceScore >= 80 ? 'bg-emerald-900 text-emerald-300'
              : governanceScore >= 60 ? 'bg-amber-900 text-amber-300'
              : 'bg-red-900 text-red-300'}`}>
              {clr.label}
            </span>
          </div>
          <div className="w-full space-y-3 mt-2">
            <PillarBar label="صحة السياسات"  pct={polPct} sublabel={`${policies.active} نشطة / ${policies.total}`} />
            <PillarBar label="إدارة المخاطر" pct={rskPct} sublabel={`${risks.open} مفتوحة`} />
            <PillarBar label="إنجاز المهام"  pct={tskPct} sublabel={tasks.overdue > 0 ? `${tasks.overdue} متأخرة` : null} />
            <PillarBar label="مؤشرات الأداء" pct={kpiPct} sublabel={`${kpis.onTrack} في المسار / ${kpis.total}`} />
          </div>
        </div>

        {/* KPI Stats */}
        <div className="space-y-3">
          <h2 className="text-xs font-bold text-gray-500 uppercase tracking-wider">الإحصائيات الكلية</h2>
          {[
            { label: 'السياسات النشطة',   val: policies.active,     total: policies.total,    path: '/policies', clr: 'text-emerald-600' },
            { label: 'المخاطر المفتوحة',  val: risks.open,          total: risks.total,       path: '/risks',    clr: 'text-red-600'     },
            { label: 'المهام المتأخرة',   val: tasks.overdue,       total: tasks.total,       path: '/tasks',    clr: 'text-amber-600'   },
            { label: 'KPI في المسار',     val: kpis.onTrack,        total: kpis.total,        path: '/kpi',      clr: 'text-blue-600'    },
          ].map((s, i) => (
            <button key={i} onClick={() => navigate(s.path)}
              className="w-full bg-white rounded-xl border border-gray-200 px-4 py-3 flex items-center justify-between hover:border-blue-200 hover:shadow-sm transition-all">
              <span className="text-sm text-gray-600">{s.label}</span>
              <div className="flex items-baseline gap-1">
                <span className={`text-2xl font-black ${s.clr}`}>{s.val}</span>
                <span className="text-xs text-gray-400">/ {s.total}</span>
              </div>
            </button>
          ))}
        </div>

        {/* Alert panel */}
        <div className="space-y-3">
          <h2 className="text-xs font-bold text-gray-500 uppercase tracking-wider">تنبيهات فورية</h2>
          <div className="space-y-2">
            <AlertRow
              label="سياسات تنتهي خلال 30 يوم"
              count={policies.expiringIn30Days}
              clr="bg-red-50 border-red-200 text-red-700"
              onClick={() => navigate('/policies')} />
            <AlertRow
              label="سياسات تنتهي خلال 60 يوم"
              count={policies.expiringIn60Days}
              clr="bg-amber-50 border-amber-200 text-amber-700"
              onClick={() => navigate('/policies')} />
            <AlertRow
              label="مهام متأخرة"
              count={tasks.overdue}
              clr="bg-red-50 border-red-200 text-red-700"
              onClick={() => navigate('/tasks')} />
            <AlertRow
              label="مخاطر حرجة (≥ 15)"
              count={risks.highRisk}
              clr="bg-red-50 border-red-200 text-red-700"
              onClick={() => navigate('/risks')} />
            <AlertRow
              label="مؤشرات متأخرة عن الهدف"
              count={kpis.behind}
              clr="bg-amber-50 border-amber-200 text-amber-700"
              onClick={() => navigate('/kpi')} />
          </div>
          {/* All-clear */}
          {!policies.expiringIn30Days && !tasks.overdue && !risks.highRisk && !kpis.behind && (
            <div className="bg-emerald-50 border border-emerald-200 rounded-lg p-4 text-center">
              <p className="text-sm font-medium text-emerald-700">لا تنبيهات عاجلة</p>
              <p className="text-xs text-emerald-500 mt-0.5">الحوكمة في وضع جيد</p>
            </div>
          )}
        </div>
      </div>

      {/* Row 2: Top Risks + Department Scorecard */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

        {/* Top Critical Risks */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-bold text-gray-700">أعلى المخاطر خطورةً</h2>
            <button onClick={() => navigate('/risks/heatmap')}
              className="text-xs text-blue-600 hover:underline">خريطة الحرارة ←</button>
          </div>
          {topRisks.length === 0
            ? <p className="text-sm text-gray-400 text-center py-4">لا توجد مخاطر مفتوحة</p>
            : topRisks.map((r) => (
                <RiskRow key={r.id} risk={r} onClick={() => navigate(`/risks/${r.id}`)} />
              ))
          }
        </div>

        {/* Department Scorecard summary */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-bold text-gray-700">أداء الأقسام – KPI</h2>
            <button onClick={() => navigate('/reports/department-scorecard')}
              className="text-xs text-blue-600 hover:underline">تفاصيل ←</button>
          </div>
          {scorecard.length === 0
            ? <p className="text-sm text-gray-400 text-center py-4">لا توجد مؤشرات مسجّلة</p>
            : <div className="space-y-3">
                {scorecard.slice(0, 6).map((d, i) => (
                  <DeptBar key={i} dept={d} />
                ))}
              </div>
          }
        </div>
      </div>

      {/* Footer */}
      <p className="text-center text-xs text-gray-300 py-2 print:block hidden">
        تم إنشاء هذا التقرير تلقائياً من نظام إدارة الحوكمة المؤسسية IGMS – {today()}
      </p>

    </div>
  )
}
