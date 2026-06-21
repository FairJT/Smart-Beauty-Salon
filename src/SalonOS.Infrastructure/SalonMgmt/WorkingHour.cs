using SalonOS.Shared;

namespace SalonOS.Infrastructure;

public class WorkingHour : TenantEntity
{
    public int DayOfWeek { get; set; }              // 0=Sat ... 6=Fri
    public string OpenTime { get; set; } = "09:00"; // "HH:mm"
    public string CloseTime { get; set; } = "21:00";
    public bool IsClosed { get; set; }              // weekly day off
}