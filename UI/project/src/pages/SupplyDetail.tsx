import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { ArrowLeft, Settings2, Package } from 'lucide-react'
import { api, type Supply } from '../lib/supabase'
import { PageHeader, Badge, Spinner, stockStatus } from '../components/ui'

export default function SupplyDetail() {
  const { id } = useParams<{ id: string }>()
  const [supply, setSupply] = useState<Supply | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    api.supplies.get(id)
      .then(setSupply)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return <Spinner />
  if (!supply) return <div className="text-center py-16 text-slate-400">Supply not found</div>

  const status = stockStatus(supply.quantity, supply.reorder_level)

  return (
    <div>
      <PageHeader title={supply.name} subtitle={`SKU: ${supply.sku}`}>
        <Link to={`/supplies/${supply.id}/adjust`} className="btn-secondary">
          <Settings2 className="h-4 w-4" /> Adjust Stock
        </Link>
        <Link to="/supplies" className="btn-secondary">
          <ArrowLeft className="h-4 w-4" /> Back
        </Link>
      </PageHeader>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <div className="card p-6">
            <h2 className="mb-4 text-lg font-semibold text-slate-900">Details</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs font-medium text-slate-500">Name</p>
                <p className="text-sm text-slate-800">{supply.name}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">SKU</p>
                <p className="text-sm font-mono text-slate-800">{supply.sku}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">Category</p>
                <p className="text-sm text-slate-800">{supply.category?.name || '...'}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">Supplier</p>
                <p className="text-sm text-slate-800">{supply.supplier || '...'}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">Unit Price</p>
                <p className="text-sm text-slate-800">${supply.unit_price.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">Created</p>
                <p className="text-sm text-slate-800">{new Date(supply.created_at).toLocaleDateString()}</p>
              </div>
              {supply.description && (
                <div className="col-span-2">
                  <p className="text-xs font-medium text-slate-500">Description</p>
                  <p className="text-sm text-slate-800">{supply.description}</p>
                </div>
              )}
            </div>
          </div>

          {supply.description && (
            <div className="card p-6">
              <h2 className="mb-2 text-lg font-semibold text-slate-900">Reorder Suggestion</h2>
              <p className="text-sm text-slate-600">
                {supply.quantity <= 0
                  ? `This item is out of stock. Reorder immediately. Suggested quantity: ${supply.reorder_level * 2} units.`
                  : supply.quantity <= supply.reorder_level
                    ? `Stock is low (${supply.quantity} remaining, minimum: ${supply.reorder_level}). Consider ordering ${(supply.reorder_level * 2) - supply.quantity} more units.`
                    : `Stock is adequate. Current: ${supply.quantity}, Minimum: ${supply.reorder_level}.`
                }
              </p>
            </div>
          )}
        </div>

        <div className="space-y-6">
          <div className="card p-6">
            <h2 className="mb-4 text-lg font-semibold text-slate-900">Stock</h2>
            <div className="text-center">
              <p className="text-4xl font-bold text-slate-900">{supply.quantity}</p>
              <p className="text-sm text-slate-500">units in stock</p>
              <div className="mt-3">
                <Badge color={status.color}>{status.label}</Badge>
              </div>
              <div className="mt-4 space-y-2 text-sm">
                <div className="flex justify-between text-slate-600">
                  <span>Min Stock:</span>
                  <span className="font-medium">{supply.reorder_level}</span>
                </div>
                <div className="flex justify-between text-slate-600">
                  <span>Unit Price:</span>
                  <span className="font-medium">${supply.unit_price.toLocaleString()}</span>
                </div>
                <div className="flex justify-between text-slate-600">
                  <span>Total Value:</span>
                  <span className="font-medium">${(supply.quantity * supply.unit_price).toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>

          {supply.image_url && (
            <div className="card p-6">
              <h2 className="mb-4 text-lg font-semibold text-slate-900">Image</h2>
              <img
                src={`http://localhost:5226${supply.image_url}`}
                alt={supply.name}
                className="w-full rounded-lg object-cover"
              />
            </div>
          )}

          {!supply.image_url && (
            <div className="card p-6">
              <h2 className="mb-4 text-lg font-semibold text-slate-900">Image</h2>
              <div className="flex flex-col items-center justify-center py-8 text-center">
                <Package className="h-12 w-12 text-slate-300" />
                <p className="mt-2 text-sm text-slate-400">No image uploaded</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
