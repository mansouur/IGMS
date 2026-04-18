import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { departmentApi } from '../../services/departmentApi'
import { userApi } from '../../services/userApi'
import { useApi } from '../../hooks/useApi'
import { Spinner, PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

export default function DepartmentForm() {
  const { t } = useTranslation()
  const { id }   = useParams()
  const isEdit   = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }    = useApi()
  const { loading: fetching, execute: fetch }  = useApi()
  const { loading: treeLoading, execute: fetchTree } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'DEPARTMENTS.UPDATE' : 'DEPARTMENTS.CREATE'
    if (!hasPermission(needed)) navigate('/departments', { replace: true })
  }, [])

  const [form, setForm] = useState({
    nameAr:        '',
    nameEn:        '',
    code:          '',
    descriptionAr: '',
    descriptionEn: '',
    parentId:      '',
    managerId:     '',
    isActive:      true,
  })

  const [tree,  setTree]  = useState([])
  const [users, setUsers] = useState([])

  useEffect(() => {
    fetchTree(() => departmentApi.getTree(), { silent: true })
      .then((data) => { if (data) setTree(flattenTree(data)) })
    userApi.getLookup()
      .then((r) => setUsers(r.data?.data ?? []))
      .catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetch(() => departmentApi.getById(id), { silent: true })
      .then((data) => {
        if (!data) return
        setForm({
          nameAr:        data.nameAr        ?? '',
          nameEn:        data.nameEn        ?? '',
          code:          data.code          ?? '',
          descriptionAr: data.descriptionAr ?? '',
          descriptionEn: data.descriptionEn ?? '',
          parentId:      data.parentId      ?? '',
          managerId:     data.managerId     ?? '',
          isActive:      data.isActive,
        })
      })
  }, [id])

  const setField = (field, value) => setForm((prev) => ({ ...prev, [field]: value }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      nameAr:        form.nameAr.trim(),
      nameEn:        form.nameEn.trim(),
      code:          form.code.trim().toUpperCase(),
      descriptionAr: form.descriptionAr.trim() || null,
      descriptionEn: form.descriptionEn.trim() || null,
      parentId:      form.parentId  !== '' ? Number(form.parentId)  : null,
      managerId:     form.managerId !== '' ? Number(form.managerId) : null,
      isActive:      form.isActive,
    }
    const result = isEdit
      ? await execute(() => departmentApi.update(id, payload), { successMsg: t('departments.messages.saved') })
      : await execute(() => departmentApi.create(payload),     { successMsg: t('departments.messages.created') })
    if (result) navigate(`/departments/${result.id}`)
  }

  if (fetching || treeLoading) return <PageLoader />

  const parentOptions = isEdit ? tree.filter((d) => d.id !== Number(id)) : tree

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/departments')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
            <path d="M9 18l6-6-6-6" />
          </svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">
          {isEdit ? t('departments.editTitle') : t('departments.createTitle')}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('departments.fields.nameAr')} required>
            <input type="text" value={form.nameAr} required
              onChange={(e) => setField('nameAr', e.target.value)}
              className={inputCls} placeholder="مثال: إدارة تقنية المعلومات" />
          </Field>
          <Field label={t('departments.fields.nameEn')}>
            <input type="text" value={form.nameEn}
              onChange={(e) => setField('nameEn', e.target.value)}
              className={inputCls} placeholder="e.g. Information Technology" dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('departments.fields.code')} required>
            <input type="text" value={form.code} required maxLength={20}
              onChange={(e) => setField('code', e.target.value.toUpperCase())}
              className={`${inputCls} font-mono`} placeholder="مثال: IT" />
            <p className="text-xs text-gray-400 mt-1">{t('departments.fields.codeHint')}</p>
          </Field>
          <Field label={t('departments.fields.parent')}>
            <select value={form.parentId} onChange={(e) => setField('parentId', e.target.value)} className={inputCls}>
              <option value="">{t('departments.fields.noParent')}</option>
              {parentOptions.map((d) => (
                <option key={d.id} value={d.id}>
                  {'　'.repeat(d.depth)}{d.nameAr} ({d.code})
                </option>
              ))}
            </select>
          </Field>
        </div>

        <Field label={t('departments.fields.manager')}>
          <select value={form.managerId} onChange={(e) => setField('managerId', e.target.value)} className={inputCls}>
            <option value="">{t('departments.fields.noManager')}</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>{u.fullNameAr} ({u.username})</option>
            ))}
          </select>
        </Field>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label={t('departments.fields.descriptionAr')}>
            <textarea value={form.descriptionAr} rows={3}
              onChange={(e) => setField('descriptionAr', e.target.value)}
              className={inputCls} placeholder="وصف مختصر للقسم..." />
          </Field>
          <Field label={t('departments.fields.descriptionEn')}>
            <textarea value={form.descriptionEn} rows={3}
              onChange={(e) => setField('descriptionEn', e.target.value)}
              className={inputCls} placeholder="Brief description..." dir="ltr" />
          </Field>
        </div>

        <div className="flex items-center gap-3">
          <button type="button" onClick={() => setField('isActive', !form.isActive)}
            className={['relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
              form.isActive ? 'bg-green-600' : 'bg-gray-300'].join(' ')}>
            <span className={['inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
              form.isActive ? 'translate-x-6' : 'translate-x-1'].join(' ')} />
          </button>
          <span className="text-sm text-gray-700">{t('departments.fields.isActive')}</span>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>
        )}

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={() => navigate('/departments')}
            className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50 transition-colors">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving}
            className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 transition-colors disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('departments.createTitle')}
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

function flattenTree(nodes, depth = 0, result = []) {
  for (const node of nodes) {
    result.push({ id: node.id, nameAr: node.nameAr, code: node.code, depth })
    if (node.children?.length) flattenTree(node.children, depth + 1, result)
  }
  return result
}
