import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import TopBar from './TopBar'
import Sidebar from './Sidebar'
import ToastContainer from '../ui/Toast'
import ConfirmDialog  from '../ui/ConfirmDialog'

/**
 * Root shell for all authenticated pages.
 * Layout: TopBar (full width, fixed top) + Sidebar + main content area.
 *
 * RTL (Arabic) – sidebar appears on the right side via dir="rtl".
 * LTR (English) – sidebar appears on the left side via dir="ltr".
 * dir is set globally on <html> by TopBar language toggle.
 *
 * Mobile: sidebar is hidden behind a full-height drawer activated by a
 * hamburger button in TopBar. An overlay backdrop closes it on tap.
 */
export default function AppLayout() {
  const [collapsed,   setCollapsed]   = useState(false)
  const [mobileOpen,  setMobileOpen]  = useState(false)

  return (
    <div className="flex flex-col h-screen overflow-hidden bg-gray-50">
      {/* ── Top bar ─────────────────────────────────────────────── */}
      <div className="print:hidden">
        <TopBar onMenuClick={() => setMobileOpen((o) => !o)} />
      </div>

      {/* ── Body: sidebar + page content ────────────────────────── */}
      <div className="flex flex-1 overflow-hidden relative">

        {/* Mobile backdrop – closes sidebar when tapped */}
        {mobileOpen && (
          <div
            className="fixed inset-0 bg-black/40 z-30 md:hidden print:hidden"
            onClick={() => setMobileOpen(false)}
          />
        )}

        <div className="print:hidden">
          <Sidebar
            collapsed={collapsed}
            onToggle={() => setCollapsed((c) => !c)}
            mobileOpen={mobileOpen}
            onClose={() => setMobileOpen(false)}
          />
        </div>

        {/* ── Page content ─────────────────────────────────────── */}
        <main className="flex-1 overflow-y-auto p-4 md:p-6 print:overflow-visible print:p-6">
          <Outlet />
        </main>
      </div>

      {/* Toast notifications – rendered above everything */}
      <ToastContainer />

      {/* Confirmation dialog – global, mounted once */}
      <ConfirmDialog />
    </div>
  )
}
