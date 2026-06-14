namespace SalonOS.Identity.Domain;

public class ClientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public int TotalVisits { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
