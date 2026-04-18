import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import ar from './ar.json'
import en from './en.json'

// ── Restore persisted language preference ─────────────────────────────────────
const savedLang = localStorage.getItem('lang') || 'ar'

// Apply dir + lang on html element immediately (before React renders)
document.documentElement.setAttribute('dir',  savedLang === 'ar' ? 'rtl' : 'ltr')
document.documentElement.setAttribute('lang', savedLang)

// ── i18next init ──────────────────────────────────────────────────────────────
i18n.use(initReactI18next).init({
  resources: {
    ar: { translation: ar },
    en: { translation: en },
  },
  lng:         savedLang,
  fallbackLng: 'ar',
  interpolation: {
    escapeValue: false,
  },
})

export default i18n
