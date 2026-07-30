import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { api, type Issue } from '../lib/supabase'
import { PageHeader, Spinner } from '../components/ui'

export default function IssueDetail() {
  const { id } = useParams<{ id: string }>()
  const [issue, setIssue] = useState<Issue | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    api.issues.get(id)
      .then(setIssue)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return <Spinner />
  if (!issue) return <div className="text-center py-16 text-slate-400">Issue not found</div>

  return (
    <div>
      <PageHeader title={`Issue #${issue.id}`} subtitle={`Issued to ${issue.issued_to}`}>
        <Link to="/issues" className="btn-secondary">
          <ArrowLeft className="h-4 w-4" /> Back
        </Link>
      </PageHeader>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <div className="card p-6">
            <h2 className="mb-4 text-lg font-semibold text-slate-900">Issue Items</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-100 text-left text-xs font-medium text-slate-500">
                    <th className="pb-2 pr-4">Supply</th>
                    <th className="pb-2 pr-4">SKU</th>
                    <th className="pb-2 pr-4 text-right">Qty</th>
                    <th className="pb-2 pr-4 text-right">Unit Price</th>
                    <th className="pb-2 text-right">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {issue.items.map((item, idx) => (
                    <tr key={idx} className="border-b border-slate-50 last:border-0">
                      <td className="py-3 pr-4 font-medium text-slate-800">
                        <Link to={`/supplies/${item.supply_id}`} className="text-primary-600 hover:underline">
                          {item.supply_name}
                        </Link>
                      </td>
                      <td className="py-3 pr-4 text-slate-500 font-mono text-xs">{item.supply_code}</td>
                      <td className="py-3 pr-4 text-right">{item.quantity}</td>
                      <td className="py-3 pr-4 text-right">${item.unit_price?.toLocaleString() ?? '0'}</td>
                      <td className="py-3 text-right font-medium">${item.subtotal?.toLocaleString() ?? '0'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div>
          <div className="card p-6">
            <h2 className="mb-4 text-lg font-semibold text-slate-900">Summary</h2>
            <div className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-slate-500">Issued To</span>
                <span className="font-medium text-slate-800">{issue.issued_to}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-slate-500">Date</span>
                <span className="text-slate-800">{new Date(issue.issued_at).toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-slate-500">Total Items</span>
                <span className="font-medium text-slate-800">{issue.item_count}</span>
              </div>
              <div className="border-t border-slate-100 pt-3">
                <div className="flex justify-between">
                  <span className="text-sm font-medium text-slate-700">Total Amount</span>
                  <span className="text-lg font-bold text-slate-900">${issue.total_amount.toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
