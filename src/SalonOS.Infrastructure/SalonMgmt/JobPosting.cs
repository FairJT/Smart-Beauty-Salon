using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum HireKind { Internship = 1, LineRental = 2, FixedSalary = 3, Percentage = 4 }

public class JobPosting : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public HireKind Kind { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

