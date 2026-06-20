# Task A06 — Inject the updater into BookingService 🟡

Make ONLY this change (constructor only).

**File:** `src/Modules/Booking/Infrastructure/BookingService.cs`

**Find (exact):**
```csharp
    private readonly BookingDbContext _context;

    public BookingService(BookingDbContext context)
    {
        _context = context;
    }
```

**Replace with:**
```csharp
    private readonly BookingDbContext _context;
    private readonly SalonOS.Shared.Identity.ISalonRatingUpdater _ratingUpdater;

    public BookingService(
        BookingDbContext context,
        SalonOS.Shared.Identity.ISalonRatingUpdater ratingUpdater)
    {
        _context = context;
        _ratingUpdater = ratingUpdater;
    }
```

**Done when:** the constructor takes `ISalonRatingUpdater` and stores it in `_ratingUpdater`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\Modules\Booking\Infrastructure\BookingService.cs -Pattern "_ratingUpdater"
```
Expect 3 hits (field + assignment + later call after Task A07).
