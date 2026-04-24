import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ── Helpers ───────────────────────────────────────────────────────────────────

const STATUS_CFG = {
  Draft:     { badge: 'bg-gray-100 text-gray-600',       label: 'مسودة'  },
  Submitted: { badge: 'bg-blue-100 text-blue-700',       label: 'مرفوع'  },
  Approved:  { badge: 'bg-emerald-100 text-emerald-700', label: 'معتمد'  },
  Rejected:  { badge: 'bg-red-100 text-red-600',         label: 'مرفوض'  },
}

const GOAL_STATUS_CFG = {
  Pending:           { cls: 'bg-gray-100 text-gray-600',    label: 'قيد التنفيذ' },
  Achieved:          { cls: 'bg-emerald-100 text-emerald-700', label: 'محقق'     },
  PartiallyAchieved: { cls: 'bg-amber-100 text-amber-700',  label: 'محقق جزئياً'},
  NotAchieved:       { cls: 'bg-red-100 text-red-600',      label: 'لم يتحقق'   },
}

const PERIOD_LBL = {
  Q1: 'الربع الأول', Q2: 'الربع الثاني', Q3: 'الربع الثالث', Q4: 'الربع الرابع',
  Annual: 'سنوي', Probation: 'فترة تجربة',
}

function StarRating({ value, size = 16 }) {
  if (!value) return <span className="text-sm text-gray-400">—</span>
  const rounded = Math.round(value * 2) / 2
  return (
    <span className="flex items-center gap-1">
      {[1,2,3,4,5].map(i => (
        <svg key={i} width={size} height={size} viewBox="0 0 24 24"
          fill={i <= rounded ? '#f59e0b' : 'none'}
          stroke={i <= rounded ? '#f59e0b' : '#d1d5db'} strokeWidth="1.5">
          <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
        </svg>
      ))}
      <span className="text-sm font-semibold text-gray-700 ms-1">{Number(value).toFixed(1)}</span>
    </span>
  )
}

function InfoRow({ label, value }) {
  return (
    <div>
      <p className="text-xs text-gray-400 mb-0.5">{label}</p>
      <p className="text-sm font-medium text-gray-800">{value || '—'}</p>
    </div>
  )
}

// ── Reject Modal ──────────────────────────────────────────────────────────────

function RejectModal({ onConfirm, onClose }) {
  const [reason, setReason] = useState('')
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <h3 className="text-base font-bold text-gray-800 mb-4">سبب الرفض</h3>
        <textarea value={reason} onChange={e => setReason(e.target.value)}
          rows={4} placeholder="اذكر سبب رفض التقييم..."
          className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 mb-4" />
        <div className="flex gap-3">
          <button onClick={() => onConfirm(reason)}
            className="flex-1 py-2 bg-red-600 text-white rounded-xl text-sm font-medium hover:bg-red-700">
            رفض التقييم
          </button>
          <button onClick={onClose}
            className="flex-1 py-2 bg-gray-100 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-200">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Main ──────────────────────────────────────────────────────────────────────

export default function PerformanceDetail() {
  const { id }        = useParams()
  const navigate      = useNavigate()
  const hasPermission = useAuthStore(s => s.hasPermission)
  const canApprove    = hasPermission('PERFORMANCE.APPROVE')

  const [review,    setReview]    = useState(null)
  const [loading,   setLoading]   = useState(true)
  const [acting,    setActing]    = useState(false)
  const [error,     setError]     = useState(null)
  const [showReject,setShowReject] = useState(false)

  useEffect(() => {
    api.get(`/api/v1/performance/${id}`)
      .then(r => setReview(r.data.data))
      .catch(() => setError('تعذّر تحميل بيانات التقييم.'))
      .finally(() => setLoading(false))
  }, [id])

  async function doAction(endpoint, body = undefined) {
    setActing(true); setError(null)
    try {
      const r = await api.post(`/api/v1/performance/${id}/${endpoint}`, body ?? {})
      setReview(r.data.data)
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'حدث خطأ.')
    } finally { setActing(false) }
  }

  if (loading) return <PageLoader />
  if (!review) return <div className="p-8 text-center text-red-500">{error || 'التقييم غير موجود.'}</div>

  const sCfg    = STATUS_CFG[review.status] ?? STATUS_CFG.Draft
  const isDraft = review.status === 'Draft'
  const isSub   = review.status === 'Submitted'

  // Calculate weighted score from goals
  const goalsWithRating = review.goals.filter(g => g.rating != null && g.weight > 0)
  const weightedScore = goalsWithRating.length
    ? goalsWithRating.reduce((s, g) => s + g.rating * g.weight, 0) /
      goalsWithRating.reduce((s, g) => s + g.weight, 0)
    : null

  return (
    <div className="max-w-4xl space-y-5">
      {showReject && (
        <RejectModal
          onConfirm={async reason => { setShowReject(false); await doAction('reject', { reason }) }}
          onClose={() => setShowReject(false)}
        />
      )}

      {/* Header */}
      <div>
        <button onClick={() => navigate('/performance')}
          className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
          تقييمات الأداء
        </button>

        <div className="flex items-start justify-between gap-4 flex-wrap">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-xl font-bold text-gray-800">{review.employeeName}</h1>
              <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${sCfg.badge}`}>{sCfg.label}</span>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              {PERIOD_LBL[review.period] ?? review.period} {review.year}
              {review.departmentName && ` · ${review.departmentName}`}
            </p>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2 flex-wrap">
            {isDraft && (
              <>
                <button onClick={() => navigate(`/performance/${id}/edit`)}
                  className="px-4 py-2 bg-white border border-gray-200 text-gray-700 rounded-xl text-sm font-medium hover:bg-gray-50">
                  تعديل
                </button>
                <button onClick={() => doAction('submit')} disabled={acting}
                  className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                  رفع للاعتماد
                </button>
              </>
            )}
            {isSub && canApprove && (
              <>
                <button onClick={() => setShowReject(true)} disabled={acting}
                  className="px-4 py-2 bg-white border border-red-200 text-red-600 rounded-xl text-sm font-medium hover:bg-red-50 disabled:opacity-50">
                  رفض
                </button>
                <button onClick={() => doAction('approve')} disabled={acting}
                  className="px-4 py-2 bg-emerald-600 text-white rounded-xl text-sm font-medium hover:bg-emerald-700 disabled:opacity-50">
                  اعتماد
                </button>
              </>
            )}
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 text-sm px-4 py-3 rounded-xl">{error}</div>
      )}

      {/* Overview cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          { label: 'التقييم الإجمالي', val: <StarRating value={review.overallRating} /> },
          { label: 'التقييم المحسوب', val: <StarRating value={weightedScore ? +weightedScore.toFixed(2) : null} /> },
          { label: 'عدد الأهداف', val: <span className="text-2xl font-black text-gray-800">{review.goals.length}</span> },
          { label: 'المقيِّم', val: <span className="text-sm font-medium text-gray-800">{review.reviewerName}</span> },
        ].map((c, i) => (
          <div key={i} className="bg-white rounded-xl border border-gray-200 p-4 space-y-1">
            <p className="text-xs text-gray-400">{c.label}</p>
            {c.val}
          </div>
        ))}
      </div>

      {/* Info */}
      <div className="bg-white rounded-xl border border-gray-200 p-5 grid grid-cols-2 md:grid-cols-3 gap-5">
        <InfoRow label="الموظف"   value={review.employeeName} />
        <InfoRow label="المقيِّم"  value={review.reviewerName} />
        <InfoRow label="القسم"    value={review.departmentName} />
        <InfoRow label="الفترة"   value={`${PERIOD_LBL[review.period] ?? review.period} ${review.year}`} />
        <InfoRow label="تاريخ الرفع"    value={review.submittedAt ? new Date(review.submittedAt).toLocaleDateString('ar-AE') : null} />
        <InfoRow label="تاريخ الاعتماد" value={review.approvedAt ? new Date(review.approvedAt).toLocaleDateString('ar-AE') : null} />
      </div>

      {/* Reject reason */}
      {review.status === 'Rejected' && review.rejectReason && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4">
          <p className="text-xs text-red-400 mb-1">سبب الرفض</p>
          <p className="text-sm text-red-700">{review.rejectReason}</p>
        </div>
      )}

      {/* Goals */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2 mb-4">الأهداف</h2>
        {review.goals.length === 0 ? (
          <p className="text-xs text-gray-400">لا توجد أهداف مسجّلة</p>
        ) : (
          <div className="space-y-3">
            {review.goals.map((g, i) => {
              const gCfg = GOAL_STATUS_CFG[g.status] ?? GOAL_STATUS_CFG.Pending
              const pct = g.targetValue && g.actualValue
                ? Math.min(100, Math.round((g.actualValue / g.targetValue) * 100))
                : null
              return (
                <div key={g.id ?? i} className="border border-gray-100 rounded-xl p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-sm font-semibold text-gray-800">{g.titleAr}</span>
                        <span className={`text-xs px-2 py-0.5 rounded-full ${gCfg.cls}`}>{gCfg.label}</span>
                      </div>
                      {g.descriptionAr && <p className="text-xs text-gray-500 mt-0.5">{g.descriptionAr}</p>}
                    </div>
                    <div className="text-end flex-shrink-0">
                      <p className="text-xs text-gray-400">الوزن</p>
                      <p className="text-sm font-bold text-gray-700">{g.weight}%</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-3 gap-4 mt-3 pt-3 border-t border-gray-50">
                    <div>
                      <p className="text-xs text-gray-400">المستهدف</p>
                      <p className="text-sm font-medium text-gray-700">{g.targetValue ?? '—'}</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-400">الفعلي</p>
                      <p className="text-sm font-medium text-gray-700">{g.actualValue ?? '—'}</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-400">تقييم الهدف</p>
                      <StarRating value={g.rating} size={13} />
                    </div>
                  </div>

                  {pct !== null && (
                    <div className="mt-3">
                      <div className="flex justify-between text-xs text-gray-400 mb-1">
                        <span>نسبة الإنجاز</span><span>{pct}%</span>
                      </div>
                      <div className="h-1.5 bg-gray-100 rounded-full">
                        <div className={`h-1.5 rounded-full transition-all ${pct >= 100 ? 'bg-emerald-500' : pct >= 70 ? 'bg-blue-500' : 'bg-amber-400'}`}
                          style={{ width: `${pct}%` }} />
                      </div>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Comments */}
      {(review.strengthsAr || review.areasForImprovementAr || review.commentsAr || review.employeeCommentsAr) && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
          <h2 className="text-sm font-bold text-gray-700 border-b border-gray-100 pb-2">الملاحظات والتعليقات</h2>
          {review.strengthsAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">نقاط القوة</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{review.strengthsAr}</p>
            </div>
          )}
          {review.areasForImprovementAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">مجالات التطوير</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{review.areasForImprovementAr}</p>
            </div>
          )}
          {review.commentsAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">تعليقات المقيِّم</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{review.commentsAr}</p>
            </div>
          )}
          {review.employeeCommentsAr && (
            <div>
              <p className="text-xs text-gray-400 mb-1">التقييم الذاتي للموظف</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">{review.employeeCommentsAr}</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
