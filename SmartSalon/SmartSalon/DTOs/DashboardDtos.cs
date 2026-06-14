namespace SmartSalon.DTOs
{
    public class DashboardMoney
    {
        public long Amount { get; set; }
        public string Currency { get; set; } = "IRR";
    }

    public class SalonManagerDashboardDto
    {
        public int TodayAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public DashboardMoney Revenue { get; set; } = new();
        public List<ArtistUtilizationDto> ArtistUtilization { get; set; } = new();
        public int ActiveServiceCount { get; set; }
        public int ActiveArtistCount { get; set; }
        public string? SubscriptionStatus { get; set; }
    }

    public class ArtistUtilizationDto
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public int TodayAppointments { get; set; }
        public int CompletedToday { get; set; }
        public double UtilizationPercent { get; set; }
    }

    public class ArtistDashboardDto
    {
        public int TodayAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public ArtistNextAppointmentDto? NextAppointment { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public int MonthAppointments { get; set; }
        public DashboardMoney? MonthRevenue { get; set; }
    }

    public class ArtistNextAppointmentDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Status { get; set; }
    }

    public class ClientDashboardDto
    {
        public int UpcomingBookings { get; set; }
        public ClientNextBookingDto? NextBooking { get; set; }
        public int LoyaltyPoints { get; set; }
        public int TotalVisits { get; set; }
        public int UnreadNotifications { get; set; }
        public List<FavoriteSalonDto> FavoriteSalons { get; set; } = new();
    }

    public class ClientNextBookingDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public int Status { get; set; }
    }

    public class FavoriteSalonDto
    {
        public int SalonId { get; set; }
        public string SalonName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public double RatingAvg { get; set; }
        public bool IsVip { get; set; }
    }

    public class SuperAdminDashboardDto
    {
        public int TotalTenants { get; set; }
        public int TotalSalons { get; set; }
        public int ActiveSalons { get; set; }
        public int TotalArtists { get; set; }
        public int ActiveSubscriptions { get; set; }
        public DashboardMoney PlatformRevenue { get; set; } = new();
    }
}
