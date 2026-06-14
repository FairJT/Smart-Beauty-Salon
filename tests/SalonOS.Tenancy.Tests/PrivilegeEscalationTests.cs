using SalonOS.Shared.Authorization;

namespace SalonOS.Tenancy.Tests;

/// <summary>
/// §R9 Test 3 — Privilege escalation.
/// A Receptionist must not hold admin-level permissions.
/// Verified directly against RolePermissions.Map — no HTTP needed.
/// </summary>
public class PrivilegeEscalationTests
{
    private static string[] ReceptionistPerms =>
        RolePermissions.Map["Receptionist"];

    // Permissions a Receptionist must NEVER hold
    private static readonly string[] Forbidden =
    {
        Permissions.FinanceRevenueView,
        Permissions.FinancePeriodClose,
        Permissions.FinancePayoutManage,
        Permissions.SalonSettingsManage,
        Permissions.StaffContractManage,
        Permissions.InventoryManage,
        Permissions.TenantManage,
        Permissions.TenantBillingManage,
        Permissions.PlatformConfigManage,
        Permissions.PlatformAuditView,
        Permissions.MarketplaceTemplateManage,
    };

    [Fact]
    public void Receptionist_does_not_hold_finance_revenue_view()
        => Assert.DoesNotContain(Permissions.FinanceRevenueView, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_finance_period_close()
        => Assert.DoesNotContain(Permissions.FinancePeriodClose, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_salon_settings_manage()
        => Assert.DoesNotContain(Permissions.SalonSettingsManage, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_staff_contract_manage()
        => Assert.DoesNotContain(Permissions.StaffContractManage, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_inventory_manage()
        => Assert.DoesNotContain(Permissions.InventoryManage, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_tenant_manage()
        => Assert.DoesNotContain(Permissions.TenantManage, ReceptionistPerms);

    [Fact]
    public void Receptionist_does_not_hold_platform_config_manage()
        => Assert.DoesNotContain(Permissions.PlatformConfigManage, ReceptionistPerms);

    [Fact]
    public void Receptionist_holds_booking_and_deposit_permissions()
    {
        Assert.Contains(Permissions.AppointmentCreate,    ReceptionistPerms);
        Assert.Contains(Permissions.AppointmentConfirm,   ReceptionistPerms);
        Assert.Contains(Permissions.AppointmentComplete,  ReceptionistPerms);
        Assert.Contains(Permissions.AppointmentCancelAll, ReceptionistPerms);
        Assert.Contains(Permissions.FinanceDepositTake,   ReceptionistPerms);
    }

    [Fact]
    public void No_forbidden_permission_appears_in_Receptionist_bundle()
    {
        var violations = Forbidden
            .Where(p => ReceptionistPerms.Contains(p))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Artist_does_not_hold_all_scoped_cancel()
        => Assert.DoesNotContain(Permissions.AppointmentCancelAll,
               RolePermissions.Map["Artist"]);

    [Fact]
    public void Client_does_not_hold_staff_or_finance_permissions()
    {
        var clientPerms = RolePermissions.Map["Client"];
        Assert.DoesNotContain(Permissions.StaffView,          clientPerms);
        Assert.DoesNotContain(Permissions.FinanceRevenueView, clientPerms);
        Assert.DoesNotContain(Permissions.InventoryView,      clientPerms);
    }
}
