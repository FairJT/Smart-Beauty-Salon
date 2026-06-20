# Task A09 — GetSalonBySlug reads rating columns 🟢

Make ONLY this change. Same idea as A08, for the single-salon endpoint.

**File:** `src/SalonOS.Api/Controllers/SalonsController.cs`

**Find (exact):**
```csharp
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        var ratingStats = await _bookingDb.Bookings
            .Where(b => b.TenantId == tenant.Id && b.IsRated && b.Rating.HasValue)
            .GroupBy(b => b.TenantId)
            .Select(g => new { AvgRating = g.Average(b => b.Rating!.Value), Count = g.Count() })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            slug = tenant.Slug,
            name = tenant.Name,
            description = tenant.Description,
            address = tenant.Address,
            phoneNumber = tenant.Phone,
            imageUrl = tenant.LogoUrl,
            latitude = 0.0,
            longitude = 0.0,
            rating = ratingStats?.AvgRating ?? 0,
            reviewCount = ratingStats?.Count ?? 0
        });
```

**Replace with:**
```csharp
        var tenant = await _identityDb.Tenants
            .Where(t => t.Slug == slug && t.IsActive)
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl, t.RatingSum, t.RatingCount })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound(new { message = "Salon not found" });

        return Ok(new
        {
            slug = tenant.Slug,
            name = tenant.Name,
            description = tenant.Description,
            address = tenant.Address,
            phoneNumber = tenant.Phone,
            imageUrl = tenant.LogoUrl,
            latitude = 0.0,
            longitude = 0.0,
            rating = tenant.RatingCount > 0 ? (double)tenant.RatingSum / tenant.RatingCount : 0,
            reviewCount = tenant.RatingCount
        });
```

**Done when:** `GetSalonBySlug` no longer queries `_bookingDb.Bookings`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Api\Controllers\SalonsController.cs -Pattern "_bookingDb.Bookings"
```
Expect **0 hits** (both endpoints now read columns).
