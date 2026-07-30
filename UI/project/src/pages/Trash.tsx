import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowLeft, RotateCcw } from 'lucide-react'
import { api, type TrashItem } from '../lib/supabase'
import { PageHeader, Spinner, EmptyState } from '../components/ui'
import { Trash2 } from 'lucide-react'

export default function TrashPage() {
  const [items, setItems] = useState<TrashItem[]>([])
  const [loading, setLoading] = useState(true)

  async function load() {
    try {
      setItems(await api.supplies.trash())
    } catch (e) {
      console.error(e)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  async function handleRestore(id: string) {
    if (!confirm('Restore this supply?')) return
    await api.supplies.restore(id)
    load()
  }

  if (loading) return <Spinner />

  return (
    <div>
      <PageHeader title="Trash" subtitle="Soft-deleted supplies">
        <Link to="/supplies" className="btn-secondary">
          <ArrowLeft className="h-4 w-4" /> Back to Supplies
        </Link>
      </PageHeader>

      {items.length === 0 ? (
        <EmptyState icon={Trash2} title="Trash is empty" message="No deleted supplies" />
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
                  <th className="px-4 py-3">Deleted At</th>
                  <th className="px-4 py-3 text-right">Action</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                    <td className="px-4 py-3 font-medium text-slate-800">{item.name}</td>
                    <td className="px-4 py-3 text-slate-500">{item.sku}</td>
                    <td className="px-4 py-3 text-slate-500">{item.category_name}</td>
                    <td className="px-4 py-3 text-right font-medium">{item.quantity}</td>
                    <td className="px-4 py-3 text-slate-400">{item.deleted_at ? new Date(item.deleted_at).toLocaleString() : '...'}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => handleRestore(item.id)} className="btn-secondary text-xs">
                        <RotateCcw className="h-3 w-3" /> Restore
                      </button>
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
