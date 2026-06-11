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
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public bool IsActive => EndDate >= DateTime.Now;
        public decimal PaidAmount { get; set; }

        // Navigation
        public SmartSalon.Models.Salon? Salon { get; set; }
        public ServicePackage? Package { get; set; }
    }
}