import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'

// ── Helpers ───────────────────────────────────────────────────────────────────

const STATUS_CFG = {
  Draft:     { badge: 'bg-gray-100 text-gray-600',    dot: 'bg-gray-400',    label: 'مسودة'    },
  Submitted: { badge: 'bg-blue-100 text-blue-700',    dot: 'bg-blue-500',    label: 'مرفوع'    },
  Approved:  { badge: 'bg-emerald-100 text-emerald-700', dot: 'bg-emerald-500', label: 'معتمد' },
  Rejected:  { badge: 'bg-red-100 text-red-600',      dot: 'bg-red-500',     label: 'مرفوض'   },
}

const PERIOD_LBL = {
  Q1: 'الربع الأول', Q2: 'الربع الثاني', Q3: 'الربع الثالث', Q4: 'الربع الرابع',
  Annual: 'سنوي', Probation: 'فترة تجربة',
}

const STATUS_TABS = [
  { key: '', label: 'الكل' },
  { key: 'Draft',     label: 'مسودة'  },
  { key: 'Submitted', label: 'مرفوع'  },
  { key: 'Approved',  label: 'معتمد'  },
  { key: 'Rejected',  label: 'مرفوض' },
]

function StarRating({ value }) {
  if (!value) return <span className="text-xs text-gray-400">—</span>
  return (
    <span className="flex items-center gap-0.5">
      {[1,2,3,4,5].map(i => (
        <svg key={i} width="12" height="12" viewBox="0 0 24 24"
          fill={i <= Math.round(value) ? '#f59e0b' : 'none'}
          stroke={i <= Math.round(value) ? '#f59e0b' : '#d1d5db'} strokeWidth="1.5">
          <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
        </svg>
      ))}
      <span className="text-xs text-gray-500 ms-1">{Number(value).toFixed(1)}</span>
    </span>
  )
}

// ── Main ──────────────────────────────────────────────────────────────────────

export default function PerformanceList() {
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canCreate     = hasPermission('PERFORMANCE.CREATE')
  const canDelete     = hasPermission('PERFORMANCE.DELETE')

  const [data,    setData]    = useState({ items: [], totalCount: 0 })
  const [search,  setSearch]  = useState('')
  const [status,  setStatus]  = useState('')
  const [page,    setPage]    = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 15

  const load = useCallback(() => {
    setLoading(true)
    api.get('/api/v1/performance', { params: { page, pageSize, search: search || undefined, status: status || undefined } })
      .then(r => setData(r.data?.data ?? { items: [], totalCount: 0 }))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [page, pageSize, search, status])

  useEffect(() => { setPage(1) }, [search, status])
  useEffect(() => { load() }, [load])

  async function handleDelete(r) {
    const ok = await confirm({
      title: 'حذف التقييم',
      message: `هل تريد حذف تقييم "${r.employeeName}"؟`,
      variant: 'danger',
    })
    if (!ok) return
    await api.delete(`/api/v1/performance/${r.id}`)
    load()
  }

  return (
    <div className="space-y-5 max-w-6xl">

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">تقييمات الأداء</h1>
          <p className="text-xs text-gray-400 mt-0.5">إدارة تقييمات أداء الموظفين والأهداف السنوية</p>
        </div>
        {canCreate && (
          <button onClick={() => navigate('/performance/new')}
            className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
              <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
            </svg>
            تقييم جديد
          </button>
        )}
      </div>

      {/* Status tabs */}
      <div className="flex gap-2 flex-wrap">
        {STATUS_TABS.map(tab => (
          <button key={tab.key} onClick={() => setStatus(tab.key)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
              status === tab.key
                ? 'bg-blue-600 text-white border-blue-600'
                : 'bg-white text-gray-600 border-gray-200 hover:border-blue-300'
            }`}>
            {tab.label}
          </button>
        ))}
      </div>

      {/* Search */}
      <div className="relative">
        <svg className="absolute end-3 top-1/2 -translate-y-1/2 text-gray-400" width="16" height="16"
          viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input value={search} onChange={e => setSearch(e.target.value)}
          placeholder="بحث باسم الموظف..."
          className="w-full border border-gray-200 rounded-xl px-4 py-2.5 pe-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      {/* List */}
      {loading ? <PageLoader /> : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا توجد تقييمات مسجّلة</p>
          {canCreate && (
            <button onClick={() => navigate('/performance/new')}
              className="mt-3 text-sm text-blue-600 hover:underline">أنشئ تقييماً</button>
          )}
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-100">
              <tr>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الموظف</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">المقيِّم</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الفترة</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الأهداف</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">التقييم</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الحالة</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.items.map(r => {
                const sCfg = STATUS_CFG[r.status] ?? STATUS_CFG.Draft
                return (
                  <tr key={r.id} onClick={() => navigate(`/performance/${r.id}`)}
                    className="hover:bg-gray-50 cursor-pointer transition-colors">
                    <td className="px-4 py-3">
                      <p className="font-medium text-gray-800">{r.employeeName}</p>
                      {r.departmentName && <p className="text-xs text-gray-400">{r.departmentName}</p>}
                    </td>
                    <td className="px-4 py-3 text-gray-600">{r.reviewerName}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-col">
                        <span className="text-gray-700">{PERIOD_LBL[r.period] ?? r.period}</span>
                        <span className="text-xs text-gray-400">{r.year}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                        {r.goalCount} هدف
                      </span>
                    </td>
                    <td className="px-4 py-3"><StarRating value={r.overallRating} /></td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${sCfg.badge}`}>
                        {sCfg.label}
                      </span>
                    </td>
                    <td className="px-4 py-3" onClick={e => e.stopPropagation()}>
                      <div className="flex items-center gap-1">
                        {r.status === 'Draft' && (
                          <button onClick={() => navigate(`/performance/${r.id}/edit`)}
                            className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                            </svg>
                          </button>
                        )}
                        {canDelete && (
                          <button onClick={() => handleDelete(r)}
                            className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                              <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/>
                              <path d="M10 11v6M14 11v6M9 6V4h6v2"/>
                            </svg>
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      <Pagination currentPage={page} totalCount={data.totalCount} pageSize={pageSize} onPageChange={setPage} />
    </div>
  )
}
