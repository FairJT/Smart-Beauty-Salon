namespace SalonOS.Identity.Domain;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SalonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? FontColor { get; set; }
    public string? License { get; set; }
    public string? Grade { get; set; }
    public string? Fax { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Region { get; set; } = "IR";
    
        public long RatingSum { get; set; }    // running sum of all booking ratings
        public int RatingCount { get; set; }   // number of ratings; avg = RatingSum / RatingCount
}
