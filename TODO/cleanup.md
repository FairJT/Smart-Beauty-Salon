# Health cleanup 🟢 (small, optional)

From the integration audit. None of these are urgent — the live paths work — but they tidy real smells.

## 1 — remove the duplicate DI registration
**File:** `src/SalonOS.Api/Program.cs`
**Find (exact):**
```csharp
builder.Services.AddScoped<SalonOS.Shared.Identity.ISalonRatingUpdater, SalonOS.Identity.Infrastructure.SalonRatingUpdater>();
builder.Services.AddScoped<SalonOS.Shared.Identity.ISalonRatingUpdater, SalonOS.Identity.Infrastructure.SalonRatingUpdater>();
```
**Replace with (one line):**
```csharp
builder.Services.AddScoped<SalonOS.Shared.Identity.ISalonRatingUpdater, SalonOS.Identity.Infrastructure.SalonRatingUpdater>();
```

## 2 — fix the dead frontend API constants
**File:** `smart_salon_app/lib/data/datasources/api_constants.dart`
Correct the three paths that don't match the backend (they're currently unused, so this just prevents
a future 404):
- `…/api/catalog/services` → `…/api/catalog-services`
- `…/api/inventory` → `…/api/inventory-items`
- `…/api/marketplace` → `…/api/service-templates` (or remove the `marketplace` constant)

**Verify:** `Select-String -Path smart_salon_app\lib -Pattern "api/catalog/services|api/marketplace\"" -Recurse` → 0 hits.

## 3 — NOTE (not a task yet): when you build Inventory or Service-Templates/Panel-sales for real
Register their contexts in `Program.cs` next to the others:
```csharp
builder.Services.AddDbContext<InventoryDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<MarketplaceDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
Until then they're dormant (no crash, but those features don't persist). Leave this until you actually
wire those features — flag it to Claude when you do.
