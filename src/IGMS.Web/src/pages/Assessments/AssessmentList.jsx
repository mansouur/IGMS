import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
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

export default function AssessmentList() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const canManage = useAuthStore((s) => s.hasPermission)('ASSESSMENTS.MANAGE')

  const { loading, execute } = useApi()
  const { execute: delEx  } = useApi()
  const [items, setItems] = useState([])

  const load = () =>
    execute(() => assessmentsApi.getAll(), { silent: true }).then((r) => r && setItems(r))

  useEffect(() => { load() }, [])

  const handleDelete = async (id) => {
    if (!window.confirm(t('assessments.confirmDelete'))) return
    const ok = await delEx(() => assessmentsApi.delete(id), { successMsg: t('assessments.messages.deleted') })
    if (ok !== null) load()
  }

  if (loading && !items.length) return <PageLoader />

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('assessments.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('assessments.subtitle')}</p>
        </div>
        {canManage && (
          <button onClick={() => navigate('/assessments/new')}
            className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800">
            + {t('assessments.new')}
          </button>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {items.length === 0 ? (
          <div className="py-16 text-center text-gray-400 text-sm">{t('common.noData')}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.title')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('common.status')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.questions')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.responses')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.dueDate')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {items.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="font-medium text-gray-800">{a.titleAr}</div>
                    {a.departmentName && <div className="text-xs text-gray-400">{a.departmentName}</div>}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[a.status]}`}>
                      {STATUS_LABEL[a.status] ?? a.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{a.questionCount}</td>
                  <td className="px-4 py-3">
                    <span className="text-gray-700">{a.submittedCount}</span>
                    <span className="text-gray-400">/{a.responseCount}</span>
                    {a.myResponseSubmitted && (
                      <span className="ms-2 text-xs text-green-600">✓ أجبت</span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {a.dueDate ? new Date(a.dueDate).toLocaleDateString('ar-AE') : '—'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      {a.status === 'Published' && !a.myResponseSubmitted && (
                        <button onClick={() => navigate(`/assessments/${a.id}/respond`)}
                          className="text-xs px-3 py-1 bg-green-700 text-white rounded-lg hover:bg-green-800">
                          {t('assessments.respond')}
                        </button>
                      )}
                      {canManage && (
                        <>
                          <button onClick={() => navigate(`/assessments/${a.id}`)}
                            className="text-xs px-3 py-1 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">
                            {t('common.view')}
                          </button>
                          {a.status === 'Draft' && (
                            <button onClick={() => navigate(`/assessments/${a.id}/edit`)}
                              className="text-xs px-3 py-1 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">
                              {t('common.edit')}
                            </button>
                          )}
                          {a.status === 'Draft' && (
                            <button onClick={() => handleDelete(a.id)}
                              className="text-xs px-3 py-1 rounded-lg border border-red-200 text-red-600 hover:bg-red-50">
                              {t('common.delete')}
                            </button>
                          )}
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
