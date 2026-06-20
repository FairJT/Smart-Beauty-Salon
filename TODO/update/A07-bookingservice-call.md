# Task A07 — Call the updater after a rating 🟡

Make ONLY this change. (Do Task A06 first — it adds `_ratingUpdater`.)

**File:** `src/Modules/Booking/Infrastructure/BookingService.cs`

**Find (exact):**
```csharp
        booking.Rating = rating;
        booking.Comment = comment;
        booking.IsRated = true;

        await _context.SaveChangesAsync();
    }
}
```

**Replace with:**
```csharp
        booking.Rating = rating;
        booking.Comment = comment;
        booking.IsRated = true;

        await _context.SaveChangesAsync();

        // Keep the salon's denormalized rating in sync (read by the public pages).
        await _ratingUpdater.AddRatingAsync(tenantId, rating);
    }
}
```

**Done when:** `RateAsync` calls `_ratingUpdater.AddRatingAsync(tenantId, rating)` after saving.

**Verify (PowerShell):**
```powershell
Select-String -Path src\Modules\Booking\Infrastructure\BookingService.cs -Pattern "AddRatingAsync"
```
Expect 1 hit.
