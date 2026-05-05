# Investment Portfolio Management System

A full-stack financial management platform built with **C# .NET Core 8**, **Entity Framework Core**, **SQL Server**, and **Blazor Server**.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Option A: Docker (Recommended)](#option-a--docker-recommended)
  - [Option B: Run Locally](#option-b--run-locally)
- [How to Access](#how-to-access)
- [Default Login](#default-login)
- [API Documentation](#api-documentation)
- [Running Tests](#running-tests)
- [Environment Variables](#environment-variables)
- [CI/CD Pipeline](#cicd-pipeline)

---

## Overview

The Investment Portfolio Management System allows financial advisors to:
- Manage client profiles with risk assessments
- Create and track investment portfolios in real time
- Buy/sell assets and record all transactions
- Fetch live market data (stock quotes, indices)
- View analytics, gain/loss calculations, and portfolio weights

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 8 Web API |
| Frontend UI | Blazor Server |
| Database | SQL Server (EF Core 8) |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| Market Data | Alpha Vantage API |
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Testing | xUnit, Moq, FluentAssertions |
| Logging | Serilog |
| API Docs | Swagger / OpenAPI |

---

## Project Structure

```
InvestmentPortfolioSystem/
├── src/
│   ├── InvestmentPortfolio.API/            # REST API (controllers, middleware, JWT config)
│   │   ├── Controllers/                    # Auth, Clients, Portfolios, Investments, Market Data
│   │   ├── Middleware/                     # Global exception handling
│   │   ├── Program.cs                      # App entry point + DI setup
│   │   └── appsettings.json
│   │
│   ├── InvestmentPortfolio.Web/            # Blazor Server UI
│   │   ├── Pages/                          # Dashboard, Clients, ClientDetail, Portfolio, Login
│   │   ├── Layout/                         # MainLayout (with live market ticker), NavMenu
│   │   └── Services/                       # ApiService, JwtAuthStateProvider
│   │
│   ├── InvestmentPortfolio.Core/           # Business logic (no infra dependencies)
│   │   ├── Entities/                       # ApplicationUser, Client, Portfolio, Investment, Transaction
│   │   ├── Interfaces/                     # Repository + Service interfaces
│   │   └── Services/                       # ClientService, PortfolioService, InvestmentService, TransactionService
│   │
│   ├── InvestmentPortfolio.Infrastructure/ # Data access + external integrations
│   │   ├── Data/                           # ApplicationDbContext, DbInitializer (seeds roles + admin)
│   │   ├── Repositories/                   # Generic + typed EF Core repositories
│   │   ├── Identity/                       # JwtService (token generation)
│   │   └── ExternalApis/                   # AlphaVantage market data (with fallback + caching)
│   │
│   └── InvestmentPortfolio.Shared/         # Shared across all projects
│       ├── DTOs/                           # Request/Response objects for Auth, Client, Portfolio, Investment...
│       └── Enums/                          # AssetType, RiskLevel, TransactionType, PortfolioStatus
│
├── tests/
│   ├── InvestmentPortfolio.Tests.Unit/     # 14 unit tests (Moq, FluentAssertions)
│   └── InvestmentPortfolio.Tests.Integration/ # End-to-end API flow tests
│
├── Dockerfile                              # API container
├── Dockerfile.web                          # Blazor Web container
├── docker-compose.yml                      # API + Web + SQL Server
└── .github/workflows/ci-cd.yml            # Build → Test → Docker push → Deploy
```

---

## Features

- **JWT Authentication** — secure register/login, role-based access (Admin, Advisor)
- **Client Management** — full CRUD with risk profile (Conservative → Very Aggressive)
- **Portfolio Tracking** — real-time value, total gain/loss, daily change
- **Investment Analytics** — weighted average cost basis, P&L %, portfolio allocation weights
- **Buy / Sell** — buying more shares merges positions with correct weighted avg; full sell marks position closed
- **Transaction History** — complete audit trail (Buy, Sell, Dividend, Deposit, Withdrawal, Fee)
- **Live Market Data** — stock quotes via Alpha Vantage, 5-minute in-memory cache, graceful fallback
- **Market Ticker Bar** — live S&P 500 / NASDAQ / DOW indices refreshed every 60s in the UI
- **Blazor Dashboard** — AUM summary, client table, portfolio cards
- **Swagger UI** — full interactive API documentation
- **Soft Deletes** — records never hard-deleted; EF Core global query filters hide them
- **Docker** — single `docker-compose up` starts everything
- **GitHub Actions CI/CD** — build, test, Docker image push, SSH deploy to staging

---

## Prerequisites

### For Docker (recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac/Linux)

### For local development
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) **or** SQL Server LocalDB (included with Visual Studio)
- [Git](https://git-scm.com/)

---

## Getting Started

### Option A — Docker (Recommended)

> Starts the API, Blazor Web UI, and SQL Server automatically — no install needed.

**1. Clone the repo**
```bash
git clone https://github.com/Akashg02/InvestmentPortfolioSystem.git
cd InvestmentPortfolioSystem
```

**2. Start all services**
```bash
docker-compose up -d
```

This pulls SQL Server, runs migrations, seeds the admin account, and starts both apps.

**3. Access the apps** → see [How to Access](#how-to-access)

**To stop:**
```bash
docker-compose down
```

---

### Option B — Run Locally

**1. Clone the repo**
```bash
git clone https://github.com/Akashg02/InvestmentPortfolioSystem.git
cd InvestmentPortfolioSystem
```

**2. (Optional) Get a free Alpha Vantage API key**

Sign up at https://www.alphavantage.co/support/#api-key and put it in `src/InvestmentPortfolio.API/appsettings.Development.json`:
```json
{
  "MarketData": {
    "AlphaVantageApiKey": "YOUR_KEY_HERE"
  }
}
```
> Without a key, the app uses realistic fallback prices — it still works fine.

**3. Install EF Core tools** (one-time)
```bash
dotnet tool install --global dotnet-ef
```

**4. Apply database migrations**
```bash
cd src/InvestmentPortfolio.API
dotnet ef migrations add InitialCreate --project ../InvestmentPortfolio.Infrastructure
dotnet ef database update
```

**5. Run the API** (Terminal 1)
```bash
dotnet run --project src/InvestmentPortfolio.API
```

**6. Run the Web UI** (Terminal 2)
```bash
dotnet run --project src/InvestmentPortfolio.Web
```

**7. Trust the dev certificate** (if you see SSL warnings)
```bash
dotnet dev-certs https --trust
```

---

## How to Access

| App | URL |
|---|---|
| Blazor Web UI | http://localhost:5002 |
| Swagger API Docs | http://localhost:5001/swagger |
| Health Check | http://localhost:5001/health |

> When running locally (not Docker) the ports are `https://localhost:5001` and `https://localhost:5002`.

---

## Default Login

The database seeds a default admin user on first startup:

```
Email:    admin@investmentportfolio.com
Password: Admin@123456!
```

Or register a new account:
- **Via Web UI:** click "Don't have an account? Register" on the login page
- **Via Swagger:** `POST /api/auth/register`

---

## API Documentation

Full Swagger UI available at `/swagger` once the API is running.

### Key Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register new advisor | Public |
| `POST` | `/api/auth/login` | Get JWT token | Public |
| `GET` | `/api/clients` | List your clients | Required |
| `POST` | `/api/clients` | Create client | Required |
| `GET` | `/api/clients/{id}` | Get client detail | Required |
| `PUT` | `/api/clients/{id}` | Update client | Required |
| `DELETE` | `/api/clients/{id}` | Soft-delete client | Required |
| `GET` | `/api/portfolios/{id}` | Portfolio + holdings | Required |
| `GET` | `/api/portfolios/by-client/{clientId}` | All portfolios for a client | Required |
| `POST` | `/api/portfolios` | Create portfolio | Required |
| `POST` | `/api/portfolios/{id}/refresh-prices` | Fetch live prices | Required |
| `POST` | `/api/investments` | Buy investment | Required |
| `POST` | `/api/investments/{id}/sell` | Sell shares | Required |
| `GET` | `/api/transactions/by-portfolio/{id}` | Transaction history | Required |
| `GET` | `/api/market-data/quote/{symbol}` | Stock quote | Required |
| `GET` | `/api/market-data/summary` | S&P 500 / NASDAQ / DOW | Required |

### Authenticating in Swagger
1. Call `POST /api/auth/login`
2. Copy the `token` from the response
3. Click the **Authorize** button (top right)
4. Enter: `Bearer <your_token>`

---

## Running Tests

```bash
# Unit tests only
dotnet test tests/InvestmentPortfolio.Tests.Unit

# Integration tests (requires running SQL Server)
dotnet test tests/InvestmentPortfolio.Tests.Integration

# All tests with coverage report
dotnet test --collect:"XPlat Code Coverage"
```

**Unit test coverage includes:**
- `ClientService` — create, update, delete, duplicate email detection
- `PortfolioService` — CRUD, gain/loss calculation, soft delete
- `InvestmentService` — buy (new + merge), partial/full sell, oversell guard

---

## Environment Variables

These can be set in `appsettings.json`, `docker-compose.yml`, or as OS environment variables:

| Variable | Default | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | LocalDB | SQL Server connection string |
| `JwtSettings__SecretKey` | *(set this in prod)* | Min 32-character signing key |
| `JwtSettings__ExpirationMinutes` | `60` | JWT token lifetime |
| `MarketData__AlphaVantageApiKey` | `demo` | Alpha Vantage key (free tier) |
| `AllowedOrigins__BlazorApp` | `https://localhost:5002` | CORS allowed origin |

---

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/ci-cd.yml`) triggers on every push/PR to `main`:

```
Push to main
    │
    ├── 1. Build & Test
    │       ├── dotnet restore + build
    │       ├── Run unit tests
    │       ├── Run integration tests (against SQL Server service container)
    │       └── Upload coverage report
    │
    ├── 2. Docker Build & Push  (main branch only)
    │       ├── Build API image → push to GHCR
    │       └── Build Web image → push to GHCR
    │
    └── 3. Deploy to Staging
            └── SSH into server → docker-compose pull + up
```

---

## License

MIT — free to use for educational and commercial projects.
