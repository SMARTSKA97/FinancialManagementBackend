# 🧠 Application Layer

The Application layer contains **all business logic**. It has no dependency on ASP.NET Core, EF Core, or any infrastructure concern — only the Domain layer.

## Contents

| Directory | Description |
|-----------|-------------|
| `Contracts/` | Service interfaces (`IAuthService`, `ITransactionService`, `IDashboardService`, etc.) |
| `DTOs/` | Data Transfer Objects for every domain operation — request and response shapes |
| `Services/` | Concrete implementations of all service interfaces |
| `Common/` | Shared result/error models (`Result<T>`, `Error`) used across all services |
| `Helpers/` | `QueryParameters` — reusable date-range filter DTO for controller query strings |
| `Validators/` | FluentValidation validators for request DTOs |
| `MappingProfile.cs` | AutoMapper profile mapping Entities ↔ DTOs |
| `JwtSettings.cs` | Strongly-typed config class bound to `appsettings.json` JWT section |

## Services

| Service | Responsibilities |
|---------|-----------------|
| `AuthService` | Registration (2-step OTP), login, refresh token rotation, logout, concurrent session detection, forgot/reset password |
| `AccountService` | CRUD for user accounts, balance recalculation |
| `AccountCategoryService` | CRUD for account category types |
| `TransactionService` | Create/read/update/delete transactions; handles Transfer type linking and balance adjustment |
| `TransactionCategoryService` | CRUD for transaction categories |
| `DashboardService` | Summary totals, spending-by-category aggregation, account summary, Deep Insights query |
| `FeedbackService` | Persists user feedback/support submissions |

## Result Pattern

All service methods return `Result<T>` — a discriminated union of success (carrying data) or failure (carrying an `Error` record with a dot-notated code and human-readable message). Controllers unwrap results and map them to appropriate HTTP status codes.

```csharp
// Success
return Result.Success(new SomeDto { ... });

// Failure
return Result.Failure<SomeDto>(new Error("Entity.NotFound", "Item not found."));
```
