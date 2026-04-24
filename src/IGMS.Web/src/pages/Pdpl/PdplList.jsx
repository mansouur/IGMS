import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'

// ── Helpers ───────────────────────────────────────────────────────────────────

const STATUS_CFG = {
  Active:      { badge: 'bg-emerald-100 text-emerald-700', label: 'نشط'          },
  UnderReview: { badge: 'bg-amber-100 text-amber-700',     label: 'قيد المراجعة'  },
  Retired:     { badge: 'bg-gray-100 text-gray-500',       label: 'متقاعد'       },
}

const CATEGORY_CFG = {
  Basic:     { cls: 'bg-blue-50 text-blue-700',    label: 'أساسية'   },
  Sensitive: { cls: 'bg-orange-50 text-orange-700',label: 'حساسة'    },
  Special:   { cls: 'bg-red-50 text-red-700',      label: 'خاصة'     },
}

const LEGAL_LBL = {
  Consent: 'موافقة', ContractPerformance: 'تنفيذ عقد', LegalObligation: 'التزام قانوني',
  VitalInterests: 'مصلحة حيوية', PublicTask: 'مهمة عامة', LegitimateInterests: 'مصلحة مشروعة',
}

const CATEGORY_TABS = [
  { key: '', label: 'الكل' },
  { key: 'Basic',     label: 'أساسية'  },
  { key: 'Sensitive', label: 'حساسة'   },
  { key: 'Special',   label: 'خاصة'    },
]

// ── Main ──────────────────────────────────────────────────────────────────────

export default function PdplList() {
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canCreate     = hasPermission('PDPL.CREATE')
  const canDelete     = hasPermission('PDPL.DELETE')

  const [data,     setData]    = useState({ items: [], totalCount: 0 })
  const [search,   setSearch]  = useState('')
  const [category, setCategory]= useState('')
  const [page,     setPage]    = useState(1)
  const [loading,  setLoading] = useState(true)
  const pageSize = 15

  const load = useCallback(() => {
    setLoading(true)
    api.get('/api/v1/pdpl', { params: { page, pageSize, search: search || undefined, dataCategory: category || undefined } })
      .then(r => setData(r.data?.data ?? { items: [], totalCount: 0 }))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [page, pageSize, search, category])

  useEffect(() => { setPage(1) }, [search, category])
  useEffect(() => { load() }, [load])

  async function handleDelete(r) {
    const ok = await confirm({ title: 'حذف السجل', message: `هل تريد حذف "${r.titleAr}"؟`, variant: 'danger' })
    if (!ok) return
    await api.delete(`/api/v1/pdpl/${r.id}`)
    load()
  }

  return (
    <div className="space-y-5 max-w-6xl">

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <div className="flex items-center gap-2">
            <span className="text-xs bg-red-100 text-red-700 px-2 py-0.5 rounded-full font-medium">UAE PDPL</span>
            <h1 className="text-xl font-bold text-gray-800">سجل معالجة البيانات الشخصية</h1>
          </div>
          <p className="text-xs text-gray-400 mt-0.5">توثيق أنشطة معالجة البيانات والموافقات وطلبات أصحاب البيانات</p>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => navigate('/pdpl/requests')}
            className="px-4 py-2 bg-white border border-gray-200 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-50">
            طلبات البيانات
          </button>
          {canCreate && (
            <button onClick={() => navigate('/pdpl/new')}
              className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
              </svg>
              سجل جديد
            </button>
          )}
        </div>
      </div>

      {/* Category tabs */}
      <div className="flex gap-2 flex-wrap">
        {CATEGORY_TABS.map(tab => (
          <button key={tab.key} onClick={() => setCategory(tab.key)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
              category === tab.key
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
          placeholder="بحث في سجلات البيانات..."
          className="w-full border border-gray-200 rounded-xl px-4 py-2.5 pe-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      {/* List */}
      {loading ? <PageLoader /> : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا توجد سجلات معالجة بيانات</p>
          {canCreate && (
            <button onClick={() => navigate('/pdpl/new')} className="mt-3 text-sm text-blue-600 hover:underline">
              أضف سجلاً
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {data.items.map(r => {
            const sCfg   = STATUS_CFG[r.status] ?? STATUS_CFG.Active
            const catCfg = CATEGORY_CFG[r.dataCategory] ?? CATEGORY_CFG.Basic
            const needsReview = r.lastReviewedAt
              ? (new Date() - new Date(r.lastReviewedAt)) / (1000 * 60 * 60 * 24) > 365
              : true
            return (
              <div key={r.id} onClick={() => navigate(`/pdpl/${r.id}`)}
                className="bg-white rounded-xl border border-gray-200 hover:border-blue-200 hover:shadow-sm transition-all cursor-pointer p-4">
                <div className="flex items-start gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <h3 className="font-semibold text-gray-800 truncate">{r.titleAr}</h3>
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${catCfg.cls}`}>{catCfg.label}</span>
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${sCfg.badge}`}>{sCfg.label}</span>
                      {needsReview && (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-amber-100 text-amber-700 font-medium">تحتاج مراجعة</span>
                      )}
                    </div>
                    <div className="flex items-center gap-3 mt-1 text-xs text-gray-400 flex-wrap">
                      <span>{LEGAL_LBL[r.legalBasis] ?? r.legalBasis}</span>
                      {r.departmentName && <span>· {r.departmentName}</span>}
                      {r.ownerName && <span>· {r.ownerName}</span>}
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="flex items-center gap-4 flex-shrink-0">
                    {r.isThirdPartySharing && (
                      <div className="flex items-center gap-1 text-xs text-gray-500">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8 M16 6l-4-4-4 4 M12 2v13"/>
                        </svg>
                        مشاركة خارجية
                      </div>
                    )}
                    {r.isCrossBorderTransfer && (
                      <div className="flex items-center gap-1 text-xs text-orange-600">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/>
                          <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
                        </svg>
                        نقل عابر حدود
                      </div>
                    )}
                    {r.pendingRequestCount > 0 && (
                      <div className="flex items-center gap-1 text-xs text-red-600 font-medium">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
                          <line x1="12" y1="16" x2="12.01" y2="16"/>
                        </svg>
                        {r.pendingRequestCount} طلب معلق
                      </div>
                    )}
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-1 flex-shrink-0" onClick={e => e.stopPropagation()}>
                    <button onClick={() => navigate(`/pdpl/${r.id}/edit`)}
                      className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                        <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                        <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                      </svg>
                    </button>
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
                </div>
              </div>
            )
          })}
        </div>
      )}

      <Pagination currentPage={page} totalCount={data.totalCount} pageSize={pageSize} onPageChange={setPage} />
    </div>
  )
}
