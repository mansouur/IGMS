import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { workflowApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

const ENTITY_TYPE_LABEL = {
  Policy:      'السياسات',
  Risk:        'المخاطر',
  ControlTest: 'اختبارات الضوابط',
}

const ENTITY_TYPE_COLOR = {
  Policy:      'bg-blue-100 text-blue-700',
  Risk:        'bg-red-100 text-red-700',
  ControlTest: 'bg-purple-100 text-purple-700',
}

export default function WorkflowList() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const canManage = useAuthStore((s) => s.hasPermission)('WORKFLOWS.MANAGE')

  const { loading, execute } = useApi()
  const { execute: delEx } = useApi()
  const [definitions, setDefinitions] = useState([])
  const [filter, setFilter] = useState('')

  const load = () =>
    execute(() => workflowApi.getDefinitions(), { silent: true }).then(
      (r) => r && setDefinitions(r)
    )

  useEffect(() => { load() }, [])

  const handleDelete = async (id) => {
    if (!window.confirm(t('workflows.confirmDelete'))) return
    const ok = await delEx(() => workflowApi.deleteDefinition(id), {
      successMsg: t('workflows.messages.deleted'),
    })
    if (ok !== null) load()
  }

  const visible = filter
    ? definitions.filter((d) => d.entityType === filter)
    : definitions

  if (loading && !definitions.length) return <PageLoader />

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('workflows.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('workflows.subtitle')}</p>
        </div>
        {canManage && (
          <button
            onClick={() => navigate('/workflows/new')}
            className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800"
          >
            + {t('workflows.new')}
          </button>
        )}
      </div>

      {/* Filter */}
      <div className="flex gap-2">
        {['', 'Policy', 'Risk', 'ControlTest'].map((type) => (
          <button
            key={type}
            onClick={() => setFilter(type)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              filter === type
                ? 'bg-green-700 text-white'
                : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'
            }`}
          >
            {type ? ENTITY_TYPE_LABEL[type] : t('common.all')}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {visible.length === 0 ? (
          <div className="py-16 text-center text-gray-400 text-sm">{t('common.noData')}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('workflows.table.name')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('workflows.table.entityType')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('workflows.table.stages')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('common.statusLabel')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {visible.map((d) => (
                <tr key={d.id} className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => navigate(`/workflows/${d.id}`)}>
                  <td className="px-4 py-3 font-medium text-gray-800">{d.nameAr}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${ENTITY_TYPE_COLOR[d.entityType] ?? 'bg-gray-100 text-gray-600'}`}>
                      {ENTITY_TYPE_LABEL[d.entityType] ?? d.entityType}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className="px-2 py-0.5 bg-gray-100 text-gray-600 rounded-full text-xs">
                      {d.stageCount} {t('workflows.stages')}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                      d.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
                    }`}>
                      {d.isActive ? t('common.active') : t('common.inactive')}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-end" onClick={(e) => e.stopPropagation()}>
                    <div className="flex justify-end gap-2">
                      <button
                        onClick={() => navigate(`/workflows/${d.id}`)}
                        className="text-xs px-3 py-1 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50"
                      >
                        {t('common.edit')}
                      </button>
                      {canManage && (
                        <button
                          onClick={() => handleDelete(d.id)}
                          className="text-xs px-3 py-1 rounded-lg border border-red-200 text-red-600 hover:bg-red-50"
                        >
                          {t('common.delete')}
                        </button>
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
