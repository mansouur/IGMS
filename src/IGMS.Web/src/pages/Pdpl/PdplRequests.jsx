import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'
import Pagination from '../../components/ui/Pagination'

const REQ_STATUS = {
  Pending:    { cls: 'bg-amber-100 text-amber-700',    label: 'معلق'        },
  InProgress: { cls: 'bg-blue-100 text-blue-700',      label: 'قيد التنفيذ' },
  Completed:  { cls: 'bg-emerald-100 text-emerald-700',label: 'مكتمل'       },
  Rejected:   { cls: 'bg-red-100 text-red-600',        label: 'مرفوض'       },
}
const REQ_TYPE_LBL = {
  Access: 'طلب اطلاع', Correction: 'طلب تصحيح', Deletion: 'طلب حذف',
  Objection: 'اعتراض', Portability: 'نقل البيانات',
}

const STATUS_TABS = [
  { key: '', label: 'الكل' },
  { key: 'Pending',    label: 'معلقة'       },
  { key: 'InProgress', label: 'قيد التنفيذ' },
  { key: 'Completed',  label: 'مكتملة'      },
  { key: 'Rejected',   label: 'مرفوضة'      },
]

export default function PdplRequests() {
  const navigate = useNavigate()
  const [data,    setData]    = useState({ items: [], totalCount: 0 })
  const [status,  setStatus]  = useState('')
  const [page,    setPage]    = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 20

  const load = useCallback(() => {
    setLoading(true)
    api.get('/api/v1/pdpl/requests', { params: { page, pageSize, status: status || undefined } })
      .then(r => setData(r.data?.data ?? { items: [], totalCount: 0 }))
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [page, pageSize, status])

  useEffect(() => { setPage(1) }, [status])
  useEffect(() => { load() }, [load])

  return (
    <div className="space-y-5 max-w-5xl">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <button onClick={() => navigate('/pdpl')}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-1">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="15 18 9 12 15 6"/>
            </svg>
            سجل معالجة البيانات
          </button>
          <h1 className="text-xl font-bold text-gray-800">طلبات أصحاب البيانات</h1>
          <p className="text-xs text-gray-400 mt-0.5">جميع طلبات الاطلاع والتصحيح والحذف — المهلة 30 يوماً</p>
        </div>
      </div>

      {/* Status tabs */}
      <div className="flex gap-2 flex-wrap">
        {STATUS_TABS.map(t => (
          <button key={t.key} onClick={() => setStatus(t.key)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors border ${
              status === t.key
                ? 'bg-blue-600 text-white border-blue-600'
                : 'bg-white text-gray-600 border-gray-200 hover:border-blue-300'
            }`}>
            {t.label}
          </button>
        ))}
      </div>

      {loading ? <PageLoader /> : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 text-sm">لا توجد طلبات</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-100">
              <tr>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">صاحب البيانات</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">نوع الطلب</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">تاريخ الاستلام</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الموعد النهائي</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">المكلّف</th>
                <th className="text-start px-4 py-3 text-xs font-semibold text-gray-500">الحالة</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.items.map(d => {
                const stCfg = REQ_STATUS[d.status] ?? REQ_STATUS.Pending
                const dueDate = new Date(d.dueAt)
                const daysLeft = Math.ceil((dueDate - Date.now()) / (1000 * 60 * 60 * 24))
                const overdue  = d.isOverdue
                return (
                  <tr key={d.id} className={`hover:bg-gray-50 ${overdue ? 'bg-red-50' : ''}`}>
                    <td className="px-4 py-3">
                      <p className="font-medium text-gray-800">{d.subjectNameAr}</p>
                      {d.subjectEmail && <p className="text-xs text-gray-400" dir="ltr">{d.subjectEmail}</p>}
                    </td>
                    <td className="px-4 py-3 text-gray-600">{REQ_TYPE_LBL[d.requestType] ?? d.requestType}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">{new Date(d.receivedAt).toLocaleDateString('ar-AE')}</td>
                    <td className="px-4 py-3">
                      <p className="text-xs text-gray-500">{dueDate.toLocaleDateString('ar-AE')}</p>
                      {d.status === 'Pending' && (
                        <p className={`text-xs font-medium ${overdue ? 'text-red-600' : daysLeft < 7 ? 'text-amber-600' : 'text-gray-400'}`}>
                          {overdue ? 'متأخر' : `${daysLeft} يوم`}
                        </p>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-600 text-xs">{d.assignedToName ?? '—'}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${stCfg.cls}`}>{stCfg.label}</span>
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
