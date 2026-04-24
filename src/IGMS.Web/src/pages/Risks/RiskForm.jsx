import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { riskApi } from '../../services/governanceApi'
import { departmentApi } from '../../services/departmentApi'
import { userApi } from '../../services/userApi'
import { useApi } from '../../hooks/useApi'
import { Spinner, PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white'

function Field({ label, children, required }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label}{required && <span className="text-red-500 ms-1">*</span>}
      </label>
      {children}
    </div>
  )
}

// ── معايير الاحتمالية (ISO 31000) ─────────────────────────────────────────────
const LIKELIHOOD_LEVELS = [
  { value: 1, label: 'نادر جداً',   prob: '< 10٪',  freq: 'مرة كل 5 سنوات أو أكثر',      color: 'green'  },
  { value: 2, label: 'نادر',        prob: '10–29٪', freq: 'مرة كل 2–5 سنوات',            color: 'lime'   },
  { value: 3, label: 'محتمل',       prob: '30–49٪', freq: 'مرة في السنة تقريباً',          color: 'yellow' },
  { value: 4, label: 'مرجح',        prob: '50–79٪', freq: 'عدة مرات في السنة',            color: 'orange' },
  { value: 5, label: 'شبه مؤكد',   prob: '≥ 80٪',  freq: 'مرة شهرياً أو أكثر',          color: 'red'    },
]

// ── معايير الأثر (ISO 31000) ──────────────────────────────────────────────────
const IMPACT_LEVELS = [
  { value: 1, label: 'ضئيل',  financial: '< 50,000 درهم',           operational: 'لا انقطاع في الخدمة',                  color: 'green'  },
  { value: 2, label: 'طفيف',  financial: '50,000–250,000 درهم',     operational: 'انقطاع بضع ساعات',                      color: 'lime'   },
  { value: 3, label: 'متوسط', financial: '250,000–1,000,000 درهم',  operational: 'انقطاع يوم أو يومين',                   color: 'yellow' },
  { value: 4, label: 'جسيم',  financial: '1–5 مليون درهم',          operational: 'ضرر بالسمعة أو شكاوى رسمية',           color: 'orange' },
  { value: 5, label: 'كارثي', financial: '> 5 ملايين درهم',         operational: 'توقف كامل أو عقوبات تنظيمية',          color: 'red'    },
]

// ── تصنيف درجة المخاطرة (4 مستويات) ─────────────────────────────────────────
function getRiskTier(score) {
  if (score >= 17) return { label: 'حرج',    cls: 'bg-red-600 text-white',      border: 'border-red-600',    note: 'تصعيد فوري لصانع القرار' }
  if (score >= 10) return { label: 'عالٍ',   cls: 'bg-orange-500 text-white',   border: 'border-orange-500', note: 'إجراء تخفيف فوري مطلوب' }
  if (score >= 5)  return { label: 'متوسط',  cls: 'bg-yellow-500 text-white',   border: 'border-yellow-500', note: 'خطة تخفيف مطلوبة ومتابعة دورية' }
  return               { label: 'منخفض',  cls: 'bg-emerald-600 text-white',  border: 'border-emerald-600', note: 'يُقبل، يُراجع سنوياً' }
}

const LEVEL_COLORS = {
  green:  { btn: 'border-emerald-600 bg-emerald-50 text-emerald-800',  num: 'bg-emerald-600 text-white'  },
  lime:   { btn: 'border-lime-600 bg-lime-50 text-lime-800',            num: 'bg-lime-600 text-white'      },
  yellow: { btn: 'border-yellow-500 bg-yellow-50 text-yellow-800',      num: 'bg-yellow-500 text-white'    },
  orange: { btn: 'border-orange-500 bg-orange-50 text-orange-800',      num: 'bg-orange-500 text-white'    },
  red:    { btn: 'border-red-600 bg-red-50 text-red-800',               num: 'bg-red-600 text-white'       },
}

function LevelPicker({ levels, value, onChange, label, sublabel }) {
  return (
    <div>
      <p className="text-sm font-semibold text-gray-700 mb-1">{label}</p>
      <p className="text-xs text-gray-400 mb-3">{sublabel}</p>
      <div className="grid grid-cols-5 gap-2">
        {levels.map((lvl) => {
          const active = value === lvl.value
          const c = LEVEL_COLORS[lvl.color]
          return (
            <button
              key={lvl.value}
              type="button"
              onClick={() => onChange(lvl.value)}
              className={[
                'flex flex-col items-center gap-1.5 p-2 rounded-xl border-2 text-center transition-all',
                active ? c.btn + ' shadow-md scale-[1.03]' : 'border-gray-200 bg-white hover:border-gray-300',
              ].join(' ')}
            >
              <span className={`w-7 h-7 rounded-full flex items-center justify-center text-sm font-bold ${active ? c.num : 'bg-gray-100 text-gray-500'}`}>
                {lvl.value}
              </span>
              <span className={`text-[11px] font-semibold leading-tight ${active ? '' : 'text-gray-500'}`}>
                {lvl.label}
              </span>
            </button>
          )
        })}
      </div>
      {/* تفاصيل المستوى المختار */}
      {value && (() => {
        const lvl = levels.find((l) => l.value === value)
        if (!lvl) return null
        return (
          <div className={`mt-2 p-2.5 rounded-lg border ${LEVEL_COLORS[lvl.color].btn} text-xs`}>
            {'prob' in lvl
              ? <><span className="font-semibold">الاحتمالية:</span> {lvl.prob} &nbsp;·&nbsp; <span className="font-semibold">التكرار:</span> {lvl.freq}</>
              : <><span className="font-semibold">مالي:</span> {lvl.financial} &nbsp;·&nbsp; <span className="font-semibold">تشغيلي:</span> {lvl.operational}</>
            }
          </div>
        )
      })()}
    </div>
  )
}

export default function RiskForm() {
  const { t } = useTranslation()
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()
  const { loading: saving, error, execute }       = useApi()
  const { loading: fetching, execute: fetchItem } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    const needed = isEdit ? 'RISKS.UPDATE' : 'RISKS.CREATE'
    if (!hasPermission(needed)) navigate('/risks', { replace: true })
  }, [])

  const [form, setForm] = useState({
    titleAr: '', titleEn: '', code: '', descriptionAr: '', mitigationPlanAr: '',
    category: 0, status: 0, likelihood: 3, impact: 3, departmentId: '', ownerId: '',
  })
  const [departments, setDepartments] = useState([])
  const [users,       setUsers]       = useState([])

  useEffect(() => {
    departmentApi.getAll({ pageSize: 100 }).then((r) => setDepartments(r.data?.data?.items ?? [])).catch(() => {})
    userApi.getLookup().then((r) => setUsers(r.data?.data ?? [])).catch(() => {})
  }, [])

  useEffect(() => {
    if (!isEdit) return
    fetchItem(() => riskApi.getById(id), { silent: true }).then((data) => {
      if (!data) return
      setForm({
        titleAr: data.titleAr ?? '', titleEn: data.titleEn ?? '', code: data.code ?? '',
        descriptionAr: data.descriptionAr ?? '', mitigationPlanAr: data.mitigationPlanAr ?? '',
        category: data.category ?? 0, status: data.status ?? 0,
        likelihood: data.likelihood ?? 3, impact: data.impact ?? 3,
        departmentId: data.departmentId ?? '', ownerId: data.ownerId ?? '',
      })
    })
  }, [id])

  const set = (f, v) => setForm((p) => ({ ...p, [f]: v }))

  const score = form.likelihood * form.impact
  const tier  = getRiskTier(score)

  const handleSubmit = async (e) => {
    e.preventDefault()
    const payload = {
      titleAr: form.titleAr.trim(), titleEn: form.titleEn.trim(), code: form.code.trim().toUpperCase(),
      descriptionAr: form.descriptionAr || null, mitigationPlanAr: form.mitigationPlanAr || null,
      category: Number(form.category), status: Number(form.status),
      likelihood: Number(form.likelihood), impact: Number(form.impact),
      departmentId: form.departmentId !== '' ? Number(form.departmentId) : null,
      ownerId:      form.ownerId      !== '' ? Number(form.ownerId)      : null,
    }
    const result = isEdit
      ? await execute(() => riskApi.update(id, payload), { successMsg: t('risks.messages.saved') })
      : await execute(() => riskApi.create(payload),     { successMsg: t('risks.messages.created') })
    if (result) navigate('/risks')
  }

  if (fetching) return <PageLoader />

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/risks')} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 18l6-6-6-6"/></svg>
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? t('risks.editTitle') : t('risks.createTitle')}</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">

        {/* ── بيانات المخاطرة ───────────────────────────────────── */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
          <h2 className="text-sm font-semibold text-gray-600 border-b pb-2">بيانات المخاطرة</h2>

          <div className="grid grid-cols-2 gap-4">
            <Field label={t('risks.fields.titleAr')} required>
              <input type="text" value={form.titleAr} required onChange={(e) => set('titleAr', e.target.value)} className={inputCls} />
            </Field>
            <Field label={t('risks.fields.code')} required>
              <input type="text" value={form.code} required onChange={(e) => set('code', e.target.value.toUpperCase())} className={`${inputCls} font-mono`} />
            </Field>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Field label={t('risks.fields.category')}>
              <select value={form.category} onChange={(e) => set('category', e.target.value)} className={inputCls}>
                <option value="0">{t('risks.category.operational')}</option>
                <option value="1">{t('risks.category.financial')}</option>
                <option value="2">{t('risks.category.technology')}</option>
                <option value="3">{t('risks.category.legal')}</option>
                <option value="4">{t('risks.category.strategic')}</option>
              </select>
            </Field>
            <Field label={t('risks.fields.status')}>
              <select value={form.status} onChange={(e) => set('status', e.target.value)} className={inputCls}>
                <option value="0">{t('risks.status.open')}</option>
                <option value="1">{t('risks.status.mitigated')}</option>
                <option value="2">{t('risks.status.closed')}</option>
              </select>
            </Field>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Field label={t('risks.fields.department')}>
              <select value={form.departmentId} onChange={(e) => set('departmentId', e.target.value)} className={inputCls}>
                <option value="">{t('common.choose')}</option>
                {departments.map((d) => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
              </select>
            </Field>
            <Field label={t('risks.fields.owner')}>
              <select value={form.ownerId} onChange={(e) => set('ownerId', e.target.value)} className={inputCls}>
                <option value="">{t('common.choose')}</option>
                {users.map((u) => <option key={u.id} value={u.id}>{u.fullNameAr}</option>)}
              </select>
            </Field>
          </div>

          <Field label={t('risks.fields.description')}>
            <textarea value={form.descriptionAr} onChange={(e) => set('descriptionAr', e.target.value)} rows={2} className={inputCls} />
          </Field>
          <Field label={t('risks.fields.mitigationPlan')}>
            <textarea value={form.mitigationPlanAr} onChange={(e) => set('mitigationPlanAr', e.target.value)} rows={2} className={inputCls} />
          </Field>
        </div>

        {/* ── تقييم المخاطرة (ISO 31000) ───────────────────────── */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-6">
          <div className="flex items-center justify-between border-b pb-2">
            <h2 className="text-sm font-semibold text-gray-600">تقييم المخاطرة — معيار ISO 31000</h2>
            <span className="text-xs text-gray-400">الدرجة = الاحتمالية × الأثر</span>
          </div>

          {/* الاحتمالية */}
          <LevelPicker
            levels={LIKELIHOOD_LEVELS}
            value={form.likelihood}
            onChange={(v) => set('likelihood', v)}
            label="الاحتمالية"
            sublabel="ما مدى احتمال وقوع هذه المخاطرة؟ اختر المستوى الذي يطابق الاحتمالية والتكرار المتوقع"
          />

          {/* الأثر */}
          <LevelPicker
            levels={IMPACT_LEVELS}
            value={form.impact}
            onChange={(v) => set('impact', v)}
            label="الأثر"
            sublabel="ما حجم الضرر المالي والتشغيلي إذا تحققت المخاطرة؟ اختر المستوى الأقرب للواقع"
          />

          {/* نتيجة الحساب */}
          <div className={`rounded-xl border-2 ${tier.border} p-4`}>
            <p className="text-xs text-gray-500 mb-2 font-medium">نتيجة الحساب</p>
            <div className="flex items-center gap-3 flex-wrap">
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <span className="w-8 h-8 rounded-full bg-amber-100 flex items-center justify-center font-bold text-amber-700">{form.likelihood}</span>
                <span className="text-gray-400 font-light">×</span>
                <span className="w-8 h-8 rounded-full bg-red-100 flex items-center justify-center font-bold text-red-700">{form.impact}</span>
                <span className="text-gray-400 font-light">=</span>
                <span className={`w-10 h-10 rounded-full flex items-center justify-center font-bold text-lg ${tier.cls}`}>{score}</span>
              </div>
              <div className="flex-1 min-w-0">
                <span className={`inline-flex px-3 py-1 rounded-full text-sm font-bold ${tier.cls} mb-1`}>
                  {tier.label}
                </span>
                <p className="text-xs text-gray-500">{tier.note}</p>
              </div>
            </div>

            {/* مقياس المستويات */}
            <div className="mt-3 grid grid-cols-4 gap-1 text-center text-[10px] font-semibold">
              <div className="rounded py-1 bg-emerald-100 text-emerald-700">1–4 منخفض</div>
              <div className="rounded py-1 bg-yellow-100 text-yellow-700">5–9 متوسط</div>
              <div className="rounded py-1 bg-orange-100 text-orange-700">10–16 عالٍ</div>
              <div className="rounded py-1 bg-red-100 text-red-700">17–25 حرج</div>
            </div>
          </div>
        </div>

        {error && <div className="bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm text-red-700">{error}</div>}

        <div className="flex justify-end gap-3">
          <button type="button" onClick={() => navigate('/risks')} className="px-4 py-2 text-sm border border-gray-300 text-gray-600 rounded-lg hover:bg-gray-50">
            {t('common.cancel')}
          </button>
          <button type="submit" disabled={saving} className="flex items-center gap-2 px-5 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800 disabled:opacity-60">
            {saving && <Spinner size="sm" />}
            {isEdit ? t('common.saveChanges') : t('risks.createTitle')}
          </button>
        </div>
      </form>
    </div>
  )
}
