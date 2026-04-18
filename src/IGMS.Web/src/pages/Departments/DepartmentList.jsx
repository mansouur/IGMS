import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { departmentApi } from '../../services/departmentApi'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { Spinner } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

export default function DepartmentList() {
  const { t } = useTranslation()
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const canCreate = hasPermission('DEPARTMENTS.CREATE')
  const canUpdate = hasPermission('DEPARTMENTS.UPDATE')
  const canDelete = hasPermission('DEPARTMENTS.DELETE')

  const [data,       setData]       = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [search,     setSearch]     = useState('')
  const [isActive,   setIsActive]   = useState('')
  const [page,       setPage]       = useState(1)
  const [pageSize,   setPageSize]   = useState(10)
  const [deletingId, setDeletingId] = useState(null)
  const [togglingId, setTogglingId] = useState(null)
  const [exporting,  setExporting]  = useState(false)

  const fetchData = useCallback(async () => {
    const result = await execute(
      () => departmentApi.getAll({
        page, pageSize,
        search:   search   || undefined,
        isActive: isActive !== '' ? isActive === 'true' : undefined,
      }),
      { silent: true }
    )
    if (result) setData(result)
  }, [page, pageSize, search, isActive])

  useEffect(() => { fetchData() }, [fetchData])

  const handleSearch = (e) => { e.preventDefault(); setPage(1) }

  const handleExport = async () => {
    setExporting(true)
    try {
      await departmentApi.export({
        search:   search   || undefined,
        isActive: isActive !== '' ? isActive === 'true' : undefined,
      })
    } catch {
      toast.error(t('common.noData'))
    } finally {
      setExporting(false)
    }
  }

  const handleDelete = async (id, name) => {
    const ok = await confirm({
      title:   t('departments.confirm.deleteTitle'),
      message: t('departments.confirm.deleteMsg', { name }),
      variant: 'danger',
    })
    if (!ok) return
    setDeletingId(id)
    const result = await execute(() => departmentApi.delete(id), { successMsg: t('departments.messages.deleted') })
    setDeletingId(null)
    if (result !== null) fetchData()
  }

  const handleToggleActive = async (id, currentActive) => {
    const next = !currentActive
    const ok = await confirm({
      title:   next ? t('departments.confirm.activateTitle') : t('departments.confirm.deactivateTitle'),
      message: next ? t('departments.confirm.activateMsg')   : t('departments.confirm.deactivateMsg'),
      variant: next ? 'info' : 'warning',
    })
    if (!ok) return
    setTogglingId(id)
    const result = await execute(
      () => departmentApi.setActive(id, next),
      { successMsg: next ? t('departments.messages.activated') : t('departments.messages.deactivated') }
    )
    setTogglingId(null)
    if (result !== null) fetchData()
  }

  return (
    <div className="space-y-5 max-w-6xl">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('departments.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('departments.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExport} disabled={exporting}
            className="flex items-center gap-2 px-4 py-2 border border-green-700 text-green-700 text-sm font-medium rounded-lg hover:bg-green-50 disabled:opacity-50 transition-colors">
            {exporting ? <Spinner size="sm" /> : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>}
            {t('common.export')}
          </button>
          {canCreate && (
            <button onClick={() => navigate('/departments/new')}
              className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 transition-colors">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><path d="M12 5v14M5 12h14" /></svg>
              {t('departments.new')}
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={handleSearch} className="flex flex-col sm:flex-row gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)}
            placeholder={t('departments.filters.searchPlaceholder')}
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600" />
          <select value={isActive} onChange={(e) => { setIsActive(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
            <option value="">{t('common.allStatuses')}</option>
            <option value="true">{t('users.status.active')}</option>
            <option value="false">{t('users.status.inactive')}</option>
          </select>
          <button type="submit" className="px-4 py-2 bg-gray-100 text-gray-700 text-sm font-medium rounded-lg hover:bg-gray-200 transition-colors">
            {t('common.search')}
          </button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={6} cols={6} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="mb-3">
              <path d="M3 3h18v18H3z M3 9h18 M9 21V9" />
            </svg>
            <p className="text-sm">{t('departments.noData')}</p>
            {canCreate && (
              <button onClick={() => navigate('/departments/new')} className="mt-3 text-sm text-green-700 hover:underline">
                {t('departments.createFirst')}
              </button>
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-start">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 font-semibold text-start">{t('departments.table.department')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('departments.table.code')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('departments.table.parent')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('departments.table.manager')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('departments.table.level')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('departments.table.children')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('departments.table.status')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <DepartmentRow key={row.id} row={row} t={t}
                      canUpdate={canUpdate} canDelete={canDelete}
                      deletingId={deletingId} togglingId={togglingId}
                      onView={()         => navigate(`/departments/${row.id}`)}
                      onEdit={()         => navigate(`/departments/${row.id}/edit`)}
                      onDelete={()       => handleDelete(row.id, row.nameAr)}
                      onToggleActive={() => handleToggleActive(row.id, row.isActive)}
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

function DepartmentRow({ row, t, canUpdate, canDelete, deletingId, togglingId, onView, onEdit, onDelete, onToggleActive }) {
  const isDeleting = deletingId === row.id
  const isToggling = togglingId === row.id

  return (
    <tr className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <button onClick={onView} className="font-medium text-green-700 hover:underline text-start">
          {row.nameAr}
        </button>
        {row.nameEn && <p className="text-xs text-gray-400 mt-0.5" dir="ltr">{row.nameEn}</p>}
      </td>
      <td className="py-3 px-4"><span className="font-mono text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded">{row.code}</span></td>
      <td className="py-3 px-4 text-gray-600 text-sm">{row.parentNameAr ?? '—'}</td>
      <td className="py-3 px-4 text-gray-600 text-sm">{row.managerNameAr ?? '—'}</td>
      <td className="py-3 px-4 text-center">
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-blue-50 text-blue-700 text-xs font-bold">{row.level}</span>
      </td>
      <td className="py-3 px-4 text-center">
        <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-gray-100 text-gray-700 text-xs font-bold">{row.childCount}</span>
      </td>
      <td className="py-3 px-4">
        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${row.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
          {row.isActive ? t('users.status.active') : t('users.status.inactive')}
        </span>
      </td>
      <td className="py-3 px-4">
        <div className="flex items-center justify-center gap-1">
          <ActionBtn onClick={onView} title={t('common.view')} color="gray">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
          </ActionBtn>
          {canUpdate && (
            <ActionBtn onClick={onEdit} title={t('common.edit')} color="blue">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
            </ActionBtn>
          )}
          {canUpdate && (
            <ActionBtn onClick={onToggleActive} title={row.isActive ? t('common.deactivate') : t('common.activate')} color={row.isActive ? 'amber' : 'green'} disabled={isToggling}>
              {isToggling ? <Spinner size="sm" /> : row.isActive
                ? <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>
                : <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><path d="M20 6L9 17l-5-5"/></svg>
              }
            </ActionBtn>
          )}
          {canDelete && (
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
