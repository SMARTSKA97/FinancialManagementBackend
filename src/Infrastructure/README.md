# 🏗️ Infrastructure Layer

The Infrastructure layer contains **all external integrations** — database, Redis, email, and any third-party services. It implements the interfaces defined in the Application layer.

## Contents

| Directory / File | Description |
|-----------------|-------------|
| `Data/AppDbContext.cs` | EF Core `DbContext` — configures all entity mappings, Identity tables, and relationships |
| `Data/Migrations/` | EF Core auto-generated migration files for the PostgreSQL schema |
| `Services/EmailService.cs` | Sends styled HTML transactional emails via SMTP — implements `IEmailService` |

## Database

PostgreSQL is used as the primary data store. Connection string is configured via `appsettings.json` under:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
}
```

EF Core handles schema creation and migrations. Run migrations with:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation
```

## Redis

Redis is used exclusively for short-lived ephemeral data:

| Redis Key Pattern | TTL | Purpose |
|-------------------|-----|---------|
| `reg:{email}` | 10 min | Pending registration payload (JSON) |
| `reg-otp:{email}` | 10 min | Registration verification OTP |
| `otp:{email}` | 10 min | Password reset OTP |

Token rotation and session management use the PostgreSQL-backed `RefreshTokens` table (as part of `ApplicationUser`), not Redis.

## Email Templates

All system emails are fully styled dark-theme HTML with:
- Inline CSS (email client safe)
- `Outfit` Google Font (with fallback stack)
- Gradient OTP code box
- Warning section for unsolicited use
- BCrypt-hashed credentials — passwords are never stored or transmitted in plain text
