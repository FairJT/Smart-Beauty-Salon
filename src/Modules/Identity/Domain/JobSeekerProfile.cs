namespace SalonOS.Identity.Domain;

public class JobSeekerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string WorkHistory { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? PreferredRole { get; set; }
    public int ExpectedSalary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
