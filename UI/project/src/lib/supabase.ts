import { createClient } from '@supabase/supabase-js'

const supabaseUrl = import.meta.env.VITE_SUPABASE_URL as string
const supabaseAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY as string

export const supabase = createClient(supabaseUrl, supabaseAnonKey, {
  auth: { persistSession: false },
})

export type SupplyCategory = {
  id: string
  name: string
  description: string | null
  created_at: string
}

export type Supply = {
  id: string
  name: string
  sku: string
  category_id: string | null
  quantity: number
  unit: string
  reorder_level: number
  expiry_date: string | null
  location: string | null
  created_at: string
  category?: SupplyCategory | null
}

export type Issue = {
  id: string
  supply_id: string
  quantity: number
  issued_to: string
  issued_by: string
  notes: string | null
  created_at: string
  supply?: Supply | null
}

export type AuditLog = {
  id: string
  action: string
  entity: string
  entity_id: string | null
  details: string | null
  performed_by: string
  created_at: string
}
