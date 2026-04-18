import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { rolesApi } from '../../services/api'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import useAuthStore from '../../store/authStore'

export default function RoleList() {
  const { t }    = useTranslation()
  const navigate = useNavigate()
  const confirm  = useConfirm()
  const { loading, execute } = useApi()
  const canManage = useAuthStore((s) => s.hasPermission)('USERS.MANAGE')

  const [roles, setRoles] = useState([])

  const load = useCallback(async () => {
    const r = await execute(() => rolesApi.getAll(), { silent: true })
    if (r) setRoles(r)
  }, [])

  useEffect(() => { load() }, [load])

  const handleDelete = async (id, name) => {
    const ok = await confirm({
      title:   t('roles.confirm.deleteTitle'),
      message: t('roles.confirm.deleteMsg', { name }),
      variant: 'danger',
    })
    if (!ok) return
    const r = await execute(() => rolesApi.delete(id), { successMsg: t('roles.messages.deleted') })
    if (r !== null) load()
  }

  return (
    <div className="space-y-5 max-w-4xl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('roles.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('roles.subtitle')}</p>
        </div>
        {canManage && (
          <button
            onClick={() => navigate('/roles/new')}
            className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
            {t('roles.new')}
          </button>
        )}
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={4} cols={5} /> : roles.length === 0 ? (
          <div className="py-16 text-center text-sm text-gray-400">{t('roles.noData')}</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
              <tr>
                <th className="py-3 px-4 text-start">{t('roles.table.role')}</th>
                <th className="py-3 px-4 text-center">{t('roles.table.permissions')}</th>
                <th className="py-3 px-4 text-center">{t('roles.table.users')}</th>
                <th className="py-3 px-4 text-center">{t('roles.table.type')}</th>
                <th className="py-3 px-4 text-center">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {roles.map((role) => (
                <tr key={role.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4">
                    <button onClick={() => navigate(`/roles/${role.id}`)} className="text-start hover:underline">
                      <p className="font-medium text-gray-800">{role.nameAr}</p>
                      {role.nameEn && <p className="text-xs text-gray-400">{role.nameEn}</p>}
                      <p className="text-xs text-gray-400 font-mono mt-0.5">{role.code}</p>
                    </button>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                      {role.permissionCount}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600">
                      {role.userCount}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    {role.isSystemRole ? (
                      <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">{t('roles.systemRole')}</span>
                    ) : (
                      <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-500">{t('roles.customRole')}</span>
                    )}
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center justify-center gap-1">
                      <button onClick={() => navigate(`/roles/${role.id}`)} title={t('common.view')} className="p-1.5 rounded-md text-gray-600 hover:bg-gray-100">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                      </button>
                      {canManage && !role.isSystemRole && (
                        <button onClick={() => handleDelete(role.id, role.nameAr)} title={t('common.delete')} className="p-1.5 rounded-md text-red-600 hover:bg-red-50">
                          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/></svg>
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
