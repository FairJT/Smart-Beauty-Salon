# Task A02 — EF migration for the rating columns 🟡

No file edit — run the command (mirror how earlier Identity migrations were created).

**Run (PowerShell) from repo root:**
```powershell
dotnet ef migrations add AddSalonRatingDenorm `
  --project src\Modules\Identity `
  --startup-project src\SalonOS.Api `
  --context IdentityDbContext
```

**Done when:** a new migration appears under
`src\Modules\Identity\Infrastructure\Migrations\` adding `RatingSum` and `RatingCount`
to the `Tenants` table.

**⚠️ If the command errors** (wrong project/context), STOP and report the exact error.
Do not hand-write the migration.
