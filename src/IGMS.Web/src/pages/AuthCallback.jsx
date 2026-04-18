import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import useAuthStore from '../store/authStore'

/// Handles UAE Pass callback redirect.
/// URL fragment contains: #token=...&sessionId=...
export default function AuthCallback() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)

  useEffect(() => {
    const fragment = window.location.hash.substring(1)
    const params = new URLSearchParams(fragment)

    const token      = params.get('token')
    const sessionId  = params.get('sessionId')
    const error      = params.get('error')
    const fullNameAr = params.get('fullNameAr') ?? ''
    const fullNameEn = params.get('fullNameEn') ?? ''

    if (error || !token) {
      navigate('/login?error=' + encodeURIComponent(error ?? 'UAE Pass login failed'))
      return
    }

    setAuth({ token, sessionId, authProvider: 'UaePass', fullNameAr, fullNameEn })
    navigate('/dashboard')
  }, [])

  return (
    <div className="min-h-screen flex items-center justify-center">
      <p className="text-gray-500">جارٍ تسجيل الدخول بالهوية الرقمية...</p>
    </div>
  )
}
