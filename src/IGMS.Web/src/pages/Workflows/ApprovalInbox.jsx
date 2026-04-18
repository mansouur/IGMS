import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { workflowApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader, Spinner } from '../../components/ui/Spinner'

const ENTITY_PATH = {
  Policy:      (id) => `/policies/${id}`,
  Risk:        (id) => `/risks/${id}`,
  ControlTest: (id) => `/controls/${id}`,
}

const ENTITY_COLOR = {
  Policy:      'bg-blue-100 text-blue-700',
  Risk:        'bg-red-100 text-red-700',
  ControlTest: 'bg-purple-100 text-purple-700',
}

const ENTITY_LABEL = {
  Policy:      'سياسة',
  Risk:        'مخاطرة',
  ControlTest: 'ضابط',
}

export default function ApprovalInbox() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const { loading, execute } = useApi()
  const { loading: acting, execute: actEx } = useApi()

  const [pending, setPending] = useState([])
  const [actingId, setActingId]   = useState(null)
  const [comment, setComment]     = useState('')
  const [showComment, setShowComment] = useState(null)  // instanceId showing comment box

  const load = () =>
    execute(() => workflowApi.getPending(), { silent: true }).then(
      (r) => r && setPending(r)
    )

  useEffect(() => { load() }, [])

  const handleAct = async (instanceId, decision) => {
    setActingId(instanceId)
    const ok = await actEx(
      () => workflowApi.act(instanceId, { decision, comment: comment || undefined }),
      { successMsg: decision === 'Approved' ? t('workflows.messages.approved') : t('workflows.messages.rejected') }
    )
    setActingId(null)
    setComment('')
    setShowComment(null)
    if (ok !== null) load()
  }

  if (loading && !pending.length) return <PageLoader />

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-bold text-gray-800">{t('workflows.inbox.title')}</h1>
        <p className="text-sm text-gray-500 mt-0.5">{t('workflows.inbox.subtitle')}</p>
      </div>

      {pending.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 py-20 text-center">
          <p className="text-4xl mb-3">✅</p>
          <p className="text-gray-500 text-sm">{t('workflows.inbox.empty')}</p>
        </div>
      ) : (
        <div className="space-y-4">
          {pending.map((item) => (
            <div key={item.instanceId} className="bg-white rounded-xl border border-gray-200 p-5">
              <div className="flex items-start gap-4">
                {/* Entity type badge */}
                <span className={`mt-0.5 px-2.5 py-0.5 rounded-full text-xs font-medium flex-shrink-0 ${ENTITY_COLOR[item.entityType] ?? 'bg-gray-100 text-gray-600'}`}>
                  {ENTITY_LABEL[item.entityType] ?? item.entityType}
                </span>

                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    {/* Entity title — clickable link */}
                    <button
                      onClick={() => navigate(ENTITY_PATH[item.entityType]?.(item.entityId) ?? '#')}
                      className="font-semibold text-gray-800 hover:text-green-700 text-sm text-start"
                    >
                      {item.entityTitle}
                    </button>
                    <span className="text-gray-300">•</span>
                    <span className="text-xs text-gray-500">{item.workflowName}</span>
                  </div>

                  <div className="mt-1 flex flex-wrap gap-3 text-xs text-gray-500">
                    <span>{t('workflows.inbox.stage')}: <strong className="text-gray-700">{item.currentStage}</strong></span>
                    <span>{t('workflows.inbox.submittedBy')}: <strong className="text-gray-700">{item.submittedBy}</strong></span>
                    <span>{new Date(item.submittedAt).toLocaleDateString('ar-AE')}</span>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex gap-2 flex-shrink-0">
                  <button
                    onClick={() => setShowComment(showComment === item.instanceId ? null : item.instanceId)}
                    className="px-3 py-1.5 text-xs border border-gray-200 text-gray-600 rounded-lg hover:bg-gray-50"
                  >
                    💬 {t('workflows.inbox.comment')}
                  </button>
                  <button
                    onClick={() => handleAct(item.instanceId, 'Rejected')}
                    disabled={acting && actingId === item.instanceId}
                    className="px-3 py-1.5 text-xs border border-red-200 text-red-600 rounded-lg hover:bg-red-50 disabled:opacity-50"
                  >
                    {acting && actingId === item.instanceId ? <Spinner size="sm" /> : t('workflows.inbox.reject')}
                  </button>
                  <button
                    onClick={() => handleAct(item.instanceId, 'Approved')}
                    disabled={acting && actingId === item.instanceId}
                    className="px-3 py-1.5 text-xs bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50"
                  >
                    {acting && actingId === item.instanceId ? <Spinner size="sm" /> : t('workflows.inbox.approve')}
                  </button>
                </div>
              </div>

              {/* Comment box */}
              {showComment === item.instanceId && (
                <div className="mt-4 border-t border-gray-100 pt-4 space-y-2">
                  <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    rows={2}
                    placeholder={t('workflows.inbox.commentPlaceholder')}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
                    dir="rtl"
                  />
                  <div className="flex justify-end gap-2">
                    <button
                      onClick={() => handleAct(item.instanceId, 'Commented')}
                      disabled={!comment.trim() || acting}
                      className="px-4 py-1.5 text-xs bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                    >
                      {t('workflows.inbox.sendComment')}
                    </button>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
