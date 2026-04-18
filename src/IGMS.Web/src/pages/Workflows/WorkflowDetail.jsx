import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { workflowApi, rolesApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader, Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'
const selectCls = inputCls

const ENTITY_TYPES = [
  { value: 'Policy',      labelAr: 'السياسات' },
  { value: 'Risk',        labelAr: 'المخاطر' },
  { value: 'ControlTest', labelAr: 'اختبارات الضوابط' },
]

export default function WorkflowDetail() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isNew = id === 'new'
  const navigate = useNavigate()
  const canManage = useAuthStore((s) => s.hasPermission)('WORKFLOWS.MANAGE')

  const { loading: fetching, execute: fetchEx } = useApi()
  const { loading: saving,   execute: saveEx  } = useApi()

  const [roles, setRoles] = useState([])
  const [form, setForm] = useState({
    entityType:    'Policy',
    nameAr:        '',
    nameEn:        '',
    descriptionAr: '',
    isActive:      true,
    stages:        [],
  })

  // Load roles for stage picker + existing definition
  useEffect(() => {
    fetchEx(() => rolesApi.getLookup(), { silent: true }).then((r) => r && setRoles(r))

    if (!isNew) {
      fetchEx(() => workflowApi.getDefinition(id), { silent: true }).then((r) => {
        if (!r) return
        setForm({
          entityType:    r.entityType,
          nameAr:        r.nameAr,
          nameEn:        r.nameEn ?? '',
          descriptionAr: r.descriptionAr ?? '',
          isActive:      r.isActive,
          stages:        r.stages.map((s) => ({
            nameAr:        s.nameAr,
            nameEn:        s.nameEn ?? '',
            requiredRoleId: s.requiredRoleId ?? '',
          })),
        })
      })
    }
  }, [id])

  const setField = (f) => (e) =>
    setForm((prev) => ({ ...prev, [f]: e.target.type === 'checkbox' ? e.target.checked : e.target.value }))

  // Stage helpers
  const addStage = () =>
    setForm((prev) => ({
      ...prev,
      stages: [...prev.stages, { nameAr: '', nameEn: '', requiredRoleId: '' }],
    }))

  const removeStage = (i) =>
    setForm((prev) => ({ ...prev, stages: prev.stages.filter((_, idx) => idx !== i) }))

  const setStage = (i, f) => (e) =>
    setForm((prev) => {
      const stages = [...prev.stages]
      stages[i] = { ...stages[i], [f]: e.target.value }
      return { ...prev, stages }
    })

  const moveStage = (i, dir) => {
    setForm((prev) => {
      const stages = [...prev.stages]
      const j = i + dir
      if (j < 0 || j >= stages.length) return prev
      ;[stages[i], stages[j]] = [stages[j], stages[i]]
      return { ...prev, stages }
    })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      ...form,
      stages: form.stages.map((s) => ({
        nameAr:        s.nameAr,
        nameEn:        s.nameEn,
        requiredRoleId: s.requiredRoleId ? Number(s.requiredRoleId) : null,
      })),
    }

    if (isNew) {
      const r = await saveEx(() => workflowApi.createDefinition(payload), {
        successMsg: t('workflows.messages.saved'),
      })
      if (r) navigate(`/workflows/${r.id}`)
    } else {
      await saveEx(() => workflowApi.updateDefinition(id, payload), {
        successMsg: t('workflows.messages.saved'),
      })
    }
  }

  if (fetching && !isNew) return <PageLoader />

  return (
    <div className="max-w-3xl space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-800">
          {isNew ? t('workflows.new') : form.nameAr || '...'}
        </h1>
        <button onClick={() => navigate('/workflows')} className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('workflows.title')}
        </button>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic info */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
          <h2 className="font-semibold text-gray-700 text-sm">{t('workflows.form.info')}</h2>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('workflows.form.entityType')} <span className="text-red-500">*</span>
            </label>
            <select value={form.entityType} onChange={setField('entityType')}
              disabled={!isNew} className={selectCls}>
              {ENTITY_TYPES.map((et) => (
                <option key={et.value} value={et.value}>{et.labelAr}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('workflows.form.nameAr')} <span className="text-red-500">*</span>
              </label>
              <input value={form.nameAr} onChange={setField('nameAr')} required
                className={inputCls} dir="rtl" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('workflows.form.nameEn')}
              </label>
              <input value={form.nameEn} onChange={setField('nameEn')}
                className={inputCls} dir="ltr" />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('workflows.form.description')}
            </label>
            <textarea value={form.descriptionAr} onChange={setField('descriptionAr')} rows={2}
              className={inputCls} dir="rtl" />
          </div>

          <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
            <input type="checkbox" checked={form.isActive} onChange={setField('isActive')}
              className="accent-green-600" />
            {t('workflows.form.isActive')}
          </label>
        </div>

        {/* Stages */}
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-700 text-sm">{t('workflows.form.stages')}</h2>
            {canManage && (
              <button type="button" onClick={addStage}
                className="text-xs px-3 py-1.5 bg-green-700 text-white rounded-lg hover:bg-green-800">
                + {t('workflows.form.addStage')}
              </button>
            )}
          </div>

          {form.stages.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-8">{t('workflows.form.noStages')}</p>
          ) : (
            <div className="space-y-3">
              {form.stages.map((stage, i) => (
                <div key={i} className="border border-gray-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 mb-3">
                    <span className="w-6 h-6 rounded-full bg-green-100 text-green-700 text-xs font-bold flex items-center justify-center flex-shrink-0">
                      {i + 1}
                    </span>
                    <span className="text-xs font-medium text-gray-500">{t('workflows.form.stage')} {i + 1}</span>
                    <div className="flex gap-1 ms-auto">
                      <button type="button" onClick={() => moveStage(i, -1)} disabled={i === 0}
                        className="p-1 rounded text-gray-400 hover:bg-gray-100 disabled:opacity-30" title="أعلى">
                        ↑
                      </button>
                      <button type="button" onClick={() => moveStage(i, 1)} disabled={i === form.stages.length - 1}
                        className="p-1 rounded text-gray-400 hover:bg-gray-100 disabled:opacity-30" title="أسفل">
                        ↓
                      </button>
                      {canManage && (
                        <button type="button" onClick={() => removeStage(i)}
                          className="p-1 rounded text-red-400 hover:bg-red-50">
                          ✕
                        </button>
                      )}
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">
                        {t('workflows.form.stageNameAr')} *
                      </label>
                      <input value={stage.nameAr} onChange={setStage(i, 'nameAr')} required
                        className={inputCls} dir="rtl" />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1">
                        {t('workflows.form.stageNameEn')}
                      </label>
                      <input value={stage.nameEn} onChange={setStage(i, 'nameEn')}
                        className={inputCls} dir="ltr" />
                    </div>
                  </div>
                  <div className="mt-3">
                    <label className="block text-xs font-medium text-gray-600 mb-1">
                      {t('workflows.form.requiredRole')}
                    </label>
                    <select value={stage.requiredRoleId ?? ''} onChange={setStage(i, 'requiredRoleId')}
                      className={selectCls}>
                      <option value="">{t('workflows.form.anyUser')}</option>
                      {roles.map((r) => (
                        <option key={r.id} value={r.id}>{r.nameAr}</option>
                      ))}
                    </select>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Save */}
        {canManage && (
          <div className="flex justify-end">
            <button type="submit" disabled={saving}
              className="flex items-center gap-2 px-6 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
              {saving && <Spinner size="sm" />}
              {t('common.save')}
            </button>
          </div>
        )}
      </form>
    </div>
  )
}
