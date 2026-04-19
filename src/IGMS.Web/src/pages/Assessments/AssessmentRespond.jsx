import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { assessmentsApi } from '../../services/api'
import { useApi, useConfirm } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'

export default function AssessmentRespond() {
  const { t } = useTranslation()
  const { id } = useParams()
  const navigate = useNavigate()

  const { loading: fetching, execute: fetchEx } = useApi()
  const { loading: saving,   execute: saveEx  } = useApi()
  const confirm = useConfirm()

  const [assessment, setAssessment] = useState(null)
  const [answers,    setAnswers]    = useState({}) // { [questionId]: string }
  const [submitted,  setSubmitted]  = useState(false)

  useEffect(() => {
    fetchEx(() => assessmentsApi.getById(id), { silent: true }).then((detail) => {
      if (!detail) return
      setAssessment(detail)
      // Load existing draft response
      fetchEx(() => assessmentsApi.getMyResponse(id), { silent: true }).then((r) => {
        if (!r) return
        if (r.isSubmitted) { setSubmitted(true); return }
        const map = {}
        r.answers.forEach((a) => { map[a.questionId] = a.answerText })
        setAnswers(map)
      })
    })
  }, [id])

  const setAnswer = (qid, val) => setAnswers((prev) => ({ ...prev, [qid]: val }))

  const buildPayload = () => ({
    answers: Object.entries(answers)
      .filter(([, v]) => v?.trim())
      .map(([questionId, answerText]) => ({ questionId: Number(questionId), answerText })),
  })

  const handleSave = async () => {
    await saveEx(() => assessmentsApi.respond(id, buildPayload(), false, null),
      { successMsg: t('assessments.messages.saved') })
  }

  const handleSubmit = async () => {
    const ok = await confirm({ title: t('assessments.confirmSubmitTitle'), message: t('assessments.confirmSubmit'), variant: 'warning' })
    if (!ok) return
    const result = await saveEx(() => assessmentsApi.respond(id, buildPayload(), true, null),
      { successMsg: t('assessments.messages.submitted') })
    if (result !== null) {
      setSubmitted(true)
      setTimeout(() => navigate('/assessments'), 1500)
    }
  }

  if (fetching && !assessment) return <PageLoader />
  if (!assessment) return null

  if (submitted) {
    return (
      <div className="flex flex-col items-center justify-center py-24 space-y-3">
        <div className="text-4xl text-green-600">✓</div>
        <p className="text-gray-700 font-medium">{t('assessments.alreadySubmitted')}</p>
        <button onClick={() => navigate('/assessments')}
          className="text-sm text-green-700 hover:underline">{t('common.back')}</button>
      </div>
    )
  }

  const requiredIds = assessment.questions.filter((q) => q.isRequired).map((q) => q.id)
  const canSubmit   = requiredIds.every((qid) => answers[qid]?.trim())

  return (
    <div className="space-y-6 max-w-2xl">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-gray-800">{assessment.titleAr}</h1>
        {assessment.descriptionAr && (
          <p className="text-sm text-gray-500 mt-1">{assessment.descriptionAr}</p>
        )}
        {assessment.dueDate && (
          <p className="text-xs text-gray-400 mt-1">
            {t('assessments.fields.dueDate')}: {new Date(assessment.dueDate).toLocaleDateString('ar-AE')}
          </p>
        )}
      </div>

      {/* Questions */}
      <div className="space-y-4">
        {assessment.questions.map((q, i) => (
          <div key={q.id} className="bg-white rounded-xl border border-gray-200 p-5">
            <div className="flex items-start gap-3 mb-3">
              <span className="text-xs bg-gray-100 text-gray-500 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0 mt-0.5">
                {i + 1}
              </span>
              <div>
                <span className="text-sm text-gray-800">{q.textAr}</span>
                {q.isRequired && <span className="text-red-500 ms-1">*</span>}
                {q.textEn && <div className="text-xs text-gray-400 mt-0.5" dir="ltr">{q.textEn}</div>}
              </div>
            </div>

            {/* Input by type */}
            <div className="ps-9">
              {q.questionType === 'YesNo' && (
                <div className="flex gap-4">
                  {['نعم', 'لا'].map((opt) => (
                    <label key={opt} className="flex items-center gap-2 cursor-pointer">
                      <input type="radio" name={`q-${q.id}`} value={opt}
                        checked={answers[q.id] === opt}
                        onChange={() => setAnswer(q.id, opt)}
                        className="accent-green-700" />
                      <span className="text-sm text-gray-700">{opt}</span>
                    </label>
                  ))}
                </div>
              )}

              {q.questionType === 'Rating' && (
                <div className="flex gap-3">
                  {[1, 2, 3, 4, 5].map((n) => (
                    <button key={n} onClick={() => setAnswer(q.id, String(n))}
                      className={`w-10 h-10 rounded-full text-sm font-medium border transition-colors
                        ${answers[q.id] === String(n)
                          ? 'bg-green-700 text-white border-green-700'
                          : 'bg-white text-gray-600 border-gray-300 hover:border-green-500'}`}>
                      {n}
                    </button>
                  ))}
                </div>
              )}

              {q.questionType === 'MultiChoice' && (
                <div className="space-y-2">
                  {(q.options ?? []).map((opt) => (
                    <label key={opt} className="flex items-center gap-2 cursor-pointer">
                      <input type="radio" name={`q-${q.id}`} value={opt}
                        checked={answers[q.id] === opt}
                        onChange={() => setAnswer(q.id, opt)}
                        className="accent-green-700" />
                      <span className="text-sm text-gray-700">{opt}</span>
                    </label>
                  ))}
                </div>
              )}

              {q.questionType === 'Text' && (
                <textarea rows={3}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white"
                  value={answers[q.id] ?? ''}
                  onChange={(e) => setAnswer(q.id, e.target.value)} />
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Actions */}
      <div className="flex justify-between items-center">
        <button onClick={() => navigate('/assessments')}
          className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('common.back')}
        </button>
        <div className="flex gap-3">
          <button onClick={handleSave} disabled={saving}
            className="px-4 py-2 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50 disabled:opacity-50">
            {t('assessments.saveDraft')}
          </button>
          <button onClick={handleSubmit} disabled={saving || !canSubmit}
            className="px-5 py-2 text-sm bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50">
            {t('assessments.submitResponse')}
          </button>
        </div>
      </div>
    </div>
  )
}
