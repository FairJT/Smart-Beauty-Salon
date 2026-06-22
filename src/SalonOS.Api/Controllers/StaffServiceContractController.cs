using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Infrastructure;

namespace SalonOS.Api.Controllers;

[Route("api/staff-contracts")]
[ApiController]
[Authorize]
public class StaffServiceContractController : ControllerBase
{
    private readonly AppDbContext _db;
    public StaffServiceContractController(AppDbContext db) => _db = db;

    public record ContractRequest(
        Guid ArtistId, Guid CatalogServiceId, StaffContractKind Kind,
        long AmountValue, string Currency, int? DiscountPercent,
        DateTime StartDate, DateTime? EndDate, string? ContractFileUrl, string? GuaranteeNote);

        [HttpGet]
        [HasPermission(Permissions.StaffView)]
        public async Task<IActionResult> List([FromQuery] Guid? artistId)
        {
            var query = _db.StaffServiceContracts.Where(c => c.IsActive);
            if (artistId.HasValue) query = query.Where(c => c.ArtistId == artistId.Value);
            return Ok(await query.ToListAsync());
        }

        // Artist view own contracts
    [HttpGet("my")]
    [HasPermission(Permissions.StaffPerformanceView)]
    public async Task<IActionResult> MyContracts()
    {
        var artistId = User.FindFirst("artist_id")?.Value;
        if (string.IsNullOrEmpty(artistId) || !Guid.TryParse(artistId, out var parsedArtistId))
            return Forbid();

        var contracts = await _db.StaffServiceContracts
            .Where(c => c.ArtistId == parsedArtistId && c.IsActive)
            .ToListAsync();

        return Ok(contracts);
    }

    [HttpPost]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> Create([FromBody] ContractRequest r)
    {
        var contract = new StaffServiceContract
        {
            ArtistId = r.ArtistId,
            CatalogServiceId = r.CatalogServiceId,
            Kind = r.Kind,
            Amount = Money.Of(r.AmountValue, r.Currency),
            DiscountPercent = r.DiscountPercent,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            ContractFileUrl = r.ContractFileUrl,
            GuaranteeNote = r.GuaranteeNote
        };
        _db.StaffServiceContracts.Add(contract);
        await _db.SaveChangesAsync();
        return Ok(contract);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.StaffContractManage)]
    public async Task<IActionResult> End(Guid id)
    {
        var contract = await _db.StaffServiceContracts.FirstOrDefaultAsync(c => c.Id == id);
        if (contract is null) return NotFound();
        contract.IsActive = false;
        contract.IsDeleted = true;
        contract.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { ended = true });
    }
}