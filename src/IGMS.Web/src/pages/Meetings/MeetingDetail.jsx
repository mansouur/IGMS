import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { meetingsApi } from '../../services/api'
import api from '../../services/api'
import { useConfirm } from '../../hooks/useApi'
import useAuthStore from '../../store/authStore'
import { PageLoader } from '../../components/ui/Spinner'

// ── Helpers ───────────────────────────────────────────────────────────────────

const STATUS_CFG = {
  Scheduled:  { badge: 'bg-blue-100 text-blue-700',       label: 'مجدول',  bar: 'bg-blue-500'    },
  InProgress: { badge: 'bg-amber-100 text-amber-700',     label: 'جارٍ',    bar: 'bg-amber-400'   },
  Completed:  { badge: 'bg-emerald-100 text-emerald-700', label: 'مكتمل',  bar: 'bg-emerald-500' },
  Cancelled:  { badge: 'bg-gray-100 text-gray-500',       label: 'ملغى',   bar: 'bg-gray-400'    },
}
const TYPE_LBL = {
  Board: 'مجلس الإدارة', Committee: 'لجنة', Department: 'قسم', Emergency: 'طارئ', Review: 'مراجعة',
}

// ── Minutes Modal ─────────────────────────────────────────────────────────────

function MinutesModal({ meeting, onSaved, onClose }) {
  const [minutes,     setMinutes]     = useState(meeting.minutesAr ?? '')
  const [presentIds,  setPresentIds]  = useState(meeting.attendees.filter(a => a.isPresent).map(a => a.userId))
  const [actionItems, setActionItems] = useState([{ titleAr: '', descriptionAr: '', assignedToId: '', dueDate: '' }])
  const [users,       setUsers]       = useState([])
  const [saving,      setSaving]      = useState(false)

  useEffect(() => {
    api.get('/api/v1/users').then(r => setUsers(r.data?.data?.items ?? r.data?.data ?? [])).catch(() => {})
  }, [])

  function togglePresent(uid) {
    setPresentIds(ids => ids.includes(uid) ? ids.filter(i => i !== uid) : [...ids, uid])
  }
  function addAction() {
    setActionItems(a => [...a, { titleAr: '', descriptionAr: '', assignedToId: '', dueDate: '' }])
  }
  function removeAction(i) {
    setActionItems(a => a.filter((_, j) => j !== i))
  }
  function updateAction(i, field, value) {
    setActionItems(a => a.map((item, j) => j === i ? { ...item, [field]: value } : item))
  }

  async function save() {
    setSaving(true)
    try {
      const dto = await meetingsApi.complete(meeting.id, {
        minutesAr:   minutes || null,
        presentIds,
        actionItems: actionItems
          .filter(a => a.titleAr.trim())
          .map(a => ({
            titleAr:       a.titleAr,
            descriptionAr: a.descriptionAr || null,
            assignedToId:  a.assignedToId ? Number(a.assignedToId) : null,
            dueDate:       a.dueDate || null,
          })),
      })
      onSaved(dto.data.data)
      onClose()
    } catch { /* silent */ }
    finally { setSaving(false) }
  }

  const inputCls = 'w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-bold text-gray-800">تسجيل محضر الاجتماع</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-4 space-y-5">

          {/* Minutes text */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">محضر الاجتماع</label>
            <textarea value={minutes} onChange={e => setMinutes(e.target.value)}
              rows={5} placeholder="أدخل ملخص الاجتماع والقرارات المتخذة..."
              className={inputCls} />
          </div>

          {/* Attendance */}
          {meeting.attendees.length > 0 && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                تسجيل الحضور ({presentIds.length}/{meeting.attendees.length})
              </label>
              <div className="grid grid-cols-2 gap-2">
                {meeting.attendees.map(a => (
                  <label key={a.userId}
                    className={`flex items-center gap-2 p-2 rounded-lg cursor-pointer border ${
                      presentIds.includes(a.userId) ? 'border-emerald-300 bg-emerald-50' : 'border-gray-100'
                    }`}>
                    <input type="checkbox"
                      checked={presentIds.includes(a.userId)}
                      onChange={() => togglePresent(a.userId)}
                      className="w-3.5 h-3.5 text-emerald-600" />
                    <span className="text-xs text-gray-700">{a.fullNameAr}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          {/* Action items */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium text-gray-700">نقاط العمل</label>
              <button type="button" onClick={addAction}
                className="text-xs text-blue-600 hover:underline">+ إضافة نقطة</button>
            </div>
            <div className="space-y-3">
              {actionItems.map((item, i) => (
                <div key={i} className="border border-gray-100 rounded-xl p-3 space-y-2 relative">
                  <button type="button" onClick={() => removeAction(i)}
                    className="absolute top-2 end-2 text-gray-300 hover:text-red-500">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                  </button>
                  <input value={item.titleAr}
                    onChange={e => updateAction(i, 'titleAr', e.target.value)}
                    placeholder="عنوان نقطة العمل *" className={inputCls} />
                  <div className="grid grid-cols-2 gap-2">
                    <select value={item.assignedToId}
                      onChange={e => updateAction(i, 'assignedToId', e.target.value)}
                      className={inputCls}>
                      <option value="">— المسؤول —</option>
                      {users.map(u => <option key={u.id} value={u.id}>{u.fullNameAr || u.username}</option>)}
                    </select>
                    <input type="date" value={item.dueDate}
                      onChange={e => updateAction(i, 'dueDate', e.target.value)}
                      className={inputCls} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="px-6 py-4 border-t border-gray-100 flex gap-3">
          <button onClick={save} disabled={saving}
            className="flex-1 py-2.5 bg-emerald-600 text-white rounded-xl text-sm font-medium hover:bg-emerald-700 disabled:opacity-50">
            {saving ? 'جاري الحفظ...' : 'إنهاء الاجتماع وحفظ المحضر'}
          </button>
          <button onClick={onClose}
            className="px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Main ──────────────────────────────────────────────────────────────────────

export default function MeetingDetail() {
  const { id }        = useParams()
  const navigate      = useNavigate()
  const confirm       = useConfirm()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canManage     = hasPermission('MEETINGS.MANAGE')
  const canDelete     = hasPermission('MEETINGS.DELETE')

  const [meeting,       setMeeting]       = useState(null)
  const [loading,       setLoading]       = useState(true)
  const [showMinutes,   setShowMinutes]   = useState(false)

  useEffect(() => {
    meetingsApi.getById(id)
      .then(r => setMeeting(r.data.data))
      .catch(() => navigate('/meetings'))
      .finally(() => setLoading(false))
  }, [id])

  async function handleStart() {
    const ok = await confirm({
      title: 'بدء الاجتماع',
      message: 'هل تريد بدء هذا الاجتماع الآن؟',
      variant: 'info',
    })
    if (!ok) return
    const r = await meetingsApi.start(id)
    setMeeting(r.data.data)
  }

  async function handleCancel() {
    const ok = await confirm({
      title: 'إلغاء الاجتماع',
      message: 'هل تريد إلغاء هذا الاجتماع؟',
      variant: 'warning',
    })
    if (!ok) return
    const r = await meetingsApi.cancel(id)
    setMeeting(r.data.data)
  }

  async function handleDelete() {
    const ok = await confirm({
      title: 'حذف الاجتماع',
      message: `هل تريد حذف "${meeting.titleAr}"؟`,
      variant: 'danger',
    })
    if (!ok) return
    await meetingsApi.delete(id)
    navigate('/meetings')
  }

  async function handleCompleteAction(actionId) {
    const r = await meetingsApi.completeAction(id, actionId)
    setMeeting(m => ({
      ...m,
      actionItems: m.actionItems.map(a => a.id === actionId ? r.data.data : a),
    }))
  }

  if (loading)  return <PageLoader />
  if (!meeting) return null

  const sCfg = STATUS_CFG[meeting.status] ?? STATUS_CFG.Scheduled

  return (
    <div className="space-y-5 max-w-4xl">

      {/* Header */}
      <div className="flex items-start justify-between flex-wrap gap-3">
        <div>
          <button onClick={() => navigate('/meetings')}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="15 18 9 12 15 6"/></svg>
            الاجتماعات
          </button>
          <h1 className="text-xl font-bold text-gray-800">{meeting.titleAr}</h1>
          {meeting.titleEn && <p className="text-sm text-gray-400">{meeting.titleEn}</p>}
          <div className="flex items-center gap-2 mt-2 flex-wrap">
            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${sCfg.badge}`}>{sCfg.label}</span>
            <span className="text-xs text-gray-400">{TYPE_LBL[meeting.type] ?? meeting.type}</span>
            {meeting.departmentName && <span className="text-xs text-gray-400">· {meeting.departmentName}</span>}
          </div>
        </div>

        {/* Action buttons based on status */}
        <div className="flex gap-2 flex-wrap">
          {meeting.status === 'Scheduled' && canManage && (
            <>
              <button onClick={() => navigate(`/meetings/${id}/edit`)}
                className="px-3 py-2 bg-gray-100 text-gray-700 text-sm rounded-xl hover:bg-gray-200 transition-colors">
                تعديل
              </button>
              <button onClick={handleStart}
                className="px-4 py-2 bg-blue-600 text-white text-sm rounded-xl hover:bg-blue-700 transition-colors">
                بدء الاجتماع
              </button>
              <button onClick={handleCancel}
                className="px-3 py-2 bg-orange-50 text-orange-600 border border-orange-200 text-sm rounded-xl hover:bg-orange-100 transition-colors">
                إلغاء
              </button>
            </>
          )}
          {meeting.status === 'InProgress' && canManage && (
            <button onClick={() => setShowMinutes(true)}
              className="px-4 py-2 bg-emerald-600 text-white text-sm rounded-xl hover:bg-emerald-700 transition-colors">
              إنهاء وتسجيل المحضر
            </button>
          )}
          {canDelete && meeting.status !== 'Completed' && (
            <button onClick={handleDelete}
              className="px-3 py-2 bg-red-50 text-red-600 border border-red-200 text-sm rounded-xl hover:bg-red-100 transition-colors">
              حذف
            </button>
          )}
        </div>
      </div>

      {/* Info cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <p className="text-xs text-gray-400 mb-1">الموعد المجدول</p>
          <p className="text-sm font-semibold text-gray-800">
            {new Date(meeting.scheduledAt).toLocaleDateString('ar-AE', {
              weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
            })}
          </p>
          <p className="text-xs text-gray-500 mt-0.5">
            {new Date(meeting.scheduledAt).toLocaleTimeString('ar-AE', { hour: '2-digit', minute: '2-digit' })}
          </p>
          {meeting.location && <p className="text-xs text-blue-600 mt-1">{meeting.location}</p>}
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <p className="text-xs text-gray-400 mb-1">المنظّم</p>
          <p className="text-sm font-semibold text-gray-800">{meeting.organizerName ?? '—'}</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <p className="text-xs text-gray-400 mb-2">نقاط العمل</p>
          <div className="flex items-center gap-3">
            <span className="text-2xl font-black text-gray-800">{meeting.actionItemCount}</span>
            {meeting.pendingActions > 0 && (
              <span className="text-xs bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full">
                {meeting.pendingActions} معلّقة
              </span>
            )}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">

        {/* Agenda */}
        {meeting.agendaAr && (
          <div className="bg-white rounded-xl border border-gray-200 p-5">
            <h2 className="text-sm font-bold text-gray-700 mb-3">جدول الأعمال</h2>
            <p className="text-sm text-gray-600 whitespace-pre-line">{meeting.agendaAr}</p>
          </div>
        )}

        {/* Attendees */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-bold text-gray-700 mb-3">
            الحضور ({meeting.attendees.length})
          </h2>
          {meeting.attendees.length === 0 ? (
            <p className="text-xs text-gray-400">لا يوجد مدعوون مسجّلون</p>
          ) : (
            <div className="space-y-2">
              {meeting.attendees.map(a => (
                <div key={a.userId} className="flex items-center gap-2">
                  <div className={`w-2 h-2 rounded-full ${a.isPresent ? 'bg-emerald-500' : 'bg-gray-300'}`} />
                  <span className="text-sm text-gray-700">{a.fullNameAr}</span>
                  {a.roleInMeeting && <span className="text-xs text-gray-400">({a.roleInMeeting})</span>}
                  {meeting.status === 'Completed' && (
                    <span className={`ms-auto text-xs ${a.isPresent ? 'text-emerald-600' : 'text-gray-400'}`}>
                      {a.isPresent ? 'حضر' : 'غائب'}
                    </span>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Minutes */}
        {meeting.minutesAr && (
          <div className="bg-white rounded-xl border border-gray-200 p-5 lg:col-span-2">
            <h2 className="text-sm font-bold text-gray-700 mb-3">محضر الاجتماع</h2>
            <p className="text-sm text-gray-600 whitespace-pre-line">{meeting.minutesAr}</p>
          </div>
        )}

        {/* Action Items */}
        {meeting.actionItems.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 p-5 lg:col-span-2">
            <h2 className="text-sm font-bold text-gray-700 mb-3">
              نقاط العمل ({meeting.actionItems.length})
            </h2>
            <div className="space-y-2">
              {meeting.actionItems.map(a => (
                <div key={a.id}
                  className={`flex items-start gap-3 p-3 rounded-xl border ${
                    a.isCompleted ? 'bg-emerald-50 border-emerald-100' : 'bg-white border-gray-100'
                  }`}>
                  <button
                    disabled={a.isCompleted}
                    onClick={() => handleCompleteAction(a.id)}
                    className={`w-5 h-5 rounded-full border-2 flex-shrink-0 mt-0.5 transition-colors ${
                      a.isCompleted
                        ? 'bg-emerald-500 border-emerald-500'
                        : 'border-gray-300 hover:border-emerald-400'
                    }`}>
                    {a.isCompleted && (
                      <svg viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3">
                        <polyline points="20 6 9 17 4 12"/>
                      </svg>
                    )}
                  </button>
                  <div className="flex-1 min-w-0">
                    <p className={`text-sm font-medium ${a.isCompleted ? 'line-through text-gray-400' : 'text-gray-800'}`}>
                      {a.titleAr}
                    </p>
                    {a.descriptionAr && <p className="text-xs text-gray-500 mt-0.5">{a.descriptionAr}</p>}
                    <div className="flex items-center gap-3 mt-1 text-xs text-gray-400">
                      {a.assigneeName && <span>{a.assigneeName}</span>}
                      {a.dueDate && (
                        <span className={new Date(a.dueDate) < Date.now() && !a.isCompleted ? 'text-red-500' : ''}>
                          {new Date(a.dueDate).toLocaleDateString('ar-AE')}
                        </span>
                      )}
                      {a.isCompleted && a.completedAt && (
                        <span className="text-emerald-600">
                          أُنجز {new Date(a.completedAt).toLocaleDateString('ar-AE')}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Minutes modal */}
      {showMinutes && (
        <MinutesModal
          meeting={meeting}
          onSaved={setMeeting}
          onClose={() => setShowMinutes(false)}
        />
      )}
    </div>
  )
}
