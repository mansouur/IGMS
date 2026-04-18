import axios from 'axios'

const TENANT_KEY = 'uae-sport' // Read from env in real deployments

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5257',
  headers: {
    'Content-Type': 'application/json',
    'X-Tenant-Key': TENANT_KEY,
  },
})

// Attach JWT token from sessionStorage on every request
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('igms_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle 401 globally – redirect to login
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      sessionStorage.removeItem('igms_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export const authApi = {
  login:              (credentials)    => api.post('/api/v1/auth/login', credentials),
  loginAd:            (credentials)    => api.post('/api/v1/auth/login/ad', credentials),
  logout:             (sessionId)      => api.post('/api/v1/auth/logout', { sessionId }),
  getMethods:         ()               => api.get('/api/v1/auth/methods'),
  getUaePassRedirect: (language)       => api.get(`/api/v1/auth/uaepass/redirect?language=${language}`),
  verifyOtp:          (userId, otp, language) =>
    api.post('/api/v1/auth/verify-otp', { userId, otp, language: language ?? 'ar' }),
  toggle2Fa:          (enabled, password) =>
    api.put('/api/v1/auth/2fa', { enabled, password }),
  getMe:              ()               => api.get('/api/v1/auth/me'),
}

export const raciApi = {
  getAll:          (params)   => api.get('/api/v1/raci', { params }),
  getById:         (id)       => api.get(`/api/v1/raci/${id}`),
  create:          (data)     => api.post('/api/v1/raci', data),
  update:          (id, data) => api.put(`/api/v1/raci/${id}`, data),
  delete:          (id)       => api.delete(`/api/v1/raci/${id}`),
  submit:          (id)       => api.post(`/api/v1/raci/${id}/submit`),
  approve:         (id)       => api.post(`/api/v1/raci/${id}/approve`),
}

export const auditApi = {
  getAll:          (params) => api.get('/api/v1/audit-logs', { params }),
  getEntityTypes:  ()       => api.get('/api/v1/audit-logs/entity-types'),
}

export const rolesApi = {
  getAll:            ()           => api.get('/api/v1/roles'),
  getLookup:         ()           => api.get('/api/v1/roles/lookup'),
  getById:           (id)         => api.get(`/api/v1/roles/${id}`),
  create:            (data)       => api.post('/api/v1/roles', data),
  update:            (id, data)   => api.put(`/api/v1/roles/${id}`, data),
  delete:            (id)         => api.delete(`/api/v1/roles/${id}`),
  setPermissions:    (id, ids)    => api.put(`/api/v1/roles/${id}/permissions`, { permissionIds: ids }),
  getAllPermissions:  ()           => api.get('/api/v1/permissions'),
}

export const notificationsApi = {
  getMyNotifications: () => api.get('/api/v1/notifications'),
}

export const reportsApi = {
  kpiTrend: () => api.get('/api/v1/reports/kpi-trend'),
}

export default api
