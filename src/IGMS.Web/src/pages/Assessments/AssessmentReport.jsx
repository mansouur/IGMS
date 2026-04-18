import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { assessmentsApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'

const COLORS = ['bg-green-500', 'bg-blue-500', 'bg-yellow-400', 'bg-red-400', 'bg-purple-400']

function DistributionBar({ distribution, total }) {
  if (!distribution || total === 0) return <p className="text-xs text-gray-400">لا توجد بيانات</p>
  const entries = Object.entries(distribution)
  return (
    <div className="space-y-2 mt-2">
      {entries.map(([key, count], i) => {
        const pct = total > 0 ? Math.round((count / total) * 100) : 0
        return (
          <div key={key} className="flex items-center gap-2 text-xs">
            <span className="w-24 text-gray-600 truncate">{key}</span>
            <div className="flex-1 bg-gray-100 rounded-full h-2 overflow-hidden">
              <div className={`h-full rounded-full ${COLORS[i % COLORS.length]}`}
                style={{ width: `${pct}%` }} />
            </div>
            <span className="w-12 text-end text-gray-500">{count} ({pct}%)</span>
          </div>
        )
      })}
    </div>
  )
}

export default function AssessmentReport() {
  const { t } = useTranslation()
  const { id } = useParams()
  const navigate = useNavigate()

  const { loading, execute } = useApi()
  const [report, setReport]  = useState(null)

  useEffect(() => {
    execute(() => assessmentsApi.getReport(id), { silent: true }).then((r) => r && setReport(r))
  }, [id])

  if (loading && !report) return <PageLoader />
  if (!report) return null

  const rateColor = report.responseRate >= 70 ? 'text-green-600' : report.responseRate >= 40 ? 'text-yellow-600' : 'text-red-500'

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{report.titleAr}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('assessments.report.title')}</p>
        </div>
        <button onClick={() => navigate(`/assessments/${id}`)}
          className="text-sm text-gray-500 hover:text-gray-700">
          ← {t('common.back')}
        </button>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-5 text-center">
          <div className="text-3xl font-bold text-gray-800">{report.totalInvited}</div>
          <div className="text-xs text-gray-500 mt-1">{t('assessments.report.totalInvited')}</div>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5 text-center">
          <div className="text-3xl font-bold text-gray-800">{report.totalResponded}</div>
          <div className="text-xs text-gray-500 mt-1">{t('assessments.report.totalResponded')}</div>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5 text-center">
          <div className={`text-3xl font-bold ${rateColor}`}>{report.responseRate}%</div>
          <div className="text-xs text-gray-500 mt-1">{t('assessments.report.responseRate')}</div>
        </div>
      </div>

      {/* Response rate bar */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <div className="flex justify-between text-xs text-gray-500 mb-2">
          <span>{t('assessments.report.responseRate')}</span>
          <span>{report.responseRate}%</span>
        </div>
        <div className="w-full bg-gray-100 rounded-full h-3 overflow-hidden">
          <div className="h-full rounded-full bg-green-600 transition-all"
            style={{ width: `${report.responseRate}%` }} />
        </div>
      </div>

      {/* Per-question results */}
      <div className="space-y-4">
        {report.questions.map((q, i) => (
          <div key={q.questionId} className="bg-white rounded-xl border border-gray-200 p-5">
            <div className="flex items-start gap-3 mb-3">
              <span className="text-xs bg-gray-100 text-gray-500 rounded-full w-6 h-6 flex items-center justify-center flex-shrink-0">
                {i + 1}
              </span>
              <div className="flex-1">
                <p className="text-sm text-gray-800 font-medium">{q.textAr}</p>
                <div className="flex gap-2 mt-1">
                  <span className="text-xs bg-blue-50 text-blue-600 rounded px-2 py-0.5">{q.questionType}</span>
                  <span className="text-xs text-gray-400">{q.answerCount} {t('assessments.report.answers')}</span>
                  {q.average != null && (
                    <span className="text-xs bg-yellow-50 text-yellow-700 rounded px-2 py-0.5">
                      {t('assessments.report.average')}: {q.average}
                    </span>
                  )}
                </div>
              </div>
            </div>

            {q.distribution && (
              <DistributionBar distribution={q.distribution} total={q.answerCount} />
            )}

            {q.textSamples && q.textSamples.length > 0 && (
              <div className="mt-2 space-y-1">
                {q.textSamples.map((sample, si) => (
                  <p key={si} className="text-xs text-gray-600 bg-gray-50 rounded px-3 py-2">
                    "{sample}"
                  </p>
                ))}
              </div>
            )}

            {q.answerCount === 0 && (
              <p className="text-xs text-gray-400 mt-2">{t('common.noData')}</p>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
