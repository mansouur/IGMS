import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { riskApi } from '../../services/governanceApi'
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

export default function RiskForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }       = useApi()
  const { loading: fetching, execute: fetchItem } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'RISKS.UPDATE' : 'RISKS.CREATE'
    if (!hasPermission(needed)) navigate('/risks', { replace: true })
  }, [])

  const [form, setForm] = useState({
    titleAr: '', titleEn: '', code: '', descriptionAr: '', mitigationPlanAr: '',
    category: 0, status: 0, likelihood: 3, impact: 3, departmentId: '', ownerId: '',
  })
  const [departments, setDepartments] = useState([])
  const [users,       setUsers]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 }).then((r) => setDepartments(r.data?.data?.items ?? [])).catch(() => {})
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchItem(() => riskApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        titleAr: data.titleAr ?? '', titleEn: data.titleEn ?? '', code: data.code ?? '',
        descriptionAr: data.descriptionAr ?? '', mitigationPlanAr: data.mitigationPlanAr ?? '',
        category: data.category ?? 0, status: data.status ?? 0,
        likelihood: data.likelihood ?? 3, impact: data.impact ?? 3,
        departmentId: data.departmentId ?? '', ownerId: data.ownerId ?? '',
      })
    })
  }, [id])

  const set = (f, v) => setForm((p) => ({ ...p, [f]: v }))
  const score = form.likelihood * form.impact

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      titleAr: form.titleAr.trim(), titleEn: form.titleEn.trim(), code: form.code.trim().toUpperCase(),
      descriptionAr: form.descriptionAr || null, mitigationPlanAr: form.mitigationPlanAr || null,
      category: Number(form.category), status: Number(form.status),
      likelihood: Number(form.likelihood), impact: Number(form.impact),
      departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
      ownerId:      form.ownerId      !== '' ? Number(form.ownerId)      : null,
    }
    const result = isEdit
      ? await execute(() => riskApi.update(id, payload), { successMsg: t('risks.messages.saved') })
      : await execute(() => riskApi.create(payload),     { successMsg: t('risks.messages.created') })
    if (result) navigate('/risks')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/risks')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 18l6-6-6-6"/></svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? t('risks.editTitle') : t('risks.createTitle')}</h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <Field label={t('risks.fields.titleAr')} required>
            <input type="text" value={form.titleAr} required onChange={(e) => set('titleAr', e.target.value)} className={inputCls} />
          </Field>
          <Field label={t('risks.fields.code')} required>
            <input type="text" value={form.code} required onChange={(e) => set('code', e.target.value.toUpperCase())} className={`${inputCls} font-mono`} />
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('risks.fields.category')}>
            <select value={form.category} onChange={(e) => set('category', e.target.value)} className={inputCls}>
              <option value="0">{t('risks.category.operational')}</option>
              <option value="1">{t('risks.category.financial')}</option>
              <option value="2">{t('risks.category.technology')}</option>
              <option value="3">{t('risks.category.legal')}</option>
              <option value="4">{t('risks.category.strategic')}</option>
            </select>
          </Field>
          <Field label={t('risks.fields.status')}>
            <select value={form.status} onChange={(e) => set('status', e.target.value)} className={inputCls}>
              <option value="0">{t('risks.status.open')}</option>
              <option value="1">{t('risks.status.mitigated')}</option>
              <option value="2">{t('risks.status.closed')}</option>
            </select>
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4 items-end">
          <Field label={t('risks.fields.likelihood', { val: form.likelihood })}>
            <input type="range" min="1" max="5" value={form.likelihood} onChange={(e) => set('likelihood', e.target.value)} className="w-full accent-green-600" />
          </Field>
          <Field label={t('risks.fields.impact', { val: form.impact })}>
            <input type="range" min="1" max="5" value={form.impact} onChange={(e) => set('impact', e.target.value)} className="w-full accent-green-600" />
          </Field>
          <div className="text-center">
            <p className="text-xs text-gray-500 mb-1">{t('risks.fields.riskScore')}</p>
            <span className={`inline-flex items-center justify-center w-12 h-12 rounded-full text-lg font-bold ${score >= 15 ? 'bg-red-100 text-red-700' : score >= 8 ? 'bg-amber-100 text-amber-700' : 'bg-emerald-100 text-emerald-700'}`}>
              {score}
            </span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('risks.fields.department')}>
            <select value={form.departmentId} onChange={(e) => set('departmentId', e.target.value)} className={inputCls}>
              <option value="">{t('common.choose')}</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
            </select>
          </Field>
          <Field label={t('risks.fields.owner')}>
            <select value={form.ownerId} onChange={(e) => set('ownerId', e.target.value)} className={inputCls}>
              <option value="">{t('common.choose')}</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.fullNameAr}</option>)}
            </select>
          </Field>
        </div>

        <Field label={t('risks.fields.description')}>
          <textarea value={form.descriptionAr} onChange={(e) => set('descriptionAr', e.target.value)} rows={2} className={inputCls} />
        </Field>
        <Field label={t('risks.fields.mitigationPlan')}>
          <textarea value={form.mitigationPlanAr} onChange={(e) => set('mitigationPlanAr', e.target.value)} rows={2} className={inputCls} />
        </Field>

        {error && <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>}

        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => navigate('/risks')} className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('risks.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}
