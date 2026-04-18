import { useState, useEffect, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { controlTestApi } from '../../services/governanceApi'
import { useApi, useConfirm } from '../../hooks/useApi'
import { PageLoader, Spinner } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

const EFF_COLOR = {
  0: 'bg-gray-100 text-gray-600',
  1: 'bg-emerald-100 text-emerald-700',
  2: 'bg-amber-100 text-amber-700',
  3: 'bg-red-100 text-red-700',
}

function FileSizeLabel({ bytes }) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

export default function ControlTestDetail() {
  const { t }      = useTranslation()
  const { id }     = useParams()
  const navigate   = useNavigate()
  const confirm    = useConfirm()
  const fileRef    = useRef(null)

  const { loading, execute }            = useApi()
  const { loading: uploading, execute: uploadEx } = useApi()

  const canUpdate = useAuthStore((s) => s.hasPermission)('CONTROLS.UPDATE')
  const canDelete = useAuthStore((s) => s.hasPermission)('CONTROLS.DELETE')

  const [test, setTest] = useState(null)

  const EFF_LABEL = {
    0: t('controls.effectiveness.0'),
    1: t('controls.effectiveness.1'),
    2: t('controls.effectiveness.2'),
    3: t('controls.effectiveness.3'),
  }

  const load = async () => {
    const r = await execute(() => controlTestApi.getById(id), { silent: true })
    if (r) setTest(r)
  }

  useEffect(() => { load() }, [id])

  // ── Evidence upload ──────────────────────────────────────────────────────────
  const handleFileSelect = async (e) => {
    const file = e.target.files?.[0]
    if (!file) return
    e.target.value = ''   // reset input

    const formData = new FormData()
    formData.append('file', file)

    const r = await uploadEx(() => controlTestApi.uploadEvidence(id, formData), {
      successMsg: t('controls.messages.evidenceUploaded'),
    })
    if (r) load()
  }

  // ── Evidence download ────────────────────────────────────────────────────────
  const handleDownload = async (evidenceId, fileName) => {
    try {
      const res = await controlTestApi.downloadEvidence(id, evidenceId)
      const url = URL.createObjectURL(new Blob([res.data], { type: res.headers['content-type'] }))
      const a   = document.createElement('a')
      a.href     = url
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    } catch {
      toast.error(t('auth.error.serverError'))
    }
  }

  // ── Evidence delete ──────────────────────────────────────────────────────────
  const handleDeleteEvidence = async (evidenceId) => {
    const ok = await confirm({
      title:   t('controls.confirm.deleteEvTitle'),
      message: t('controls.confirm.deleteEvMsg'),
      variant: 'danger',
    })
    if (!ok) return
    const r = await execute(() => controlTestApi.deleteEvidence(id, evidenceId), {
      successMsg: t('controls.messages.evidenceDeleted'),
    })
    if (r !== null) load()
  }

  if (loading && !test) return <PageLoader />
  if (!test) return null

  return (
    <div className="max-w-4xl space-y-6">
      {/* ── Header ── */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{test.titleAr}</h1>
          {test.titleEn && <p className="text-sm text-gray-500 mt-0.5">{test.titleEn}</p>}
          <p className="text-xs text-gray-400 font-mono mt-1">{test.code}</p>
        </div>
        <div className="flex items-center gap-2">
          {canUpdate && (
            <button
              onClick={() => navigate(`/controls/${id}/edit`)}
              className="flex items-center gap-2 px-3 py-2 border border-gray-300 text-gray-700 text-sm rounded-lg hover:bg-gray-50"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              {t('common.edit')}
            </button>
          )}
          <button onClick={() => navigate('/controls')} className="px-3 py-2 text-sm text-gray-500 hover:text-gray-700">
            ← {t('controls.title')}
          </button>
        </div>
      </div>

      {/* ── Details card ── */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-6">

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('controls.table.entity')}</p>
            <p className="text-sm font-medium text-gray-500 mb-0.5">
              {test.entityType === 'Policy' ? t('controls.form.entityPolicy') : t('controls.form.entityRisk')}
            </p>
            <p className="text-sm text-gray-800">{test.entityTitleAr ?? `#${test.entityId}`}</p>
          </div>

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('controls.table.effectiveness')}</p>
            <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${EFF_COLOR[test.effectiveness]}`}>
              {EFF_LABEL[test.effectiveness]}
            </span>
          </div>

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('controls.table.testedBy')}</p>
            <p className="text-sm text-gray-800">{test.testedByName ?? '—'}</p>
          </div>

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('controls.form.testedAt')}</p>
            <p className="text-sm text-gray-800">
              {test.testedAt ? new Date(test.testedAt).toLocaleDateString('ar-AE') : '—'}
            </p>
          </div>

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('controls.form.nextTestDate')}</p>
            <p className="text-sm text-gray-800">
              {test.nextTestDate ? new Date(test.nextTestDate).toLocaleDateString('ar-AE') : '—'}
            </p>
          </div>

          <div>
            <p className="text-xs text-gray-400 mb-1">{t('tasks.table.createdAt')}</p>
            <p className="text-sm text-gray-800">{new Date(test.createdAt).toLocaleDateString('ar-AE')}</p>
          </div>
        </div>

        {test.descriptionAr && (
          <div className="mt-5 pt-5 border-t border-gray-100">
            <p className="text-xs text-gray-400 mb-1">{t('controls.form.descriptionAr')}</p>
            <p className="text-sm text-gray-700 whitespace-pre-wrap" dir="rtl">{test.descriptionAr}</p>
          </div>
        )}

        {test.findingsAr && (
          <div className="mt-5 pt-5 border-t border-gray-100">
            <p className="text-xs text-gray-400 mb-1">{t('controls.form.findingsAr')}</p>
            <p className="text-sm text-gray-700 whitespace-pre-wrap" dir="rtl">{test.findingsAr}</p>
          </div>
        )}
      </div>

      {/* ── Evidence card ── */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-gray-800">{t('controls.evidence.title')}</h2>
          {canUpdate && (
            <>
              <input
                ref={fileRef} type="file" className="hidden"
                accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.gif"
                onChange={handleFileSelect}
              />
              <button
                onClick={() => fileRef.current?.click()}
                disabled={uploading}
                className="flex items-center gap-2 px-3 py-1.5 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-60"
              >
                {uploading ? <Spinner size="sm" /> : (
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                )}
                {uploading ? t('controls.evidence.uploading') : t('controls.evidence.upload')}
              </button>
            </>
          )}
        </div>

        <p className="text-xs text-gray-400 mb-4">{t('controls.evidence.uploadHint')}</p>

        {test.evidences.length === 0 ? (
          <div className="text-center py-8 text-gray-400 text-sm">{t('controls.evidence.noEvidence')}</div>
        ) : (
          <ul className="space-y-2">
            {test.evidences.map((ev) => (
              <li key={ev.id} className="flex items-center justify-between p-3 border border-gray-100 rounded-lg hover:bg-gray-50">
                <div className="flex items-center gap-3 min-w-0">
                  <div className="w-8 h-8 rounded-lg bg-blue-50 flex items-center justify-center flex-shrink-0">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#3b82f6" strokeWidth="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-800 truncate">{ev.fileName}</p>
                    <p className="text-xs text-gray-400">
                      <FileSizeLabel bytes={ev.fileSizeBytes} /> · {ev.uploadedBy} · {new Date(ev.uploadedAt).toLocaleDateString('ar-AE')}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-1 flex-shrink-0 ms-3">
                  <button
                    onClick={() => handleDownload(ev.id, ev.fileName)}
                    title={t('controls.evidence.download')}
                    className="p-1.5 rounded-md text-blue-600 hover:bg-blue-50"
                  >
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
                  </button>
                  {canDelete && (
                    <button
                      onClick={() => handleDeleteEvidence(ev.id)}
                      title={t('controls.evidence.delete')}
                      className="p-1.5 rounded-md text-red-600 hover:bg-red-50"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/></svg>
                    </button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}
