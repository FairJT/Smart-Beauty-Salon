namespace SalonOS.Infrastructure;

public enum PlacementType { Vip = 1, Ladder = 2, Ad = 3 }

public class SalonPlacement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SalonTenantId { get; set; }   // which salon is promoted (reference only — NOT row scoping)
    public PlacementType Type { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Weight { get; set; }            // higher = shown first
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}