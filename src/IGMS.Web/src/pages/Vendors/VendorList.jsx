import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { vendorsApi } from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'

// ── Helpers ───────────────────────────────────────────────────────────────────

const RISK_CLS = {
  Low:      { badge: 'bg-emerald-100 text-emerald-700', dot: 'bg-emerald-500' },
  Medium:   { badge: 'bg-amber-100 text-amber-700',    dot: 'bg-amber-400'   },
  High:     { badge: 'bg-orange-100 text-orange-700',  dot: 'bg-orange-500'  },
  Critical: { badge: 'bg-red-100 text-red-700',        dot: 'bg-red-500'     },
}
const STATUS_CLS = {
  Active:      'bg-emerald-100 text-emerald-700',
  Inactive:    'bg-gray-100 text-gray-600',
  UnderReview: 'bg-amber-100 text-amber-700',
  Blacklisted: 'bg-red-100 text-red-700',
}
const STATUS_LBL = {
  Active: 'نشط', Inactive: 'غير نشط', UnderReview: 'قيد المراجعة', Blacklisted: 'محظور',
}
const TYPE_LBL = {
  Supplier: 'مورّد', Partner: 'شريك', Consultant: 'استشاري',
  Contractor: 'مقاول', CloudProvider: 'خدمات سحابية',
}
const RISK_LBL = {
  Low: 'منخفض', Medium: 'متوسط', High: 'عالٍ', Critical: 'حرج',
}

const RISK_TABS = [
  { key: '', label: 'الكل' },
  { key: 'Critical', label: 'حرج' },
  { key: 'High',     label: 'عالٍ' },
  { key: 'Medium',   label: 'متوسط' },
  { key: 'Low',      label: 'منخفض' },
]

// ── Main ──────────────────────────────────────────────────────────────────────

export default function VendorList() {
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canCreate     = hasPermission('VENDORS.CREATE')
  const canDelete     = hasPermission('VENDORS.DELETE')

  const [data,    setData]    = useState({ items: [], totalCount: 0 })
  const [search,  setSearch]  = useState('')
  const [risk,    setRisk]    = useState('')
  const [page,    setPage]    = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 15

  const load = useCallback(() => {
    setLoading(true)
    vendorsApi.getAll({ page, pageSize, search: search || undefined, riskLevel: risk || undefined })
      .then(r => setData(r.data?.data ?? { items: [], totalCount: 0 }))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [page, pageSize, search, risk])

  useEffect(() => { setPage(1) }, [search, risk])
  useEffect(() => { load() }, [load])

  async function handleDelete(v) {
    const ok = await confirm({
      title: `حذف المورد`,
      message: `هل تريد حذف "${v.nameAr}"؟ لا يمكن التراجع.`,
      variant: 'danger',
    })
    if (!ok) return
    await vendorsApi.delete(v.id)
    load()
  }

  const contractDaysLeft = (end) => {
    if (!end) return null
    const d = Math.ceil((new Date(end) - Date.now()) / 86400000)
    return d
  }

  return (
    <div className="space-y-5 max-w-6xl">

      {/* Header */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-800">إدارة الموردين</h1>
          <p className="text-xs text-gray-400 mt-0.5">متابعة مخاطر الأطراف الثالثة وعقودها</p>
        </div>
        {canCreate && (
          <button onClick={() => navigate('/vendors/new')}
            className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            إضافة مورد
          </button>
        )}
      </div>

      {/* Risk filter tabs */}
      <div className="flex gap-2 flex-wrap">
        {RISK_TABS.map(tab => (
          <button key={tab.key}
            onClick={() => setRisk(tab.key)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
              risk === tab.key
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
          placeholder="بحث باسم المورد أو التصنيف..."
          className="w-full border border-gray-200 rounded-xl px-4 py-2.5 pe-10 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      {/* Table */}
      {loading ? <PageLoader /> : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا يوجد موردون مسجّلون</p>
          {canCreate && (
            <button onClick={() => navigate('/vendors/new')}
              className="mt-3 text-sm text-blue-600 hover:underline">إضافة أول مورد</button>
          )}
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                {['اسم المورد', 'النوع', 'الحالة', 'مستوى المخاطرة', 'انتهاء العقد', 'الامتثال', ''].map(h => (
                  <th key={h} className="text-start text-xs font-semibold text-gray-500 px-4 py-3">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.items.map(v => {
                const riskCls  = RISK_CLS[v.riskLevel] ?? RISK_CLS.Low
                const days     = contractDaysLeft(v.contractEnd)
                const expiring = days !== null && days <= 30 && days >= 0
                const expired  = days !== null && days < 0
                return (
                  <tr key={v.id} className="hover:bg-gray-50 transition-colors cursor-pointer"
                    onClick={() => navigate(`/vendors/${v.id}`)}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <div className={`w-2 h-2 rounded-full flex-shrink-0 ${riskCls.dot}`} />
                        <div>
                          <p className="font-medium text-gray-800">{v.nameAr}</p>
                          {v.nameEn && <p className="text-xs text-gray-400">{v.nameEn}</p>}
                          {v.category && <p className="text-xs text-gray-400">{v.category}</p>}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-gray-600">{TYPE_LBL[v.type] ?? v.type}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_CLS[v.status] ?? ''}`}>
                        {STATUS_LBL[v.status] ?? v.status}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${riskCls.badge}`}>
                        {RISK_LBL[v.riskLevel] ?? v.riskLevel}
                        {v.riskScore ? ` (${v.riskScore})` : ''}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      {v.contractEnd ? (
                        <span className={`text-xs font-medium ${expired ? 'text-red-600' : expiring ? 'text-amber-600' : 'text-gray-600'}`}>
                          {new Date(v.contractEnd).toLocaleDateString('ar-AE')}
                          {expired && ' — منتهي'}
                          {expiring && !expired && ` — ${days} يوم`}
                        </span>
                      ) : (
                        <span className="text-xs text-gray-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1.5">
                        {v.hasNda          && <span title="NDA" className="text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded">NDA</span>}
                        {v.hasDataAgreement && <span title="اتفاقية بيانات" className="text-xs bg-purple-50 text-purple-600 px-1.5 py-0.5 rounded">DPA</span>}
                        {v.isCertified     && <span title="شهادة" className="text-xs bg-emerald-50 text-emerald-600 px-1.5 py-0.5 rounded">ISO</span>}
                      </div>
                    </td>
                    <td className="px-4 py-3" onClick={e => e.stopPropagation()}>
                      <div className="flex items-center gap-1">
                        <button onClick={() => navigate(`/vendors/${v.id}/edit`)}
                          className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="تعديل">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                          </svg>
                        </button>
                        {canDelete && (
                          <button onClick={() => handleDelete(v)}
                            className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="حذف">
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

      <Pagination
        currentPage={page}
        totalCount={data.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
      />
    </div>
  )
}
