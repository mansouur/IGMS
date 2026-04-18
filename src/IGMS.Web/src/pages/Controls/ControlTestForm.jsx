import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { controlTestApi, policyApi, riskApi } from '../../services/governanceApi'
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

const EMPTY_FORM = {
  titleAr: '', titleEn: '', code: '', descriptionAr: '',
  entityType: 'Policy', entityId: '',
  testedById: '', testedAt: '', nextTestDate: '',
  effectiveness: 0, findingsAr: '',
}

export default function ControlTestForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { loading: saving, execute }            = useApi()
  const { loading: fetching, execute: fetchEx } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'CONTROLS.UPDATE' : 'CONTROLS.CREATE'
    if (!hasPermission(needed)) navigate('/controls', { replace: true })
  }, [])

  const [form,     setForm]     = useState(EMPTY_FORM)
  const [users,    setUsers]    = useState([])
  const [entities, setEntities] = useState([])

  // Load lookup data
  useEffect(() => {
    fetchEx(() => userApi.getLookup(), { silent: true }).then((r) => {
      if (r) setUsers(r)
    })
  }, [])

  // Load entities when entityType changes
  useEffect(() => {
    if (!form.entityType) return
    if (form.entityType === 'Policy') {
      fetchEx(() => policyApi.getAll({ pageSize: 200 }), { silent: true }).then((r) => {
        if (r) setEntities(r.items ?? [])
      })
    } else {
      fetchEx(() => riskApi.getAll({ pageSize: 200 }), { silent: true }).then((r) => {
        if (r) setEntities(r.items ?? [])
      })
    }
  }, [form.entityType])

  // Load existing record for edit
  useEffect(() => {
    if (!isEdit) return
    fetchEx(() => controlTestApi.getById(id), { silent: true }).then((r) => {
      if (!r) return
      setForm({
        titleAr:       r.titleAr       ?? '',
        titleEn:       r.titleEn       ?? '',
        code:          r.code          ?? '',
        descriptionAr: r.descriptionAr ?? '',
        entityType:    r.entityType    ?? 'Policy',
        entityId:      r.entityId      ?? '',
        testedById:    r.testedById    ?? '',
        testedAt:      r.testedAt      ? r.testedAt.slice(0, 10) : '',
        nextTestDate:  r.nextTestDate  ? r.nextTestDate.slice(0, 10) : '',
        effectiveness: r.effectiveness ?? 0,
        findingsAr:    r.findingsAr    ?? '',
      })
    })
  }, [id])

  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      ...form,
      entityId:   Number(form.entityId)   || 0,
      testedById: Number(form.testedById) || null,
      testedAt:   form.testedAt    || null,
      nextTestDate: form.nextTestDate || null,
      effectiveness: Number(form.effectiveness),
    }
    const call = isEdit
      ? () => controlTestApi.update(id, payload)
      : () => controlTestApi.create(payload)

    const r = await execute(call, { successMsg: t('controls.messages.saved') })
    if (r) navigate(`/controls/${r.id}`)
  }

  if (fetching && isEdit) return <PageLoader />

  return (
    <div className="max-w-2xl">
      <div className="mb-6">
        <h1 className="text-xl font-bold text-gray-800">
          {isEdit ? t('common.edit') : t('controls.new')} — {t('controls.title')}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">

        {/* Entity type */}
        <Field label={t('controls.form.entityType')} required>
          <div className="flex gap-3">
            {['Policy', 'Risk'].map((type) => (
              <label key={type} className="flex items-center gap-2 text-sm cursor-pointer">
                <input
                  type="radio" name="entityType" value={type}
                  checked={form.entityType === type}
                  onChange={(e) => {
                    setForm((f) => ({ ...f, entityType: e.target.value, entityId: '' }))
                  }}
                  className="text-green-600"
                />
                {type === 'Policy' ? t('controls.form.entityPolicy') : t('controls.form.entityRisk')}
              </label>
            ))}
          </div>
        </Field>

        {/* Entity selector */}
        <Field label={t('controls.form.entityId')} required>
          <select value={form.entityId} onChange={set('entityId')} required className={inputCls}>
            <option value="">{t('controls.form.selectEntity')}</option>
            {entities.map((e) => (
              <option key={e.id} value={e.id}>{e.titleAr} ({e.code})</option>
            ))}
          </select>
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('controls.form.titleAr')} required>
            <input type="text" value={form.titleAr} onChange={set('titleAr')} required className={inputCls} dir="rtl" />
          </Field>
          <Field label={t('controls.form.titleEn')}>
            <input type="text" value={form.titleEn} onChange={set('titleEn')} className={inputCls} dir="ltr" />
          </Field>
        </div>

        <Field label={t('controls.form.code')} required>
          <input type="text" value={form.code} onChange={set('code')} required className={inputCls} placeholder="CTRL-001" />
        </Field>

        <Field label={t('controls.form.descriptionAr')}>
          <textarea value={form.descriptionAr} onChange={set('descriptionAr')} rows={3} className={inputCls} dir="rtl" />
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('controls.form.testedById')}>
            <select value={form.testedById} onChange={set('testedById')} className={inputCls}>
              <option value="">—</option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>{u.fullNameAr}</option>
              ))}
            </select>
          </Field>
          <Field label={t('controls.form.effectiveness')} required>
            <select value={form.effectiveness} onChange={set('effectiveness')} className={inputCls}>
              <option value={0}>{t('controls.effectiveness.0')}</option>
              <option value={1}>{t('controls.effectiveness.1')}</option>
              <option value={2}>{t('controls.effectiveness.2')}</option>
              <option value={3}>{t('controls.effectiveness.3')}</option>
            </select>
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('controls.form.testedAt')}>
            <input type="date" value={form.testedAt} onChange={set('testedAt')} className={inputCls} />
          </Field>
          <Field label={t('controls.form.nextTestDate')}>
            <input type="date" value={form.nextTestDate} onChange={set('nextTestDate')} className={inputCls} />
          </Field>
        </div>

        <Field label={t('controls.form.findingsAr')}>
          <textarea value={form.findingsAr} onChange={set('findingsAr')} rows={4} className={inputCls} dir="rtl" placeholder={t('controls.form.findingsAr') + '...'} />
        </Field>

        <div className="flex justify-end gap-3 pt-2">
          <button type="button" onClick={() => navigate(-1)} className="px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {t('common.save')}
          </button>
        </div>
      </form>
    </div>
  )
}
