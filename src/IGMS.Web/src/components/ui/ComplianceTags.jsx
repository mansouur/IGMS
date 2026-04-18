import { useState, useEffect, useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import { complianceApi } from '../../services/governanceApi'
import { toast } from '../../store/toastStore'
import { Spinner } from './Spinner'

// Framework options matching the C# enum
const FRAMEWORKS = [
  { value: 0, label: 'ISO 31000',  color: 'bg-blue-100 text-blue-800 border-blue-200',    group: 'دولي' },
  { value: 1, label: 'COBIT 2019', color: 'bg-purple-100 text-purple-800 border-purple-200', group: 'دولي' },
  { value: 2, label: 'UAE NESA',   color: 'bg-emerald-100 text-emerald-800 border-emerald-200', group: 'إماراتي' },
  { value: 3, label: 'ISO 27001',  color: 'bg-amber-100 text-amber-800 border-amber-200',  group: 'دولي' },
  { value: 4, label: 'NIAS UAE',   color: 'bg-rose-100 text-rose-800 border-rose-200',     group: 'إماراتي' },
  { value: 5, label: 'مخصص',       color: 'bg-gray-100 text-gray-700 border-gray-200',     group: 'أخرى' },
  { value: 6, label: 'ADAA',       color: 'bg-teal-100 text-teal-800 border-teal-200',     group: 'إماراتي' },
  { value: 7, label: 'TDRA',       color: 'bg-cyan-100 text-cyan-800 border-cyan-200',     group: 'إماراتي' },
  { value: 8, label: 'UAE IA',     color: 'bg-indigo-100 text-indigo-800 border-indigo-200', group: 'إماراتي' },
  { value: 9, label: 'DSM',        color: 'bg-sky-100 text-sky-800 border-sky-200',        group: 'إماراتي' },
]

const FW_MAP = Object.fromEntries(FRAMEWORKS.map((f) => [f.value, f]))
const fwColor = (fw) => FW_MAP[fw]?.color ?? FW_MAP[5].color
const fwLabel = (fw) => FW_MAP[fw]?.label ?? 'مخصص'

export default function ComplianceTags({ entityType, entityId, canEdit = false }) {
  const { t } = useTranslation()
  const [items,    setItems]    = useState([])
  const [loading,  setLoading]  = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [saving,   setSaving]   = useState(false)
  const [deleting, setDeleting] = useState(null)
  const [form, setForm] = useState({ framework: 0, clause: '', notes: '' })

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const r = await complianceApi.getByEntity(entityType, entityId)
      setItems(r.data?.data ?? [])
    } catch { /* silent */ }
    finally { setLoading(false) }
  }, [entityType, entityId])

  useEffect(() => { load() }, [load])

  const handleAdd = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      const r = await complianceApi.add({
        entityType,
        entityId,
        framework: Number(form.framework),
        clause:    form.clause || null,
        notes:     form.notes  || null,
      })
      if (r.data?.data) {
        setItems((prev) => [...prev, r.data.data])
        setShowForm(false)
        setForm({ framework: 0, clause: '', notes: '' })
        toast.success(t('compliance.added'))
      }
    } catch (err) {
      toast.error(err?.response?.data?.message ?? t('compliance.addError'))
    } finally { setSaving(false) }
  }

  const handleDelete = async (id) => {
    setDeleting(id)
    try {
      await complianceApi.delete(id)
      setItems((prev) => prev.filter((x) => x.id !== id))
      toast.success(t('compliance.deleted'))
    } catch {
      toast.error(t('compliance.deleteError'))
    } finally { setDeleting(null) }
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-sm font-semibold text-gray-700">{t('compliance.title')}</h2>
          <p className="text-xs text-gray-400 mt-0.5">{t('compliance.subtitle')}</p>
        </div>
        {canEdit && (
          <button
            onClick={() => setShowForm((v) => !v)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs bg-green-700 text-white rounded-lg hover:bg-green-800"
          >
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
            {t('compliance.add')}
          </button>
        )}
      </div>

      {/* Add form */}
      {showForm && (
        <form onSubmit={handleAdd} className="mb-4 p-4 bg-gray-50 rounded-lg border border-gray-200 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('compliance.framework')}</label>
              <select
                value={form.framework}
                onChange={(e) => setForm((f) => ({ ...f, framework: e.target.value }))}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600 bg-white"
              >
                {FRAMEWORKS.map((fw) => (
                  <option key={fw.value} value={fw.value}>{fw.label}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('compliance.clause')}</label>
              <input
                type="text"
                placeholder={t('compliance.clausePlaceholder')}
                value={form.clause}
                onChange={(e) => setForm((f) => ({ ...f, clause: e.target.value }))}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
              />
            </div>
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 mb-1">{t('compliance.notes')}</label>
            <input
              type="text"
              placeholder={t('compliance.notesPlaceholder')}
              value={form.notes}
              onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
            />
          </div>
          <div className="flex justify-end gap-2 pt-1">
            <button type="button" onClick={() => setShowForm(false)}
              className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-100">
              {t('common.cancel')}
            </button>
            <button type="submit" disabled={saving}
              className="px-3 py-1.5 text-sm bg-green-700 text-white rounded-lg hover:bg-green-800 disabled:opacity-50 flex items-center gap-2">
              {saving && <Spinner size="sm" />}
              {t('compliance.save')}
            </button>
          </div>
        </form>
      )}

      {/* Tags */}
      {loading ? (
        <div className="flex justify-center py-4"><Spinner size="sm" /></div>
      ) : items.length === 0 ? (
        <p className="text-sm text-gray-400 text-center py-4">{t('compliance.empty')}</p>
      ) : (
        <div className="flex flex-wrap gap-2">
          {items.map((item) => (
            <div
              key={item.id}
              className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium border ${fwColor(item.framework)}`}
              title={item.notes || ''}
            >
              <span className="font-semibold">{fwLabel(item.framework)}</span>
              {item.clause && <span className="opacity-75">· {item.clause}</span>}
              {canEdit && (
                <button
                  onClick={() => handleDelete(item.id)}
                  disabled={deleting === item.id}
                  className="ms-1 opacity-60 hover:opacity-100 transition-opacity"
                >
                  {deleting === item.id
                    ? <Spinner size="sm" />
                    : <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                  }
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
