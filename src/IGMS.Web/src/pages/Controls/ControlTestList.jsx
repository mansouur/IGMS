import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { controlTestApi } from '../../services/governanceApi'
import { useApi, useConfirm } from '../../hooks/useApi'
import { SkeletonTable } from '../../components/ui/Skeleton'
import Pagination from '../../components/ui/Pagination'
import useAuthStore from '../../store/authStore'
import { toast } from '../../store/toastStore'

const EFF_COLOR = {
  0: 'bg-gray-100 text-gray-600',
  1: 'bg-emerald-100 text-emerald-700',
  2: 'bg-amber-100 text-amber-700',
  3: 'bg-red-100 text-red-700',
}

const ShieldIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
  </svg>
)

export default function ControlTestList() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const confirm  = useConfirm()
  const { loading, execute } = useApi()
  const canCreate = useAuthStore((s) => s.hasPermission)('CONTROLS.CREATE')
  const canUpdate = useAuthStore((s) => s.hasPermission)('CONTROLS.UPDATE')
  const canDelete = useAuthStore((s) => s.hasPermission)('CONTROLS.DELETE')

  const EFF_LABEL = {
    0: t('controls.effectiveness.0'),
    1: t('controls.effectiveness.1'),
    2: t('controls.effectiveness.2'),
    3: t('controls.effectiveness.3'),
  }

  const [data,        setData]        = useState({ items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 10 })
  const [search,      setSearch]      = useState('')
  const [entityType,  setEntityType]  = useState('')
  const [effectiveness, setEff]       = useState('')
  const [page,        setPage]        = useState(1)
  const [pageSize,    setPageSize]    = useState(10)

  const fetchData = useCallback(async () => {
    const r = await execute(() => controlTestApi.getAll({
      page, pageSize,
      search:        search      || undefined,
      entityType:    entityType  || undefined,
      effectiveness: effectiveness !== '' ? effectiveness : undefined,
    }), { silent: true })
    if (r) setData(r)
  }, [page, pageSize, search, entityType, effectiveness])

  useEffect(() => { fetchData() }, [fetchData])

  const handleDelete = async (id, title) => {
    const ok = await confirm({
      title:   t('controls.confirm.deleteTitle'),
      message: t('controls.confirm.deleteMsg', { name: title }),
      variant: 'danger',
    })
    if (!ok) return
    const r = await execute(() => controlTestApi.delete(id), { successMsg: t('controls.messages.deleted') })
    if (r !== null) fetchData()
  }

  return (
    <div className="space-y-5 max-w-6xl">
      {/* ── Header ── */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-800">{t('controls.title')}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{t('controls.subtitle')}</p>
        </div>
        {canCreate && (
          <button
            onClick={() => navigate('/controls/new')}
            className="flex items-center gap-2 px-4 py-2 bg-green-700 text-white text-sm font-medium rounded-lg hover:bg-green-800"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M12 5v14M5 12h14"/></svg>
            {t('controls.new')}
          </button>
        )}
      </div>

      {/* ── Filters ── */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <form onSubmit={(e) => { e.preventDefault(); setPage(1) }} className="flex gap-3 flex-wrap">
          <input
            type="text" value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t('common.search')}
            className="flex-1 min-w-48 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
          />
          <select
            value={entityType}
            onChange={(e) => { setEntityType(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
          >
            <option value="">{t('controls.filter.allTypes')}</option>
            <option value="Policy">{t('controls.form.entityPolicy')}</option>
            <option value="Risk">{t('controls.form.entityRisk')}</option>
          </select>
          <select
            value={effectiveness}
            onChange={(e) => { setEff(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
          >
            <option value="">{t('controls.filter.allEffectiveness')}</option>
            <option value="0">{t('controls.effectiveness.0')}</option>
            <option value="1">{t('controls.effectiveness.1')}</option>
            <option value="2">{t('controls.effectiveness.2')}</option>
            <option value="3">{t('controls.effectiveness.3')}</option>
          </select>
          <button type="submit" className="px-4 py-2 bg-gray-100 text-gray-700 text-sm rounded-lg hover:bg-gray-200">
            {t('common.search')}
          </button>
        </form>
      </div>

      {/* ── Table ── */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        {loading ? <SkeletonTable rows={5} cols={6} /> : data.items.length === 0 ? (
          <div className="flex flex-col items-center py-16 text-gray-400">
            <ShieldIcon />
            <p className="text-sm mt-2">{t('controls.noData')}</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-xs text-gray-500 uppercase border-b border-gray-100">
                  <tr>
                    <th className="py-3 px-4 text-start">{t('controls.table.test')}</th>
                    <th className="py-3 px-4 text-start">{t('controls.table.entity')}</th>
                    <th className="py-3 px-4 text-center">{t('controls.table.effectiveness')}</th>
                    <th className="py-3 px-4 text-center">{t('controls.table.testedAt')}</th>
                    <th className="py-3 px-4 text-center">{t('controls.table.evidence')}</th>
                    <th className="py-3 px-4 text-center">{t('common.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <tr key={row.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4">
                        <button onClick={() => navigate(`/controls/${row.id}`)} className="text-start hover:underline">
                          <p className="font-medium text-blue-800">{row.titleAr}</p>
                          <p className="text-xs text-gray-400 font-mono">{row.code}</p>
                        </button>
                      </td>
                      <td className="py-3 px-4">
                        <p className="text-xs font-medium text-gray-500">{row.entityType === 'Policy' ? t('controls.form.entityPolicy') : t('controls.form.entityRisk')}</p>
                        <p className="text-xs text-gray-700 truncate max-w-40">{row.entityTitleAr ?? `#${row.entityId}`}</p>
                      </td>
                      <td className="py-3 px-4 text-center">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold ${EFF_COLOR[row.effectiveness]}`}>
                          {EFF_LABEL[row.effectiveness]}
                        </span>
                      </td>
                      <td className="py-3 px-4 text-center text-gray-600 text-xs">
                        {row.testedAt ? new Date(row.testedAt).toLocaleDateString('ar-AE') : '—'}
                      </td>
                      <td className="py-3 px-4 text-center">
                        <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${row.evidenceCount > 0 ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500'}`}>
                          <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                          {row.evidenceCount}
                        </span>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center justify-center gap-1">
                          <button onClick={() => navigate(`/controls/${row.id}`)} title={t('common.view')} className="p-1.5 rounded-md text-gray-600 hover:bg-gray-100">
                            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                          </button>
                          {canUpdate && (
                            <button onClick={() => navigate(`/controls/${row.id}/edit`)} title={t('common.edit')} className="p-1.5 rounded-md text-blue-600 hover:bg-blue-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                          )}
                          {canDelete && (
                            <button onClick={() => handleDelete(row.id, row.titleAr)} title={t('common.delete')} className="p-1.5 rounded-md text-red-600 hover:bg-red-50">
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6M10 11v6M14 11v6M9 6V4h6v2"/></svg>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="border-t border-gray-100 px-4">
              <Pagination
                currentPage={data.currentPage} totalPages={data.totalPages}
                totalCount={data.totalCount} pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={(s) => { setPageSize(s); setPage(1) }}
              />
            </div>
          </>
        )}
      </div>
    </div>
  )
}
