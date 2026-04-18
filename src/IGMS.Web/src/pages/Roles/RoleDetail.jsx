import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { rolesApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader, Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

// Module display order + Arabic labels
const MODULE_LABEL = {
  USERS:       'المستخدمون',
  DEPARTMENTS: 'الأقسام',
  RACI:        'مصفوفة RACI',
  POLICIES:    'السياسات',
  TASKS:       'المهام',
  KPI:         'مؤشرات الأداء',
  RISKS:       'المخاطر',
  CONTROLS:    'اختبارات الضوابط',
  REPORTS:     'التقارير',
  SETTINGS:    'الإعدادات',
  AUDIT:       'سجل المراجعة',
}

const ACTION_LABEL = {
  READ:    'عرض',
  CREATE:  'إنشاء',
  UPDATE:  'تعديل',
  DELETE:  'حذف',
  APPROVE: 'اعتماد',
  PUBLISH: 'نشر',
  EXPORT:  'تصدير',
  ASSIGN:  'إسناد',
  MANAGE:  'إدارة كاملة',
}

const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'

export default function RoleDetail() {
  const { t }   = useTranslation()
  const { id }  = useParams()
  const isNew   = id === 'new'
  const navigate = useNavigate()
  const canManage = useAuthStore((s) => s.hasPermission)('USERS.MANAGE')

  const { loading: fetching, execute: fetchEx } = useApi()
  const { loading: saving,   execute: saveEx  } = useApi()
  const { loading: savingPerms, execute: permEx } = useApi()

  const [role,        setRole]        = useState(null)
  const [allGroups,   setAllGroups]   = useState([])  // [{module, permissions:[]}]
  const [checked,     setChecked]     = useState(new Set())
  const [form,        setForm]        = useState({ nameAr: '', nameEn: '', code: '', descriptionAr: '' })
  const [permsDirty,  setPermsDirty]  = useState(false)

  // Load permissions catalogue
  useEffect(() => {
    fetchEx(() => rolesApi.getAllPermissions(), { silent: true }).then((r) => {
      if (r) setAllGroups(r)
    })
  }, [])

  // Load role (edit mode)
  useEffect(() => {
    if (isNew) return
    fetchEx(() => rolesApi.getById(id), { silent: true }).then((r) => {
      if (!r) return
      setRole(r)
      setForm({
        nameAr: r.nameAr ?? '',
        nameEn: r.nameEn ?? '',
        code:   r.code   ?? '',
        descriptionAr: r.descriptionAr ?? '',
      })
      setChecked(new Set(r.permissionIds))
    })
  }, [id])

  const set = (f) => (e) => setForm((prev) => ({ ...prev, [f]: e.target.value }))

  // Toggle single permission
  const toggle = (pid) => {
    if (!canManage) return
    setChecked((prev) => {
      const next = new Set(prev)
      next.has(pid) ? next.delete(pid) : next.add(pid)
      return next
    })
    setPermsDirty(true)
  }

  // Toggle entire module
  const toggleModule = (perms) => {
    if (!canManage) return
    const ids    = perms.map((p) => p.id)
    const allOn  = ids.every((id) => checked.has(id))
    setChecked((prev) => {
      const next = new Set(prev)
      if (allOn) ids.forEach((id) => next.delete(id))
      else       ids.forEach((id) => next.add(id))
      return next
    })
    setPermsDirty(true)
  }

  // Save role info (create or update)
  const handleSaveRole = async (e) => {
    e.preventDefault()
    if (isNew) {
      const r = await saveEx(() => rolesApi.create(form), { successMsg: t('roles.messages.saved') })
      if (r) navigate(`/roles/${r.id}`)
    } else {
      await saveEx(() => rolesApi.update(id, form), { successMsg: t('roles.messages.saved') })
    }
  }

  // Save permissions
  const handleSavePerms = async () => {
    await permEx(
      () => rolesApi.setPermissions(id, [...checked]),
      { successMsg: t('roles.messages.permsSaved') }
    )
    setPermsDirty(false)
  }

  if (fetching && !isNew) return <PageLoader />

  const isSystem = role?.isSystemRole ?? false

  return (
    <div className="max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">
            {isNew ? t('roles.new') : (role?.nameAr ?? '...')}
          </h1>
          {isSystem && (
            <span className="inline-flex mt-1 px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
              {t('roles.systemRole')} — {t('roles.systemRoleNote')}
            </span>
          )}
        </div>
        <button onClick={() => navigate('/roles')} className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('roles.title')}
        </button>
      </div>

      {/* Role info form */}
      <form onSubmit={handleSaveRole} className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
        <h2 className="font-semibold text-gray-700 text-sm mb-2">{t('roles.info')}</h2>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('roles.form.nameAr')} <span className="text-red-500">*</span></label>
            <input value={form.nameAr} onChange={set('nameAr')} required disabled={isSystem}
              className={inputCls} dir="rtl" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('roles.form.nameEn')}</label>
            <input value={form.nameEn} onChange={set('nameEn')} disabled={isSystem}
              className={inputCls} dir="ltr" />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">{t('roles.form.code')} <span className="text-red-500">*</span></label>
          <input value={form.code} onChange={set('code')} required disabled={!isNew}
            placeholder="CUSTOM_ROLE" className={`${inputCls} font-mono uppercase`} />
          {isNew && <p className="text-xs text-gray-400 mt-1">{t('roles.form.codeHint')}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">{t('roles.form.description')}</label>
          <textarea value={form.descriptionAr} onChange={set('descriptionAr')} rows={2}
            disabled={isSystem} className={inputCls} dir="rtl" />
        </div>

        {canManage && !isSystem && (
          <div className="flex justify-end pt-1">
            <button type="submit" disabled={saving}
              className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
              {saving && <Spinner size="sm" />}
              {t('common.save')}
            </button>
          </div>
        )}
      </form>

      {/* Permissions matrix — only in edit mode */}
      {!isNew && (
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-5">
            <h2 className="font-semibold text-gray-700">{t('roles.permissions')}</h2>
            {canManage && (
              <button
                onClick={handleSavePerms}
                disabled={savingPerms || !permsDirty}
                className="flex items-center gap-2 px-4 py-1.5 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {savingPerms && <Spinner size="sm" />}
                {t('roles.savePerms')} {permsDirty && '●'}
              </button>
            )}
          </div>

          <div className="space-y-4">
            {allGroups.map(({ module, permissions }) => {
              const ids    = permissions.map((p) => p.id)
              const allOn  = ids.every((id) => checked.has(id))
              const someOn = ids.some((id) => checked.has(id))

              return (
                <div key={module} className="border border-gray-100 rounded-lg overflow-hidden">
                  {/* Module header */}
                  <div
                    className="flex items-center justify-between px-4 py-2.5 bg-gray-50 cursor-pointer select-none"
                    onClick={() => toggleModule(permissions)}
                  >
                    <div className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={allOn}
                        ref={(el) => { if (el) el.indeterminate = someOn && !allOn }}
                        onChange={() => toggleModule(permissions)}
                        disabled={!canManage}
                        className="accent-green-600"
                        onClick={(e) => e.stopPropagation()}
                      />
                      <span className="text-sm font-semibold text-gray-700">
                        {MODULE_LABEL[module] ?? module}
                      </span>
                    </div>
                    <span className="text-xs text-gray-400">
                      {ids.filter((id) => checked.has(id)).length} / {ids.length}
                    </span>
                  </div>

                  {/* Permissions row */}
                  <div className="flex flex-wrap gap-2 px-4 py-3">
                    {permissions.map((p) => (
                      <label key={p.id} className={`flex items-center gap-1.5 px-2.5 py-1 rounded-lg border text-xs cursor-pointer select-none transition-colors ${
                        checked.has(p.id)
                          ? 'border-green-500 bg-green-50 text-green-700'
                          : 'border-gray-200 bg-white text-gray-500 hover:border-gray-300'
                      }`}>
                        <input type="checkbox" checked={checked.has(p.id)}
                          onChange={() => toggle(p.id)} disabled={!canManage}
                          className="hidden" />
                        {checked.has(p.id) && (
                          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3"><polyline points="20 6 9 17 4 12"/></svg>
                        )}
                        {ACTION_LABEL[p.action] ?? p.action}
                      </label>
                    ))}
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
