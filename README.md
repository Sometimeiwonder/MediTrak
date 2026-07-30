# MediTrack - Medical Inventory Management System

A full-stack medical supply inventory management system with **React 18 + Tailwind CSS** frontend and **ASP.NET Core 10.0** backend API with cookie-based authentication.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-teal)
![SQLite](https://img.shields.io/badge/SQLite-3-green)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

### Authentication & Authorization
- Cookie-based login/register/logout (ASP.NET Identity)
- Role-based access: Admin, Staff, User
- Persistent sessions with sliding expiration

### Dashboard
- Total supplies, low stock alerts, out of stock items
- Created/updated today counts
- Inventory value calculation
- Stock by category bar chart
- Stock status doughnut chart (In Stock / Low Stock / Out of Stock)
- Monthly activity line chart (created vs updated)
- Issues trend bar chart (7 days)
- Security metrics (access denied, sensitive actions, rejected uploads)
- Low stock alerts with links to detail pages

### Supply Management
- Full CRUD with detailed forms (Name, SKU, Category, Quantity, Reorder Level, Supplier, Unit Price, Description)
- Supply detail page with reorder suggestions
- Pagination (10 items per page)
- Search by name or SKU
- Filter by stock status (In Stock / Low Stock / Out of Stock)
- Filter by category
- Export to CSV
- Image upload (JPG, PNG, WEBP, max 2MB)
- Soft delete with Trash page
- Restore from trash
- Adjust stock page with preview
- SKU validation (uppercase letters, numbers, hyphens)
- Concurrency version tracking

### Issue Tracking
- Create supply issuances with multi-item support
- Automatic stock deduction with transaction safety
- Issue detail page with line items, subtotals, and total amount
- Pagination

### Categories
- List with supply count and total inventory value per category
- Create and delete categories (with protection against deleting non-empty categories)

### Audit Logs
- Filter by user name
- Filter by action type (Create, Update, Delete, Login, Access Denied)
- Filter by result (Success, Failed, Rejected)
- Filter by date range
- Pagination

### Health Checks
- `/health/live` - Liveness check
- `/health/ready` - Readiness check (includes database connectivity)

## Tech Stack

| Technology | Purpose |
|------------|---------|
| React 18 | Frontend UI |
| React Router 6 | Client-side routing |
| Tailwind CSS | Utility-first styling |
| Recharts | Dashboard charts |
| Lucide React | Icons |
| Vite | Build tool |
| ASP.NET Core 10.0 | Backend API |
| ASP.NET Identity | Authentication |
| Entity Framework Core | ORM |
| SQLite | Database |
| Serilog | Structured logging |
| EPPlus | Excel export |

## Project Structure

```
MediTrack/
├── MediTrack.Mvc/
│   ├── Controllers/Api/     # REST API endpoints
│   │   ├── AuthController.cs
│   │   ├── SuppliesController.cs
│   │   ├── IssuesController.cs
│   │   ├── CategoriesController.cs
│   │   └── AuditLogsController.cs
│   ├── Controllers/         # MVC controllers
│   ├── Data/                # DbContext & Migrations
│   ├── Models/              # Domain entities
│   ├── Services/            # Business logic
│   ├── Repositories/        # Data access
│   ├── wwwroot/             # Static files & built React app
│   │   ├── index.html
│   │   ├── assets/
│   │   └── uploads/         # Uploaded images
│   └── Program.cs
├── UI/project/              # React source code
│   ├── src/
│   │   ├── components/      # Topbar, Sidebar, UI components
│   │   ├── pages/           # Page components
│   │   └── lib/             # API client (supabase.ts)
│   └── package.json
└── build-spa.ps1            # Build script
```

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Sometimeiwonder/MediTrak.git
   cd MediTrak
   ```

2. **Build the React UI**
   ```powershell
   .\build-spa.ps1
   # Or manually:
   cd UI/project
   npm install
   npm run build
   # Copy dist/* to MediTrack.Mvc/wwwroot/
   ```

3. **Run the application**
   ```bash
   cd MediTrack.Mvc
   dotnet run
   ```

4. **Open in browser**
   - https://localhost:7226
   - http://localhost:5226

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/auth/me` | Get current user |
| POST | `/api/v1/auth/login` | Login |
| POST | `/api/v1/auth/register` | Register |
| POST | `/api/v1/auth/logout` | Logout |

### Supplies
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/supplies` | List (paginated, filterable) |
| GET | `/api/v1/supplies/{id}` | Get by ID |
| POST | `/api/v1/supplies` | Create |
| PUT | `/api/v1/supplies/{id}` | Update |
| DELETE | `/api/v1/supplies/{id}` | Soft delete |
| GET | `/api/v1/supplies/trash` | List deleted items |
| POST | `/api/v1/supplies/{id}/restore` | Restore from trash |
| POST | `/api/v1/supplies/{id}/adjust` | Adjust stock |
| POST | `/api/v1/supplies/{id}/upload-image` | Upload image |
| GET | `/api/v1/supplies/stats` | Get statistics |
| GET | `/api/v1/supplies/export` | Export CSV |

### Issues
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/issues` | List (paginated) |
| GET | `/api/v1/issues/{id}` | Get detail |
| POST | `/api/v1/issues` | Create (multi-item) |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/categories` | List with stats |
| POST | `/api/v1/categories` | Create |
| DELETE | `/api/v1/categories/{id}` | Delete |

### Audit Logs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/auditlogs` | List (paginated, filterable) |

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/dashboard` | Full dashboard data |

### Health
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health/live` | Liveness check |
| GET | `/health/ready` | Readiness check |

## Default Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@shop.test | Admin@123 | Admin |
| staff@shop.test | Staff@123 | Staff |
| user@shop.test | User@123 | User |

## License

MIT License
