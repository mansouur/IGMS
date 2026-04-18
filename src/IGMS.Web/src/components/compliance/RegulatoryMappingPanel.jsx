import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { regulatoryApi } from '../../services/api'
import { useApi } from '../../hooks/useApi'
import { Spinner } from '../ui/Spinner'

const STATUS_STYLE = {
  Compliant:          'bg-green-100 text-green-700',
  PartiallyCompliant: 'bg-yellow-100 text-yellow-700',
  NonCompliant:       'bg-red-100 text-red-700',
  NotAssessed:        'bg-gray-100 text-gray-500',
}

const STATUS_LABEL_AR = {
  Compliant:          'مطابق',
  PartiallyCompliant: 'جزئياً',
  NonCompliant:       'غير مطابق',
  NotAssessed:        'لم يُقيَّم',
}

const STATUSES = ['NotAssessed', 'Compliant', 'PartiallyCompliant', 'NonCompliant']

/**
 * Panel for linking a ControlTest/Policy/Risk to regulatory controls.
 * Props:
 *   entityType: 'ControlTest' | 'Policy' | 'Risk'
 *   entityId:   number
 *   canManage:  boolean
 */
export default function RegulatoryMappingPanel({ entityType, entityId, canManage = false }) {
  const { t } = useTranslation()

  const { execute: loadEx  } = useApi()
  const { execute: fwEx    } = useApi()
  const { execute: ctlEx   } = useApi()
  const { execute: saveEx  } = useApi()
  const { loading: deleting, execute: delEx } = useApi()

  const [mappings,    setMappings]    = useState([])
  const [showAdd,     setShowAdd]     = useState(false)
  const [frameworks,  setFrameworks]  = useState([])
  const [selFwId,     setSelFwId]     = useState('')
  const [fwControls,  setFwControls]  = useState([])
  const [selCtrlId,   setSelCtrlId]   = useState('')
  const [selStatus,   setSelStatus]   = useState('NotAssessed')
  const [notes,       setNotes]       = useState('')
  const [loadingCtls, setLoadingCtls] = useState(false)

  const load = () =>
    loadEx(() => regulatoryApi.getMappings(entityType, entityId), { silent: true })
      .then((r) => r && setMappings(r))

  useEffect(() => {
    if (entityId) load()
  }, [entityType, entityId])

  // Load frameworks when add panel opens
  useEffect(() => {
    if (!showAdd || frameworks.length) return
    fwEx(() => regulatoryApi.getFrameworks(), { silent: true }).then((r) => r && setFrameworks(r))
  }, [showAdd])

  // Load controls when framework is selected
  useEffect(() => {
    if (!selFwId) { setFwControls([]); return }
    setLoadingCtls(true)
    ctlEx(() => regulatoryApi.getControls(Number(selFwId)), { silent: true }).then((r) => {
      setFwControls(r ?? [])
      setLoadingCtls(false)
    })
  }, [selFwId])

  const handleAdd = async () => {
    if (!selCtrlId) return
    const ok = await saveEx(
      () => regulatoryApi.createMapping({
        regulatoryControlId: Number(selCtrlId),
        entityType,
        entityId,
        complianceStatus: selStatus,
        notes: notes || undefined,
      }),
      { successMsg: 'تم إضافة الربط.' }
    )
    if (ok !== null) {
      setShowAdd(false); setSelFwId(''); setSelCtrlId(''); setSelStatus('NotAssessed'); setNotes('')
      load()
    }
  }

  const handleDelete = async (id) => {
    const ok = await delEx(() => regulatoryApi.deleteMapping(id), { successMsg: 'تم حذف الربط.' })
    if (ok !== null) load()
  }

  const handleStatusChange = async (id, newStatus) => {
    await saveEx(() => regulatoryApi.updateMapping(id, { complianceStatus: newStatus }), { silent: true })
    load()
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold text-gray-700 text-sm">ربط الضوابط التنظيمية</h3>
        {canManage && (
          <button
            onClick={() => setShowAdd(!showAdd)}
            className="text-xs px-3 py-1.5 bg-green-700 text-white rounded-lg hover:bg-green-800"
          >
            {showAdd ? '✕ إلغاء' : '+ إضافة ربط'}
          </button>
        )}
      </div>

      {/* Add mapping form */}
      {showAdd && (
        <div className="border border-gray-200 rounded-lg p-4 space-y-3 bg-gray-50">
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">الإطار التنظيمي</label>
            <select value={selFwId} onChange={(e) => setSelFwId(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
              <option value="">-- اختر إطاراً --</option>
              {frameworks.map((f) => (
                <option key={f.id} value={f.id}>{f.nameAr}</option>
              ))}
            </select>
          </div>

          {selFwId && (
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">الضابط</label>
              {loadingCtls ? (
                <div className="flex items-center gap-2 text-xs text-gray-400"><Spinner size="sm" /> جارٍ التحميل...</div>
              ) : (
                <select value={selCtrlId} onChange={(e) => setSelCtrlId(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
                  <option value="">-- اختر ضابطاً --</option>
                  {fwControls.map((c) => (
                    <option key={c.id} value={c.id}>[{c.controlCode}] {c.titleAr}</option>
                  ))}
                </select>
              )}
            </div>
          )}

          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">حالة الامتثال</label>
            <select value={selStatus} onChange={(e) => setSelStatus(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600">
              {STATUSES.map((s) => (
                <option key={s} value={s}>{STATUS_LABEL_AR[s]}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">ملاحظات (اختياري)</label>
            <input value={notes} onChange={(e) => setNotes(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
              dir="rtl" />
          </div>

          <button onClick={handleAdd} disabled={!selCtrlId}
            className="w-full py-1.5 bg-green-700 text-white text-sm rounded-lg hover:bg-green-800 disabled:opacity-50">
            حفظ الربط
          </button>
        </div>
      )}

      {/* Existing mappings */}
      {mappings.length === 0 && !showAdd ? (
        <p className="text-xs text-gray-400 text-center py-4">لا توجد ضوابط تنظيمية مرتبطة.</p>
      ) : (
        <div className="space-y-2">
          {mappings.map((m) => (
            <div key={m.id} className="flex items-center gap-2 border border-gray-100 rounded-lg px-3 py-2">
              <span className="font-mono text-xs text-gray-500 flex-shrink-0">{m.controlCode}</span>
              <span className="text-xs text-gray-700 flex-1 truncate">{m.controlTitleAr}</span>

              {canManage ? (
                <select
                  value={m.complianceStatus}
                  onChange={(e) => handleStatusChange(m.id, e.target.value)}
                  className={`text-xs px-2 py-0.5 rounded-full border-0 font-medium cursor-pointer ${STATUS_STYLE[m.complianceStatus]}`}
                >
                  {STATUSES.map((s) => (
                    <option key={s} value={s}>{STATUS_LABEL_AR[s]}</option>
                  ))}
                </select>
              ) : (
                <span className={`text-xs px-2 py-0.5 rounded-full ${STATUS_STYLE[m.complianceStatus]}`}>
                  {STATUS_LABEL_AR[m.complianceStatus]}
                </span>
              )}

              {canManage && (
                <button onClick={() => handleDelete(m.id)} disabled={deleting}
                  className="text-red-400 hover:text-red-600 text-xs p-0.5 flex-shrink-0">
                  ✕
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
