using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ApplicationStatus { Pending = 1, Approved = 2, Rejected = 3 }

public class JobApplication : TenantEntity
{
    public Guid JobPostingId { get; set; }
    public string ApplicantUserId { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}