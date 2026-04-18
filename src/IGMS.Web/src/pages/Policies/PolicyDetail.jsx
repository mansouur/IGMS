import { useState, useEffect, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { policyApi } from '../../services/governanceApi'
import api from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { PageLoader, Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'
import ComplianceTags from '../../components/ui/ComplianceTags'
import PolicyLifecycle from '../../components/ui/PolicyLifecycle'

// ─── Allowed file types ───────────────────────────────────────────────────────
const ACCEPTED = '.pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.gif'

const ICON_BY_TYPE = (contentType) => {
  if (contentType.includes('pdf'))   return { icon: 'PDF',  cls: 'bg-red-100 text-red-700' }
  if (contentType.includes('word'))  return { icon: 'DOC',  cls: 'bg-blue-100 text-blue-700' }
  if (contentType.includes('sheet') || contentType.includes('excel'))
                                     return { icon: 'XLS',  cls: 'bg-emerald-100 text-emerald-700' }
  if (contentType.includes('image')) return { icon: 'IMG',  cls: 'bg-purple-100 text-purple-700' }
  return                                    { icon: 'FILE', cls: 'bg-gray-100 text-gray-600' }
}

const STATUS_CLS   = { 0: 'bg-gray-100 text-gray-600', 1: 'bg-emerald-100 text-emerald-700', 2: 'bg-amber-100 text-amber-700' }

// ─── PolicyDetail ─────────────────────────────────────────────────────────────

export default function PolicyDetail() {
  const { t }      = useTranslation()
  const { id }     = useParams()
  const navigate   = useNavigate()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canUpdate  = hasPermission('POLICIES.UPDATE')
  const canCreate  = hasPermission('POLICIES.CREATE')

  const [policy,        setPolicy]        = useState(null)
  const [renewing,      setRenewing]      = useState(false)
  const [attachments,   setAttachments]   = useState([])
  const [uploading,     setUploading]     = useState(false)
  const [deleting,      setDeleting]      = useState(null) // attachment id being deleted
  const [ackStatus,     setAckStatus]     = useState(null) // { hasAcknowledged, acknowledgedAt, totalAcknowledged }
  const [ackRecords,    setAckRecords]    = useState([])
  const [ackLoading,    setAckLoading]    = useState(false)
  const [showAckList,   setShowAckList]   = useState(false)
  const [ackListLoaded, setAckListLoaded] = useState(false)
  const [versions,      setVersions]      = useState([])
  const fileRef = useRef(null)

  // Format helpers – date only vs date+time
  const fmtDate     = (d) => d ? new Date(d).toLocaleDateString()         : '—'
  const fmtDateTime = (d) => d ? new Date(d).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' }) : '—'

  const STATUS_LABEL = { 0: t('policies.status.draft'), 1: t('policies.status.active'), 2: t('policies.status.archived') }
  const CAT_LABEL    = {
    0: t('policies.category.governance'), 1: t('policies.category.technology'),
    2: t('policies.category.hr'),         3: t('policies.category.finance'),
    4: t('policies.category.operational'),
  }

  useEffect(() => {
    execute(() => policyApi.getById(id), { silent: true })
      .then((data) => { if (data) setPolicy(data) })
    loadAttachments()
    loadAckStatus()
    policyApi.getVersions(id)
      .then((r) => setVersions(r.data?.data ?? []))
      .catch(() => {})
  }, [id])

  const loadAttachments = () => {
    api.get(`/api/v1/policies/${id}/attachments`)
      .then((r) => setAttachments(r.data?.data ?? []))
      .catch(() => {})
  }

  const loadAckStatus = () => {
    api.get(`/api/v1/policies/${id}/acknowledgments/status`)
      .then((r) => setAckStatus(r.data?.data ?? null))
      .catch(() => {})
  }

  const loadAckRecords = () => {
    api.get(`/api/v1/policies/${id}/acknowledgments`)
      .then((r) => { setAckRecords(r.data?.data ?? []); setAckListLoaded(true) })
      .catch(() => {})
  }

  const handleAcknowledge = async () => {
    setAckLoading(true)
    try {
      const r = await api.post(`/api/v1/policies/${id}/acknowledgments`)
      setAckStatus(r.data?.data ?? null)
      // Refresh list if already open
      if (showAckList) loadAckRecords()
      toast.success(t('policies.acknowledgment.success'))
    } catch {
      toast.error(t('policies.acknowledgment.error'))
    } finally {
      setAckLoading(false)
    }
  }

  const toggleAckList = () => {
    if (!showAckList && !ackListLoaded) loadAckRecords()
    setShowAckList((v) => !v)
  }

  const handleFileChange = async (e) => {
    const file = e.target.files?.[0]
    if (!file) return
    e.target.value = '' // reset so same file can be re-selected

    setUploading(true)
    try {
      const formData = new FormData()
      formData.append('file', file)
      await api.post(`/api/v1/policies/${id}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      toast.success(t('policies.attachments.uploaded'))
      loadAttachments()
    } catch (err) {
      const msg = err.response?.data?.errors?.[0] ?? t('policies.attachments.uploadError')
      toast.error(msg)
    } finally {
      setUploading(false)
    }
  }

  const handleDownload = async (att) => {
    try {
      const r = await api.get(
        `/api/v1/policies/${id}/attachments/${att.id}/download`,
        { responseType: 'blob' }
      )
      const url  = URL.createObjectURL(new Blob([r.data], { type: att.contentType }))
      const link = document.createElement('a')
      link.href = url; link.download = att.fileName
      document.body.appendChild(link); link.click()
      document.body.removeChild(link); URL.revokeObjectURL(url)
    } catch {
      toast.error(t('policies.attachments.downloadError'))
    }
  }

  const handleRenew = async () => {
    setRenewing(true)
    try {
      const r = await policyApi.renew(id)
      const newId = r.data?.data?.id
      toast.success(t('policies.renew.success'))
      navigate(`/policies/${newId}/edit`)
    } catch {
      toast.error(t('policies.renew.error'))
    } finally {
      setRenewing(false)
    }
  }

  const handleDelete = async (att) => {
    setDeleting(att.id)
    try {
      await api.delete(`/api/v1/policies/${id}/attachments/${att.id}`)
      toast.success(t('policies.attachments.deleted'))
      setAttachments((prev) => prev.filter((a) => a.id !== att.id))
    } catch {
      toast.error(t('policies.attachments.deleteError'))
    } finally {
      setDeleting(null)
    }
  }

  if (loading || !policy) return <PageLoader />

  const stCls   = STATUS_CLS[policy.status]   ?? STATUS_CLS[0]
  const stLabel = STATUS_LABEL[policy.status]  ?? ''

  return (
    <div className="max-w-4xl space-y-6">

      {/* ── Header ─────────────────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <button onClick={() => navigate('/policies')}
            className="mt-1 p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 flex-shrink-0">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <h1 className="text-xl font-bold text-gray-800">{policy.titleAr}</h1>
              <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${stCls}`}>
                {stLabel}
              </span>
            </div>
            {policy.titleEn && (
              <p className="text-sm text-gray-400 mt-0.5" dir="ltr">{policy.titleEn}</p>
            )}
            <div className="flex flex-wrap gap-3 mt-1 text-xs text-gray-500">
              <span className="font-mono bg-gray-100 px-2 py-0.5 rounded">{policy.code}</span>
              {policy.departmentNameAr && <span>{policy.departmentNameAr}</span>}
              {policy.ownerNameAr      && <span>{t('policies.detail.owner')}: {policy.ownerNameAr}</span>}
              <span>{CAT_LABEL[policy.category]}</span>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2 flex-shrink-0">
          {canCreate && policy.status !== 0 && (
            <button onClick={handleRenew} disabled={renewing}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm border border-emerald-600
                text-emerald-700 rounded-lg hover:bg-emerald-50 transition-colors disabled:opacity-60">
              {renewing
                ? <Spinner size="sm" />
                : <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M23 4v6h-6M1 20v-6h6"/><path d="M3.51 9a9 9 0 0114.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0020.49 15"/>
                  </svg>}
              {t('policies.renew.button')}
            </button>
          )}
          {canUpdate && policy.status < 2 && (
            <button onClick={() => navigate(`/policies/${id}/edit`)}
              className="flex-shrink-0 px-3 py-1.5 text-sm border border-gray-300 text-gray-600
                rounded-lg hover:bg-gray-50 transition-colors">
              {t('common.edit')}
            </button>
          )}
        </div>
      </div>

      {/* ── Description ─────────────────────────────────────────────── */}
      {policy.descriptionAr && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-sm text-gray-700 leading-relaxed">{policy.descriptionAr}</p>
        </div>
      )}

      {/* ── دورة حياة السياسة ───────────────────────────────────────── */}
      <PolicyLifecycle policy={policy} fmtDate={fmtDate} fmtDateTime={fmtDateTime} />

      {/* ── Version History ─────────────────────────────────────────── */}
      {versions.length > 1 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">
            سجل الإصدارات
            <span className="ms-2 text-xs font-normal text-gray-400">({versions.length} إصدار)</span>
          </h2>
          <div className="relative">
            {/* vertical line */}
            <div className="absolute right-3.5 top-0 bottom-0 w-px bg-gray-200" />
            <div className="space-y-4">
              {versions.map((v, i) => {
                const stCls  = STATUS_CLS[v.status]   ?? STATUS_CLS[0]
                const stLbl  = STATUS_LABEL[v.status]  ?? ''
                const isCurrent = v.isCurrent
                return (
                  <div key={v.id} className="flex gap-4 relative">
                    {/* dot */}
                    <div className={`flex-shrink-0 w-7 h-7 rounded-full border-2 flex items-center justify-center z-10
                      ${isCurrent ? 'bg-green-700 border-green-700' : 'bg-white border-gray-300'}`}>
                      {isCurrent
                        ? <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3"><polyline points="20 6 9 17 4 12"/></svg>
                        : <span className="text-[10px] font-bold text-gray-400">{v.version}</span>
                      }
                    </div>
                    {/* content */}
                    <div className={`flex-1 pb-4 ${i < versions.length - 1 ? 'border-b border-gray-100' : ''}`}>
                      <div className="flex items-center gap-2 flex-wrap">
                        <button
                          onClick={() => navigate(`/policies/${v.id}`)}
                          className={`text-sm font-semibold hover:underline ${isCurrent ? 'text-green-700' : 'text-gray-700'}`}
                        >
                          {v.code}
                        </button>
                        {isCurrent && (
                          <span className="text-xs bg-green-100 text-green-700 px-1.5 py-0.5 rounded font-medium">الحالي</span>
                        )}
                        <span className={`text-xs px-1.5 py-0.5 rounded font-medium ${stCls}`}>{stLbl}</span>
                      </div>
                      <p className="text-sm text-gray-600 mt-0.5">{v.titleAr}</p>
                      <div className="flex flex-wrap gap-3 mt-1 text-xs text-gray-400">
                        <span>أُنشئ: {fmtDate(v.createdAt)}</span>
                        {v.approvedAt && <span>اعتُمد: {fmtDate(v.approvedAt)} {v.approverNameAr ? `بواسطة ${v.approverNameAr}` : ''}</span>}
                        {v.effectiveDate && <span>نافذ: {fmtDate(v.effectiveDate)}</span>}
                        {v.expiryDate    && <span>ينتهي: {fmtDate(v.expiryDate)}</span>}
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      )}

      {/* ── Acknowledgment ──────────────────────────────────────────── */}
      {policy.status === 1 && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700">
              {t('policies.acknowledgment.title')}
              {ackStatus?.totalAcknowledged > 0 && (
                <span className="ms-2 text-xs font-normal text-gray-400">
                  ({ackStatus.totalAcknowledged})
                </span>
              )}
            </h2>
            {canUpdate && ackStatus?.totalAcknowledged > 0 && (
              <button
                onClick={toggleAckList}
                className="text-xs text-green-700 hover:text-green-800 font-medium"
              >
                {showAckList ? t('policies.acknowledgment.hideList') : t('policies.acknowledgment.showList')}
              </button>
            )}
          </div>

          <div className="px-5 py-4 space-y-4">
            {/* Status / Action */}
            {ackStatus?.hasAcknowledged ? (
              <div className="flex flex-col sm:flex-row sm:items-center gap-3">
                {/* Green acknowledged badge */}
                <div className="flex items-center gap-2 flex-1">
                  <span className="flex-shrink-0 w-8 h-8 rounded-full bg-emerald-100 flex items-center justify-center">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
                      stroke="#059669" strokeWidth="2.5" strokeLinecap="round">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                  </span>
                  <div>
                    <p className="text-sm font-semibold text-emerald-700">
                      {t('policies.acknowledgment.acknowledged', {
                        date: fmtDateTime(ackStatus.acknowledgedAt)
                      })}
                    </p>
                    {ackStatus.totalAcknowledged > 0 && (
                      <p className="text-xs text-gray-400 mt-0.5">
                        {t('policies.acknowledgment.totalCount', { n: ackStatus.totalAcknowledged })}
                      </p>
                    )}
                  </div>
                </div>
                {/* Re-acknowledge button */}
                <button
                  onClick={handleAcknowledge}
                  disabled={ackLoading}
                  className="text-xs px-3 py-1.5 border border-emerald-300 text-emerald-700
                    rounded-lg hover:bg-emerald-50 disabled:opacity-60 transition-colors flex-shrink-0"
                >
                  {ackLoading ? <Spinner size="sm" /> : t('policies.acknowledgment.reAcknowledge')}
                </button>
              </div>
            ) : (
              <div className="flex flex-col sm:flex-row sm:items-center gap-3">
                <p className="flex-1 text-sm text-gray-600">
                  {ackStatus?.totalAcknowledged > 0
                    ? t('policies.acknowledgment.totalCount', { n: ackStatus.totalAcknowledged })
                    : t('policies.acknowledgment.noRecords')}
                </p>
                <button
                  onClick={handleAcknowledge}
                  disabled={ackLoading}
                  className="flex items-center gap-2 px-4 py-2 text-sm bg-green-700 text-white
                    rounded-lg hover:bg-green-800 disabled:opacity-60 transition-colors flex-shrink-0 font-medium"
                >
                  {ackLoading ? <Spinner size="sm" /> : (
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                      stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                  )}
                  {t('policies.acknowledgment.acknowledge')}
                </button>
              </div>
            )}

            {/* Acknowledgment records list – managers only */}
            {canUpdate && showAckList && (
              <div className="border border-gray-100 rounded-lg overflow-hidden">
                {ackRecords.length === 0 ? (
                  <p className="text-center text-sm text-gray-400 py-6">
                    {t('policies.acknowledgment.noRecords')}
                  </p>
                ) : (
                  <table className="w-full text-sm">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="text-start px-4 py-2 text-xs font-semibold text-gray-500">
                          {t('policies.acknowledgment.table.employee')}
                        </th>
                        <th className="text-start px-4 py-2 text-xs font-semibold text-gray-500">
                          {t('policies.acknowledgment.table.username')}
                        </th>
                        <th className="text-start px-4 py-2 text-xs font-semibold text-gray-500">
                          {t('policies.acknowledgment.table.date')}
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                      {ackRecords.map((rec) => (
                        <tr key={rec.id} className="hover:bg-gray-50">
                          <td className="px-4 py-2.5 font-medium text-gray-800">{rec.fullNameAr}</td>
                          <td className="px-4 py-2.5 text-gray-500 font-mono text-xs">{rec.username}</td>
                          <td className="px-4 py-2.5 text-gray-500">
                            {fmtDateTime(rec.acknowledgedAt)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── Compliance Framework Mapping ────────────────────────────── */}
      <ComplianceTags entityType="Policy" entityId={Number(id)} canEdit={canUpdate} />

      {/* ── Attachments ─────────────────────────────────────────────── */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="px-5 py-3 border-b border-gray-100 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-gray-700">
            {t('policies.attachments.title')}
            {attachments.length > 0 && (
              <span className="ms-2 text-xs font-normal text-gray-400">({attachments.length})</span>
            )}
          </h2>
          {canUpdate && (
            <>
              <input
                ref={fileRef}
                type="file"
                accept={ACCEPTED}
                className="hidden"
                onChange={handleFileChange}
              />
              <button
                onClick={() => fileRef.current?.click()}
                disabled={uploading}
                className="flex items-center gap-2 px-3 py-1.5 text-sm bg-green-700 text-white
                  rounded-lg hover:bg-green-800 disabled:opacity-60 transition-colors"
              >
                {uploading ? <Spinner size="sm" /> : (
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                    stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                    <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
                    <polyline points="17 8 12 3 7 8"/>
                    <line x1="12" y1="3" x2="12" y2="15"/>
                  </svg>
                )}
                {t('policies.attachments.upload')}
              </button>
            </>
          )}
        </div>

        {attachments.length === 0 ? (
          <div className="px-5 py-10 text-center text-sm text-gray-400">
            {t('policies.attachments.empty')}
          </div>
        ) : (
          <ul className="divide-y divide-gray-100">
            {attachments.map((att) => {
              const { icon, cls } = ICON_BY_TYPE(att.contentType)
              return (
                <li key={att.id}
                  className="flex items-center gap-3 px-5 py-3 hover:bg-gray-50 transition-colors">
                  {/* Type badge */}
                  <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded flex-shrink-0 ${cls}`}>
                    {icon}
                  </span>

                  {/* Name + meta */}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-800 truncate">{att.fileName}</p>
                    <p className="text-xs text-gray-400 mt-0.5">
                      {att.fileSizeDisplay} · {att.uploadedBy} · {new Date(att.uploadedAt).toLocaleDateString()}
                    </p>
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-1 flex-shrink-0">
                    <button
                      onClick={() => handleDownload(att)}
                      title={t('policies.attachments.download')}
                      className="p-1.5 rounded text-green-600 hover:bg-green-50 transition-colors"
                    >
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                        stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                        <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
                        <polyline points="7 10 12 15 17 10"/>
                        <line x1="12" y1="15" x2="12" y2="3"/>
                      </svg>
                    </button>
                    {canUpdate && (
                      <button
                        onClick={() => handleDelete(att)}
                        disabled={deleting === att.id}
                        title={t('common.delete')}
                        className="p-1.5 rounded text-red-500 hover:bg-red-50 transition-colors disabled:opacity-40"
                      >
                        {deleting === att.id
                          ? <Spinner size="sm" />
                          : <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                              <polyline points="3 6 5 6 21 6"/>
                              <path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/>
                            </svg>
                        }
                      </button>
                    )}
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
