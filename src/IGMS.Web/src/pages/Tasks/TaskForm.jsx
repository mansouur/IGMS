import { useState, useEffect } from 'react'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { taskApi } from '../../services/governanceApi'
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

export default function TaskForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const location = useLocation()
  const { loading: saving, error, execute }       = useApi()
  const { loading: fetching, execute: fetchItem } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canAssign = hasPermission('TASKS.ASSIGN')

  // riskId passed from RiskDetail via navigate state
  const linkedRiskId    = location.state?.riskId    ?? null
  const linkedRiskTitle = location.state?.riskTitleAr ?? null

  useEffect(() => {
    const needed = isEdit ? 'TASKS.UPDATE' : 'TASKS.CREATE'
    if (!hasPermission(needed)) navigate('/tasks', { replace: true })
  }, [])

  const [form, setForm] = useState({
    titleAr: '', titleEn: '', descriptionAr: '',
    status: 0, priority: 1, dueDate: '', assignedToId: '', departmentId: '',
  })
  const [departments, setDepartments] = useState([])
  const [users,       setUsers]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 }).then((r) => setDepartments(r.data?.data?.items ?? [])).catch(() => {})
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchItem(() => taskApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        titleAr: data.titleAr ?? '', titleEn: data.titleEn ?? '',
        descriptionAr: data.descriptionAr ?? '',
        status: data.status ?? 0, priority: data.priority ?? 1,
        dueDate: data.dueDate ? data.dueDate.split('T')[0] : '',
        assignedToId: data.assignedToId ?? '', departmentId: data.departmentId ?? '',
      })
    })
  }, [id])

  const set = (f, v) => setForm((p) => ({ ...p, [f]: v }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      titleAr: form.titleAr.trim(), titleEn: form.titleEn.trim(),
      descriptionAr: form.descriptionAr || null,
      status: Number(form.status), priority: Number(form.priority),
      dueDate:      form.dueDate      !== '' ? form.dueDate      : null,
      assignedToId: form.assignedToId !== '' ? Number(form.assignedToId) : null,
      departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
      riskId:       linkedRiskId,
    }
    const result = isEdit
      ? await execute(() => taskApi.update(id, payload), { successMsg: t('tasks.messages.saved') })
      : await execute(() => taskApi.create(payload),     { successMsg: t('tasks.messages.created') })
    if (result) {
      // If we came from a risk, go back to it
      if (linkedRiskId) navigate(`/risks/${linkedRiskId}`)
      else navigate('/tasks')
    }
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => linkedRiskId ? navigate(`/risks/${linkedRiskId}`) : navigate('/tasks')}
          className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 18l6-6-6-6"/></svg>
        </button>
        <div>
          <h1 className="text-xl font-bold text-gray-800">{isEdit ? t('tasks.editTitle') : t('tasks.createTitle')}</h1>
          {linkedRiskTitle && (
            <p className="text-xs text-blue-600 mt-0.5 flex items-center gap-1">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M10 13a5 5 0 007.54.54l3-3a5 5 0 00-7.07-7.07l-1.72 1.71M14 11a5 5 0 00-7.54-.54l-3 3a5 5 0 007.07 7.07l1.71-1.71"/></svg>
              مرتبط بالمخاطرة: {linkedRiskTitle}
            </p>
          )}
        </div>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <Field label={t('tasks.fields.titleAr')} required>
            <input type="text" value={form.titleAr} required onChange={(e) => set('titleAr', e.target.value)} className={inputCls} />
          </Field>
          <Field label={t('tasks.fields.titleEn')}>
            <input type="text" value={form.titleEn} onChange={(e) => set('titleEn', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label={t('tasks.fields.status')}>
            <select value={form.status} onChange={(e) => set('status', e.target.value)} className={inputCls}>
              <option value="0">{t('tasks.status.pending')}</option>
              <option value="1">{t('tasks.status.inProgress')}</option>
              <option value="2">{t('tasks.status.done')}</option>
              <option value="3">{t('tasks.status.cancelled')}</option>
            </select>
          </Field>
          <Field label={t('tasks.fields.priority')}>
            <select value={form.priority} onChange={(e) => set('priority', e.target.value)} className={inputCls}>
              <option value="0">{t('tasks.priority.low')}</option>
              <option value="1">{t('tasks.priority.medium')}</option>
              <option value="2">{t('tasks.priority.high')}</option>
              <option value="3">{t('tasks.priority.critical')}</option>
            </select>
          </Field>
          <Field label={t('tasks.fields.dueDate')}>
            <input type="date" value={form.dueDate} onChange={(e) => set('dueDate', e.target.value)} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          {canAssign ? (
            <Field label={t('tasks.fields.assignedTo')}>
              <select value={form.assignedToId} onChange={(e) => set('assignedToId', e.target.value)} className={inputCls}>
                <option value="">{t('common.choose')}</option>
                {users.map((u) => <option key={u.id} value={u.id}>{u.fullNameAr}</option>)}
              </select>
            </Field>
          ) : (
            <Field label={t('tasks.fields.assignedTo')}>
              <input type="text" readOnly disabled
                value={users.find((u) => String(u.id) === String(form.assignedToId))?.fullNameAr ?? '—'}
                className={`${inputCls} bg-gray-50 cursor-not-allowed text-gray-500`} />
            </Field>
          )}
          <Field label={t('tasks.fields.department')}>
            <select value={form.departmentId} onChange={(e) => set('departmentId', e.target.value)} className={inputCls}>
              <option value="">{t('common.choose')}</option>
              {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
            </select>
          </Field>
        </div>

        <Field label={t('tasks.fields.description')}>
          <textarea value={form.descriptionAr} onChange={(e) => set('descriptionAr', e.target.value)} rows={3} className={inputCls} />
        </Field>

        {error && <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>}

        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => navigate('/tasks')} className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('tasks.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}
