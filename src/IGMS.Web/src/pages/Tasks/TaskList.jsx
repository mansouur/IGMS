import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { taskApi } from '../../services/governanceApi'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { Spinner } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

export default function TaskList() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const confirm  = useConfirm()
  const { loading, execute } = useApi()
  const canCreate = useAuthStore((s) => s.hasPermission)('TASKS.CREATE')
  const canUpdate = useAuthStore((s) => s.hasPermission)('TASKS.UPDATE')
  const canDelete = useAuthStore((s) => s.hasPermission)('TASKS.DELETE')

  const STATUS_LABEL = {
    0: t('tasks.status.pending'), 1: t('tasks.status.inProgress'),
    2: t('tasks.status.done'),    3: t('tasks.status.cancelled'),
  }
  const STATUS_COLOR = {
    0: 'bg-gray-100 text-gray-600', 1: 'bg-blue-100 text-blue-700',
    2: 'bg-emerald-100 text-emerald-700', 3: 'bg-red-100 text-red-600',
  }
  const PRI_LABEL = {
    0: t('tasks.priority.low'),    1: t('tasks.priority.medium'),
    2: t('tasks.priority.high'),   3: t('tasks.priority.critical'),
  }
  const PRI_COLOR = {
    0: 'text-gray-500', 1: 'text-blue-600', 2: 'text-amber-600', 3: 'text-red-600 font-bold',
  }

  const [data,      setData]      = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [search,    setSearch]    = useState('')
  const [status,    setStatus]    = useState('')
  const [page,      setPage]      = useState(1)
  const [pageSize,  setPageSize]  = useState(10)
  const [exporting, setExporting] = useState(false)

  const fetchData = useCallback(async () => {
    const r = await execute(() => taskApi.getAll({ page, pageSize, search: search || undefined, status: status !== '' ? status : undefined }), { silent: true })
    if (r) setData(r)
  }, [page, pageSize, search, status])

  useEffect(() => { fetchData() }, [fetchData])

  const handleExport = async () => {
    setExporting(true)
    try {
      await taskApi.export({ search: search || undefined, status: status !== '' ? status : undefined })
    } catch {
      toast.error(t('common.noData'))
    } finally {
      setExporting(false)
    }
  }

  const handleDelete = async (id, title) => {
    const ok = await confirm({ title: t('tasks.confirm.deleteTitle'), message: t('tasks.confirm.deleteMsg', { name: title }), variant: 'danger' })
    if (!ok) return
    const r = await execute(() => taskApi.delete(id), { successMsg: t('tasks.messages.deleted') })
    if (r !== null) fetchData()
  }

  return (
    <div className="space-y-5 max-w-6xl">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('tasks.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('tasks.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExport} disabled={exporting}
            className="flex items-center gap-2 px-4 py-2 border border-green-700 text-green-700 text-sm font-medium rounded-lg hover:bg-green-50 disabled:opacity-50">
            {exporting ? <Spinner size="sm" /> : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>}
            {t('common.export')}
          </button>
          {canCreate && (
            <button onClick={() => navigate('/tasks/new')} className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
              {t('tasks.new')}
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={(e) => { e.preventDefault(); setPage(1) }} className="flex gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder={t('common.search')} className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600" />
          <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }} className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
            <option value="">{t('common.allStatuses')}</option>
            <option value="0">{t('tasks.status.pending')}</option>
            <option value="1">{t('tasks.status.inProgress')}</option>
            <option value="2">{t('tasks.status.done')}</option>
            <option value="3">{t('tasks.status.cancelled')}</option>
          </select>
          <button type="submit" className="px-4 py-2 bg-gray-100 text-gray-700 text-sm rounded-lg hover:bg-gray-200">{t('common.search')}</button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={5} cols={5} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center py-16 text-gray-400"><p className="text-sm">{t('tasks.noData')}</p></div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 text-start">{t('tasks.table.task')}</th>
                    <th className="py-3 px-4 text-start">{t('tasks.table.assignedTo')}</th>
                    <th className="py-3 px-4 text-start">{t('tasks.table.priority')}</th>
                    <th className="py-3 px-4 text-start">{t('tasks.table.dueDate')}</th>
                    <th className="py-3 px-4 text-start">{t('tasks.table.status')}</th>
                    <th className="py-3 px-4 text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <tr key={row.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4">
                        <button onClick={() => navigate(`/tasks/${row.id}`)}
                          className="font-medium text-blue-700 hover:underline text-start">
                          {row.titleAr}
                        </button>
                      </td>
                      <td className="py-3 px-4 text-gray-600">{row.assignedToNameAr ?? '—'}</td>
                      <td className={`py-3 px-4 text-sm ${PRI_COLOR[row.priority]}`}>{PRI_LABEL[row.priority]}</td>
                      <td className="py-3 px-4 text-gray-600 text-xs">{row.dueDate ? new Date(row.dueDate).toLocaleDateString() : '—'}</td>
                      <td className="py-3 px-4">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLOR[row.status]}`}>{STATUS_LABEL[row.status]}</span>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center justify-center gap-1">
                          {canUpdate && (
                            <button onClick={() => navigate(`/tasks/${row.id}/edit`)} title={t('common.edit')} className="p-1.5 rounded-md text-blue-600 hover:bg-blue-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                          )}
                          {canDelete && (
                            <button onClick={() => handleDelete(row.id, row.titleAr)} title={t('common.delete')} className="p-1.5 rounded-md text-red-600 hover:bg-red-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/></svg>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="border-t border-gray-100 px-4">
              <Pagination currentPage={data.currentPage} totalPages={data.totalPages} totalCount={data.totalCount} pageSize={pageSize} onPageChange={setPage} onPageSizeChange={(s) => { setPageSize(s); setPage(1) }} />
            </div>
          </>
        )}
      </div>
    </div>
  )
}
