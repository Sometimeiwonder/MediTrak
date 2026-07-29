import { useEffect, useState, useMemo } from 'react'
import { Plus, Search, Pencil, Trash2, Package, Filter, Download } from 'lucide-react'
import { supabase, type Supply, type SupplyCategory } from '../lib/supabase'
import { PageHeader, Badge, Spinner, EmptyState, stockStatus } from '../components/ui'

type SupplyForm = Omit<Supply, 'id' | 'created_at' | 'category'>

const emptyForm: SupplyForm = {
  name: '',
  sku: '',
  category_id: null,
  quantity: 0,
  unit: 'units',
  reorder_level: 0,
  expiry_date: null,
  location: null,
}

export default function Supplies() {
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [categories, setCategories] = useState<SupplyCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [filterCat, setFilterCat] = useState('all')
  const [filterStatus, setFilterStatus] = useState('all')
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Supply | null>(null)
  const [form, setForm] = useState<SupplyForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function loadData() {
    setLoading(true)
    const [supRes, catRes] = await Promise.all([
      supabase.from('supplies').select('*, category:supply_categories(*)').order('name'),
      supabase.from('supply_categories').select('*').order('name'),
    ])
    setSupplies(supRes.data ?? [])
    setCategories(catRes.data ?? [])
    setLoading(false)
  }

  useEffect(() => { loadData() }, [])

  const filtered = useMemo(() => {
    return supplies.filter((s) => {
      const matchesSearch =
        !search ||
        s.name.toLowerCase().includes(search.toLowerCase()) ||
        s.sku.toLowerCase().includes(search.toLowerCase()) ||
        (s.location ?? '').toLowerCase().includes(search.toLowerCase())
      const matchesCat = filterCat === 'all' || s.category_id === filterCat
      const status = stockStatus(s.quantity, s.reorder_level)
      const matchesStatus = filterStatus === 'all' || status.label.toLowerCase().replace(' ', '-') === filterStatus
      return matchesSearch && matchesCat && matchesStatus
    })
  }, [supplies, search, filterCat, filterStatus])

  function openAdd() {
    setEditing(null)
    setForm(emptyForm)
    setError('')
    setModalOpen(true)
  }

  function openEdit(s: Supply) {
    setEditing(s)
    setForm({
      name: s.name, sku: s.sku, category_id: s.category_id,
      quantity: s.quantity, unit: s.unit, reorder_level: s.reorder_level,
      expiry_date: s.expiry_date, location: s.location,
    })
    setError('')
    setModalOpen(true)
  }

  async function handleSave() {
    if (!form.name.trim() || !form.sku.trim()) {
      setError('Name and SKU are required')
      return
    }
    setSaving(true)
    const payload = {
      name: form.name.trim(),
      sku: form.sku.trim(),
      category_id: form.category_id,
      quantity: Number(form.quantity),
      unit: form.unit.trim() || 'units',
      reorder_level: Number(form.reorder_level),
      expiry_date: form.expiry_date || null,
      location: form.location?.trim() || null,
    }
    if (editing) {
      await supabase.from('supplies').update(payload).eq('id', editing.id)
      await supabase.from('audit_logs').insert({
        action: 'UPDATE', entity: 'supply', entity_id: editing.id,
        details: `${editing.name} updated`, performed_by: 'Admin',
      })
    } else {
      const { data } = await supabase.from('supplies').insert(payload).select().single()
      if (data) {
        await supabase.from('audit_logs').insert({
          action: 'CREATE', entity: 'supply', entity_id: data.id,
          details: `${data.name} added to inventory`, performed_by: 'Admin',
        })
      }
    }
    setSaving(false)
    setModalOpen(false)
    loadData()
  }

  async function handleDelete(s: Supply) {
    if (!confirm(`Delete "${s.name}"? This cannot be undone.`)) return
    await supabase.from('supplies').delete().eq('id', s.id)
    await supabase.from('audit_logs').insert({
      action: 'DELETE', entity: 'supply', entity_id: s.id,
      details: `${s.name} removed from inventory`, performed_by: 'Admin',
    })
    loadData()
  }

  function exportCsv() {
    const headers = ['Name', 'SKU', 'Category', 'Quantity', 'Unit', 'Reorder Level', 'Expiry Date', 'Location']
    const rows = filtered.map((s) => [
      s.name, s.sku, s.category?.name ?? '', s.quantity, s.unit, s.reorder_level,
      s.expiry_date ?? '', s.location ?? '',
    ])
    const csv = [headers, ...rows].map((r) => r.map((c) => `"${c}"`).join(',')).join('\n')
    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'supplies.csv'
    a.click()
    URL.revokeObjectURL(url)
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Supplies" subtitle={`${supplies.length} items in inventory`}>
        <button onClick={exportCsv} className="btn-secondary">
          <Download className="h-4 w-4" /> Export
        </button>
        <button onClick={openAdd} className="btn-primary">
          <Plus className="h-4 w-4" /> Add Supply
        </button>
      </PageHeader>

      {/* Filters */}
      <div className="card mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Search by name, SKU, or location..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input pl-10"
            />
          </div>
          <div className="flex items-center gap-2">
            <Filter className="h-4 w-4 text-slate-400" />
            <select value={filterCat} onChange={(e) => setFilterCat(e.target.value)} className="input w-auto">
              <option value="all">All Categories</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)} className="input w-auto">
              <option value="all">All Status</option>
              <option value="in-stock">In Stock</option>
              <option value="low-stock">Low Stock</option>
              <option value="out-of-stock">Out of Stock</option>
            </select>
          </div>
        </div>
      </div>

      {/* Table */}
      {filtered.length === 0 ? (
        <div className="card">
          <EmptyState icon={Package} title="No supplies found" message="Try adjusting your filters or add a new supply." />
        </div>
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-400">
                  <th className="px-4 py-3 font-medium">Name</th>
                  <th className="px-4 py-3 font-medium">SKU</th>
                  <th className="px-4 py-3 font-medium">Category</th>
                  <th className="px-4 py-3 font-medium">Quantity</th>
                  <th className="px-4 py-3 font-medium">Reorder Level</th>
                  <th className="px-4 py-3 font-medium">Location</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((s) => {
                  const status = stockStatus(s.quantity, s.reorder_level)
                  return (
                    <tr key={s.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50/50">
                      <td className="px-4 py-3 font-medium text-slate-800">{s.name}</td>
                      <td className="px-4 py-3 text-slate-500">{s.sku}</td>
                      <td className="px-4 py-3 text-slate-600">{s.category?.name ?? '—'}</td>
                      <td className="px-4 py-3 text-slate-700">{s.quantity.toLocaleString()} {s.unit}</td>
                      <td className="px-4 py-3 text-slate-500">{s.reorder_level}</td>
                      <td className="px-4 py-3 text-slate-500">{s.location ?? '—'}</td>
                      <td className="px-4 py-3"><Badge color={status.color}>{status.label}</Badge></td>
                      <td className="px-4 py-3">
                        <div className="flex justify-end gap-1">
                          <button onClick={() => openEdit(s)} className="rounded-lg p-1.5 text-slate-400 hover:bg-primary-50 hover:text-primary-600" aria-label="Edit">
                            <Pencil className="h-4 w-4" />
                          </button>
                          <button onClick={() => handleDelete(s)} className="rounded-lg p-1.5 text-slate-400 hover:bg-danger-50 hover:text-danger-600" aria-label="Delete">
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Modal */}
      {modalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-4" onClick={() => setModalOpen(false)}>
          <div className="card w-full max-w-lg p-6" onClick={(e) => e.stopPropagation()}>
            <h2 className="mb-4 text-lg font-semibold text-slate-900">{editing ? 'Edit Supply' : 'Add Supply'}</h2>
            {error && <div className="mb-4 rounded-lg bg-danger-50 px-3 py-2 text-sm text-danger-700">{error}</div>}
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <label className="label">Name *</label>
                <input className="input" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="e.g. Nitrile Gloves" />
              </div>
              <div>
                <label className="label">SKU *</label>
                <input className="input" value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} placeholder="e.g. PPE-001" />
              </div>
              <div>
                <label className="label">Category</label>
                <select className="input" value={form.category_id ?? ''} onChange={(e) => setForm({ ...form, category_id: e.target.value || null })}>
                  <option value="">Uncategorized</option>
                  {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="label">Quantity</label>
                <input type="number" className="input" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} />
              </div>
              <div>
                <label className="label">Unit</label>
                <input className="input" value={form.unit} onChange={(e) => setForm({ ...form, unit: e.target.value })} placeholder="units" />
              </div>
              <div>
                <label className="label">Reorder Level</label>
                <input type="number" className="input" value={form.reorder_level} onChange={(e) => setForm({ ...form, reorder_level: Number(e.target.value) })} />
              </div>
              <div>
                <label className="label">Expiry Date</label>
                <input type="date" className="input" value={form.expiry_date ?? ''} onChange={(e) => setForm({ ...form, expiry_date: e.target.value || null })} />
              </div>
              <div className="sm:col-span-2">
                <label className="label">Location</label>
                <input className="input" value={form.location ?? ''} onChange={(e) => setForm({ ...form, location: e.target.value })} placeholder="e.g. Warehouse A-1" />
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button onClick={() => setModalOpen(false)} className="btn-secondary">Cancel</button>
              <button onClick={handleSave} disabled={saving} className="btn-primary">{saving ? 'Saving...' : editing ? 'Update' : 'Add Supply'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
