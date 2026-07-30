const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5226/api/v1'

// ===== Types =====

export type AuthUser = {
  authenticated: boolean
  id?: string
  email?: string
  full_name?: string
  roles?: string[]
}

export type SupplyCategory = {
  id: string
  name: string
  supply_count?: number
  total_inventory_value?: number
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
  supplier: string
  unit_price: number
  description: string | null
  image_url: string | null
  concurrency_version?: number
  created_at: string
  updated_at?: string
  category?: SupplyCategory | null
}

export type SupplyListResponse = {
  items: Supply[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export type TrashItem = {
  id: string
  name: string
  sku: string
  category_name: string
  quantity: number
  deleted_at: string
  created_at: string
}

export type SupplyStats = {
  totalSupplies: number
  totalQuantity: number
  totalValue: number
  outOfStock: number
  needReorder: number
}

export type IssueItem = {
  id?: string
  supply_id: string
  supply_name?: string
  supply_code?: string
  quantity: number
  unit_price?: number
  subtotal?: number
}

export type Issue = {
  id: string
  issued_to: string
  issued_at: string
  total_amount: number
  item_count: number
  items: IssueItem[]
}

export type IssueListResponse = {
  items: Issue[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export type AuditLog = {
  id: string
  action: string
  entity: string
  entity_id: string | null
  details: string | null
  performed_by: string
  result: string | null
  ip_address: string | null
  created_at: string
}

export type AuditLogListResponse = {
  items: AuditLog[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export type DashboardData = {
  totalSupplies: number
  totalUnits: number
  lowStock: number
  outOfStock: number
  createdToday: number
  updatedToday: number
  accessDeniedToday: number
  sensitiveToday: number
  rejectedUploadsToday: number
  totalIssues: number
  monthlyActivity: { month: string; created: number; updated: number }[]
  stockStatus: { inStock: number; lowStock: number; outOfStock: number }
  issueTrend: { day: string; count: number }[]
  categoryStock: { category: string; totalQuantity: number; totalValue: number }[]
}

// ===== API helpers =====

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(`API error ${res.status}: ${text}`)
  }
  if (res.status === 204) return null as T
  if (res.headers.get('content-type')?.includes('text/csv')) {
    return res.blob() as unknown as T
  }
  return res.json()
}

function toQuery(params: Record<string, string | number | boolean | null | undefined>): string {
  const q = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) {
    if (v !== null && v !== undefined && v !== '') q.set(k, String(v))
  }
  return q.toString()
}

// ===== API =====

export const api = {
  auth: {
    me: () => apiFetch<AuthUser>('/auth/me'),
    login: (email: string, password: string, rememberMe = false) =>
      apiFetch<AuthUser>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password, rememberMe }),
      }),
    register: (email: string, password: string, fullName?: string) =>
      apiFetch<AuthUser>('/auth/register', {
        method: 'POST',
        body: JSON.stringify({ email, password, fullName }),
      }),
    logout: () => apiFetch<{ message: string }>('/auth/logout', { method: 'POST' }),
  },

  supplies: {
    list: (params?: { page?: number; pageSize?: number; search?: string; stockStatus?: string; categoryId?: number }) =>
      apiFetch<SupplyListResponse>(`/supplies?${toQuery(params || {})}`),
    get: (id: string) => apiFetch<Supply>(`/supplies/${id}`),
    create: (data: Partial<Supply>) =>
      apiFetch<{ id: string }>('/supplies', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: Partial<Supply>) =>
      apiFetch<{ id: string; concurrency_version: number }>(`/supplies/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) =>
      apiFetch<null>(`/supplies/${id}`, { method: 'DELETE' }),
    trash: () => apiFetch<TrashItem[]>('/supplies/trash'),
    restore: (id: string) =>
      apiFetch<{ id: string }>(`/supplies/${id}/restore`, { method: 'POST' }),
    adjust: (id: string, adjustment: number, note?: string) =>
      apiFetch<{ id: string; new_quantity: number }>(`/supplies/${id}/adjust`, {
        method: 'POST',
        body: JSON.stringify({ adjustment, note }),
      }),
    uploadImage: (id: string, file: File) => {
      const form = new FormData()
      form.append('file', file)
      return apiFetch<{ image_url: string }>(`/supplies/${id}/upload-image`, {
        method: 'POST',
        body: form,
        headers: {},
      })
    },
    stats: () => apiFetch<SupplyStats>('/supplies/stats'),
    export: (format: string) =>
      apiFetch<Blob>(`/supplies/export?format=${format}`),
  },

  categories: {
    list: () => apiFetch<SupplyCategory[]>('/categories'),
    create: (data: { name: string }) =>
      apiFetch<{ id: string }>('/categories', { method: 'POST', body: JSON.stringify(data) }),
    delete: (id: string) =>
      apiFetch<null>(`/categories/${id}`, { method: 'DELETE' }),
  },

  issues: {
    list: (params?: { page?: number; pageSize?: number }) =>
      apiFetch<IssueListResponse>(`/issues?${toQuery(params || {})}`),
    get: (id: string) => apiFetch<Issue>(`/issues/${id}`),
    create: (data: { issued_to: string; items: { supply_id: string; quantity: number }[] }) =>
      apiFetch<{ id: string }>('/issues', { method: 'POST', body: JSON.stringify(data) }),
  },

  auditLogs: {
    list: (params?: { page?: number; pageSize?: number; userName?: string; action?: string; result?: string; fromDate?: string; toDate?: string }) =>
      apiFetch<AuditLogListResponse>(`/auditlogs?${toQuery(params || {})}`),
  },

  dashboard: {
    get: () => apiFetch<DashboardData>('/dashboard'),
  },
}
