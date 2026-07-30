import { useEffect, useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { api, type Supply } from '../lib/supabase'
import { PageHeader, Spinner } from '../components/ui'

export default function AdjustStock() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [supply, setSupply] = useState<Supply | null>(null)
  const [loading, setLoading] = useState(true)
  const [adjustment, setAdjustment] = useState(0)
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    api.supplies.get(id)
      .then(setSupply)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!id || !supply) return
    setSaving(true)
    setError('')
    try {
      await api.supplies.adjust(id, adjustment, note)
      navigate(`/supplies/${id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to adjust stock')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <Spinner />
  if (!supply) return <div className="text-center py-16 text-slate-400">Supply not found</div>

  const newQty = supply.quantity + adjustment

  return (
    <div>
      <PageHeader title={`Adjust Stock: ${supply.name}`} subtitle={`Current quantity: ${supply.quantity}`}>
        <Link to={`/supplies/${supply.id}`} className="btn-secondary">
          <ArrowLeft className="h-4 w-4" /> Back
        </Link>
      </PageHeader>

      <div className="max-w-lg">
        <div className="card p-6">
          {error && <div className="mb-4 rounded-lg bg-danger-50 p-3 text-sm text-danger-700">{error}</div>}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="label">Adjustment Amount</label>
              <p className="text-xs text-slate-400 mb-1">Positive to add, negative to remove</p>
              <input
                type="number"
                className="input"
                value={adjustment}
                onChange={(e) => setAdjustment(+e.target.value)}
                required
              />
            </div>

            <div>
              <label className="label">Note (optional)</label>
              <input
                type="text"
                className="input"
                placeholder="Reason for adjustment..."
                value={note}
                onChange={(e) => setNote(e.target.value)}
              />
            </div>

            <div className="rounded-lg bg-slate-50 p-4">
              <div className="flex justify-between text-sm">
                <span className="text-slate-500">Current Quantity</span>
                <span className="font-medium">{supply.quantity}</span>
              </div>
              <div className="flex justify-between text-sm mt-1">
                <span className="text-slate-500">Adjustment</span>
                <span className={`font-medium ${adjustment >= 0 ? 'text-accent-600' : 'text-danger-600'}`}>
                  {adjustment >= 0 ? '+' : ''}{adjustment}
                </span>
              </div>
              <div className="border-t border-slate-200 mt-2 pt-2 flex justify-between">
                <span className="text-sm font-medium text-slate-700">New Quantity</span>
                <span className={`text-lg font-bold ${newQty < 0 ? 'text-danger-600' : 'text-slate-900'}`}>{newQty}</span>
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <button type="button" onClick={() => navigate(`/supplies/${supply.id}`)} className="btn-secondary">Cancel</button>
              <button type="submit" disabled={saving || adjustment === 0} className="btn-primary">
                {saving ? 'Saving...' : 'Apply Adjustment'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
