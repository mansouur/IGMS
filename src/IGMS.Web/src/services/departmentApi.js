import api from './api'
import { downloadExcel } from './governanceApi'

const today = () => new Date().toISOString().slice(0, 10)

export const departmentApi = {
  getAll:    (params)   => api.get('/api/v1/departments', { params }),
  getTree:   ()         => api.get('/api/v1/departments/tree'),
  getById:   (id)       => api.get(`/api/v1/departments/${id}`),
  create:    (data)     => api.post('/api/v1/departments', data),
  update:    (id, data) => api.put(`/api/v1/departments/${id}`, data),
  delete:    (id)       => api.delete(`/api/v1/departments/${id}`),
  setActive: (id, isActive) => api.patch(`/api/v1/departments/${id}/active`, { isActive }),
  export:    (params)   => downloadExcel('/api/v1/departments/export', params, `departments_${today()}.xlsx`),
}
