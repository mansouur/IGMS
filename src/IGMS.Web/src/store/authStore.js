import { create } from 'zustand'

/**
 * Decodes the payload of a JWT without a library.
 * Returns parsed claims or null if the token is malformed.
 */
function decodeJwt(token) {
  try {
    const payload = token.split('.')[1]
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
    return JSON.parse(json)
  } catch {
    return null
  }
}

/**
 * Extracts IGMS-relevant fields from raw JWT claims.
 * Handles both single-value and array role/permission claims.
 */
function parseJwtClaims(token) {
  const claims = decodeJwt(token)
  if (!claims) return null

  const ROLE_CLAIM  = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
  const NAME_CLAIM  = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
  const ID_CLAIM    = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'

  const toArray = (val) =>
    val == null ? [] : Array.isArray(val) ? val : [val]

  return {
    userId:      claims[ID_CLAIM] ?? '',
    username:    claims[NAME_CLAIM] ?? '',
    tenantKey:   claims['tenant_key'] ?? '',
    language:    claims['language'] ?? 'ar',
    roles:       toArray(claims[ROLE_CLAIM]),
    permissions: toArray(claims['permission']),
  }
}

/**
 * Restores user data from a persisted token in sessionStorage.
 * Returns null if no token is found or it cannot be parsed.
 */
function restoreFromStorage() {
  const token = sessionStorage.getItem('igms_token')
  if (!token) return null
  const parsed = parseJwtClaims(token)
  if (!parsed) return null

  // Restore any extra fields (fullName, etc.) saved alongside the token
  const extra = sessionStorage.getItem('igms_user')
  const extraData = extra ? JSON.parse(extra) : {}
  return { token, ...parsed, ...extraData }
}

const initial = restoreFromStorage()

/**
 * Global auth state.
 * - token / roles / permissions parsed from JWT on every login.
 * - hasPermission(code) – used by Sidebar + route guards.
 */
const useAuthStore = create((set, get) => ({
  token:           initial?.token ?? null,
  userId:          initial?.userId ?? '',
  username:        initial?.username ?? '',
  fullNameAr:      initial?.fullNameAr ?? '',
  fullNameEn:      initial?.fullNameEn ?? '',
  tenantKey:       initial?.tenantKey ?? '',
  language:        initial?.language ?? 'ar',
  roles:           initial?.roles ?? [],
  permissions:     initial?.permissions ?? [],
  isAuthenticated: !!initial?.token,

  setAuth: (loginResponse) => {
    const parsed = parseJwtClaims(loginResponse.token)

    const user = {
      token:       loginResponse.token,
      userId:      parsed?.userId      ?? '',
      username:    parsed?.username    ?? loginResponse.username    ?? '',
      fullNameAr:  loginResponse.fullNameAr ?? '',
      fullNameEn:  loginResponse.fullNameEn ?? '',
      tenantKey:   parsed?.tenantKey   ?? loginResponse.tenantKey  ?? '',
      language:    parsed?.language    ?? loginResponse.language   ?? 'ar',
      roles:       parsed?.roles       ?? loginResponse.roles      ?? [],
      permissions: parsed?.permissions ?? [],
      sessionId:   loginResponse.sessionId ?? null,
      authProvider: loginResponse.authProvider ?? 'Local',
    }

    sessionStorage.setItem('igms_token', loginResponse.token)
    sessionStorage.setItem('igms_user', JSON.stringify({
      fullNameAr:   user.fullNameAr,
      fullNameEn:   user.fullNameEn,
      sessionId:    user.sessionId,
      authProvider: user.authProvider,
    }))

    set({ ...user, isAuthenticated: true })
  },

  logout: () => {
    sessionStorage.removeItem('igms_token')
    sessionStorage.removeItem('igms_user')
    set({
      token: null, userId: '', username: '', fullNameAr: '', fullNameEn: '',
      tenantKey: '', language: 'ar', roles: [], permissions: [], isAuthenticated: false,
    })
  },

  /** Fine-grained guard: hasPermission('RACI.APPROVE') */
  hasPermission: (code) => get().permissions.includes(code),

  /** Coarse guard: hasRole('ADMIN') */
  hasRole: (role) => get().roles.includes(role),
}))

export default useAuthStore
