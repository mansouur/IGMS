import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { vendorsApi } from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'

// ── Helpers ───────────────────────────────────────────────────────────────────

const RISK_OPTS = [
  { value: 'Low',      label: 'منخفض',  cls: 'bg-emerald-500' },
  { value: 'Medium',   label: 'متوسط',  cls: 'bg-amber-400'   },
  { value: 'High',     label: 'عالٍ',   cls: 'bg-orange-500'  },
  { value: 'Critical', label: 'حرج',    cls: 'bg-red-500'     },
]
const RISK_BADGE = {
  Low:      'bg-emerald-100 text-emerald-700',
  Medium:   'bg-amber-100 text-amber-700',
  High:     'bg-orange-100 text-orange-700',
  Critical: 'bg-red-100 text-red-700',
}
const STATUS_BADGE = {
  Active:      'bg-emerald-100 text-emerald-700',
  Inactive:    'bg-gray-100 text-gray-600',
  UnderReview: 'bg-amber-100 text-amber-700',
  Blacklisted: 'bg-red-100 text-red-700',
}
const STATUS_LBL = { Active: 'نشط', Inactive: 'غير نشط', UnderReview: 'قيد المراجعة', Blacklisted: 'محظور' }
const TYPE_LBL   = { Supplier: 'مورّد', Partner: 'شريك', Consultant: 'استشاري', Contractor: 'مقاول', CloudProvider: 'خدمات سحابية' }
const RISK_LBL   = { Low: 'منخفض', Medium: 'متوسط', High: 'عالٍ', Critical: 'حرج' }

// ── Risk assessment panel ─────────────────────────────────────────────────────

function RiskPanel({ vendor, onSaved }) {
  const [open,    setOpen]    = useState(false)
  const [level,   setLevel]   = useState(vendor.riskLevel ?? 'Low')
  const [score,   setScore]   = useState(vendor.riskScore ?? '')
  const [notes,   setNotes]   = useState(vendor.riskNotes ?? '')
  const [saving,  setSaving]  = useState(false)

  async function save(e) {
    e.preventDefault()
    setSaving(true)
    try {
      const r = await vendorsApi.assessRisk(vendor.id, {
        riskLevel: level,
        riskScore: score ? Number(score) : null,
        riskNotes: notes || null,
      })
      onSaved(r.data.data)
      setOpen(false)
    } catch { /* silent */ }
    finally { setSaving(false) }
  }

  const rClr = RISK_BADGE[vendor.riskLevel] ?? RISK_BADGE.Low

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-sm font-bold text-gray-700">تقييم المخاطر</h2>
        <button onClick={() => setOpen(o => !o)}
          className="text-xs text-blue-600 hover:underline">
          {open ? 'إلغاء' : 'تحديث التقييم'}
        </button>
      </div>

      {!open ? (
        <div className="space-y-3">
          <div className="flex items-center gap-3">
            <span className={`text-sm font-bold px-3 py-1 rounded-full ${rClr}`}>
              {RISK_LBL[vendor.riskLevel] ?? vendor.riskLevel}
              {vendor.riskScore ? ` — درجة ${vendor.riskScore}/25` : ''}
            </span>
          </div>
          {vendor.riskNotes && <p className="text-sm text-gray-600">{vendor.riskNotes}</p>}
          {vendor.lastAssessedAt && (
            <p className="text-xs text-gray-400">
              آخر تقييم: {new Date(vendor.lastAssessedAt).toLocaleDateString('ar-AE')}
            </p>
          )}
          {!vendor.lastAssessedAt && (
            <p className="text-xs text-amber-600">لم يُقيَّم بعد — يُرجى إجراء تقييم</p>
          )}
        </div>
      ) : (
        <form onSubmit={save} className="space-y-3">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">مستوى المخاطرة</label>
            <div className="grid grid-cols-4 gap-2">
              {RISK_OPTS.map(o => (
                <button type="button" key={o.value}
                  onClick={() => setLevel(o.value)}
                  className={`py-2 rounded-lg text-xs font-bold text-white transition-opacity ${o.cls} ${level === o.value ? 'opacity-100 ring-2 ring-offset-1 ring-gray-400' : 'opacity-60 hover:opacity-80'}`}>
                  {o.label}
                </button>
              ))}
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">درجة المخاطرة (1–25)</label>
            <input type="number" min="1" max="25" value={score}
              onChange={e => setScore(e.target.value)}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">ملاحظات التقييم</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <button type="submit" disabled={saving}
            className="w-full py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : 'حفظ التقييم'}
          </button>
        </form>
      )}
    </div>
  )
}

// ── Field helper ──────────────────────────────────────────────────────────────

function Field({ label, value, dir }) {
  if (!value && value !== 0 && value !== false) return null
  return (
    <div>
      <p className="text-xs text-gray-400 font-medium">{label}</p>
      <p className={`text-sm text-gray-800 mt-0.5 ${dir === 'ltr' ? 'dir-ltr text-start' : ''}`}>{value}</p>
    </div>
  )
}

// ── Main ──────────────────────────────────────────────────────────────────────

export default function VendorDetail() {
  const { id }        = useParams()
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canEdit       = hasPermission('VENDORS.UPDATE')
  const canDelete     = hasPermission('VENDORS.DELETE')

  const [vendor,  setVendor]  = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    vendorsApi.getById(id)
      .then(r => setVendor(r.data.data))
      .catch(() => navigate('/vendors'))
      .finally(() => setLoading(false))
  }, [id])

  async function handleDelete() {
    const ok = await confirm({
      title: 'حذف المورد',
      message: `هل تريد حذف "${vendor.nameAr}"؟ لا يمكن التراجع.`,
      variant: 'danger',
    })
    if (!ok) return
    await vendorsApi.delete(id)
    navigate('/vendors')
  }

  if (loading) return <PageLoader />
  if (!vendor) return null

  const contractDaysLeft = vendor.contractEnd
    ? Math.ceil((new Date(vendor.contractEnd) - Date.now()) / 86400000)
    : null

  return (
    <div className="space-y-5 max-w-4xl">

      {/* Header */}
      <div className="flex items-start justify-between flex-wrap gap-3">
        <div>
          <button onClick={() => navigate('/vendors')}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
            الموردون
          </button>
          <h1 className="text-xl font-bold text-gray-800">{vendor.nameAr}</h1>
          {vendor.nameEn && <p className="text-sm text-gray-400">{vendor.nameEn}</p>}
          <div className="flex flex-wrap items-center gap-2 mt-2">
            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_BADGE[vendor.status] ?? ''}`}>
              {STATUS_LBL[vendor.status] ?? vendor.status}
            </span>
            <span className="text-xs text-gray-400">{TYPE_LBL[vendor.type] ?? vendor.type}</span>
            {vendor.category && <span className="text-xs text-gray-400">· {vendor.category}</span>}
          </div>
        </div>
        <div className="flex gap-2">
          {canEdit && (
            <button onClick={() => navigate(`/vendors/${id}/edit`)}
              className="px-4 py-2 bg-blue-600 text-white text-sm rounded-xl hover:bg-blue-700 transition-colors">
              تعديل
            </button>
          )}
          {canDelete && (
            <button onClick={handleDelete}
              className="px-4 py-2 bg-red-50 text-red-600 border border-red-200 text-sm rounded-xl hover:bg-red-100 transition-colors">
              حذف
            </button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">

        {/* Left: Details */}
        <div className="lg:col-span-2 space-y-4">

          {/* Contract */}
          <div className="bg-white rounded-xl border border-gray-200 p-5">
            <h2 className="text-sm font-bold text-gray-700 mb-4">بيانات العقد</h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              <Field label="تاريخ البداية"
                value={vendor.contractStart ? new Date(vendor.contractStart).toLocaleDateString('ar-AE') : null} />
              <div>
                <p className="text-xs text-gray-400 font-medium">تاريخ الانتهاء</p>
                {vendor.contractEnd ? (
                  <p className={`text-sm mt-0.5 font-medium ${contractDaysLeft < 0 ? 'text-red-600' : contractDaysLeft <= 30 ? 'text-amber-600' : 'text-gray-800'}`}>
                    {new Date(vendor.contractEnd).toLocaleDateString('ar-AE')}
                    {contractDaysLeft < 0  && ' — منتهي'}
                    {contractDaysLeft >= 0 && contractDaysLeft <= 30 && ` (${contractDaysLeft} يوم متبقي)`}
                  </p>
                ) : <p className="text-sm text-gray-400 mt-0.5">—</p>}
              </div>
              {vendor.contractValue && (
                <Field label="قيمة العقد"
                  value={`${Number(vendor.contractValue).toLocaleString('ar-AE')} د.إ`} />
              )}
            </div>
          </div>

          {/* Contact */}
          <div className="bg-white rounded-xl border border-gray-200 p-5">
            <h2 className="text-sm font-bold text-gray-700 mb-4">بيانات التواصل</h2>
            <div className="grid grid-cols-2 gap-4">
              <Field label="جهة التواصل" value={vendor.contactName} />
              <Field label="البريد الإلكتروني" value={vendor.contactEmail} dir="ltr" />
              <Field label="رقم الهاتف" value={vendor.contactPhone} dir="ltr" />
              <Field label="الموقع الإلكتروني" value={vendor.website} dir="ltr" />
            </div>
          </div>

          {/* Compliance */}
          <div className="bg-white rounded-xl border border-gray-200 p-5">
            <h2 className="text-sm font-bold text-gray-700 mb-4">الامتثال والضمانات</h2>
            <div className="grid grid-cols-3 gap-3">
              {[
                { key: 'hasNda',           label: 'اتفاقية NDA',  tag: 'NDA' },
                { key: 'hasDataAgreement', label: 'اتفاقية DPA',  tag: 'DPA' },
                { key: 'isCertified',      label: 'شهادة دولية', tag: 'ISO' },
              ].map(({ key, label, tag }) => (
                <div key={key} className={`rounded-lg p-3 text-center border ${vendor[key] ? 'bg-emerald-50 border-emerald-200' : 'bg-gray-50 border-gray-100'}`}>
                  <p className={`text-sm font-bold ${vendor[key] ? 'text-emerald-700' : 'text-gray-400'}`}>{tag}</p>
                  <p className="text-xs text-gray-500 mt-0.5">{label}</p>
                  <p className={`text-xs mt-1 ${vendor[key] ? 'text-emerald-600' : 'text-gray-400'}`}>
                    {vendor[key] ? '✓ موجود' : '✗ غير موجود'}
                  </p>
                </div>
              ))}
            </div>
          </div>

          {/* Notes */}
          {vendor.notes && (
            <div className="bg-white rounded-xl border border-gray-200 p-5">
              <h2 className="text-sm font-bold text-gray-700 mb-2">ملاحظات</h2>
              <p className="text-sm text-gray-600">{vendor.notes}</p>
            </div>
          )}
        </div>

        {/* Right: Risk + Meta */}
        <div className="space-y-4">
          <RiskPanel vendor={vendor} onSaved={setVendor} />

          <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-3">
            <h2 className="text-sm font-bold text-gray-700">معلومات إضافية</h2>
            {vendor.departmentName && <Field label="القسم المسؤول" value={vendor.departmentName} />}
            <Field label="تاريخ الإضافة"
              value={new Date(vendor.createdAt).toLocaleDateString('ar-AE')} />
          </div>
        </div>
      </div>
    </div>
  )
}
