# MediTrack - Medical Inventory Management System

A full-stack medical supply inventory management system with **React + Tailwind CSS** frontend and **ASP.NET Core** backend API.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-teal)
![SQLite](https://img.shields.io/badge/SQLite-3-green)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

### Dashboard
- Real-time overview with Recharts (Bar, Pie, Line charts)
- Total supplies, low stock alerts, out of stock items
- Stock by category visualization
- Issues trend over last 7 days
- Recent issues table

### Supply Management
- Full CRUD operations
- Search by name or SKU
- Category filtering
- Stock status badges (In Stock, Low Stock, Out of Stock)

### Issue Tracking
- Create supply issuances
- Automatic stock deduction
- Track who received supplies

### Categories
- Manage supply categories

### Audit Logs
- Track all system activities

## Tech Stack

| Technology | Purpose |
|------------|---------|
| React 18 | Frontend UI |
| Tailwind CSS | Utility-first styling |
| Recharts | Dashboard charts |
| Vite | Build tool |
| ASP.NET Core 10.0 | Backend API |
| Entity Framework Core | ORM |
| SQLite | Database |

## Project Structure

```
MediTrack/
├── MediTrack.Mvc/
│   ├── Controllers/Api/     # REST API endpoints
│   ├── Data/                # DbContext & Migrations
│   ├── Models/              # Domain entities
│   ├── wwwroot/spa/         # Built React app (generated)
│   └── Program.cs           # API configuration
├── UI/project/              # React source code
│   ├── src/
│   │   ├── components/      # UI components
│   │   ├── pages/           # Page components
│   │   └── lib/             # API client
│   └── package.json
├── build-spa.ps1            # Build script (PowerShell)
└── build-spa.bat            # Build script (Batch)
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
   # PowerShell
   .\build-spa.ps1
   
   # Or manually
   cd UI/project
   npm install
   npm run build
   # Copy dist/* to MediTrack.Mvc/wwwroot/spa/
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

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/supplies` | List all supplies |
| GET | `/api/v1/supplies/{id}` | Get supply by ID |
| POST | `/api/v1/supplies` | Create supply |
| PUT | `/api/v1/supplies/{id}` | Update supply |
| DELETE | `/api/v1/supplies/{id}` | Delete supply |
| GET | `/api/v1/categories` | List categories |
| POST | `/api/v1/categories` | Create category |
| GET | `/api/v1/issues` | List issues |
| POST | `/api/v1/issues` | Create issue |
| GET | `/api/v1/auditlogs` | List audit logs |

## Development

### Backend (ASP.NET)
```bash
cd MediTrack.Mvc
dotnet watch run
```

### Frontend (React with hot reload)
```bash
cd UI/project
npm run dev
```
The dev server runs on http://localhost:5173 with API proxy to the backend.

## License

MIT License
