# Task 10 — Add the JobSeekerEnabled opt-in flag 🟡 (review after)

Two steps: add the field, then generate the migration.

---

## Step 1 — add the field

**File:** `src/Modules/Identity/Domain/ClientProfile.cs`

**Find (exact):**
```csharp
    public int TotalVisits { get; set; }
```

**Replace with:**
```csharp
    public int TotalVisits { get; set; }
    public bool JobSeekerEnabled { get; set; } = false;   // opt-in JobSeeker capability
```

**Done when:** `ClientProfile` has a `JobSeekerEnabled` bool defaulting to false.

---

## Step 2 — generate the EF migration

Run from the repo root (mirror how previous Identity migrations were created — same context):
```powershell
dotnet ef migrations add AddJobSeekerEnabled `
  --project src\Modules\Identity `
  --startup-project src\SalonOS.Api `
  --context IdentityDbContext
```

**Done when:** a new migration file appears under
`src\Modules\Identity\Infrastructure\Migrations\` adding a `JobSeekerEnabled` column,
and `dotnet build SalonOS.slnx` succeeds.

**⚠️ Human review:** if the `dotnet ef` command errors (wrong project/context path), STOP and report
the exact error — do not hand-write the migration. `ClientProfile` is a GLOBAL entity (no TenantId),
so the column must NOT be tenant-scoped.
