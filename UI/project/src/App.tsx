import { useState, useEffect, createContext, useContext } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import Sidebar from './components/Sidebar'
import Topbar from './components/Topbar'
import Dashboard from './pages/Dashboard'
import Supplies from './pages/Supplies'
import SupplyDetail from './pages/SupplyDetail'
import Trash from './pages/Trash'
import AdjustStock from './pages/AdjustStock'
import Issues from './pages/Issues'
import IssueDetail from './pages/IssueDetail'
import Categories from './pages/Categories'
import AuditLogs from './pages/AuditLogs'
import Login from './pages/Login'
import { api, type AuthUser } from './lib/supabase'

type AuthContextType = {
  user: AuthUser | null
  setUser: (u: AuthUser | null) => void
}

export const AuthContext = createContext<AuthContextType>({ user: null, setUser: () => {} })

export function useAuth() {
  return useContext(AuthContext)
}

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.auth.me()
      .then((u) => { if (u.authenticated) setUser(u) })
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-2 border-slate-200 border-t-primary-600" />
      </div>
    )
  }

  if (!user?.authenticated) {
    return <Login onLogin={(u) => setUser(u)} />
  }

  return (
    <AuthContext.Provider value={{ user, setUser }}>
      <div className="flex h-screen overflow-hidden">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        <div className="flex flex-1 flex-col overflow-hidden">
          <Topbar onMenuClick={() => setSidebarOpen(true)} />
          <main className="flex-1 overflow-y-auto bg-slate-50 p-4 lg:p-6">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/supplies" element={<Supplies />} />
              <Route path="/supplies/:id" element={<SupplyDetail />} />
              <Route path="/supplies/:id/adjust" element={<AdjustStock />} />
              <Route path="/trash" element={<Trash />} />
              <Route path="/issues" element={<Issues />} />
              <Route path="/issues/:id" element={<IssueDetail />} />
              <Route path="/categories" element={<Categories />} />
              <Route path="/audit-logs" element={<AuditLogs />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </main>
        </div>
      </div>
    </AuthContext.Provider>
  )
}
