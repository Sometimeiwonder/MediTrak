import { useEffect, useState, useMemo } from 'react'
import { Plus, Search, ArrowDownToLine, Trash2 } from 'lucide-react'
import { supabase, type Issue, type Supply } from '../lib/supabase'
import { PageHeader, Badge, Spinner, EmptyState } from '../components/ui'

type IssueForm = {
  supply_id: string
  quantity: number
  issued_to: string
  issued_by: string
  notes: string
}

const emptyForm: IssueForm = { supply_id: '', quantity: 1, issued_to: '', issued_by: 'Admin', notes: '' }

export default function Issues() {
  const [issues, setIssues] = useState<Issue[]>([])
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState<IssueForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function loadData() {
    setLoading(true)
    const [issRes, supRes] = await Promise.all([
      supabase.from('issues').select('*, supply:supplies(*)').order('created_at', { ascending: false }),
      supabase.from('supplies').select('*').order('name'),
    ])
    setIssues(issRes.data ?? [])
    setSupplies(supRes.data ?? [])
    setLoading(false)
  }

  useEffect(() => { loadData() }, [])

  const filtered = useMemo(() => {
    if (!search) return issues
    return issues.filter((i) =>
      (i.supply?.name ?? '').toLowerCase().includes(search.toLowerCase()) ||
      i.issued_to.toLowerCase().includes(search.toLowerCase()) ||
      i.issued_by.toLowerCase().includes(search.toLowerCase())
    )
  }, [issues, search])

  async function handleSave() {
    if (!form.supply_id || !form.issued_to.trim()) {
      setError('Supply and recipient are required')
      return
    }
    setSaving(true)
    const supply = supplies.find((s) => s.id === form.supply_id)
    if (supply && form.quantity > supply.quantity) {
      setError(`Only ${supply.quantity} ${supply.unit} available`)
      setSaving(false)
      return
    }
    const { data } = await supabase.from('issues').insert({
      supply_id: form.supply_id,
      quantity: Number(form.quantity),
      issued_to: form.issued_to.trim(),
      issued_by: form.issued_by.trim(),
      notes: form.notes.trim() || null,
    }).select().single()
    if (data && supply) {
      await supabase.from('supplies').update({ quantity: supply.quantity - Number(form.quantity) }).eq('id', supply.id)
      await supabase.from('audit_logs').insert({
        action: 'ISSUE', entity: 'supply', entity_id: supply.id,
        details: `${form.quantity} ${supply.unit} of ${supply.name} issued to ${form.issued_to}`,
        performed_by: form.issued_by,
      })
    }
    setSaving(false)
    setModalOpen(false)
    setForm(emptyForm)
    setError('')
    loadData()
  }

  async function handleDelete(iss: Issue) {
    if (!confirm('Delete this issue record? The stock will be returned to inventory.')) return
    await supabase.from('issues').delete().eq('id', iss.id)
    if (iss.supply) {
      await supabase.from('supplies').update({ quantity: iss.supply.quantity + iss.quantity }).eq('id', iss.supply_id)
    }
    loadData()
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Issues" subtitle="Track supplies issued to departments and staff">
        <button onClick={() => { setForm(emptyForm); setError(''); setModalOpen(true) }} className="btn-primary">
          <Plus className="h-4 w-4" /> New Issue
        </button>
      </PageHeader>

      <div className="card mb-4 p-4">
        <div className="relative">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            placeholder="Search by supply, recipient, or issuer..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="input pl-10"
          />
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="card">
          <EmptyState icon={ArrowDownToLine} title="No issues found" message="Record a new issue to track supplies going out." />
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-400">
                  <th className="px-4 py-3 font-medium">Supply</th>
                  <th className="px-4 py-3 font-medium">Quantity</th>
                  <th className="px-4 py-3 font-medium">Issued To</th>
                  <th className="px-4 py-3 font-medium">Issued By</th>
                  <th className="px-4 py-3 font-medium">Notes</th>
                  <th className="px-4 py-3 font-medium">Date</th>
                  <th className="px-4 py-3 font-medium text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((iss) => (
                  <tr key={iss.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50/50">
                    <td className="px-4 py-3 font-medium text-slate-800">{iss.supply?.name ?? '—'}</td>
                    <td className="px-4 py-3">
                      <Badge color="primary">{iss.quantity} {iss.supply?.unit ?? ''}</Badge>
                    </td>
                    <td className="px-4 py-3 text-slate-600">{iss.issued_to}</td>
                    <td className="px-4 py-3 text-slate-600">{iss.issued_by}</td>
                    <td className="px-4 py-3 text-slate-500 max-w-xs truncate">{iss.notes ?? '—'}</td>
                    <td className="px-4 py-3 text-slate-400">{new Date(iss.created_at).toLocaleDateString()}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => handleDelete(iss)} className="rounded-lg p-1.5 text-slate-400 hover:bg-danger-50 hover:text-danger-600" aria-label="Delete">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-4" onClick={() => setModalOpen(false)}>
          <div className="card w-full max-w-md p-6" onClick={(e) => e.stopPropagation()}>
            <h2 className="mb-4 text-lg font-semibold text-slate-900">New Issue</h2>
            {error && <div className="mb-4 rounded-lg bg-danger-50 px-3 py-2 text-sm text-danger-700">{error}</div>}
            <div className="space-y-4">
              <div>
                <label className="label">Supply *</label>
                <select className="input" value={form.supply_id} onChange={(e) => setForm({ ...form, supply_id: e.target.value })}>
                  <option value="">Select a supply...</option>
                  {supplies.map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.quantity} {s.unit} avail.)</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="label">Quantity *</label>
                <input type="number" min="1" className="input" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} />
              </div>
              <div>
                <label className="label">Issued To *</label>
                <input className="input" value={form.issued_to} onChange={(e) => setForm({ ...form, issued_to: e.target.value })} placeholder="e.g. Emergency Dept" />
              </div>
              <div>
                <label className="label">Issued By</label>
                <input className="input" value={form.issued_by} onChange={(e) => setForm({ ...form, issued_by: e.target.value })} />
              </div>
              <div>
                <label className="label">Notes</label>
                <textarea className="input min-h-[80px]" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} placeholder="Optional notes..." />
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button onClick={() => setModalOpen(false)} className="btn-secondary">Cancel</button>
              <button onClick={handleSave} disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Record Issue'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
