import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { raciApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { Spinner } from '../../components/ui/Spinner'
import { PageLoader } from '../../components/ui/Spinner'
import UserIdPicker from '../../components/ui/UserIdPicker'
import useAuthStore from '../../store/authStore'

// ─── RaciForm – Create & Edit ─────────────────────────────────────────────────

export default function RaciForm() {
  const { t }    = useTranslation()
  const { id }   = useParams()
  const isEdit   = !!id
  const navigate = useNavigate()
  const { loading, error, execute } = useApi()
  const { loading: fetching, execute: fetch } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'RACI.UPDATE' : 'RACI.CREATE'
    if (!hasPermission(needed)) navigate('/raci', { replace: true })
  }, [])

  const [form, setForm] = useState({
    titleAr: '', titleEn: '',
    descriptionAr: '', descriptionEn: '',
    departmentId: '',
    activities: [],
  })

  // Load existing data in edit mode
  useEffect(() => {
    if (!isEdit) return
    fetch(
      () => raciApi.getById(id),
      { silent: true }
    ).then((data) => {
      if (!data) return
      setForm({
        titleAr:       data.titleAr       ?? '',
        titleEn:       data.titleEn       ?? '',
        descriptionAr: data.descriptionAr ?? '',
        descriptionEn: data.descriptionEn ?? '',
        departmentId:  data.departmentId  ?? '',
        activities:    (data.activities ?? []).map((a) => ({
          nameAr:             a.nameAr,
          nameEn:             a.nameEn,
          displayOrder:       a.displayOrder,
          responsibleUserIds: (a.responsible ?? []).map((u) => u.id),
          accountableUserId:  a.accountable?.id ?? '',
          consultedUserIds:   (a.consulted ?? []).map((u) => u.id),
          informedUserIds:    (a.informed  ?? []).map((u) => u.id),
        })),
      })
    })
  }, [id])

  // ── Field handlers ────────────────────────────────────────────────────────

  const handleField = (field, value) =>
    setForm((prev) => ({ ...prev, [field]: value }))

  const addActivity = () =>
    setForm((prev) => ({
      ...prev,
      activities: [
        ...prev.activities,
        { nameAr: '', nameEn: '', displayOrder: prev.activities.length,
          responsibleUserIds: [], accountableUserId: '',
          consultedUserIds: [], informedUserIds: [] }
      ]
    }))

  const removeActivity = (idx) =>
    setForm((prev) => ({
      ...prev,
      activities: prev.activities.filter((_, i) => i !== idx)
    }))

  const handleActivity = (idx, field, value) =>
    setForm((prev) => {
      const acts = [...prev.activities]
      acts[idx] = { ...acts[idx], [field]: value }
      return { ...prev, activities: acts }
    })

  // ── Submit ────────────────────────────────────────────────────────────────

  const handleSubmit = async (e) => {
    e.preventDefault()

    const payload = {
      ...form,
      departmentId: form.departmentId ? Number(form.departmentId) : null,
      activities: form.activities.map((a, i) => ({
        ...a,
        displayOrder:       i,
        responsibleUserIds: (a.responsibleUserIds ?? []).map(Number).filter(Boolean),
        accountableUserId:  a.accountableUserId ? Number(a.accountableUserId) : null,
      }))
    }

    const result = await execute(
      () => isEdit ? raciApi.update(id, payload) : raciApi.create(payload),
      { successMsg: isEdit ? t('raci.messages.saved') : t('raci.messages.created') }
    )

    if (result) navigate('/raci')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-3xl space-y-6">

      {/* Header */}
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/raci')}
          className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" strokeWidth="2" strokeLinecap="round">
            <path d="M9 18l6-6-6-6" />
          </svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">
          {isEdit ? t('raci.editTitle') : t('raci.createTitle')}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6" noValidate>

        {/* ── Basic Info ────────────────────────────────── */}
        <Section title={t('raci.form.basicInfo')}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Field label={`${t('raci.form.titleAr')} *`}>
              <input type="text" required
                value={form.titleAr}
                onChange={(e) => handleField('titleAr', e.target.value)}
                className={inputCls}
              />
            </Field>
            <Field label={t('raci.form.titleEn')}>
              <input type="text" dir="ltr"
                value={form.titleEn}
                onChange={(e) => handleField('titleEn', e.target.value)}
                className={inputCls}
              />
            </Field>
            <Field label={t('raci.form.descriptionAr')} className="sm:col-span-2">
              <textarea rows={3}
                value={form.descriptionAr}
                onChange={(e) => handleField('descriptionAr', e.target.value)}
                className={inputCls}
              />
            </Field>
          </div>
        </Section>

        {/* ── Activities ───────────────────────────────── */}
        <Section title={t('raci.form.activities')}>
          {form.activities.length === 0 && (
            <p className="text-sm text-gray-400 text-center py-4">
              {t('raci.form.noActivities')}
            </p>
          )}

          {form.activities.map((act, idx) => (
            <ActivityRow
              key={idx}
              idx={idx}
              act={act}
              t={t}
              onChange={(f, v) => handleActivity(idx, f, v)}
              onRemove={() => removeActivity(idx)}
            />
          ))}

          <button type="button" onClick={addActivity}
            className="flex items-center gap-2 text-sm text-green-700
              hover:text-green-800 font-medium transition-colors">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <path d="M12 5v14M5 12h14" />
            </svg>
            {t('raci.form.addActivity')}
          </button>
        </Section>

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 rounded-xl px-4 py-3 text-sm">
            {error}
          </div>
        )}

        {/* Actions */}
        <div className="flex items-center gap-3">
          <button type="submit" disabled={loading}
            className="flex items-center gap-2 px-6 py-2 bg-green-700 text-white
              text-sm font-medium rounded-lg hover:bg-green-800 transition-colors
              disabled:opacity-60">
            {loading && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('raci.createTitle')}
          </button>
          <button type="button" onClick={() => navigate('/raci')}
            className="px-4 py-2 text-sm text-gray-600 hover:bg-gray-100
              rounded-lg transition-colors">
            {t('common.cancel')}
          </button>
        </div>
      </form>
    </div>
  )
}

// ─── Activity Row ─────────────────────────────────────────────────────────────

function ActivityRow({ idx, act, t, onChange, onRemove }) {
  return (
    <div className="border border-gray-200 rounded-xl p-4 space-y-3 relative">
      <div className="flex items-center justify-between">
        <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
          {t('raci.form.activityN', { n: idx + 1 })}
        </span>
        <button type="button" onClick={onRemove}
          className="text-red-400 hover:text-red-600 transition-colors p-1">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
            <path d="M18 6L6 18M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <Field label={`${t('raci.form.activityNameAr')} *`}>
          <input type="text" required
            value={act.nameAr}
            onChange={(e) => onChange('nameAr', e.target.value)}
            className={inputCls}
          />
        </Field>
        <Field label={t('raci.form.activityNameEn')}>
          <input type="text" dir="ltr"
            value={act.nameEn}
            onChange={(e) => onChange('nameEn', e.target.value)}
            className={inputCls}
          />
        </Field>
        <div>
          <UserIdPicker
            multi
            badge="R"
            label={t('raci.form.r')}
            values={act.responsibleUserIds ?? []}
            onChange={(ids) => onChange('responsibleUserIds', ids)}
          />
        </div>
        <div>
          <UserIdPicker
            badge="A"
            label={t('raci.form.a')}
            value={act.accountableUserId ?? ''}
            onChange={(id) => onChange('accountableUserId', id)}
          />
        </div>
        <div className="sm:col-span-2">
          <UserIdPicker
            multi
            badge="C"
            label={t('raci.form.c')}
            values={act.consultedUserIds ?? []}
            onChange={(ids) => onChange('consultedUserIds', ids)}
          />
        </div>
        <div className="sm:col-span-2">
          <UserIdPicker
            multi
            badge="I"
            label={t('raci.form.i')}
            values={act.informedUserIds ?? []}
            onChange={(ids) => onChange('informedUserIds', ids)}
          />
        </div>
      </div>
    </div>
  )
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const inputCls = `w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
  focus:outline-none focus:ring-2 focus:ring-green-600 bg-white`

function Section({ title, children }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
      <h2 className="text-sm font-semibold text-gray-700 border-b border-gray-100 pb-2">
        {title}
      </h2>
      {children}
    </div>
  )
}

function Field({ label, children, className = '' }) {
  return (
    <div className={`space-y-1 ${className}`}>
      <label className="block text-xs font-medium text-gray-600">{label}</label>
      {children}
    </div>
  )
}
