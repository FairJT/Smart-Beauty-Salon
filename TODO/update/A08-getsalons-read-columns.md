# Task A08 — GetSalons reads rating columns, not Bookings 🟢

Make ONLY this change. This removes the `Bookings` query (which RLS now blocks for
anonymous users) and reads the denormalized columns instead.

**File:** `src/SalonOS.Api/Controllers/SalonsController.cs`

**Find (exact):**
```csharp
        var query = _identityDb.Tenants.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || (t.Address != null && t.Address.Contains(search)));

        var tenantList = await query
            .Select(t => new { t.Id, t.Name, t.Slug, t.Description, t.Address, t.Phone, t.LogoUrl })
            .ToListAsync();

        var tenantIds = tenantList.Select(t => t.Id).ToList();
        var ratingStats = await _bookingDb.Bookings
            .Where(b => b.IsRated && b.Rating.HasValue && tenantIds.Contains(b.TenantId))
            .GroupBy(b => b.TenantId)
            .Select(g => new { TenantId = g.Key, AvgRating = g.Average(b => b.Rating!.Value), Count = g.Count() })
            .ToListAsync();

        var ratingMap = ratingStats.ToDictionary(r => r.TenantId);

        var salons = tenantList.Select(t =>
        {
            var stats = ratingMap.GetValueOrDefault(t.Id);
            return new
            {
                slug = t.Slug,
                name = t.Name,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                rating = stats?.AvgRating ?? 0,
                reviewCount = stats?.Count ?? 0
            };
        }).ToList();

        return Ok(salons);
```

**Replace with:**
```csharp
        var query = _identityDb.Tenants.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || (t.Address != null && t.Address.Contains(search)));

        var salons = await query
            .Select(t => new
            {
                slug = t.Slug,
                name = t.Name,
                description = t.Description,
                address = t.Address,
                phoneNumber = t.Phone,
                imageUrl = t.LogoUrl,
                rating = t.RatingCount > 0 ? (double)t.RatingSum / t.RatingCount : 0,
                reviewCount = t.RatingCount
            })
            .ToListAsync();

        return Ok(salons);
```

**Done when:** `GetSalons` no longer queries `_bookingDb.Bookings`.

**Verify (PowerShell):**
```powershell
Select-String -Path src\SalonOS.Api\Controllers\SalonsController.cs -Pattern "RatingSum"
```
Expect at least 1 hit.
