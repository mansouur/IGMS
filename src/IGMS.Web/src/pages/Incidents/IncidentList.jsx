import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
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

export default function IncidentList() {
  const { t } = useTranslation()
  const navigate   = useNavigate()
  const canManage  = useAuthStore((s) => s.hasPermission)('INCIDENTS.MANAGE')

  const { loading, execute } = useApi()
  const { execute: delEx   } = useApi()
  const [items,  setItems]   = useState([])
  const [filter, setFilter]  = useState('') // status filter

  const load = (status = filter) =>
    execute(() => incidentsApi.getAll(status ? { status } : {}), { silent: true })
      .then((r) => r && setItems(r))

  useEffect(() => { load() }, [])

  const handleDelete = async (id) => {
    if (!window.confirm(t('incidents.confirmDelete'))) return
    const ok = await delEx(() => incidentsApi.delete(id), { successMsg: t('incidents.messages.deleted') })
    if (ok !== null) load()
  }

  const handleFilter = (st) => {
    setFilter(st)
    load(st)
  }

  if (loading && !items.length) return <PageLoader />

  const TABS = [
    { key: '',            label: t('common.all') },
    { key: 'Open',        label: STATUS_LABEL.Open },
    { key: 'UnderReview', label: STATUS_LABEL.UnderReview },
    { key: 'Resolved',    label: STATUS_LABEL.Resolved },
    { key: 'Closed',      label: STATUS_LABEL.Closed },
  ]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('incidents.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('incidents.subtitle')}</p>
        </div>
        {canManage && (
          <button onClick={() => navigate('/incidents/new')}
            className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700">
            + {t('incidents.new')}
          </button>
        )}
      </div>

      {/* Filter tabs */}
      <div className="flex gap-1 border-b border-gray-200">
        {TABS.map((tab) => (
          <button key={tab.key} onClick={() => handleFilter(tab.key)}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors
              ${filter === tab.key
                ? 'border-red-600 text-red-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
            {tab.label}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {items.length === 0 ? (
          <div className="py-16 text-center text-gray-400 text-sm">{t('common.noData')}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('incidents.fields.title')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('incidents.fields.severity')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('common.status')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('incidents.fields.occurredAt')}</th>
                <th className="text-start px-4 py-3 font-medium text-gray-600">{t('incidents.fields.riskLink')}</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {items.map((inc) => (
                <tr key={inc.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="font-medium text-gray-800">{inc.titleAr}</div>
                    {inc.departmentName && <div className="text-xs text-gray-400">{inc.departmentName}</div>}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${SEVERITY_STYLE[inc.severity]}`}>
                      {SEVERITY_LABEL[inc.severity] ?? inc.severity}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[inc.status]}`}>
                      {STATUS_LABEL[inc.status] ?? inc.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {new Date(inc.occurredAt).toLocaleDateString('ar-AE')}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500">
                    {inc.riskTitleAr ? (
                      <span className="text-blue-600 hover:underline cursor-pointer"
                        onClick={() => navigate(`/risks/${inc.riskId}`)}>
                        {inc.riskTitleAr}
                      </span>
                    ) : '—'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <button onClick={() => navigate(`/incidents/${inc.id}`)}
                        className="text-xs px-3 py-1 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">
                        {t('common.view')}
                      </button>
                      {canManage && inc.status !== 'Closed' && (
                        <button onClick={() => navigate(`/incidents/${inc.id}/edit`)}
                          className="text-xs px-3 py-1 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50">
                          {t('common.edit')}
                        </button>
                      )}
                      {canManage && (
                        <button onClick={() => handleDelete(inc.id)}
                          className="text-xs px-3 py-1 rounded-lg border border-red-200 text-red-600 hover:bg-red-50">
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
