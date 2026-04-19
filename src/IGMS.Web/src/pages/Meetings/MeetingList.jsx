import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { meetingsApi } from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'

// ── Helpers ───────────────────────────────────────────────────────────────────

const STATUS_CFG = {
  Scheduled:  { badge: 'bg-blue-100 text-blue-700',    dot: 'bg-blue-500',    label: 'مجدول'       },
  InProgress: { badge: 'bg-amber-100 text-amber-700',  dot: 'bg-amber-400',   label: 'جارٍ'         },
  Completed:  { badge: 'bg-emerald-100 text-emerald-700', dot: 'bg-emerald-500', label: 'مكتمل'    },
  Cancelled:  { badge: 'bg-gray-100 text-gray-500',    dot: 'bg-gray-400',    label: 'ملغى'         },
}
const TYPE_LBL = {
  Board: 'مجلس الإدارة', Committee: 'لجنة', Department: 'قسم',
  Emergency: 'طارئ', Review: 'مراجعة',
}
const STATUS_TABS = [
  { key: '', label: 'الكل' },
  { key: 'Scheduled',  label: 'مجدول'  },
  { key: 'InProgress', label: 'جارٍ'    },
  { key: 'Completed',  label: 'مكتمل'  },
  { key: 'Cancelled',  label: 'ملغى'   },
]

// ── Main ──────────────────────────────────────────────────────────────────────

export default function MeetingList() {
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canCreate     = hasPermission('MEETINGS.CREATE')
  const canDelete     = hasPermission('MEETINGS.DELETE')

  const [data,    setData]    = useState({ items: [], totalCount: 0 })
  const [search,  setSearch]  = useState('')
  const [status,  setStatus]  = useState('')
  const [page,    setPage]    = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 15

  const load = useCallback(() => {
    setLoading(true)
    meetingsApi.getAll({ page, pageSize, search: search || undefined, status: status || undefined })
      .then(r => setData(r.data?.data ?? { items: [], totalCount: 0 }))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [page, pageSize, search, status])

  useEffect(() => { setPage(1) }, [search, status])
  useEffect(() => { load() }, [load])

  async function handleDelete(m) {
    const ok = await confirm({
      title: 'حذف الاجتماع',
      message: `هل تريد حذف "${m.titleAr}"؟`,
      variant: 'danger',
    })
    if (!ok) return
    await meetingsApi.delete(m.id)
    load()
  }

  return (
    <div className="space-y-5 max-w-6xl">

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">اجتماعات اللجان</h1>
          <p className="text-xs text-gray-400 mt-0.5">إدارة اجتماعات الحوكمة وجداول الأعمال والمحاضر</p>
        </div>
        {canCreate && (
          <button onClick={() => navigate('/meetings/new')}
            className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            إضافة اجتماع
          </button>
        )}
      </div>

      {/* Status tabs */}
      <div className="flex gap-2 flex-wrap">
        {STATUS_TABS.map(tab => (
          <button key={tab.key}
            onClick={() => setStatus(tab.key)}
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
          placeholder="بحث باسم الاجتماع..."
          className="w-full border border-gray-200 rounded-xl px-4 py-2.5 pe-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      {/* List */}
      {loading ? <PageLoader /> : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا توجد اجتماعات مسجّلة</p>
          {canCreate && (
            <button onClick={() => navigate('/meetings/new')}
              className="mt-3 text-sm text-blue-600 hover:underline">جدول اجتماعاً</button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {data.items.map(m => {
            const sCfg = STATUS_CFG[m.status] ?? STATUS_CFG.Scheduled
            const isPast = new Date(m.scheduledAt) < Date.now()
            return (
              <div key={m.id}
                className="bg-white rounded-xl border border-gray-200 hover:border-blue-200 hover:shadow-sm transition-all cursor-pointer"
                onClick={() => navigate(`/meetings/${m.id}`)}>
                <div className="p-4 flex items-start gap-4">
                  {/* Date block */}
                  <div className="flex-shrink-0 text-center bg-gray-50 rounded-xl p-3 w-14">
                    <p className="text-xs text-gray-400 font-medium">
                      {new Date(m.scheduledAt).toLocaleDateString('ar-AE', { month: 'short' })}
                    </p>
                    <p className="text-xl font-black text-gray-800">
                      {new Date(m.scheduledAt).getDate()}
                    </p>
                    <p className="text-xs text-gray-400">
                      {new Date(m.scheduledAt).toLocaleTimeString('ar-AE', { hour: '2-digit', minute: '2-digit' })}
                    </p>
                  </div>

                  {/* Content */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className={`w-2 h-2 rounded-full flex-shrink-0 ${sCfg.dot}`} />
                          <h3 className="font-semibold text-gray-800 truncate">{m.titleAr}</h3>
                        </div>
                        <div className="flex items-center gap-3 mt-1 text-xs text-gray-400 flex-wrap">
                          <span>{TYPE_LBL[m.type] ?? m.type}</span>
                          {m.departmentName && <span>· {m.departmentName}</span>}
                          {m.organizerName  && <span>· {m.organizerName}</span>}
                        </div>
                      </div>
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0 ${sCfg.badge}`}>
                        {sCfg.label}
                      </span>
                    </div>

                    {/* Stats */}
                    <div className="flex items-center gap-4 mt-2">
                      <div className="flex items-center gap-1 text-xs text-gray-500">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2 M9 7a4 4 0 1 0 0-8 4 4 0 0 0 0 8"/>
                        </svg>
                        {m.attendeeCount} حاضر
                      </div>
                      {m.actionItemCount > 0 && (
                        <div className={`flex items-center gap-1 text-xs ${m.pendingActions > 0 ? 'text-amber-600' : 'text-emerald-600'}`}>
                          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M9 11l3 3L22 4 M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
                          </svg>
                          {m.pendingActions > 0 ? `${m.pendingActions} نقطة معلّقة` : `${m.actionItemCount} نقاط مكتملة`}
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-1 flex-shrink-0" onClick={e => e.stopPropagation()}>
                    {m.status === 'Scheduled' && (
                      <button onClick={() => navigate(`/meetings/${m.id}/edit`)}
                        className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                        </svg>
                      </button>
                    )}
                    {canDelete && (
                      <button onClick={() => handleDelete(m)}
                        className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/>
                          <path d="M10 11v6M14 11v6M9 6V4h6v2"/>
                        </svg>
                      </button>
                    )}
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <Pagination
        currentPage={page}
        totalCount={data.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
      />
    </div>
  )
}
