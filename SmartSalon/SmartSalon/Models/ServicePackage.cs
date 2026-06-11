namespace SmartSalon.Models
{
    public class ServicePackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public int DurationMonths { get; set; } = 6;
        public bool IsActive { get; set; } = true;
       
    }

    public class SalonPackageSubscription
    {
        public int Id { get; set; }
        public int SalonId { get; set; }
        public int PackageId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal PaidAmount { get; set; }

        // Navigation
        public Salon? Salon { get; set; }
        public ServicePackage? Package { get; set; }
    }
}