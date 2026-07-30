import { useEffect, useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { api, type SupplyCategory } from '../lib/supabase'
import { PageHeader, Spinner, Modal, EmptyState } from '../components/ui'
import { FolderOpen } from 'lucide-react'

export default function Categories() {
  const [categories, setCategories] = useState<SupplyCategory[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)

  async function load() {
    try {
      setCategories(await api.categories.list())
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Delete category "${name}"?`)) return
    try {
      await api.categories.delete(id)
      load()
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete')
    }
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Categories" subtitle="Manage supply categories">
        <button onClick={() => setShowCreate(true)} className="btn-primary">
          <Plus className="h-4 w-4" /> Add Category
        </button>
      </PageHeader>

      {categories.length === 0 ? (
        <EmptyState icon={FolderOpen} title="No categories" message="No categories yet" />
      ) : (
        <div className="card overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50 text-left text-xs font-medium text-slate-500">
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3 text-right">Supplies</th>
                  <th className="px-4 py-3 text-right">Inventory Value</th>
                  <th className="px-4 py-3 text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((c) => (
                  <tr key={c.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium text-slate-800">{c.name}</td>
                    <td className="px-4 py-3 text-right text-slate-600">{c.supply_count ?? 0}</td>
                    <td className="px-4 py-3 text-right text-slate-600">${(c.total_inventory_value ?? 0).toLocaleString()}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => handleDelete(c.id, c.name)} className="p-1 text-slate-400 hover:text-danger-500">
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

      {showCreate && (
        <CategoryModal
          onClose={() => setShowCreate(false)}
          onSaved={async () => {
            setCategories(await api.categories.list())
            setShowCreate(false)
          }}
        />
      )}
    </div>
  )
}

function CategoryModal({ onClose, onSaved }: { onClose: () => void; onSaved: () => void }) {
  const [name, setName] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      await api.categories.create({ name })
      onSaved()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="New Category" onClose={onClose}>
      {error && <div className="mb-4 rounded-lg bg-danger-50 p-3 text-sm text-danger-700">{error}</div>}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="label">Name</label>
          <input className="input" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="btn-secondary">Cancel</button>
          <button type="submit" disabled={saving} className="btn-primary">{saving ? 'Saving...' : 'Create'}</button>
        </div>
      </form>
    </Modal>
  )
}
