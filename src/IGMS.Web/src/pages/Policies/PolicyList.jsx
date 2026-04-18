import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { policyApi } from '../../services/governanceApi'
import { userApi } from '../../services/userApi'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { Spinner } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

export default function PolicyList() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const confirm  = useConfirm()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canCreate  = hasPermission('POLICIES.CREATE')
  const canUpdate  = hasPermission('POLICIES.UPDATE')
  const canDelete  = hasPermission('POLICIES.DELETE')
  const canPublish = hasPermission('POLICIES.PUBLISH')
  const canApprove = hasPermission('POLICIES.APPROVE')

  const STATUS_LABEL = { 0: t('policies.status.draft'), 1: t('policies.status.active'), 2: t('policies.status.archived') }
  const STATUS_COLOR  = { 0: 'bg-gray-100 text-gray-600', 1: 'bg-emerald-100 text-emerald-700', 2: 'bg-amber-100 text-amber-700' }
  const CAT_LABEL    = {
    0: t('policies.category.governance'), 1: t('policies.category.technology'),
    2: t('policies.category.hr'), 3: t('policies.category.finance'), 4: t('policies.category.operational'),
  }

  const [data,      setData]      = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [search,    setSearch]    = useState('')
  const [status,    setStatus]    = useState('')
  const [page,      setPage]      = useState(1)
  const [pageSize,  setPageSize]  = useState(10)
  const [exporting, setExporting] = useState(false)

  // Publish modal state
  const [publishModal,      setPublishModal]      = useState(null) // { id, title }
  const [publishApproverId, setPublishApproverId] = useState('')
  const [publishing,        setPublishing]        = useState(false)
  const [users,             setUsers]             = useState([])

  useEffect(() => {
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  const fetchData = useCallback(async () => {
    const r = await execute(() => policyApi.getAll({ page, pageSize, search: search || undefined, status: status !== '' ? status : undefined }), { silent: true })
    if (r) setData(r)
  }, [page, pageSize, search, status])

  useEffect(() => { fetchData() }, [fetchData])

  const handleExport = async () => {
    setExporting(true)
    try {
      await policyApi.export({ search: search || undefined, status: status !== '' ? status : undefined })
    } catch {
      toast.error(t('common.noData'))
    } finally {
      setExporting(false)
    }
  }

  const handleDelete = async (id, title) => {
    const ok = await confirm({ title: t('policies.confirm.deleteTitle'), message: t('policies.confirm.deleteMsg', { name: title }), variant: 'danger' })
    if (!ok) return
    const r = await execute(() => policyApi.delete(id), { successMsg: t('policies.messages.deleted') })
    if (r !== null) fetchData()
  }

  const handleSetStatus = async (id, newStatus, label, confirmMsg) => {
    const ok = await confirm({ title: label, message: confirmMsg, variant: 'warning' })
    if (!ok) return
    const r = await execute(() => policyApi.setStatus(id, newStatus), { successMsg: t('policies.messages.statusUpdated') })
    if (r !== null) fetchData()
  }

  const openPublishModal = (row) => {
    setPublishApproverId('')
    setPublishModal({ id: row.id, title: row.titleAr })
  }

  const handlePublish = async () => {
    if (!publishApproverId) return
    setPublishing(true)
    try {
      await policyApi.setStatus(publishModal.id, 1, Number(publishApproverId))
      toast.success(t('policies.messages.statusUpdated'))
      setPublishModal(null)
      fetchData()
    } catch (err) {
      const msg = err.response?.data?.errors?.[0] ?? t('policies.messages.publishError')
      toast.error(msg)
    } finally {
      setPublishing(false)
    }
  }

  return (
    <>
    <div className="space-y-5 max-w-6xl">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('policies.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('policies.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExport} disabled={exporting}
            className="flex items-center gap-2 px-4 py-2 border border-green-700 text-green-700 text-sm font-medium rounded-lg hover:bg-green-50 disabled:opacity-50">
            {exporting ? <Spinner size="sm" /> : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>}
            {t('common.export')}
          </button>
          {canCreate && (
            <button onClick={() => navigate('/policies/new')} className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
              {t('policies.new')}
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={(e) => { e.preventDefault(); setPage(1) }} className="flex gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder={t('common.search')} className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600" />
          <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }} className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
            <option value="">{t('common.allStatuses')}</option>
            <option value="0">{t('policies.status.draft')}</option>
            <option value="1">{t('policies.status.active')}</option>
            <option value="2">{t('policies.status.archived')}</option>
          </select>
          <button type="submit" className="px-4 py-2 bg-gray-100 text-gray-700 text-sm rounded-lg hover:bg-gray-200">{t('common.search')}</button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={5} cols={5} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center py-16 text-gray-400"><p className="text-sm">{t('policies.noData')}</p></div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 text-start">{t('policies.table.title')}</th>
                    <th className="py-3 px-4 text-start">{t('policies.table.code')}</th>
                    <th className="py-3 px-4 text-start">{t('policies.table.category')}</th>
                    <th className="py-3 px-4 text-start">{t('policies.table.department')}</th>
                    <th className="py-3 px-4 text-start">{t('policies.table.status')}</th>
                    <th className="py-3 px-4 text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <tr key={row.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4">
                        <button onClick={() => navigate(`/policies/${row.id}`)}
                          className="font-medium text-gray-800 hover:text-green-700 text-start transition-colors">
                          {row.titleAr}
                        </button>
                      </td>
                      <td className="py-3 px-4"><span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.code}</span></td>
                      <td className="py-3 px-4 text-gray-600">{CAT_LABEL[row.category] ?? row.category}</td>
                      <td className="py-3 px-4 text-gray-600">{row.departmentNameAr ?? '—'}</td>
                      <td className="py-3 px-4">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLOR[row.status]}`}>{STATUS_LABEL[row.status]}</span>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center justify-center gap-1">
                          {canPublish && row.status === 0 && (
                            <button onClick={() => openPublishModal(row)} title={t('policies.actions.publish')}
                              className="p-1.5 rounded-md text-emerald-600 hover:bg-emerald-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                            </button>
                          )}
                          {canApprove && row.status === 1 && (
                            <button onClick={() => handleSetStatus(row.id, 2, t('policies.actions.archive'), t('policies.confirm.archiveMsg'))} title={t('policies.actions.archive')}
                              className="p-1.5 rounded-md text-amber-600 hover:bg-amber-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="21 8 21 21 3 21 3 8"/><rect x="1" y="3" width="22" height="5"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
                            </button>
                          )}
                          {canUpdate && (
                            <button onClick={() => navigate(`/policies/${row.id}/edit`)} title={t('common.edit')}
                              className="p-1.5 rounded-md text-blue-600 hover:bg-blue-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                          )}
                          {canDelete && (
                            <button onClick={() => handleDelete(row.id, row.titleAr)} title={t('common.delete')}
                              className="p-1.5 rounded-md text-red-600 hover:bg-red-50">
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

    {/* ── Publish Modal ─────────────────────────────────────────────── */}
    {publishModal && (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-5">
          {/* Header */}
          <div>
            <h2 className="text-base font-bold text-gray-800">{t('policies.actions.publish')}</h2>
            <p className="text-sm text-gray-500 mt-1 truncate">«{publishModal.title}»</p>
          </div>

          {/* Approver picker */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              {t('policies.publish.approver')}
              <span className="text-red-500 ms-1">*</span>
            </label>
            <select
              value={publishApproverId}
              onChange={(e) => setPublishApproverId(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
            >
              <option value="">{t('policies.publish.chooseApprover')}</option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>{u.fullNameAr} ({u.username})</option>
              ))}
            </select>
            <p className="text-xs text-gray-400 mt-1">{t('policies.publish.approverHint')}</p>
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-1">
            <button
              onClick={() => setPublishModal(null)}
              className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50"
            >
              {t('common.cancel')}
            </button>
            <button
              onClick={handlePublish}
              disabled={!publishApproverId || publishing}
              className="flex items-center gap-2 px-5 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
            >
              {publishing && <Spinner size="sm" />}
              {t('policies.actions.publish')}
            </button>
          </div>
        </div>
      </div>
    )}
    </>
  )
}
