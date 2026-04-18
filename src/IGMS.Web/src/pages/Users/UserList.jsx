import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { userApi } from '../../services/userApi'
import { departmentApi } from '../../services/departmentApi'
import { toast } from '../../store/toastStore'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import { Spinner } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'

export default function UserList() {
  const { t } = useTranslation()
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const { loading, execute }    = useApi()
  const { execute: fetchDepts } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const canCreate = hasPermission('USERS.CREATE')
  const canUpdate = hasPermission('USERS.UPDATE')
  const canDelete = hasPermission('USERS.DELETE')

  const [data,        setData]        = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [departments, setDepartments] = useState([])
  const [search,      setSearch]      = useState('')
  const [deptFilter,  setDeptFilter]  = useState('')
  const [isActive,    setIsActive]    = useState('')
  const [page,        setPage]        = useState(1)
  const [pageSize,    setPageSize]    = useState(10)
  const [deletingId,  setDeletingId]  = useState(null)
  const [togglingId,  setTogglingId]  = useState(null)
  const [exporting,   setExporting]   = useState(false)

  useEffect(() => {
    fetchDepts(() => departmentApi.getAll({ pageSize: 100 }), { silent: true })
      .then((res) => { if (res) setDepartments(res.items) })
  }, [])

  const fetchData = useCallback(async () => {
    const result = await execute(
      () => userApi.getAll({
        page, pageSize,
        search:       search     || undefined,
        departmentId: deptFilter || undefined,
        isActive:     isActive !== '' ? isActive === 'true' : undefined,
      }),
      { silent: true }
    )
    if (result) setData(result)
  }, [page, pageSize, search, deptFilter, isActive])

  useEffect(() => { fetchData() }, [fetchData])

  const handleSearch = (e) => { e.preventDefault(); setPage(1) }

  const handleExport = async () => {
    setExporting(true)
    try {
      await userApi.export({
        search:       search     || undefined,
        departmentId: deptFilter || undefined,
        isActive:     isActive !== '' ? isActive === 'true' : undefined,
      })
    } catch {
      toast.error(t('common.noData'))
    } finally {
      setExporting(false)
    }
  }

  const handleDelete = async (id, name) => {
    const ok = await confirm({
      title:   t('users.confirm.deleteTitle'),
      message: t('users.confirm.deleteMsg', { name }),
      variant: 'danger',
    })
    if (!ok) return
    setDeletingId(id)
    const res = await execute(() => userApi.delete(id), { successMsg: t('users.messages.deleted') })
    setDeletingId(null)
    if (res !== null) fetchData()
  }

  const handleToggleActive = async (id, currentActive) => {
    const next = !currentActive
    const ok = await confirm({
      title:   next ? t('users.confirm.activateTitle') : t('users.confirm.deactivateTitle'),
      message: next ? t('users.confirm.activateMsg')   : t('users.confirm.deactivateMsg'),
      variant: next ? 'info' : 'warning',
    })
    if (!ok) return
    setTogglingId(id)
    const res = await execute(
      () => userApi.setActive(id, next),
      { successMsg: next ? t('users.messages.activated') : t('users.messages.deactivated') }
    )
    setTogglingId(null)
    if (res !== null) fetchData()
  }

  return (
    <div className="space-y-5 max-w-6xl">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('users.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('users.subtitle')}</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={handleExport} disabled={exporting}
            className="flex items-center gap-2 px-4 py-2 border border-green-700 text-green-700 text-sm font-medium rounded-lg hover:bg-green-50 disabled:opacity-50 transition-colors">
            {exporting ? <Spinner size="sm" /> : <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>}
            {t('common.export')}
          </button>
          {canCreate && (
            <button onClick={() => navigate('/users/new')}
              className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 transition-colors">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><path d="M12 5v14M5 12h14" /></svg>
              {t('users.new')}
            </button>
          )}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={handleSearch} className="flex flex-col sm:flex-row gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)}
            placeholder={t('users.filters.searchPlaceholder')}
            className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600" />
          <select value={deptFilter} onChange={(e) => { setDeptFilter(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
            <option value="">{t('users.filters.allDepts')}</option>
            {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
          </select>
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
        {loading ? <SkeletonTable rows={6} cols={5} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" className="mb-3">
              <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" /><circle cx="12" cy="7" r="4" />
            </svg>
            <p className="text-sm">{t('users.noData')}</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-start">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 font-semibold text-start">{t('users.table.user')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('users.table.username')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('users.table.department')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('users.table.roles')}</th>
                    <th className="py-3 px-4 font-semibold text-start">{t('users.table.status')}</th>
                    <th className="py-3 px-4 font-semibold text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <UserRow key={row.id} row={row} t={t}
                      canUpdate={canUpdate} canDelete={canDelete}
                      deletingId={deletingId} togglingId={togglingId}
                      onEdit={()         => navigate(`/users/${row.id}/edit`)}
                      onDelete={()       => handleDelete(row.id, row.fullNameAr)}
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

function UserRow({ row, t, canUpdate, canDelete, deletingId, togglingId, onEdit, onDelete, onToggleActive }) {
  const isDeleting = deletingId === row.id
  const isToggling = togglingId === row.id

  return (
    <tr className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
      <td className="py-3 px-4">
        <p className="font-medium text-gray-800">{row.fullNameAr}</p>
        <p className="text-xs text-gray-400" dir="ltr">{row.email}</p>
      </td>
      <td className="py-3 px-4"><span className="font-mono text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded" dir="ltr">{row.username}</span></td>
      <td className="py-3 px-4 text-gray-600 text-sm">{row.departmentNameAr ?? '—'}</td>
      <td className="py-3 px-4">
        <div className="flex flex-wrap gap-1">
          {row.roles.length === 0
            ? <span className="text-gray-400 text-xs">—</span>
            : row.roles.map((r) => <span key={r} className="text-xs bg-blue-50 text-blue-700 px-2 py-0.5 rounded-full">{r}</span>)
          }
        </div>
      </td>
      <td className="py-3 px-4">
        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${row.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
          {row.isActive ? t('users.status.active') : t('users.status.inactive')}
        </span>
      </td>
      <td className="py-3 px-4">
        <div className="flex items-center justify-center gap-1">
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
  const colors = { blue: 'text-blue-600 hover:bg-blue-50', amber: 'text-amber-600 hover:bg-amber-50', green: 'text-green-700 hover:bg-green-50', red: 'text-red-600 hover:bg-red-50' }
  return (
    <button onClick={onClick} disabled={disabled} title={title}
      className={['p-1.5 rounded-md transition-colors', colors[color] ?? 'text-gray-500 hover:bg-gray-100', disabled ? 'opacity-50 cursor-not-allowed' : ''].join(' ')}>
      {children}
    </button>
  )
}
