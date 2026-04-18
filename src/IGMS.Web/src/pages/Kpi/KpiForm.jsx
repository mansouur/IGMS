import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { kpiApi } from '../../services/governanceApi'
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

export default function KpiForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }       = useApi()
  const { loading: fetching, execute: fetchItem } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'KPI.UPDATE' : 'KPI.CREATE'
    if (!hasPermission(needed)) navigate('/kpi', { replace: true })
  }, [])

  const [form, setForm] = useState({
    titleAr: '', titleEn: '', code: '', unit: '',
    targetValue: '', actualValue: '',
    year: new Date().getFullYear(), quarter: '',
    status: 0, departmentId: '', ownerId: '',
  })
  const [departments, setDepartments] = useState([])
  const [users,       setUsers]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 }).then((r) => setDepartments(r.data?.data?.items ?? [])).catch(() => {})
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchItem(() => kpiApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        titleAr: data.titleAr ?? '', titleEn: data.titleEn ?? '', code: data.code ?? '',
        unit: data.unit ?? '', targetValue: data.targetValue ?? '',
        actualValue: data.actualValue ?? '', year: data.year ?? new Date().getFullYear(),
        quarter: data.quarter ?? '', status: data.status ?? 0,
        departmentId: data.departmentId ?? '', ownerId: data.ownerId ?? '',
      })
    })
  }, [id])

  const set = (f, v) => setForm((p) => ({ ...p, [f]: v }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      titleAr: form.titleAr.trim(), titleEn: form.titleEn.trim(),
      code: form.code.trim().toUpperCase(), unit: form.unit || null,
      targetValue: Number(form.targetValue), actualValue: Number(form.actualValue),
      year: Number(form.year), quarter: form.quarter !== '' ? Number(form.quarter) : null,
      status: Number(form.status),
      departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
      ownerId:      form.ownerId      !== '' ? Number(form.ownerId)      : null,
    }
    const result = isEdit
      ? await execute(() => kpiApi.update(id, payload), { successMsg: t('kpi.messages.saved') })
      : await execute(() => kpiApi.create(payload),     { successMsg: t('kpi.messages.created') })
    if (result) navigate('/kpi')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/kpi')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 18l6-6-6-6"/></svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? t('kpi.editTitle') : t('kpi.createTitle')}</h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <Field label={t('kpi.fields.titleAr')} required>
            <input type="text" value={form.titleAr} required onChange={(e) => set('titleAr', e.target.value)} className={inputCls} />
          </Field>
          <Field label={t('kpi.fields.code')} required>
            <input type="text" value={form.code} required onChange={(e) => set('code', e.target.value.toUpperCase())} className={`${inputCls} font-mono`} />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label={t('kpi.fields.unit')}>
            <input type="text" value={form.unit} onChange={(e) => set('unit', e.target.value)} className={inputCls} placeholder={t('kpi.fields.unitPlaceholder')} />
          </Field>
          <Field label={t('kpi.fields.targetValue')} required>
            <input type="number" value={form.targetValue} required onChange={(e) => set('targetValue', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
          <Field label={t('kpi.fields.actualValue')} required>
            <input type="number" value={form.actualValue} required onChange={(e) => set('actualValue', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label={t('kpi.fields.year')} required>
            <input type="number" value={form.year} required onChange={(e) => set('year', e.target.value)} className={inputCls} dir="ltr" min="2020" max="2030" />
          </Field>
          <Field label={t('kpi.fields.quarter')}>
            <select value={form.quarter} onChange={(e) => set('quarter', e.target.value)} className={inputCls}>
              <option value="">{t('kpi.fields.annual')}</option>
              <option value="1">{t('kpi.fields.q1')}</option>
              <option value="2">{t('kpi.fields.q2')}</option>
              <option value="3">{t('kpi.fields.q3')}</option>
              <option value="4">{t('kpi.fields.q4')}</option>
            </select>
          </Field>
          <Field label={t('kpi.fields.status')}>
            <select value={form.status} onChange={(e) => set('status', e.target.value)} className={inputCls}>
              <option value="0">{t('kpi.status.onTrack')}</option>
              <option value="1">{t('kpi.status.atRisk')}</option>
              <option value="2">{t('kpi.status.behind')}</option>
            </select>
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('kpi.fields.department')}>
            <select value={form.departmentId} onChange={(e) => set('departmentId', e.target.value)} className={inputCls}>
              <option value="">{t('common.choose')}</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
            </select>
          </Field>
          <Field label={t('kpi.fields.owner')}>
            <select value={form.ownerId} onChange={(e) => set('ownerId', e.target.value)} className={inputCls}>
              <option value="">{t('common.choose')}</option>
              {users.map((u) => <option key={u.id} value={u.id}>{u.fullNameAr}</option>)}
            </select>
          </Field>
        </div>

        {error && <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>}

        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => navigate('/kpi')} className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('kpi.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}
