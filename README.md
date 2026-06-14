# SmartSalon / SalonOS

A multi-tenant SaaS platform for beauty salon management. Includes booking, staff and payroll management, a social layer, and a marketplace of service packages.

## Architecture

The project contains two parallel backends and one mobile client:

| Component | Location | Tech Stack |
|---|---|---|
| Legacy Monolith | `SmartSalon/SmartSalon/` | ASP.NET Core 9, EF Core, SQL Server, Razor Pages |
| New Modular Monolith | `src/` | ASP.NET Core 9, EF Core, DDD modules, Hangfire |
| Mobile/Web Client | `smart_salon_app/` | Flutter 3.x, Dart, Riverpod |

## Prerequisites

- .NET 9 SDK
- Docker Desktop (for SQL Server and containerized deployment)
- Flutter 3.x SDK (for mobile/web development)

## Quick Start

```bash
# 1. Copy environment template and set secrets
cp .env.example .env
# Edit .env with your MSSQL_SA_PASSWORD and JWT_SECRET

# 2. Start all services with Docker Compose
docker compose up -d

# 3. Services:
#    - SmartSalon API:       http://localhost:5015/swagger
#    - SalonOS API:          http://localhost:5016/swagger
#    - Flutter Web:          http://localhost:8081
#    - SQL Server:           localhost:1433
```

## Development

### Running backend locally
```bash
dotnet run --project SmartSalon/SmartSalon
dotnet run --project src/SalonOS.Api
```

### Running Flutter app
```bash
cd smart_salon_app
flutter pub get
flutter run
```
