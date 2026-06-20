# Task A05 — Register the updater in DI 🟡

Make ONLY this change.

**File:** `src/SalonOS.Api/Program.cs`

**Find (exact):**
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

**Replace with:**
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<SalonOS.Shared.Identity.ISalonRatingUpdater, SalonOS.Identity.Infrastructure.SalonRatingUpdater>();
```

**Done when:** `Program.cs` registers `ISalonRatingUpdater`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Api\Program.cs -Pattern "ISalonRatingUpdater"
```
Expect 1 hit.
