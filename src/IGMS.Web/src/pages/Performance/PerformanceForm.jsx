import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'

const PERIOD_OPTS = [
  { value: 'Annual',    label: 'سنوي'          },
  { value: 'Q1',        label: 'الربع الأول'   },
  { value: 'Q2',        label: 'الربع الثاني'  },
  { value: 'Q3',        label: 'الربع الثالث'  },
  { value: 'Q4',        label: 'الربع الرابع'  },
  { value: 'Probation', label: 'فترة تجربة'    },
]

const GOAL_STATUS_OPTS = [
  { value: 'Pending',           label: 'قيد التنفيذ'  },
  { value: 'Achieved',          label: 'محقق'         },
  { value: 'PartiallyAchieved', label: 'محقق جزئياً'  },
  { value: 'NotAchieved',       label: 'لم يتحقق'     },
]

const EMPTY_GOAL = { titleAr: '', descriptionAr: '', weight: '', targetValue: '', actualValue: '', rating: '', status: 'Pending' }

const EMPTY_FORM = {
  employeeId: '', reviewerId: '', period: 'Annual', year: new Date().getFullYear(),
  departmentId: '', overallRating: '', strengthsAr: '', areasForImprovementAr: '',
  commentsAr: '', employeeCommentsAr: '', goals: [{ ...EMPTY_GOAL }],
}

export default function PerformanceForm() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const isEdit   = Boolean(id)

  const [form,    setForm]    = useState(EMPTY_FORM)
  const [users,   setUsers]   = useState([])
  const [depts,   setDepts]   = useState([])
  const [saving,  setSaving]  = useState(false)
  const [loading, setLoading] = useState(isEdit)
  const [error,   setError]   = useState(null)

  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  const labelCls = 'block text-sm font-medium text-gray-700 mb-1'

  useEffect(() => {
    api.get('/api/v1/users').then(r => setUsers(r.data?.data?.items ?? r.data?.data ?? [])).catch(() => {})
    api.get('/api/v1/departments').then(r => setDepts(r.data?.data ?? [])).catch(() => {})

    if (!isEdit) return
    api.get(`/api/v1/performance/${id}`).then(r => {
      const d = r.data.data
      setForm({
        employeeId:           d.employeeId,
        reviewerId:           d.reviewerId,
        period:               d.period,
        year:                 d.year,
        departmentId:         d.departmentName ? '' : '',  // departmentId not in detail — re-fetch if needed
        overallRating:        d.overallRating ?? '',
        strengthsAr:          d.strengthsAr ?? '',
        areasForImprovementAr:d.areasForImprovementAr ?? '',
        commentsAr:           d.commentsAr ?? '',
        employeeCommentsAr:   d.employeeCommentsAr ?? '',
        goals: d.goals.length ? d.goals.map(g => ({
          titleAr:      g.titleAr,
          descriptionAr:g.descriptionAr ?? '',
          weight:       g.weight,
          targetValue:  g.targetValue ?? '',
          actualValue:  g.actualValue ?? '',
          rating:       g.rating ?? '',
          status:       g.status,
        })) : [{ ...EMPTY_GOAL }],
      })
    }).catch(() => setError('تعذّر تحميل بيانات التقييم.'))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  function handleChange(e) {
    const { name, value } = e.target
    setForm(f => ({ ...f, [name]: value }))
  }

  function handleGoalChange(idx, e) {
    const { name, value } = e.target
    setForm(f => {
      const goals = [...f.goals]
      goals[idx] = { ...goals[idx], [name]: value }
      return { ...f, goals }
    })
  }

  function addGoal() {
    setForm(f => ({ ...f, goals: [...f.goals, { ...EMPTY_GOAL }] }))
  }

  function removeGoal(idx) {
    setForm(f => ({ ...f, goals: f.goals.filter((_, i) => i !== idx) }))
  }

  const totalWeight = form.goals.reduce((s, g) => s + (parseFloat(g.weight) || 0), 0)

  async function handleSubmit(e) {
    e.preventDefault()
    if (!form.employeeId) { setError('اختر الموظف.'); return }
    if (!form.reviewerId)  { setError('اختر المقيِّم.'); return }
    if (form.goals.length === 0) { setError('أضف هدفاً واحداً على الأقل.'); return }

    setSaving(true); setError(null)
    const payload = {
      employeeId:            Number(form.employeeId),
      reviewerId:            Number(form.reviewerId),
      period:                form.period,
      year:                  Number(form.year),
      departmentId:          form.departmentId ? Number(form.departmentId) : null,
      overallRating:         form.overallRating ? parseFloat(form.overallRating) : null,
      strengthsAr:           form.strengthsAr || null,
      areasForImprovementAr: form.areasForImprovementAr || null,
      commentsAr:            form.commentsAr || null,
      employeeCommentsAr:    form.employeeCommentsAr || null,
      goals: form.goals.map(g => ({
        titleAr:       g.titleAr,
        descriptionAr: g.descriptionAr || null,
        weight:        parseFloat(g.weight) || 0,
        targetValue:   g.targetValue !== '' ? parseFloat(g.targetValue) : null,
        actualValue:   g.actualValue !== '' ? parseFloat(g.actualValue) : null,
        rating:        g.rating !== '' ? parseFloat(g.rating) : null,
        status:        g.status,
      })),
    }

    try {
      if (isEdit) await api.put(`/api/v1/performance/${id}`, payload)
      else        await api.post('/api/v1/performance', payload)
      navigate('/performance')
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'حدث خطأ أثناء الحفظ.')
    } finally { setSaving(false) }
  }

  if (loading) return <PageLoader />

  return (
    <div className="max-w-3xl">
      <div className="mb-6">
        <button onClick={() => navigate('/performance')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
          تقييمات الأداء
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? 'تعديل التقييم' : 'تقييم أداء جديد'}</h1>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-xl">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5">

        {/* Basic info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">معلومات التقييم</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className={labelCls}>الموظف <span className="text-red-500">*</span></label>
              <select name="employeeId" value={form.employeeId} onChange={handleChange} className={inputCls} required>
                <option value="">— اختر الموظف —</option>
                {users.map(u => <option key={u.id} value={u.id}>{u.fullNameAr || u.username}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>المقيِّم <span className="text-red-500">*</span></label>
              <select name="reviewerId" value={form.reviewerId} onChange={handleChange} className={inputCls} required>
                <option value="">— اختر المقيِّم —</option>
                {users.map(u => <option key={u.id} value={u.id}>{u.fullNameAr || u.username}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>الفترة</label>
              <select name="period" value={form.period} onChange={handleChange} className={inputCls}>
                {PERIOD_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>السنة</label>
              <input name="year" type="number" value={form.year} onChange={handleChange}
                min="2020" max="2035" className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>القسم</label>
              <select name="departmentId" value={form.departmentId} onChange={handleChange} className={inputCls}>
                <option value="">— لا يوجد —</option>
                {depts.map(d => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>التقييم الإجمالي (1–5)</label>
              <input name="overallRating" type="number" value={form.overallRating} onChange={handleChange}
                min="1" max="5" step="0.1" placeholder="مثال: 4.2" className={inputCls} />
            </div>
          </div>
        </div>

        {/* Goals */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <div className="flex items-center justify-between border-b border-gray-100 pb-2">
            <h2 className="text-sm font-bold text-gray-700">
              الأهداف
              <span className={`ms-2 text-xs font-normal ${Math.abs(totalWeight - 100) > 0.1 && totalWeight > 0 ? 'text-amber-600' : 'text-gray-400'}`}>
                (مجموع الأوزان: {totalWeight.toFixed(0)}%)
              </span>
            </h2>
            <button type="button" onClick={addGoal}
              className="text-xs text-blue-600 hover:underline flex items-center gap-1">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
              </svg>
              إضافة هدف
            </button>
          </div>

          {form.goals.map((g, idx) => (
            <div key={idx} className="border border-gray-100 rounded-xl p-4 space-y-3 bg-gray-50">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-gray-500">هدف {idx + 1}</span>
                {form.goals.length > 1 && (
                  <button type="button" onClick={() => removeGoal(idx)}
                    className="text-xs text-red-400 hover:text-red-600">حذف</button>
                )}
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                <div className="md:col-span-2">
                  <label className={labelCls}>عنوان الهدف <span className="text-red-500">*</span></label>
                  <input name="titleAr" value={g.titleAr} onChange={e => handleGoalChange(idx, e)}
                    required className={inputCls} />
                </div>
                <div>
                  <label className={labelCls}>الوزن (%)</label>
                  <input name="weight" type="number" value={g.weight} onChange={e => handleGoalChange(idx, e)}
                    min="0" max="100" step="5" className={inputCls} />
                </div>
                <div>
                  <label className={labelCls}>حالة الهدف</label>
                  <select name="status" value={g.status} onChange={e => handleGoalChange(idx, e)} className={inputCls}>
                    {GOAL_STATUS_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className={labelCls}>القيمة المستهدفة</label>
                  <input name="targetValue" type="number" value={g.targetValue} onChange={e => handleGoalChange(idx, e)}
                    step="any" className={inputCls} />
                </div>
                <div>
                  <label className={labelCls}>القيمة الفعلية</label>
                  <input name="actualValue" type="number" value={g.actualValue} onChange={e => handleGoalChange(idx, e)}
                    step="any" className={inputCls} />
                </div>
                <div>
                  <label className={labelCls}>تقييم الهدف (1–5)</label>
                  <input name="rating" type="number" value={g.rating} onChange={e => handleGoalChange(idx, e)}
                    min="1" max="5" step="0.1" className={inputCls} />
                </div>
                <div>
                  <label className={labelCls}>ملاحظات الهدف</label>
                  <input name="descriptionAr" value={g.descriptionAr} onChange={e => handleGoalChange(idx, e)}
                    className={inputCls} />
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Comments */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">التعليقات والملاحظات</h2>
          <div>
            <label className={labelCls}>نقاط القوة</label>
            <textarea name="strengthsAr" value={form.strengthsAr} onChange={handleChange}
              rows={3} className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>مجالات التطوير</label>
            <textarea name="areasForImprovementAr" value={form.areasForImprovementAr} onChange={handleChange}
              rows={3} className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>تعليقات المقيِّم</label>
            <textarea name="commentsAr" value={form.commentsAr} onChange={handleChange}
              rows={2} className={inputCls} />
          </div>
          <div>
            <label className={labelCls}>التقييم الذاتي للموظف</label>
            <textarea name="employeeCommentsAr" value={form.employeeCommentsAr} onChange={handleChange}
              rows={2} className={inputCls} />
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3">
          <button type="submit" disabled={saving}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : isEdit ? 'حفظ التعديلات' : 'إنشاء التقييم'}
          </button>
          <button type="button" onClick={() => navigate('/performance')}
            className="px-6 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors">
            إلغاء
          </button>
        </div>
      </form>
    </div>
  )
}
