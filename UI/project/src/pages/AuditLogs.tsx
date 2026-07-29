import { useEffect, useState, useMemo } from 'react'
import { Search, ShieldCheck, Filter } from 'lucide-react'
import { supabase, type AuditLog } from '../lib/supabase'
import { PageHeader, Badge, Spinner, EmptyState } from '../components/ui'

const actionColor: Record<string, 'accent' | 'primary' | 'warning' | 'danger' | 'slate'> = {
  CREATE: 'accent',
  UPDATE: 'primary',
  DELETE: 'danger',
  ISSUE: 'warning',
}

export default function AuditLogs() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [filterAction, setFilterAction] = useState('all')

  useEffect(() => {
    async function load() {
      const { data } = await supabase.from('audit_logs').select('*').order('created_at', { ascending: false })
      setLogs(data ?? [])
      setLoading(false)
    }
    load()
  }, [])

  const filtered = useMemo(() => {
    return logs.filter((l) => {
      const matchesSearch = !search ||
        l.details?.toLowerCase().includes(search.toLowerCase()) ||
        l.performed_by.toLowerCase().includes(search.toLowerCase()) ||
        l.entity.toLowerCase().includes(search.toLowerCase())
      const matchesAction = filterAction === 'all' || l.action === filterAction
      return matchesSearch && matchesAction
    })
  }, [logs, search, filterAction])

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Audit Logs" subtitle={`${logs.length} recorded actions`} />

      <div className="card mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Search by details, entity, or user..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input pl-10"
            />
          </div>
          <div className="flex items-center gap-2">
            <Filter className="h-4 w-4 text-slate-400" />
            <select value={filterAction} onChange={(e) => setFilterAction(e.target.value)} className="input w-auto">
              <option value="all">All Actions</option>
              <option value="CREATE">Create</option>
              <option value="UPDATE">Update</option>
              <option value="DELETE">Delete</option>
              <option value="ISSUE">Issue</option>
            </select>
          </div>
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="card">
          <EmptyState icon={ShieldCheck} title="No audit logs found" message="Actions will appear here as they happen." />
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-400">
                  <th className="px-4 py-3 font-medium">Action</th>
                  <th className="px-4 py-3 font-medium">Entity</th>
                  <th className="px-4 py-3 font-medium">Details</th>
                  <th className="px-4 py-3 font-medium">Performed By</th>
                  <th className="px-4 py-3 font-medium">Timestamp</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((log) => (
                  <tr key={log.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50/50">
                    <td className="px-4 py-3">
                      <Badge color={actionColor[log.action] ?? 'slate'}>{log.action}</Badge>
                    </td>
                    <td className="px-4 py-3 text-slate-600">{log.entity}</td>
                    <td className="px-4 py-3 text-slate-700 max-w-md">{log.details ?? '—'}</td>
                    <td className="px-4 py-3 text-slate-600">{log.performed_by}</td>
                    <td className="px-4 py-3 text-slate-400 whitespace-nowrap">
                      {new Date(log.created_at).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
