using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonOS.Catalog.Application.DTOs;
using SalonOS.Catalog.Domain;
using SalonOS.Catalog.Infrastructure;
using SalonOS.Shared.Authorization;

namespace SalonOS.Catalog.API.Controllers;

[Route("api/service-types")]
[ApiController]
public class ServiceTypesController : ControllerBase
{
    private readonly CatalogDbContext _db;

    public ServiceTypesController(CatalogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [HasPermission(Permissions.CatalogView)]
    public async Task<IActionResult> GetServiceTypes()
    {
        var types = await _db.ServiceTypes
            .Where(t => t.IsActive)
            .Select(t => new ServiceTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                Description = t.Description,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.CatalogView)]
    public async Task<IActionResult> GetServiceType(Guid id)
    {
        var type = await _db.ServiceTypes.FindAsync(id);
        if (type == null)
            return NotFound(new { message = "Service type not found" });

        return Ok(new ServiceTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Category = type.Category,
            Description = type.Description,
            IsActive = type.IsActive
        });
    }

    [HttpPost]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> CreateServiceType([FromBody] CreateServiceTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var type = new ServiceType
        {
            Name = dto.Name,
            Category = dto.Category,
            Description = dto.Description
        };

        _db.ServiceTypes.Add(type);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServiceType), new { id = type.Id }, new ServiceTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Category = type.Category,
            Description = type.Description,
            IsActive = type.IsActive
        });
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> UpdateServiceType(Guid id, [FromBody] CreateServiceTypeDto dto)
    {
        var type = await _db.ServiceTypes.FindAsync(id);
        if (type == null)
            return NotFound(new { message = "Service type not found" });

        type.Name = dto.Name;
        type.Category = dto.Category;
        type.Description = dto.Description;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Service type updated successfully" });
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.PlatformConfigManage)]
    public async Task<IActionResult> DeleteServiceType(Guid id)
    {
        var type = await _db.ServiceTypes.FindAsync(id);
        if (type == null)
            return NotFound(new { message = "Service type not found" });

        type.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Service type deactivated successfully" });
    }
}
