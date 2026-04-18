import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { riskApi } from '../../services/governanceApi'
import { PageLoader } from '../../components/ui/Spinner'

// ─── Color logic ──────────────────────────────────────────────────────────────
// score = likelihood × impact  (1–25)

function cellColor(likelihood, impact) {
  const score = likelihood * impact
  if (score >= 15) return { bg: 'bg-red-500',    hover: 'hover:bg-red-400',    text: 'text-white', label: 'critical' }
  if (score >= 8)  return { bg: 'bg-amber-400',  hover: 'hover:bg-amber-300',  text: 'text-white', label: 'high' }
  if (score >= 4)  return { bg: 'bg-yellow-200', hover: 'hover:bg-yellow-100', text: 'text-gray-800', label: 'medium' }
  return                  { bg: 'bg-emerald-200',hover: 'hover:bg-emerald-100',text: 'text-gray-800', label: 'low' }
}

const STATUS_CLS = {
  0: 'bg-red-100 text-red-700',
  1: 'bg-amber-100 text-amber-700',
  2: 'bg-emerald-100 text-emerald-700',
}

// ─── RiskHeatMap ──────────────────────────────────────────────────────────────

export default function RiskHeatMap() {
  const { t }      = useTranslation()
  const navigate   = useNavigate()
  const [risks,    setRisks]    = useState(null)
  const [selected, setSelected] = useState(null) // { likelihood, impact }

  useEffect(() => {
    riskApi.getHeatMap()
      .then((r) => setRisks(r.data?.value ?? []))
      .catch(() => setRisks([]))
  }, [])

  if (risks === null) return <PageLoader />

  // Build lookup: key = `${l}-${i}` → array of risks
  const cellMap = {}
  for (const risk of risks) {
    const key = `${risk.likelihood}-${risk.impact}`
    if (!cellMap[key]) cellMap[key] = []
    cellMap[key].push(risk)
  }

  const selectedKey = selected ? `${selected.likelihood}-${selected.impact}` : null
  const selectedRisks = selectedKey ? (cellMap[selectedKey] ?? []) : []

  const STATUS_LABEL = {
    0: t('risks.status.open'),
    1: t('risks.status.mitigated'),
    2: t('risks.status.closed'),
  }

  return (
    <div className="space-y-6">

      {/* ── Legend ──────────────────────────────────────────────────── */}
      <div className="flex flex-wrap gap-3 text-xs">
        {[
          { cls: 'bg-emerald-200', label: t('risks.heatmap.low'),      range: '1–3' },
          { cls: 'bg-yellow-200',  label: t('risks.heatmap.medium'),   range: '4–7' },
          { cls: 'bg-amber-400',   label: t('risks.heatmap.high'),     range: '8–14' },
          { cls: 'bg-red-500',     label: t('risks.heatmap.critical'), range: '15–25' },
        ].map(({ cls, label, range }) => (
          <span key={label} className="flex items-center gap-1.5">
            <span className={`w-4 h-4 rounded ${cls} inline-block`} />
            <span className="text-gray-600">{label}</span>
            <span className="text-gray-400">({range})</span>
          </span>
        ))}
        <span className="text-gray-400 ms-auto">{t('risks.heatmap.totalRisks', { n: risks.length })}</span>
      </div>

      {/* ── Grid ────────────────────────────────────────────────────── */}
      <div className="overflow-x-auto">
        <div className="inline-block min-w-[400px]">

          {/* X-axis label */}
          <p className="text-center text-xs font-semibold text-gray-500 mb-1">
            {t('risks.heatmap.likelihood')} →
          </p>

          <div className="flex gap-1">
            {/* Y-axis label (rotated) */}
            <div className="flex flex-col items-center justify-center w-7 flex-shrink-0">
              <span
                className="text-xs font-semibold text-gray-500 whitespace-nowrap"
                style={{ writingMode: 'vertical-rl', transform: 'rotate(180deg)' }}
              >
                ← {t('risks.heatmap.impact')}
              </span>
            </div>

            {/* Grid body */}
            <div className="flex flex-col gap-1 flex-1">
              {/* Column headers (likelihood) */}
              <div className="flex gap-1 ms-7">
                {[1, 2, 3, 4, 5].map((l) => (
                  <div key={l} className="flex-1 text-center text-xs font-bold text-gray-500">{l}</div>
                ))}
              </div>

              {/* Rows: impact 5 → 1 (top = highest impact) */}
              {[5, 4, 3, 2, 1].map((impact) => (
                <div key={impact} className="flex gap-1 items-center">
                  {/* Row header (impact) */}
                  <div className="w-7 text-center text-xs font-bold text-gray-500 flex-shrink-0">{impact}</div>

                  {[1, 2, 3, 4, 5].map((likelihood) => {
                    const key   = `${likelihood}-${impact}`
                    const items = cellMap[key] ?? []
                    const color = cellColor(likelihood, impact)
                    const isSelected = selected?.likelihood === likelihood && selected?.impact === impact

                    return (
                      <button
                        key={likelihood}
                        onClick={() => setSelected(
                          isSelected ? null : { likelihood, impact }
                        )}
                        className={[
                          'flex-1 aspect-square flex flex-col items-center justify-center rounded-lg',
                          'transition-all duration-150 border-2',
                          color.bg, color.hover, color.text,
                          isSelected
                            ? 'border-gray-800 shadow-md scale-105'
                            : 'border-transparent',
                        ].join(' ')}
                        title={`L${likelihood} × I${impact} = ${likelihood * impact}`}
                      >
                        {items.length > 0 && (
                          <span className="text-lg font-bold leading-none">{items.length}</span>
                        )}
                        <span className="text-[10px] opacity-70 mt-0.5">
                          {likelihood * impact}
                        </span>
                      </button>
                    )
                  })}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* ── Selected cell risks panel ───────────────────────────────── */}
      {selected && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-700">
              {t('risks.heatmap.cellTitle', {
                l: selected.likelihood,
                i: selected.impact,
                score: selected.likelihood * selected.impact,
              })}
              <span className="ms-2 text-xs font-normal text-gray-400">
                ({selectedRisks.length} {t('risks.heatmap.risksFound')})
              </span>
            </h3>
            <button
              onClick={() => setSelected(null)}
              className="text-gray-400 hover:text-gray-600 p-1 rounded hover:bg-gray-100"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                <path d="M18 6L6 18M6 6l12 12" />
              </svg>
            </button>
          </div>

          {selectedRisks.length === 0 ? (
            <div className="px-5 py-8 text-center text-sm text-gray-400">
              {t('risks.heatmap.noRisksInCell')}
            </div>
          ) : (
            <ul className="divide-y divide-gray-100">
              {selectedRisks.map((risk) => (
                <li key={risk.id}
                  className="flex items-center justify-between px-5 py-3 hover:bg-gray-50 transition-colors">
                  <div>
                    <p className="text-sm font-medium text-gray-800">{risk.titleAr}</p>
                    <div className="flex items-center gap-2 mt-0.5">
                      <span className="text-xs font-mono text-gray-400">{risk.code}</span>
                      {risk.ownerNameAr && (
                        <span className="text-xs text-gray-500">{risk.ownerNameAr}</span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-3 flex-shrink-0">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-semibold ${STATUS_CLS[risk.status]}`}>
                      {STATUS_LABEL[risk.status]}
                    </span>
                    <span className={`w-8 h-8 flex items-center justify-center rounded-full text-xs font-bold
                      ${risk.riskScore >= 15 ? 'bg-red-500 text-white'
                        : risk.riskScore >= 8 ? 'bg-amber-400 text-white'
                        : 'bg-emerald-500 text-white'}`}>
                      {risk.riskScore}
                    </span>
                    <button
                      onClick={() => navigate(`/risks/${risk.id}/edit`)}
                      className="p-1.5 rounded text-blue-500 hover:bg-blue-50 transition-colors"
                      title={t('common.edit')}
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                        stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                        <path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" />
                        <path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z" />
                      </svg>
                    </button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}
