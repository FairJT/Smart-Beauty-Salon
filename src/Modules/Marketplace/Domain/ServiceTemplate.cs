namespace SalonOS.Marketplace.Domain;

/// <summary>
/// Service template entity - GLOBAL entity (no TenantId).
/// Defines the structure and options for salon services.
/// </summary>
public class ServiceTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<TemplateOptionGroup> OptionGroups { get; set; } = new List<TemplateOptionGroup>();
}
