import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { authApi } from '../services/api'
import useAuthStore from '../store/authStore'

export default function Login() {
  const { t } = useTranslation()
  const navigate   = useNavigate()
  const setAuth    = useAuthStore((s) => s.setAuth)

  // ── Step: 'credentials' | 'otp' ──────────────────────────────────────────
  const [step,         setStep]         = useState('credentials')
  const [pendingUserId, setPendingUserId] = useState(null)
  const [otp,          setOtp]          = useState(['', '', '', '', '', ''])
  const otpRefs = [useRef(), useRef(), useRef(), useRef(), useRef(), useRef()]

  const [form,           setForm]           = useState({ username: '', password: '', language: 'ar' })
  const [error,          setError]          = useState(null)
  const [loading,        setLoading]        = useState(false)
  const [uaePassLoading, setUaePassLoading] = useState(false)
  const [authMethods,    setAuthMethods]    = useState({ local: true, ad: false, uaePass: false })

  useEffect(() => {
    authApi.getMethods()
      .then(res => setAuthMethods(res.data.data))
      .catch(() => {})
  }, [])

  // Auto-focus first OTP box when step changes
  useEffect(() => {
    if (step === 'otp') setTimeout(() => otpRefs[0]?.current?.focus(), 50)
  }, [step])

  const handleChange = (e) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }))
    setError(null)
  }

  const handleLocalLogin = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const res  = await authApi.login(form)
      const data = res.data.data

      if (data.requiresOtp) {
        // 2FA required → switch to OTP step
        setPendingUserId(data.pendingUserId)
        setStep('otp')
      } else {
        setAuth(data)
        navigate('/dashboard')
      }
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? t('auth.error.serverError'))
    } finally {
      setLoading(false)
    }
  }

  // ── OTP input handlers ───────────────────────────────────────────────────
  const handleOtpChange = (index, value) => {
    if (!/^\d*$/.test(value)) return
    const next = [...otp]
    next[index] = value.slice(-1)
    setOtp(next)
    setError(null)
    if (value && index < 5) otpRefs[index + 1]?.current?.focus()
  }

  const handleOtpKeyDown = (index, e) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) {
      otpRefs[index - 1]?.current?.focus()
    }
  }

  const handleOtpPaste = (e) => {
    const text = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
    if (text.length === 6) {
      setOtp(text.split(''))
      otpRefs[5]?.current?.focus()
    }
  }

  const handleVerifyOtp = async (e) => {
    e.preventDefault()
    const code = otp.join('')
    if (code.length !== 6) { setError('أدخل الرمز المكون من 6 أرقام'); return }
    setLoading(true)
    setError(null)
    try {
      const res  = await authApi.verifyOtp(pendingUserId, code, form.language)
      setAuth(res.data.data)
      navigate('/dashboard')
    } catch (err) {
      setError(err.response?.data?.errors?.[0] ?? 'رمز التحقق غير صحيح أو منتهي الصلاحية.')
      setOtp(['', '', '', '', '', ''])
      otpRefs[0]?.current?.focus()
    } finally {
      setLoading(false)
    }
  }

  const handleUaePassLogin = async () => {
    setUaePassLoading(true)
    setError(null)
    try {
      const res = await authApi.getUaePassRedirect(form.language)
      window.location.href = res.data.data.redirectUrl
    } catch {
      setError(t('auth.error.serverError'))
      setUaePassLoading(false)
    }
  }

  // ── OTP Step UI ────────────────────────────────────────────────────────────
  if (step === 'otp') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-100">
        <div className="bg-white rounded-2xl shadow-md w-full max-w-sm p-8">
          <div className="text-center mb-8">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="#15803d" strokeWidth="2">
                <rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 7V5a2 2 0 00-4 0v2M8 7V5a2 2 0 00-4 0v2"/>
                <path d="M12 12v4M12 12h.01"/>
              </svg>
            </div>
            <h2 className="text-xl font-bold text-gray-800">التحقق بخطوتين</h2>
            <p className="text-sm text-gray-500 mt-1">
              تم إرسال رمز مكون من 6 أرقام إلى بريدك الإلكتروني
            </p>
          </div>

          <form onSubmit={handleVerifyOtp} noValidate>
            {/* OTP Input boxes */}
            <div className="flex gap-2 justify-center mb-6 dir-ltr" dir="ltr"
              onPaste={handleOtpPaste}>
              {otp.map((digit, i) => (
                <input
                  key={i}
                  ref={otpRefs[i]}
                  type="text"
                  inputMode="numeric"
                  maxLength={1}
                  value={digit}
                  onChange={(e) => handleOtpChange(i, e.target.value)}
                  onKeyDown={(e) => handleOtpKeyDown(i, e)}
                  className="w-11 h-12 text-center text-xl font-bold border-2 border-gray-300 rounded-lg
                    focus:outline-none focus:border-green-600 focus:ring-2 focus:ring-green-100 transition-colors"
                />
              ))}
            </div>

            {error && (
              <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={loading || otp.join('').length < 6}
              className="w-full py-2 rounded-lg text-white font-medium text-sm transition-opacity disabled:opacity-50 mb-3"
              style={{ backgroundColor: 'var(--tenant-primary)' }}
            >
              {loading ? 'جاري التحقق...' : 'تحقق من الرمز'}
            </button>

            <button
              type="button"
              onClick={() => { setStep('credentials'); setOtp(['', '', '', '', '', '']); setError(null) }}
              className="w-full py-2 text-sm text-gray-500 hover:text-gray-700"
            >
              العودة لتسجيل الدخول
            </button>
          </form>

          <p className="text-xs text-gray-400 text-center mt-4">
            لم يصلك الرمز؟ تأكد من مجلد البريد العشوائي أو أعد تسجيل الدخول.
          </p>
        </div>
      </div>
    )
  }

  // ── Credentials Step UI ────────────────────────────────────────────────────
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="bg-white rounded-2xl shadow-md w-full max-w-sm p-8">

        {/* Title */}
        <div className="text-center mb-8">
          <h1 className="text-2xl font-bold" style={{ color: 'var(--tenant-primary)' }}>
            {t('common.appName')}
          </h1>
          <p className="text-sm text-gray-500 mt-1">{t('auth.login')}</p>
        </div>

        {/* UAE Pass Button */}
        {authMethods.uaePass && (
          <div className="mb-6">
            <button
              type="button"
              onClick={handleUaePassLogin}
              disabled={uaePassLoading}
              className="w-full flex items-center justify-center gap-3 py-2.5 rounded-lg border-2 font-medium text-sm transition-opacity disabled:opacity-60"
              style={{ borderColor: '#00843D', color: '#00843D' }}
            >
              {uaePassLoading ? (
                <span>{t('auth.loading')}</span>
              ) : (
                <>
                  <svg width="24" height="24" viewBox="0 0 40 40" fill="none">
                    <circle cx="20" cy="20" r="20" fill="#00843D"/>
                    <text x="50%" y="55%" dominantBaseline="middle" textAnchor="middle"
                      fill="white" fontSize="13" fontWeight="bold">UAE</text>
                  </svg>
                  <span>{t('auth.loginWithUaePass')}</span>
                </>
              )}
            </button>
            {authMethods.local && (
              <div className="flex items-center gap-3 my-5">
                <div className="flex-1 border-t border-gray-200" />
                <span className="text-xs text-gray-400">{t('auth.or')}</span>
                <div className="flex-1 border-t border-gray-200" />
              </div>
            )}
          </div>
        )}

        {/* Local / AD Login Form */}
        {authMethods.local && (
          <form onSubmit={handleLocalLogin} noValidate>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('auth.username')}
              </label>
              <input
                type="text"
                name="username"
                value={form.username}
                onChange={handleChange}
                required
                autoComplete="username"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
              />
            </div>

            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {t('auth.password')}
              </label>
              <input
                type="password"
                name="password"
                value={form.password}
                onChange={handleChange}
                required
                autoComplete="current-password"
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-600"
              />
            </div>

            {error && (
              <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={loading}
              className="w-full py-2 rounded-lg text-white font-medium text-sm transition-opacity disabled:opacity-60"
              style={{ backgroundColor: 'var(--tenant-primary)' }}
            >
              {loading ? t('auth.loading') : t('auth.submit')}
            </button>
          </form>
        )}

      </div>
    </div>
  )
}
