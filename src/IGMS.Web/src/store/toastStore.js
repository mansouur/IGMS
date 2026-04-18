import { create } from 'zustand'

/**
 * Toast notification store.
 * Types: 'success' | 'error' | 'warning' | 'info'
 * Auto-dismiss after `duration` ms (default 4000).
 */
const useToastStore = create((set) => ({
  toasts: [],

  add: (type, message, duration = 4000) => {
    const id = Date.now() + Math.random()
    set((s) => ({ toasts: [...s.toasts, { id, type, message }] }))
    if (duration > 0) {
      setTimeout(() => {
        set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) }))
      }, duration)
    }
    return id
  },

  remove: (id) => set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })),
  clear:  ()   => set({ toasts: [] }),
}))

// ── Convenience helpers ────────────────────────────────────────────────────────
export const toast = {
  success: (msg, duration) => useToastStore.getState().add('success', msg, duration),
  error:   (msg, duration) => useToastStore.getState().add('error',   msg, duration),
  warning: (msg, duration) => useToastStore.getState().add('warning', msg, duration),
  info:    (msg, duration) => useToastStore.getState().add('info',    msg, duration),
}

export default useToastStore
