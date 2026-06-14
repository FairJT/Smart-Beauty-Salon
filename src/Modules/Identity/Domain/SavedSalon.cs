namespace SalonOS.Identity.Domain;

public class SavedSalon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public int SalonId { get; set; }
    public string SalonName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
