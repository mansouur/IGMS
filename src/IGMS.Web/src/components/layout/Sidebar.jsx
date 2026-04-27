import { NavLink } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import useAuthStore from '../../store/authStore'

// ─── Icon components (inline SVG – no extra dependency) ──────────────────────

const Icon = ({ d, size = 20 }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
    <path d={d} />
  </svg>
)

const icons = {
  dashboard:   'M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z M9 22V12h6v10',
  users:       'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2 M23 21v-2a4 4 0 0 0-3-3.87 M16 3.13a4 4 0 0 1 0 7.75',
  departments: 'M3 3h18v18H3z M3 9h18 M9 21V9',
  raci:        'M9 11l3 3L22 4 M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11',
  policies:    'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6 M16 13H8 M16 17H8 M10 9H8',
  tasks:       'M9 11l3 3L22 4 M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11',
  kpi:         'M18 20V10 M12 20V4 M6 20v-6',
  risks:       'M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z M12 9v4 M12 17h.01',
  executive:  'M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5',
  workflows:  'M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 0 2-2h2a2 2 0 0 0 2 2m-6 9l2 2 4-4',
  compliance: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0 1 12 2.944a11.955 11.955 0 0 1-8.618 3.04A12.02 12.02 0 0 0 3 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z',
  approvals:  'M9 12l2 2 4-4m6 2a9 9 0 1 1-18 0 9 9 0 0 1 18 0z',
  assessments:'M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 0 2-2h2a2 2 0 0 0 2 2M12 12h.01M12 16h.01M8 12h.01M8 16h.01M16 12h.01M16 16h.01',
  incidents:  'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
  vendors:    'M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z M9 22V12h6v10',
  meetings:    'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2z',
  performance: 'M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z M2 9h4v12H2z M4 6a2 2 0 1 0 0-4 2 2 0 1 0 0 4z',
  pdpl:        'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z M9 12l2 2 4-4',
  guide:       'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253',
  rankings:    'M3 17l4-8 4 4 4-6 4 4M3 21h18',
  reports:    'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6 M12 18v-6 M9 15h6',
  controls:   'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0 1 12 2.944a11.955 11.955 0 0 1-8.618 3.04A12.02 12.02 0 0 0 3 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z',
  roles:      'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2 M9 7a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M23 21v-2a4 4 0 0 0-3-3.87 M16 3.13a4 4 0 0 1 0 7.75',
  auditLog:   'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z',
  settings:   'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z',
}

// ─── Navigation definition ────────────────────────────────────────────────────

/**
 * Each item declares the minimum permission required to appear.
 * null = always visible (Dashboard).
 * Settings requires the ADMIN role.
 */
const NAV_ITEMS = [
  { key: 'dashboard',   path: '/dashboard',   icon: 'dashboard',   permission: null,             section: 'main' },
  { key: 'users',       path: '/users',        icon: 'users',       permission: 'USERS.READ',     section: 'management' },
  { key: 'departments', path: '/departments',  icon: 'departments', permission: 'DEPARTMENTS.READ', section: 'management' },
  { key: 'raci',        path: '/raci',         icon: 'raci',        permission: 'RACI.READ',      section: 'management' },
  { key: 'policies',    path: '/policies',     icon: 'policies',    permission: 'POLICIES.READ',  section: 'management' },
  { key: 'tasks',       path: '/tasks',        icon: 'tasks',       permission: 'TASKS.READ',     section: 'management' },
  { key: 'assessments', path: '/assessments',  icon: 'assessments', permission: 'ASSESSMENTS.READ', section: 'analytics' },
  { key: 'incidents',   path: '/incidents',    icon: 'incidents',   permission: 'INCIDENTS.READ',   section: 'analytics' },
  { key: 'vendors',    path: '/vendors',      icon: 'vendors',     permission: 'VENDORS.READ',     section: 'analytics' },
  { key: 'meetings',   path: '/meetings',     icon: 'meetings',    permission: 'MEETINGS.READ',    section: 'analytics' },
  { key: 'performance',path: '/performance',  icon: 'performance', permission: 'PERFORMANCE.READ', section: 'analytics' },
  { key: 'pdpl',       path: '/pdpl',         icon: 'pdpl',        permission: 'PDPL.READ',        section: 'analytics' },
  { key: 'compliance',  path: '/compliance',   icon: 'compliance',  permission: 'COMPLIANCE.READ', section: 'analytics' },
  { key: 'kpi',         path: '/kpi',          icon: 'kpi',         permission: 'KPI.READ',       section: 'analytics' },
  { key: 'risks',       path: '/risks',        icon: 'risks',       permission: 'RISKS.READ',     section: 'analytics' },
  { key: 'controls',   path: '/controls',     icon: 'controls',    permission: 'CONTROLS.READ',  section: 'analytics' },
  { key: 'rankings',    path: '/rankings',     icon: 'rankings',    permission: 'REPORTS.READ',   section: 'analytics' },
  { key: 'executive',   path: '/executive',    icon: 'executive',   permission: 'REPORTS.READ',   section: 'analytics' },
  { key: 'reports',     path: '/reports',      icon: 'reports',     permission: 'REPORTS.READ',   section: 'analytics' },
  { key: 'workflows',  path: '/workflows',     icon: 'workflows',   permission: 'WORKFLOWS.MANAGE', section: 'admin' },
  { key: 'approvals',  path: '/approvals',     icon: 'approvals',   permission: 'WORKFLOWS.APPROVE', section: 'admin' },
  { key: 'roles',      path: '/roles',         icon: 'roles',       permission: 'USERS.MANAGE',    section: 'admin' },
  { key: 'auditLog',   path: '/audit-log',    icon: 'auditLog',    permission: 'AUDIT.READ',      section: 'admin' },
  { key: 'settings',   path: '/settings',     icon: 'settings',    permission: 'SETTINGS.UPDATE', section: 'admin' },
  { key: 'guide',      path: '/guide',        icon: 'guide',       permission: 'SETTINGS.UPDATE', section: 'admin' },
]

const SECTIONS = ['main', 'management', 'analytics', 'admin']

// ─── Component ────────────────────────────────────────────────────────────────

export default function Sidebar({ collapsed, onToggle, mobileOpen, onClose }) {
  const { t } = useTranslation()
  const hasPermission = useAuthStore((s) => s.hasPermission)

  const visibleItems = NAV_ITEMS.filter(
    (item) => item.permission === null || hasPermission(item.permission)
  )

  const itemsBySection = SECTIONS.reduce((acc, section) => {
    acc[section] = visibleItems.filter((i) => i.section === section)
    return acc
  }, {})

  return (
    <aside
      className={[
        'flex flex-col bg-white border-e border-gray-200 transition-transform duration-200 flex-shrink-0',
        // Mobile: fixed full-height drawer
        'fixed inset-y-0 start-0 z-40 h-screen',
        // Desktop: normal flow, always visible — !important يتغلب على ltr:/rtl: variants
        'md:relative md:inset-auto md:z-auto md:h-full md:!translate-x-0',
        // Mobile show/hide (لا تأثير على desktop بسبب md:!translate-x-0)
        mobileOpen ? 'translate-x-0' : 'ltr:-translate-x-full rtl:translate-x-full',
        // Width
        collapsed ? 'w-64 md:w-16' : 'w-64 md:w-56',
      ].join(' ')}
    >
      {/* Logo + collapse toggle */}
      <div className="flex items-center justify-between h-16 px-3 border-b border-gray-100 flex-shrink-0">
        <span className="text-sm font-bold text-green-700 truncate">
          {t('common.appShort')}
        </span>
        <div className="flex items-center gap-1 ms-auto">
          {/* Close button – mobile only */}
          <button
            onClick={onClose}
            className="md:hidden p-1.5 rounded-md text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
            aria-label="إغلاق القائمة"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
          {/* Collapse toggle – desktop only */}
          <button
            onClick={onToggle}
            className="hidden md:flex p-1.5 rounded-md text-gray-400 hover:bg-gray-100 hover:text-gray-600 transition-colors"
            aria-label={collapsed ? 'توسيع القائمة' : 'طي القائمة'}
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              {collapsed
                ? <path d="M9 18l6-6-6-6" />
                : <path d="M15 18l-6-6 6-6" />}
            </svg>
          </button>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto py-3 space-y-5">
        {SECTIONS.map((section) => {
          const items = itemsBySection[section]
          if (!items?.length) return null
          const showLabel = !collapsed || mobileOpen
          return (
            <div key={section}>
              {/* Section label */}
              {showLabel && (
                <p className="px-3 mb-1 text-[10px] font-semibold uppercase tracking-wider text-gray-400 select-none">
                  {t(`nav.section.${section}`)}
                </p>
              )}
              {items.map((item) => (
                <NavItem key={item.key} item={item} collapsed={collapsed && !mobileOpen} t={t} onClose={onClose} />
              ))}
            </div>
          )
        })}
      </nav>
    </aside>
  )
}

function NavItem({ item, collapsed, t, onClose }) {
  return (
    <NavLink
      to={item.path}
      title={collapsed ? t(`nav.${item.key}`) : undefined}
      onClick={onClose}
      className={({ isActive }) =>
        [
          'flex items-center gap-3 px-3 py-2 mx-2 rounded-lg text-sm font-medium transition-colors',
          isActive
            ? 'bg-green-50 text-green-700'
            : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900',
        ].join(' ')
      }
    >
      <span className="flex-shrink-0">
        <Icon d={icons[item.icon]} size={19} />
      </span>
      {!collapsed && (
        <span className="truncate">{t(`nav.${item.key}`)}</span>
      )}
    </NavLink>
  )
}
