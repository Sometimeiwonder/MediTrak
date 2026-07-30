import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Package,
  AlertTriangle,
  ArrowDownToLine,
  Activity,
  TrendingUp,
  Clock,
  Plus,
  DollarSign,
  ShieldAlert,
  ShieldCheck,
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
  LineChart,
  Line,
} from 'recharts'
import { api, type DashboardData, type Supply } from '../lib/supabase'
import { PageHeader, StatCard, Badge, Spinner, stockStatus } from '../components/ui'

const PIE_COLORS = ['#2563eb', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4']

export default function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [supplies, setSupplies] = useState<Supply[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function load() {
      try {
        const [dashData, supData] = await Promise.all([
          api.dashboard.get(),
          api.supplies.list({ pageSize: 200 }),
        ])
        setData(dashData)
        setSupplies(supData.items)
      } catch (e) {
        console.error('Failed to load dashboard data:', e)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <Spinner />
  if (!data) return <div className="text-center py-16 text-slate-400">Failed to load dashboard</div>

  const lowStock = supplies.filter((s) => s.quantity > 0 && s.quantity <= s.reorder_level)

  const stockByCategory = data.categoryStock.map((c) => ({
    name: c.category.length > 15 ? c.category.slice(0, 13) + '...' : c.category,
    units: c.totalQuantity,
  }))

  const stockPieData = [
    { name: 'In Stock', value: data.stockStatus.inStock },
    { name: 'Low Stock', value: data.stockStatus.lowStock },
    { name: 'Out of Stock', value: data.stockStatus.outOfStock },
  ].filter((d) => d.value > 0)

  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Overview of your medical inventory" />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Supplies" value={data.totalSupplies} icon={Package} trend={`${data.totalUnits.toLocaleString()} units in stock`} trendUp color="primary" />
        <StatCard label="Low Stock Alerts" value={data.lowStock} icon={AlertTriangle} trend={`${data.lowStock} items need reordering`} color="warning" />
        <StatCard label="Out of Stock" value={data.outOfStock} icon={AlertTriangle} trend={`${data.outOfStock} items unavailable`} color="danger" />
        <StatCard label="Total Issues" value={data.totalIssues} icon={ArrowDownToLine} trend="All time" color="accent" />
      </div>

      <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard label="Created Today" value={data.createdToday} icon={Plus} color="primary" />
        <StatCard label="Updated Today" value={data.updatedToday} icon={Activity} color="accent" />
        <StatCard label="Inventory Value" value={`$${(data.categoryStock.reduce((s, c) => s + c.totalValue, 0) / 1000).toFixed(1)}k`} icon={DollarSign} color="primary" />
      </div>

      <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard label="Access Denied Today" value={data.accessDeniedToday} icon={ShieldAlert} color="danger" />
        <StatCard label="Sensitive Actions Today" value={data.sensitiveToday} icon={ShieldCheck} color="warning" />
        <StatCard label="Rejected Uploads Today" value={data.rejectedUploadsToday} icon={AlertTriangle} color="danger" />
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
          <h2 className="mb-4 text-base font-semibold text-slate-900">Stock Status</h2>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie data={stockPieData} cx="50%" cy="45%" innerRadius={50} outerRadius={85} paddingAngle={2} dataKey="value">
                {stockPieData.map((_, i) => (
                  <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} />
              <Legend wrapperStyle={{ fontSize: '12px' }} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="card p-5">
          <div className="mb-4 flex items-center gap-2">
            <Activity className="h-5 w-5 text-primary-500" />
            <h2 className="text-base font-semibold text-slate-900">Issues This Week</h2>
          </div>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={data.issueTrend} margin={{ top: 5, right: 10, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
              <XAxis dataKey="day" tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} allowDecimals={false} />
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} cursor={{ fill: '#f8fafc' }} />
              <Bar dataKey="count" fill="#10b981" radius={[6, 6, 0, 0]} barSize={24} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="card p-5">
          <div className="mb-4 flex items-center gap-2">
            <Clock className="h-5 w-5 text-primary-500" />
            <h2 className="text-base font-semibold text-slate-900">Monthly Activity</h2>
          </div>
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={data.monthlyActivity} margin={{ top: 5, right: 10, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: '#64748b' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} allowDecimals={false} />
              <Tooltip contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '13px' }} />
              <Legend wrapperStyle={{ fontSize: '12px' }} />
              <Line type="monotone" dataKey="created" stroke="#2563eb" strokeWidth={2} dot={{ r: 3 }} />
              <Line type="monotone" dataKey="updated" stroke="#10b981" strokeWidth={2} dot={{ r: 3 }} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      {lowStock.length > 0 && (
        <div className="mt-6 card p-5">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-warning-500" />
              <h2 className="text-base font-semibold text-slate-900">Low Stock Alerts</h2>
            </div>
            <Link to="/supplies?stockStatus=lowstock" className="text-sm text-primary-600 hover:underline">View all</Link>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {lowStock.slice(0, 6).map((s) => {
              const status = stockStatus(s.quantity, s.reorder_level)
              return (
                <Link to={`/supplies/${s.id}`} key={s.id} className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50 p-3 hover:bg-slate-100 transition-colors">
                  <div>
                    <p className="text-sm font-medium text-slate-800">{s.name}</p>
                    <p className="text-xs text-slate-400">SKU: {s.sku}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-semibold text-slate-700">{s.quantity}</p>
                    <Badge color={status.color}>{status.label}</Badge>
                  </div>
                </Link>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
