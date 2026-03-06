# 🌐 Presentation Layer

The Presentation layer is the **HTTP entry point** — thin ASP.NET Core controllers that orchestrate HTTP concerns (authentication, routing, request binding, response mapping) without containing any business logic.

## Controllers

| Controller | Base Route | Key Endpoints |
|-----------|-----------|--------------|
| `AuthController` | `/api/auth` | register, verify-registration, login, logout, refresh-token, forgot-password, reset-password, change-password |
| `AccountsController` | `/api/accounts` | CRUD for user accounts |
| `AccountCategoriesController` | `/api/account-categories` | CRUD for account category types |
| `TransactionCategoriesController` | `/api/transaction-categories` | CRUD for transaction categories |
| `TransactionsController` | `/api/transactions` | Per-account transactions, bulk create |
| `DashboardController` | `/api/dashboard` | summary, spending-by-category, account-summary, insights |
| `FeedbackController` | `/api/feedback` | Submit user feedback / support requests |
| `HealthController` | `/health` | Simple health check returning 200 |
| `BaseController` | — | Shared helper: extracts authenticated user ID from JWT claims |

## Design Rules

- Controllers are `[Authorize]` by default (except Auth and Health endpoints)
- No business logic in controllers — all logic is delegated to Application services via injected interfaces
- `Result<T>` returns from services are matched to HTTP responses: `Ok()`, `BadRequest()`, `NotFound()`, `Unauthorized()`, `Conflict()`
- All endpoints accept and return **camelCase JSON** via global `JsonSerializer` options

## User Identity

All authenticated controllers inherit from `BaseController`, which exposes:

```csharp
protected string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
```

This extracts the user's ID from the validated JWT claims without any additional DB query.
