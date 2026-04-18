import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { raciApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import { Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ─── RACI cell colors ─────────────────────────────────────────────────────────

const CELL = {
  R: 'bg-blue-100   text-blue-800   font-bold',
  A: 'bg-red-100    text-red-800    font-bold',
  C: 'bg-amber-100  text-amber-800',
  I: 'bg-gray-100   text-gray-600',
}

// ─── RaciDetail ───────────────────────────────────────────────────────────────

const STATUS_CLS = {
  0: 'bg-gray-100  text-gray-600',
  1: 'bg-amber-100 text-amber-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-slate-100 text-slate-500',
}

export default function RaciDetail() {
  const { t }      = useTranslation()
  const { id }     = useParams()
  const navigate   = useNavigate()
  const { loading, execute } = useApi()
  const { loading: acting, execute: act } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canUpdate = hasPermission('RACI.UPDATE')

  const STATUS_LABEL = {
    0: t('raci.status.draft'),
    1: t('raci.status.underReview'),
    2: t('raci.status.approved'),
    3: t('raci.status.archived'),
  }

  const [matrix, setMatrix] = useState(null)

  useEffect(() => {
    execute(() => raciApi.getById(id), { silent: true })
      .then((data) => { if (data) setMatrix(data) })
  }, [id])

  const handleSubmit = async () => {
    const res = await act(() => raciApi.submit(id), { successMsg: t('raci.messages.submitted') })
    if (res !== null) setMatrix((m) => ({ ...m, status: 1 }))
  }

  const handleApprove = async () => {
    const res = await act(() => raciApi.approve(id), { successMsg: t('raci.messages.approved') })
    if (res) setMatrix(res)
  }

  if (loading || !matrix) return <PageLoader />

  const stLabel = STATUS_LABEL[matrix.status] ?? ''
  const stCls   = STATUS_CLS[matrix.status]   ?? STATUS_CLS[0]

  return (
    <div className="max-w-5xl space-y-6">

      {/* ── Header ─────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <button onClick={() => navigate('/raci')}
            className="mt-1 p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 flex-shrink-0">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <h1 className="text-xl font-bold text-gray-800">{matrix.titleAr}</h1>
              <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${stCls}`}>
                {stLabel}
              </span>
            </div>
            {matrix.titleEn && (
              <p className="text-sm text-gray-400 mt-0.5" dir="ltr">{matrix.titleEn}</p>
            )}
            {matrix.departmentAr && (
              <p className="text-sm text-gray-500 mt-1">
                {t('raci.detail.department')} <span className="font-medium">{matrix.departmentAr}</span>
              </p>
            )}
            {matrix.approvedByName && (
              <p className="text-sm text-gray-500">
                {t('raci.detail.approvedBy')} <span className="font-medium">{matrix.approvedByName}</span>
                {' '}{t('raci.detail.approvedAt')} {new Date(matrix.approvedAt).toLocaleDateString()}
              </p>
            )}
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-2 flex-shrink-0">
          {canUpdate && matrix.status < 2 && (
            <button onClick={() => navigate(`/raci/${id}/edit`)}
              className="px-3 py-1.5 text-sm border border-gray-300 text-gray-600
                rounded-lg hover:bg-gray-50 transition-colors">
              {t('common.edit')}
            </button>
          )}
          {canUpdate && matrix.status === 0 && (
            <button onClick={handleSubmit} disabled={acting}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-amber-500
                text-white rounded-lg hover:bg-amber-600 transition-colors disabled:opacity-60">
              {acting && <Spinner size="sm" />}
              {t('raci.actions.submit')}
            </button>
          )}
          {hasPermission('RACI.APPROVE') && matrix.status === 1 && (
            <button onClick={handleApprove} disabled={acting}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm bg-emerald-700
                text-white rounded-lg hover:bg-emerald-800 transition-colors disabled:opacity-60">
              {acting && <Spinner size="sm" />}
              {t('raci.actions.approve')}
            </button>
          )}
        </div>
      </div>

      {/* Description */}
      {matrix.descriptionAr && (
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <p className="text-sm text-gray-700 leading-relaxed">{matrix.descriptionAr}</p>
        </div>
      )}

      {/* ── RACI Table ──────────────────────────────────── */}
      {matrix.activities?.length > 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700">
              {t('raci.detail.activitiesN', { n: matrix.activities.length })}
            </h2>
            <div className="flex gap-3 text-xs">
              {[['R', t('raci.detail.rLabel')], ['A', t('raci.detail.aLabel')],
                ['C', t('raci.detail.cLabel')], ['I', t('raci.detail.iLabel')]].map(([k, v]) => (
                <span key={k} className="flex items-center gap-1">
                  <span className={`px-1.5 py-0.5 rounded text-xs ${CELL[k]}`}>{k}</span>
                  <span className="text-gray-500">{v}</span>
                </span>
              ))}
            </div>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-start">
              <thead className="bg-gray-50 text-xs text-gray-500 border-b border-gray-100">
                <tr>
                  <th className="py-2 px-4 text-start font-semibold w-8">#</th>
                  <th className="py-2 px-4 text-start font-semibold">{t('raci.detail.activityCol')}</th>
                  <th className="py-2 px-4 text-start font-semibold">R</th>
                  <th className="py-2 px-4 text-center font-semibold">A</th>
                  <th className="py-2 px-4 text-start font-semibold">C</th>
                  <th className="py-2 px-4 text-start font-semibold">I</th>
                </tr>
              </thead>
              <tbody>
                {matrix.activities.map((act, idx) => (
                  <tr key={act.id} className="border-t border-gray-100 hover:bg-gray-50">
                    <td className="py-3 px-4 text-gray-400 text-xs">{idx + 1}</td>
                    <td className="py-3 px-4">
                      <p className="font-medium text-gray-800">{act.nameAr}</p>
                      {act.nameEn && <p className="text-xs text-gray-400" dir="ltr">{act.nameEn}</p>}
                    </td>
                    <td className="py-3 px-4">
                      <UserList users={act.responsible} badge="R" />
                    </td>
                    <td className="py-3 px-4 text-center">
                      <UserCell user={act.accountable} badge="A" />
                    </td>
                    <td className="py-3 px-4">
                      <UserList users={act.consulted} badge="C" />
                    </td>
                    <td className="py-3 px-4">
                      <UserList users={act.informed} badge="I" />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 p-10 text-center text-gray-400">
          <p className="text-sm">{t('raci.detail.noActivities')}</p>
        </div>
      )}
    </div>
  )
}

// ─── Cell components ──────────────────────────────────────────────────────────

function UserCell({ user, badge }) {
  if (!user) return <span className="text-gray-300">—</span>
  return (
    <div className="flex flex-col items-center gap-1">
      <span className={`w-6 h-6 flex items-center justify-center rounded text-xs ${CELL[badge]}`}>
        {badge}
      </span>
      <span className="text-xs text-gray-600 whitespace-nowrap">{user.fullNameAr}</span>
    </div>
  )
}

function UserList({ users, badge }) {
  if (!users?.length) return <span className="text-gray-300 text-xs">—</span>
  return (
    <div className="flex flex-col gap-1">
      {users.map((u) => (
        <span key={u.id} className="flex items-center gap-1 text-xs">
          <span className={`px-1 rounded text-[10px] ${CELL[badge]}`}>{badge}</span>
          <span className="text-gray-600">{u.fullNameAr}</span>
        </span>
      ))}
    </div>
  )
}
