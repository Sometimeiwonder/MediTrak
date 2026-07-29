/*
# MediTrak — Medical Inventory Management Schema

## Overview
Creates the core tables for a medical supply inventory system: categories,
supplies, issues (stock issued to people/departments), and audit logs.

## New Tables

1. **supply_categories** — groups for medical supplies (e.g. PPE, Medication).
   - id (uuid PK)
   - name (text, unique, not null)
   - description (text, nullable)
   - created_at (timestamptz default now())

2. **supplies** — individual medical supply items in inventory.
   - id (uuid PK)
   - name (text, not null)
   - sku (text, unique, not null) — stock keeping unit
   - category_id (uuid FK -> supply_categories, nullable)
   - quantity (int, not null, default 0)
   - unit (text, not null, default 'units')
   - reorder_level (int, not null, default 0) — threshold for low-stock alerts
   - expiry_date (date, nullable)
   - location (text, nullable) — storage location
   - created_at (timestamptz default now())

3. **issues** — records of supplies being issued/consumed.
   - id (uuid PK)
   - supply_id (uuid FK -> supplies, cascade delete)
   - quantity (int, not null)
   - issued_to (text, not null) — person or department receiving the supply
   - issued_by (text, not null) — who issued it
   - notes (text, nullable)
   - created_at (timestamptz default now())

4. **audit_logs** — append-only record of system actions.
   - id (uuid PK)
   - action (text, not null) — e.g. CREATE, UPDATE, DELETE, ISSUE
   - entity (text, not null) — table affected
   - entity_id (uuid, nullable)
   - details (text, nullable) — JSON-ish description
   - performed_by (text, not null)
   - created_at (timestamptz default now())

## Security
- RLS enabled on all tables.
- This is a single-tenant demo app (no sign-in screen), so policies allow
  anon + authenticated full CRUD. Data is intentionally shared/public.

## Notes
1. Indexes added on frequently-filtered columns (category_id, supply_id, created_at).
2. Audit logs are insert-only by design (no update/delete policies).
*/

CREATE TABLE IF NOT EXISTS supply_categories (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text UNIQUE NOT NULL,
  description text,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS supplies (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL,
  sku text UNIQUE NOT NULL,
  category_id uuid REFERENCES supply_categories(id) ON DELETE SET NULL,
  quantity int NOT NULL DEFAULT 0,
  unit text NOT NULL DEFAULT 'units',
  reorder_level int NOT NULL DEFAULT 0,
  expiry_date date,
  location text,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS issues (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  supply_id uuid NOT NULL REFERENCES supplies(id) ON DELETE CASCADE,
  quantity int NOT NULL,
  issued_to text NOT NULL,
  issued_by text NOT NULL,
  notes text,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS audit_logs (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  action text NOT NULL,
  entity text NOT NULL,
  entity_id uuid,
  details text,
  performed_by text NOT NULL,
  created_at timestamptz DEFAULT now()
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_supplies_category_id ON supplies(category_id);
CREATE INDEX IF NOT EXISTS idx_issues_supply_id ON issues(supply_id);
CREATE INDEX IF NOT EXISTS idx_issues_created_at ON issues(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON audit_logs(entity);

-- Enable RLS
ALTER TABLE supply_categories ENABLE ROW LEVEL SECURITY;
ALTER TABLE supplies ENABLE ROW LEVEL SECURITY;
ALTER TABLE issues ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;

-- supply_categories policies (single-tenant, anon+auth)
DROP POLICY IF EXISTS "anon_select_categories" ON supply_categories;
CREATE POLICY "anon_select_categories" ON supply_categories FOR SELECT
  TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "anon_insert_categories" ON supply_categories;
CREATE POLICY "anon_insert_categories" ON supply_categories FOR INSERT
  TO anon, authenticated WITH CHECK (true);

DROP POLICY IF EXISTS "anon_update_categories" ON supply_categories;
CREATE POLICY "anon_update_categories" ON supply_categories FOR UPDATE
  TO anon, authenticated USING (true) WITH CHECK (true);

DROP POLICY IF EXISTS "anon_delete_categories" ON supply_categories;
CREATE POLICY "anon_delete_categories" ON supply_categories FOR DELETE
  TO anon, authenticated USING (true);

-- supplies policies
DROP POLICY IF EXISTS "anon_select_supplies" ON supplies;
CREATE POLICY "anon_select_supplies" ON supplies FOR SELECT
  TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "anon_insert_supplies" ON supplies;
CREATE POLICY "anon_insert_supplies" ON supplies FOR INSERT
  TO anon, authenticated WITH CHECK (true);

DROP POLICY IF EXISTS "anon_update_supplies" ON supplies;
CREATE POLICY "anon_update_supplies" ON supplies FOR UPDATE
  TO anon, authenticated USING (true) WITH CHECK (true);

DROP POLICY IF EXISTS "anon_delete_supplies" ON supplies;
CREATE POLICY "anon_delete_supplies" ON supplies FOR DELETE
  TO anon, authenticated USING (true);

-- issues policies
DROP POLICY IF EXISTS "anon_select_issues" ON issues;
CREATE POLICY "anon_select_issues" ON issues FOR SELECT
  TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "anon_insert_issues" ON issues;
CREATE POLICY "anon_insert_issues" ON issues FOR INSERT
  TO anon, authenticated WITH CHECK (true);

DROP POLICY IF EXISTS "anon_update_issues" ON issues;
CREATE POLICY "anon_update_issues" ON issues FOR UPDATE
  TO anon, authenticated USING (true) WITH CHECK (true);

DROP POLICY IF EXISTS "anon_delete_issues" ON issues;
CREATE POLICY "anon_delete_issues" ON issues FOR DELETE
  TO anon, authenticated USING (true);

-- audit_logs: insert-only (no update/delete)
DROP POLICY IF EXISTS "anon_select_audit_logs" ON audit_logs;
CREATE POLICY "anon_select_audit_logs" ON audit_logs FOR SELECT
  TO anon, authenticated USING (true);

DROP POLICY IF EXISTS "anon_insert_audit_logs" ON audit_logs;
CREATE POLICY "anon_insert_audit_logs" ON audit_logs FOR INSERT
  TO anon, authenticated WITH CHECK (true);
