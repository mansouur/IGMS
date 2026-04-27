import { useState, useEffect } from 'react'
import api from '../../services/api'
import { PageLoader } from '../../components/ui/Spinner'
import useAuthStore from '../../store/authStore'

// ── شريط الدرجة الملوّن ──────────────────────────────────────────────────────
function ScoreBar({ value, color = 'bg-green-500' }) {
  const pct = Math.min(Math.max(value ?? 0), 100)
  return (
    <div className="w-full bg-gray-100 rounded-full h-1.5 mt-1">
      <div className={`h-1.5 rounded-full transition-all ${color}`} style={{ width: `${pct}%` }} />
    </div>
  )
}

function scoreColor(v) {
  if (v >= 80) return 'text-emerald-600'
  if (v >= 60) return 'text-amber-500'
  return 'text-red-500'
}
function barColor(v) {
  if (v >= 80) return 'bg-emerald-500'
  if (v >= 60) return 'bg-amber-400'
  return 'bg-red-400'
}

// ── شارة الترتيب ─────────────────────────────────────────────────────────────
function RankBadge({ rank, total, isPhantom }) {
  const colors = [
    '', // فارغ
    'bg-yellow-400 text-yellow-900',   // 1
    'bg-gray-300 text-gray-700',       // 2
    'bg-amber-600 text-amber-100',     // 3
  ]
  const cls = rank <= 3 ? colors[rank] : 'bg-blue-100 text-blue-700'
  return (
    <div className="flex flex-col items-center gap-0.5">
      <span className={`w-10 h-10 rounded-full flex items-center justify-center font-black text-lg ${cls}`}>
        {rank}
      </span>
      <span className="text-[10px] text-gray-400">من {total}</span>
      {isPhantom && (
        <span className="text-[9px] text-purple-400 font-medium">الأعلى!</span>
      )}
    </div>
  )
}

// ── محور التفاصيل ─────────────────────────────────────────────────────────────
function PillarRow({ label, value }) {
  return (
    <div>
      <div className="flex justify-between text-xs text-gray-500 mb-0.5">
        <span>{label}</span>
        <span className={`font-semibold ${scoreColor(value)}`}>{value}%</span>
      </div>
      <ScoreBar value={value} color={barColor(value)} />
    </div>
  )
}

// ── بطاقة قسم ────────────────────────────────────────────────────────────────
function DeptCard({ dept, isAdmin }) {
  const [expanded, setExpanded] = useState(false)
  const isPhantom = !isAdmin && dept.displayRank === 2 && dept.trueRank === 1

  return (
    <div className={`bg-white rounded-xl border-2 p-5 transition-all ${
      dept.isCurrentUserDept ? 'border-green-400 shadow-md' : 'border-gray-200'
    }`}>
      <div className="flex items-start gap-4">
        <RankBadge rank={dept.displayRank} total={dept.totalDepartments} isPhantom={isPhantom} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between gap-2 flex-wrap">
            <div>
              <p className="font-bold text-gray-800 text-sm">{dept.departmentNameAr}</p>
              <p className="text-xs text-gray-400">{dept.memberCount} موظف</p>
            </div>
            <div className="text-end">
              <span className={`text-3xl font-black ${scoreColor(dept.overallScore)}`}>
                {dept.overallScore}
              </span>
              <span className="text-xs text-gray-400 ms-1">/ 100</span>
            </div>
          </div>
          <ScoreBar value={dept.overallScore} color={barColor(dept.overallScore)} />

          {/* توسيع للتفاصيل */}
          <button
            onClick={() => setExpanded(v => !v)}
            className="mt-2 text-xs text-blue-500 hover:underline"
          >
            {expanded ? 'إخفاء التفاصيل' : 'عرض التفاصيل'}
          </button>

          {expanded && (
            <div className="mt-3 space-y-2 border-t pt-3">
              <PillarRow label="إنجاز المهام"        value={dept.tasksScore} />
              <PillarRow label="تحقق المؤشرات (KPI)" value={dept.kpisScore} />
              <PillarRow label="معالجة المخاطر"      value={dept.risksScore} />
              <PillarRow label="إقرار السياسات"      value={dept.policiesScore} />
              <PillarRow label="حل الحوادث"          value={dept.incidentsScore} />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

// ── بطاقة موظف ───────────────────────────────────────────────────────────────
function EmpCard({ emp, isAdmin }) {
  const [expanded, setExpanded] = useState(false)
  const isPhantom = !isAdmin && emp.displayRank === 2 && emp.trueRank === 1

  return (
    <div className={`bg-white rounded-xl border-2 p-4 transition-all ${
      emp.isCurrentUser ? 'border-green-400 shadow-md' : 'border-gray-200'
    }`}>
      <div className="flex items-start gap-3">
        <RankBadge rank={emp.displayRank} total={emp.totalInScope} isPhantom={isPhantom} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between gap-2 flex-wrap">
            <div>
              <p className="font-bold text-gray-800 text-sm">{emp.fullNameAr}</p>
              <p className="text-xs text-gray-400">{emp.departmentNameAr}</p>
            </div>
            <div className="text-end">
              <span className={`text-2xl font-black ${scoreColor(emp.overallScore)}`}>
                {emp.overallScore}
              </span>
              <span className="text-xs text-gray-400 ms-1">/ 100</span>
            </div>
          </div>
          <ScoreBar value={emp.overallScore} color={barColor(emp.overallScore)} />

          <button
            onClick={() => setExpanded(v => !v)}
            className="mt-2 text-xs text-blue-500 hover:underline"
          >
            {expanded ? 'إخفاء التفاصيل' : 'عرض التفاصيل'}
          </button>

          {expanded && (
            <div className="mt-3 space-y-2 border-t pt-3">
              <PillarRow label="إنجاز المهام"       value={emp.tasksScore} />
              <PillarRow label="إقرار السياسات"     value={emp.policiesScore} />
              <PillarRow label="بنود الاجتماعات"    value={emp.meetingActionsScore} />
              <PillarRow label="حل الحوادث"         value={emp.incidentsScore} />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

// ── بطاقة درجتي الشخصية ──────────────────────────────────────────────────────
function MyScoreCard({ my }) {
  if (!my) return null
  const emp  = my.employeeRank
  const dept = my.departmentRank

  return (
    <div className="bg-gradient-to-l from-green-50 to-emerald-50 rounded-2xl border-2 border-green-300 p-6 space-y-4">
      <h2 className="text-sm font-bold text-green-800">درجتك الشخصية</h2>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        {[
          { label: 'إنجاز المهام',      val: emp?.tasksScore,          total: my.tasksTotal,          done: my.tasksDone          },
          { label: 'إقرار السياسات',    val: emp?.policiesScore,        total: my.policiesTotal,       done: my.policiesAcked       },
          { label: 'بنود الاجتماعات',   val: emp?.meetingActionsScore,  total: my.meetingActionsTotal, done: my.meetingActionsDone  },
          { label: 'حل الحوادث',        val: emp?.incidentsScore,       total: my.incidentsTotal,      done: my.incidentsResolved   },
        ].map(({ label, val, total, done }) => (
          <div key={label} className="bg-white rounded-xl p-3 border border-green-100">
            <p className="text-[11px] text-gray-500 mb-1">{label}</p>
            <p className={`text-2xl font-black ${scoreColor(val ?? 0)}`}>{val ?? 0}%</p>
            <p className="text-[10px] text-gray-400 mt-0.5">{done}/{total}</p>
            <ScoreBar value={val ?? 0} color={barColor(val ?? 0)} />
          </div>
        ))}
      </div>

      <div className="flex flex-wrap gap-4 pt-2 border-t border-green-200">
        {emp && (
          <div className="text-sm">
            <span className="text-gray-500">ترتيبك بين الموظفين: </span>
            <span className="font-bold text-green-700">#{emp.displayRank}</span>
            <span className="text-gray-400"> من {emp.totalInScope}</span>
          </div>
        )}
        {dept && (
          <div className="text-sm">
            <span className="text-gray-500">ترتيب قسمك: </span>
            <span className="font-bold text-green-700">#{dept.displayRank}</span>
            <span className="text-gray-400"> من {dept.totalDepartments}</span>
          </div>
        )}
      </div>
    </div>
  )
}

// ── الصفحة الرئيسية ──────────────────────────────────────────────────────────
export default function RankingDashboard() {
  const { roles } = useAuthStore()
  const isAdmin   = roles.includes('ADMIN')

  const [tab,        setTab]        = useState('departments') // departments | employees
  const [deptData,   setDeptData]   = useState(null)
  const [empData,    setEmpData]    = useState(null)
  const [myScore,    setMyScore]    = useState(null)
  const [loading,    setLoading]    = useState(true)

  useEffect(() => {
    const load = async () => {
      try {
        const [d, e, m] = await Promise.all([
          api.get('/api/v1/rankings/departments'),
          api.get('/api/v1/rankings/employees'),
          api.get('/api/v1/rankings/my-score'),
        ])
        setDeptData(d.data?.data)
        setEmpData(e.data?.data)
        setMyScore(m.data?.data)
      } catch {}
      setLoading(false)
    }
    load()
  }, [])

  if (loading) return <PageLoader />

  const depts = deptData?.rankings ?? []
  const emps  = empData?.rankings  ?? []

  return (
    <div className="space-y-6 max-w-4xl">

      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-gray-800">لوحة الأداء والترتيب</h1>
        <p className="text-sm text-gray-500 mt-1">
          القياس يشمل كل ما يرتبط بالقسم أو الموظف فعلياً — مهام، مؤشرات، مخاطر، سياسات، حوادث، واجتماعات
        </p>
      </div>

      {/* درجتي الشخصية — لكل المستخدمين */}
      <MyScoreCard my={myScore} />

      {/* تبويب الأقسام / الموظفين */}
      <div className="flex border-b border-gray-200">
        {[
          { key: 'departments', label: `ترتيب الأقسام${isAdmin ? ` (${depts.length})` : ''}` },
          { key: 'employees',   label: `${empData?.scopeLabel ?? 'الموظفون'}${empData ? ` (${emps.length})` : ''}` },
        ].map(({ key, label }) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            className={[
              'px-5 py-2.5 text-sm font-medium border-b-2 transition-colors',
              tab === key
                ? 'border-green-600 text-green-700'
                : 'border-transparent text-gray-500 hover:text-gray-700',
            ].join(' ')}
          >
            {label}
          </button>
        ))}
      </div>

      {/* ملاحظة الشفافية */}
      {!isAdmin && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 text-xs text-amber-700 flex items-start gap-2">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="mt-0.5 flex-shrink-0">
            <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          <span>
            <strong>ملاحظة:</strong> الترتيب المعروض يهدف للتحفيز المستمر — يتجدد تلقائياً مع كل إنجاز جديد.
            درجتك الشخصية مستقلة عن أداء القسم وتعكس نشاطك الفردي فقط.
          </span>
        </div>
      )}

      {/* قائمة الترتيب */}
      <div className="space-y-3">
        {tab === 'departments'
          ? depts.length === 0
            ? <p className="text-center text-gray-400 py-8">لا توجد بيانات</p>
            : depts.map(d => <DeptCard key={d.departmentId} dept={d} isAdmin={isAdmin} />)
          : emps.length === 0
            ? <p className="text-center text-gray-400 py-8">لا توجد بيانات</p>
            : emps.map(e => <EmpCard key={e.userId} emp={e} isAdmin={isAdmin} />)
        }
      </div>

      {/* أسلوب الحساب — شفافية للمستخدمين */}
      <details className="bg-gray-50 rounded-xl border border-gray-200">
        <summary className="px-5 py-3 text-sm font-medium text-gray-600 cursor-pointer select-none">
          كيف تُحسب الدرجات؟
        </summary>
        <div className="px-5 pb-4 space-y-3 text-xs text-gray-600">
          <div>
            <p className="font-semibold text-gray-700 mb-1">درجة القسم (مجموع 100%)</p>
            <ul className="space-y-0.5 text-gray-500">
              <li>• المؤشرات (KPI) المرتبطة بالقسم — 25%</li>
              <li>• إنجاز المهام المسندة لأعضاء القسم — 20%</li>
              <li>• معالجة المخاطر المملوكة للقسم — 20%</li>
              <li>• إقرار أعضاء القسم بالسياسات النشطة — 20%</li>
              <li>• حل الحوادث المرتبطة بالقسم — 15%</li>
            </ul>
          </div>
          <div>
            <p className="font-semibold text-gray-700 mb-1">درجة الموظف (مجموع 100%)</p>
            <ul className="space-y-0.5 text-gray-500">
              <li>• إنجاز المهام المسندة إليه — 35%</li>
              <li>• الإقرار بالسياسات النشطة — 25%</li>
              <li>• إنجاز بنود الاجتماعات المسندة إليه — 20%</li>
              <li>• حل الحوادث المبلّغة منه — 20%</li>
            </ul>
          </div>
        </div>
      </details>
    </div>
  )
}
