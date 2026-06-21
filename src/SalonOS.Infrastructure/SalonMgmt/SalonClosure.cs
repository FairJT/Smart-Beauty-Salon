using SalonOS.Shared;

namespace SalonOS.Infrastructure;

// Overrides the weekly hours for a specific date.
// IsClosed=true → closed that day; IsClosed=false → OPEN even if it's an official holiday/Friday.
public class SalonClosure : TenantEntity
{
    public DateTime Date { get; set; }
    public bool IsClosed { get; set; } = true;
    public string? Reason { get; set; }
}