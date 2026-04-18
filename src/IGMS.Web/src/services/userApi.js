import api from './api'
import { downloadExcel } from './governanceApi'

const today = () => new Date().toISOString().slice(0, 10).replace(/-/g, '')

export const userApi = {
  getAll:    (params)        => api.get('/api/v1/users', { params }),
  getLookup: ()              => api.get('/api/v1/users/lookup'),
  getById:   (id)            => api.get(`/api/v1/users/${id}`),
  create:    (data)          => api.post('/api/v1/users', data),
  update:    (id, data)      => api.put(`/api/v1/users/${id}`, data),
  delete:    (id)            => api.delete(`/api/v1/users/${id}`),
  setActive: (id, isActive)  => api.patch(`/api/v1/users/${id}/active`, { isActive }),
  export:    (params)        => downloadExcel('/api/v1/users/export', params, `users_${today()}.xlsx`),
}
