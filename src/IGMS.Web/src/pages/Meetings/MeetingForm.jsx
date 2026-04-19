import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { meetingsApi } from '../../services/api'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'

const TYPE_OPTS = [
  { value: 'Board',      label: 'مجلس الإدارة' },
  { value: 'Committee',  label: 'لجنة'          },
  { value: 'Department', label: 'قسم'           },
  { value: 'Emergency',  label: 'طارئ'          },
  { value: 'Review',     label: 'مراجعة'        },
]

const EMPTY = {
  titleAr: '', titleEn: '', type: 'Committee',
  scheduledAt: '', location: '', agendaAr: '', notesAr: '',
  departmentId: '', attendeeIds: [],
}

export default function MeetingForm() {
  const { id }   = useParams()
  const navigate = useNavigate()
  const isEdit   = Boolean(id)

  const [form,    setForm]    = useState(EMPTY)
  const [depts,   setDepts]   = useState([])
  const [users,   setUsers]   = useState([])
  const [saving,  setSaving]  = useState(false)
  const [loading, setLoading] = useState(isEdit)
  const [error,   setError]   = useState(null)

  useEffect(() => {
    api.get('/api/v1/departments').then(r => setDepts(r.data?.data ?? [])).catch(() => {})
    api.get('/api/v1/users').then(r => setUsers(r.data?.data?.items ?? r.data?.data ?? [])).catch(() => {})
    if (!isEdit) return
    meetingsApi.getById(id).then(r => {
      const m = r.data.data
      setForm({
        titleAr:      m.titleAr      ?? '',
        titleEn:      m.titleEn      ?? '',
        type:         m.type         ?? 'Committee',
        scheduledAt:  m.scheduledAt  ? new Date(m.scheduledAt).toISOString().slice(0, 16) : '',
        location:     m.location     ?? '',
        agendaAr:     m.agendaAr     ?? '',
        notesAr:      m.notesAr      ?? '',
        departmentId: m.departmentId ?? '',
        attendeeIds:  m.attendees?.map(a => a.userId) ?? [],
      })
    }).catch(() => setError('تعذّر تحميل بيانات الاجتماع.'))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  function handleChange(e) {
    const { name, value } = e.target
    setForm(f => ({ ...f, [name]: value }))
  }

  function toggleAttendee(userId) {
    setForm(f => ({
      ...f,
      attendeeIds: f.attendeeIds.includes(userId)
        ? f.attendeeIds.filter(id => id !== userId)
        : [...f.attendeeIds, userId],
    }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (!form.titleAr.trim()) { setError('عنوان الاجتماع مطلوب.'); return }
    if (!form.scheduledAt)    { setError('تاريخ الاجتماع مطلوب.'); return }
    setSaving(true); setError(null)
    const payload = {
      ...form,
      scheduledAt:  new Date(form.scheduledAt).toISOString(),
      departmentId: form.departmentId ? Number(form.departmentId) : null,
    }
    try {
      if (isEdit) await meetingsApi.update(id, payload)
      else        await meetingsApi.create(payload)
      navigate('/meetings')
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'حدث خطأ أثناء الحفظ.')
    } finally { setSaving(false) }
  }

  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
  const labelCls = 'block text-sm font-medium text-gray-700 mb-1'

  if (loading) return <PageLoader />

  return (
    <div className="max-w-3xl">
      <div className="mb-6">
        <button onClick={() => navigate('/meetings')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
          الاجتماعات
        </button>
        <h1 className="text-xl font-bold text-gray-800">{isEdit ? 'تعديل الاجتماع' : 'جدولة اجتماع جديد'}</h1>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-xl">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-5">

        {/* Basic info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">معلومات الاجتماع</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className={labelCls}>عنوان الاجتماع (عربي) <span className="text-red-500">*</span></label>
              <input name="titleAr" value={form.titleAr} onChange={handleChange} className={inputCls} required />
            </div>
            <div>
              <label className={labelCls}>عنوان الاجتماع (إنجليزي)</label>
              <input name="titleEn" value={form.titleEn} onChange={handleChange} className={inputCls} dir="ltr" />
            </div>
            <div>
              <label className={labelCls}>نوع الاجتماع</label>
              <select name="type" value={form.type} onChange={handleChange} className={inputCls}>
                {TYPE_OPTS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className={labelCls}>التاريخ والوقت <span className="text-red-500">*</span></label>
              <input name="scheduledAt" type="datetime-local" value={form.scheduledAt}
                onChange={handleChange} className={inputCls} required />
            </div>
            <div>
              <label className={labelCls}>الموقع / رابط الاجتماع</label>
              <input name="location" value={form.location} onChange={handleChange}
                placeholder="قاعة الاجتماعات أو رابط Zoom/Teams" className={inputCls} />
            </div>
            <div>
              <label className={labelCls}>القسم</label>
              <select name="departmentId" value={form.departmentId} onChange={handleChange} className={inputCls}>
                <option value="">— لا يوجد —</option>
                {depts.map(d => <option key={d.id} value={d.id}>{d.nameAr}</option>)}
              </select>
            </div>
          </div>
        </div>

        {/* Agenda */}
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-3">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">جدول الأعمال</h2>
          <textarea name="agendaAr" value={form.agendaAr} onChange={handleChange}
            rows={4} placeholder="أدخل بنود جدول الأعمال..." className={inputCls} />
          <div>
            <label className={labelCls}>ملاحظات إضافية</label>
            <textarea name="notesAr" value={form.notesAr} onChange={handleChange}
              rows={2} className={inputCls} />
          </div>
        </div>

        {/* Attendees */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2 mb-3">
            المدعوون
            {form.attendeeIds.length > 0 && (
              <span className="ms-2 text-xs text-blue-600 font-normal">({form.attendeeIds.length} محدد)</span>
            )}
          </h2>
          {users.length === 0 ? (
            <p className="text-xs text-gray-400">لا يوجد مستخدمون</p>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2 max-h-60 overflow-y-auto">
              {users.map(u => (
                <label key={u.id}
                  className={`flex items-center gap-2 p-2 rounded-lg cursor-pointer border transition-colors ${
                    form.attendeeIds.includes(u.id)
                      ? 'border-blue-300 bg-blue-50'
                      : 'border-gray-100 hover:border-gray-200 hover:bg-gray-50'
                  }`}>
                  <input type="checkbox"
                    checked={form.attendeeIds.includes(u.id)}
                    onChange={() => toggleAttendee(u.id)}
                    className="w-3.5 h-3.5 text-blue-600" />
                  <span className="text-xs text-gray-700 truncate">{u.fullNameAr || u.username}</span>
                </label>
              ))}
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="flex gap-3">
          <button type="submit" disabled={saving}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 transition-colors disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : isEdit ? 'حفظ التعديلات' : 'جدولة الاجتماع'}
          </button>
          <button type="button" onClick={() => navigate('/meetings')}
            className="px-6 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200 transition-colors">
            إلغاء
          </button>
        </div>
      </form>
    </div>
  )
}
