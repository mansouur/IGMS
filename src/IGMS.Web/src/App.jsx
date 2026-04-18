import { Routes, Route, Navigate } from 'react-router-dom'
import useAuthStore from './store/authStore'
import AppLayout   from './components/layout/AppLayout'
import Login       from './pages/Login'
import AuthCallback from './pages/AuthCallback'
import Dashboard   from './pages/Dashboard'
import RaciList          from './pages/Raci/RaciList'
import RaciForm          from './pages/Raci/RaciForm'
import RaciDetail        from './pages/Raci/RaciDetail'
import DepartmentList    from './pages/Departments/DepartmentList'
import DepartmentForm    from './pages/Departments/DepartmentForm'
import DepartmentDetail  from './pages/Departments/DepartmentDetail'
import UserList          from './pages/Users/UserList'
import UserForm          from './pages/Users/UserForm'
import PolicyList        from './pages/Policies/PolicyList'
import PolicyForm        from './pages/Policies/PolicyForm'
import PolicyDetail      from './pages/Policies/PolicyDetail'
import RiskList          from './pages/Risks/RiskList'
import RiskForm          from './pages/Risks/RiskForm'
import RiskDetail        from './pages/Risks/RiskDetail'
import RoleList          from './pages/Roles/RoleList'
import RoleDetail        from './pages/Roles/RoleDetail'
import ControlTestList   from './pages/Controls/ControlTestList'
import ControlTestForm   from './pages/Controls/ControlTestForm'
import ControlTestDetail from './pages/Controls/ControlTestDetail'
import TaskList          from './pages/Tasks/TaskList'
import TaskForm          from './pages/Tasks/TaskForm'
import TaskDetail        from './pages/Tasks/TaskDetail'
import KpiList           from './pages/Kpi/KpiList'
import KpiForm           from './pages/Kpi/KpiForm'
import KpiDetail         from './pages/Kpi/KpiDetail'
import Reports                from './pages/Reports/Reports'
import DepartmentScorecard   from './pages/Reports/DepartmentScorecard'
import ExecutiveDashboard    from './pages/Executive/ExecutiveDashboard'
import Settings          from './pages/Settings/Settings'
import AuditLogPage      from './pages/AuditLog/AuditLogPage'
import ComplianceLibrary from './pages/Compliance/ComplianceLibrary'
import WorkflowList     from './pages/Workflows/WorkflowList'
import WorkflowDetail   from './pages/Workflows/WorkflowDetail'
import ApprovalInbox    from './pages/Workflows/ApprovalInbox'
import AssessmentList   from './pages/Assessments/AssessmentList'
import AssessmentForm   from './pages/Assessments/AssessmentForm'
import AssessmentDetail from './pages/Assessments/AssessmentDetail'
import AssessmentRespond from './pages/Assessments/AssessmentRespond'
import AssessmentReport  from './pages/Assessments/AssessmentReport'
import IncidentList   from './pages/Incidents/IncidentList'
import IncidentForm   from './pages/Incidents/IncidentForm'
import IncidentDetail from './pages/Incidents/IncidentDetail'

// ── Route guards ──────────────────────────────────────────────────────────────

/** Redirect unauthenticated users to /login */
function ProtectedRoute({ children }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  return isAuthenticated ? children : <Navigate to="/login" replace />
}

/** Redirect already-authenticated users away from /login */
function GuestRoute({ children }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : children
}

// ── Placeholder page – shown for modules not yet built ────────────────────────
function ComingSoon({ name }) {
  return (
    <div className="flex flex-col items-center justify-center h-full text-center py-24">
      <p className="text-4xl mb-4">🚧</p>
      <p className="text-lg font-semibold text-gray-600">{name}</p>
      <p className="text-sm text-gray-400 mt-1">قيد التطوير – قريباً</p>
    </div>
  )
}

// ── Application routes ────────────────────────────────────────────────────────

export default function App() {
  return (
    <Routes>
      {/* Public */}
      <Route path="/login"          element={<GuestRoute><Login /></GuestRoute>} />
      <Route path="/auth/callback"  element={<AuthCallback />} />

      {/* Protected – wrapped in AppLayout (Sidebar + TopBar) */}
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route path="/dashboard"   element={<Dashboard />} />
        <Route path="/users"               element={<UserList />} />
        <Route path="/users/new"           element={<UserForm />} />
        <Route path="/users/:id/edit"      element={<UserForm />} />
        <Route path="/departments"         element={<DepartmentList />} />
        <Route path="/departments/new"     element={<DepartmentForm />} />
        <Route path="/departments/:id"     element={<DepartmentDetail />} />
        <Route path="/departments/:id/edit" element={<DepartmentForm />} />
        <Route path="/raci"            element={<RaciList />} />
        <Route path="/raci/new"        element={<RaciForm />} />
        <Route path="/raci/:id"        element={<RaciDetail />} />
        <Route path="/raci/:id/edit"   element={<RaciForm />} />
        <Route path="/policies"          element={<PolicyList />} />
        <Route path="/policies/new"      element={<PolicyForm />} />
        <Route path="/policies/:id"      element={<PolicyDetail />} />
        <Route path="/policies/:id/edit" element={<PolicyForm />} />
        <Route path="/risks"             element={<RiskList />} />
        <Route path="/risks/new"         element={<RiskForm />} />
        <Route path="/risks/:id"         element={<RiskDetail />} />
        <Route path="/risks/:id/edit"    element={<RiskForm />} />
        <Route path="/roles"             element={<RoleList />} />
        <Route path="/roles/:id"         element={<RoleDetail />} />
        <Route path="/controls"          element={<ControlTestList />} />
        <Route path="/controls/new"      element={<ControlTestForm />} />
        <Route path="/controls/:id"      element={<ControlTestDetail />} />
        <Route path="/controls/:id/edit" element={<ControlTestForm />} />
        <Route path="/tasks"             element={<TaskList />} />
        <Route path="/tasks/new"         element={<TaskForm />} />
        <Route path="/tasks/:id"         element={<TaskDetail />} />
        <Route path="/tasks/:id/edit"    element={<TaskForm />} />
        <Route path="/kpi"               element={<KpiList />} />
        <Route path="/kpi/new"           element={<KpiForm />} />
        <Route path="/kpi/:id"           element={<KpiDetail />} />
        <Route path="/kpi/:id/edit"      element={<KpiForm />} />
        <Route path="/executive"                      element={<ExecutiveDashboard />} />
        <Route path="/reports"                    element={<Reports />} />
        <Route path="/reports/department-scorecard" element={<DepartmentScorecard />} />
        <Route path="/compliance"        element={<ComplianceLibrary />} />
        <Route path="/workflows"         element={<WorkflowList />} />
        <Route path="/workflows/new"     element={<WorkflowDetail />} />
        <Route path="/workflows/:id"     element={<WorkflowDetail />} />
        <Route path="/approvals"              element={<ApprovalInbox />} />
        <Route path="/assessments"            element={<AssessmentList />} />
        <Route path="/assessments/new"        element={<AssessmentForm />} />
        <Route path="/assessments/:id"        element={<AssessmentDetail />} />
        <Route path="/assessments/:id/edit"   element={<AssessmentForm />} />
        <Route path="/assessments/:id/respond" element={<AssessmentRespond />} />
        <Route path="/assessments/:id/report"  element={<AssessmentReport />} />
        <Route path="/incidents"           element={<IncidentList />} />
        <Route path="/incidents/new"       element={<IncidentForm />} />
        <Route path="/incidents/:id"       element={<IncidentDetail />} />
        <Route path="/incidents/:id/edit"  element={<IncidentForm />} />
        <Route path="/audit-log"   element={<AuditLogPage />} />
        <Route path="/settings"    element={<Settings />} />
      </Route>

      {/* Default */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}
