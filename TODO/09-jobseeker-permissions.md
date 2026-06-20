# Task 09 — Add JobSeeker permission constants 🟢

Constants only. They are not mapped to any role yet — that's a later step. No behavior change.

**File:** `src/SalonOS.Shared/Authorization/Permissions.cs`

**Find (exact):**
```csharp
    // ─── Platform / Tenant ──────────────────────────────────
```

**Replace with:**
```csharp
    // ─── JobSeeker / Job market ──────────────────────────────
    public const string JobSeekerProfileManage = "jobseeker.profile.manage";
    public const string JobPostingView         = "job.posting.view";
    public const string JobPostingManage       = "job.posting.manage";   // SalonManager
    public const string JobApplicationCreate   = "job.application.create";

    // ─── Platform / Tenant ──────────────────────────────────
```

**Done when:** the four `Job*` constants exist and the file compiles.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Shared\Authorization\Permissions.cs -Pattern "JobSeekerProfileManage|JobPostingView|JobPostingManage|JobApplicationCreate"
```
Expect 4 hits.
