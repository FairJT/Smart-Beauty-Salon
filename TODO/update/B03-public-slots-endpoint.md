# Task B03 — Add a public slots endpoint 🟡 (review after)

Two steps in `src/SalonOS.Api/Controllers/SalonsController.cs`. Do Step 1, then Step 2.

---

## Step 1 — inject IBookingService + ITenantContext

**Find (exact):**
```csharp
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;

    public SalonsController(IdentityDbContext identityDb, BookingDbContext bookingDb)
    {
        _identityDb = identityDb;
        _bookingDb = bookingDb;
    }
```

**Replace with:**
```csharp
    private readonly IdentityDbContext _identityDb;
    private readonly BookingDbContext _bookingDb;
    private readonly SalonOS.Booking.Infrastructure.IBookingService _bookings;
    private readonly SalonOS.Shared.ITenantContext _tenant;

    public SalonsController(
        IdentityDbContext identityDb,
        BookingDbContext bookingDb,
        SalonOS.Booking.Infrastructure.IBookingService bookings,
        SalonOS.Shared.ITenantContext tenant)
    {
        _identityDb = identityDb;
        _bookingDb = bookingDb;
        _bookings = bookings;
        _tenant = tenant;
    }
```

---

## Step 2 — add the endpoint (insert before GetSalonBySlug)

**Find (exact):**
```csharp
    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalonBySlug(string slug)
```

**Replace with:**
```csharp
    // Public availability for one of a salon's artists. Resolves the tenant from the
    // PUBLIC slug, then scopes the request so RLS allows that salon's schedule/bookings.
    [HttpGet("{slug}/slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSlots(
        string slug,
        [FromQuery] string artistId,
        [FromQuery] DateTime date,
        [FromQuery] int durationMinutes = 30)
    {
        if (!Guid.TryParse(artistId, out var parsedArtistId))
            return BadRequest(new { message = "Invalid artistId" });

        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        // Scope this anonymous request to the resolved salon (read-only).
        _tenant.SetPublicTenant(tenant.Id);

        var slots = await _bookings.GetAvailableSlotsAsync(
            parsedArtistId, date, durationMinutes, tenant.Id);
        return Ok(slots);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalonBySlug(string slug)
```

**Done when:** the controller compiles and exposes `GET /api/salons/{slug}/slots`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Api\Controllers\SalonsController.cs -Pattern "GetPublicSlots"
```
Expect 1 hit.

**⚠️ Human review (one quick runtime check):** after `docker compose up`, call
`GET /api/salons/<a-real-slug>/slots?artistId=<id>&date=2026-07-01` with NO auth token.
It must return slots, not an empty list. If empty, the RLS session context wasn't picked up
for the second query — report back to Claude (the slots query may be reusing the slug-lookup
connection). Everything else in this batch is independent of that check.
