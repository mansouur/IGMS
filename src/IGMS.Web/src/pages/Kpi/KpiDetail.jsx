import { useState, useEffect, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { kpiApi } from '../../services/governanceApi'
import ComplianceTags from '../../components/ui/ComplianceTags'
import { useApi, useConfirm } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import { Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'

const QUARTER_OPTS = [
  { value: '', label: 'kpi.history.annual' },
  { value: '1', label: 'kpi.fields.q1' },
  { value: '2', label: 'kpi.fields.q2' },
  { value: '3', label: 'kpi.fields.q3' },
  { value: '4', label: 'kpi.fields.q4' },
]

const STATUS_COLOR = {
  0: 'bg-emerald-100 text-emerald-700',
  1: 'bg-amber-100 text-amber-700',
  2: 'bg-red-100 text-red-700',
}

const fmtDateTime = (v) =>
  v ? new Date(v).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' }) : '—'

export default function KpiDetail() {
  const { t } = useTranslation()
  const { id } = useParams()
  const navigate = useNavigate()
  const confirm = useConfirm()
  const { loading, execute } = useApi()

  const canUpdate = useAuthStore((s) => s.hasPermission)('KPI.UPDATE')
  const canDelete = useAuthStore((s) => s.hasPermission)('KPI.DELETE')

  const STATUS_LABEL = {
    0: t('kpi.status.onTrack'),
    1: t('kpi.status.atRisk'),
    2: t('kpi.status.behind'),
  }

  const [kpi,       setKpi]       = useState(null)
  const [records,   setRecords]   = useState([])
  const [riskLinks, setRiskLinks] = useState([])
  const [saving,    setSaving]    = useState(false)
  const [showForm,  setShowForm]  = useState(false)
  const [form, setForm] = useState({
    year: new Date().getFullYear(),
    quarter: '',
    targetValue: '',
    actualValue: '',
    notes: '',
  })

  const loadKpi = useCallback(async () => {
    const r = await execute(() => kpiApi.getById(id), { silent: true })
    if (r) setKpi(r)
  }, [id])

  const loadHistory = useCallback(async () => {
    const r = await execute(() => kpiApi.getHistory(id), { silent: true })
    if (r) setRecords(r)
  }, [id])

  useEffect(() => {
    loadKpi()
    loadHistory()
    kpiApi.getRiskLinks(id).then((r) => setRiskLinks(r.data?.data ?? [])).catch(() => {})
  }, [loadKpi, loadHistory])

  const handleSave = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const payload = {
        year:        Number(form.year),
        quarter:     form.quarter !== '' ? Number(form.quarter) : null,
        targetValue: Number(form.targetValue),
        actualValue: Number(form.actualValue),
        notes:       form.notes || null,
      }
      const r = await kpiApi.upsertRecord(id, payload)
      if (r.data?.data) {
        toast.success(t('kpi.messages.recordSaved'))
        setShowForm(false)
        setForm({ year: new Date().getFullYear(), quarter: '', targetValue: '', actualValue: '', notes: '' })
        loadHistory()
      }
    } catch {
      toast.error(t('common.noData'))
    } finally {
      setSaving(false)
    }
  }

  const handleDeleteRecord = async (rec) => {
    const ok = await confirm({
      title: t('kpi.history.confirmDelete'),
      message: `${rec.periodLabel}`,
      variant: 'danger',
    })
    if (!ok) return
    const r = await execute(() => kpiApi.deleteRecord(id, rec.id), { silent: true })
    if (r !== null) {
      toast.success(t('kpi.messages.recordDeleted'))
      loadHistory()
    }
  }

  const pct = (actual, target) =>
    target === 0 ? 0 : Math.round((actual / target) * 100)

  if (loading && !kpi) return <PageLoader />
  if (!kpi) return null

  const chartData = records.map((r) => ({
    name:   r.periodLabel,
    target: r.targetValue,
    actual: r.actualValue,
  }))

  return (
    <div className="space-y-6 max-w-5xl">

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <button onClick={() => navigate('/kpi')} className="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1 mb-2">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
            {t('kpi.title')}
          </button>
          <h1 className="text-xl font-bold text-gray-800">{kpi.titleAr}</h1>
          <p className="text-xs text-gray-400 font-mono mt-0.5">{kpi.code}</p>
        </div>
        <div className="flex items-center gap-2">
          <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${STATUS_COLOR[kpi.status]}`}>
            {STATUS_LABEL[kpi.status]}
          </span>
          {canUpdate && (
            <button onClick={() => navigate(`/kpi/${id}/edit`)} className="px-3 py-1.5 text-sm border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50">
              {t('common.edit')}
            </button>
          )}
        </div>
      </div>

      {/* KPI Info Card */}
      <div className="bg-white rounded-xl border border-gray-200 p-5 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('kpi.fields.department')}</p>
          <p className="font-medium text-gray-800">{kpi.departmentNameAr || '—'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('kpi.fields.owner')}</p>
          <p className="font-medium text-gray-800">{kpi.ownerNameAr || '—'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('kpi.fields.unit')}</p>
          <p className="font-medium text-gray-800">{kpi.unit || '—'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('kpi.fields.period')}</p>
          <p className="font-medium text-gray-800">
            {kpi.year}{kpi.quarter ? ` - Q${kpi.quarter}` : ''}
          </p>
        </div>
      </div>

      {/* Trend Chart */}
      {records.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">{t('kpi.history.chart')}</h2>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={chartData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis tick={{ fontSize: 12 }} />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="target"
                name={t('kpi.history.targetLine')}
                stroke="#94a3b8"
                strokeWidth={2}
                strokeDasharray="5 5"
                dot={{ r: 4 }}
              />
              <Line
                type="monotone"
                dataKey="actual"
                name={t('kpi.history.actualLine')}
                stroke="#059669"
                strokeWidth={2.5}
                dot={{ r: 4 }}
                activeDot={{ r: 6 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Add Record Form */}
      {canUpdate && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold text-gray-700">{t('kpi.history.addRecord')}</h2>
            <button
              onClick={() => setShowForm((v) => !v)}
              className="text-xs text-green-700 hover:underline"
            >
              {showForm ? t('common.cancel') : '+'}
            </button>
          </div>

          {showForm && (
            <form onSubmit={handleSave} className="grid grid-cols-2 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('kpi.history.year')}</label>
                <input
                  type="number" min="2000" max="2100" required
                  value={form.year}
                  onChange={(e) => setForm((f) => ({ ...f, year: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('kpi.history.quarter')}</label>
                <select
                  value={form.quarter}
                  onChange={(e) => setForm((f) => ({ ...f, quarter: e.target.value }))}
                  className={inputCls}
                >
                  {QUARTER_OPTS.map((o) => (
                    <option key={o.value} value={o.value}>{t(o.label)}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('kpi.history.target')}</label>
                <input
                  type="number" step="any" required
                  value={form.targetValue}
                  onChange={(e) => setForm((f) => ({ ...f, targetValue: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('kpi.history.actual')}</label>
                <input
                  type="number" step="any" required
                  value={form.actualValue}
                  onChange={(e) => setForm((f) => ({ ...f, actualValue: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('kpi.history.notes')}</label>
                <input
                  type="text"
                  value={form.notes}
                  onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                  className={inputCls}
                />
              </div>
              <div className="md:col-span-3 flex justify-end gap-2 pt-1">
                <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
                  {t('common.cancel')}
                </button>
                <button type="submit" disabled={saving} className="px-4 py-2 text-sm bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50 flex items-center gap-2">
                  {saving && <Spinner size="sm" />}
                  {t('common.save')}
                </button>
              </div>
            </form>
          )}
        </div>
      )}

      {/* Linked Risks */}
      {riskLinks.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center gap-2 mb-4">
            <h2 className="text-sm font-bold text-gray-700">المخاطر المؤثرة</h2>
            <span className="bg-red-100 text-red-700 text-xs font-semibold px-2 py-0.5 rounded-full">{riskLinks.length}</span>
          </div>
          <div className="space-y-2">
            {riskLinks.map((link) => {
              const rs = Number(link.riskScore)
              const scoreCls = !Number.isFinite(rs) ? 'bg-gray-400 text-white' : rs >= 15 ? 'bg-red-600 text-white' : rs >= 8 ? 'bg-amber-500 text-white' : 'bg-emerald-600 text-white'
              const statusLbl = { 0: 'مفتوحة', 1: 'مخففة', 2: 'مغلقة' }[link.riskStatus] ?? ''
              const statusCls = { 0: 'bg-red-100 text-red-700', 1: 'bg-amber-100 text-amber-700', 2: 'bg-emerald-100 text-emerald-700' }[link.riskStatus] ?? ''
              return (
                <div key={link.mappingId}
                  onClick={() => navigate(`/risks/${link.riskId}`)}
                  className="flex items-center justify-between px-4 py-3 rounded-lg border border-gray-100 hover:border-red-200 hover:bg-red-50 cursor-pointer transition-colors group">
                  <div className="flex items-center gap-3 min-w-0">
                    <span className="flex-shrink-0 w-7 h-7 rounded-lg bg-red-100 flex items-center justify-center">
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#dc2626" strokeWidth="2"><path d="M12 9v4M12 17h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/></svg>
                    </span>
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-800 truncate group-hover:text-red-700">{link.riskTitleAr}</p>
                      <p className="text-xs text-gray-400 font-mono">{link.riskCode}{link.notes && ` · ${link.notes}`}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 flex-shrink-0">
                    <span className={`text-xs px-1.5 py-0.5 rounded font-bold ${scoreCls}`}>{Number.isFinite(rs) ? rs : '—'}</span>
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${statusCls}`}>{statusLbl}</span>
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}

      {/* Compliance Tags */}
      <ComplianceTags entityType="Kpi" entityId={Number(id)} canEdit={canUpdate} />

      {/* History Table */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-700">{t('kpi.history.table')}</h2>
        </div>
        {records.length === 0 ? (
          <div className="py-12 text-center text-sm text-gray-400">{t('kpi.history.noRecords')}</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                <tr>
                  <th className="py-3 px-4 text-start">{t('kpi.history.year')}</th>
                  <th className="py-3 px-4 text-start">{t('kpi.history.quarter')}</th>
                  <th className="py-3 px-4 text-center">{t('kpi.history.target')}</th>
                  <th className="py-3 px-4 text-center">{t('kpi.history.actual')}</th>
                  <th className="py-3 px-4 text-center">{t('kpi.history.achievement')}</th>
                  <th className="py-3 px-4 text-start">{t('kpi.history.notes')}</th>
                  <th className="py-3 px-4 text-start">{t('kpi.history.recordedBy')}</th>
                  <th className="py-3 px-4 text-start">{t('kpi.history.recordedAt')}</th>
                  {canDelete && <th className="py-3 px-4" />}
                </tr>
              </thead>
              <tbody>
                {records.map((rec) => {
                  const p = pct(rec.actualValue, rec.targetValue)
                  return (
                    <tr key={rec.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4 font-medium">{rec.year}</td>
                      <td className="py-3 px-4 text-gray-600">
                        {rec.quarter ? `Q${rec.quarter}` : t('kpi.history.annual')}
                      </td>
                      <td className="py-3 px-4 text-center">{rec.targetValue} {kpi.unit}</td>
                      <td className="py-3 px-4 text-center">{rec.actualValue} {kpi.unit}</td>
                      <td className="py-3 px-4 text-center">
                        <div className="flex items-center gap-2">
                          <div className="flex-1 bg-gray-100 rounded-full h-1.5">
                            <div
                              className={`h-1.5 rounded-full ${p >= 90 ? 'bg-emerald-500' : p >= 70 ? 'bg-amber-500' : 'bg-red-500'}`}
                              style={{ width: `${Math.min(p, 100)}%` }}
                            />
                          </div>
                          <span className="text-xs font-medium w-8">{p}%</span>
                        </div>
                      </td>
                      <td className="py-3 px-4 text-gray-500 max-w-xs truncate">{rec.notes || '—'}</td>
                      <td className="py-3 px-4 text-gray-600">{rec.recordedBy}</td>
                      <td className="py-3 px-4 text-gray-500 text-xs">{fmtDateTime(rec.recordedAt)}</td>
                      {canDelete && (
                        <td className="py-3 px-4 text-center">
                          <button
                            onClick={() => handleDeleteRecord(rec)}
                            className="p-1.5 rounded-md text-red-500 hover:bg-red-50"
                            title={t('common.delete')}
                          >
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                              <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/>
                            </svg>
                          </button>
                        </td>
                      )}
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
