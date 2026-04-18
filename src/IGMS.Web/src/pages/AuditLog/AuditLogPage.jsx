import { useState, useEffect, useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { auditApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'

// ─── Action badge styles ──────────────────────────────────────────────────────

const ACTION_CLS = {
  Created: 'bg-emerald-100 text-emerald-700',
  Updated: 'bg-blue-100   text-blue-700',
  Deleted: 'bg-red-100    text-red-700',
  Login:   'bg-amber-100  text-amber-700',
  Logout:  'bg-gray-100   text-gray-600',
}

const ACTION_DOT = {
  Created: 'bg-emerald-500',
  Updated: 'bg-blue-500',
  Deleted: 'bg-red-500',
  Login:   'bg-amber-500',
  Logout:  'bg-gray-400',
}

// ─── AuditLogPage ─────────────────────────────────────────────────────────────

export default function AuditLogPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  useEffect(() => {
    if (!hasPermission('AUDIT.READ')) navigate('/dashboard', { replace: true })
  }, [])

  const [data,       setData]       = useState({ items: [], totalCount: 0, currentPage: 1, pageSize: 25 })
  const [entityType, setEntityType] = useState('')
  const [action,     setAction]     = useState('')
  const [from,       setFrom]       = useState('')
  const [to,         setTo]         = useState('')
  const [page,       setPage]       = useState(1)
  const [pageSize,   setPageSize]   = useState(25)
  const [expanded,   setExpanded]   = useState(null)
  const [entityTypes, setEntityTypes] = useState([])

  // Load filter options once
  useEffect(() => {
    auditApi.getEntityTypes()
      .then((r) => setEntityTypes(r.data?.value ?? []))
      .catch(() => {})
  }, [])

  const fetchData = useCallback(async () => {
    const r = await execute(() => auditApi.getAll({
      page,
      pageSize,
      entityName: entityType || undefined,
      action:     action     || undefined,
      from:       from       || undefined,
      to:         to         || undefined,
    }), { silent: true })
    if (r) setData(r)
  }, [page, pageSize, entityType, action, from, to])

  useEffect(() => { fetchData() }, [fetchData])

  const handleFilter = (setter) => (e) => {
    setter(e.target.value)
    setPage(1)
  }

  const toggleExpand = (id) => setExpanded((prev) => (prev === id ? null : id))

  return (
    <div className="space-y-5">

      {/* ── Header ─────────────────────────────────────────────────── */}
      <div>
        <h1 className="text-xl font-bold text-gray-800">{t('auditLog.title')}</h1>
        <p className="text-sm text-gray-500 mt-0.5">{t('auditLog.subtitle')}</p>
      </div>

      {/* ── Filters ────────────────────────────────────────────────── */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
          {/* Entity type */}
          <select
            value={entityType}
            onChange={handleFilter(setEntityType)}
            className={selectCls}
          >
            <option value="">{t('auditLog.filters.allEntities')}</option>
            {entityTypes.map((et) => (
              <option key={et} value={et}>{et}</option>
            ))}
          </select>

          {/* Action */}
          <select
            value={action}
            onChange={handleFilter(setAction)}
            className={selectCls}
          >
            <option value="">{t('auditLog.filters.allActions')}</option>
            {['Created', 'Updated', 'Deleted', 'Login', 'Logout'].map((a) => (
              <option key={a} value={a}>{t(`auditLog.actions.${a.toLowerCase()}`)}</option>
            ))}
          </select>

          {/* Date from */}
          <input
            type="datetime-local"
            value={from}
            onChange={handleFilter(setFrom)}
            className={inputCls}
            placeholder={t('auditLog.filters.from')}
          />

          {/* Date to */}
          <input
            type="datetime-local"
            value={to}
            onChange={handleFilter(setTo)}
            className={inputCls}
            placeholder={t('auditLog.filters.to')}
          />
        </div>
      </div>

      {/* ── Timeline ───────────────────────────────────────────────── */}
      {loading ? (
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <SkeletonTable rows={8} cols={5} />
        </div>
      ) : data.items.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center text-gray-400">
          <p className="text-sm">{t('auditLog.noData')}</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-100">
            <span className="text-sm font-semibold text-gray-700">
              {t('auditLog.totalRecords', { n: data.totalCount })}
            </span>
          </div>

          <ul className="divide-y divide-gray-100">
            {data.items.map((entry) => (
              <AuditRow
                key={entry.id}
                entry={entry}
                isExpanded={expanded === entry.id}
                onToggle={() => toggleExpand(entry.id)}
                t={t}
              />
            ))}
          </ul>
        </div>
      )}

      {/* ── Pagination ─────────────────────────────────────────────── */}
      {data.totalCount > 0 && (
        <Pagination
          currentPage={page}
          totalPages={Math.ceil(data.totalCount / pageSize)}
          totalCount={data.totalCount}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(s) => { setPageSize(s); setPage(1) }}
        />
      )}
    </div>
  )
}

// ─── AuditRow ─────────────────────────────────────────────────────────────────

function AuditRow({ entry, isExpanded, onToggle, t }) {
  const dotCls = ACTION_DOT[entry.action] ?? 'bg-gray-400'
  const badgeCls = ACTION_CLS[entry.action] ?? 'bg-gray-100 text-gray-600'
  const hasDiff = entry.oldValues || entry.newValues

  return (
    <li className="px-5 py-3 hover:bg-gray-50 transition-colors">
      <div className="flex items-start gap-3">
        {/* Action dot */}
        <div className="flex-shrink-0 mt-1.5">
          <span className={`w-2.5 h-2.5 rounded-full block ${dotCls}`} />
        </div>

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${badgeCls}`}>
              {t(`auditLog.actions.${entry.action.toLowerCase()}`)}
            </span>
            <span className="text-sm font-medium text-gray-700">{entry.entityName}</span>
            <span className="text-xs text-gray-400">#{entry.entityId}</span>
          </div>

          <div className="flex flex-wrap items-center gap-3 mt-1 text-xs text-gray-500">
            <span className="flex items-center gap-1">
              <UserIcon />
              {entry.username}
            </span>
            <span>{new Date(entry.timestamp).toLocaleString()}</span>
            {entry.ipAddress && <span dir="ltr">{entry.ipAddress}</span>}
          </div>
        </div>

        {/* Expand button (only if diff exists) */}
        {hasDiff && (
          <button
            onClick={onToggle}
            className="flex-shrink-0 p-1 rounded text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
            title={isExpanded ? t('auditLog.hideDiff') : t('auditLog.showDiff')}
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              {isExpanded
                ? <path d="M18 15l-6-6-6 6" />
                : <path d="M6 9l6 6 6-6" />}
            </svg>
          </button>
        )}
      </div>

      {/* Diff panel */}
      {isExpanded && hasDiff && (
        <div className="mt-3 ms-5 grid grid-cols-1 sm:grid-cols-2 gap-3">
          {entry.oldValues && (
            <DiffPane
              label={t('auditLog.diff.before')}
              json={entry.oldValues}
              colorCls="border-red-200 bg-red-50"
              labelCls="text-red-600"
            />
          )}
          {entry.newValues && (
            <DiffPane
              label={t('auditLog.diff.after')}
              json={entry.newValues}
              colorCls="border-emerald-200 bg-emerald-50"
              labelCls="text-emerald-600"
            />
          )}
        </div>
      )}
    </li>
  )
}

// ─── DiffPane ─────────────────────────────────────────────────────────────────

function DiffPane({ label, json, colorCls, labelCls }) {
  let parsed = null
  try { parsed = JSON.parse(json) } catch { parsed = null }

  return (
    <div className={`border rounded-lg p-3 text-xs ${colorCls}`}>
      <p className={`font-semibold mb-2 ${labelCls}`}>{label}</p>
      {parsed ? (
        <dl className="space-y-1">
          {Object.entries(parsed).map(([k, v]) => (
            <div key={k} className="flex gap-2">
              <dt className="font-medium text-gray-500 shrink-0">{k}:</dt>
              <dd className="text-gray-700 break-all">
                {v === null || v === undefined ? <em className="text-gray-400">null</em> : String(v)}
              </dd>
            </div>
          ))}
        </dl>
      ) : (
        <pre className="text-gray-600 whitespace-pre-wrap break-all">{json}</pre>
      )}
    </div>
  )
}

// ─── Inline icons ─────────────────────────────────────────────────────────────

function UserIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2" strokeLinecap="round">
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  )
}

// ─── Styles ───────────────────────────────────────────────────────────────────

const selectCls = `w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
  focus:outline-none focus:ring-2 focus:ring-green-600 bg-white text-gray-700`

const inputCls = `w-full border border-gray-300 rounded-lg px-3 py-2 text-sm
  focus:outline-none focus:ring-2 focus:ring-green-600 bg-white text-gray-700`
