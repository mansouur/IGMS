import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { taskApi } from '../../services/governanceApi'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

const STATUS_CLS = {
  0: 'bg-gray-100 text-gray-600',
  1: 'bg-blue-100 text-blue-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-red-100 text-red-600',
}
const PRI_CLS = {
  0: 'bg-gray-50 text-gray-500 border-gray-200',
  1: 'bg-blue-50 text-blue-600 border-blue-200',
  2: 'bg-amber-50 text-amber-600 border-amber-200',
  3: 'bg-red-50 text-red-600 border-red-200',
}

// ── Progress stepper ──────────────────────────────────────────────────────────
function StatusStepper({ status }) {
  const steps = [
    { value: 0, label: 'قيد الانتظار', icon: '○' },
    { value: 1, label: 'جارية',         icon: '◑' },
    { value: 2, label: 'مكتملة',        icon: '●' },
  ]
  if (status === 3) {
    return (
      <div className="flex items-center gap-2 text-sm text-red-500">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
        </svg>
        ملغاة
      </div>
    )
  }
  return (
    <div className="flex items-center gap-0">
      {steps.map((s, i) => (
        <div key={s.value} className="flex items-center">
          <div className={`flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-all
            ${status >= s.value
              ? 'bg-emerald-50 border border-emerald-200'
              : 'bg-gray-50 border border-gray-100'}`}>
            <div className={`w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold
              ${status > s.value ? 'bg-emerald-500 text-white' : status === s.value ? 'bg-blue-500 text-white' : 'bg-gray-200 text-gray-400'}`}>
              {status > s.value ? '✓' : i + 1}
            </div>
            <span className={`text-xs font-medium ${status >= s.value ? 'text-emerald-700' : 'text-gray-400'}`}>
              {s.label}
            </span>
          </div>
          {i < steps.length - 1 && (
            <div className={`h-0.5 w-8 ${status > s.value ? 'bg-emerald-400' : 'bg-gray-200'}`} />
          )}
        </div>
      ))}
    </div>
  )
}

// ── InfoRow ───────────────────────────────────────────────────────────────────
function InfoRow({ label, children }) {
  return (
    <div className="flex items-start gap-4 py-3 border-b border-gray-50 last:border-0">
      <span className="text-xs text-gray-400 font-medium w-28 flex-shrink-0 pt-0.5">{label}</span>
      <span className="text-sm text-gray-800 flex-1">{children}</span>
    </div>
  )
}

// ── TaskDetail ────────────────────────────────────────────────────────────────
export default function TaskDetail() {
  const { t }      = useTranslation()
  const { id }     = useParams()
  const navigate   = useNavigate()
  const { loading, execute } = useApi()
  const canUpdate  = useAuthStore((s) => s.hasPermission)('TASKS.UPDATE')

  const [task, setTask] = useState(null)

  const STATUS_LABEL = {
    0: t('tasks.status.pending'),    1: t('tasks.status.inProgress'),
    2: t('tasks.status.done'),       3: t('tasks.status.cancelled'),
  }
  const PRI_LABEL = {
    0: t('tasks.priority.low'),    1: t('tasks.priority.medium'),
    2: t('tasks.priority.high'),   3: t('tasks.priority.critical'),
  }

  useEffect(() => {
    execute(() => taskApi.getById(id), { silent: true }).then((r) => { if (r) setTask(r) })
  }, [id])

  if (loading && !task) return <PageLoader />
  if (!task) return null

  const now      = new Date()
  const due      = task.dueDate ? new Date(task.dueDate) : null
  const isOverdue = due && due < now && task.status !== 2 && task.status !== 3
  const daysLeft  = due ? Math.ceil((due - now) / 86400000) : null

  return (
    <div className="space-y-5 max-w-3xl">

      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <button onClick={() => navigate('/tasks')}
            className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1 mb-2">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="15 18 9 12 15 6"/>
            </svg>
            {t('tasks.title')}
          </button>
          <h1 className="text-xl font-bold text-gray-800">{task.titleAr}</h1>
          {task.titleEn && <p className="text-xs text-gray-400 mt-0.5">{task.titleEn}</p>}
        </div>
        <div className="flex items-center gap-2 flex-shrink-0">
          <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${STATUS_CLS[task.status]}`}>
            {STATUS_LABEL[task.status]}
          </span>
          {canUpdate && (
            <button onClick={() => navigate(`/tasks/${id}/edit`)}
              className="px-3 py-1.5 text-sm border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50">
              {t('common.edit')}
            </button>
          )}
        </div>
      </div>

      {/* Status Stepper */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <p className="text-xs text-gray-400 font-medium mb-4">{t('tasks.fields.progress')}</p>
        <StatusStepper status={task.status} />
      </div>

      {/* Due date alert */}
      {isOverdue && (
        <div className="flex items-center gap-3 bg-red-50 border border-red-200 rounded-xl px-4 py-3">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#dc2626" strokeWidth="2">
            <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z M12 9v4 M12 17h.01"/>
          </svg>
          <p className="text-sm text-red-700 font-medium">
            {t('tasks.overdue')} — {Math.abs(daysLeft)} {t('tasks.daysAgo')}
          </p>
        </div>
      )}
      {!isOverdue && due && daysLeft != null && daysLeft <= 3 && daysLeft >= 0 && task.status !== 2 && (
        <div className="flex items-center gap-3 bg-amber-50 border border-amber-200 rounded-xl px-4 py-3">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#d97706" strokeWidth="2">
            <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
          </svg>
          <p className="text-sm text-amber-700 font-medium">
            {daysLeft === 0 ? t('tasks.dueToday') : t('tasks.dueSoon', { days: daysLeft })}
          </p>
        </div>
      )}

      {/* Details */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-2">{t('tasks.fields.details')}</h2>
        <InfoRow label={t('tasks.fields.priority')}>
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ${PRI_CLS[task.priority]}`}>
            {PRI_LABEL[task.priority]}
          </span>
        </InfoRow>
        <InfoRow label={t('tasks.fields.assignedTo')}>
          {task.assignedToNameAr
            ? <span className="flex items-center gap-2">
                <span className="w-6 h-6 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-xs font-bold">
                  {task.assignedToNameAr[0]}
                </span>
                {task.assignedToNameAr}
              </span>
            : <span className="text-gray-400">—</span>}
        </InfoRow>
        <InfoRow label={t('tasks.fields.department')}>
          {task.departmentNameAr ?? <span className="text-gray-400">—</span>}
        </InfoRow>
        <InfoRow label={t('tasks.fields.dueDate')}>
          {due
            ? <span className={isOverdue ? 'text-red-600 font-semibold' : 'text-gray-800'}>
                {due.toLocaleDateString('ar-AE', { year: 'numeric', month: 'long', day: 'numeric' })}
                {!isOverdue && daysLeft != null && daysLeft >= 0 && daysLeft <= 7 &&
                  <span className="ms-2 text-xs text-amber-600">({daysLeft} أيام)</span>}
                {isOverdue &&
                  <span className="ms-2 text-xs text-red-500">(متأخرة {Math.abs(daysLeft)} يوم)</span>}
              </span>
            : <span className="text-gray-400">—</span>}
        </InfoRow>
        <InfoRow label={t('tasks.fields.createdAt')}>
          {new Date(task.createdAt).toLocaleDateString('ar-AE', { year: 'numeric', month: 'long', day: 'numeric' })}
        </InfoRow>
        {task.riskId && (
          <InfoRow label="المخاطرة المرتبطة">
            <button onClick={() => navigate(`/risks/${task.riskId}`)}
              className="flex items-center gap-1.5 text-blue-600 hover:text-blue-800 hover:underline">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M10 13a5 5 0 007.54.54l3-3a5 5 0 00-7.07-7.07l-1.72 1.71M14 11a5 5 0 00-7.54-.54l-3 3a5 5 0 007.07 7.07l1.71-1.71"/>
              </svg>
              {task.riskTitleAr ?? `مخاطرة #${task.riskId}`}
            </button>
          </InfoRow>
        )}
      </div>

      {/* Description */}
      {task.descriptionAr && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-xs text-gray-400 font-medium mb-2">{t('tasks.fields.description')}</p>
          <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">{task.descriptionAr}</p>
        </div>
      )}

    </div>
  )
}
