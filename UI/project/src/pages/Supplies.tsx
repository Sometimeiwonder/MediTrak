import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Pencil, Trash2, Search, Eye, Download, ChevronLeft, ChevronRight } from 'lucide-react'
import { api, type Supply, type SupplyCategory, type SupplyListResponse } from '../lib/supabase'
import { PageHeader, Badge, Spinner, stockStatus, Modal, EmptyState } from '../components/ui'
import { Package } from 'lucide-react'

export default function Supplies() {
  const [data, setData] = useState<SupplyListResponse | null>(null)
  const [categories, setCategories] = useState<SupplyCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [stockFilter, setStockFilter] = useState('')
  const [catFilter, setCatFilter] = useState('')
  const [page, setPage] = useState(1)
  const [editItem, setEditItem] = useState<Supply | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [showUpload, setShowUpload] = useState<Supply | null>(null)

  async function load() {
    try {
      const params: Record<string, string | number> = { page, pageSize: 10 }
      if (search) params.search = search
      if (stockFilter) params.stockStatus = stockFilter
      if (catFilter) params.categoryId = Number(catFilter)

      const [supData, catData] = await Promise.all([
        api.supplies.list(params as any),
        api.categories.list(),
      ])
      setData(supData)
      setCategories(catData)
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [page, stockFilter, catFilter])

  function handleSearch() {
    setPage(1)
    load()
  }

  async function handleDelete(id: string) {
    if (!confirm('Move this supply to trash?')) return
    await api.supplies.delete(id)
    load()
  }

  async function handleExport(format: string) {
    try {
      const blob = await api.supplies.export(format)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `supplies_export.${format}`
      a.click()
      URL.revokeObjectURL(url)
    } catch (e) {
      console.error(e)
    }
  }

  if (loading && !data) return <Spinner />

  const items = data?.items || []
  const totalPages = data?.totalPages || 1

  return (
    <div>
      <PageHeader title="Supplies" subtitle="Manage medical inventory items">
        <button onClick={() => handleExport('csv')} className="btn-secondary">
          <Download className="h-4 w-4" /> Export CSV
        </button>
        <button onClick={() => setShowCreate(true)} className="btn-primary">
          <Plus className="h-4 w-4" /> Add Supply
        </button>
      </PageHeader>

      <div className="card mb-4 p-3">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Search by name or SKU..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              className="input pl-9"
            />
          </div>
          <select
            className="input w-auto"
            value={stockFilter}
            onChange={(e) => { setStockFilter(e.target.value); setPage(1) }}
          >
            <option value="">All Status</option>
            <option value="instock">In Stock</option>
            <option value="lowstock">Low Stock</option>
            <option value="outofstock">Out of Stock</option>
          </select>
          <select
            className="input w-auto"
            value={catFilter}
            onChange={(e) => { setCatFilter(e.target.value); setPage(1) }}
          >
            <option value="">All Categories</option>
            {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <button onClick={handleSearch} className="btn-primary">Search</button>
        </div>
      </div>

      {items.length === 0 ? (
        <EmptyState icon={Package} title="No supplies" message="No supplies found" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">SKU</th>
                  <th className="px-4 py-3">Category</th>
                  <th className="px-4 py-3">Supplier</th>
                  <th className="px-4 py-3 text-right">Price</th>
                  <th className="px-4 py-3 text-right">Qty</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((s) => {
                  const status = stockStatus(s.quantity, s.reorder_level)
                  return (
                    <tr key={s.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                      <td className="px-4 py-3">
                        <Link to={`/supplies/${s.id}`} className="font-medium text-primary-600 hover:underline">{s.name}</Link>
                      </td>
                      <td className="px-4 py-3 text-slate-500">{s.sku}</td>
                      <td className="px-4 py-3 text-slate-500">{s.category?.name ?? '...'}</td>
                      <td className="px-4 py-3 text-slate-500">{s.supplier || '...'}</td>
                      <td className="px-4 py-3 text-right text-slate-600">${s.unit_price.toLocaleString()}</td>
                      <td className="px-4 py-3 text-right font-medium">{s.quantity}</td>
                      <td className="px-4 py-3"><Badge color={status.color}>{status.label}</Badge></td>
                      <td className="px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Link to={`/supplies/${s.id}`} className="p-1 text-slate-400 hover:text-primary-500"><Eye className="h-4 w-4" /></Link>
                          <button onClick={() => setShowUpload(s)} className="p-1 text-slate-400 hover:text-accent-500" title="Upload image"><Package className="h-4 w-4" /></button>
                          <button onClick={() => setEditItem(s)} className="p-1 text-slate-400 hover:text-primary-500"><Pencil className="h-4 w-4" /></button>
                          <button onClick={() => handleDelete(s.id)} className="p-1 text-slate-400 hover:text-danger-500"><Trash2 className="h-4 w-4" /></button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
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
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  const start = Math.max(1, Math.min(page - 2, totalPages - 4))
                  const p = start + i
                  if (p > totalPages) return null
                  return (
                    <button
                      key={p}
                      onClick={() => setPage(p)}
                      className={`h-8 w-8 rounded-lg text-sm font-medium ${
                        p === page ? 'bg-primary-600 text-white' : 'text-slate-600 hover:bg-slate-100'
                      }`}
                    >
                      {p}
                    </button>
                  )
                })}
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

      {(showCreate || editItem) && (
        <SupplyModal
          supply={editItem}
          categories={categories}
          onClose={() => { setShowCreate(false); setEditItem(null) }}
          onSaved={() => { load(); setShowCreate(false); setEditItem(null) }}
        />
      )}

      {showUpload && (
        <UploadModal
          supply={showUpload}
          onClose={() => setShowUpload(null)}
          onSaved={() => { load(); setShowUpload(null) }}
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
    supplier: supply?.supplier ?? '',
    unit_price: supply?.unit_price ?? 0,
    description: supply?.description ?? '',
  })
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      if (supply) {
        await api.supplies.update(supply.id, form)
      } else {
        await api.supplies.create(form)
      }
      onSaved()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={supply ? 'Edit Supply' : 'New Supply'} onClose={onClose}>
      {error && <div className="mb-4 rounded-lg bg-danger-50 p-3 text-sm text-danger-700">{error}</div>}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">Name *</label>
            <input className="input" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          </div>
          <div>
            <label className="label">SKU *</label>
            <input className="input" value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} required pattern="^[A-Z0-9\-]+$" title="Uppercase letters, numbers, and hyphens only" />
          </div>
        </div>
        <div>
          <label className="label">Category</label>
          <select className="input" value={form.category_id} onChange={(e) => setForm({ ...form, category_id: e.target.value })}>
            <option value="">None</option>
            {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">Quantity</label>
            <input type="number" min="0" className="input" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: +e.target.value })} />
          </div>
          <div>
            <label className="label">Reorder Level</label>
            <input type="number" min="0" className="input" value={form.reorder_level} onChange={(e) => setForm({ ...form, reorder_level: +e.target.value })} />
          </div>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">Supplier</label>
            <input className="input" value={form.supplier} onChange={(e) => setForm({ ...form, supplier: e.target.value })} />
          </div>
          <div>
            <label className="label">Unit Price ($)</label>
            <input type="number" min="0" step="0.01" className="input" value={form.unit_price} onChange={(e) => setForm({ ...form, unit_price: +e.target.value })} />
          </div>
        </div>
        <div>
          <label className="label">Description</label>
          <textarea className="input" rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancel</button>
          <button type="submit" disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Save'}</button>
        </div>
      </form>
    </Modal>
  )
}

function UploadModal({ supply, onClose, onSaved }: {
  supply: Supply
  onClose: () => void
  onSaved: () => void
}) {
  const [file, setFile] = useState<File | null>(null)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState('')

  async function handleUpload() {
    if (!file) return
    setUploading(true)
    setError('')
    try {
      await api.supplies.uploadImage(supply.id, file)
      onSaved()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed')
    } finally {
      setUploading(false)
    }
  }

  return (
    <Modal title="Upload Image" onClose={onClose}>
      <div className="space-y-4">
        <p className="text-sm text-slate-600">Upload an image for <strong>{supply.name}</strong></p>
        <p className="text-xs text-slate-400">Accepted: JPG, PNG, WEBP. Max 2MB.</p>
        {error && <div className="rounded-lg bg-danger-50 p-3 text-sm text-danger-700">{error}</div>}
        <input
          type="file"
          accept=".jpg,.jpeg,.png,.webp"
          onChange={(e) => setFile(e.target.files?.[0] || null)}
          className="input"
        />
        {supply.image_url && (
          <div>
            <p className="text-xs text-slate-500 mb-1">Current image:</p>
            <img src={`http://localhost:5226${supply.image_url}`} alt="" className="h-20 rounded-lg object-cover" />
          </div>
        )}
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancel</button>
          <button onClick={handleUpload} disabled={!file || uploading} className="btn-primary">
            {uploading ? 'Uploading...' : 'Upload'}
          </button>
        </div>
      </div>
    </Modal>
  )
}
