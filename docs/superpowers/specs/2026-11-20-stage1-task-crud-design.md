# Stage 1 Design: Task CRUD Foundation

**Date:** 2026-11-20
**Status:** Approved, pending written-spec review
**Mode:** Guided build. The learner writes all code in `src/` and `tests/`; the assistant explains, plans, and reviews.

## Context

The Task Manager API is a learning project for C#/.NET upskilling, built in stages across stack meetings. The learner has a Java + Spring Boot background and is new to C#; explanations should map .NET concepts onto Spring equivalents.

### Roadmap (each stage gets its own spec → plan → build cycle)

1. **Foundation (this spec):** project structure, EF Core + Postgres, Task CRUD, Swagger
2. **Auth:** register/login/logout with JWT, Admin/User roles
3. **Ownership & assignment:** creator/assignee rules, `/assign`, `/status`, `/api/users/{id}/tasks`, admin user endpoints
4. **Middleware & logging:** custom request-logging middleware, Serilog, global exception handling
5. **Advanced (optional):** SignalR, Redis caching, email notifications, policy-based authorization

## Environment

- .NET SDK 10 (10.0.404 installed)
- Docker 29 installed; Postgres runs via `docker-compose.yml`
- `dotnet-ef` global tool is **not** installed yet. Installing it is a setup step.

## Architecture

A single API project with folders for each layer, plus one test project, in one solution:

```
TaskManager.sln
docker-compose.yml
src/TaskManager.Api/
  Controllers/   Services/   Data/   Entities/   Dtos/
tests/TaskManager.Api.Tests/
```

| Layer | Folder | Responsibility | Spring analogue |
| --- | --- | --- | --- |
| Controller | `Controllers/` | HTTP only: routing, binding, choosing status codes | `@RestController` |
| Service | `Services/` | Business rules (timestamps, paging rules), entity↔DTO mapping | `@Service` |
| Data | `Data/` | `AppDbContext` with `DbSet<TaskItem>` | `JpaRepository` / `EntityManager` |

- No separate repository layer: `DbContext` already acts as the repository and unit of work.
- The service is `ITaskService` + `TaskService`, registered in DI as **scoped** (one instance per request).
- Controllers use `[ApiController]` with attribute routing at `api/tasks`.
- All I/O is async (`Task<T>`, `ToListAsync`, `FindAsync`, `SaveChangesAsync`).
- The connection string lives in `appsettings.Development.json` and matches the docker-compose credentials (local dev only).

## Data Model

Entity `TaskItem`. It is not named `Task`, to avoid clashing with `System.Threading.Tasks.Task`.

| Property | Type | Rules |
| --- | --- | --- |
| `Id` | `Guid` | Primary key, generated on create |
| `Title` | `string` | Required, max 200 chars |
| `Description` | `string?` | Optional, max 2000 chars |
| `DueDate` | `DateTime?` | Optional, UTC |
| `Status` | `TaskItemStatus` | Defaults to `Pending` |
| `CreatedAt` | `DateTime` | UTC, set by the service on create |
| `UpdatedAt` | `DateTime` | UTC, set by the service on create and on every update |

Enum `TaskItemStatus { Pending, InProgress, Completed }`, stored as a **string** in the database (readable rows, safe if the enum is reordered).

All timestamps are UTC (Npgsql maps `DateTime` to `timestamptz` and rejects non-UTC values).

The schema is created by the first EF Core migration (`InitialCreate`). Later stages add `CreatedById` and `AssignedToId` through new migrations.

## API Contract

| Method | Route | Success | Errors |
| --- | --- | --- | --- |
| `POST` | `/api/tasks` | `201 Created` + `Location` header + `TaskResponse` | `400` invalid body |
| `GET` | `/api/tasks?status=&page=&pageSize=` | `200` + `PagedResponse<TaskResponse>` | `400` invalid paging/status |
| `GET` | `/api/tasks/{id}` | `200` + `TaskResponse` | `404` |
| `PUT` | `/api/tasks/{id}` | `200` + updated `TaskResponse` | `400`, `404` |
| `DELETE` | `/api/tasks/{id}` | `204 No Content` | `404` |

### DTOs (C# `record` types, in `Dtos/`)

- `CreateTaskRequest(Title, Description, DueDate, Status?)`: if `Status` is omitted, it defaults to `Pending`.
- `UpdateTaskRequest(Title, Description, DueDate, Status)`: full replacement (PUT semantics); all fields are applied.
- `TaskResponse(Id, Title, Description, DueDate, Status, CreatedAt, UpdatedAt)`
- `PagedResponse<T>(Items, Page, PageSize, TotalCount)`

The entity is never returned directly from a controller.

### Listing rules

- `status`: optional filter; an unknown value returns `400`.
- `page`: defaults to `1`; must be ≥ 1.
- `pageSize`: defaults to `20`; must be between 1 and 100.
- Out-of-range `page` or `pageSize` returns `400`. Values are rejected, not silently clamped, so the client learns about the mistake.
- Ordering: `CreatedAt` descending, then `Id` (stable paging).
- `totalCount` counts all rows matching the filter, before paging.

### Validation and errors

- Data annotations on request DTOs (`[Required]`, `[MaxLength]`). `[ApiController]` returns `400` automatically.
- Errors use **ProblemDetails** (RFC 7807) via `AddProblemDetails()`.
- Not found: the service returns `null`; the controller maps it to `NotFound()`.
- Enums are serialized as strings in JSON (`"InProgress"`, not `1`) via `JsonStringEnumConverter`.

### Swagger

The .NET 10 template's built-in OpenAPI document (`AddOpenApi()` / `MapOpenApi()`) is kept. Swagger UI is added via `Swashbuckle.AspNetCore.SwaggerUI`, pointed at `/openapi/v1.json`, in the Development environment only.

## Testing

- **Framework:** xUnit with its built-in `Assert`. FluentAssertions is avoided because v8 has a commercial licence.
- **Integration tests (primary):** `WebApplicationFactory<Program>` plus `Testcontainers.PostgreSql`. A real Postgres runs in Docker, migrations are applied at startup of the test fixture, and data is isolated between tests.
- **No EF InMemory provider:** it does not behave like a relational database.
- **Unit tests:** only for pure logic, such as paging-parameter validation.
- **Required coverage:** create 201 + Location; invalid create 400; get by id 200/404; list filters by status; list paging and `totalCount`; invalid paging 400; update 200/404; delete 204 then 404.
- **Workflow:** for each endpoint, write the test first, watch it fail, then implement it.

## Done When

- `docker compose up -d` starts Postgres
- `dotnet ef database update` creates the schema
- `dotnet test` passes, including all integration tests
- All five endpoints work in Swagger UI

## Out of Scope (later stages)

Authentication and authorization, users, ownership and assignment, logging middleware and Serilog, global exception middleware, caching, SignalR, and deployment.
