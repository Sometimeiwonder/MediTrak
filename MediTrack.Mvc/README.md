# MediTrack - Medical Inventory Management System

A comprehensive medical supply inventory management system built with ASP.NET Core MVC, featuring role-based authorization, real-time dashboard with charts, export capabilities, and audit logging.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-purple)
![Entity Framework](https://img.shields.io/badge/EF_Core-10.0-blue)
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

| Technology | Purpose |
|------------|---------|
| ASP.NET Core 10.0 | Web framework |
| Entity Framework Core | ORM with SQLite |
| ASP.NET Core Identity | Authentication & Authorization |
| Serilog | Structured logging |
| Chart.js | Dashboard visualizations |
| EPPlus | Excel export |
| Bootstrap 5 | UI framework |
| Docker | Containerization |

## Project Structure

```
MediTrack/
├── MediTrack.Mvc/           # Main application
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
├── MediTrack.Tests/         # Unit tests (xUnit)
├── Dockerfile               # Docker configuration
└── docker-compose.yml       # Docker Compose configuration
```

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Sometimeiwonder/MediTrak.git
   cd MediTrak
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   cd MediTrack.Mvc
   dotnet run
   ```

4. **Open in browser**
   - HTTPS: https://localhost:7226
   - HTTP: http://localhost:5226

### Docker

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
