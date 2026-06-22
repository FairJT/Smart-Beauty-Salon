using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public enum ClientNoteType { Preference = 1, Sensitivity = 2, Suggestion = 3, ProductTip = 4 }

public class ClientNote : TenantEntity
{
    public Guid ArtistId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public ClientNoteType Type { get; set; } = ClientNoteType.Preference;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}