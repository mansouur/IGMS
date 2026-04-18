import { create } from 'zustand'

/**
 * confirmStore – يدير حالة نافذة التأكيد العالمية.
 * استخدم useConfirm() من useApi.js بدلاً من التعامل مع هذا الـ store مباشرة.
 */
const useConfirmStore = create((set) => ({
  open:    false,
  title:   '',
  message: '',
  variant: 'danger',   // 'danger' | 'warning' | 'info'
  resolve: null,

  show: ({ title, message, variant = 'danger' }) =>
    new Promise((resolve) => {
      set({ open: true, title, message, variant, resolve })
    }),

  confirm: () => set((s) => { s.resolve?.(true);  return { open: false, resolve: null } }),
  cancel:  () => set((s) => { s.resolve?.(false); return { open: false, resolve: null } }),
}))

export default useConfirmStore
