# M5 — Financial-transaction ledger + support staff 🟡

A simple salon ledger (rent, purchases, bills, salaries) and a "support staff" account type that
has no panel but exists so its salary can be paid.

## Step 1 — entity: FinancialTransaction
**New file:** `src/SalonOS.Infrastructure/SalonMgmt/FinancialTransaction.cs`
```csharp
using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum FinanceKind { Rent = 1, Purchase = 2, Bill = 3, Payroll = 4, Income = 5, Other = 6 }
public enum FinanceDirection { In = 1, Out = 2 }

public class FinancialTransaction : TenantEntity
{
    public FinanceKind Kind { get; set; }
    public FinanceDirection Direction { get; set; }
    public Money Amount { get; set; } = Money.Zero("IRR");
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? CounterpartyUserId { get; set; }   // e.g. the staff member for Payroll
    public string? Note { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Step 2 — register DbSet
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
```
**Replace with:**
```csharp
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
```

## Step 3 — configure the Money field
**File:** `src/SalonOS.Infrastructure/AppDbContext.cs`
**Find (exact):**
```csharp
        base.OnModelCreating(builder);
```
**Replace with:**
```csharp
        base.OnModelCreating(builder);

        builder.Entity<FinancialTransaction>(e => e.OwnsOne(t => t.Amount));
```

## Step 4 — migration
```powershell
dotnet ef migrations add FinancialTransactions --project src\SalonOS.Infrastructure --startup-project src\SalonOS.Api --context AppDbContext
```

## Step 5 — controller
**New file:** `src/SalonOS.Api/Controllers/FinanceController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/salon/finance")]
[ApiController]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _db;
    public FinanceController(AppDbContext db) => _db = db;

    public record TxRequest(FinanceKind Kind, FinanceDirection Direction, long AmountValue, string Currency,
        DateTime Date, string? CounterpartyUserId, string? Note, string? AttachmentUrl);

    [HttpGet]
    [HasPermission(Permissions.FinanceRevenueView)]
    public async Task<IActionResult> List([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var q = _db.FinancialTransactions.AsQueryable();
        if (from.HasValue) q = q.Where(t => t.Date >= from.Value);
        if (to.HasValue)   q = q.Where(t => t.Date <= to.Value);
        return Ok(await q.OrderByDescending(t => t.Date).ToListAsync());
    }

    [HttpPost]
    [HasPermission(Permissions.FinancePayoutManage)]
    public async Task<IActionResult> Create([FromBody] TxRequest r)
    {
        var t = new FinancialTransaction
        {
            Kind = r.Kind, Direction = r.Direction, Amount = Money.Of(r.AmountValue, r.Currency),
            Date = r.Date, CounterpartyUserId = r.CounterpartyUserId, Note = r.Note, AttachmentUrl = r.AttachmentUrl
        };
        _db.FinancialTransactions.Add(t);
        await _db.SaveChangesAsync();
        return Ok(t);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.FinancePayoutManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var t = await _db.FinancialTransactions.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        t.IsDeleted = true; t.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
```

## Step 6 — support staff role (account, no panel)
**File:** `src/Modules/Identity/Domain/Membership.cs`
**Find (exact):**
```csharp
    Staff        = 5,   // Artist / stylist
    Member       = 6    // Client
```
**Replace with:**
```csharp
    Staff        = 5,   // Artist / stylist
    Member       = 6,   // Client
    Support      = 7    // نیروی خدماتی — account only (for payroll), NO panel access
```
> No permission mapping is added for `Support`, so it gets no panel — exactly the intent. Its salary
> is recorded as a `FinancialTransaction` with `CounterpartyUserId = <that user>`.

**Done when:** build succeeds; manager can record/list/delete transactions, and a `Support` member has an account but no permissions.
