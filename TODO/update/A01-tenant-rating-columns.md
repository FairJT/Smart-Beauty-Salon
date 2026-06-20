# Task A01 — Add rating columns to Tenant 🟡

Make ONLY this change.

**File:** `src/Modules/Identity/Domain/Tenant.cs`

**Find (exact):**
```csharp
    public string Region { get; set; } = "IR";
```

**Replace with:**
```csharp
    public string Region { get; set; } = "IR";
    public long RatingSum { get; set; }    // running sum of all booking ratings
    public int RatingCount { get; set; }   // number of ratings; avg = RatingSum / RatingCount
```

**Done when:** `Tenant` has `RatingSum` and `RatingCount`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\Modules\Identity\Domain\Tenant.cs -Pattern "RatingSum|RatingCount"
```
Expect 2 hits.
