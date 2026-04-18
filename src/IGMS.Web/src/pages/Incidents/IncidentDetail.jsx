import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { incidentsApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const SEVERITY_STYLE = {
  Low:      'bg-blue-50 text-blue-600',
  Medium:   'bg-yellow-50 text-yellow-700',
  High:     'bg-orange-100 text-orange-700',
  Critical: 'bg-red-100 text-red-700',
}
const SEVERITY_LABEL = { Low: 'منخفض', Medium: 'متوسط', High: 'عالٍ', Critical: 'حرج' }

const STATUS_STYLE = {
  Open:        'bg-red-50 text-red-600',
  UnderReview: 'bg-yellow-50 text-yellow-700',
  Resolved:    'bg-green-50 text-green-700',
  Closed:      'bg-gray-100 text-gray-500',
}
const STATUS_LABEL = { Open: 'مفتوح', UnderReview: 'قيد المراجعة', Resolved: 'محلول', Closed: 'مغلق' }

export default function IncidentDetail() {
  const { t } = useTranslation()
  const { id } = useParams()
  const navigate   = useNavigate()
  const canManage  = useAuthStore((s) => s.hasPermission)('INCIDENTS.MANAGE')

  const { loading, execute }               = useApi()
  const { loading: acting, execute: actEx } = useApi()
  const [incident, setIncident]            = useState(null)
  const [resolveNote, setResolveNote]      = useState('')
  const [showResolve, setShowResolve]      = useState(false)

  const load = () =>
    execute(() => incidentsApi.getById(id), { silent: true }).then((r) => r && setIncident(r))

  useEffect(() => { load() }, [id])

  const handleResolve = async () => {
    const ok = await actEx(
      () => incidentsApi.resolve(id, { resolutionNotes: resolveNote }),
      { successMsg: t('incidents.messages.resolved') }
    )
    if (ok !== null) { setShowResolve(false); load() }
  }

  const handleDelete = async () => {
    if (!window.confirm(t('incidents.confirmDelete'))) return
    const ok = await actEx(() => incidentsApi.delete(id), { successMsg: t('incidents.messages.deleted') })
    if (ok !== null) navigate('/incidents')
  }

  if (loading && !incident) return <PageLoader />
  if (!incident) return null

  const inc = incident

  return (
    <div className="space-y-6 max-w-3xl">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <div className="flex items-center gap-3 mb-1">
            <h1 className="text-xl font-bold text-gray-800">{inc.titleAr}</h1>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${SEVERITY_STYLE[inc.severity]}`}>
              {SEVERITY_LABEL[inc.severity] ?? inc.severity}
            </span>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[inc.status]}`}>
              {STATUS_LABEL[inc.status] ?? inc.status}
            </span>
          </div>
          {inc.titleEn && <p className="text-sm text-gray-400" dir="ltr">{inc.titleEn}</p>}
        </div>

        <div className="flex gap-2 flex-shrink-0">
          <button onClick={() => navigate('/incidents')}
            className="text-sm text-gray-500 hover:text-gray-700 px-3 py-1.5">
            ← {t('common.back')}
          </button>
          {canManage && inc.status !== 'Closed' && (
            <button onClick={() => navigate(`/incidents/${id}/edit`)}
              className="text-sm px-4 py-1.5 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
              {t('common.edit')}
            </button>
          )}
          {canManage && (inc.status === 'Open' || inc.status === 'UnderReview') && (
            <button onClick={() => setShowResolve(true)}
              className="text-sm px-4 py-1.5 bg-green-700 text-white rounded-lg hover:bg-green-800">
              {t('incidents.resolve')}
            </button>
          )}
          {canManage && (
            <button onClick={handleDelete}
              className="text-sm px-4 py-1.5 border border-red-200 text-red-600 rounded-lg hover:bg-red-50">
              {t('common.delete')}
            </button>
          )}
        </div>
      </div>

      {/* Meta grid */}
      <div className="grid grid-cols-3 gap-4">
        {[
          { label: t('incidents.fields.occurredAt'),  value: new Date(inc.occurredAt).toLocaleString('ar-AE') },
          { label: t('incidents.fields.department'),  value: inc.departmentName ?? '—' },
          { label: t('incidents.fields.reportedBy'),  value: inc.reportedByName ?? '—' },
        ].map((card) => (
          <div key={card.label} className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="text-xs text-gray-500 mb-1">{card.label}</div>
            <div className="text-sm font-medium text-gray-800">{card.value}</div>
          </div>
        ))}
      </div>

      {/* Description */}
      {inc.descriptionAr && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-2">{t('incidents.fields.description')}</h2>
          <p className="text-sm text-gray-600 leading-relaxed">{inc.descriptionAr}</p>
        </div>
      )}

      {/* Links */}
      {(inc.riskTitleAr || inc.taskTitleAr) && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-3">{t('incidents.linkedItems')}</h2>
          <div className="space-y-2">
            {inc.riskTitleAr && (
              <div className="flex items-center gap-2 text-sm">
                <span className="text-gray-500">{t('incidents.fields.riskLink')}:</span>
                <button onClick={() => navigate(`/risks/${inc.riskId}`)}
                  className="text-blue-600 hover:underline">{inc.riskTitleAr}</button>
              </div>
            )}
            {inc.taskTitleAr && (
              <div className="flex items-center gap-2 text-sm">
                <span className="text-gray-500">{t('incidents.fields.taskLink')}:</span>
                <button onClick={() => navigate(`/tasks/${inc.taskId}`)}
                  className="text-blue-600 hover:underline">{inc.taskTitleAr}</button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Resolution */}
      {(inc.resolutionNotes || inc.resolvedAt) && (
        <div className="bg-green-50 rounded-xl border border-green-200 p-5">
          <h2 className="text-sm font-semibold text-green-800 mb-2">{t('incidents.resolution')}</h2>
          {inc.resolvedAt && (
            <p className="text-xs text-green-600 mb-2">
              {t('incidents.resolvedAt')}: {new Date(inc.resolvedAt).toLocaleString('ar-AE')}
            </p>
          )}
          {inc.resolutionNotes && (
            <p className="text-sm text-green-700">{inc.resolutionNotes}</p>
          )}
        </div>
      )}

      {/* Resolve modal */}
      {showResolve && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl p-6 w-full max-w-md mx-4 space-y-4">
            <h3 className="font-semibold text-gray-800">{t('incidents.resolveTitle')}</h3>
            <textarea rows={4}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
              placeholder={t('incidents.resolveNotesPlaceholder')}
              value={resolveNote}
              onChange={(e) => setResolveNote(e.target.value)} />
            <div className="flex justify-end gap-3">
              <button onClick={() => setShowResolve(false)}
                className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
                {t('common.cancel')}
              </button>
              <button onClick={handleResolve} disabled={acting}
                className="px-4 py-2 text-sm bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50">
                {t('incidents.resolve')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
