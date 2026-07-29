const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5226/api/v1'

export type SupplyCategory = {
  id: string
  name: string
  description: string | null
  created_at: string
}

export type Supply = {
  id: string
  name: string
  sku: string
  category_id: string | null
  quantity: number
  unit: string
  reorder_level: number
  expiry_date: string | null
  location: string | null
  created_at: string
  category?: SupplyCategory | null
}

export type Issue = {
  id: string
  supply_id: string
  quantity: number
  issued_to: string
  issued_by: string
  notes: string | null
  created_at: string
  supply?: Supply | null
}

export type AuditLog = {
  id: string
  action: string
  entity: string
  entity_id: string | null
  details: string | null
  performed_by: string
  created_at: string
}

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(`API error ${res.status}: ${text}`)
  }
  if (res.status === 204) return null as T
  return res.json()
}

export const api = {
  supplies: {
    list: () => apiFetch<Supply[]>('/supplies'),
    get: (id: string) => apiFetch<Supply>(`/supplies/${id}`),
    create: (data: Partial<Supply>) => apiFetch<{ id: string }>('/supplies', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: Partial<Supply>) => apiFetch<{ id: string }>(`/supplies/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => apiFetch<null>(`/supplies/${id}`, { method: 'DELETE' }),
  },
  categories: {
    list: () => apiFetch<SupplyCategory[]>('/categories'),
    create: (data: Partial<SupplyCategory>) => apiFetch<{ id: string }>('/categories', { method: 'POST', body: JSON.stringify(data) }),
  },
  issues: {
    list: () => apiFetch<Issue[]>('/issues'),
    create: (data: Partial<Issue>) => apiFetch<{ id: string }>('/issues', { method: 'POST', body: JSON.stringify(data) }),
  },
  auditLogs: {
    list: () => apiFetch<AuditLog[]>('/auditlogs'),
  },
}
