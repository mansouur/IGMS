import { useState, useEffect, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { regulatoryApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'

const STATUS_STYLE = {
  Compliant:          'bg-green-100 text-green-700 border-green-300',
  PartiallyCompliant: 'bg-yellow-100 text-yellow-700 border-yellow-300',
  NonCompliant:       'bg-red-100 text-red-700 border-red-300',
  NotAssessed:        'bg-gray-100 text-gray-500 border-gray-200',
}

const STATUS_LABEL = {
  Compliant:          'مطابق',
  PartiallyCompliant: 'مطابق جزئياً',
  NonCompliant:       'غير مطابق',
  NotAssessed:        'لم يُقيَّم',
}

function CoverageBar({ value, total, className = '' }) {
  const pct = total === 0 ? 0 : Math.round(value / total * 100)
  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <div className="flex-1 bg-gray-200 rounded-full h-2 overflow-hidden">
        <div className="h-full bg-green-500 rounded-full transition-all" style={{ width: `${pct}%` }} />
      </div>
      <span className="text-xs text-gray-500 w-8 text-end">{pct}%</span>
    </div>
  )
}

export default function ComplianceLibrary() {
  const { t } = useTranslation()

  const { execute: fwEx }  = useApi()
  const { execute: ctlEx } = useApi()
  const { execute: covEx } = useApi()

  const [frameworks,  setFrameworks]  = useState([])
  const [activeFwId,  setActiveFwId]  = useState(null)
  const [controls,    setControls]    = useState([])
  const [coverage,    setCoverage]    = useState(null)
  const [loading,     setLoading]     = useState(false)
  const [domainFilter, setDomainFilter] = useState('')
  const [search,      setSearch]      = useState('')

  // Load frameworks on mount
  useEffect(() => {
    fwEx(() => regulatoryApi.getFrameworks(), { silent: true }).then((r) => {
      if (r) {
        setFrameworks(r)
        if (r.length > 0) setActiveFwId(r[0].id)
      }
    })
  }, [])

  // Load controls + coverage when active framework changes
  useEffect(() => {
    if (!activeFwId) return
    setLoading(true)
    setDomainFilter('')
    setSearch('')
    Promise.all([
      ctlEx(() => regulatoryApi.getControls(activeFwId), { silent: true }),
      covEx(() => regulatoryApi.getCoverage(activeFwId), { silent: true }),
    ]).then(([ctl, cov]) => {
      if (ctl) setControls(ctl)
      if (cov) setCoverage(cov)
      setLoading(false)
    })
  }, [activeFwId])

  // Domains in current framework
  const domains = useMemo(() => {
    const seen = new Set()
    return controls.filter((c) => {
      if (seen.has(c.domainAr)) return false
      seen.add(c.domainAr)
      return true
    }).map((c) => ({ ar: c.domainAr, en: c.domainEn }))
  }, [controls])

  // Filtered controls
  const visible = useMemo(() => controls.filter((c) => {
    if (domainFilter && c.domainAr !== domainFilter) return false
    if (search) {
      const q = search.toLowerCase()
      return c.controlCode.toLowerCase().includes(q) || c.titleAr.includes(search) || c.titleEn.toLowerCase().includes(q)
    }
    return true
  }), [controls, domainFilter, search])

  const activeFw = frameworks.find((f) => f.id === activeFwId)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-gray-800">{t('compliance.title')}</h1>
        <p className="text-sm text-gray-500 mt-0.5">{t('compliance.subtitle')}</p>
      </div>

      {/* Framework tabs */}
      <div className="flex gap-2 flex-wrap">
        {frameworks.map((fw) => (
          <button
            key={fw.id}
            onClick={() => setActiveFwId(fw.id)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              activeFwId === fw.id
                ? 'bg-green-700 text-white'
                : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'
            }`}
          >
            {fw.nameAr}
            <span className={`ms-2 text-xs px-1.5 py-0.5 rounded-full ${activeFwId === fw.id ? 'bg-green-600' : 'bg-gray-100 text-gray-500'}`}>
              {fw.mappedCount}/{fw.controlCount}
            </span>
          </button>
        ))}
      </div>

      {/* Coverage summary card */}
      {coverage && activeFw && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="font-semibold text-gray-800">{coverage.frameworkName}</h2>
              <p className="text-xs text-gray-400 mt-0.5">{t('compliance.coverage.total', { count: coverage.totalControls })}</p>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-green-700">{coverage.coveragePercent}%</div>
              <div className="text-xs text-gray-400">{t('compliance.coverage.label')}</div>
            </div>
          </div>

          <div className="grid grid-cols-4 gap-3 text-center mb-4">
            {[
              { label: 'مطابق',      count: coverage.compliant,          cls: 'text-green-700' },
              { label: 'جزئياً',     count: coverage.partiallyCompliant, cls: 'text-yellow-600' },
              { label: 'غير مطابق', count: coverage.nonCompliant,       cls: 'text-red-600' },
              { label: 'لم يُقيَّم', count: coverage.notAssessed,        cls: 'text-gray-500' },
            ].map((s) => (
              <div key={s.label} className="bg-gray-50 rounded-lg p-3">
                <div className={`text-xl font-bold ${s.cls}`}>{s.count}</div>
                <div className="text-xs text-gray-400">{s.label}</div>
              </div>
            ))}
          </div>

          {/* Domain breakdown */}
          {coverage.domains.length > 0 && (
            <div className="space-y-2 border-t border-gray-100 pt-3">
              {coverage.domains.map((d) => (
                <div key={d.domainAr} className="flex items-center gap-3">
                  <span className="text-xs text-gray-600 w-40 truncate flex-shrink-0">{d.domainAr}</span>
                  <CoverageBar value={d.compliant + d.partiallyCompliant * 0.5} total={d.totalControls} className="flex-1" />
                  <span className="text-xs text-gray-400 flex-shrink-0">{d.totalControls}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Filters */}
      <div className="flex gap-3 flex-wrap">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('compliance.search')}
          className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 w-64"
        />
        <select
          value={domainFilter}
          onChange={(e) => setDomainFilter(e.target.value)}
          className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
        >
          <option value="">{t('common.all')}</option>
          {domains.map((d) => (
            <option key={d.ar} value={d.ar}>{d.ar}</option>
          ))}
        </select>
      </div>

      {/* Controls list */}
      {loading ? (
        <PageLoader />
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          {visible.length === 0 ? (
            <div className="py-16 text-center text-gray-400 text-sm">{t('common.noData')}</div>
          ) : (
            <div className="divide-y divide-gray-100">
              {visible.map((ctrl) => {
                const bestStatus = ctrl.mappings.length === 0
                  ? 'NotAssessed'
                  : ctrl.mappings.reduce((best, m) => {
                      const rank = { Compliant: 3, PartiallyCompliant: 2, NonCompliant: 1, NotAssessed: 0 }
                      return rank[m.complianceStatus] > rank[best] ? m.complianceStatus : best
                    }, 'NotAssessed')

                return (
                  <div key={ctrl.id} className="p-4 hover:bg-gray-50">
                    <div className="flex items-start gap-3">
                      {/* Code badge */}
                      <span className="font-mono text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded flex-shrink-0 mt-0.5">
                        {ctrl.controlCode}
                      </span>

                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <p className="text-sm font-medium text-gray-800">{ctrl.titleAr}</p>
                          <span className={`text-xs px-2 py-0.5 rounded-full border ${STATUS_STYLE[bestStatus]}`}>
                            {STATUS_LABEL[bestStatus]}
                          </span>
                        </div>
                        <p className="text-xs text-gray-400 mt-0.5">{ctrl.titleEn}</p>

                        {/* Mappings */}
                        {ctrl.mappings.length > 0 && (
                          <div className="mt-2 flex flex-wrap gap-1.5">
                            {ctrl.mappings.map((m) => (
                              <span key={m.id}
                                className={`text-xs px-2 py-0.5 rounded border ${STATUS_STYLE[m.complianceStatus]}`}
                                title={m.notes}>
                                {m.entityType === 'Policy' ? '📄' : m.entityType === 'Risk' ? '⚠️' : '🛡️'} {m.entityTitle || `${m.entityType} #${m.entityId}`}
                              </span>
                            ))}
                          </div>
                        )}
                      </div>

                      <span className="text-xs text-gray-400 flex-shrink-0">{ctrl.domainAr}</span>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </div>
      )}
    </div>
  )
}
