import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ── helpers ───────────────────────────────────────────────────────────────────
const scoreColor = (s) =>
  s >= 80 ? { bar: 'bg-emerald-500', text: 'text-emerald-700', ring: 'ring-emerald-200', bg: 'bg-emerald-50', badge: 'bg-emerald-100 text-emerald-700' }
: s >= 60 ? { bar: 'bg-amber-400',   text: 'text-amber-700',   ring: 'ring-amber-200',   bg: 'bg-amber-50',   badge: 'bg-amber-100 text-amber-700'   }
:           { bar: 'bg-red-500',     text: 'text-red-700',     ring: 'ring-red-200',     bg: 'bg-red-50',     badge: 'bg-red-100 text-red-600'       }

const statusDot = (s) =>
  s === 0 ? 'bg-emerald-500' : s === 1 ? 'bg-amber-400' : 'bg-red-500'

const STATUS_LBL = { 0: 'في المسار', 1: 'في خطر', 2: 'متأخر' }

// ── Circular score gauge ──────────────────────────────────────────────────────
function ScoreGauge({ score, size = 72 }) {
  const r   = (size - 8) / 2
  const c   = 2 * Math.PI * r
  const pct = Math.min(score, 100) / 100
  const clr = score >= 80 ? '#10b981' : score >= 60 ? '#f59e0b' : '#ef4444'
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="flex-shrink-0">
      <circle cx={size/2} cy={size/2} r={r} fill="none" stroke="#f1f5f9" strokeWidth="6" />
      <circle cx={size/2} cy={size/2} r={r} fill="none" stroke={clr} strokeWidth="6"
        strokeDasharray={`${pct * c} ${c}`}
        strokeLinecap="round"
        transform={`rotate(-90 ${size/2} ${size/2})`} />
      <text x="50%" y="50%" textAnchor="middle" dominantBaseline="central"
        fontSize="15" fontWeight="700" fill={clr}>{score}</text>
    </svg>
  )
}

// ── KPI mini bar ──────────────────────────────────────────────────────────────
function KpiBar({ kpi, onClick }) {
  const pct  = Math.min(kpi.achievementPct, 100)
  const over = kpi.achievementPct > 100
  const clr  = kpi.status === 0 ? 'bg-emerald-500' : kpi.status === 1 ? 'bg-amber-400' : 'bg-red-400'
  return (
    <div className="flex items-center gap-3 cursor-pointer hover:bg-gray-50 px-2 py-1.5 rounded-lg -mx-2 transition-colors"
      onClick={onClick}>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between mb-0.5">
          <span className="text-xs text-gray-700 truncate font-medium">{kpi.titleAr}</span>
          <span className={`text-xs font-bold ms-2 flex-shrink-0 ${
            kpi.status === 0 ? 'text-emerald-600' : kpi.status === 1 ? 'text-amber-600' : 'text-red-600'
          }`}>
            {kpi.achievementPct.toFixed(0)}%{over && ' ↑'}
          </span>
        </div>
        <div className="w-full bg-gray-100 rounded-full h-1.5">
          <div className={`h-1.5 rounded-full transition-all duration-500 ${clr}`}
            style={{ width: `${pct}%` }} />
        </div>
      </div>
      <span className={`text-xs px-1.5 py-0.5 rounded-full flex-shrink-0 ${
        kpi.status === 0 ? 'bg-emerald-100 text-emerald-700'
      : kpi.status === 1 ? 'bg-amber-100 text-amber-700'
      : 'bg-red-100 text-red-600'}`}>
        {STATUS_LBL[kpi.status]}
      </span>
    </div>
  )
}

// ── Comparison bar chart ──────────────────────────────────────────────────────
function ComparisonChart({ data }) {
  const max = Math.max(...data.map(d => d.score), 1)
  return (
    <div className="space-y-3">
      {data.map((d, i) => {
        const clr = scoreColor(d.score)
        return (
          <div key={i} className="flex items-center gap-3">
            <span className="text-xs text-gray-600 w-32 text-left truncate flex-shrink-0">{d.departmentNameAr}</span>
            <div className="flex-1 bg-gray-100 rounded-full h-5 relative">
              <div className={`h-5 rounded-full transition-all duration-700 ${clr.bar} flex items-center justify-end pe-2`}
                style={{ width: `${(d.score / max) * 100}%` }}>
                <span className="text-white text-xs font-bold">{d.score > 10 ? d.score : ''}</span>
              </div>
              {d.score <= 10 && (
                <span className="absolute right-0 top-0 h-5 flex items-center pe-1 text-xs text-gray-500">{d.score}</span>
              )}
            </div>
            <span className="text-xs text-gray-400 w-12 text-left">{d.kpiCount} مؤشر</span>
          </div>
        )
      })}
    </div>
  )
}

// ── Main page ─────────────────────────────────────────────────────────────────
export default function DepartmentScorecard() {
  const navigate    = useNavigate()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canView     = hasPermission('REPORTS.READ')

  const currentYear = new Date().getFullYear()
  const [year,    setYear]    = useState(currentYear)
  const [data,    setData]    = useState([])
  const [loading, setLoading] = useState(true)
  const [expanded, setExpanded] = useState(null)

  useEffect(() => {
    if (!canView) { navigate('/dashboard', { replace: true }); return }
  }, [])

  useEffect(() => {
    setLoading(true)
    api.get('/api/v1/reports/department-scorecard', { params: { year } })
      .then((r) => setData(r.data?.data ?? []))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [year])

  if (loading) return <PageLoader />

  const orgScore = data.length > 0
    ? Math.round(data.reduce((s, d) => s + d.score, 0) / data.length)
    : 0
  const orgClr   = scoreColor(orgScore)
  const years    = [currentYear, currentYear - 1, currentYear - 2]

  return (
    <div className="space-y-6 max-w-5xl">

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <button onClick={() => navigate('/reports')}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-1">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
            التقارير
          </button>
          <h1 className="text-xl font-bold text-gray-800">بطاقة أداء الأقسام</h1>
          <p className="text-xs text-gray-400 mt-0.5">مؤشرات KPI مجمّعة حسب القسم</p>
        </div>
        <select value={year} onChange={(e) => setYear(Number(e.target.value))}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500">
          {years.map((y) => <option key={y} value={y}>{y}</option>)}
        </select>
      </div>

      {data.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا توجد مؤشرات KPI مسجّلة لسنة {year}</p>
          <button onClick={() => navigate('/kpi/new')}
            className="mt-3 text-sm text-blue-600 hover:underline">إضافة مؤشر</button>
        </div>
      ) : (<>

        {/* Org summary row */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {/* Org score */}
          <div className={`bg-white rounded-xl border-2 ${orgClr.ring.replace('ring','border')} p-4 flex items-center gap-4 col-span-2 md:col-span-1`}>
            <ScoreGauge score={orgScore} size={72} />
            <div>
              <p className="text-xs text-gray-400">درجة الحوكمة الكلية</p>
              <p className={`text-lg font-bold ${orgClr.text}`}>
                {orgScore >= 80 ? 'ممتاز' : orgScore >= 60 ? 'مقبول' : 'يحتاج تحسين'}
              </p>
              <p className="text-xs text-gray-400">{data.length} قسم · {year}</p>
            </div>
          </div>

          {/* Stats */}
          {[
            { label: 'في المسار',  val: data.reduce((s, d) => s + d.onTrackCount, 0),  cls: 'text-emerald-600' },
            { label: 'في خطر',     val: data.reduce((s, d) => s + d.atRiskCount,  0),  cls: 'text-amber-600'   },
            { label: 'متأخر',      val: data.reduce((s, d) => s + d.behindCount,  0),  cls: 'text-red-600'     },
          ].map((s, i) => (
            <div key={i} className="bg-white rounded-xl border border-gray-200 p-4 flex flex-col justify-center">
              <p className={`text-2xl font-black ${s.cls}`}>{s.val}</p>
              <p className="text-xs text-gray-400 mt-0.5">مؤشر {s.label}</p>
            </div>
          ))}
        </div>

        {/* Comparison chart */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-bold text-gray-700 mb-4">مقارنة درجات الأقسام</h2>
          <ComparisonChart data={data} />
        </div>

        {/* Department cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {data.map((dept, i) => {
            const clr      = scoreColor(dept.score)
            const isOpen   = expanded === i
            return (
              <div key={i} className={`bg-white rounded-xl border-2 transition-all ${isOpen ? 'border-blue-300' : 'border-gray-200'}`}>
                {/* Card header */}
                <div className="p-4 flex items-center gap-4 cursor-pointer"
                  onClick={() => setExpanded(isOpen ? null : i)}>
                  <ScoreGauge score={dept.score} size={60} />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <h3 className="font-bold text-gray-800 truncate">{dept.departmentNameAr}</h3>
                      <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${clr.badge}`}>
                        {dept.score >= 80 ? 'ممتاز' : dept.score >= 60 ? 'مقبول' : 'ضعيف'}
                      </span>
                    </div>
                    {/* Status dots */}
                    <div className="flex items-center gap-3 mt-2">
                      {[
                        { count: dept.onTrackCount, label: 'في المسار', cls: 'bg-emerald-500' },
                        { count: dept.atRiskCount,  label: 'في خطر',   cls: 'bg-amber-400'   },
                        { count: dept.behindCount,  label: 'متأخر',    cls: 'bg-red-400'     },
                      ].filter(s => s.count > 0).map((s, j) => (
                        <div key={j} className="flex items-center gap-1">
                          <div className={`w-2 h-2 rounded-full ${s.cls}`} />
                          <span className="text-xs text-gray-500">{s.count} {s.label}</span>
                        </div>
                      ))}
                    </div>
                    {/* Mini progress bar */}
                    <div className="mt-2 flex gap-0.5 h-1.5 rounded-full overflow-hidden">
                      {dept.kpiCount > 0 && (<>
                        <div className="bg-emerald-500 transition-all"
                          style={{ width: `${dept.onTrackCount / dept.kpiCount * 100}%` }} />
                        <div className="bg-amber-400 transition-all"
                          style={{ width: `${dept.atRiskCount  / dept.kpiCount * 100}%` }} />
                        <div className="bg-red-400 transition-all"
                          style={{ width: `${dept.behindCount  / dept.kpiCount * 100}%` }} />
                      </>)}
                    </div>
                  </div>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#9ca3af" strokeWidth="2"
                    className={`flex-shrink-0 transition-transform ${isOpen ? 'rotate-180' : ''}`}>
                    <polyline points="6 9 12 15 18 9"/>
                  </svg>
                </div>

                {/* Expanded KPI list */}
                {isOpen && (
                  <div className="border-t border-gray-100 px-4 py-3 space-y-1">
                    <p className="text-xs text-gray-400 mb-2 font-medium">المؤشرات ({dept.kpiCount})</p>
                    {dept.kpis.map((kpi) => (
                      <KpiBar key={kpi.id} kpi={kpi} onClick={() => navigate(`/kpi/${kpi.id}`)} />
                    ))}
                    <div className="pt-2 border-t border-gray-50 flex justify-between items-center">
                      <span className="text-xs text-gray-400">متوسط الإنجاز</span>
                      <span className={`text-sm font-bold ${clr.text}`}>{dept.avgAchievementPct.toFixed(1)}%</span>
                    </div>
                  </div>
                )}
              </div>
            )
          })}
        </div>

      </>)}
    </div>
  )
}
