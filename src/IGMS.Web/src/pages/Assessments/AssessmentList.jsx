import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { assessmentsApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'
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

  const [data,     setData]    = useState({ items: [], totalCount: 0, currentPage: 1, pageSize: 20 })
  const [search,   setSearch]  = useState('')
  const [filter,   setFilter]  = useState('')
  const [page,     setPage]    = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const load = useCallback(async () => {
    const r = await execute(
      () => assessmentsApi.getAll({ page, pageSize, search: search || undefined, status: filter || undefined }),
      { silent: true }
    )
    if (r) setData(r)
  }, [page, pageSize, search, filter])

  useEffect(() => { load() }, [load])

  const handleDelete = async (id) => {
    if (!window.confirm(t('assessments.confirmDelete'))) return
    const ok = await delEx(() => assessmentsApi.delete(id), { successMsg: t('assessments.messages.deleted') })
    if (ok !== null) load()
  }

  const handleFilter = (st) => { setFilter(st); setPage(1) }
  const handleSearch = (e)  => { setSearch(e.target.value); setPage(1) }

  const totalPages = Math.ceil(data.totalCount / pageSize) || 1

  if (loading && !data.items.length) return <PageLoader />

  const TABS = [
    { key: '',          label: t('common.all') },
    { key: 'Draft',     label: STATUS_LABEL.Draft },
    { key: 'Published', label: STATUS_LABEL.Published },
    { key: 'Closed',    label: STATUS_LABEL.Closed },
  ]

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

      {/* Search */}
      <input
        type="search"
        value={search}
        onChange={handleSearch}
        placeholder={t('common.search')}
        className="w-full sm:w-72 border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
      />

      {/* Status tabs */}
      <div className="flex gap-1 border-b border-gray-200">
        {TABS.map((tab) => (
          <button key={tab.key} onClick={() => handleFilter(tab.key)}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors
              ${filter === tab.key
                ? 'border-green-700 text-green-700'
                : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
            {tab.label}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {data.items.length === 0 ? (
          <div className="py-16 text-center text-gray-400 text-sm">{t('common.noData')}</div>
        ) : (
          <>
            <table className="w-full text-sm">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.title')}</th>
                  <th className="text-start px-4 py-3 font-medium text-gray-600">{t('common.statusLabel')}</th>
                  <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.questions')}</th>
                  <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.responses')}</th>
                  <th className="text-start px-4 py-3 font-medium text-gray-600">{t('assessments.table.dueDate')}</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((a) => (
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
            <div className="border-t border-gray-100 px-4">
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalCount={data.totalCount}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={(s) => { setPageSize(s); setPage(1) }}
              />
            </div>
          </>
        )}
      </div>
    </div>
  )
}
