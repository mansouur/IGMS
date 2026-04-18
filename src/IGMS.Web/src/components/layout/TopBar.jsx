import { useState, useRef, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import useAuthStore from '../../store/authStore'
import { authApi, notificationsApi } from '../../services/api'

// ── Severity colours ──────────────────────────────────────────────────────────
const SEV = {
  high:   { dot: 'bg-red-500',    badge: 'bg-red-50 text-red-700 border-red-200'    },
  medium: { dot: 'bg-amber-400',  badge: 'bg-amber-50 text-amber-700 border-amber-200' },
  low:    { dot: 'bg-blue-400',   badge: 'bg-blue-50 text-blue-700 border-blue-200'  },
}

const TYPE_ICON = {
  overdue_task:           'M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z M12 9v4 M12 17h.01',
  task_due_soon:          'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z',
  pending_acknowledgment: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6',
  policy_expiring:        'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z',
  critical_risk:          'M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z M12 9v4 M12 17h.01',
}

export default function TopBar({ onMenuClick }) {
  const { t, i18n } = useTranslation()
  const navigate    = useNavigate()
  const { username, fullNameAr, fullNameEn, language, sessionId, logout } = useAuthStore()

  const [menuOpen,  setMenuOpen]  = useState(false)
  const [bellOpen,  setBellOpen]  = useState(false)
  const [notifs,    setNotifs]    = useState([])
  const [unread,    setUnread]    = useState(0)
  const [notifLoad, setNotifLoad] = useState(false)

  const menuRef = useRef(null)
  const bellRef = useRef(null)

  const displayName = language === 'ar' ? (fullNameAr || username) : (fullNameEn || username)
  const initials    = displayName
    ? displayName.split(' ').slice(0, 2).map((w) => w[0]).join('').toUpperCase()
    : '؟'

  // ── Fetch notifications ────────────────────────────────────────────────────
  const fetchNotifs = useCallback(async () => {
    try {
      setNotifLoad(true)
      const res = await notificationsApi.getMyNotifications()
      const data = res.data.data
      setNotifs(data.notifications ?? [])
      setUnread(data.unread ?? 0)
    } catch {
      // fail silently
    } finally {
      setNotifLoad(false)
    }
  }, [])

  useEffect(() => {
    fetchNotifs()
    const interval = setInterval(fetchNotifs, 5 * 60 * 1000) // كل 5 دقائق
    return () => clearInterval(interval)
  }, [fetchNotifs])

  // ── Close dropdowns on outside click ─────────────────────────────────────
  useEffect(() => {
    const handler = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) setMenuOpen(false)
      if (bellRef.current && !bellRef.current.contains(e.target)) setBellOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  const handleLogout = async () => {
    try { if (sessionId) await authApi.logout(sessionId) } catch {}
    finally { logout(); navigate('/login', { replace: true }) }
  }

  const toggleLanguage = () => {
    const next = i18n.language === 'ar' ? 'en' : 'ar'
    i18n.changeLanguage(next)
    localStorage.setItem('lang', next)
    document.documentElement.setAttribute('dir',  next === 'ar' ? 'rtl' : 'ltr')
    document.documentElement.setAttribute('lang', next)
  }

  const handleNotifClick = (link) => {
    setBellOpen(false)
    setUnread(0)
    if (link) navigate(link)
  }

  return (
    <header className="h-16 bg-white border-b border-gray-200 flex items-center px-4 gap-3 flex-shrink-0">

      {/* Hamburger – mobile only */}
      <button onClick={onMenuClick}
        className="md:hidden p-2 rounded-md text-gray-500 hover:bg-gray-100 transition-colors flex-shrink-0">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
          <line x1="3" y1="6" x2="21" y2="6" />
          <line x1="3" y1="12" x2="21" y2="12" />
          <line x1="3" y1="18" x2="21" y2="18" />
        </svg>
      </button>

      {/* App name */}
      <span className="text-base font-bold text-green-700 select-none truncate">
        {t('common.appName')}
      </span>

      <div className="flex-1" />

      {/* Language toggle */}
      <button onClick={toggleLanguage}
        className="px-2.5 py-1 text-xs font-semibold border border-gray-300 rounded-md
          text-gray-600 hover:bg-gray-100 transition-colors">
        {i18n.language === 'ar' ? 'EN' : 'ع'}
      </button>

      {/* ── Bell (Notification Center) ───────────────────────────────────── */}
      <div className="relative" ref={bellRef}>
        <button
          onClick={() => { setBellOpen((o) => !o); if (!bellOpen) fetchNotifs() }}
          className="relative p-2 rounded-full text-gray-500 hover:bg-gray-100 transition-colors"
          aria-label="الإشعارات"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" strokeWidth="1.75" strokeLinecap="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9
              M13.73 21a2 2 0 0 1-3.46 0" />
          </svg>
          {unread > 0 && (
            <span className="absolute top-1 end-1 w-4 h-4 bg-red-500 text-white text-[9px]
              font-bold rounded-full flex items-center justify-center leading-none">
              {unread > 9 ? '9+' : unread}
            </span>
          )}
        </button>

        {/* Notification dropdown */}
        {bellOpen && (
          <div className="absolute end-0 mt-2 w-80 bg-white border border-gray-200 rounded-xl shadow-xl z-50 overflow-hidden">
            {/* Header */}
            <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
              <h3 className="text-sm font-semibold text-gray-800">الإشعارات</h3>
              {notifLoad && (
                <span className="text-[11px] text-gray-400">جاري التحميل...</span>
              )}
              {!notifLoad && unread > 0 && (
                <span className="text-[11px] text-gray-400">{unread} غير مقروءة</span>
              )}
            </div>

            {/* List */}
            <div className="max-h-80 overflow-y-auto divide-y divide-gray-50">
              {notifs.length === 0 && !notifLoad && (
                <div className="py-8 text-center text-sm text-gray-400">
                  لا توجد إشعارات
                </div>
              )}
              {notifs.map((n, i) => {
                const sev = SEV[n.severity] ?? SEV.low
                const icon = TYPE_ICON[n.type] ?? TYPE_ICON.critical_risk
                return (
                  <button key={i} onClick={() => handleNotifClick(n.link)}
                    className="w-full flex items-start gap-3 px-4 py-3 text-start hover:bg-gray-50 transition-colors">
                    {/* Icon */}
                    <div className={`mt-0.5 flex-shrink-0 w-7 h-7 rounded-full flex items-center justify-center border ${sev.badge}`}>
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none"
                        stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                        <path d={icon} />
                      </svg>
                    </div>
                    {/* Text */}
                    <div className="min-w-0 flex-1">
                      <p className="text-xs font-semibold text-gray-700 leading-snug">{n.titleAr}</p>
                      <p className="text-xs text-gray-500 mt-0.5 leading-snug line-clamp-2">{n.bodyAr}</p>
                    </div>
                    {/* Severity dot */}
                    <span className={`mt-1.5 flex-shrink-0 w-2 h-2 rounded-full ${sev.dot}`} />
                  </button>
                )
              })}
            </div>

            {/* Footer */}
            {notifs.length > 0 && (
              <div className="border-t border-gray-100 px-4 py-2">
                <button onClick={() => { setBellOpen(false); setUnread(0) }}
                  className="text-xs text-gray-400 hover:text-gray-600 transition-colors">
                  تحديد الكل كمقروء
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      {/* ── User avatar + dropdown ───────────────────────────────────────── */}
      <div className="relative" ref={menuRef}>
        <button onClick={() => setMenuOpen((o) => !o)}
          className="flex items-center gap-2 rounded-full focus:outline-none focus:ring-2 focus:ring-green-500">
          <span className="w-9 h-9 rounded-full bg-green-700 text-white text-sm font-bold
            flex items-center justify-center select-none">
            {initials}
          </span>
          <span className="hidden md:block text-sm text-gray-700 max-w-[140px] truncate">
            {displayName}
          </span>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" strokeWidth="2" strokeLinecap="round" className="text-gray-400">
            <path d="M6 9l6 6 6-6" />
          </svg>
        </button>

        {menuOpen && (
          <div className="absolute end-0 mt-2 w-44 bg-white border border-gray-200 rounded-xl shadow-lg z-50 py-1">
            <div className="px-4 py-2 border-b border-gray-100">
              <p className="text-xs font-semibold text-gray-800 truncate">{displayName}</p>
              <p className="text-[11px] text-gray-400 truncate">{username}</p>
            </div>
            <button onClick={() => { setMenuOpen(false); navigate('/settings') }}
              className="w-full flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                <circle cx="12" cy="12" r="3"/>
                <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/>
              </svg>
              {t('nav.settings')}
            </button>
            <button onClick={handleLogout}
              className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600
                hover:bg-red-50 transition-colors">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4 M16 17l5-5-5-5 M21 12H9" />
              </svg>
              {t('auth.logout')}
            </button>
          </div>
        )}
      </div>
    </header>
  )
}
