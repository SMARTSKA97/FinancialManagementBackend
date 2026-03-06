# 🧱 Domain / Core Layer

The Core (Domain) layer contains **pure C# entities and value objects** — no framework dependencies, no EF Core attributes, no ASP.NET references. This is the innermost ring of the Clean Architecture onion.

## Entities

| Entity | Description |
|--------|-------------|
| `ApplicationUser` | Extends `IdentityUser` — adds `Name`, `DateOfBirth`, `CurrentSessionId`, `LastLoginTime`, `LastKnownIp`, `LastKnownUserAgent`, and a collection of `RefreshTokens` |
| `RefreshToken` | Owned by `ApplicationUser` — stores token string, expiry, creation IP, revocation details |
| `Account` | A financial container (bank account, wallet, etc.) owned by a user — has `Name`, `Balance`, `AccountCategoryId` |
| `AccountCategory` | User-defined category for grouping accounts (e.g., Savings, Credit Card) |
| `Transaction` | An individual financial event — `Amount`, `Date`, `Type` (Income/Expense/Transfer), `CategoryId`, `AccountId`, optional `DestinationAccountId` for Transfers |
| `TransactionCategory` | User-defined category for classifying transactions (e.g., Groceries, Salary, Fuel) |
| `Feedback` | A support / feedback submission from an authenticated user |

## Design Rules

- No EF Core data annotations on entities — configuration is done via `OnModelCreating` in `AppDbContext`
- No `INotifyPropertyChanged` or any framework-specific interfaces
- Entities are **POCOs** (Plain Old C# Objects)
- Navigation properties use `ICollection<T>` or direct references, initialised with `new List<T>()` defaults

## Value Semantics

The `RefreshToken` is configured as an **owned entity** in EF Core — it has no primary key table of its own and is stored as part of the user aggregate.
