using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ArtistContractType { Internship = 1, LineRental = 2, FixedSalary = 3, Percentage = 4 }

public class ArtistContract : TenantEntity
{
    public Guid ArtistId { get; set; }
    public ArtistContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}