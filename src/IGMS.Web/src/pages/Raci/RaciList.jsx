import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { raciApi } from '../../services/api'
import { useApi, useConfirm } from '../../hooks/useApi'
import { Spinner } from '../../components/ui/Spinner'
import { SkeletonTable } from '../../components/ui/Skeleton'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'

const STATUS_STYLES = {
  0: 'bg-gray-100  text-gray-600',
  1: 'bg-amber-100 text-amber-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-slate-100 text-slate-500',
}

function StatusBadge({ status, label }) {
  return (
    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_STYLES[status] ?? STATUS_STYLES[0]}`}>
      {label}
    </span>
  )
}

export default function RaciList() {
  const { t }      = useTranslation()
  const navigate   = useNavigate()
  const confirm    = useConfirm()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const canCreate  = hasPermission('RACI.CREATE')
  const canUpdate  = hasPermission('RACI.UPDATE')
  const canDelete  = hasPermission('RACI.DELETE')
  const canApprove = hasPermission('RACI.APPROVE')

  const STATUS_LABEL = {
    0: t('raci.status.draft'),
    1: t('raci.status.underReview'),
    2: t('raci.status.approved'),
    3: t('raci.status.archived'),
  }

  const [data,       setData]       = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [search,     setSearch]     = useState('')
  const [status,     setStatus]     = useState('')
  const [page,       setPage]       = useState(1)
  const [pageSize,   setPageSize]   = useState(10)
  const [deletingId, setDeletingId] = useState(null)

  const fetchData = useCallback(async () => {
    const result = await execute(
      () => raciApi.getAll({ page, pageSize, search: search || undefined, status: status || undefined }),
      { silent: true }
    )
    if (result) setData(result)
  }, [page, pageSize, search, status])

  useEffect(() => { fetchData() }, [fetchData])

  const handleSearch = (e) => { e.preventDefault(); setPage(1) }

  const handleDelete = async (id, title) => {
    const ok = await confirm({
      title:   t('raci.confirm.deleteTitle'),
      message: t('raci.confirm.deleteMsg', { name: title }),
      variant: 'danger',
    })
    if (!ok) return
    setDeletingId(id)
    const result = await execute(() => raciApi.delete(id), { successMsg: t('raci.messages.deleted') })
    setDeletingId(null)
    if (result !== null) fetchData()
  }

  const handleSubmit = async (id) => {
    const result = await execute(() => raciApi.submit(id), { successMsg: t('raci.messages.submitted') })
    if (result !== null) fetchData()
  }

  const handleApprove = async (id) => {
    const ok = await confirm({
      title:   t('raci.confirm.approveTitle'),
      message: t('raci.confirm.approveMsg'),
      variant: 'warning',
    })
    if (!ok) return
    const result = await execute(() => raciApi.approve(id), { successMsg: t('raci.messages.approved') })
    if (result !== null) fetchData()
  }

  return (
    <div className="space-y-5 max-w-6xl">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('raci.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('raci.subtitle')}</p>
        </div>
        {canCreate && (
          <button onClick={() => navigate('/raci/new')}
            className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 transition-colors">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <path d="M12 5v14M5 12h14" />
            </svg>
            {t('raci.new')}
          </button>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={handleSearch} className="flex flex-col sm:flex-row gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)}
            placeholder={t('raci.filters.searchPlaceholder')}
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600" />
          <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
            <option value="">{t('common.allStatuses')}</option>
            <option value="0">{t('raci.status.draft')}</option>
            <option value="1">{t('raci.status.underReview')}</option>
            <option value="2">{t('raci.status.approved')}</option>
            <option value="3">{t('raci.status.archived')}</option>
          </select>
          <button type="submit" className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-200 transition-colors">
            {t('common.search')}
          </button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={6} cols={5} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="mb-3">
              <path d="M9 11l3 3L22 4M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11" />
            </svg>
            <p className="text-sm">{t('raci.noData')}</p>
            {canCreate && (
              <button onClick={() => navigate('/raci/new')} className="mt-3 text-sm text-green-700 hover:underline">
                {t('raci.createFirst')}
              </button>
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-start">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 font-semibold text-start">{t('raci.table.title')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('raci.table.department')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('raci.table.activities')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('raci.table.status')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('raci.table.createdAt')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <RaciRow key={row.id} row={row} t={t}
                      canUpdate={canUpdate} canDelete={canDelete} canApprove={canApprove}
                      deletingId={deletingId}
                      statusLabel={STATUS_LABEL[row.status] ?? row.statusLabel}
                      onView={()    => navigate(`/raci/${row.id}`)}
                      onEdit={()    => navigate(`/raci/${row.id}/edit`)}
                      onDelete={()  => handleDelete(row.id, row.titleAr)}
                      onSubmit={()  => handleSubmit(row.id)}
                      onApprove={() => handleApprove(row.id)}
                    />
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

function RaciRow({ row, t, canUpdate, canDelete, canApprove, deletingId, statusLabel, onView, onEdit, onDelete, onSubmit, onApprove }) {
  const isDeleting = deletingId === row.id

  return (
    <tr className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <button onClick={onView} className="font-medium text-green-700 hover:underline text-start">{row.titleAr}</button>
        {row.titleEn && <p className="text-xs text-gray-400 mt-0.5">{row.titleEn}</p>}
      </td>
      <td className="py-3 px-4 text-gray-600">{row.departmentAr ?? '—'}</td>
      <td className="py-3 px-4 text-center">
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-gray-100 text-gray-700 text-xs font-bold">{row.activityCount}</span>
      </td>
      <td className="py-3 px-4"><StatusBadge status={row.status} label={statusLabel} /></td>
      <td className="py-3 px-4 text-gray-500 text-xs whitespace-nowrap">
        {new Date(row.createdAt).toLocaleDateString()}
      </td>
      <td className="py-3 px-4">
        <div className="flex items-center justify-center gap-1">
          <ActionBtn onClick={onView} title={t('common.view')} color="gray">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
          </ActionBtn>
          {canUpdate && row.status < 2 && (
            <ActionBtn onClick={onEdit} title={t('common.edit')} color="blue">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
            </ActionBtn>
          )}
          {canUpdate && row.status === 0 && (
            <ActionBtn onClick={onSubmit} title={t('raci.actions.submit')} color="amber">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z"/></svg>
            </ActionBtn>
          )}
          {canApprove && row.status === 1 && (
            <ActionBtn onClick={onApprove} title={t('raci.actions.approve')} color="green">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><path d="M20 6L9 17l-5-5"/></svg>
            </ActionBtn>
          )}
          {canDelete && row.status < 2 && (
            <ActionBtn onClick={onDelete} title={t('common.delete')} color="red" disabled={isDeleting}>
              {isDeleting ? <Spinner size="sm" /> : <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/></svg>}
            </ActionBtn>
          )}
        </div>
      </td>
    </tr>
  )
}

function ActionBtn({ children, onClick, title, color, disabled }) {
  const colors = { gray: 'text-gray-500 hover:bg-gray-100', blue: 'text-blue-600 hover:bg-blue-50', amber: 'text-amber-600 hover:bg-amber-50', green: 'text-green-700 hover:bg-green-50', red: 'text-red-600 hover:bg-red-50' }
  return (
    <button onClick={onClick} disabled={disabled} title={title}
      className={['p-1.5 rounded-md transition-colors', colors[color] ?? colors.gray, disabled ? 'opacity-50 cursor-not-allowed' : ''].join(' ')}>
      {children}
    </button>
  )
}
