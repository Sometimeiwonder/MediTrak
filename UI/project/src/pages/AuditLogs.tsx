import { useEffect, useState } from 'react'
import { api, type AuditLog } from '../lib/supabase'
import { PageHeader, Spinner, Badge, EmptyState } from '../components/ui'

const actionColors: Record<string, string> = {
  CREATE: 'success',
  INSERT: 'success',
  UPDATE: 'warning',
  DELETE: 'danger',
  LOGIN: 'primary',
  ISSUE: 'accent',
}

export default function AuditLogs() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function load() {
      try {
        setLogs(await api.auditLogs.list())
      } catch (e) {
        console.error(e)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Audit Logs" subtitle="System activity trail" />

      {logs.length === 0 ? (
        <EmptyState message="No audit logs" />
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
                  <th className="px-4 py-3">Date</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((log) => (
                  <tr key={log.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3"><Badge color={actionColors[log.action] || 'default'}>{log.action}</Badge></td>
                    <td className="px-4 py-3 font-medium text-slate-800">{log.entity}</td>
                    <td className="px-4 py-3 text-slate-500 font-mono text-xs">{log.entity_id ?? '...'}</td>
                    <td className="px-4 py-3 text-slate-600">{log.performed_by}</td>
                    <td className="px-4 py-3 text-slate-400">{new Date(log.created_at).toLocaleString()}</td>
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
