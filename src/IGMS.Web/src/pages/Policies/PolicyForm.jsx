import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { policyApi } from '../../services/governanceApi'
import { departmentApi } from '../../services/departmentApi'
import { userApi } from '../../services/userApi'
import { useApi } from '../../hooks/useApi'
import { Spinner, PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'

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

export default function PolicyForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }     = useApi()
  const { loading: fetching, execute: fetchItem } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canPublish = hasPermission('POLICIES.PUBLISH')
  const canApprove = hasPermission('POLICIES.APPROVE')

  const [form, setForm] = useState({
    titleAr: '', titleEn: '', code: '', descriptionAr: '', descriptionEn: '',
    category: 0, status: 0, effectiveDate: '', expiryDate: '',
    departmentId: '', ownerId: '', approverId: '',
  })
  const [departments, setDepartments] = useState([])
  const [users,       setUsers]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 }).then((r) => setDepartments(r.data?.data?.items ?? [])).catch(() => {})
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchItem(() => policyApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        titleAr: data.titleAr ?? '', titleEn: data.titleEn ?? '', code: data.code ?? '',
        descriptionAr: data.descriptionAr ?? '', descriptionEn: data.descriptionEn ?? '',
        category: data.category ?? 0, status: data.status ?? 0,
        effectiveDate: data.effectiveDate ? data.effectiveDate.split('T')[0] : '',
        expiryDate:    data.expiryDate    ? data.expiryDate.split('T')[0]    : '',
        departmentId:  data.departmentId ?? '',
        ownerId:       data.ownerId      ?? '',
        approverId:    data.approverId   ?? '',
      })
    })
  }, [id])

  const set = (f, v) => setForm((p) => ({ ...p, [f]: v }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      titleAr: form.titleAr.trim(), titleEn: form.titleEn.trim(), code: form.code.trim().toUpperCase(),
      descriptionAr: form.descriptionAr || null, descriptionEn: form.descriptionEn || null,
      category: Number(form.category), status: Number(form.status),
      effectiveDate: form.effectiveDate || null, expiryDate: form.expiryDate || null,
      departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
      ownerId:      form.ownerId      !== '' ? Number(form.ownerId)      : null,
      approverId:   form.approverId   !== '' ? Number(form.approverId)   : null,
    }
    const result = isEdit
      ? await execute(() => policyApi.update(id, payload), { successMsg: t('policies.messages.saved') })
      : await execute(() => policyApi.create(payload),     { successMsg: t('policies.messages.created') })
    if (result) navigate('/policies')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/policies')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 18l6-6-6-6"/></svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? t('policies.editTitle') : t('policies.createTitle')}</h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <Field label={t('policies.fields.titleAr')} required>
            <input type="text" value={form.titleAr} required onChange={(e) => set('titleAr', e.target.value)} className={inputCls} />
          </Field>
          <Field label={t('policies.fields.titleEn')}>
            <input type="text" value={form.titleEn} onChange={(e) => set('titleEn', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label={t('policies.fields.code')} required>
            <input type="text" value={form.code} required onChange={(e) => set('code', e.target.value.toUpperCase())} className={`${inputCls} font-mono`} />
          </Field>
          <Field label={t('policies.fields.category')}>
            <select value={form.category} onChange={(e) => set('category', e.target.value)} className={inputCls}>
              <option value="0">{t('policies.category.governance')}</option>
              <option value="1">{t('policies.category.technology')}</option>
              <option value="2">{t('policies.category.hr')}</option>
              <option value="3">{t('policies.category.finance')}</option>
              <option value="4">{t('policies.category.operational')}</option>
            </select>
          </Field>
          <Field label={t('policies.fields.status')}>
            <select value={form.status} onChange={(e) => set('status', e.target.value)} className={inputCls}>
              <option value="0">{t('policies.status.draft')}</option>
              {canPublish && <option value="1">{t('policies.status.active')}</option>}
              {canApprove && <option value="2">{t('policies.status.archived')}</option>}
            </select>
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('policies.fields.effectiveDate')}>
            <input type="date" value={form.effectiveDate} onChange={(e) => set('effectiveDate', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
          <Field label={t('policies.fields.expiryDate')}>
            <input type="date" value={form.expiryDate} onChange={(e) => set('expiryDate', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('policies.fields.department')}>
            <select value={form.departmentId} onChange={(e) => set('departmentId', e.target.value)} className={inputCls}>
              <option value="">{t('policies.fields.chooseDept')}</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
            </select>
          </Field>
          <Field label={t('policies.fields.owner')}>
            <select value={form.ownerId} onChange={(e) => set('ownerId', e.target.value)} className={inputCls}>
              <option value="">{t('policies.fields.chooseOwner')}</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.fullNameAr}</option>)}
            </select>
          </Field>
        </div>

        {/* Approver — required only when publishing (status = Active) */}
        {Number(form.status) === 1 && (
          <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4">
            <Field label={t('policies.publish.approver')} required>
              <select
                value={form.approverId}
                onChange={(e) => set('approverId', e.target.value)}
                className={inputCls}
                required
              >
                <option value="">{t('policies.publish.chooseApprover')}</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>{u.fullNameAr} ({u.username})</option>
                ))}
              </select>
              <p className="text-xs text-emerald-700 mt-1">{t('policies.publish.approverHint')}</p>
            </Field>
          </div>
        )}


        <Field label={t('policies.fields.description')}>
          <textarea value={form.descriptionAr} onChange={(e) => set('descriptionAr', e.target.value)} rows={3} className={inputCls} />
        </Field>

        {error && <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>}

        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => navigate('/policies')} className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('policies.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}
