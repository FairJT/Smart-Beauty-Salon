using SalonOS.Shared.Authorization;

namespace SalonOS.Shared.Authorization
{
    /// <summary>
    /// Role → permissions map (§R6.2).
    /// PlatformOwner is NOT here — it uses the bypass in §R6.4.
    /// </summary>
    public static class RolePermissions
    {
        public static readonly Dictionary<string, string[]> Map = new()
        {
            ["SalonManager"] = new[]
            {
                Permissions.SalonView,
                Permissions.SalonEdit,
                Permissions.SalonSettingsManage,
                Permissions.StaffView,
                Permissions.StaffCreate,
                Permissions.StaffEdit,
                Permissions.StaffDelete,
                Permissions.StaffContractManage,
            Permissions.StaffPerformanceView,
            Permissions.CatalogView,
            Permissions.ArtistLeaveView,
            Permissions.ArtistLeaveManage,
            Permissions.ArtistContractView,
            Permissions.ArtistContractManage,
            Permissions.ClientNoteCreate,
            Permissions.ClientNoteView,
            Permissions.ClientNoteDelete,
            Permissions.StaffRequestCreate,
            Permissions.StaffRequestView,
                Permissions.CatalogCreate,
                Permissions.CatalogEdit,
                Permissions.CatalogDelete,
                Permissions.CatalogPackageManage,
                Permissions.AppointmentViewAll,
                //Permissions.AppointmentCreate,      // book-on-behalf (replaces Receptionist role)
                Permissions.AppointmentConfirm,
                Permissions.AppointmentComplete,
                Permissions.AppointmentCancelAll,
                Permissions.InventoryView,
                Permissions.InventoryAdjust,
                Permissions.InventoryManage,
                Permissions.FinanceRevenueView,
                Permissions.FinanceDepositTake,
                Permissions.FinancePayoutManage,
                Permissions.FinancePeriodClose,
                Permissions.ReportSalonView,
                Permissions.LoyaltyConfigManage,
                Permissions.NotificationSend,
                Permissions.NotificationViewOwn,
                Permissions.MarketplaceBrowse,
                Permissions.MarketplaceLicensePurchase,
                Permissions.JobPostingView,
                Permissions.JobPostingManage,
            },
            ["Receptionist"] = new[]
            {
                Permissions.SalonView,
                Permissions.StaffView,
                Permissions.CatalogView,
                Permissions.AppointmentViewOwn,
                Permissions.AppointmentCreate,
                Permissions.AppointmentConfirm,
                Permissions.AppointmentComplete,
                Permissions.AppointmentCancelOwn,
                Permissions.AppointmentRate,
                Permissions.AppointmentCancelAll,
                Permissions.FinanceDepositTake,
                Permissions.NotificationViewOwn,
            },
            ["Artist"] = new[]
            {
                Permissions.SalonView,
                Permissions.CatalogView,
                Permissions.StaffPerformanceView,
                Permissions.AppointmentViewOwn,
                Permissions.AppointmentConfirm,
                Permissions.AppointmentComplete,
                Permissions.AppointmentCancelOwn,
                Permissions.ReportStaffViewOwn,
                Permissions.FinancePayoutViewOwn,
                Permissions.NotificationViewOwn,
                // New artist permissions
                Permissions.ArtistLeaveView,
                Permissions.ArtistLeaveManage,
                Permissions.ArtistContractView,
                Permissions.ArtistContractManage,
                Permissions.ClientNoteCreate,
                Permissions.ClientNoteView,
                Permissions.ClientNoteDelete,
                Permissions.StaffRequestCreate,
                Permissions.StaffRequestView,
                // Additional artist permissions
                Permissions.LeaveRequestOwn,
                Permissions.AppointmentCheckIn,
                Permissions.RescheduleRequestCreate,
                Permissions.ClientNoteManageOwn,
                Permissions.ProductUsageRecord,
            },
            ["Client"] = new[]
            {
                Permissions.ClientSelf,
                Permissions.AppointmentViewOwn,
                Permissions.AppointmentCreate,
                Permissions.AppointmentCancelOwn,
                Permissions.AppointmentRate,
                Permissions.LoyaltyViewOwn,
                Permissions.NotificationViewOwn,
            },
        };
    }
}