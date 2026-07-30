import { useEffect, useState } from 'react'
import { api, type AuditLogListResponse } from '../lib/supabase'
import { PageHeader, Spinner, Badge, EmptyState } from '../components/ui'
import { ClipboardList, ChevronLeft, ChevronRight } from 'lucide-react'

const actionColors: Record<string, 'primary' | 'accent' | 'warning' | 'danger'> = {
  CREATE: 'accent',
  INSERT: 'accent',
  UPDATE: 'warning',
  DELETE: 'danger',
  LOGIN: 'primary',
  ISSUE: 'primary',
  ACCESS_DENIED: 'danger',
  SENSITIVE_ACTION: 'warning',
}

export default function AuditLogs() {
  const [data, setData] = useState<AuditLogListResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)
  const [userName, setUserName] = useState('')
  const [action, setAction] = useState('')
  const [result, setResult] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')

  async function load() {
    try {
      const params: Record<string, string | number> = { page, pageSize: 15 }
      if (userName) params.userName = userName
      if (action) params.action = action
      if (result) params.result = result
      if (fromDate) params.fromDate = fromDate
      if (toDate) params.toDate = toDate

      setData(await api.auditLogs.list(params as any))
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [page])

  function handleFilter() {
    setPage(1)
    load()
  }

  if (loading && !data) return <Spinner />

  const items = data?.items || []
  const totalPages = data?.totalPages || 1

  return (
    <div>
      <PageHeader title="Audit Logs" subtitle="System activity trail" />

      <div className="card mb-4 p-3">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <div className="flex-1">
            <label className="label">User</label>
            <input type="text" className="input" placeholder="Filter by user..." value={userName} onChange={(e) => setUserName(e.target.value)} />
          </div>
          <div className="w-40">
            <label className="label">Action</label>
            <select className="input" value={action} onChange={(e) => setAction(e.target.value)}>
              <option value="">All Actions</option>
              <option value="CREATE">Create</option>
              <option value="UPDATE">Update</option>
              <option value="DELETE">Delete</option>
              <option value="LOGIN">Login</option>
              <option value="ACCESS_DENIED">Access Denied</option>
            </select>
          </div>
          <div className="w-40">
            <label className="label">Result</label>
            <select className="input" value={result} onChange={(e) => setResult(e.target.value)}>
              <option value="">All Results</option>
              <option value="Success">Success</option>
              <option value="Failed">Failed</option>
              <option value="Rejected">Rejected</option>
            </select>
          </div>
          <div className="w-40">
            <label className="label">From</label>
            <input type="date" className="input" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          </div>
          <div className="w-40">
            <label className="label">To</label>
            <input type="date" className="input" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          </div>
          <button onClick={handleFilter} className="btn-primary">Filter</button>
        </div>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={ClipboardList} title="No logs" message="No audit logs found" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">Action</th>
                  <th className="px-4 py-3">Entity</th>
                  <th className="px-4 py-3">Entity ID</th>
                  <th className="px-4 py-3">By</th>
                  <th className="px-4 py-3">Result</th>
                  <th className="px-4 py-3">Date</th>
                </tr>
              </thead>
              <tbody>
                {items.map((log) => (
                  <tr key={log.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3"><Badge color={actionColors[log.action] || 'slate'}>{log.action}</Badge></td>
                    <td className="px-4 py-3 font-medium text-slate-800">{log.entity}</td>
                    <td className="px-4 py-3 text-slate-500 font-mono text-xs">{log.entity_id ?? '...'}</td>
                    <td className="px-4 py-3 text-slate-600">{log.performed_by}</td>
                    <td className="px-4 py-3">
                      {log.result && (
                        <Badge color={log.result === 'Success' ? 'accent' : log.result === 'Failed' ? 'danger' : 'warning'}>
                          {log.result}
                        </Badge>
                      )}
                    </td>
                    <td className="px-4 py-3 text-slate-400">{new Date(log.created_at).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-between border-t border-slate-100 px-4 py-3">
              <span className="text-sm text-slate-500">
                Page {page} of {totalPages} ({data?.totalCount || 0} items)
              </span>
              <div className="flex items-center gap-1">
                <button
                  onClick={() => setPage(Math.max(1, page - 1))}
                  disabled={page === 1}
                  className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 disabled:opacity-30"
                >
                  <ChevronLeft className="h-4 w-4" />
                </button>
                <button
                  onClick={() => setPage(Math.min(totalPages, page + 1))}
                  disabled={page === totalPages}
                  className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 disabled:opacity-30"
                >
                  <ChevronRight className="h-4 w-4" />
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
