import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const CATEGORY_CFG = {
  Basic:     { cls: 'bg-blue-50 text-blue-700',    label: 'أساسية'  },
  Sensitive: { cls: 'bg-orange-50 text-orange-700',label: 'حساسة'   },
  Special:   { cls: 'bg-red-50 text-red-700',      label: 'خاصة'    },
}
const LEGAL_LBL = {
  Consent: 'موافقة', ContractPerformance: 'تنفيذ عقد', LegalObligation: 'التزام قانوني',
  VitalInterests: 'مصلحة حيوية', PublicTask: 'مهمة عامة', LegitimateInterests: 'مصالح مشروعة',
}
const REQ_STATUS = {
  Pending:    { cls: 'bg-amber-100 text-amber-700', label: 'معلق'          },
  InProgress: { cls: 'bg-blue-100 text-blue-700',   label: 'قيد التنفيذ'   },
  Completed:  { cls: 'bg-emerald-100 text-emerald-700', label: 'مكتمل'     },
  Rejected:   { cls: 'bg-red-100 text-red-600',     label: 'مرفوض'         },
}
const REQ_TYPE_LBL = {
  Access: 'طلب اطلاع', Correction: 'طلب تصحيح', Deletion: 'طلب حذف',
  Objection: 'اعتراض', Portability: 'نقل البيانات',
}

function InfoRow({ label, value }) {
  return (
    <div>
      <p className="text-xs text-gray-400 mb-0.5">{label}</p>
      <p className="text-sm font-medium text-gray-800">{value || '—'}</p>
    </div>
  )
}

// ── Consent Modal ─────────────────────────────────────────────────────────────

function ConsentModal({ onSave, onClose }) {
  const [form, setForm] = useState({ subjectNameAr: '', subjectEmail: '', subjectIdNumber: '', isConsented: true, notes: '' })
  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4">
        <h3 className="text-base font-bold text-gray-800">تسجيل موافقة</h3>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">اسم صاحب البيانات <span className="text-red-500">*</span></label>
          <input value={form.subjectNameAr} onChange={e => setForm(f => ({ ...f, subjectNameAr: e.target.value }))} className={inputCls} />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
            <input type="email" value={form.subjectEmail} onChange={e => setForm(f => ({ ...f, subjectEmail: e.target.value }))} className={inputCls} dir="ltr" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">رقم الهوية</label>
            <input value={form.subjectIdNumber} onChange={e => setForm(f => ({ ...f, subjectIdNumber: e.target.value }))} className={inputCls} dir="ltr" />
          </div>
        </div>
        <label className="flex items-center gap-2 cursor-pointer">
          <input type="checkbox" checked={form.isConsented} onChange={e => setForm(f => ({ ...f, isConsented: e.target.checked }))} className="w-4 h-4" />
          <span className="text-sm text-gray-700">وافق على المعالجة</span>
        </label>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
          <input value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} className={inputCls} />
        </div>
        <div className="flex gap-3">
          <button onClick={() => onSave(form)} disabled={!form.subjectNameAr.trim()}
            className="flex-1 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            حفظ
          </button>
          <button onClick={onClose} className="flex-1 py-2 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Request Modal ─────────────────────────────────────────────────────────────

function RequestModal({ users, onSave, onClose }) {
  const [form, setForm] = useState({ requestType: 'Access', subjectNameAr: '', subjectEmail: '', detailsAr: '', assignedToId: '' })
  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4">
        <h3 className="text-base font-bold text-gray-800">طلب بيانات جديد</h3>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">نوع الطلب</label>
          <select value={form.requestType} onChange={e => setForm(f => ({ ...f, requestType: e.target.value }))} className={inputCls}>
            {Object.entries(REQ_TYPE_LBL).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">اسم صاحب البيانات <span className="text-red-500">*</span></label>
          <input value={form.subjectNameAr} onChange={e => setForm(f => ({ ...f, subjectNameAr: e.target.value }))} className={inputCls} />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
          <input type="email" value={form.subjectEmail} onChange={e => setForm(f => ({ ...f, subjectEmail: e.target.value }))} className={inputCls} dir="ltr" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">تفاصيل الطلب</label>
          <textarea value={form.detailsAr} onChange={e => setForm(f => ({ ...f, detailsAr: e.target.value }))} rows={3} className={inputCls} />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">تكليف إلى</label>
          <select value={form.assignedToId} onChange={e => setForm(f => ({ ...f, assignedToId: e.target.value }))} className={inputCls}>
            <option value="">— غير مكلّف —</option>
            {users.map(u => <option key={u.id} value={u.id}>{u.fullNameAr || u.username}</option>)}
          </select>
        </div>
        <div className="flex gap-3">
          <button onClick={() => onSave({ ...form, assignedToId: form.assignedToId ? Number(form.assignedToId) : null })}
            disabled={!form.subjectNameAr.trim()}
            className="flex-1 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            حفظ
          </button>
          <button onClick={onClose} className="flex-1 py-2 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Main ──────────────────────────────────────────────────────────────────────

export default function PdplDetail() {
  const { id }        = useParams()
  const navigate      = useNavigate()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canManage     = hasPermission('PDPL.MANAGE')

  const [record,      setRecord]      = useState(null)
  const [users,       setUsers]       = useState([])
  const [loading,     setLoading]     = useState(true)
  const [error,       setError]       = useState(null)
  const [showConsent, setShowConsent] = useState(false)
  const [showRequest, setShowRequest] = useState(false)
  const [activeTab,   setActiveTab]   = useState('info')

  const reload = () =>
    api.get(`/api/v1/pdpl/${id}`).then(r => setRecord(r.data.data)).catch(() => setError('تعذّر تحميل البيانات.'))

  useEffect(() => {
    reload().finally(() => setLoading(false))
    api.get('/api/v1/users').then(r => setUsers(r.data?.data?.items ?? r.data?.data ?? [])).catch(() => {})
  }, [id])

  async function markReviewed() {
    await api.post(`/api/v1/pdpl/${id}/review`)
    reload()
  }

  async function withdrawConsent(consentId) {
    await api.post(`/api/v1/pdpl/${id}/consents/${consentId}/withdraw`)
    reload()
  }

  async function resolveRequest(requestId, rejected) {
    const resolution = prompt(rejected ? 'سبب الرفض:' : 'ملخص الإجراء المتخذ:')
    if (resolution === null) return
    await api.post(`/api/v1/pdpl/${id}/requests/${requestId}/resolve`, { resolutionAr: resolution, rejected })
    reload()
  }

  if (loading) return <PageLoader />
  if (!record) return <div className="p-8 text-center text-red-500">{error || 'السجل غير موجود.'}</div>

  const catCfg = CATEGORY_CFG[record.dataCategory] ?? CATEGORY_CFG.Basic
  const pendingReqs = record.dataRequests.filter(r => r.status === 'Pending')

  return (
    <div className="max-w-4xl space-y-5">
      {showConsent && (
        <ConsentModal
          onSave={async data => {
            setShowConsent(false)
            await api.post(`/api/v1/pdpl/${id}/consents`, data)
            reload()
          }}
          onClose={() => setShowConsent(false)}
        />
      )}
      {showRequest && (
        <RequestModal
          users={users}
          onSave={async data => {
            setShowRequest(false)
            await api.post(`/api/v1/pdpl/${id}/requests`, data)
            reload()
          }}
          onClose={() => setShowRequest(false)}
        />
      )}

      {/* Header */}
      <div>
        <button onClick={() => navigate('/pdpl')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
          سجل معالجة البيانات
        </button>
        <div className="flex items-start justify-between gap-4 flex-wrap">
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-xs bg-red-100 text-red-700 px-2 py-0.5 rounded-full font-medium">UAE PDPL</span>
              <h1 className="text-xl font-bold text-gray-800">{record.titleAr}</h1>
              <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${catCfg.cls}`}>{catCfg.label}</span>
            </div>
            <p className="text-sm text-gray-500 mt-1">{LEGAL_LBL[record.legalBasis] ?? record.legalBasis}</p>
          </div>
          <div className="flex gap-2 flex-wrap">
            {canManage && (
              <button onClick={markReviewed}
                className="px-3 py-2 bg-white border border-gray-200 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-50">
                ✓ تسجيل مراجعة
              </button>
            )}
            <button onClick={() => navigate(`/pdpl/${id}/edit`)}
              className="px-3 py-2 bg-white border border-gray-200 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-50">
              تعديل
            </button>
          </div>
        </div>
      </div>

      {pendingReqs.length > 0 && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 flex items-center gap-2">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#d97706" strokeWidth="2">
            <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <p className="text-sm text-amber-700 font-medium">{pendingReqs.length} طلب بيانات معلق — يجب الرد خلال 30 يوماً</p>
        </div>
      )}

      {/* Tabs */}
      <div className="flex gap-1 border-b border-gray-200">
        {[
          { key: 'info',     label: 'معلومات السجل' },
          { key: 'consents', label: `الموافقات (${record.consents.length})` },
          { key: 'requests', label: `طلبات البيانات (${record.dataRequests.length})` },
        ].map(t => (
          <button key={t.key} onClick={() => setActiveTab(t.key)}
            className={`px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
              activeTab === t.key
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}>
            {t.label}
          </button>
        ))}
      </div>

      {/* Info tab */}
      {activeTab === 'info' && (
        <div className="space-y-4">
          <div className="bg-white rounded-xl border border-gray-200 p-5 grid grid-cols-2 md:grid-cols-3 gap-5">
            <InfoRow label="القسم المسؤول"   value={record.departmentName} />
            <InfoRow label="مالك البيانات"   value={record.ownerName} />
            <InfoRow label="فترة الاحتفاظ"   value={record.retentionPeriod} />
            <InfoRow label="آخر مراجعة"       value={record.lastReviewedAt ? new Date(record.lastReviewedAt).toLocaleDateString('ar-AE') : 'لم تُراجَع'} />
            <InfoRow label="مشاركة خارجية"   value={record.isThirdPartySharing ? 'نعم' : 'لا'} />
            <InfoRow label="نقل عابر للحدود" value={record.isCrossBorderTransfer ? 'نعم' : 'لا'} />
          </div>
          {record.purposeAr && (
            <div className="bg-white rounded-xl border border-gray-200 p-5">
              <p className="text-xs text-gray-400 mb-1">الغرض من المعالجة</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{record.purposeAr}</p>
            </div>
          )}
          {record.dataSubjectsAr && (
            <div className="bg-white rounded-xl border border-gray-200 p-5">
              <p className="text-xs text-gray-400 mb-1">أصحاب البيانات</p>
              <p className="text-sm text-gray-700">{record.dataSubjectsAr}</p>
            </div>
          )}
          {record.securityMeasures && (
            <div className="bg-white rounded-xl border border-gray-200 p-5">
              <p className="text-xs text-gray-400 mb-1">الضمانات الأمنية</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{record.securityMeasures}</p>
            </div>
          )}
          {record.isThirdPartySharing && record.thirdPartyDetails && (
            <div className="bg-white rounded-xl border border-gray-200 p-5">
              <p className="text-xs text-gray-400 mb-1">الأطراف الثالثة</p>
              <p className="text-sm text-gray-700">{record.thirdPartyDetails}</p>
            </div>
          )}
          {record.isCrossBorderTransfer && (
            <div className="bg-white rounded-xl border border-gray-200 p-5 grid grid-cols-2 gap-4">
              <InfoRow label="الدولة المستقبِلة"     value={record.transferCountry} />
              <InfoRow label="الضمانات المعمول بها"  value={record.transferSafeguards} />
            </div>
          )}
        </div>
      )}

      {/* Consents tab */}
      {activeTab === 'consents' && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-bold text-gray-700">الموافقات المسجّلة</h2>
            {canManage && (
              <button onClick={() => setShowConsent(true)}
                className="text-xs text-blue-600 hover:underline flex items-center gap-1">
                + إضافة موافقة
              </button>
            )}
          </div>
          {record.consents.length === 0 ? (
            <p className="text-xs text-gray-400">لا توجد موافقات مسجّلة</p>
          ) : (
            <div className="space-y-2">
              {record.consents.map(c => (
                <div key={c.id} className="flex items-center justify-between p-3 border border-gray-100 rounded-xl">
                  <div>
                    <p className="text-sm font-medium text-gray-800">{c.subjectNameAr}</p>
                    <p className="text-xs text-gray-400">{c.subjectEmail} · {new Date(c.consentedAt).toLocaleDateString('ar-AE')}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${c.isConsented && !c.withdrawnAt ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-600'}`}>
                      {c.isConsented && !c.withdrawnAt ? 'موافق' : 'سُحبت'}
                    </span>
                    {canManage && c.isConsented && !c.withdrawnAt && (
                      <button onClick={() => withdrawConsent(c.id)}
                        className="text-xs text-red-500 hover:underline">سحب</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Requests tab */}
      {activeTab === 'requests' && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-bold text-gray-700">طلبات أصحاب البيانات</h2>
            {canManage && (
              <button onClick={() => setShowRequest(true)}
                className="text-xs text-blue-600 hover:underline flex items-center gap-1">
                + طلب جديد
              </button>
            )}
          </div>
          {record.dataRequests.length === 0 ? (
            <p className="text-xs text-gray-400">لا توجد طلبات</p>
          ) : (
            <div className="space-y-3">
              {record.dataRequests.map(d => {
                const stCfg = REQ_STATUS[d.status] ?? REQ_STATUS.Pending
                const dueDate = new Date(d.dueAt)
                const daysLeft = Math.ceil((dueDate - Date.now()) / (1000 * 60 * 60 * 24))
                return (
                  <div key={d.id} className={`p-4 border rounded-xl ${d.isOverdue ? 'border-red-200 bg-red-50' : 'border-gray-100'}`}>
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="text-sm font-medium text-gray-800">{d.subjectNameAr}</span>
                          <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${stCfg.cls}`}>{stCfg.label}</span>
                          <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                            {REQ_TYPE_LBL[d.requestType] ?? d.requestType}
                          </span>
                        </div>
                        <p className="text-xs text-gray-400 mt-0.5">
                          مستلم: {new Date(d.receivedAt).toLocaleDateString('ar-AE')} ·
                          الموعد النهائي: {dueDate.toLocaleDateString('ar-AE')}
                          {d.status === 'Pending' && (
                            <span className={` ms-1 font-medium ${daysLeft < 7 ? 'text-red-600' : 'text-gray-500'}`}>
                              ({daysLeft > 0 ? `${daysLeft} يوم متبقٍ` : 'متأخر'})
                            </span>
                          )}
                        </p>
                        {d.detailsAr && <p className="text-xs text-gray-600 mt-1">{d.detailsAr}</p>}
                        {d.resolutionAr && <p className="text-xs text-emerald-700 mt-1">الإجراء: {d.resolutionAr}</p>}
                      </div>
                      {canManage && d.status === 'Pending' && (
                        <div className="flex gap-1 flex-shrink-0">
                          <button onClick={() => resolveRequest(d.id, false)}
                            className="px-2 py-1 text-xs bg-emerald-600 text-white rounded-lg hover:bg-emerald-700">
                            إنجاز
                          </button>
                          <button onClick={() => resolveRequest(d.id, true)}
                            className="px-2 py-1 text-xs bg-red-600 text-white rounded-lg hover:bg-red-700">
                            رفض
                          </button>
                        </div>
                      )}
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
