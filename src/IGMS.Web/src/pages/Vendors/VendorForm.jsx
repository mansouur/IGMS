import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { vendorsApi } from '../../services/api'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'

const TYPE_OPTS = [
  { value: 'Supplier',      label: 'مورّد' },
  { value: 'Partner',       label: 'شريك' },
  { value: 'Consultant',    label: 'استشاري' },
  { value: 'Contractor',    label: 'مقاول' },
  { value: 'CloudProvider', label: 'خدمات سحابية' },
]
const STATUS_OPTS = [
  { value: 'Active',      label: 'نشط' },
  { value: 'Inactive',    label: 'غير نشط' },
  { value: 'UnderReview', label: 'قيد المراجعة' },
  { value: 'Blacklisted', label: 'محظور' },
]

const EMPTY = {
  nameAr: '', nameEn: '', type: 'Supplier', status: 'Active',
  category: '', contactName: '', contactEmail: '', contactPhone: '',
  website: '', notes: '', contractStart: '', contractEnd: '',
  contractValue: '', departmentId: '', hasNda: false, hasDataAgreement: false, isCertified: false,
}

export default function VendorForm() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const isEdit   = Boolean(id)

  const [form,     setForm]     = useState(EMPTY)
  const [depts,    setDepts]    = useState([])
  const [saving,   setSaving]   = useState(false)
  const [loading,  setLoading]  = useState(isEdit)
  const [error,    setError]    = useState(null)

  useEffect(() => {
    api.get('/api/v1/departments').then(r => setDepts(r.data?.data ?? [])).catch(() => {})
    if (!isEdit) return
    vendorsApi.getById(id)
      .then(r => {
        const v = r.data.data
        setForm({
          nameAr:          v.nameAr        ?? '',
          nameEn:          v.nameEn        ?? '',
          type:            v.type          ?? 'Supplier',
          status:          v.status        ?? 'Active',
          category:        v.category      ?? '',
          contactName:     v.contactName   ?? '',
          contactEmail:    v.contactEmail  ?? '',
          contactPhone:    v.contactPhone  ?? '',
          website:         v.website       ?? '',
          notes:           v.notes         ?? '',
          contractStart:   v.contractStart ? v.contractStart.slice(0, 10) : '',
          contractEnd:     v.contractEnd   ? v.contractEnd.slice(0, 10)   : '',
          contractValue:   v.contractValue ?? '',
          departmentId:    v.departmentId  ?? '',
          hasNda:          v.hasNda        ?? false,
          hasDataAgreement: v.hasDataAgreement ?? false,
          isCertified:     v.isCertified   ?? false,
        })
      })
      .catch(() => setError('تعذّر تحميل بيانات المورد.'))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  function handleChange(e) {
    const { name, value, type, checked } = e.target
    setForm(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (!form.nameAr.trim()) { setError('اسم المورد بالعربي مطلوب.'); return }
    setSaving(true); setError(null)
    const payload = {
      ...form,
      contractStart: form.contractStart || null,
      contractEnd:   form.contractEnd   || null,
      contractValue: form.contractValue ? Number(form.contractValue) : null,
      departmentId:  form.departmentId  ? Number(form.departmentId) : null,
    }
    try {
      if (isEdit) await vendorsApi.update(id, payload)
      else        await vendorsApi.create(payload)
      navigate('/vendors')
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'حدث خطأ أثناء الحفظ.')
    } finally {
      setSaving(false)
    }
  }

  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  const labelCls = 'block text-sm font-medium text-gray-700 mb-1'

  if (loading) return <PageLoader />

  return (
    <div className="max-w-3xl">
      {/* Header */}
      <div className="mb-6">
        <button onClick={() => navigate('/vendors')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
          الموردون
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? 'تعديل مورد' : 'إضافة مورد جديد'}</h1>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-xl">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">

        {/* Basic info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">المعلومات الأساسية</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className={labelCls}>اسم المورد (عربي) <span className="text-red-500">*</span></label>
              <input name="nameAr" value={form.nameAr} onChange={handleChange} className={inputCls} required />
            </div>
            <div>
              <label className={labelCls}>اسم المورد (إنجليزي)</label>
              <input name="nameEn" value={form.nameEn} onChange={handleChange} className={inputCls} dir="ltr" />
            </div>
            <div>
              <label className={labelCls}>النوع</label>
              <select name="type" value={form.type} onChange={handleChange} className={inputCls}>
                {TYPE_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>الحالة</label>
              <select name="status" value={form.status} onChange={handleChange} className={inputCls}>
                {STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>التصنيف</label>
              <input name="category" value={form.category} onChange={handleChange}
                placeholder="مثال: تقنية المعلومات، قانوني، لوجستي" className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>القسم المسؤول</label>
              <select name="departmentId" value={form.departmentId} onChange={handleChange} className={inputCls}>
                <option value="">— لا يوجد —</option>
                {depts.map(d => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className={labelCls}>ملاحظات</label>
            <textarea name="notes" value={form.notes} onChange={handleChange}
              rows={3} className={inputCls} />
          </div>
        </div>

        {/* Contact */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">بيانات التواصل</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className={labelCls}>اسم جهة التواصل</label>
              <input name="contactName" value={form.contactName} onChange={handleChange} className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>البريد الإلكتروني</label>
              <input name="contactEmail" type="email" value={form.contactEmail} onChange={handleChange}
                className={inputCls} dir="ltr" />
            </div>
            <div>
              <label className={labelCls}>رقم الهاتف</label>
              <input name="contactPhone" value={form.contactPhone} onChange={handleChange}
                className={inputCls} dir="ltr" />
            </div>
            <div>
              <label className={labelCls}>الموقع الإلكتروني</label>
              <input name="website" type="url" value={form.website} onChange={handleChange}
                placeholder="https://" className={inputCls} dir="ltr" />
            </div>
          </div>
        </div>

        {/* Contract */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">بيانات العقد</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className={labelCls}>تاريخ البداية</label>
              <input name="contractStart" type="date" value={form.contractStart} onChange={handleChange} className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>تاريخ الانتهاء</label>
              <input name="contractEnd" type="date" value={form.contractEnd} onChange={handleChange} className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>قيمة العقد (د.إ)</label>
              <input name="contractValue" type="number" min="0" value={form.contractValue} onChange={handleChange}
                className={inputCls} dir="ltr" />
            </div>
          </div>
        </div>

        {/* Compliance */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2 mb-4">الامتثال والضمانات</h2>
          <div className="space-y-3">
            {[
              { name: 'hasNda',           label: 'اتفاقية عدم إفصاح (NDA) موقّعة' },
              { name: 'hasDataAgreement', label: 'اتفاقية معالجة البيانات (DPA) موقّعة' },
              { name: 'isCertified',      label: 'حاصل على شهادة دولية (ISO 27001 / SOC 2)' },
            ].map(({ name, label }) => (
              <label key={name} className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" name={name} checked={form[name]} onChange={handleChange}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
                <span className="text-sm text-gray-700">{label}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3">
          <button type="submit" disabled={saving}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : isEdit ? 'حفظ التعديلات' : 'إضافة المورد'}
          </button>
          <button type="button" onClick={() => navigate('/vendors')}
            className="px-6 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors">
            إلغاء
          </button>
        </div>
      </form>
    </div>
  )
}
