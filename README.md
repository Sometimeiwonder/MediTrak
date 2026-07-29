# MediTrack - Medical Inventory Management System

A comprehensive medical supply inventory management system with **two frontend options**: ASP.NET Core MVC (server-rendered) and React + Tailwind CSS (SPA with Supabase).

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-teal)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

### Core Functionality
- **Dashboard** - Real-time overview with Chart.js visualizations (bar, doughnut, line charts)
- **CRUD Operations** - Full Create, Read, Update, Delete with validation
- **Soft Delete** - Trash and Restore functionality
- **Concurrency Control** - RowVersion conflict detection
- **Transaction Management** - Issue creation with stock deduction
- **Search** - LINQ-based search with keyword filter
- **Pagination** - Server-side paging for large datasets

### Export Capabilities
- **Excel Export** - Export to .xlsx format using EPPlus library
- **CSV Export** - Export to .csv format

### Security Features
- **Role-based Authorization** - Admin, Staff, User roles with 5 policies
- **Audit Logging** - Track all CRUD operations
- **File Upload Security** - Whitelist validation, GUID naming, 2MB limit
- **Health Checks** - `/health/live` and `/health/ready` endpoints

### API
- **RESTful Endpoints** - Get supply by ID, search with filters
- **ProblemDetails** - Structured error responses with traceId

## Tech Stack

### Backend (ASP.NET MVC)
| Technology | Purpose |
|------------|---------|
| ASP.NET Core 10.0 | Web framework |
| Entity Framework Core | ORM with SQLite |
| ASP.NET Core Identity | Authentication & Authorization |
| Serilog | Structured logging |
| EPPlus | Excel export |

### Frontend Options

**Option 1: ASP.NET MVC (Server-rendered)**
| Technology | Purpose |
|------------|---------|
| Razor Views | Server-side rendering |
| Bootstrap 5 | UI framework |
| Chart.js | Dashboard visualizations |

**Option 2: React SPA (with Supabase)**
| Technology | Purpose |
|------------|---------|
| React 18 | UI library |
| Tailwind CSS | Utility-first CSS |
| Supabase | Backend-as-a-Service |
| Recharts | Chart library |
| Vite | Build tool |

### Infrastructure
| Technology | Purpose |
|------------|---------|
| Docker | Containerization |
| Supabase | PostgreSQL database (React version) |

## Project Structure

```
MediTrack/
├── MediTrack.Mvc/           # ASP.NET MVC application
│   ├── Controllers/         # MVC Controllers
│   ├── Data/                # DbContext, Migrations, Seeds
│   ├── Filters/             # Authorization filters
│   ├── Models/              # Domain entities
│   ├── Options/             # Configuration options
│   ├── Repositories/        # Data access layer
│   ├── Services/            # Business logic layer
│   ├── ViewModels/          # View models
│   ├── Views/               # Razor views
│   └── wwwroot/             # Static files
├── UI/project/              # React SPA (Supabase backend)
│   ├── src/
│   │   ├── components/      # React components
│   │   ├── pages/           # Page components
│   │   └── lib/             # Utilities & Supabase client
│   └── supabase/            # Database migrations
├── MediTrack.Tests/         # Unit tests (xUnit)
├── Dockerfile               # Docker configuration
└── docker-compose.yml       # Docker Compose configuration
```

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for MVC version)
- [Node.js 18+](https://nodejs.org/) (for React version)
- Docker (optional)

### Option 1: ASP.NET MVC Version

1. **Clone the repository**
   ```bash
   git clone https://github.com/Sometimeiwonder/MediTrak.git
   cd MediTrak
   ```

2. **Restore packages and run**
   ```bash
   cd MediTrack.Mvc
   dotnet restore
   dotnet run
   ```

3. **Open in browser**
   - HTTPS: https://localhost:7226
   - HTTP: http://localhost:5226

### Option 2: React SPA Version

1. **Navigate to UI folder**
   ```bash
   cd UI/project
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Run development server**
   ```bash
   npm run dev
   ```

4. **Open in browser**
   - http://localhost:5173

### Docker (MVC Version)

```bash
docker-compose up --build
```

## Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@shop.test | Admin@123 |
| Staff | staff@shop.test | Staff@123 |
| User | user@shop.test | User@123 |

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/supplies/{id}` | Get supply by ID |
| GET | `/api/supplies/search?keyword=...` | Search supplies |
| GET | `/health/live` | Liveness check |
| GET | `/health/ready` | Readiness check (DB) |

## Authorization Policies

| Policy | Access |
|--------|--------|
| CanViewSupply | Admin, Staff |
| CanManageSupply | Admin only |
| CanAdjustStock | Admin, Staff |
| CanViewAuditLog | Admin only |
| CanManageIssue | Admin, Staff |

## Testing

```bash
dotnet test
```

## License

MIT License - see [LICENSE](LICENSE) for details.
