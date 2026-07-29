import { useEffect, useState } from 'react'
import { Plus, Pencil, Trash2, Search } from 'lucide-react'
import { api, type Supply, type SupplyCategory } from '../lib/supabase'
import { PageHeader, Badge, Spinner, stockStatus, Modal, EmptyState } from '../components/ui'

export default function Supplies() {
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [categories, setCategories] = useState<SupplyCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [editItem, setEditItem] = useState<Supply | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  async function load() {
    try {
      const [supData, catData] = await Promise.all([
        api.supplies.list(),
        api.categories.list(),
      ])
      setSupplies(supData)
      setCategories(catData)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const filtered = supplies.filter((s) =>
    s.name.toLowerCase().includes(search.toLowerCase()) ||
    s.sku.toLowerCase().includes(search.toLowerCase())
  )

  async function handleDelete(id: string) {
    if (!confirm('Delete this supply?')) return
    await api.supplies.delete(id)
    setSupplies(supplies.filter((s) => s.id !== id))
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Supplies" subtitle="Manage medical inventory items">
        <button onClick={() => setShowCreate(true)} className="btn-primary">
          <Plus className="h-4 w-4" /> Add Supply
        </button>
      </PageHeader>

      <div className="card mb-4 p-3">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            placeholder="Search by name or SKU..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="input pl-9"
          />
        </div>
      </div>

      {filtered.length === 0 ? (
        <EmptyState message="No supplies found" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">SKU</th>
                  <th className="px-4 py-3">Category</th>
                  <th className="px-4 py-3 text-right">Qty</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((s) => {
                  const status = stockStatus(s.quantity, s.reorder_level)
                  return (
                    <tr key={s.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                      <td className="px-4 py-3 font-medium text-slate-800">{s.name}</td>
                      <td className="px-4 py-3 text-slate-500">{s.sku}</td>
                      <td className="px-4 py-3 text-slate-500">{s.category?.name ?? '...'}</td>
                      <td className="px-4 py-3 text-right font-medium">{s.quantity}</td>
                      <td className="px-4 py-3"><Badge color={status.color}>{status.label}</Badge></td>
                      <td className="px-4 py-3 text-right">
                        <button onClick={() => setEditItem(s)} className="p-1 text-slate-400 hover:text-primary-500"><Pencil className="h-4 w-4" /></button>
                        <button onClick={() => handleDelete(s.id)} className="p-1 text-slate-400 hover:text-danger-500"><Trash2 className="h-4 w-4" /></button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {(showCreate || editItem) && (
        <SupplyModal
          supply={editItem}
          categories={categories}
          onClose={() => { setShowCreate(false); setEditItem(null) }}
          onSaved={async () => {
            setSupplies(await api.supplies.list())
            setShowCreate(false)
            setEditItem(null)
          }}
        />
      )}
    </div>
  )
}

function SupplyModal({ supply, categories, onClose, onSaved }: {
  supply: Supply | null
  categories: SupplyCategory[]
  onClose: () => void
  onSaved: () => void
}) {
  const [form, setForm] = useState({
    name: supply?.name ?? '',
    sku: supply?.sku ?? '',
    category_id: supply?.category_id ?? '',
    quantity: supply?.quantity ?? 0,
    reorder_level: supply?.reorder_level ?? 10,
  })
  const [saving, setSaving] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    try {
      if (supply) {
        await api.supplies.update(supply.id, form)
      } else {
        await api.supplies.create(form)
      }
      onSaved()
    } catch (err) {
      console.error(err)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={supply ? 'Edit Supply' : 'New Supply'} onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">Name</label>
          <input className="input" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">SKU</label>
          <input className="input" value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} required />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-slate-700">Category</label>
          <select className="input" value={form.category_id} onChange={(e) => setForm({ ...form, category_id: e.target.value })}>
            <option value="">None</option>
            {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Quantity</label>
            <input type="number" className="input" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: +e.target.value })} />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">Reorder Level</label>
            <input type="number" className="input" value={form.reorder_level} onChange={(e) => setForm({ ...form, reorder_level: +e.target.value })} />
          </div>
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-outline">Cancel</button>
          <button type="submit" disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Save'}</button>
        </div>
      </form>
    </Modal>
  )
}
