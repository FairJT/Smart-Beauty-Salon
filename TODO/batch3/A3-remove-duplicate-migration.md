# Task A3 — Remove the empty duplicate JobSeeker migration 🟡 (review after)

There are two migrations adding the JobSeeker flag; the second
(`...AddJobSeekerEnabledFlag`) has an empty `Up()` and is dead weight.

**Run (PowerShell) from repo root:**
```powershell
dotnet ef migrations remove `
  --project src\Modules\Identity `
  --startup-project src\SalonOS.Api `
  --context IdentityDbContext
```

**Done when:** the `AddJobSeekerEnabledFlag` migration (`.cs` + `.Designer.cs`) is gone and the
model snapshot still builds. Confirm `AddJobSeekerEnabled` (the real one that adds the column) is STILL present.

**⚠️ Human review:** `migrations remove` only removes the LAST migration. Confirm
`AddJobSeekerEnabledFlag` is the most recent Identity migration before running. If it has already
been applied to a shared database, do NOT remove it — just leave it (it's harmless). If `dotnet ef`
errors, STOP and report.
