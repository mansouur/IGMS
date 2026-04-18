import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { riskApi, taskApi, kpiApi } from '../../services/governanceApi'
import { incidentsApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import ComplianceTags from '../../components/ui/ComplianceTags'
import RiskLifecycle  from '../../components/ui/RiskLifecycle'

const STATUS_CLS   = { 0: 'bg-red-100 text-red-700', 1: 'bg-amber-100 text-amber-700', 2: 'bg-emerald-100 text-emerald-700' }
const SCORE_CLS    = (s) => { const n = Number(s); if (!Number.isFinite(n) || n === 0) return 'bg-gray-400 text-white'; return n >= 15 ? 'bg-red-600 text-white' : n >= 8 ? 'bg-amber-500 text-white' : 'bg-emerald-600 text-white' }

export default function RiskDetail() {
  const { t }      = useTranslation()
  const { id }     = useParams()
  const navigate   = useNavigate()
  const { loading, execute } = useApi()
  const canUpdate  = useAuthStore((s) => s.hasPermission)('RISKS.UPDATE')

  const [risk, setRisk]         = useState(null)
  const [tasks, setTasks]       = useState([])
  const [kpiLinks, setKpiLinks] = useState([])
  const [allKpis, setAllKpis]   = useState([])
  const [incidents, setIncidents] = useState([])
  const [showKpiPicker, setShowKpiPicker] = useState(false)
  const [kpiNotes, setKpiNotes] = useState('')
  const [selectedKpiId, setSelectedKpiId] = useState('')
  const [kpiSaving, setKpiSaving] = useState(false)
  const canCreate = useAuthStore((s) => s.hasPermission)('TASKS.CREATE')

  const STATUS_LABEL   = { 0: t('risks.status.open'), 1: t('risks.status.mitigated'), 2: t('risks.status.closed') }
  const CAT_LABEL      = {
    0: t('risks.category.operational'), 1: t('risks.category.financial'),
    2: t('risks.category.technology'),  3: t('risks.category.legal'),
    4: t('risks.category.strategic'),
  }
  const LIKELIHOOD_LABEL = { 1: t('risks.likelihood.rare'), 2: t('risks.likelihood.unlikely'), 3: t('risks.likelihood.possible'), 4: t('risks.likelihood.likely'), 5: t('risks.likelihood.almostCertain') }
  const IMPACT_LABEL     = { 1: t('risks.impact.negligible'), 2: t('risks.impact.minor'), 3: t('risks.impact.moderate'), 4: t('risks.impact.major'), 5: t('risks.impact.catastrophic') }

  const loadKpiLinks = () =>
    riskApi.getKpiLinks(id).then((r) => setKpiLinks(r.data?.data ?? [])).catch(() => {})

  useEffect(() => {
    execute(() => riskApi.getById(id), { silent: true }).then((r) => { if (r) setRisk(r) })
    taskApi.getByRisk(id).then((r) => setTasks(r.data?.data ?? [])).catch(() => {})
    loadKpiLinks()
    kpiApi.getAll({ pageSize: 100 }).then((r) => setAllKpis(r.data?.data?.items ?? [])).catch(() => {})
    incidentsApi.getAll({ riskId: id }).then((r) => setIncidents(r?.data?.data ?? [])).catch(() => {})
  }, [id])

  const handleAddKpiLink = async () => {
    if (!selectedKpiId) return
    setKpiSaving(true)
    try {
      await riskApi.addKpiLink(id, { kpiId: Number(selectedKpiId), notes: kpiNotes || null })
      setShowKpiPicker(false); setSelectedKpiId(''); setKpiNotes('')
      loadKpiLinks()
    } catch (err) {
      const msg = err.response?.data?.errors?.[0] ?? 'حدث خطأ'
      alert(msg)
    } finally { setKpiSaving(false) }
  }

  const handleRemoveKpiLink = async (mappingId) => {
    await riskApi.removeKpiLink(mappingId)
    setKpiLinks((prev) => prev.filter((l) => l.mappingId !== mappingId))
  }

  if (loading && !risk) return <PageLoader />
  if (!risk) return null

  const score = (risk.likelihood ?? 0) * (risk.impact ?? 0)

  return (
    <div className="space-y-6 max-w-4xl">

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <button onClick={() => navigate('/risks')} className="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1 mb-2">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
            {t('risks.title')}
          </button>
          <h1 className="text-xl font-bold text-gray-800">{risk.titleAr}</h1>
          <p className="text-xs text-gray-400 font-mono mt-0.5">{risk.code}</p>
        </div>
        <div className="flex items-center gap-2">
          <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${STATUS_CLS[risk.status]}`}>
            {STATUS_LABEL[risk.status]}
          </span>
          {canUpdate && (
            <button onClick={() => navigate(`/risks/${id}/edit`)} className="px-3 py-1.5 text-sm border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50">
              {t('common.edit')}
            </button>
          )}
        </div>
      </div>

      {/* Info grid */}
      <div className="bg-white rounded-xl border border-gray-200 p-5 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('risks.fields.category')}</p>
          <p className="font-medium text-gray-800">{CAT_LABEL[risk.category]}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('risks.fields.department')}</p>
          <p className="font-medium text-gray-800">{risk.departmentNameAr || '—'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('risks.fields.owner')}</p>
          <p className="font-medium text-gray-800">{risk.ownerNameAr || '—'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400 mb-0.5">{t('risks.fields.riskScore')}</p>
          <span className={`inline-flex px-2 py-0.5 rounded text-xs font-bold ${SCORE_CLS(score)}`}>{Number.isFinite(score) && score > 0 ? score : '—'}</span>
        </div>
      </div>

      {/* Risk matrix */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-4">{t('risks.fields.riskMatrix')}</h2>
        <div className="grid grid-cols-2 gap-6">
          <div>
            <p className="text-xs text-gray-400 mb-1">{t('risks.table.likelihood')} ({risk.likelihood}/5)</p>
            <div className="flex items-center gap-2">
              <div className="flex gap-1">
                {[1,2,3,4,5].map((n) => (
                  <div key={n} className={`w-6 h-6 rounded flex items-center justify-center text-xs font-bold
                    ${n <= risk.likelihood ? 'bg-amber-500 text-white' : 'bg-gray-100 text-gray-400'}`}>{n}</div>
                ))}
              </div>
              <span className="text-sm text-gray-600">{LIKELIHOOD_LABEL[risk.likelihood] ?? risk.likelihood}</span>
            </div>
          </div>
          <div>
            <p className="text-xs text-gray-400 mb-1">{t('risks.table.impact')} ({risk.impact}/5)</p>
            <div className="flex items-center gap-2">
              <div className="flex gap-1">
                {[1,2,3,4,5].map((n) => (
                  <div key={n} className={`w-6 h-6 rounded flex items-center justify-center text-xs font-bold
                    ${n <= risk.impact ? 'bg-red-500 text-white' : 'bg-gray-100 text-gray-400'}`}>{n}</div>
                ))}
              </div>
              <span className="text-sm text-gray-600">{IMPACT_LABEL[risk.impact] ?? risk.impact}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Risk Lifecycle */}
      <RiskLifecycle risk={risk} />

      {/* Description + Mitigation */}
      {(risk.descriptionAr || risk.mitigationPlanAr) && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          {risk.descriptionAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">{t('risks.fields.description')}</p>
              <p className="text-sm text-gray-700 leading-relaxed">{risk.descriptionAr}</p>
            </div>
          )}
          {risk.mitigationPlanAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">{t('risks.fields.mitigation')}</p>
              <p className="text-sm text-gray-700 leading-relaxed">{risk.mitigationPlanAr}</p>
            </div>
          )}
        </div>
      )}

      {/* Mitigation Tasks */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <h2 className="text-sm font-bold text-gray-700">مهام التخفيف</h2>
            {tasks.length > 0 && (
              <span className="bg-blue-100 text-blue-700 text-xs font-semibold px-2 py-0.5 rounded-full">{tasks.length}</span>
            )}
          </div>
          {canCreate && (
            <button
              onClick={() => navigate('/tasks/new', { state: { riskId: Number(id), riskTitleAr: risk.titleAr } })}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium bg-blue-600 text-white rounded-lg hover:bg-blue-700">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
              إضافة مهمة تخفيف
            </button>
          )}
        </div>

        {tasks.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-4">لا توجد مهام مرتبطة بهذه المخاطرة</p>
        ) : (
          <div className="space-y-2">
            {tasks.map((task) => {
              const STATUS_COLOR = { 0: 'bg-gray-100 text-gray-600', 1: 'bg-blue-100 text-blue-700', 2: 'bg-emerald-100 text-emerald-700', 3: 'bg-red-100 text-red-600' }
              const STATUS_LBL   = { 0: 'قيد الانتظار', 1: 'جارية', 2: 'منجزة', 3: 'ملغاة' }
              const PRI_COLOR    = { 0: 'text-gray-400', 1: 'text-blue-500', 2: 'text-amber-500', 3: 'text-red-600' }
              const PRI_LBL      = { 0: 'منخفضة', 1: 'متوسطة', 2: 'عالية', 3: 'حرجة' }
              const isOverdue    = task.dueDate && new Date(task.dueDate) < new Date() && task.status !== 2 && task.status !== 3
              return (
                <div key={task.id}
                  onClick={() => navigate(`/tasks/${task.id}`)}
                  className="flex items-center justify-between px-4 py-3 rounded-lg border border-gray-100 hover:border-blue-200 hover:bg-blue-50 cursor-pointer transition-colors group">
                  <div className="flex items-center gap-3 min-w-0">
                    <span className={`w-2 h-2 rounded-full flex-shrink-0 ${task.status === 2 ? 'bg-emerald-500' : task.status === 1 ? 'bg-blue-500' : 'bg-gray-300'}`} />
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-800 truncate group-hover:text-blue-700">{task.titleAr}</p>
                      <div className="flex items-center gap-2 mt-0.5">
                        <span className={`text-xs ${PRI_COLOR[task.priority]}`}>{PRI_LBL[task.priority]}</span>
                        {task.assignedToNameAr && <span className="text-xs text-gray-400">• {task.assignedToNameAr}</span>}
                        {isOverdue && <span className="text-xs text-red-600 font-medium">• متأخرة</span>}
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 flex-shrink-0">
                    {task.dueDate && (
                      <span className={`text-xs ${isOverdue ? 'text-red-600 font-medium' : 'text-gray-400'}`}>
                        {new Date(task.dueDate).toLocaleDateString('ar-AE', { month: 'short', day: 'numeric' })}
                      </span>
                    )}
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_COLOR[task.status]}`}>
                      {STATUS_LBL[task.status]}
                    </span>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* KPI Impact Links */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <h2 className="text-sm font-bold text-gray-700">المؤشرات المتأثرة</h2>
            {kpiLinks.length > 0 && (
              <span className="bg-purple-100 text-purple-700 text-xs font-semibold px-2 py-0.5 rounded-full">{kpiLinks.length}</span>
            )}
          </div>
          {canUpdate && (
            <button
              onClick={() => setShowKpiPicker((v) => !v)}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium border border-purple-600 text-purple-700 rounded-lg hover:bg-purple-50">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
              ربط مؤشر
            </button>
          )}
        </div>

        {showKpiPicker && (
          <div className="mb-4 p-3 bg-purple-50 rounded-lg border border-purple-100 space-y-2">
            <select
              value={selectedKpiId}
              onChange={(e) => setSelectedKpiId(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500 bg-white"
            >
              <option value="">— اختر مؤشراً —</option>
              {allKpis
                .filter((k) => !kpiLinks.some((l) => l.kpiId === k.id))
                .map((k) => (
                  <option key={k.id} value={k.id}>{k.code} – {k.titleAr}</option>
                ))}
            </select>
            <input
              type="text"
              placeholder="ملاحظات (اختياري)"
              value={kpiNotes}
              onChange={(e) => setKpiNotes(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500 bg-white"
            />
            <div className="flex justify-end gap-2">
              <button onClick={() => setShowKpiPicker(false)} className="px-3 py-1.5 text-xs border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">إلغاء</button>
              <button onClick={handleAddKpiLink} disabled={!selectedKpiId || kpiSaving}
                className="px-3 py-1.5 text-xs bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50">
                {kpiSaving ? '...' : 'ربط'}
              </button>
            </div>
          </div>
        )}

        {kpiLinks.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-4">لا توجد مؤشرات مرتبطة بهذه المخاطرة</p>
        ) : (
          <div className="space-y-2">
            {kpiLinks.map((link) => (
              <div key={link.mappingId}
                className="flex items-center justify-between px-4 py-3 rounded-lg border border-gray-100 hover:border-purple-200 hover:bg-purple-50 transition-colors group">
                <div className="flex items-center gap-3 min-w-0 cursor-pointer" onClick={() => navigate(`/kpi/${link.kpiId}`)}>
                  <span className="flex-shrink-0 w-7 h-7 rounded-lg bg-purple-100 flex items-center justify-center">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#7c3aed" strokeWidth="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
                  </span>
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-800 truncate group-hover:text-purple-700">{link.kpiTitleAr}</p>
                    <p className="text-xs text-gray-400 font-mono">{link.kpiCode}{link.notes && ` · ${link.notes}`}</p>
                  </div>
                </div>
                {canUpdate && (
                  <button onClick={() => handleRemoveKpiLink(link.mappingId)}
                    className="p-1.5 rounded text-red-400 hover:bg-red-50 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0"
                    title="إزالة الربط">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Linked Incidents */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-gray-700">
            الحوادث المرتبطة
            {incidents.length > 0 && (
              <span className="ms-2 text-xs bg-red-100 text-red-600 rounded-full px-2 py-0.5">{incidents.length}</span>
            )}
          </h2>
          <button
            onClick={() => navigate(`/incidents/new?riskId=${id}`)}
            className="text-xs px-3 py-1.5 bg-red-600 text-white rounded-lg hover:bg-red-700">
            + حادثة جديدة
          </button>
        </div>
        {incidents.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-4">لا توجد حوادث مرتبطة بهذه المخاطرة</p>
        ) : (
          <div className="space-y-2">
            {incidents.map((inc) => {
              const sevColor = { Low: 'bg-blue-50 text-blue-600', Medium: 'bg-yellow-50 text-yellow-700', High: 'bg-orange-100 text-orange-700', Critical: 'bg-red-100 text-red-700' }
              const stColor  = { Open: 'text-red-500', UnderReview: 'text-yellow-600', Resolved: 'text-green-600', Closed: 'text-gray-400' }
              const stLabel  = { Open: 'مفتوح', UnderReview: 'قيد المراجعة', Resolved: 'محلول', Closed: 'مغلق' }
              const sevLabel = { Low: 'منخفض', Medium: 'متوسط', High: 'عالٍ', Critical: 'حرج' }
              return (
                <div key={inc.id}
                  className="flex items-center justify-between px-4 py-3 rounded-lg border border-gray-100 hover:border-red-200 hover:bg-red-50 transition-colors group cursor-pointer"
                  onClick={() => navigate(`/incidents/${inc.id}`)}>
                  <div className="flex items-center gap-3 min-w-0">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0 ${sevColor[inc.severity] ?? ''}`}>
                      {sevLabel[inc.severity] ?? inc.severity}
                    </span>
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-800 truncate group-hover:text-red-700">{inc.titleAr}</p>
                      <p className="text-xs text-gray-400">{new Date(inc.occurredAt).toLocaleDateString('ar-AE')}</p>
                    </div>
                  </div>
                  <span className={`text-xs font-medium flex-shrink-0 ${stColor[inc.status] ?? ''}`}>
                    {stLabel[inc.status] ?? inc.status}
                  </span>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Compliance Framework Mapping */}
      <ComplianceTags entityType="Risk" entityId={Number(id)} canEdit={canUpdate} />

    </div>
  )
}
