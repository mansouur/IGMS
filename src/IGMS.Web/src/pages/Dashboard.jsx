import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import {
  PieChart, Pie, Cell, Tooltip, ResponsiveContainer,
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  LineChart, Line, ReferenceLine,
} from 'recharts'
import api, { reportsApi, incidentsApi, assessmentsApi } from '../services/api'
import useAuthStore from '../store/authStore'

// ── Colour maps (code-keyed, stable across languages) ────────────────────────
const RISK_C   = { open: '#ef4444', mitigated: '#f59e0b', closed: '#10b981' }
const TASK_C   = { pending: '#94a3b8', inProgress: '#3b82f6', done: '#10b981' }
const KPI_C    = { onTrack: '#10b981', atRisk: '#f59e0b', behind: '#ef4444' }
const POLICY_C = { draft: '#94a3b8', active: '#10b981', archived: '#f59e0b' }
const BAR_C    = ['#8b5cf6', '#ef4444', '#3b82f6', '#10b981']

// ── Shared UI ─────────────────────────────────────────────────────────────────
function Card({ children, className = '' }) {
  return (
    <div className={`bg-white rounded-xl border border-gray-200 ${className}`}>
      {children}
    </div>
  )
}

function StatCard({ label, value, sub, color, iconPath, onClick }) {
  const bg = color
    .replace('text-purple', 'bg-purple').replace('text-red', 'bg-red')
    .replace('text-blue', 'bg-blue').replace('text-emerald', 'bg-emerald')
    .replace('-600', '-100').replace('-700', '-100')

  return (
    <button onClick={onClick}
      className="bg-white rounded-xl border border-gray-200 p-5 flex items-center gap-4
        hover:border-green-300 hover:shadow-sm transition-all text-start w-full">
      <div className={`w-10 h-10 rounded-lg flex-shrink-0 flex items-center justify-center ${bg}`}>
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" className={color}>
          <path d={iconPath} />
        </svg>
      </div>
      <div className="min-w-0">
        <p className="text-xs text-gray-500 font-medium">{label}</p>
        <p className={`text-2xl font-bold mt-0.5 ${color}`}>{value ?? '—'}</p>
        {sub && <p className="text-xs text-gray-400 mt-0.5">{sub}</p>}
      </div>
    </button>
  )
}

const ChartTooltip = ({ active, payload }) => {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-white border border-gray-200 rounded-lg px-3 py-2 shadow text-sm">
      <span className="font-medium text-gray-600">{payload[0].name}: </span>
      <span className="font-bold text-gray-900">{payload[0].value}</span>
    </div>
  )
}

function Donut({ data, colors }) {
  return (
    <ResponsiveContainer width="100%" height={160}>
      <PieChart>
        <Pie data={data} cx="50%" cy="50%" innerRadius={45} outerRadius={68}
          paddingAngle={3} dataKey="value" nameKey="name">
          {data.map((d) => <Cell key={d.code} fill={colors[d.code] ?? '#94a3b8'} />)}
        </Pie>
        <Tooltip content={<ChartTooltip />} />
      </PieChart>
    </ResponsiveContainer>
  )
}

function Legend({ data, colors }) {
  return (
    <div className="flex flex-wrap justify-center gap-x-3 gap-y-1">
      {data.map((d) => (
        <span key={d.code} className="flex items-center gap-1 text-xs text-gray-500">
          <span className="w-2 h-2 rounded-full" style={{ background: colors[d.code] ?? '#94a3b8' }} />
          {d.name} ({d.value})
        </span>
      ))}
    </div>
  )
}

function SkeletonCards() {
  return (
    <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
      {[...Array(4)].map((_, i) => (
        <div key={i} className="bg-white rounded-xl border border-gray-200 p-5 h-24 animate-pulse">
          <div className="h-3 bg-gray-100 rounded w-1/2 mb-3" />
          <div className="h-7 bg-gray-100 rounded w-1/3" />
        </div>
      ))}
    </div>
  )
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
export default function Dashboard() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { fullNameAr, fullNameEn, language, roles } = useAuthStore()
  const name = language === 'ar' ? fullNameAr : fullNameEn

  const [summary, setSummary]           = useState(null)
  const [topRisks, setTopRisks]         = useState([])
  const [recentActivity, setActivity]   = useState([])
  const [kpiTrend, setKpiTrend]         = useState([])
  const [loading, setLoading]           = useState(true)
  const [openIncidents, setOpenIncidents]   = useState([])
  const [assessmentStats, setAssessmentStats] = useState(null)

  useEffect(() => {
    api.get('/api/v1/reports/summary')
      .then(r => setSummary(r.data.data))
      .catch(() => {})
      .finally(() => setLoading(false))
    api.get('/api/v1/reports/top-risks').then(r => setTopRisks(r.data.data ?? [])).catch(() => {})
    api.get('/api/v1/reports/recent-activity').then(r => setActivity(r.data.data ?? [])).catch(() => {})
    reportsApi.kpiTrend().then(r => setKpiTrend(r.data.data ?? [])).catch(() => {})
    // Incident + Assessment widgets
    incidentsApi.getAll({ status: 'Open', pageSize: 100 }).then(r => setOpenIncidents(r?.data?.data?.items ?? [])).catch(() => {})
    assessmentsApi.getAll({ pageSize: 100 }).then(r => {
      const arr = r?.data?.data?.items ?? []
      if (!arr.length) return
      const published = arr.filter(a => a.status === 'Published')
      const totalResp = published.reduce((s, a) => s + (a.responseCount ?? 0), 0)
      const submitted = published.reduce((s, a) => s + (a.submittedCount ?? 0), 0)
      setAssessmentStats({
        published: published.length,
        total: arr.length,
        responseRate: totalResp > 0 ? Math.round(submitted / totalResp * 100) : 0,
        submitted,
        totalResp,
      })
    }).catch(() => {})
  }, [])

  const riskData = summary ? [
    { code: 'open',      name: t('risks.status.open'),      value: summary.risks.open      },
    { code: 'mitigated', name: t('risks.status.mitigated'), value: summary.risks.mitigated },
    { code: 'closed',    name: t('risks.status.closed'),    value: summary.risks.closed    },
  ].filter(d => d.value > 0) : []

  const taskData = summary ? [
    { code: 'pending',    name: t('tasks.status.pending'),    value: summary.tasks.todo       },
    { code: 'inProgress', name: t('tasks.status.inProgress'), value: summary.tasks.inProgress },
    { code: 'done',       name: t('tasks.status.done'),       value: summary.tasks.done       },
  ].filter(d => d.value > 0) : []

  const kpiData = summary ? [
    { code: 'onTrack', name: t('kpi.status.onTrack'), value: summary.kpis.onTrack },
    { code: 'atRisk',  name: t('kpi.status.atRisk'),  value: summary.kpis.atRisk  },
    { code: 'behind',  name: t('kpi.status.behind'),  value: summary.kpis.behind  },
  ].filter(d => d.value > 0) : []

  const policyData = summary ? [
    { code: 'draft',    name: t('policies.status.draft'),    value: summary.policies.draft    },
    { code: 'active',   name: t('policies.status.active'),   value: summary.policies.active   },
    { code: 'archived', name: t('policies.status.archived'), value: summary.policies.archived },
  ].filter(d => d.value > 0) : []

  const barData = summary ? [
    { name: t('dashboard.bar.policies'), value: summary.policies.total },
    { name: t('dashboard.bar.risks'),    value: summary.risks.total    },
    { name: t('dashboard.bar.tasks'),    value: summary.tasks.total    },
    { name: t('dashboard.bar.kpis'),     value: summary.kpis.total     },
  ] : []

  const ICONS = {
    policy: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6',
    risk:   'M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z M12 9v4 M12 17h.01',
    task:   'M9 11l3 3L22 4 M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11',
    kpi:    'M18 20V10 M12 20V4 M6 20v-6',
  }

  const donuts = [
    { title: t('dashboard.charts.risksByStatus'),    data: riskData,   colors: RISK_C   },
    { title: t('dashboard.charts.tasksByStatus'),    data: taskData,   colors: TASK_C   },
    { title: t('dashboard.charts.kpisByStatus'),     data: kpiData,    colors: KPI_C    },
    { title: t('dashboard.charts.policiesByStatus'), data: policyData, colors: POLICY_C },
  ]

  const quickActions = [
    { label: t('dashboard.actions.newPolicy'), path: '/policies/new', cls: 'text-purple-600 bg-purple-50 hover:bg-purple-100 border-purple-200' },
    { label: t('dashboard.actions.newRisk'),   path: '/risks/new',    cls: 'text-red-600 bg-red-50 hover:bg-red-100 border-red-200' },
    { label: t('dashboard.actions.newTask'),   path: '/tasks/new',    cls: 'text-blue-600 bg-blue-50 hover:bg-blue-100 border-blue-200' },
    { label: t('dashboard.actions.newKpi'),    path: '/kpi/new',      cls: 'text-emerald-600 bg-emerald-50 hover:bg-emerald-100 border-emerald-200' },
  ]

  return (
    <div className="space-y-6 max-w-7xl">

      {/* Welcome */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">
            {t('dashboard.welcome')}{name ? `، ${name}` : ''}
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            {t('dashboard.subtitle')}
            <span className="ms-2 text-xs bg-green-50 text-green-700 border border-green-200 px-2 py-0.5 rounded-full font-medium">
              {roles[0] ?? 'User'}
            </span>
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          {summary?.tasks?.overdue > 0 && (
            <button onClick={() => navigate('/tasks')}
              className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-2 rounded-xl hover:bg-red-100 transition-colors">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z M12 9v4 M12 17h.01"/>
              </svg>
              {t('dashboard.overdueAlert', { count: summary.tasks.overdue })}
            </button>
          )}
          {summary?.policies?.expiringIn30Days > 0 && (
            <button onClick={() => navigate('/policies')}
              className="flex items-center gap-2 bg-orange-50 border border-orange-200 text-orange-700 text-sm px-4 py-2 rounded-xl hover:bg-orange-100 transition-colors">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              </svg>
              {t('dashboard.expiryAlert30', { count: summary.policies.expiringIn30Days })}
            </button>
          )}
          {summary?.policies?.expiringIn30Days === 0 && summary?.policies?.expiringIn60Days > 0 && (
            <button onClick={() => navigate('/policies')}
              className="flex items-center gap-2 bg-yellow-50 border border-yellow-200 text-yellow-700 text-sm px-4 py-2 rounded-xl hover:bg-yellow-100 transition-colors">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              </svg>
              {t('dashboard.expiryAlert60', { count: summary.policies.expiringIn60Days })}
            </button>
          )}
        </div>
      </div>

      {/* Stat cards */}
      {loading ? <SkeletonCards /> : (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            label={t('dashboard.stats.activePolicies')}
            value={summary?.policies.active}
            sub={t('dashboard.stats.ofTotal', { total: summary?.policies.total })}
            color="text-purple-600" iconPath={ICONS.policy}
            onClick={() => navigate('/policies')} />
          <StatCard
            label={t('dashboard.stats.openRisks')}
            value={summary?.risks.open}
            sub={t('dashboard.stats.highImpact', { count: summary?.risks.highRisk })}
            color="text-red-600" iconPath={ICONS.risk}
            onClick={() => navigate('/risks')} />
          <StatCard
            label={t('dashboard.stats.activeTasks')}
            value={summary?.tasks.inProgress}
            sub={summary?.tasks.overdue > 0
              ? t('dashboard.overdueAlert', { count: summary.tasks.overdue })
              : t('dashboard.noDelay')}
            color="text-blue-600" iconPath={ICONS.task}
            onClick={() => navigate('/tasks')} />
          <StatCard
            label={t('dashboard.stats.kpisOnTrack')}
            value={summary?.kpis.onTrack}
            sub={summary?.kpis.avgAchievement != null
              ? t('dashboard.kpiAvg', { pct: summary.kpis.avgAchievement })
              : t('dashboard.stats.ofKpis', { total: summary?.kpis.total })}
            color="text-emerald-600" iconPath={ICONS.kpi}
            onClick={() => navigate('/kpi')} />
        </div>
      )}

      {/* Governance Score + Compliance Coverage */}
      {summary && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Governance Health Score */}
          {(() => {
            const s = summary.governanceScore
            const color = s >= 80 ? 'text-emerald-600' : s >= 60 ? 'text-amber-500' : 'text-red-600'
            const ring  = s >= 80 ? 'border-emerald-200 bg-emerald-50' : s >= 60 ? 'border-amber-200 bg-amber-50' : 'border-red-200 bg-red-50'
            const bar   = s >= 80 ? 'bg-emerald-500' : s >= 60 ? 'bg-amber-400' : 'bg-red-500'
            return (
              <Card className={`p-5 border-2 ${ring} col-span-1`}>
                <p className="text-xs font-semibold text-gray-500 mb-3">{t('dashboard.governanceScore')}</p>
                <div className="flex items-end gap-2 mb-3">
                  <span className={`text-5xl font-black ${color}`}>{s}</span>
                  <span className="text-sm text-gray-400 mb-1">{t('dashboard.governanceScoreOf')}</span>
                </div>
                <div className="w-full bg-gray-100 rounded-full h-2">
                  <div className={`h-2 rounded-full transition-all ${bar}`} style={{ width: `${s}%` }} />
                </div>
              </Card>
            )
          })()}

          {/* Compliance Coverage */}
          <Card className="p-5 col-span-1 md:col-span-2">
            <p className="text-xs font-semibold text-gray-500 mb-4">{t('dashboard.compliance.title')}</p>
            <div className="space-y-3">
              {[
                { label: t('dashboard.compliance.policies'), pct: summary.compliance.policiesCoveredPct, count: summary.compliance.policiesCovered },
                { label: t('dashboard.compliance.risks'),    pct: summary.compliance.risksCoveredPct,    count: summary.compliance.risksCovered    },
              ].map(({ label, pct, count }) => (
                <div key={label}>
                  <div className="flex justify-between text-xs text-gray-500 mb-1">
                    <span>{label}</span>
                    <span className="font-semibold text-gray-700">{count} ({pct}%)</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className="h-2 rounded-full bg-blue-500 transition-all" style={{ width: `${Math.min(pct, 100)}%` }} />
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      )}

      {/* Donut charts */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {donuts.map(({ title, data, colors }) => (
          <Card key={title} className="p-5">
            <h3 className="text-xs font-semibold text-gray-600 mb-3">{title}</h3>
            {data.length > 0
              ? <><Donut data={data} colors={colors} /><Legend data={data} colors={colors} /></>
              : <div className="h-40 flex items-center justify-center text-xs text-gray-300">{t('dashboard.noData')}</div>
            }
          </Card>
        ))}
      </div>

      {/* Bar chart */}
      <Card className="p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">{t('dashboard.charts.overview')}</h3>
        <ResponsiveContainer width="100%" height={200}>
          <BarChart data={barData} barSize={44}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
            <XAxis dataKey="name" tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} />
            <YAxis tick={{ fontSize: 12, fill: '#6b7280' }} axisLine={false} tickLine={false} allowDecimals={false} />
            <Tooltip content={<ChartTooltip />} cursor={{ fill: '#f8fafc' }} />
            <Bar dataKey="value" name={t('dashboard.bar.total')} radius={[6, 6, 0, 0]}>
              {barData.map((d, i) => <Cell key={d.name} fill={BAR_C[i]} />)}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </Card>

      {/* KPI Trend Line Chart */}
      {kpiTrend.length >= 2 && (
        <Card className="p-5">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold text-gray-700">{t('dashboard.charts.kpiTrend')}</h3>
            <span className="text-xs text-gray-400">{t('dashboard.charts.kpiTrendSub')}</span>
          </div>
          <ResponsiveContainer width="100%" height={200}>
            <LineChart data={kpiTrend} margin={{ top: 4, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
              <XAxis dataKey="period" tick={{ fontSize: 11, fill: '#6b7280' }} axisLine={false} tickLine={false} />
              <YAxis domain={[0, 100]} tickFormatter={v => `${v}%`}
                tick={{ fontSize: 11, fill: '#6b7280' }} axisLine={false} tickLine={false} />
              <Tooltip
                formatter={(v) => [`${v}%`, t('dashboard.charts.kpiAvgAchievement')]}
                contentStyle={{ fontSize: 12, borderRadius: 8, border: '1px solid #e5e7eb' }} />
              <ReferenceLine y={80} stroke="#10b981" strokeDasharray="4 4"
                label={{ value: '80%', position: 'insideTopRight', fontSize: 10, fill: '#10b981' }} />
              <ReferenceLine y={60} stroke="#f59e0b" strokeDasharray="4 4"
                label={{ value: '60%', position: 'insideTopRight', fontSize: 10, fill: '#f59e0b' }} />
              <Line type="monotone" dataKey="avgAchievement" name={t('dashboard.charts.kpiAvgAchievement')}
                stroke="#3b82f6" strokeWidth={2.5} dot={{ r: 4, fill: '#3b82f6', strokeWidth: 0 }}
                activeDot={{ r: 6 }} />
            </LineChart>
          </ResponsiveContainer>
          <p className="text-xs text-gray-400 text-center mt-2">
            {t('dashboard.charts.kpiTrendNote')}
          </p>
        </Card>
      )}

      {/* Incident + Assessment widgets */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">

        {/* Open Incidents */}
        <Card className="p-5">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold text-gray-700">{t('dashboard.incidents.title')}</h3>
            <button onClick={() => navigate('/incidents')}
              className="text-xs text-red-600 hover:underline">{t('common.view')}</button>
          </div>
          {openIncidents.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-6 text-center">
              <span className="text-2xl mb-1">✓</span>
              <p className="text-xs text-gray-400">{t('dashboard.incidents.allClear')}</p>
            </div>
          ) : (
            <div className="space-y-2">
              <div className="flex gap-3 mb-3">
                <div className="text-center flex-1 bg-red-50 rounded-lg py-2">
                  <div className="text-xl font-bold text-red-600">{openIncidents.length}</div>
                  <div className="text-xs text-red-400">{t('dashboard.incidents.open')}</div>
                </div>
                <div className="text-center flex-1 bg-orange-50 rounded-lg py-2">
                  <div className="text-xl font-bold text-orange-600">
                    {openIncidents.filter(i => i.severity === 'Critical').length}
                  </div>
                  <div className="text-xs text-orange-400">{t('dashboard.incidents.critical')}</div>
                </div>
              </div>
              {openIncidents
                .sort((a, b) => { const o = { Critical: 4, High: 3, Medium: 2, Low: 1 }; return (o[b.severity] ?? 0) - (o[a.severity] ?? 0) })
                .slice(0, 3)
                .map((inc) => {
                  const sevColor = { Critical: 'bg-red-100 text-red-700', High: 'bg-orange-100 text-orange-600', Medium: 'bg-yellow-50 text-yellow-700', Low: 'bg-blue-50 text-blue-600' }
                  const sevLabel = { Critical: 'حرج', High: 'عالٍ', Medium: 'متوسط', Low: 'منخفض' }
                  return (
                    <button key={inc.id} onClick={() => navigate(`/incidents/${inc.id}`)}
                      className="w-full flex items-center gap-2 text-start hover:bg-gray-50 rounded-lg px-2 py-1.5 transition-colors">
                      <span className={`text-xs px-1.5 py-0.5 rounded font-medium flex-shrink-0 ${sevColor[inc.severity] ?? ''}`}>
                        {sevLabel[inc.severity] ?? inc.severity}
                      </span>
                      <p className="text-sm text-gray-700 truncate">{inc.titleAr}</p>
                    </button>
                  )
                })
              }
            </div>
          )}
        </Card>

        {/* Active Assessments */}
        <Card className="p-5">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold text-gray-700">{t('dashboard.assessments.title')}</h3>
            <button onClick={() => navigate('/assessments')}
              className="text-xs text-green-700 hover:underline">{t('common.view')}</button>
          </div>
          {!assessmentStats || assessmentStats.total === 0 ? (
            <div className="flex flex-col items-center justify-center py-6 text-center">
              <p className="text-xs text-gray-400">{t('common.noData')}</p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex gap-3">
                <div className="text-center flex-1 bg-green-50 rounded-lg py-2">
                  <div className="text-xl font-bold text-green-700">{assessmentStats.published}</div>
                  <div className="text-xs text-green-600">{t('dashboard.assessments.published')}</div>
                </div>
                <div className="text-center flex-1 bg-blue-50 rounded-lg py-2">
                  <div className="text-xl font-bold text-blue-600">{assessmentStats.total}</div>
                  <div className="text-xs text-blue-500">{t('dashboard.assessments.total')}</div>
                </div>
              </div>
              {assessmentStats.published > 0 && (
                <div>
                  <div className="flex justify-between text-xs text-gray-500 mb-1">
                    <span>{t('dashboard.assessments.responseRate')}</span>
                    <span className="font-semibold">{assessmentStats.submitted}/{assessmentStats.totalResp} ({assessmentStats.responseRate}%)</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className={`h-2 rounded-full transition-all ${assessmentStats.responseRate >= 70 ? 'bg-green-500' : assessmentStats.responseRate >= 40 ? 'bg-yellow-400' : 'bg-red-400'}`}
                      style={{ width: `${assessmentStats.responseRate}%` }} />
                  </div>
                </div>
              )}
            </div>
          )}
        </Card>
      </div>

      {/* Top Risks + Recent Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">

        {/* Top Critical Risks */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">{t('dashboard.topRisks.title')}</h3>
          {topRisks.length === 0
            ? <p className="text-xs text-gray-300 text-center py-6">{t('dashboard.topRisks.noData')}</p>
            : (
              <div className="space-y-2">
                {topRisks.map((r) => {
                  const score = r.score
                  const cls = score >= 15 ? 'bg-red-600' : score >= 8 ? 'bg-amber-500' : 'bg-emerald-600'
                  return (
                    <button key={r.id} onClick={() => navigate(`/risks/${r.id}`)}
                      className="w-full flex items-center justify-between text-start hover:bg-gray-50 rounded-lg px-2 py-1.5 transition-colors">
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-gray-800 truncate">{r.titleAr}</p>
                        <p className="text-xs text-gray-400">{r.code} · {r.departmentNameAr ?? '—'}</p>
                      </div>
                      <span className={`ms-3 flex-shrink-0 text-xs font-bold text-white px-2 py-0.5 rounded ${cls}`}>{score}</span>
                    </button>
                  )
                })}
              </div>
            )
          }
        </Card>

        {/* Recent Activity */}
        <Card className="p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">{t('dashboard.recentActivity.title')}</h3>
          {recentActivity.length === 0
            ? <p className="text-xs text-gray-300 text-center py-6">{t('dashboard.recentActivity.noActivity')}</p>
            : (
              <div className="space-y-2">
                {recentActivity.map((a, i) => (
                  <div key={i} className="flex items-start gap-3 text-xs border-b border-gray-50 pb-2 last:border-0 last:pb-0">
                    <span className="flex-shrink-0 mt-0.5 w-1.5 h-1.5 rounded-full bg-blue-400 mt-1.5" />
                    <div className="min-w-0">
                      <p className="text-gray-700">
                        <span className="font-semibold">{a.username}</span>
                        {' · '}{a.action}
                        {' · '}<span className="text-gray-400">{a.entityName}</span>
                      </p>
                      <p className="text-gray-400">{new Date(a.timestamp).toLocaleString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            )
          }
        </Card>

      </div>

      {/* Quick actions */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {quickActions.map(({ label, path, cls }) => (
          <button key={path} onClick={() => navigate(path)}
            className={`border rounded-xl py-3 px-4 text-sm font-medium transition-colors ${cls}`}>
            + {label}
          </button>
        ))}
      </div>

    </div>
  )
}
