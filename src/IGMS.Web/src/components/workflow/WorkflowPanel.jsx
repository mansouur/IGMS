import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { workflowApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { Spinner } from '../ui/Spinner'

const DECISION_STYLE = {
  Approved:  'bg-green-100 text-green-700',
  Rejected:  'bg-red-100 text-red-700',
  Commented: 'bg-blue-100 text-blue-700',
}

const DECISION_LABEL = {
  Approved:  'اعتمد',
  Rejected:  'رفض',
  Commented: 'علّق',
}

const STATUS_STYLE = {
  Pending:  'bg-yellow-100 text-yellow-700',
  Approved: 'bg-green-100 text-green-700',
  Rejected: 'bg-red-100 text-red-700',
}

const STATUS_LABEL = {
  Pending:  'قيد الاعتماد',
  Approved: 'معتمد',
  Rejected: 'مرفوض',
}

/**
 * Embeddable workflow panel for detail pages.
 * Props:
 *   entityType: 'Policy' | 'Risk' | 'ControlTest'
 *   entityId:   number
 *   canSubmit:  boolean — whether current user can submit for approval
 */
export default function WorkflowPanel({ entityType, entityId, canSubmit = false }) {
  const { t } = useTranslation()

  const { loading: loadingInst, execute: loadEx } = useApi()
  const { loading: submitting,  execute: submitEx } = useApi()
  const { loading: acting,      execute: actEx } = useApi()

  const [instance, setInstance] = useState(undefined) // undefined = not loaded yet, null = none
  const [comment, setComment]   = useState('')
  const [showComment, setShowComment] = useState(false)

  const load = async () => {
    const r = await loadEx(() => workflowApi.getInstance(entityType, entityId), { silent: true })
    setInstance(r ?? null)
  }

  useEffect(() => {
    if (entityId) load()
  }, [entityType, entityId])

  const handleSubmit = async () => {
    const r = await submitEx(
      () => workflowApi.submit({ entityType, entityId }),
      { successMsg: 'تم إرسال الطلب للاعتماد' }
    )
    if (r !== null) load()
  }

  const handleAct = async (decision) => {
    if (!instance) return
    const r = await actEx(
      () => workflowApi.act(instance.id, { decision, comment: comment || undefined }),
      { successMsg: decision === 'Approved' ? 'تم الاعتماد' : decision === 'Rejected' ? 'تم الرفض' : 'تم إرسال التعليق' }
    )
    setComment('')
    setShowComment(false)
    if (r !== null) load()
  }

  // Loading state
  if (instance === undefined) {
    return (
      <div className="bg-white rounded-xl border border-gray-200 p-5 flex items-center justify-center h-24">
        <Spinner size="sm" />
      </div>
    )
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold text-gray-700 text-sm">دورة الاعتماد</h3>
        {instance && (
          <span className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${STATUS_STYLE[instance.status] ?? 'bg-gray-100 text-gray-600'}`}>
            {STATUS_LABEL[instance.status] ?? instance.status}
          </span>
        )}
      </div>

      {/* No active workflow */}
      {!instance && (
        <div className="text-center py-4">
          <p className="text-sm text-gray-400 mb-3">لم يُرسل هذا العنصر للاعتماد بعد.</p>
          {canSubmit && (
            <button
              onClick={handleSubmit}
              disabled={submitting}
              className="flex items-center gap-2 mx-auto px-4 py-2 bg-green-700 text-white text-sm rounded-lg hover:bg-green-800 disabled:opacity-60"
            >
              {submitting && <Spinner size="sm" />}
              إرسال للاعتماد
            </button>
          )}
        </div>
      )}

      {/* Active workflow */}
      {instance && (
        <>
          {/* Definition + current stage */}
          <div className="text-xs text-gray-500 space-y-1">
            <p>سير العمل: <span className="font-medium text-gray-700">{instance.definitionNameAr}</span></p>
            {instance.status === 'Pending' && instance.currentStageNameAr && (
              <p>المرحلة الحالية: <span className="font-medium text-gray-700">{instance.currentStageNameAr}</span></p>
            )}
            <p>أُرسل بواسطة: <span className="font-medium text-gray-700">{instance.submittedByName}</span></p>
          </div>

          {/* Action history */}
          {instance.actions.length > 0 && (
            <div className="space-y-2 border-t border-gray-100 pt-3">
              <p className="text-xs font-medium text-gray-500">سجل الإجراءات</p>
              {instance.actions.map((a) => (
                <div key={a.id} className="flex items-start gap-2">
                  <span className={`mt-0.5 px-1.5 py-0.5 rounded text-xs flex-shrink-0 ${DECISION_STYLE[a.decision] ?? 'bg-gray-100 text-gray-600'}`}>
                    {DECISION_LABEL[a.decision] ?? a.decision}
                  </span>
                  <div className="min-w-0">
                    <p className="text-xs text-gray-700">
                      {a.actorName} — <span className="text-gray-400">{a.stageNameAr}</span>
                    </p>
                    {a.comment && <p className="text-xs text-gray-500 mt-0.5">"{a.comment}"</p>}
                    <p className="text-xs text-gray-400">{new Date(a.actedAt).toLocaleString('ar-AE')}</p>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Act buttons (if CanAct) */}
          {instance.status === 'Pending' && instance.canAct && (
            <div className="border-t border-gray-100 pt-3 space-y-2">
              <div className="flex gap-2">
                <button
                  onClick={() => setShowComment(!showComment)}
                  className="flex-1 py-1.5 text-xs border border-gray-200 text-gray-600 rounded-lg hover:bg-gray-50"
                >
                  💬 تعليق
                </button>
                <button
                  onClick={() => handleAct('Rejected')}
                  disabled={acting}
                  className="flex-1 py-1.5 text-xs border border-red-200 text-red-600 rounded-lg hover:bg-red-50 disabled:opacity-50"
                >
                  {acting ? <Spinner size="sm" /> : '✕ رفض'}
                </button>
                <button
                  onClick={() => handleAct('Approved')}
                  disabled={acting}
                  className="flex-1 py-1.5 text-xs bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50"
                >
                  {acting ? <Spinner size="sm" /> : '✓ اعتماد'}
                </button>
              </div>
              {showComment && (
                <div className="space-y-2">
                  <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    rows={2}
                    placeholder="أضف تعليقك..."
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
                    dir="rtl"
                  />
                  <button
                    onClick={() => handleAct('Commented')}
                    disabled={!comment.trim() || acting}
                    className="w-full py-1.5 text-xs bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    إرسال التعليق
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Resubmit if rejected */}
          {instance.status === 'Rejected' && canSubmit && (
            <div className="border-t border-gray-100 pt-3">
              <button
                onClick={handleSubmit}
                disabled={submitting}
                className="flex items-center gap-2 px-4 py-1.5 text-xs bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 disabled:opacity-60"
              >
                {submitting && <Spinner size="sm" />}
                إعادة الإرسال للاعتماد
              </button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
