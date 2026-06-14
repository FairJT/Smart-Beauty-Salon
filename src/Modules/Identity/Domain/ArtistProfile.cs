using SalonOS.Shared;

namespace SalonOS.Identity.Domain;

public class ArtistProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public int? SalonId { get; set; }
    public string Skill { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public ContractType ContractType { get; set; } = ContractType.FixedSalary;
    public Money? Salary { get; set; }
    public Money? RentAmount { get; set; }
    public string? RentTerms { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

public enum ContractType
{
    FixedSalary = 1,
    LineRental = 2
}
