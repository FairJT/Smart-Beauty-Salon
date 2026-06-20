# Task A03 — Create the ISalonRatingUpdater interface 🟢

Create a NEW file. (This interface lets the Booking module update the salon rating
without touching Identity's tables directly — the cross-module rule.)

**New file:** `src/SalonOS.Shared/Identity/ISalonRatingUpdater.cs`

**Content:**
```csharp
namespace SalonOS.Shared.Identity;

/// <summary>
/// Lets other modules keep a salon's denormalized rating aggregate in sync
/// without reaching into the Identity tables directly.
/// </summary>
public interface ISalonRatingUpdater
{
    Task AddRatingAsync(Guid tenantId, int rating);
}
```

**Done when:** the file exists and the solution still compiles.

**Verify (PowerShell):**
```powershell
Test-Path src\SalonOS.Shared\Identity\ISalonRatingUpdater.cs
```
Expect `True`.
