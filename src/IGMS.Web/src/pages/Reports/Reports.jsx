import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import {
  PieChart, Pie, Cell, Tooltip, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
} from 'recharts'
import api from '../../services/api'
import useAuthStore from '../../store/authStore'

// ── Shared colours ────────────────────────────────────────────────────────────
const C = {
  green:  '#10b981', amber:  '#f59e0b', red:    '#ef4444',
  blue:   '#3b82f6', purple: '#8b5cf6', gray:   '#94a3b8',
  slate:  '#64748b', indigo: '#6366f1',
}

// ── Governance Score SVG gauge ────────────────────────────────────────────────
function GovernanceGauge({ score }) {
  const r = 68
  const circ = 2 * Math.PI * r
  const filled = (score / 100) * circ
  const color = score >= 80 ? C.green : score >= 60 ? C.amber : C.red
  const label = score >= 80 ? 'ممتاز' : score >= 60 ? 'جيد' : 'يحتاج تحسين'

  return (
    <div className="flex flex-col items-center">
      <svg viewBox="0 0 160 160" className="w-44 h-44">
        <circle cx="80" cy="80" r={r} fill="none" stroke="#f1f5f9" strokeWidth="14" />
        <circle cx="80" cy="80" r={r} fill="none" stroke={color} strokeWidth="14"
          strokeDasharray={`${filled} ${circ}`} strokeLinecap="round"
          transform="rotate(-90 80 80)"
          style={{ transition: 'stroke-dasharray 1s ease' }} />
        <text x="80" y="72" textAnchor="middle" fontSize="32" fontWeight="800" fill={color}>{score}</text>
        <text x="80" y="92" textAnchor="middle" fontSize="12" fill="#9ca3af">/ 100</text>
      </svg>
      <span className="text-xs font-semibold mt-1" style={{ color }}>{label}</span>
    </div>
  )
}

// ── Pillar score bar ──────────────────────────────────────────────────────────
function PillarBar({ label, pct, color }) {
  return (
    <div>
      <div className="flex justify-between text-xs mb-1">
        <span className="text-gray-600 font-medium">{label}</span>
        <span className="font-bold" style={{ color }}>{Math.round(pct)}%</span>
      </div>
      <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
        <div className="h-2 rounded-full transition-all duration-700"
          style={{ width: `${Math.min(pct, 100)}%`, background: color }} />
      </div>
    </div>
  )
}

// ── Donut chart ───────────────────────────────────────────────────────────────
function Donut({ data, colors, centerLabel, centerValue }) {
  return (
    <div className="relative">
      <ResponsiveContainer width="100%" height={180}>
        <PieChart>
          <Pie data={data} cx="50%" cy="50%" innerRadius={52} outerRadius={72}
            paddingAngle={3} dataKey="value">
            {data.map((d, i) => <Cell key={i} fill={colors[i]} />)}
          </Pie>
          <Tooltip formatter={(v, n) => [v, n]} />
        </PieChart>
      </ResponsiveContainer>
      {centerLabel != null && (
        <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
          <span className="text-2xl font-black text-gray-800">{centerValue}</span>
          <span className="text-xs text-gray-400">{centerLabel}</span>
        </div>
      )}
    </div>
  )
}



// ── KPI Achievement radial ────────────────────────────────────────────────────
function KpiGauge({ pct }) {
  const r = 36
  const circ = 2 * Math.PI * r
  const filled = (Math.min(pct, 100) / 100) * circ
  const color = pct >= 90 ? C.green : pct >= 70 ? C.amber : C.red
  return (
    <svg viewBox="0 0 88 88" className="w-24 h-24 mx-auto">
      <circle cx="44" cy="44" r={r} fill="none" stroke="#f1f5f9" strokeWidth="8" />
      <circle cx="44" cy="44" r={r} fill="none" stroke={color} strokeWidth="8"
        strokeDasharray={`${filled} ${circ}`} strokeLinecap="round"
        transform="rotate(-90 44 44)" />
      <text x="44" y="48" textAnchor="middle" fontSize="14" fontWeight="800" fill={color}>{Math.round(pct)}%</text>
    </svg>
  )
}

// ── Card wrapper ──────────────────────────────────────────────────────────────
function Card({ title, children, className = '' }) {
  return (
    <div className={`bg-white rounded-2xl border border-gray-100 shadow-sm p-5 ${className}`}>
      {title && <h3 className="text-sm font-bold text-gray-600 mb-4 flex items-center gap-2">{title}</h3>}
      {children}
    </div>
  )
}

// ── Custom tooltip ────────────────────────────────────────────────────────────
const Tip = ({ active, payload }) => {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-white border border-gray-200 rounded-xl px-3 py-2 shadow-lg text-sm">
      <p className="font-semibold text-gray-800">{payload[0].name}</p>
      <p className="text-gray-500">{payload[0].value}</p>
    </div>
  )
}

// ── Skeleton ──────────────────────────────────────────────────────────────────
function Skeleton() {
  return (
    <div className="space-y-4 animate-pulse max-w-6xl">
      <div className="h-8 bg-gray-100 rounded w-1/4" />
      <div className="grid grid-cols-3 gap-4">
        {[...Array(3)].map((_, i) => <div key={i} className="h-56 bg-gray-100 rounded-2xl" />)}
      </div>
      <div className="grid grid-cols-2 gap-4">
        {[...Array(2)].map((_, i) => <div key={i} className="h-48 bg-gray-100 rounded-2xl" />)}
      </div>
    </div>
  )
}

// ── Reports Page ──────────────────────────────────────────────────────────────
export default function Reports() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const [data,     setData]     = useState(null)
  const [topRisks, setTopRisks] = useState([])
  const [loading,  setLoading]  = useState(true)

  useEffect(() => {
    if (!hasPermission('REPORTS.READ')) { navigate('/dashboard', { replace: true }); return }
    Promise.all([
      api.get('/api/v1/reports/summary'),
      api.get('/api/v1/reports/top-risks'),
    ]).then(([s, r]) => {
      setData(s.data.data)
      setTopRisks(r.data.data ?? [])
    }).finally(() => setLoading(false))
  }, [])

  if (loading) return <Skeleton />
  if (!data)   return null

  const { policies, risks, tasks, kpis, compliance, governanceScore } = data

  // Pillar scores
  const polHealth = policies.total > 0 ? policies.active / policies.total * 100 : 0
  const rskHealth = risks.total    > 0 ? (risks.mitigated + risks.closed) / risks.total * 100 : 100
  const tskBase   = tasks.total    > 0 ? tasks.done / tasks.total * 100 : 100
  const tskHealth = Math.max(0, tskBase - (tasks.total > 0 ? tasks.overdue / tasks.total * 50 : 0))
  const kpiHealth = kpis.total     > 0 ? kpis.onTrack / kpis.total * 100 : 100

  // Charts data
  const riskPieData = [
    { name: t('risks.status.open'),      value: risks.open      },
    { name: t('risks.status.mitigated'), value: risks.mitigated },
    { name: t('risks.status.closed'),    value: risks.closed    },
  ].filter(d => d.value > 0)

  const taskPieData = [
    { name: t('tasks.status.pending'),    value: tasks.todo       },
    { name: t('tasks.status.inProgress'), value: tasks.inProgress },
    { name: t('tasks.status.done'),       value: tasks.done       },
  ].filter(d => d.value > 0)

  const kpiPieData = [
    { name: t('kpi.status.onTrack'), value: kpis.onTrack },
    { name: t('kpi.status.atRisk'),  value: kpis.atRisk  },
    { name: t('kpi.status.behind'),  value: kpis.behind  },
  ].filter(d => d.value > 0)

  const topRisksBar = topRisks.map(r => ({
    name: r.code, label: r.titleAr, value: r.score,
    fill: r.score >= 15 ? C.red : r.score >= 8 ? C.amber : C.green,
  }))

  return (
    <div className="space-y-5 max-w-6xl">

      {/* Title */}
      <div>
        <h1 className="text-2xl font-black text-gray-800">{t('reports.title')}</h1>
        <p className="text-sm text-gray-400 mt-0.5">{t('reports.subtitle', 'لمحة شاملة عن صحة الحوكمة المؤسسية')}</p>
      </div>

      {/* ── Quick access: Department Scorecard ── */}
      <button onClick={() => navigate('/reports/department-scorecard')}
        className="w-full flex items-center justify-between bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-xl px-5 py-3.5 hover:from-blue-700 hover:to-indigo-700 transition-all shadow-sm">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 bg-white/20 rounded-lg flex items-center justify-center">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="2" y="3" width="6" height="18" rx="1"/><rect x="9" y="8" width="6" height="13" rx="1"/><rect x="16" y="5" width="6" height="16" rx="1"/>
            </svg>
          </div>
          <div className="text-right">
            <p className="font-bold text-sm">بطاقة أداء الأقسام</p>
            <p className="text-xs text-blue-200">مؤشرات KPI مجمّعة حسب كل قسم مع درجة الأداء</p>
          </div>
        </div>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <polyline points="9 18 3 12 9 6"/>
        </svg>
      </button>

      {/* ── Row 1: Governance Score + 4 Pillars ── */}
      <Card className="bg-gradient-to-br from-slate-900 to-slate-700 border-0 text-white">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-center">

          {/* Gauge */}
          <div className="flex flex-col items-center gap-2 col-span-1">
            <p className="text-xs font-semibold text-slate-300 tracking-wider uppercase">{t('dashboard.governanceScore')}</p>
            <GovernanceGauge score={governanceScore} />
          </div>

          {/* Pillars */}
          <div className="col-span-2 space-y-3">
            <p className="text-xs font-semibold text-slate-300 tracking-wider uppercase mb-4">أبعاد الحوكمة</p>
            <PillarBar label="صحة السياسات" pct={polHealth}
              color={polHealth >= 80 ? C.green : polHealth >= 60 ? C.amber : C.red} />
            <PillarBar label="إدارة المخاطر" pct={rskHealth}
              color={rskHealth >= 80 ? C.green : rskHealth >= 60 ? C.amber : C.red} />
            <PillarBar label="إنجاز المهام"  pct={tskHealth}
              color={tskHealth >= 80 ? C.green : tskHealth >= 60 ? C.amber : C.red} />
            <PillarBar label="مؤشرات الأداء" pct={kpiHealth}
              color={kpiHealth >= 80 ? C.green : kpiHealth >= 60 ? C.amber : C.red} />
          </div>
        </div>
      </Card>

      {/* ── Row 2: Policy Lifecycle + Risk Distribution + KPI ── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

        {/* Policy Lifecycle Flow – macro view */}
        <Card title="مسار دورة حياة السياسات" className="col-span-1">
          <div className="flex flex-col gap-3">
            {[
              { label: 'مسودة',          value: policies.draft,    color: C.slate, bg: '#f8fafc', icon: '✏️', desc: 'قيد المراجعة' },
              { label: 'نشطة ومعتمدة',  value: policies.active,   color: C.green, bg: '#f0fdf4', icon: '✅', desc: 'سارية المفعول' },
              { label: 'مؤرشفة',         value: policies.archived, color: C.gray,  bg: '#f9fafb', icon: '📦', desc: 'منتهية أو مستبدلة' },
            ].map((s, i, arr) => (
              <div key={s.label}>
                <div className="flex items-center gap-3 rounded-xl p-3 border"
                  style={{ background: s.bg, borderColor: s.color + '40' }}>
                  <span className="text-xl flex-shrink-0">{s.icon}</span>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-semibold" style={{ color: s.color }}>{s.label}</span>
                      <span className="text-xl font-black" style={{ color: s.color }}>{s.value}</span>
                    </div>
                    <div className="w-full bg-white rounded-full h-1.5 mt-1 overflow-hidden">
                      <div className="h-1.5 rounded-full"
                        style={{ width: `${policies.total > 0 ? s.value / policies.total * 100 : 0}%`, background: s.color }} />
                    </div>
                  </div>
                </div>
                {i < arr.length - 1 && (
                  <div className="flex justify-center my-0.5">
                    <svg width="18" height="14" viewBox="0 0 18 14">
                      <path d="M9 0 L9 10 M4 6 L9 12 L14 6" stroke="#cbd5e1" strokeWidth="1.5"
                        fill="none" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                  </div>
                )}
              </div>
            ))}
            {(policies.expiringIn30Days > 0 || policies.expiringIn60Days > 0) && (
              <div className="space-y-1.5 pt-2 border-t border-gray-100">
                {policies.expiringIn30Days > 0 && (
                  <div className="flex items-center gap-2 text-xs bg-orange-50 border border-orange-200 rounded-lg px-3 py-2">
                    <span className="w-2 h-2 rounded-full bg-orange-500 flex-shrink-0" />
                    <span className="text-orange-700 font-medium">⚠ {policies.expiringIn30Days} تنتهي خلال 30 يوماً</span>
                  </div>
                )}
                {policies.expiringIn60Days > 0 && (
                  <div className="flex items-center gap-2 text-xs bg-yellow-50 border border-yellow-200 rounded-lg px-3 py-2">
                    <span className="w-2 h-2 rounded-full bg-yellow-400 flex-shrink-0" />
                    <span className="text-yellow-700 font-medium">🕐 {policies.expiringIn60Days} تنتهي خلال 60 يوماً</span>
                  </div>
                )}
              </div>
            )}
          </div>
        </Card>

        {/* Risk Distribution */}
        <Card title="توزيع المخاطر">
          {riskPieData.length > 0 ? (
            <>
              <Donut data={riskPieData} colors={[C.red, C.amber, C.green]}
                centerLabel="إجمالي" centerValue={risks.total} />
              <div className="flex justify-center gap-4 flex-wrap mt-2">
                {riskPieData.map((d, i) => (
                  <span key={i} className="flex items-center gap-1.5 text-xs text-gray-500">
                    <span className="w-2.5 h-2.5 rounded-full" style={{ background: [C.red, C.amber, C.green][i] }} />
                    {d.name} ({d.value})
                  </span>
                ))}
              </div>
              {risks.highRisk > 0 && (
                <div className="mt-3 flex items-center gap-2 text-xs bg-red-50 border border-red-100 rounded-lg px-3 py-2">
                  <span className="w-2 h-2 rounded-full bg-red-500 flex-shrink-0" />
                  <span className="text-red-700 font-medium">{risks.highRisk} مخاطرة عالية الخطورة (درجة ≥ 15)</span>
                </div>
              )}
            </>
          ) : (
            <p className="text-xs text-gray-300 text-center py-12">لا مخاطر</p>
          )}
        </Card>

        {/* KPI Achievement */}
        <Card title="مؤشرات الأداء الرئيسية">
          {kpiPieData.length > 0 ? (
            <>
              <KpiGauge pct={kpis.avgAchievement ?? 0} />
              <p className="text-center text-xs text-gray-400 mt-1 mb-3">متوسط تحقق المؤشرات</p>
              <div className="space-y-2">
                {[
                  { label: 'على المسار',   value: kpis.onTrack, color: C.green  },
                  { label: 'في خطر',       value: kpis.atRisk,  color: C.amber  },
                  { label: 'متأخرة',       value: kpis.behind,  color: C.red    },
                ].map(({ label, value, color }) => (
                  <div key={label} className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full flex-shrink-0" style={{ background: color }} />
                    <div className="flex-1 bg-gray-100 rounded-full h-2">
                      <div className="h-2 rounded-full transition-all"
                        style={{ width: `${kpis.total > 0 ? value / kpis.total * 100 : 0}%`, background: color }} />
                    </div>
                    <span className="text-xs font-semibold text-gray-600 w-8 text-end">{value}</span>
                    <span className="text-xs text-gray-400 w-14">{label}</span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <p className="text-xs text-gray-300 text-center py-12">لا مؤشرات</p>
          )}
        </Card>
      </div>

      {/* ── Row 3: Task Progress + Compliance Coverage ── */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">

        {/* Task Progress */}
        <Card title="إنجاز المهام">
          <div className="grid grid-cols-4 gap-3 mb-4">
            {[
              { label: 'قيد الانتظار', value: tasks.todo,       color: C.gray  },
              { label: 'جارية',        value: tasks.inProgress, color: C.blue  },
              { label: 'مكتملة',       value: tasks.done,       color: C.green },
              { label: 'متأخرة',       value: tasks.overdue,    color: C.red   },
            ].map(({ label, value, color }) => (
              <div key={label} className="flex flex-col items-center rounded-xl py-3 bg-gray-50 border border-gray-100">
                <span className="text-2xl font-black" style={{ color }}>{value}</span>
                <span className="text-xs text-gray-400 mt-0.5">{label}</span>
              </div>
            ))}
          </div>
          {/* Stacked progress bar */}
          {tasks.total > 0 && (
            <>
              <div className="flex h-4 rounded-full overflow-hidden gap-0.5">
                {[
                  { v: tasks.todo,       c: C.gray  },
                  { v: tasks.inProgress, c: C.blue  },
                  { v: tasks.done,       c: C.green },
                ].map(({ v, c }, i) => v > 0 && (
                  <div key={i} className="h-full transition-all"
                    style={{ width: `${v / tasks.total * 100}%`, background: c }} />
                ))}
              </div>
              <p className="text-xs text-gray-400 mt-2 text-end">
                معدل الإنجاز: <span className="font-bold text-emerald-600">{Math.round(tasks.done / tasks.total * 100)}%</span>
                {tasks.overdue > 0 && <span className="text-red-500 ms-3">· {tasks.overdue} متأخرة</span>}
              </p>
            </>
          )}
          {tasks.total > 0 && (
            <div className="mt-4">
              <ResponsiveContainer width="100%" height={100}>
                <BarChart data={taskPieData} barSize={36}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f8fafc" vertical={false} />
                  <XAxis dataKey="name" tick={{ fontSize: 11, fill: '#9ca3af' }} axisLine={false} tickLine={false} />
                  <YAxis hide allowDecimals={false} />
                  <Tooltip content={<Tip />} />
                  <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                    {taskPieData.map((_, i) => <Cell key={i} fill={[C.gray, C.blue, C.green][i]} />)}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </Card>

        {/* Compliance Coverage */}
        <Card title="تغطية أطر الامتثال">
          <div className="space-y-5">
            {[
              { label: 'السياسات المرتبطة بإطار امتثال', pct: compliance.policiesCoveredPct, count: compliance.policiesCovered, total: policies.total, color: C.purple },
              { label: 'المخاطر المرتبطة بإطار امتثال',  pct: compliance.risksCoveredPct,    count: compliance.risksCovered,    total: risks.total,    color: C.indigo },
            ].map(({ label, pct, count, total, color }) => (
              <div key={label}>
                <div className="flex justify-between items-end mb-2">
                  <span className="text-sm text-gray-600">{label}</span>
                  <div className="text-end">
                    <span className="text-2xl font-black" style={{ color }}>{pct}%</span>
                    <p className="text-xs text-gray-400">{count} من {total}</p>
                  </div>
                </div>
                <div className="h-3 bg-gray-100 rounded-full overflow-hidden">
                  <div className="h-3 rounded-full transition-all duration-700"
                    style={{ width: `${Math.min(pct, 100)}%`, background: color }} />
                </div>
              </div>
            ))}
            <div className="mt-4 grid grid-cols-2 gap-3 text-center">
              <div className="bg-purple-50 rounded-xl py-3 border border-purple-100">
                <p className="text-2xl font-black text-purple-700">{compliance.policiesCovered + compliance.risksCovered}</p>
                <p className="text-xs text-gray-400 mt-0.5">إجمالي العناصر المرتبطة</p>
              </div>
              <div className="bg-indigo-50 rounded-xl py-3 border border-indigo-100">
                <p className="text-2xl font-black text-indigo-700">
                  {Math.round((compliance.policiesCoveredPct + compliance.risksCoveredPct) / 2)}%
                </p>
                <p className="text-xs text-gray-400 mt-0.5">متوسط تغطية الامتثال</p>
              </div>
            </div>
          </div>
        </Card>
      </div>

      {/* ── Row 4: Top Critical Risks Bar Chart ── */}
      {topRisksBar.length > 0 && (
        <Card title="أعلى المخاطر الحرجة – مقارنة درجات الخطورة">
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={topRisksBar} layout="vertical" barSize={24} margin={{ left: 60 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" horizontal={false} />
              <XAxis type="number" domain={[0, 25]} tick={{ fontSize: 11, fill: '#9ca3af' }}
                axisLine={false} tickLine={false} />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 12, fill: '#64748b', fontWeight: 600 }}
                axisLine={false} tickLine={false} width={55} />
              <Tooltip
                content={({ active, payload }) => {
                  if (!active || !payload?.length) return null
                  const d = topRisksBar.find(r => r.name === payload[0]?.payload?.name)
                  return (
                    <div className="bg-white border border-gray-200 rounded-xl px-3 py-2 shadow-lg text-xs max-w-xs">
                      <p className="font-bold text-gray-800">{d?.label}</p>
                      <p className="text-gray-500">درجة الخطورة: <span className="font-bold">{payload[0].value}</span></p>
                    </div>
                  )
                }}
              />
              <Bar dataKey="value" radius={[0, 6, 6, 0]}>
                {topRisksBar.map((d, i) => <Cell key={i} fill={d.fill} />)}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
          {/* Score legend */}
          <div className="flex gap-4 justify-center mt-2">
            {[
              { label: 'عالية (≥15)', color: C.red   },
              { label: 'متوسطة (8-14)', color: C.amber },
              { label: 'منخفضة (<8)', color: C.green },
            ].map(({ label, color }) => (
              <span key={label} className="flex items-center gap-1.5 text-xs text-gray-500">
                <span className="w-3 h-3 rounded" style={{ background: color }} />{label}
              </span>
            ))}
          </div>
        </Card>
      )}

    </div>
  )
}
