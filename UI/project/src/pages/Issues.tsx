import { useEffect, useState } from 'react'
import { Plus } from 'lucide-react'
import { api, type Issue, type Supply } from '../lib/supabase'
import { PageHeader, Spinner, Modal, EmptyState } from '../components/ui'

export default function Issues() {
  const [issues, setIssues] = useState<Issue[]>([])
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)

  async function load() {
    try {
      const [issData, supData] = await Promise.all([
        api.issues.list(),
        api.supplies.list(),
      ])
      setIssues(issData)
      setSupplies(supData)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Issues" subtitle="Track supply issuances">
        <button onClick={() => setShowCreate(true)} className="btn-primary">
          <Plus className="h-4 w-4" /> New Issue
        </button>
      </PageHeader>

      {issues.length === 0 ? (
        <EmptyState message="No issues recorded" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">Supply</th>
                  <th className="px-4 py-3 text-right">Qty</th>
                  <th className="px-4 py-3">Issued To</th>
                  <th className="px-4 py-3">Date</th>
                </tr>
              </thead>
              <tbody>
                {issues.map((iss) => (
                  <tr key={iss.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium text-slate-800">{iss.supply?.name ?? '...'}</td>
                    <td className="px-4 py-3 text-right">{iss.quantity}</td>
                    <td className="px-4 py-3 text-slate-600">{iss.issued_to}</td>
                    <td className="px-4 py-3 text-slate-400">{new Date(iss.created_at).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {showCreate && (
        <IssueModal
          supplies={supplies}
          onClose={() => setShowCreate(false)}
          onSaved={async () => {
            setIssues(await api.issues.list())
            setShowCreate(false)
          }}
        />
      )}
    </div>
  )
}

function IssueModal({ supplies, onClose, onSaved }: {
  supplies: Supply[]
  onClose: () => void
  onSaved: () => void
}) {
  const [form, setForm] = useState({ supply_id: '', quantity: 1, issued_to: '' })
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    try {
      await api.issues.create(form)
      onSaved()
    } catch (err) {
      console.error(err)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="New Issue" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">Supply</label>
          <select className="input" value={form.supply_id} onChange={(e) => setForm({ ...form, supply_id: e.target.value })} required>
            <option value="">Select supply...</option>
            {supplies.filter((s) => s.quantity > 0).map((s) => (
              <option key={s.id} value={s.id}>{s.name} ({s.quantity} available)</option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">Quantity</label>
          <input type="number" min="1" className="input" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: +e.target.value })} required />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">Issued To</label>
          <input className="input" value={form.issued_to} onChange={(e) => setForm({ ...form, issued_to: e.target.value })} required />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-outline">Cancel</button>
          <button type="submit" disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Create'}</button>
        </div>
      </form>
    </Modal>
  )
}
