import { type ReactNode } from 'react'

export function PageHeader({ title, subtitle, children }: { title: string; subtitle?: string; children?: ReactNode }) {
  return (
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
      </div>
      {children && <div className="flex items-center gap-2">{children}</div>}
    </div>
  )
}

export function StatCard({
  label,
  value,
  icon: Icon,
  trend,
  trendUp,
  color = 'primary',
}: {
  label: string
  value: string | number
  icon: React.ComponentType<{ className?: string }>
  trend?: string
  trendUp?: boolean
  color?: 'primary' | 'accent' | 'warning' | 'danger'
}) {
  const colorMap = {
    primary: 'bg-primary-50 text-primary-600',
    accent: 'bg-accent-50 text-accent-600',
    warning: 'bg-warning-50 text-warning-600',
    danger: 'bg-danger-50 text-danger-600',
  }
  return (
    <div className="card p-5">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-slate-500">{label}</span>
        <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${colorMap[color]}`}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
      <p className="mt-3 text-3xl font-semibold text-slate-900">{value}</p>
      {trend && (
        <p className={`mt-1.5 text-xs font-medium ${trendUp ? 'text-accent-600' : 'text-danger-600'}`}>
          {trend}
        </p>
      )}
    </div>
  )
}

export function Badge({ children, color = 'slate' }: { children: ReactNode; color?: 'slate' | 'accent' | 'warning' | 'danger' | 'primary' }) {
  const colorMap = {
    slate: 'bg-slate-100 text-slate-600',
    accent: 'bg-accent-100 text-accent-700',
    warning: 'bg-warning-100 text-warning-700',
    danger: 'bg-danger-100 text-danger-700',
    primary: 'bg-primary-100 text-primary-700',
  }
  return <span className={`badge ${colorMap[color]}`}>{children}</span>
}

export function EmptyState({ icon: Icon, title, message }: { icon: React.ComponentType<{ className?: string }>; title: string; message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="flex h-14 w-14 items-center justify-center rounded-full bg-slate-100">
        <Icon className="h-7 w-7 text-slate-400" />
      </div>
      <h3 className="mt-4 text-base font-medium text-slate-900">{title}</h3>
      <p className="mt-1 text-sm text-slate-500">{message}</p>
    </div>
  )
}

export function Spinner() {
  return (
    <div className="flex items-center justify-center py-16">
      <div className="h-8 w-8 animate-spin rounded-full border-2 border-slate-200 border-t-primary-600" />
    </div>
  )
}

export function stockStatus(qty: number, reorder: number): { label: string; color: 'accent' | 'warning' | 'danger' } {
  if (qty <= 0) return { label: 'Out of stock', color: 'danger' }
  if (qty <= reorder) return { label: 'Low stock', color: 'warning' }
  return { label: 'In stock', color: 'accent' }
}

export function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: ReactNode }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40" onClick={onClose}>
      <div className="w-full max-w-lg rounded-xl bg-white p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">&times;</button>
        </div>
        {children}
      </div>
    </div>
  )
}
