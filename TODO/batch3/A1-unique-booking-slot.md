# Task A1 — Prevent double-booking 🟡 (review after)

Two steps. The current index on `(ArtistId, StartsAt)` is NOT unique, so two clients can
take the same slot. Make it a UNIQUE index, but FILTERED so cancelled/deleted rows don't
block re-booking.

---

## Step 1 — make the index unique + filtered

**File:** `src/Modules/Booking/Infrastructure/BookingDbContext.cs`

**Find (exact):**
```csharp
            e.HasIndex(b => new { b.ArtistId, b.StartsAt });
```

**Replace with:**
```csharp
            // Prevent double-booking: one ACTIVE booking per artist per start time.
            // Cancelled (Status = 5) and soft-deleted rows are excluded so freed slots can be re-booked.
            e.HasIndex(b => new { b.ArtistId, b.StartsAt })
                .IsUnique()
                .HasFilter("[Status] <> 5 AND [IsDeleted] = 0");
```

---

## Step 2 — migration

```powershell
dotnet ef migrations add UniqueActiveBookingSlot `
  --project src\Modules\Booking `
  --startup-project src\SalonOS.Api `
  --context BookingDbContext
```

**Done when:** a migration appears under the Booking module's `Migrations/` creating a unique
filtered index on `Bookings (ArtistId, StartsAt)`.

**⚠️ Human review:** if the DB already has two ACTIVE bookings on the same artist+time, applying
this will FAIL. Before deploying, check for and resolve duplicates:
```sql
SELECT ArtistId, StartsAt, COUNT(*) FROM Bookings
WHERE Status <> 5 AND IsDeleted = 0
GROUP BY ArtistId, StartsAt HAVING COUNT(*) > 1;
```
