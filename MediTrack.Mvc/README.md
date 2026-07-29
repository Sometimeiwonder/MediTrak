# MediTrack - Medical Inventory Management System

## Overview
A comprehensive medical supply inventory management system built with ASP.NET Core MVC, featuring role-based authorization, audit logging, real-time dashboard with charts, and export capabilities.

## Tech Stack
- ASP.NET Core 10.0
- Entity Framework Core + SQLite
- ASP.NET Core Identity
- Serilog (structured logging)
- Bootstrap 5
- Chart.js (dashboard visualization)
- EPPlus (Excel export)

## Features

### Core
- **Dashboard** - Real-time overview with Chart.js visualizations (stock levels, categories, recent activity)
- **CRUD** - Full Create, Read, Update, Delete with validation
- **Soft Delete** - Trash and Restore functionality
- **Concurrency** - RowVersion conflict detection
- **Transaction** - Issue creation with stock deduction
- **Search** - LINQ-based search with keyword filter
- **Pagination** - Server-side paging for large datasets
- **Export** - Export to Excel (.xlsx) and CSV formats

### Security
- **Role-based Authorization** - Admin, Staff, User roles with 5 policies
- **Audit Logging** - Track all CRUD operations
- **File Upload Security** - Whitelist validation, GUID naming, 2MB limit
- **Health Checks** - `/health/live` and `/health/ready` endpoints

### API
- **RESTful Endpoints** - Get supply by ID, search with filters
- **ProblemDetails** - Structured error responses with traceId

## How to Run

```bash
# 1. Restore packages
dotnet restore

# 2. Apply migrations (auto on first run)
dotnet run

# 3. Open browser
# HTTPS: https://localhost:7226
# HTTP:  http://localhost:5226
```

## Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Or build manually
docker build -t meditrack .
docker run -p 5000:80 meditrack
```

## Demo Accounts

| Role  | Email            | Password    |
|-------|------------------|-------------|
| Admin | admin@shop.test  | Admin@123   |
| Staff | staff@shop.test  | Staff@123   |
| User  | user@shop.test   | User@123    |

## Project Structure

```
MediTrack/
├── Controllers/          # MVC Controllers
├── Data/                 # DbContext, Migrations, Seeds
├── Filters/              # Authorization filters
├── Models/               # Domain entities
├── Options/              # Configuration options
├── Repositories/         # Data access layer
├── Services/             # Business logic layer
├── ViewModels/           # View models
├── Views/                # Razor views
├── wwwroot/              # Static files
├── Dockerfile            # Docker configuration
└── docker-compose.yml    # Docker Compose configuration
```

## License
MIT
