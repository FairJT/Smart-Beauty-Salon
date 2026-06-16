using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Application.DTOs;
using SalonOS.Catalog.Domain;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Shared;
using SalonOS.Shared.Authorization;
using SalonOS.Shared.Identity;
using Microsoft.AspNetCore.Authorization;

namespace SalonOS.Catalog.API.Controllers;

[Route("api/catalog-services")]
[ApiController]
[Authorize]
public class CatalogServiceController : ControllerBase
{
    private readonly CatalogDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly ICurrentUser _currentUser;

    public CatalogServiceController(CatalogDbContext db, ITenantContext tenant, ICurrentUser currentUser)
    {
        _db = db;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    [HttpGet]
    [HasPermission(Permissions.CatalogView)]
    public async Task<IActionResult> GetCatalogServices()
    {
        var services = await _db.CatalogServices
            .Where(s => s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .Include(s => s.ServiceType)
            .Include(s => s.Options)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.CatalogView)]
    public async Task<IActionResult> GetCatalogService(Guid id)
    {
        var service = await _db.CatalogServices
            .Where(s => s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .Include(s => s.ServiceType)
            .Include(s => s.Options)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound(new { message = "Catalog service not found" });

        return Ok(MapToDto(service));
    }

    [HttpPost]
    [HasPermission(Permissions.CatalogCreate)]
    public async Task<IActionResult> CreateCatalogService([FromBody] CreateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var service = new CatalogService
        {
            Name = dto.Name,
            Description = dto.Description,
            ServiceTypeId = dto.ServiceTypeId,
            BasePrice = Money.Of(dto.BasePriceAmount, dto.BasePriceCurrency),
            BaseDurationMinutes = dto.BaseDurationMinutes,
            TenantId = _tenant.TenantId
        };

        _db.CatalogServices.Add(service);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCatalogService), new { id = service.Id }, MapToDto(service));
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.CatalogEdit)]
    public async Task<IActionResult> UpdateCatalogService(Guid id, [FromBody] UpdateCatalogServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var service = await _db.CatalogServices
            .Where(s => s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound(new { message = "Catalog service not found" });

        if (dto.Name != null) service.Name = dto.Name;
        if (dto.Description != null) service.Description = dto.Description;
        if (dto.ServiceTypeId.HasValue) service.ServiceTypeId = dto.ServiceTypeId.Value;
        if (dto.BaseDurationMinutes.HasValue) service.BaseDurationMinutes = dto.BaseDurationMinutes.Value;
        if (dto.BasePriceAmount.HasValue && dto.BasePriceCurrency != null)
            service.BasePrice = Money.Of(dto.BasePriceAmount.Value, dto.BasePriceCurrency);

        service.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Catalog service updated successfully" });
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.CatalogDelete)]
    public async Task<IActionResult> DeleteCatalogService(Guid id)
    {
        var service = await _db.CatalogServices
            .Where(s => s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound(new { message = "Catalog service not found" });

        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Catalog service deleted successfully" });
    }

    // ── Service Options ───────────────────────────────────────────────────

    [HttpPost("{serviceId}/options")]
    [HasPermission(Permissions.CatalogEdit)]
    public async Task<IActionResult> AddServiceOption(Guid serviceId, [FromBody] CreateServiceOptionDto dto)
    {
        var service = await _db.CatalogServices
            .Where(s => s.TenantId == _tenant.TenantId && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            return NotFound(new { message = "Catalog service not found" });

        var option = new ServiceOption
        {
            CatalogServiceId = serviceId,
            Name = dto.Name,
            Description = dto.Description,
            PriceDelta = Money.Of(dto.PriceDeltaAmount, dto.PriceDeltaCurrency),
            DurationDeltaMinutes = dto.DurationDeltaMinutes,
            TenantId = _tenant.TenantId
        };

        _db.ServiceOptions.Add(option);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Option added successfully", id = option.Id });
    }

    [HttpDelete("{serviceId}/options/{optionId}")]
    [HasPermission(Permissions.CatalogEdit)]
    public async Task<IActionResult> RemoveServiceOption(Guid serviceId, Guid optionId)
    {
        var option = await _db.ServiceOptions
            .Where(o => o.TenantId == _tenant.TenantId && o.CatalogServiceId == serviceId)
            .FirstOrDefaultAsync(o => o.Id == optionId);

        if (option == null)
            return NotFound(new { message = "Option not found" });

        option.IsDeleted = true;
        option.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Option removed successfully" });
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static CatalogServiceDto MapToDto(CatalogService s)
    {
        return new CatalogServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ServiceTypeId = s.ServiceTypeId,
            ServiceTypeName = s.ServiceType?.Name ?? "",
            BaseDurationMinutes = s.BaseDurationMinutes,
            BasePriceAmount = s.BasePrice?.Amount ?? 0,
            BasePriceCurrency = s.BasePrice?.Currency ?? "IRR",
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            Options = s.Options?.Select(o => new ServiceOptionDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
                PriceDeltaAmount = o.PriceDelta?.Amount ?? 0,
                PriceDeltaCurrency = o.PriceDelta?.Currency ?? "IRR",
                DurationDeltaMinutes = o.DurationDeltaMinutes,
                IsActive = o.IsActive
            }).ToList() ?? new()
        };
    }
}
