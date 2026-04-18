import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { userApi } from '../../services/userApi'
import { departmentApi } from '../../services/departmentApi'
import api from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { Spinner, PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

export default function UserForm() {
  const { t } = useTranslation()
  const { id }   = useParams()
  const isEdit   = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }       = useApi()
  const { loading: fetching, execute: fetchUser } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'USERS.UPDATE' : 'USERS.CREATE'
    if (!hasPermission(needed)) navigate('/users', { replace: true })
  }, [])

  const [form, setForm] = useState({
    fullNameAr:   '',
    fullNameEn:   '',
    username:     '',
    email:        '',
    password:     '',
    phoneNumber:  '',
    departmentId: '',
    roleIds:      [],
    isActive:     true,
  })

  const [departments, setDepartments] = useState([])
  const [roles,       setRoles]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 })
      .then((r) => setDepartments(r.data?.data?.items ?? []))
      .catch(() => {})

    api.get('/api/v1/roles/lookup')
      .then((r) => setRoles(r.data?.data ?? []))
      .catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchUser(() => userApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        fullNameAr:   data.fullNameAr   ?? '',
        fullNameEn:   data.fullNameEn   ?? '',
        username:     data.username     ?? '',
        email:        data.email        ?? '',
        password:     '',
        phoneNumber:  data.phoneNumber  ?? '',
        departmentId: data.departmentId ?? '',
        roleIds:      data.roles
          ? roles.filter((r) => data.roles.includes(r.nameAr)).map((r) => r.id)
          : [],
        isActive:     data.isActive,
      })
    })
  }, [id, roles.length])

  const setField = (field, value) => setForm((prev) => ({ ...prev, [field]: value }))

  const toggleRole = (roleId) => {
    setForm((prev) => ({
      ...prev,
      roleIds: prev.roleIds.includes(roleId)
        ? prev.roleIds.filter((r) => r !== roleId)
        : [...prev.roleIds, roleId],
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    const payload = isEdit
      ? {
          fullNameAr:   form.fullNameAr.trim(),
          fullNameEn:   form.fullNameEn.trim(),
          email:        form.email.trim(),
          phoneNumber:  form.phoneNumber.trim() || null,
          departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
          roleIds:      form.roleIds,
          isActive:     form.isActive,
        }
      : {
          username:     form.username.trim(),
          fullNameAr:   form.fullNameAr.trim(),
          fullNameEn:   form.fullNameEn.trim(),
          email:        form.email.trim(),
          password:     form.password || null,
          phoneNumber:  form.phoneNumber.trim() || null,
          departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
          roleIds:      form.roleIds,
          isActive:     form.isActive,
        }

    const result = isEdit
      ? await execute(() => userApi.update(id, payload), { successMsg: t('users.messages.saved') })
      : await execute(() => userApi.create(payload),     { successMsg: t('users.messages.created') })

    if (result) navigate('/users')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/users')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
            <path d="M9 18l6-6-6-6" />
          </svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">
          {isEdit ? t('users.editTitle') : t('users.createTitle')}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('users.fields.fullNameAr')} required>
            <input type="text" value={form.fullNameAr} required
              onChange={(e) => setField('fullNameAr', e.target.value)}
              className={inputCls} placeholder="مثال: محمد الكعبي" />
          </Field>
          <Field label={t('users.fields.fullNameEn')}>
            <input type="text" value={form.fullNameEn}
              onChange={(e) => setField('fullNameEn', e.target.value)}
              className={inputCls} placeholder="e.g. Mohammed Al Kaabi" dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('users.fields.username')} required>
            <input type="text" value={form.username} required={!isEdit}
              readOnly={isEdit}
              onChange={(e) => !isEdit && setField('username', e.target.value)}
              className={`${inputCls} font-mono ${isEdit ? 'bg-gray-50 text-gray-500' : ''}`}
              placeholder="مثال: m.alkaabi"
              dir="ltr" />
          </Field>
          <Field label={t('users.fields.email')} required>
            <input type="email" value={form.email} required
              onChange={(e) => setField('email', e.target.value)}
              className={inputCls} placeholder="example@mosc.gov.ae" dir="ltr" />
          </Field>
        </div>

        {!isEdit && (
          <Field label={t('users.fields.password')}>
            <input type="password" value={form.password}
              onChange={(e) => setField('password', e.target.value)}
              className={inputCls} placeholder={t('users.fields.passwordPlaceholder')} dir="ltr" />
            <p className="text-xs text-gray-400 mt-1">{t('users.fields.passwordHint')}</p>
          </Field>
        )}

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('users.fields.department')}>
            <select value={form.departmentId}
              onChange={(e) => setField('departmentId', e.target.value)}
              className={inputCls}>
              <option value="">{t('users.fields.noDept')}</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.nameAr}</option>
              ))}
            </select>
          </Field>
          <Field label={t('users.fields.phone')}>
            <input type="text" value={form.phoneNumber}
              onChange={(e) => setField('phoneNumber', e.target.value)}
              className={inputCls} placeholder="+971 50 000 0000" dir="ltr" />
          </Field>
        </div>

        {roles.length > 0 && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">{t('users.fields.roles')}</label>
            <div className="flex flex-wrap gap-2">
              {roles.map((r) => {
                const selected = form.roleIds.includes(r.id)
                return (
                  <button key={r.id} type="button" onClick={() => toggleRole(r.id)}
                    className={['px-3 py-1.5 rounded-full text-sm font-medium border transition-colors',
                      selected ? 'bg-green-700 border-green-700 text-white' : 'bg-white border-gray-300 text-gray-600 hover:border-green-600',
                    ].join(' ')}>
                    {r.nameAr}
                  </button>
                )
              })}
            </div>
          </div>
        )}

        <div className="flex items-center gap-3">
          <button type="button" onClick={() => setField('isActive', !form.isActive)}
            className={['relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
              form.isActive ? 'bg-green-600' : 'bg-gray-300'].join(' ')}>
            <span className={['inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
              form.isActive ? 'translate-x-6' : 'translate-x-1'].join(' ')} />
          </button>
          <span className="text-sm text-gray-700">{t('users.fields.isActive')}</span>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>
        )}

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={() => navigate('/users')}
            className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50 transition-colors">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving}
            className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 transition-colors disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('users.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}

const inputCls = `w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white`

function Field({ label, children, required }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label}{required && <span className="text-red-500 ms-1">*</span>}
      </label>
      {children}
    </div>
  )
}
