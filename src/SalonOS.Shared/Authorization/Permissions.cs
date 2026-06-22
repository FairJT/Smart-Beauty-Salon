namespace SalonOS.Shared.Authorization;

/// <summary>
/// Permission constants — one per row in §R3 of the access-control design.
/// Format: resource.action[.scope]
/// </summary>
public static class Permissions
{
    // ─── Salon ───────────────────────────────────────────────
    public const string SalonView = "salon.view";
    public const string SalonEdit = "salon.edit";
    public const string SalonSettingsManage = "salon.settings.manage";

    // ─── Staff / Artist ──────────────────────────────────────
    public const string StaffView = "staff.view";
    public const string StaffCreate = "staff.create";
    public const string StaffEdit = "staff.edit";
    public const string StaffDelete = "staff.delete";
    public const string StaffContractManage = "staff.contract.manage";
    public const string StaffPerformanceView = "staff.performance.view";
    // Artist leave & contract permissions
    public const string ArtistLeaveView = "artist.leave.view";
    public const string ArtistLeaveManage = "artist.leave.manage";
    public const string ArtistContractView = "artist.contract.view";
    public const string ArtistContractManage = "artist.contract.manage";
    public const string ClientNoteCreate = "client.note.create";
    public const string ClientNoteView = "client.note.view";
    public const string ClientNoteDelete = "client.note.delete";
    public const string StaffRequestCreate = "staff.request.create";
    public const string StaffRequestView = "staff.request.view";

    // ─── Catalog / Service ───────────────────────────────────
    public const string CatalogView = "catalog.view";
    public const string CatalogCreate = "catalog.create";
    public const string CatalogEdit = "catalog.edit";
    public const string CatalogDelete = "catalog.delete";
    public const string CatalogPackageManage = "catalog.package.manage";

    // ─── Appointment ─────────────────────────────────────────
    public const string AppointmentViewAll = "appointment.view.all";
    public const string AppointmentViewOwn = "appointment.view.own";
    public const string AppointmentCreate = "appointment.create";
    public const string AppointmentConfirm = "appointment.confirm";
    public const string AppointmentComplete = "appointment.complete";
    public const string AppointmentCancelAll = "appointment.cancel.all";
    public const string AppointmentCancelOwn = "appointment.cancel.own";
    public const string AppointmentRate = "appointment.rate";

    // ─── Inventory ───────────────────────────────────────────
    public const string InventoryView = "inventory.view";
    public const string InventoryAdjust = "inventory.adjust";
    public const string InventoryManage = "inventory.manage";

    // ─── Finance ─────────────────────────────────────────────
    public const string FinanceRevenueView = "finance.revenue.view";
    public const string FinanceDepositTake = "finance.deposit.take";
    public const string FinancePayoutViewOwn = "finance.payout.view.own";
    public const string FinancePayoutManage = "finance.payout.manage";
    public const string FinancePeriodClose = "finance.period.close";

    // ─── Reports ─────────────────────────────────────────────
    public const string ReportSalonView = "report.salon.view";
    public const string ReportStaffViewOwn = "report.staff.view.own";
    public const string ReportPlatformView = "report.platform.view";

    // ─── Loyalty ─────────────────────────────────────────────
    public const string LoyaltyConfigManage = "loyalty.config.manage";
    public const string LoyaltyViewOwn = "loyalty.view.own";

    // ─── Client self-service ─────────────────────────────────
    public const string ClientSelf = "client.self";
    public const string ClientFeedbackCreate = "clientfeedback.create";

    // ─── Notification ────────────────────────────────────────
    public const string NotificationSend = "notification.send";
    public const string NotificationViewOwn = "notification.view.own";

    // ─── Marketplace ─────────────────────────────────────────
    public const string MarketplaceBrowse = "marketplace.browse";
    public const string MarketplaceLicensePurchase = "marketplace.license.purchase";
    public const string MarketplaceTemplateManage = "marketplace.template.manage";

    // ─── JobSeeker / Job market ──────────────────────────────
    public const string JobSeekerProfileManage = "jobseeker.profile.manage";
    public const string JobPostingView         = "job.posting.view";
    public const string JobPostingManage       = "job.posting.manage";   // SalonManager
    public const string JobApplicationCreate   = "job.application.create";

    // ─── Artist self-service ─────────────────────────────────
    public const string LeaveRequestOwn        = "leave.request.own";
    public const string AppointmentCheckIn     = "appointment.checkin";
    public const string RescheduleRequestCreate = "reschedule.request.create";
    public const string ClientNoteManageOwn    = "clientnote.manage.own";
    public const string ProductUsageRecord     = "productusage.record";

    // ─── Platform / Tenant ──────────────────────────────────
    public const string TenantManage = "tenant.manage";
    public const string TenantBillingManage = "tenant.billing.manage";
    public const string PlatformConfigManage = "platform.config.manage";
    public const string PlatformAuditView = "platform.audit.view";
}
