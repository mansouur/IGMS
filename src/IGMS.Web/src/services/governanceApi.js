import api from './api'

// ── Shared download helper ────────────────────────────────────────────────────
// يُرسل الطلب عبر axios (التوكن في الـ header)، ثم يُنشئ رابط تحميل مؤقت.
// أكثر أمانًا من وضع التوكن في query string.
export async function downloadExcel(url, params, filename) {
  const response = await api.get(url, { params, responseType: 'blob' })
  const blob = new Blob([response.data], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  })
  const link = document.createElement('a')
  link.href  = URL.createObjectURL(blob)
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(link.href)
}

// ── Module APIs ───────────────────────────────────────────────────────────────

export const policyApi = {
  getAll:      (params)      => api.get('/api/v1/policies', { params }),
  getById:     (id)          => api.get(`/api/v1/policies/${id}`),
  create:      (data)        => api.post('/api/v1/policies', data),
  update:      (id, data)    => api.put(`/api/v1/policies/${id}`, data),
  delete:      (id)          => api.delete(`/api/v1/policies/${id}`),
  setStatus:   (id, status, approverId = null) => api.patch(`/api/v1/policies/${id}/status`, { status, approverId }),
  renew:       (id)          => api.post(`/api/v1/policies/${id}/renew`),
  export:      (params)      => downloadExcel('/api/v1/policies/export', params, `policies_${today()}.xlsx`),
  getVersions: (id)          => api.get(`/api/v1/policies/${id}/versions`),
}

export const riskApi = {
  getAll:        (params)           => api.get('/api/v1/risks', { params }),
  getById:       (id)               => api.get(`/api/v1/risks/${id}`),
  create:        (data)             => api.post('/api/v1/risks', data),
  update:        (id, data)         => api.put(`/api/v1/risks/${id}`, data),
  delete:        (id)               => api.delete(`/api/v1/risks/${id}`),
  export:        (params)           => downloadExcel('/api/v1/risks/export', params, `risks_${today()}.xlsx`),
  getHeatMap:    ()                 => api.get('/api/v1/risks/heatmap'),
  getKpiLinks:   (riskId)           => api.get(`/api/v1/risks/${riskId}/kpi-links`),
  addKpiLink:    (riskId, data)     => api.post(`/api/v1/risks/${riskId}/kpi-links`, data),
  removeKpiLink: (mappingId)        => api.delete(`/api/v1/risks/kpi-links/${mappingId}`),
}

export const taskApi = {
  getAll:    (params)    => api.get('/api/v1/tasks', { params }),
  getById:   (id)        => api.get(`/api/v1/tasks/${id}`),
  getByRisk: (riskId)    => api.get(`/api/v1/tasks/by-risk/${riskId}`),
  create:    (data)      => api.post('/api/v1/tasks', data),
  update:    (id, data)  => api.put(`/api/v1/tasks/${id}`, data),
  delete:    (id)        => api.delete(`/api/v1/tasks/${id}`),
  export:    (params)    => downloadExcel('/api/v1/tasks/export', params, `tasks_${today()}.xlsx`),
}

export const kpiApi = {
  getAll:         (params)           => api.get('/api/v1/kpis', { params }),
  getById:        (id)               => api.get(`/api/v1/kpis/${id}`),
  create:         (data)             => api.post('/api/v1/kpis', data),
  update:         (id, data)         => api.put(`/api/v1/kpis/${id}`, data),
  delete:         (id)               => api.delete(`/api/v1/kpis/${id}`),
  export:         (params)           => downloadExcel('/api/v1/kpis/export', params, `kpis_${today()}.xlsx`),
  // History (trend records)
  getHistory:     (kpiId)            => api.get(`/api/v1/kpis/${kpiId}/history`),
  upsertRecord:   (kpiId, data)      => api.post(`/api/v1/kpis/${kpiId}/history`, data),
  deleteRecord:   (kpiId, recordId)  => api.delete(`/api/v1/kpis/${kpiId}/history/${recordId}`),
  // Risk impact links
  getRiskLinks:   (kpiId)            => api.get(`/api/v1/kpis/${kpiId}/risk-links`),
}

export const complianceApi = {
  getByEntity: (entityType, entityId) => api.get('/api/v1/compliance', { params: { entityType, entityId } }),
  add:         (data)                  => api.post('/api/v1/compliance', data),
  delete:      (id)                    => api.delete(`/api/v1/compliance/${id}`),
}

export const controlTestApi = {
  getAll:          (params)               => api.get('/api/v1/control-tests', { params }),
  getById:         (id)                   => api.get(`/api/v1/control-tests/${id}`),
  create:          (data)                 => api.post('/api/v1/control-tests', data),
  update:          (id, data)             => api.put(`/api/v1/control-tests/${id}`, data),
  delete:          (id)                   => api.delete(`/api/v1/control-tests/${id}`),
  uploadEvidence:  (id, formData)         => api.post(`/api/v1/control-tests/${id}/evidence`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  downloadEvidence: (id, evidenceId)      => api.get(`/api/v1/control-tests/${id}/evidence/${evidenceId}`, { responseType: 'blob' }),
  deleteEvidence:   (id, evidenceId)      => api.delete(`/api/v1/control-tests/${id}/evidence/${evidenceId}`),
}

const today = () => new Date().toISOString().slice(0, 10).replace(/-/g, '')
