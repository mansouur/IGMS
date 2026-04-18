import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import api, { authApi } from '../../services/api'

export default function Settings() {
  const { t } = useTranslation()

  // ── Change Password ────────────────────────────────────────────────────────
  const [form, setForm]     = useState({ currentPassword: '', newPassword: '', confirm: '' })
  const [saving, setSaving] = useState(false)
  const [msg, setMsg]       = useState(null)

  function handleChange(e) {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }))
    setMsg(null)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    if (form.newPassword !== form.confirm) {
      setMsg({ type: 'error', text: t('settings.changePassword.mismatch') })
      return
    }
    if (form.newPassword.length < 6) {
      setMsg({ type: 'error', text: t('settings.changePassword.tooShort') })
      return
    }
    setSaving(true)
    try {
      await api.post('/users/me/change-password', {
        currentPassword: form.currentPassword,
        newPassword:     form.newPassword,
      })
      setMsg({ type: 'success', text: t('settings.changePassword.success') })
      setForm({ currentPassword: '', newPassword: '', confirm: '' })
    } catch (err) {
      const text = err.response?.data?.errors?.[0] ?? t('settings.changePassword.error')
      setMsg({ type: 'error', text })
    } finally {
      setSaving(false)
    }
  }

  // ── Two-Factor Authentication ──────────────────────────────────────────────
  const [twoFaEnabled,  setTwoFaEnabled]  = useState(false)
  const [twoFaLoading,  setTwoFaLoading]  = useState(true)
  const [twoFaPassword, setTwoFaPassword] = useState('')
  const [twoFaSaving,   setTwoFaSaving]   = useState(false)
  const [twoFaMsg,      setTwoFaMsg]      = useState(null)
  const [showTwoFaForm, setShowTwoFaForm] = useState(false)
  const [pendingEnabled, setPendingEnabled] = useState(false)

  useEffect(() => {
    authApi.getMe()
      .then(res => setTwoFaEnabled(res.data.data.twoFactorEnabled))
      .catch(() => {})
      .finally(() => setTwoFaLoading(false))
  }, [])

  function openTwoFaConfirm(enable) {
    setPendingEnabled(enable)
    setTwoFaPassword('')
    setTwoFaMsg(null)
    setShowTwoFaForm(true)
  }

  async function handleToggle2Fa(e) {
    e.preventDefault()
    if (!twoFaPassword) return
    setTwoFaSaving(true)
    setTwoFaMsg(null)
    try {
      await authApi.toggle2Fa(pendingEnabled, twoFaPassword)
      setTwoFaEnabled(pendingEnabled)
      setShowTwoFaForm(false)
      setTwoFaPassword('')
      setTwoFaMsg({
        type: 'success',
        text: pendingEnabled
          ? 'تم تفعيل المصادقة الثنائية بنجاح.'
          : 'تم إلغاء المصادقة الثنائية.',
      })
    } catch (err) {
      setTwoFaMsg({ type: 'error', text: err.response?.data?.errors?.[0] ?? 'حدث خطأ، حاول مرة أخرى.' })
    } finally {
      setTwoFaSaving(false)
    }
  }

  const inputCls = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600'

  return (
    <div className="max-w-lg space-y-6">
      <div>
        <h1 className="text-xl font-bold text-gray-800">{t('settings.title')}</h1>
        <p className="text-sm text-gray-500 mt-0.5">{t('settings.subtitle')}</p>
      </div>

      {/* ── Change Password Card ─────────────────────────────────────────────── */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
        <h2 className="text-base font-semibold text-gray-700 mb-4 border-b pb-2">
          {t('settings.changePassword.sectionTitle')}
        </h2>

        {msg && (
          <div className={`mb-4 rounded-lg px-4 py-3 text-sm ${msg.type === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}>
            {msg.text}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('settings.changePassword.currentPassword')}</label>
            <input type="password" name="currentPassword" value={form.currentPassword} onChange={handleChange} required className={inputCls} dir="ltr" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('settings.changePassword.newPassword')}</label>
            <input type="password" name="newPassword" value={form.newPassword} onChange={handleChange} required minLength={6} className={inputCls} dir="ltr" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('settings.changePassword.confirmPassword')}</label>
            <input type="password" name="confirm" value={form.confirm} onChange={handleChange} required className={inputCls} dir="ltr" />
          </div>
          <button type="submit" disabled={saving}
            className="w-full bg-green-700 hover:bg-green-800 text-white text-sm font-medium py-2 px-4 rounded-lg disabled:opacity-50 transition-colors">
            {saving ? t('settings.changePassword.saving') : t('settings.changePassword.submit')}
          </button>
        </form>
      </div>

      {/* ── Two-Factor Authentication Card ──────────────────────────────────── */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
        <h2 className="text-base font-semibold text-gray-700 mb-1 border-b pb-2">
          المصادقة الثنائية (2FA)
        </h2>
        <p className="text-sm text-gray-500 mb-4 mt-2">
          عند تفعيلها، سيُطلب منك إدخال رمز يُرسل إلى بريدك الإلكتروني في كل تسجيل دخول.
        </p>

        {twoFaMsg && (
          <div className={`mb-4 rounded-lg px-4 py-3 text-sm ${twoFaMsg.type === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}>
            {twoFaMsg.text}
          </div>
        )}

        {twoFaLoading ? (
          <div className="text-sm text-gray-400">جاري التحميل...</div>
        ) : (
          <>
            {/* Status badge + action button */}
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${twoFaEnabled ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
                  {twoFaEnabled ? 'مفعّلة' : 'معطّلة'}
                </span>
              </div>
              {!showTwoFaForm && (
                <button
                  type="button"
                  onClick={() => openTwoFaConfirm(!twoFaEnabled)}
                  className={`text-sm font-medium px-4 py-1.5 rounded-lg transition-colors ${
                    twoFaEnabled
                      ? 'bg-red-50 text-red-700 hover:bg-red-100'
                      : 'bg-green-50 text-green-700 hover:bg-green-100'
                  }`}
                >
                  {twoFaEnabled ? 'إلغاء التفعيل' : 'تفعيل'}
                </button>
              )}
            </div>

            {/* Password confirmation form */}
            {showTwoFaForm && (
              <form onSubmit={handleToggle2Fa} className="mt-4 space-y-3 border-t pt-4">
                <p className="text-sm text-gray-600">
                  {pendingEnabled
                    ? 'أدخل كلمة مرورك الحالية لتفعيل المصادقة الثنائية:'
                    : 'أدخل كلمة مرورك الحالية لإلغاء المصادقة الثنائية:'}
                </p>
                <input
                  type="password"
                  value={twoFaPassword}
                  onChange={e => { setTwoFaPassword(e.target.value); setTwoFaMsg(null) }}
                  placeholder="كلمة المرور"
                  required
                  autoFocus
                  className={inputCls}
                  dir="ltr"
                />
                <div className="flex gap-2">
                  <button
                    type="submit"
                    disabled={twoFaSaving || !twoFaPassword}
                    className={`flex-1 py-2 text-sm font-medium rounded-lg text-white transition-colors disabled:opacity-50 ${
                      pendingEnabled ? 'bg-green-700 hover:bg-green-800' : 'bg-red-600 hover:bg-red-700'
                    }`}
                  >
                    {twoFaSaving ? 'جاري الحفظ...' : (pendingEnabled ? 'تفعيل' : 'إلغاء التفعيل')}
                  </button>
                  <button
                    type="button"
                    onClick={() => { setShowTwoFaForm(false); setTwoFaMsg(null) }}
                    className="flex-1 py-2 text-sm font-medium rounded-lg text-gray-600 bg-gray-100 hover:bg-gray-200 transition-colors"
                  >
                    إلغاء
                  </button>
                </div>
              </form>
            )}
          </>
        )}
      </div>
    </div>
  )
}
