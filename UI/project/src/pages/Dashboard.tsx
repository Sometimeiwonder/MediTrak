import { useEffect, useState } from 'react'
import {
  Package,
  AlertTriangle,
  ArrowDownToLine,
  Activity,
  TrendingUp,
  Clock,
} from 'lucide-react'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts'
import { api, type Supply, type Issue, type SupplyCategory } from '../lib/supabase'
import { PageHeader, StatCard, Badge, Spinner, stockStatus } from '../components/ui'

const PIE_COLORS = ['#2563eb', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4']

export default function Dashboard() {
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [issues, setIssues] = useState<Issue[]>([])
  const [categories, setCategories] = useState<SupplyCategory[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function load() {
      try {
        const [supData, issData, catData] = await Promise.all([
          api.supplies.list(),
          api.issues.list(),
          api.categories.list(),
        ])
        setSupplies(supData)
        setIssues(issData)
        setCategories(catData)
      } catch (e) {
        console.error('Failed to load dashboard data:', e)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <Spinner />

  const totalItems = supplies.length
  const lowStock = supplies.filter((s) => s.quantity > 0 && s.quantity <= s.reorder_level)
  const outOfStock = supplies.filter((s) => s.quantity <= 0)
  const totalUnits = supplies.reduce((sum, s) => sum + s.quantity, 0)
  const recentIssues = issues

  const categoryData = categories.map((cat) => {
    const count = supplies.filter((s) => s.category_id === cat.id).length
    return { name: cat.name, value: count }
  }).filter((d) => d.value > 0)

  const stockByCategory = categories.map((cat) => {
    const items = supplies.filter((s) => s.category_id === cat.id)
    return {
      name: cat.name.length > 15 ? cat.name.slice(0, 13) + '...' : cat.name,
      units: items.reduce((sum, s) => sum + s.quantity, 0),
    }
  }).filter((d) => d.units > 0)

  const issueTrend = Array.from({ length: 7 }, (_, i) => {
    const d = new Date()
    d.setDate(d.getDate() - (6 - i))
    const dayStr = d.toLocaleDateString('en-US', { weekday: 'short' })
    const count = issues.filter((iss) => {
      const issDate = new Date(iss.created_at)
      return issDate.toDateString() === d.toDateString()
    }).length
    return { day: dayStr, issues: count }
  })

  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Overview of your medical inventory" />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Supplies" value={totalItems} icon={Package} trend={`${totalUnits.toLocaleString()} units in stock`} trendUp color="primary" />
        <StatCard label="Low Stock Alerts" value={lowStock.length} icon={AlertTriangle} trend={`${lowStock.length} items need reordering`} color="warning" />
        <StatCard label="Out of Stock" value={outOfStock.length} icon={AlertTriangle} trend={`${outOfStock.length} items unavailable`} color="danger" />
        <StatCard label="Recent Issues" value={issues.length} icon={ArrowDownToLine} trend="Last 10 transactions" color="accent" />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="card p-5 lg:col-span-2">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-base font-semibold text-slate-900">Stock by Category</h2>
            <TrendingUp className="h-5 w-5 text-slate-400" />
          </div>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={stockByCategory} margin={{ top: 5, right: 10, left: -10, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
              <XAxis dataKey="name" tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} cursor={{ fill: '#f8fafc' }} />
              <Bar dataKey="units" fill="#2563eb" radius={[6, 6, 0, 0]} barSize={36} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="card p-5">
          <h2 className="mb-4 text-base font-semibold text-slate-900">Category Distribution</h2>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie data={categoryData} cx="50%" cy="45%" innerRadius={50} outerRadius={85} paddingAngle={2} dataKey="value">
                {categoryData.map((_, i) => (
                  <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} />
              <Legend wrapperStyle={{ fontSize: '12px' }} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="card p-5 lg:col-span-1">
          <div className="mb-4 flex items-center gap-2">
            <Activity className="h-5 w-5 text-primary-500" />
            <h2 className="text-base font-semibold text-slate-900">Issues This Week</h2>
          </div>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={issueTrend} margin={{ top: 5, right: 10, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
              <XAxis dataKey="day" tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} allowDecimals={false} />
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} cursor={{ fill: '#f8fafc' }} />
              <Bar dataKey="issues" fill="#10b981" radius={[6, 6, 0, 0]} barSize={24} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="card p-5 lg:col-span-2">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-base font-semibold text-slate-900">Recent Issues</h2>
            <Clock className="h-5 w-5 text-slate-400" />
          </div>
          {recentIssues.length === 0 ? (
            <p className="py-8 text-center text-sm text-slate-400">No recent issues</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-100 text-left text-xs font-medium text-slate-400">
                    <th className="pb-2 pr-4 font-medium">Supply</th>
                    <th className="pb-2 pr-4 font-medium">Qty</th>
                    <th className="pb-2 pr-4 font-medium">Issued To</th>
                    <th className="pb-2 font-medium">Date</th>
                  </tr>
                </thead>
                <tbody>
                  {recentIssues.slice(0, 6).map((iss) => (
                    <tr key={iss.id} className="border-b border-slate-50 last:border-0">
                      <td className="py-2.5 pr-4 font-medium text-slate-700">{iss.supply?.name ?? '...'}</td>
                      <td className="py-2.5 pr-4 text-slate-600">{iss.quantity}</td>
                      <td className="py-2.5 pr-4 text-slate-600">{iss.issued_to}</td>
                      <td className="py-2.5 text-slate-400">{new Date(iss.created_at).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {lowStock.length > 0 && (
        <div className="mt-6 card p-5">
          <div className="mb-4 flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-warning-500" />
            <h2 className="text-base font-semibold text-slate-900">Low Stock Alerts</h2>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {lowStock.map((s) => {
              const status = stockStatus(s.quantity, s.reorder_level)
              return (
                <div key={s.id} className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50 p-3">
                  <div>
                    <p className="text-sm font-medium text-slate-800">{s.name}</p>
                    <p className="text-xs text-slate-400">SKU: {s.sku}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-semibold text-slate-700">{s.quantity} {s.unit}</p>
                    <Badge color={status.color}>{status.label}</Badge>
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
