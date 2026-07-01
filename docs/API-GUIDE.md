# Sales API — Usage and Testing Guide

Sales **CRUD** API for the DeveloperStore evaluation, built with **.NET 8**, **DDD**, **CQRS/MediatR**, **EF Core + PostgreSQL**, **AutoMapper**, **FluentValidation**, **Serilog** and domain events.

> This guide covers what the API does, how to run it and how to test it. Business rules live **in the domain** (never in controllers/handlers).

---

## Table of contents
- [Architecture](#architecture)
- [Business rules](#business-rules)
- [Running the API](#running-the-api)
- [Authentication (JWT)](#authentication-jwt)
- [Response and error format](#response-and-error-format)
- [Endpoints](#endpoints)
- [Pagination, filtering and ordering](#pagination-filtering-and-ordering)
- [Domain events](#domain-events)
- [Testing and coverage](#testing-and-coverage)

---

## Architecture

Layered solution (under `src/`):

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Domain` | `Sale` aggregate, `SaleItem` entity, `Customer`/`Branch`/`Product` value objects (External Identities), business rules, events |
| Application | `Application` | CQRS use cases (Command/Query + Handler + Validator), read DTOs, event dispatch |
| Persistence | `ORM` | `DefaultContext` (EF Core), mappings, PostgreSQL repositories, migrations |
| API | `WebApi` | REST controllers, error-handling middleware, JWT auth, Swagger |
| Cross-cutting | `Common`, `IoC` | Security (JWT/BCrypt), validation (MediatR pipeline), health checks, DI registration |

**External Identities:** references to Customer, Branch and Product are stored as **id + denormalized name** (value objects), with no relational FK to other domains.

---

## Business rules

Discount by quantity of **identical items** (same product on the same line):

| Quantity | Discount |
|---|---|
| < 4 | none (a discount is not allowed) |
| 4 to 9 | 10% |
| 10 to 20 | 20% |
| > 20 | **invalid** — the sale is rejected |

- **Item total** = `quantity × unit price − discount`.
- **Sale total** = sum of the totals of the **non-cancelled** items.
- Cancelling an item recomputes the total; cancelling a sale cancels every item.

---

## Running the API

**Prerequisites:** .NET 8 SDK, Docker (for PostgreSQL) and the `dotnet-ef` tool
(`dotnet tool install --global dotnet-ef`).

### 1. Start PostgreSQL

`appsettings.json` points to `Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n`. Start a Postgres on that port:

```bash
docker run -d --name sales_pg -p 5432:5432 \
  -e POSTGRES_DB=developer_evaluation \
  -e POSTGRES_USER=developer \
  -e POSTGRES_PASSWORD='ev@luAt10n' \
  postgres:13
```

> The repository also ships a `docker-compose.yml` with PostgreSQL, MongoDB and Redis. MongoDB/Redis are not required for the sales CRUD (the Mongo connection string exists, but the Mongo read repository was not implemented — an optional item).

### 2. Apply the migrations

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

> **Environment note:** if you only have an SDK newer than .NET 8 installed, prefix the `dotnet ef` commands with `DOTNET_ROLL_FORWARD=LatestMajor`.

### 3. Run the API

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

- HTTP: `http://localhost:5119`  ·  HTTPS: `https://localhost:7181`
- **Swagger/OpenAPI:** `https://localhost:7181/swagger`
- **Health check:** `GET /health` (also `/health/live` and `/health/ready`)

The examples below use `http://localhost:5119` as the base URL.

---

## Authentication (JWT)

The **sales endpoints are open** (no token required) to make evaluation easier. The API also exposes a JWT authentication flow:

```bash
# 1. Create a user
curl -X POST http://localhost:5119/api/users -H "Content-Type: application/json" -d '{
  "username": "reviewer",
  "password": "Passw0rd!",
  "email": "reviewer@test.com",
  "phone": "+5511999999999",
  "status": 1,
  "role": 1
}'

# 2. Authenticate and obtain the token
curl -X POST http://localhost:5119/api/auth -H "Content-Type: application/json" -d '{
  "email": "reviewer@test.com",
  "password": "Passw0rd!"
}'
```

The login response returns `data.token`. To call a protected endpoint, send `Authorization: Bearer <token>`.
`status`: 1=Active, 2=Inactive, 3=Suspended · `role`: 1=Customer, 2=Manager, 3=Admin.

---

## Response and error format

Every successful response uses an **envelope**:

```jsonc
{
  "data": { /* payload */ },
  "success": true,
  "message": "Sale created successfully",
  "errors": []
}
```

- **List responses** also carry `currentPage`, `totalPages`, `totalCount` at the root level and `data` as an array.
- **Payload-less operations** (e.g. delete) return just `{ "success", "message", "errors" }`.

**Errors** use a standardized `{ type, error, detail }` shape (global middleware):

```json
{ "type": "ValidationError", "error": "Invalid input data", "detail": "Cannot sell more than 20 identical items." }
```

| Situation | HTTP | `type` |
|---|---|---|
| Validation / business rule | 400 | `ValidationError` / `DomainRuleViolation` |
| Not found | 404 | `ResourceNotFound` |
| Not authenticated | 401 | `AuthenticationError` |
| Conflict | 409 | `ConflictError` |
| Unexpected error | 500 | `InternalServerError` |

---

## Endpoints

Base path: `/api/sales`

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/sales` | Create a sale |
| `GET` | `/api/sales/{id}` | Get a sale by id |
| `GET` | `/api/sales` | List sales (pagination/filtering/ordering) |
| `PUT` | `/api/sales/{id}` | Update a sale (replaces header and items) |
| `DELETE` | `/api/sales/{id}` | Delete a sale |
| `POST` | `/api/sales/{id}/cancel` | Cancel the whole sale |
| `POST` | `/api/sales/{saleId}/items/{itemId}/cancel` | Cancel a single item |

### Create a sale

```bash
curl -X POST http://localhost:5119/api/sales -H "Content-Type: application/json" -d '{
  "saleNumber": "S-0001",
  "saleDate": "2026-07-01T10:00:00Z",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Ada Lovelace",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "Downtown",
  "items": [
    { "productId": "33333333-3333-3333-3333-333333333333", "productName": "Widget", "quantity": 5, "unitPrice": 100.00 }
  ]
}'
```

Response (201) — 5 identical items → 10% discount (discount 50, total 450):

```jsonc
{
  "data": {
    "id": "…", "saleNumber": "S-0001", "totalAmount": 450.00, "isCancelled": false,
    "items": [
      { "id": "…", "productId": "…", "productName": "Widget",
        "quantity": 5, "unitPrice": 100.00, "discount": 50.00, "total": 450.00, "isCancelled": false }
    ]
  },
  "success": true, "message": "Sale created successfully", "errors": []
}
```

### Other operations

```bash
# Get
curl http://localhost:5119/api/sales/{id}

# Update (same body as create; the id comes from the route)
curl -X PUT http://localhost:5119/api/sales/{id} -H "Content-Type: application/json" -d '{ … }'

# Cancel item / cancel sale
curl -X POST http://localhost:5119/api/sales/{id}/items/{itemId}/cancel
curl -X POST http://localhost:5119/api/sales/{id}/cancel

# Delete
curl -X DELETE http://localhost:5119/api/sales/{id}
```

Trying to sell more than 20 identical items returns **400** with `detail: "Cannot sell more than 20 identical items."`.

---

## Pagination, filtering and ordering

`GET /api/sales` accepts (query string):

| Parameter | Example | Description |
|---|---|---|
| `_page` | `_page=1` | Page number (1-based, default 1) |
| `_size` | `_size=10` | Items per page (1–100, default 10) |
| `_order` | `_order=saleDate desc, saleNumber asc` | Ordering by one or more fields (`saleNumber`, `saleDate`, `totalAmount`, `createdAt`) |
| `saleNumber` / `customerName` / `branchName` | `customerName=Ada*` | Text filter; `*` = **case-insensitive** wildcard (ILIKE) |
| `_minDate` / `_maxDate` | `_minDate=2026-01-01` | Sale date range |
| `isCancelled` | `isCancelled=false` | Filter by cancellation status |

```bash
curl "http://localhost:5119/api/sales?_page=1&_size=10&customerName=ada*&_order=saleDate%20desc"
```

---

## Domain events

The aggregate raises `SaleCreated`, `SaleModified`, `SaleCancelled` and `ItemCancelled`. After persistence, the events are dispatched (via MediatR) and **written to the log** (Serilog) by notification handlers — no broker is required. Example log line:

```
[INF] SaleCreated at 2026-07-01T…: SaleId=…, SaleNumber=S-0001, TotalAmount=450.00
```

---

## Testing and coverage

**Test suite:** xUnit + NSubstitute + Bogus (unit) and `WebApplicationFactory` + **Testcontainers PostgreSQL** (functional, end-to-end against a real, disposable Postgres).

```bash
# All tests (the functional project requires a running Docker)
dotnet test Ambev.DeveloperEvaluation.sln
```

**Coverage report** (produces HTML at `TestResults/CoverageReport/index.html`):

```bash
# Linux/macOS
./coverage-report.sh
# Windows
coverage-report.bat
```

> The scripts install `coverlet.console`/`reportgenerator`, run the tests with coverage and generate the report. **Docker must be running** (the functional tests spin up a PostgreSQL container via Testcontainers). On Windows Git Bash, run the `.bat` (the `.sh` suffers from MSYS mangling of `/p:` arguments).

Current coverage: **~88% line coverage** (Application ~95%, Domain ~93%, Common ~88%, ORM ~81%, WebApi ~80%).
