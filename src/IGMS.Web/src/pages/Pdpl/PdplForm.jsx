import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'

const CATEGORY_OPTS = [
  { value: 'Basic',     label: 'أساسية (اسم، رقم هاتف، بريد إلكتروني)' },
  { value: 'Sensitive', label: 'حساسة (صحة، مالية، بصمة، سجل جنائي)'  },
  { value: 'Special',   label: 'خاصة (بيانات أطفال، جينية)'            },
]

const LEGAL_OPTS = [
  { value: 'Consent',              label: 'موافقة صريحة'         },
  { value: 'ContractPerformance',  label: 'تنفيذ عقد'            },
  { value: 'LegalObligation',      label: 'التزام قانوني'        },
  { value: 'VitalInterests',       label: 'مصلحة حيوية'         },
  { value: 'PublicTask',           label: 'مهمة ذات مصلحة عامة' },
  { value: 'LegitimateInterests',  label: 'مصالح مشروعة'        },
]

const STATUS_OPTS = [
  { value: 'Active',      label: 'نشط'           },
  { value: 'UnderReview', label: 'قيد المراجعة'  },
  { value: 'Retired',     label: 'متقاعد'        },
]

const EMPTY = {
  titleAr: '', titleEn: '', purposeAr: '', dataSubjectsAr: '', retentionPeriod: '',
  securityMeasures: '', dataCategory: 'Basic', legalBasis: 'LegalObligation',
  status: 'Active', isThirdPartySharing: false, thirdPartyDetails: '',
  isCrossBorderTransfer: false, transferCountry: '', transferSafeguards: '',
  departmentId: '', ownerId: '',
}

export default function PdplForm() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const isEdit   = Boolean(id)

  const [form,    setForm]    = useState(EMPTY)
  const [depts,   setDepts]   = useState([])
  const [users,   setUsers]   = useState([])
  const [saving,  setSaving]  = useState(false)
  const [loading, setLoading] = useState(isEdit)
  const [error,   setError]   = useState(null)

  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  const labelCls = 'block text-sm font-medium text-gray-700 mb-1'

  useEffect(() => {
    api.get('/api/v1/departments').then(r => setDepts(r.data?.data ?? [])).catch(() => {})
    api.get('/api/v1/users').then(r => setUsers(r.data?.data?.items ?? r.data?.data ?? [])).catch(() => {})
    if (!isEdit) return
    api.get(`/api/v1/pdpl/${id}`).then(r => {
      const d = r.data.data
      setForm({
        titleAr: d.titleAr ?? '', titleEn: d.titleEn ?? '',
        purposeAr: d.purposeAr ?? '', dataSubjectsAr: d.dataSubjectsAr ?? '',
        retentionPeriod: d.retentionPeriod ?? '', securityMeasures: d.securityMeasures ?? '',
        dataCategory: d.dataCategory, legalBasis: d.legalBasis, status: d.status,
        isThirdPartySharing: d.isThirdPartySharing, thirdPartyDetails: d.thirdPartyDetails ?? '',
        isCrossBorderTransfer: d.isCrossBorderTransfer, transferCountry: d.transferCountry ?? '',
        transferSafeguards: d.transferSafeguards ?? '',
        departmentId: '', ownerId: d.ownerId ?? '',
      })
    }).catch(() => setError('تعذّر تحميل البيانات.'))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  function handleChange(e) {
    const { name, value, type, checked } = e.target
    setForm(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (!form.titleAr.trim()) { setError('العنوان مطلوب.'); return }
    setSaving(true); setError(null)
    const payload = {
      ...form,
      departmentId: form.departmentId ? Number(form.departmentId) : null,
      ownerId:      form.ownerId      ? Number(form.ownerId)      : null,
      thirdPartyDetails: form.isThirdPartySharing ? form.thirdPartyDetails : null,
      transferCountry:   form.isCrossBorderTransfer ? form.transferCountry : null,
      transferSafeguards:form.isCrossBorderTransfer ? form.transferSafeguards : null,
    }
    try {
      if (isEdit) await api.put(`/api/v1/pdpl/${id}`, payload)
      else        await api.post('/api/v1/pdpl', payload)
      navigate('/pdpl')
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'حدث خطأ أثناء الحفظ.')
    } finally { setSaving(false) }
  }

  if (loading) return <PageLoader />

  return (
    <div className="max-w-3xl">
      <div className="mb-6">
        <button onClick={() => navigate('/pdpl')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
          سجل معالجة البيانات
        </button>
        <div className="flex items-center gap-2">
          <span className="text-xs bg-red-100 text-red-700 px-2 py-0.5 rounded-full font-medium">UAE PDPL</span>
          <h1 className="text-xl font-bold text-gray-800">{isEdit ? 'تعديل سجل المعالجة' : 'سجل معالجة جديد'}</h1>
        </div>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-xl">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5">

        {/* Basic info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">معلومات النشاط</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className={labelCls}>اسم نشاط المعالجة (عربي) <span className="text-red-500">*</span></label>
              <input name="titleAr" value={form.titleAr} onChange={handleChange} className={inputCls} required />
            </div>
            <div>
              <label className={labelCls}>الاسم (إنجليزي)</label>
              <input name="titleEn" value={form.titleEn} onChange={handleChange} className={inputCls} dir="ltr" />
            </div>
            <div>
              <label className={labelCls}>الحالة</label>
              <select name="status" value={form.status} onChange={handleChange} className={inputCls}>
                {STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>تصنيف البيانات</label>
              <select name="dataCategory" value={form.dataCategory} onChange={handleChange} className={inputCls}>
                {CATEGORY_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>الأساس القانوني للمعالجة</label>
              <select name="legalBasis" value={form.legalBasis} onChange={handleChange} className={inputCls}>
                {LEGAL_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>القسم المسؤول</label>
              <select name="departmentId" value={form.departmentId} onChange={handleChange} className={inputCls}>
                <option value="">— لا يوجد —</option>
                {depts.map(d => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>مالك البيانات (Data Controller)</label>
              <select name="ownerId" value={form.ownerId} onChange={handleChange} className={inputCls}>
                <option value="">— لا يوجد —</option>
                {users.map(u => <option key={u.id} value={u.id}>{u.fullNameAr || u.username}</option>)}
              </select>
            </div>
          </div>
        </div>

        {/* Processing details */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">تفاصيل المعالجة</h2>
          <div>
            <label className={labelCls}>الغرض من المعالجة</label>
            <textarea name="purposeAr" value={form.purposeAr} onChange={handleChange}
              rows={3} placeholder="صف الغرض الذي تُعالَج البيانات من أجله..." className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>أصحاب البيانات (من؟)</label>
            <input name="dataSubjectsAr" value={form.dataSubjectsAr} onChange={handleChange}
              placeholder="مثال: موظفو الوزارة، زوار الموقع..." className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>فترة الاحتفاظ</label>
            <input name="retentionPeriod" value={form.retentionPeriod} onChange={handleChange}
              placeholder="مثال: 5 سنوات من تاريخ الانتهاء" className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>الضمانات الأمنية المطبّقة</label>
            <textarea name="securityMeasures" value={form.securityMeasures} onChange={handleChange}
              rows={2} placeholder="التشفير، التحكم في الوصول، النسخ الاحتياطي..." className={inputCls} />
          </div>
        </div>

        {/* Third party & cross-border */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">المشاركة والنقل الخارجي</h2>

          <label className="flex items-center gap-3 cursor-pointer">
            <input type="checkbox" name="isThirdPartySharing" checked={form.isThirdPartySharing}
              onChange={handleChange} className="w-4 h-4 text-blue-600 rounded" />
            <span className="text-sm text-gray-700">يتم مشاركة البيانات مع أطراف ثالثة</span>
          </label>
          {form.isThirdPartySharing && (
            <div>
              <label className={labelCls}>تفاصيل الأطراف الثالثة</label>
              <textarea name="thirdPartyDetails" value={form.thirdPartyDetails} onChange={handleChange}
                rows={2} placeholder="اذكر الجهات التي تُشارَك معها البيانات..." className={inputCls} />
            </div>
          )}

          <label className="flex items-center gap-3 cursor-pointer">
            <input type="checkbox" name="isCrossBorderTransfer" checked={form.isCrossBorderTransfer}
              onChange={handleChange} className="w-4 h-4 text-blue-600 rounded" />
            <span className="text-sm text-gray-700">تُنقل البيانات خارج الإمارات</span>
          </label>
          {form.isCrossBorderTransfer && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className={labelCls}>الدولة المستقبِلة</label>
                <input name="transferCountry" value={form.transferCountry} onChange={handleChange} className={inputCls} />
              </div>
              <div>
                <label className={labelCls}>الضمانات المعمول بها</label>
                <input name="transferSafeguards" value={form.transferSafeguards} onChange={handleChange}
                  placeholder="قرار ملاءمة / SCCs / BCRs" className={inputCls} />
              </div>
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="flex gap-3">
          <button type="submit" disabled={saving}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : isEdit ? 'حفظ التعديلات' : 'إنشاء السجل'}
          </button>
          <button type="button" onClick={() => navigate('/pdpl')}
            className="px-6 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors">
            إلغاء
          </button>
        </div>
      </form>
    </div>
  )
}
