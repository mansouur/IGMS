import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { departmentApi } from '../../services/departmentApi'
import { useApi } from '../../hooks/useApi'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ─── DepartmentDetail ─────────────────────────────────────────────────────────

export default function DepartmentDetail() {
  const { id }    = useParams()
  const navigate  = useNavigate()
  const { loading, execute } = useApi()
  const hasPermission = useAuthStore((s) => s.hasPermission)
  const canUpdate = hasPermission('DEPARTMENTS.UPDATE')

  const [dept, setDept] = useState(null)

  useEffect(() => {
    execute(() => departmentApi.getById(id), { silent: true })
      .then((data) => { if (data) setDept(data) })
  }, [id])

  if (loading || !dept) return <PageLoader />

  return (
    <div className="max-w-4xl space-y-6">

      {/* ── Header ─────────────────────────────────────── */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <button onClick={() => navigate('/departments')}
            className="mt-1 p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 flex-shrink-0">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <path d="M9 18l6-6-6-6" />
            </svg>
          </button>
          <div>
            <div className="flex items-center gap-2 flex-wrap">
              <h1 className="text-xl font-bold text-gray-800">{dept.nameAr}</h1>
              <span className={`text-xs font-semibold px-2 py-0.5 rounded-full
                ${dept.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
                {dept.isActive ? 'نشط' : 'معطل'}
              </span>
              <span className="font-mono text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                {dept.code}
              </span>
            </div>
            {dept.nameEn && (
              <p className="text-sm text-gray-400 mt-0.5" dir="ltr">{dept.nameEn}</p>
            )}
            {dept.parentNameAr && (
              <p className="text-sm text-gray-500 mt-1">
                القسم الأعلى: <span className="font-medium">{dept.parentNameAr}</span>
              </p>
            )}
            {dept.managerNameAr && (
              <p className="text-sm text-gray-500">
                المدير: <span className="font-medium">{dept.managerNameAr}</span>
              </p>
            )}
          </div>
        </div>

        {canUpdate && (
          <button onClick={() => navigate(`/departments/${id}/edit`)}
            className="px-3 py-1.5 text-sm border border-gray-300 text-gray-600
              rounded-lg hover:bg-gray-50 transition-colors flex-shrink-0">
            تعديل
          </button>
        )}
      </div>

      {/* ── Info Cards ─────────────────────────────────── */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <InfoCard label="المستوى" value={dept.level} />
        <InfoCard label="الأقسام الفرعية" value={dept.childCount} />
        <InfoCard label="الموظفين" value={dept.memberCount} />
        <InfoCard label="تاريخ الإنشاء" value={new Date(dept.createdAt).toLocaleDateString('ar-AE')} />
      </div>

      {/* ── Description ────────────────────────────────── */}
      {(dept.descriptionAr || dept.descriptionEn) && (
        <div className="bg-white rounded-xl border border-gray-200 p-4 space-y-2">
          {dept.descriptionAr && (
            <p className="text-sm text-gray-700 leading-relaxed">{dept.descriptionAr}</p>
          )}
          {dept.descriptionEn && (
            <p className="text-sm text-gray-500 leading-relaxed" dir="ltr">{dept.descriptionEn}</p>
          )}
        </div>
      )}

      {/* ── Sub-Departments ─────────────────────────────── */}
      {dept.children?.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <div className="px-5 py-3 border-b border-gray-100">
            <h2 className="text-sm font-semibold text-gray-700">
              الأقسام الفرعية ({dept.children.length})
            </h2>
          </div>
          <div className="divide-y divide-gray-100">
            {dept.children.map((child) => (
              <div key={child.id}
                className="flex items-center justify-between px-5 py-3 hover:bg-gray-50 cursor-pointer"
                onClick={() => navigate(`/departments/${child.id}`)}>
                <div>
                  <p className="text-sm font-medium text-green-700">{child.nameAr}</p>
                  {child.nameEn && <p className="text-xs text-gray-400" dir="ltr">{child.nameEn}</p>}
                </div>
                <div className="flex items-center gap-3 text-xs text-gray-500">
                  {child.managerNameAr && <span>{child.managerNameAr}</span>}
                  <span className={`px-2 py-0.5 rounded-full font-semibold
                    ${child.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
                    {child.isActive ? 'نشط' : 'معطل'}
                  </span>
                  <span className="font-mono bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded">
                    {child.code}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function InfoCard({ label, value }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4 text-center">
      <p className="text-2xl font-bold text-gray-800">{value}</p>
      <p className="text-xs text-gray-500 mt-1">{label}</p>
    </div>
  )
}
