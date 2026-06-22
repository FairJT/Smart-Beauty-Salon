# DB1 — FK indexes + key string lengths 🟡 (review after)

Adds indexes on the foreign-key-like `Guid` columns and bounds the strings that need to be indexable.
All in `AppDbContext` (where these entities are configured).

## Step 1 — add the config block

**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`

**Find (exact):**
```csharp
        builder.Entity<StaffServiceContract>(e => e.OwnsOne(c => c.Amount));
        builder.Entity<FinancialTransaction>(e => e.OwnsOne(t => t.Amount));
```

**Replace with:**
```csharp
        builder.Entity<StaffServiceContract>(e =>
        {
            e.OwnsOne(c => c.Amount);
            e.HasIndex(x => x.ArtistId);
            e.HasIndex(x => x.CatalogServiceId);
        });
        builder.Entity<FinancialTransaction>(e =>
        {
            e.OwnsOne(t => t.Amount);
            e.HasIndex(x => x.CounterpartyUserId);
            e.Property(x => x.CounterpartyUserId).HasMaxLength(450);
        });

        // ── Indexes on FK-like columns + bounded strings (only TenantId is auto-indexed) ──
        builder.Entity<ClientNote>(e =>
        {
            e.HasIndex(x => new { x.ArtistId, x.ClientId });
            e.Property(x => x.ClientId).HasMaxLength(450);
        });
        builder.Entity<ClientFeedback>(e =>
        {
            e.HasIndex(x => x.ClientId);
            e.Property(x => x.ClientId).HasMaxLength(450);
            e.Property(x => x.Title).HasMaxLength(200);
        });
        builder.Entity<StaffRequest>(e =>
        {
            e.HasIndex(x => x.ArtistId);
            e.Property(x => x.Title).HasMaxLength(200);
        });
        builder.Entity<RescheduleRequest>(e =>
        {
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ArtistId);
        });
        builder.Entity<ProductUsage>(e =>
        {
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ArtistId);
            e.HasIndex(x => x.InventoryItemId);
        });
        builder.Entity<ArtistContract>(e => e.HasIndex(x => x.ArtistId));
        builder.Entity<ArtistLeave>(e => e.HasIndex(x => x.ArtistId));
        builder.Entity<JobApplication>(e => e.HasIndex(x => x.JobPostingId));
        builder.Entity<Discount>(e =>
        {
            e.HasIndex(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(64);
            e.Property(x => x.TargetClientId).HasMaxLength(450);
        });
        builder.Entity<SalonAmenity>(e => e.Property(x => x.Name).HasMaxLength(100));
        builder.Entity<SalonNotice>(e => e.Property(x => x.Title).HasMaxLength(200));
        builder.Entity<JobPosting>(e => e.Property(x => x.Title).HasMaxLength(200));
        builder.Entity<WorkingHour>(e =>
        {
            e.Property(x => x.OpenTime).HasMaxLength(5);
            e.Property(x => x.CloseTime).HasMaxLength(5);
        });
```

## Step 2 — migration
```powershell
dotnet ef migrations add DbIndexesAndLengths --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

**Done when:** build succeeds and the migration creates the indexes + alters the column lengths.

**⚠️ Review:**
- If any property name doesn't match (e.g. `WorkingHour.OpenTime` isn't a string, or `StaffRequest.Title`
  doesn't exist), STOP and report — don't guess.
- Shrinking a column that already holds longer data fails on apply. These are short fields (codes, ids,
  titles) so it's low-risk, but check on a copy of the DB first if you have real data.
