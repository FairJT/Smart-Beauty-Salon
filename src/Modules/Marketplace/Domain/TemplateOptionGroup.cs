namespace SalonOS.Marketplace.Domain;

/// <summary>
/// Template option group entity - GLOBAL entity (no TenantId).
/// Groups related options together (e.g., "Hair Color", "Treatment Type").
/// </summary>
public class TemplateOptionGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ServiceTemplate ServiceTemplate { get; set; } = null!;
    public ICollection<TemplateOption> Options { get; set; } = new List<TemplateOption>();
}
