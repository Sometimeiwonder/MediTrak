import { useEffect, useState } from 'react'
import { Plus, Pencil, Trash2, FolderTree, Package } from 'lucide-react'
import { supabase, type SupplyCategory, type Supply } from '../lib/supabase'
import { PageHeader, Spinner, EmptyState } from '../components/ui'

const emptyForm = { name: '', description: '' }

export default function Categories() {
  const [categories, setCategories] = useState<(SupplyCategory & { supply_count?: number })[]>([])
  const [loading, setLoading] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<SupplyCategory | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function loadData() {
    setLoading(true)
    const [catRes, supRes] = await Promise.all([
      supabase.from('supply_categories').select('*').order('name'),
      supabase.from('supplies').select('category_id'),
    ])
    const cats = (catRes.data ?? []) as SupplyCategory[]
    const sups = (supRes.data ?? []) as Pick<Supply, 'category_id'>[]
    const withCounts = cats.map((c) => ({
      ...c,
      supply_count: sups.filter((s) => s.category_id === c.id).length,
    }))
    setCategories(withCounts)
    setLoading(false)
  }

  useEffect(() => { loadData() }, [])

  function openAdd() { setEditing(null); setForm(emptyForm); setError(''); setModalOpen(true) }
  function openEdit(c: SupplyCategory) { setEditing(c); setForm({ name: c.name, description: c.description ?? '' }); setError(''); setModalOpen(true) }

  async function handleSave() {
    if (!form.name.trim()) { setError('Name is required'); return }
    setSaving(true)
    if (editing) {
      await supabase.from('supply_categories').update({
        name: form.name.trim(),
        description: form.description.trim() || null,
      }).eq('id', editing.id)
    } else {
      await supabase.from('supply_categories').insert({
        name: form.name.trim(),
        description: form.description.trim() || null,
      })
    }
    setSaving(false)
    setModalOpen(false)
    loadData()
  }

  async function handleDelete(c: SupplyCategory) {
    if (!confirm(`Delete category "${c.name}"? Supplies in this category will become uncategorized.`)) return
    await supabase.from('supply_categories').delete().eq('id', c.id)
    loadData()
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Categories" subtitle={`${categories.length} supply categories`}>
        <button onClick={openAdd} className="btn-primary">
          <Plus className="h-4 w-4" /> Add Category
        </button>
      </PageHeader>

      {categories.length === 0 ? (
        <div className="card">
          <EmptyState icon={FolderTree} title="No categories yet" message="Create categories to organize your supplies." />
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {categories.map((c) => (
            <div key={c.id} className="card p-5">
              <div className="flex items-start justify-between">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-50">
                    <FolderTree className="h-5 w-5 text-primary-600" />
                  </div>
                  <div>
                    <h3 className="text-base font-semibold text-slate-900">{c.name}</h3>
                    <p className="mt-0.5 flex items-center gap-1 text-xs text-slate-400">
                      <Package className="h-3 w-3" /> {c.supply_count ?? 0} supplies
                    </p>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button onClick={() => openEdit(c)} className="rounded-lg p-1.5 text-slate-400 hover:bg-primary-50 hover:text-primary-600">
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button onClick={() => handleDelete(c)} className="rounded-lg p-1.5 text-slate-400 hover:bg-danger-50 hover:text-danger-600">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              {c.description && <p className="mt-3 text-sm text-slate-500">{c.description}</p>}
            </div>
          ))}
        </div>
      )}

      {modalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm p-4" onClick={() => setModalOpen(false)}>
          <div className="card w-full max-w-md p-6" onClick={(e) => e.stopPropagation()}>
            <h2 className="mb-4 text-lg font-semibold text-slate-900">{editing ? 'Edit Category' : 'Add Category'}</h2>
            {error && <div className="mb-4 rounded-lg bg-danger-50 px-3 py-2 text-sm text-danger-700">{error}</div>}
            <div className="space-y-4">
              <div>
                <label className="label">Name *</label>
                <input className="input" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="e.g. PPE" />
              </div>
              <div>
                <label className="label">Description</label>
                <textarea className="input min-h-[80px]" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder="Optional description..." />
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button onClick={() => setModalOpen(false)} className="btn-secondary">Cancel</button>
              <button onClick={handleSave} disabled={saving} className="btn-primary">{saving ? 'Saving...' : editing ? 'Update' : 'Add'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
