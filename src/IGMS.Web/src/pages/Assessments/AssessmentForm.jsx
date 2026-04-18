import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { assessmentsApi } from '../../services/api'
import { departmentApi } from '../../services/departmentApi'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'

const inputCls  = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'
const selectCls = inputCls

const QUESTION_TYPES = [
  { value: 'YesNo',       labelAr: 'نعم / لا' },
  { value: 'Rating',      labelAr: 'تقييم (1-5)' },
  { value: 'Text',        labelAr: 'إجابة نصية' },
  { value: 'MultiChoice', labelAr: 'اختيار متعدد' },
]

const emptyQuestion = () => ({
  textAr:       '',
  textEn:       '',
  questionType: 'YesNo',
  isRequired:   true,
  options:      [],
})

export default function AssessmentForm() {
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
    departmentId:  '',
    dueDate:       '',
    questions:     [emptyQuestion()],
  })

  useEffect(() => {
    departmentApi.getAll().then((r) => setDepartments(r?.data?.data ?? []))

    if (!isNew) {
      fetchEx(() => assessmentsApi.getById(id), { silent: true }).then((r) => {
        if (!r) return
        setForm({
          titleAr:       r.titleAr,
          titleEn:       r.titleEn ?? '',
          descriptionAr: r.descriptionAr ?? '',
          departmentId:  r.departmentId ?? '',
          dueDate:       r.dueDate ? r.dueDate.slice(0, 10) : '',
          questions:     r.questions.length > 0
            ? r.questions.map((q) => ({
                textAr:       q.textAr,
                textEn:       q.textEn ?? '',
                questionType: q.questionType,
                isRequired:   q.isRequired,
                options:      q.options ?? [],
              }))
            : [emptyQuestion()],
        })
      })
    }
  }, [id])

  // ── Form helpers ──────────────────────────────────────────────────────────

  const setField = (k, v) => setForm((f) => ({ ...f, [k]: v }))

  const setQ = (i, k, v) =>
    setForm((f) => {
      const qs = [...f.questions]
      qs[i] = { ...qs[i], [k]: v }
      return { ...f, questions: qs }
    })

  const addQuestion = () =>
    setForm((f) => ({ ...f, questions: [...f.questions, emptyQuestion()] }))

  const removeQuestion = (i) =>
    setForm((f) => ({ ...f, questions: f.questions.filter((_, idx) => idx !== i) }))

  const moveQuestion = (i, dir) =>
    setForm((f) => {
      const qs = [...f.questions]
      const j  = i + dir
      if (j < 0 || j >= qs.length) return f
      ;[qs[i], qs[j]] = [qs[j], qs[i]]
      return { ...f, questions: qs }
    })

  const addOption = (qi) =>
    setForm((f) => {
      const qs = [...f.questions]
      qs[qi] = { ...qs[qi], options: [...qs[qi].options, ''] }
      return { ...f, questions: qs }
    })

  const setOption = (qi, oi, v) =>
    setForm((f) => {
      const qs = [...f.questions]
      const opts = [...qs[qi].options]
      opts[oi] = v
      qs[qi] = { ...qs[qi], options: opts }
      return { ...f, questions: qs }
    })

  const removeOption = (qi, oi) =>
    setForm((f) => {
      const qs = [...f.questions]
      qs[qi] = { ...qs[qi], options: qs[qi].options.filter((_, idx) => idx !== oi) }
      return { ...f, questions: qs }
    })

  // ── Submit ────────────────────────────────────────────────────────────────

  const handleSave = async () => {
    const payload = {
      titleAr:       form.titleAr,
      titleEn:       form.titleEn || null,
      descriptionAr: form.descriptionAr || null,
      departmentId:  form.departmentId ? Number(form.departmentId) : null,
      dueDate:       form.dueDate || null,
      questions:     form.questions.map((q) => ({
        textAr:       q.textAr,
        textEn:       q.textEn || null,
        questionType: q.questionType,
        isRequired:   q.isRequired,
        options:      q.questionType === 'MultiChoice' ? q.options.filter(Boolean) : [],
      })),
    }

    const result = isNew
      ? await saveEx(() => assessmentsApi.create(payload), { successMsg: t('assessments.messages.created') })
      : await saveEx(() => assessmentsApi.update(id, payload), { successMsg: t('assessments.messages.updated') })

    if (result) navigate('/assessments')
  }

  if (fetching && !isNew) return <PageLoader />

  return (
    <div className="space-y-6 max-w-3xl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">
            {isNew ? t('assessments.newTitle') : t('assessments.editTitle')}
          </h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('assessments.formSubtitle')}</p>
        </div>
        <button onClick={() => navigate('/assessments')}
          className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('common.back')}
        </button>
      </div>

      {/* Basic Info */}
      <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
        <h2 className="font-semibold text-gray-700 text-sm">{t('assessments.basicInfo')}</h2>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('assessments.fields.titleAr')} *</label>
            <input className={inputCls} value={form.titleAr}
              onChange={(e) => setField('titleAr', e.target.value)} />
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('assessments.fields.titleEn')}</label>
            <input className={inputCls} value={form.titleEn} dir="ltr"
              onChange={(e) => setField('titleEn', e.target.value)} />
          </div>
        </div>

        <div>
          <label className="block text-xs text-gray-500 mb-1">{t('assessments.fields.description')}</label>
          <textarea rows={2} className={inputCls} value={form.descriptionAr}
            onChange={(e) => setField('descriptionAr', e.target.value)} />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('assessments.fields.department')}</label>
            <select className={selectCls} value={form.departmentId}
              onChange={(e) => setField('departmentId', e.target.value)}>
              <option value="">{t('common.none')}</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.nameAr}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs text-gray-500 mb-1">{t('assessments.fields.dueDate')}</label>
            <input type="date" className={inputCls} value={form.dueDate}
              onChange={(e) => setField('dueDate', e.target.value)} />
          </div>
        </div>
      </div>

      {/* Questions */}
      <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="font-semibold text-gray-700 text-sm">{t('assessments.questions')} ({form.questions.length})</h2>
          <button onClick={addQuestion}
            className="text-xs px-3 py-1.5 bg-green-700 text-white rounded-lg hover:bg-green-800">
            + {t('assessments.addQuestion')}
          </button>
        </div>

        {form.questions.map((q, qi) => (
          <div key={qi} className="border border-gray-200 rounded-lg p-4 space-y-3">
            {/* Question header */}
            <div className="flex items-center justify-between">
              <span className="text-xs font-medium text-gray-500">{t('assessments.question')} {qi + 1}</span>
              <div className="flex items-center gap-1">
                <button onClick={() => moveQuestion(qi, -1)} disabled={qi === 0}
                  className="text-gray-400 hover:text-gray-600 disabled:opacity-30 px-1">↑</button>
                <button onClick={() => moveQuestion(qi, 1)} disabled={qi === form.questions.length - 1}
                  className="text-gray-400 hover:text-gray-600 disabled:opacity-30 px-1">↓</button>
                <button onClick={() => removeQuestion(qi)} disabled={form.questions.length === 1}
                  className="text-red-400 hover:text-red-600 disabled:opacity-30 text-xs px-2">✕</button>
              </div>
            </div>

            {/* Type + Required */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-gray-500 mb-1">{t('assessments.questionType')}</label>
                <select className={selectCls} value={q.questionType}
                  onChange={(e) => setQ(qi, 'questionType', e.target.value)}>
                  {QUESTION_TYPES.map((qt) => (
                    <option key={qt.value} value={qt.value}>{qt.labelAr}</option>
                  ))}
                </select>
              </div>
              <div className="flex items-end pb-1">
                <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
                  <input type="checkbox" checked={q.isRequired}
                    onChange={(e) => setQ(qi, 'isRequired', e.target.checked)}
                    className="w-4 h-4 accent-green-700" />
                  {t('assessments.required')}
                </label>
              </div>
            </div>

            {/* Text */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-gray-500 mb-1">{t('assessments.questionTextAr')} *</label>
                <input className={inputCls} value={q.textAr}
                  onChange={(e) => setQ(qi, 'textAr', e.target.value)} />
              </div>
              <div>
                <label className="block text-xs text-gray-500 mb-1">{t('assessments.questionTextEn')}</label>
                <input className={inputCls} value={q.textEn} dir="ltr"
                  onChange={(e) => setQ(qi, 'textEn', e.target.value)} />
              </div>
            </div>

            {/* MultiChoice options */}
            {q.questionType === 'MultiChoice' && (
              <div className="space-y-2">
                <label className="block text-xs text-gray-500">{t('assessments.options')}</label>
                {q.options.map((opt, oi) => (
                  <div key={oi} className="flex gap-2">
                    <input className={`${inputCls} flex-1`} value={opt}
                      placeholder={`${t('assessments.option')} ${oi + 1}`}
                      onChange={(e) => setOption(qi, oi, e.target.value)} />
                    <button onClick={() => removeOption(qi, oi)}
                      className="text-red-400 hover:text-red-600 text-sm px-2">✕</button>
                  </div>
                ))}
                <button onClick={() => addOption(qi)}
                  className="text-xs text-green-700 hover:underline">
                  + {t('assessments.addOption')}
                </button>
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <button onClick={() => navigate('/assessments')}
          className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50">
          {t('common.cancel')}
        </button>
        <button onClick={handleSave} disabled={saving || !form.titleAr || form.questions.some((q) => !q.textAr)}
          className="px-5 py-2 text-sm bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50">
          {saving ? '...' : t('common.save')}
        </button>
      </div>
    </div>
  )
}
