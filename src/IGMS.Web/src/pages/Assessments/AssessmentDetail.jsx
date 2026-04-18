import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { assessmentsApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const STATUS_STYLE = {
  Draft:     'bg-gray-100 text-gray-600',
  Published: 'bg-green-100 text-green-700',
  Closed:    'bg-red-100 text-red-600',
}
const STATUS_LABEL = { Draft: 'مسودة', Published: 'منشور', Closed: 'مغلق' }

const Q_TYPE_LABEL = { YesNo: 'نعم/لا', Rating: 'تقييم', Text: 'نصي', MultiChoice: 'اختيار متعدد' }

export default function AssessmentDetail() {
  const { t } = useTranslation()
  const { id } = useParams()
  const navigate = useNavigate()
  const canManage = useAuthStore((s) => s.hasPermission)('ASSESSMENTS.MANAGE')

  const { loading, execute }             = useApi()
  const { loading: acting, execute: actEx } = useApi()
  const [assessment, setAssessment]      = useState(null)

  const load = () =>
    execute(() => assessmentsApi.getById(id), { silent: true }).then((r) => r && setAssessment(r))

  useEffect(() => { load() }, [id])

  const handlePublish = async () => {
    if (!window.confirm(t('assessments.confirmPublish'))) return
    const ok = await actEx(() => assessmentsApi.publish(id), { successMsg: t('assessments.messages.published') })
    if (ok !== null) load()
  }

  const handleClose = async () => {
    if (!window.confirm(t('assessments.confirmClose'))) return
    const ok = await actEx(() => assessmentsApi.close(id), { successMsg: t('assessments.messages.closed') })
    if (ok !== null) load()
  }

  if (loading && !assessment) return <PageLoader />
  if (!assessment) return null

  const a = assessment

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <div className="flex items-center gap-3 mb-1">
            <h1 className="text-xl font-bold text-gray-800">{a.titleAr}</h1>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[a.status]}`}>
              {STATUS_LABEL[a.status] ?? a.status}
            </span>
          </div>
          {a.titleEn && <p className="text-sm text-gray-400 mb-1" dir="ltr">{a.titleEn}</p>}
          {a.descriptionAr && <p className="text-sm text-gray-500">{a.descriptionAr}</p>}
        </div>

        <div className="flex gap-2 flex-shrink-0">
          <button onClick={() => navigate('/assessments')}
            className="text-sm text-gray-500 hover:text-gray-700 px-3 py-1.5">
            ← {t('common.back')}
          </button>
          {canManage && a.status === 'Draft' && (
            <>
              <button onClick={() => navigate(`/assessments/${id}/edit`)}
                className="text-sm px-4 py-1.5 border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
                {t('common.edit')}
              </button>
              <button onClick={handlePublish} disabled={acting}
                className="text-sm px-4 py-1.5 bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50">
                {t('assessments.publish')}
              </button>
            </>
          )}
          {canManage && a.status === 'Published' && (
            <button onClick={handleClose} disabled={acting}
              className="text-sm px-4 py-1.5 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50">
              {t('assessments.close')}
            </button>
          )}
          {canManage && a.status === 'Closed' && (
            <button onClick={() => navigate(`/assessments/${id}/report`)}
              className="text-sm px-4 py-1.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
              {t('assessments.viewReport')}
            </button>
          )}
        </div>
      </div>

      {/* Meta cards */}
      <div className="grid grid-cols-4 gap-4">
        {[
          { label: t('assessments.table.questions'),  value: a.questions?.length ?? 0 },
          { label: t('assessments.table.responses'),  value: `${a.submittedCount ?? 0}/${a.responseCount ?? 0}` },
          { label: t('assessments.fields.department'), value: a.departmentName ?? '—' },
          { label: t('assessments.fields.dueDate'),    value: a.dueDate ? new Date(a.dueDate).toLocaleDateString('ar-AE') : '—' },
        ].map((card) => (
          <div key={card.label} className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="text-xs text-gray-500 mb-1">{card.label}</div>
            <div className="text-lg font-bold text-gray-800">{card.value}</div>
          </div>
        ))}
      </div>

      {/* Questions */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="px-5 py-3 border-b border-gray-100 bg-gray-50">
          <h2 className="text-sm font-semibold text-gray-700">{t('assessments.questions')}</h2>
        </div>
        {(!a.questions || a.questions.length === 0) ? (
          <div className="py-10 text-center text-sm text-gray-400">{t('common.noData')}</div>
        ) : (
          <ul className="divide-y divide-gray-100">
            {a.questions.map((q, i) => (
              <li key={q.id} className="px-5 py-3 flex items-start gap-3">
                <span className="text-xs bg-gray-100 text-gray-500 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0 mt-0.5">
                  {i + 1}
                </span>
                <div className="flex-1 min-w-0">
                  <div className="text-sm text-gray-800">{q.textAr}</div>
                  {q.textEn && <div className="text-xs text-gray-400 mt-0.5" dir="ltr">{q.textEn}</div>}
                  {q.questionType === 'MultiChoice' && q.options?.length > 0 && (
                    <div className="flex flex-wrap gap-1.5 mt-1.5">
                      {q.options.map((opt, oi) => (
                        <span key={oi} className="text-xs bg-gray-100 text-gray-600 rounded px-2 py-0.5">{opt}</span>
                      ))}
                    </div>
                  )}
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <span className="text-xs bg-blue-50 text-blue-600 rounded px-2 py-0.5">
                    {Q_TYPE_LABEL[q.questionType] ?? q.questionType}
                  </span>
                  {q.isRequired && (
                    <span className="text-xs bg-orange-50 text-orange-600 rounded px-2 py-0.5">
                      {t('assessments.required')}
                    </span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {/* Report shortcut for published */}
      {canManage && a.status === 'Published' && (a.submittedCount ?? 0) > 0 && (
        <div className="flex justify-end">
          <button onClick={() => navigate(`/assessments/${id}/report`)}
            className="text-sm px-4 py-2 border border-blue-300 text-blue-600 rounded-lg hover:bg-blue-50">
            {t('assessments.viewReport')}
          </button>
        </div>
      )}
    </div>
  )
}
