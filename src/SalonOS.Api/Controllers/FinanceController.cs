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
        var query = _db.FinancialTransactions.AsQueryable();
        if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue)   query = query.Where(t => t.Date <= to.Value);
        return Ok(await query.OrderByDescending(t => t.Date).ToListAsync());
    }

    [HttpPost]
    [HasPermission(Permissions.FinancePayoutManage)]
    public async Task<IActionResult> Create([FromBody] TxRequest r)
    {
        var tx = new FinancialTransaction
        {
            Kind = r.Kind,
            Direction = r.Direction,
            Amount = Money.Of(r.AmountValue, r.Currency),
            Date = r.Date,
            CounterpartyUserId = r.CounterpartyUserId,
            Note = r.Note,
            AttachmentUrl = r.AttachmentUrl
        };
        _db.FinancialTransactions.Add(tx);
        await _db.SaveChangesAsync();
        return Ok(tx);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.FinancePayoutManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tx = await _db.FinancialTransactions.FirstOrDefaultAsync(t => t.Id == id);
        if (tx is null) return NotFound();
        tx.IsDeleted = true;
        tx.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}