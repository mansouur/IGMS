import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { incidentsApi } from '../../services/api'
import { departmentApi } from '../../services/departmentApi'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'

const inputCls  = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 bg-white'
const selectCls = inputCls

const SEVERITIES = [
  { value: 'Low',      labelAr: 'منخفض' },
  { value: 'Medium',   labelAr: 'متوسط' },
  { value: 'High',     labelAr: 'عالٍ' },
  { value: 'Critical', labelAr: 'حرج' },
]

const STATUSES = [
  { value: 'Open',        labelAr: 'مفتوح' },
  { value: 'UnderReview', labelAr: 'قيد المراجعة' },
  { value: 'Resolved',    labelAr: 'محلول' },
  { value: 'Closed',      labelAr: 'مغلق' },
]

const today = () => new Date().toISOString().slice(0, 16)

export default function IncidentForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isNew   = id === 'new'
  const navigate = useNavigate()

  const { loading: fetching, execute: fetchEx } = useApi()
  const { loading: saving,   execute: saveEx  } = useApi()

  const [departments, setDepartments] = useState([])
  const [form, setForm] = useState({
    titleAr:       '',
    titleEn:       '',
    descriptionAr: '',
    severity:      'Medium',
    status:        'Open',
    occurredAt:    today(),
    departmentId:  '',
    riskId:        '',
    taskId:        '',
    resolutionNotes: '',
  })

  useEffect(() => {
    departmentApi.getAll().then((r) => setDepartments(r?.data?.data ?? []))

    if (!isNew) {
      fetchEx(() => incidentsApi.getById(id), { silent: true }).then((r) => {
        if (!r) return
        setForm({
          titleAr:         r.titleAr,
          titleEn:         r.titleEn ?? '',
          descriptionAr:   r.descriptionAr ?? '',
          severity:        r.severity,
          status:          r.status,
          occurredAt:      new Date(r.occurredAt).toISOString().slice(0, 16),
          departmentId:    r.departmentId ?? '',
          riskId:          r.riskId ?? '',
          taskId:          r.taskId ?? '',
          resolutionNotes: r.resolutionNotes ?? '',
        })
      })
    }
  }, [id])

  const setField = (k, v) => setForm((f) => ({ ...f, [k]: v }))

  const handleSave = async () => {
    const payload = {
      titleAr:        form.titleAr,
      titleEn:        form.titleEn || null,
      descriptionAr:  form.descriptionAr || null,
      severity:       form.severity,
      status:         form.status,
      occurredAt:     new Date(form.occurredAt).toISOString(),
      departmentId:   form.departmentId ? Number(form.departmentId) : null,
      riskId:         form.riskId ? Number(form.riskId) : null,
      taskId:         form.taskId ? Number(form.taskId) : null,
      resolutionNotes: form.resolutionNotes || null,
    }

    const result = isNew
      ? await saveEx(() => incidentsApi.create(payload), { successMsg: t('incidents.messages.created') })
      : await saveEx(() => incidentsApi.update(id, payload), { successMsg: t('incidents.messages.updated') })

    if (result) navigate('/incidents')
  }

  if (fetching && !isNew) return <PageLoader />

  return (
    <div className="space-y-6 max-w-2xl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">
            {isNew ? t('incidents.newTitle') : t('incidents.editTitle')}
          </h1>
        </div>
        <button onClick={() => navigate('/incidents')}
          className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('common.back')}
        </button>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
        {/* Title */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.titleAr')} *</label>
            <input className={inputCls} value={form.titleAr}
              onChange={(e) => setField('titleAr', e.target.value)} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.titleEn')}</label>
            <input className={inputCls} value={form.titleEn} dir="ltr"
              onChange={(e) => setField('titleEn', e.target.value)} />
          </div>
        </div>

        {/* Description */}
        <div>
          <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.description')}</label>
          <textarea rows={3} className={inputCls} value={form.descriptionAr}
            onChange={(e) => setField('descriptionAr', e.target.value)} />
        </div>

        {/* Severity + Status */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.severity')} *</label>
            <select className={selectCls} value={form.severity}
              onChange={(e) => setField('severity', e.target.value)}>
              {SEVERITIES.map((s) => (
                <option key={s.value} value={s.value}>{s.labelAr}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('common.statusLabel')}</label>
            <select className={selectCls} value={form.status}
              onChange={(e) => setField('status', e.target.value)}>
              {STATUSES.map((s) => (
                <option key={s.value} value={s.value}>{s.labelAr}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Occurred At + Department */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.occurredAt')} *</label>
            <input type="datetime-local" className={inputCls} value={form.occurredAt}
              onChange={(e) => setField('occurredAt', e.target.value)} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.department')}</label>
            <select className={selectCls} value={form.departmentId}
              onChange={(e) => setField('departmentId', e.target.value)}>
              <option value="">{t('common.none')}</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.nameAr}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Risk ID + Task ID (manual input — in a full build these would be dropdowns) */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.riskLink')}</label>
            <input type="number" className={inputCls} value={form.riskId}
              placeholder={t('incidents.fields.riskIdPlaceholder')}
              onChange={(e) => setField('riskId', e.target.value)} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.taskLink')}</label>
            <input type="number" className={inputCls} value={form.taskId}
              placeholder={t('incidents.fields.taskIdPlaceholder')}
              onChange={(e) => setField('taskId', e.target.value)} />
          </div>
        </div>

        {/* Resolution notes */}
        <div>
          <label className="block text-xs text-gray-500 mb-1">{t('incidents.fields.resolutionNotes')}</label>
          <textarea rows={2} className={inputCls} value={form.resolutionNotes}
            onChange={(e) => setField('resolutionNotes', e.target.value)} />
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <button onClick={() => navigate('/incidents')}
          className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
          {t('common.cancel')}
        </button>
        <button onClick={handleSave} disabled={saving || !form.titleAr}
          className="px-5 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50">
          {saving ? '...' : t('common.save')}
        </button>
      </div>
    </div>
  )
}
