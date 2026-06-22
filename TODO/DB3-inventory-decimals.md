# DB3 — Inventory decimal precision 🟢 (optional, cleanup)

Inventory entities use raw `decimal` (EF defaults to `decimal(18,2)` and logs a warning). Set explicit
precision to silence the warning and make intent clear. Cosmetic — not a bug.

**Step 1 — find the decimal properties:**
```powershell
Select-String -Path src\Modules\Inventory\Domain\*.cs -Pattern "public decimal"
```

**Step 2 — add precision in `InventoryDbContext`.**
**File:** `src/Modules/Inventory/Infrastructure/InventoryDbContext.cs`
In `OnModelCreating`, for each decimal property found, add (adjust names to what Step 1 returned):
```csharp
        builder.Entity<InventoryItem>(e => e.Property(p => p.OnHandQty).HasColumnType("decimal(18,4)"));
        builder.Entity<StockMovement>(e => e.Property(p => p.Quantity).HasColumnType("decimal(18,4)"));
```
(Use the REAL property names from Step 1. If a name differs, use that name; don't invent one.)

**Step 3 — migration:**
```powershell
dotnet ef migrations add InventoryDecimalPrecision --project src\Modules\Inventory --startup-project src\SalonOS.Api --context InventoryDbContext
```

**Done when:** build succeeds and the decimal columns have explicit precision. Skip this task entirely
if you'd rather not touch inventory now — it's the lowest-priority item.
