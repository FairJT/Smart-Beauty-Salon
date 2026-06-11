namespace SalonOS.Marketplace.Domain;

/// <summary>
/// Template option entity - GLOBAL entity (no TenantId).
/// Individual options within an option group.
/// </summary>
public class TemplateOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OptionGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public TemplateOptionGroup OptionGroup { get; set; } = null!;
}
