import { useEffect, useState } from 'react'
import { useNavigate }         from 'react-router-dom'
import { authApi }             from '../services/api'
import useAuthStore            from '../store/authStore'

/**
 * UAE Pass OAuth2 callback page.
 *
 * UAE Pass redirects here with:
 *   http://localhost:5173/auth/callback?code=XXX&state=YYY
 *
 * We call POST /api/v1/auth/uaepass/exchange with the code,
 * receive our IGMS JWT, store it, and navigate to the dashboard.
 */
export default function AuthCallback() {
  const navigate  = useNavigate()
  const setAuth   = useAuthStore((s) => s.setAuth)
  const [status, setStatus] = useState('جارٍ تسجيل الدخول بالهوية الرقمية...')

  useEffect(() => {
    const params = new URLSearchParams(window.location.search)
    const code   = params.get('code')
    const state  = params.get('state')
    const error  = params.get('error')

    if (error) {
      navigate('/login?error=' + encodeURIComponent(error))
      return
    }

    if (!code) {
      navigate('/login?error=' + encodeURIComponent('لم يتم استلام رمز التفويض من الهوية الرقمية.'))
      return
    }

    // Guard against React StrictMode double-invocation (code is single-use)
    if (sessionStorage.getItem('uae_exchanging') === code) return
    sessionStorage.setItem('uae_exchanging', code)

    authApi.exchangeUaePassCode(code, state ?? '')
      .then((res) => {
        const data = res.data?.data
        if (!data?.token) throw new Error('no token')
        sessionStorage.removeItem('uae_exchanging')
        setAuth(data)
        navigate('/dashboard')
      })
      .catch((err) => {
        sessionStorage.removeItem('uae_exchanging')
        const msg = err.response?.data?.errors?.[0]
          ?? err.response?.data?.message
          ?? 'فشل تسجيل الدخول بالهوية الرقمية.'
        navigate('/login?error=' + encodeURIComponent(msg))
      })
  }, [])

  return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-4">
      {status && (
        <>
          <svg className="animate-spin text-green-600" width="32" height="32"
            viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 12a9 9 0 1 1-6.219-8.56"/>
          </svg>
          <p className="text-gray-600 text-sm">{status}</p>
        </>
      )}
    </div>
  )
}
