import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Eye, ChevronLeft, ChevronRight } from 'lucide-react'
import { api, type IssueListResponse, type Supply } from '../lib/supabase'
import { PageHeader, Spinner, Modal, EmptyState } from '../components/ui'
import { ArrowDownToLine } from 'lucide-react'

export default function Issues() {
  const [data, setData] = useState<IssueListResponse | null>(null)
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)
  const [showCreate, setShowCreate] = useState(false)

  async function load() {
    try {
      const [issData, supData] = await Promise.all([
        api.issues.list({ page, pageSize: 10 }),
        api.supplies.list({ pageSize: 200 }),
      ])
      setData(issData)
      setSupplies(supData.items)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [page])

  if (loading && !data) return <Spinner />

  const items = data?.items || []
  const totalPages = data?.totalPages || 1

  return (
    <div>
      <PageHeader title="Issues" subtitle="Track supply issuances">
        <button onClick={() => setShowCreate(true)} className="btn-primary">
          <Plus className="h-4 w-4" /> New Issue
        </button>
      </PageHeader>

      {items.length === 0 ? (
        <EmptyState icon={ArrowDownToLine} title="No issues" message="No issues recorded" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">ID</th>
                  <th className="px-4 py-3">Issued To</th>
                  <th className="px-4 py-3 text-right">Items</th>
                  <th className="px-4 py-3 text-right">Total</th>
                  <th className="px-4 py-3">Date</th>
                  <th className="px-4 py-3 text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                {items.map((iss) => (
                  <tr key={iss.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3 font-mono text-xs text-slate-500">#{iss.id}</td>
                    <td className="px-4 py-3 font-medium text-slate-800">{iss.issued_to}</td>
                    <td className="px-4 py-3 text-right text-slate-600">{iss.item_count}</td>
                    <td className="px-4 py-3 text-right font-medium">${iss.total_amount.toLocaleString()}</td>
                    <td className="px-4 py-3 text-slate-400">{new Date(iss.issued_at).toLocaleDateString()}</td>
                    <td className="px-4 py-3 text-right">
                      <Link to={`/issues/${iss.id}`} className="p-1 text-slate-400 hover:text-primary-500">
                        <Eye className="h-4 w-4" />
                      </Link>
                    </td>
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

      {showCreate && (
        <IssueModal
          supplies={supplies}
          onClose={() => setShowCreate(false)}
          onSaved={() => { load(); setShowCreate(false) }}
        />
      )}
    </div>
  )
}

type IssueItemForm = { supply_id: string; quantity: number }

function IssueModal({ supplies, onClose, onSaved }: {
  supplies: Supply[]
  onClose: () => void
  onSaved: () => void
}) {
  const [issuedTo, setIssuedTo] = useState('')
  const [items, setItems] = useState<IssueItemForm[]>([{ supply_id: '', quantity: 1 }])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  function addItem() {
    setItems([...items, { supply_id: '', quantity: 1 }])
  }

  function removeItem(idx: number) {
    if (items.length <= 1) return
    setItems(items.filter((_, i) => i !== idx))
  }

  function updateItem(idx: number, field: keyof IssueItemForm, value: string | number) {
    const next = [...items]
    next[idx] = { ...next[idx], [field]: value }
    setItems(next)
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      const validItems = items.filter((i) => i.supply_id && i.quantity > 0)
      if (validItems.length === 0) {
        setError('Add at least one item')
        setSaving(false)
        return
      }
      await api.issues.create({ issued_to: issuedTo, items: validItems })
      onSaved()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create issue')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="New Issue" onClose={onClose}>
      {error && <div className="mb-4 rounded-lg bg-danger-50 p-3 text-sm text-danger-700">{error}</div>}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="label">Issued To *</label>
          <input className="input" value={issuedTo} onChange={(e) => setIssuedTo(e.target.value)} required placeholder="Recipient name" />
        </div>

        <div>
          <div className="flex items-center justify-between mb-2">
            <label className="label mb-0">Items</label>
            <button type="button" onClick={addItem} className="text-xs text-primary-600 hover:underline">+ Add item</button>
          </div>
          <div className="space-y-2">
            {items.map((item, idx) => (
              <div key={idx} className="flex items-center gap-2">
                <select
                  className="input flex-1"
                  value={item.supply_id}
                  onChange={(e) => updateItem(idx, 'supply_id', e.target.value)}
                  required
                >
                  <option value="">Select supply...</option>
                  {supplies.filter((s) => s.quantity > 0).map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.quantity} available)</option>
                  ))}
                </select>
                <input
                  type="number"
                  min="1"
                  className="input w-24"
                  value={item.quantity}
                  onChange={(e) => updateItem(idx, 'quantity', +e.target.value)}
                  required
                />
                {items.length > 1 && (
                  <button type="button" onClick={() => removeItem(idx)} className="text-slate-400 hover:text-danger-500 text-xs">&times;</button>
                )}
              </div>
            ))}
          </div>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancel</button>
          <button type="submit" disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Create Issue'}</button>
        </div>
      </form>
    </Modal>
  )
}
