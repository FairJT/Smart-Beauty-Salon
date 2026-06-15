using Microsoft.AspNetCore.Authorization;
using Moq;
using SalonOS.Shared.Authorization;
using System.Security.Claims;

namespace SalonOS.Tenancy.Tests;

public class AuthEnforcementTests
{
    // ── helpers ───────────────────────────────────────────────────────

    private static ClaimsPrincipal MakePrincipal(string role, params string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new("role", role),
        };
        foreach (var p in permissions)
            claims.Add(new Claim("permission", p));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    private static PermissionHandler CreateHandler() => new();

    private static async Task<bool> EvaluatePermission(string permission, ClaimsPrincipal user)
    {
        var handler = CreateHandler();
        var requirement = new PermissionRequirement(permission);
        var context = new AuthorizationHandlerContext(
            new[] { requirement }, user, null);
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    // ── PermissionHandler unit tests ──────────────────────────────────

    [Fact]
    public async Task PermissionHandler_succeeds_when_user_has_matching_claim()
    {
        var user = MakePrincipal("Client", Permissions.ClientSelf);
        var result = await EvaluatePermission(Permissions.ClientSelf, user);
        Assert.True(result);
    }

    [Fact]
    public async Task PermissionHandler_fails_when_user_lacks_permission_claim()
    {
        var user = MakePrincipal("Client");
        var result = await EvaluatePermission(Permissions.SalonSettingsManage, user);
        Assert.False(result);
    }

    [Fact]
    public async Task PermissionHandler_fails_for_wrong_permission_value()
    {
        var user = MakePrincipal("Client", Permissions.ClientSelf);
        var result = await EvaluatePermission(Permissions.FinanceRevenueView, user);
        Assert.False(result);
    }

    // ── Client role matrix ────────────────────────────────────────────

    [Fact]
    public void Client_role_holds_self_service_permissions()
    {
        var perms = RolePermissions.Map["Client"];
        Assert.Contains(Permissions.ClientSelf, perms);
        Assert.Contains(Permissions.AppointmentViewOwn, perms);
        Assert.Contains(Permissions.AppointmentCreate, perms);
        Assert.Contains(Permissions.AppointmentCancelOwn, perms);
        Assert.Contains(Permissions.AppointmentRate, perms);
        Assert.Contains(Permissions.LoyaltyViewOwn, perms);
    }

    [Fact]
    public void Client_role_does_not_hold_staff_or_finance_permissions()
    {
        var perms = RolePermissions.Map["Client"];
        Assert.DoesNotContain(Permissions.StaffView, perms);
        Assert.DoesNotContain(Permissions.StaffEdit, perms);
        Assert.DoesNotContain(Permissions.FinanceRevenueView, perms);
        Assert.DoesNotContain(Permissions.FinanceDepositTake, perms);
        Assert.DoesNotContain(Permissions.InventoryView, perms);
        Assert.DoesNotContain(Permissions.InventoryManage, perms);
        Assert.DoesNotContain(Permissions.SalonSettingsManage, perms);
        Assert.DoesNotContain(Permissions.SalonEdit, perms);
    }

    [Fact]
    public void Client_role_does_not_hold_catalog_or_staff_permissions()
    {
        var perms = RolePermissions.Map["Client"];
        Assert.DoesNotContain(Permissions.CatalogView, perms);
        Assert.DoesNotContain(Permissions.CatalogCreate, perms);
        Assert.DoesNotContain(Permissions.CatalogEdit, perms);
        Assert.DoesNotContain(Permissions.StaffCreate, perms);
        Assert.DoesNotContain(Permissions.StaffDelete, perms);
    }

    // ── Artist role matrix ────────────────────────────────────────────

    [Fact]
    public void Artist_role_holds_appointment_self_service_permissions()
    {
        var perms = RolePermissions.Map["Artist"];
        Assert.Contains(Permissions.AppointmentViewOwn, perms);
        Assert.Contains(Permissions.AppointmentConfirm, perms);
        Assert.Contains(Permissions.AppointmentComplete, perms);
        Assert.Contains(Permissions.AppointmentCancelOwn, perms);
    }

    [Fact]
    public void Artist_role_does_not_hold_all_scoped_operations()
    {
        var perms = RolePermissions.Map["Artist"];
        Assert.DoesNotContain(Permissions.AppointmentCancelAll, perms);
        Assert.DoesNotContain(Permissions.AppointmentViewAll, perms);
        Assert.DoesNotContain(Permissions.FinanceRevenueView, perms);
        Assert.DoesNotContain(Permissions.FinanceDepositTake, perms);
        Assert.DoesNotContain(Permissions.InventoryManage, perms);
        Assert.DoesNotContain(Permissions.StaffCreate, perms);
        Assert.DoesNotContain(Permissions.StaffDelete, perms);
    }

    [Fact]
    public void Artist_role_holds_catalog_and_performance_view()
    {
        var perms = RolePermissions.Map["Artist"];
        Assert.Contains(Permissions.CatalogView, perms);
        Assert.Contains(Permissions.StaffPerformanceView, perms);
        Assert.Contains(Permissions.ReportStaffViewOwn, perms);
        Assert.Contains(Permissions.FinancePayoutViewOwn, perms);
    }

    // ── Receptionist role matrix ──────────────────────────────────────

    [Fact]
    public void Receptionist_role_holds_booking_and_deposit_permissions()
    {
        var perms = RolePermissions.Map["Receptionist"];
        Assert.Contains(Permissions.AppointmentCreate, perms);
        Assert.Contains(Permissions.AppointmentConfirm, perms);
        Assert.Contains(Permissions.AppointmentComplete, perms);
        Assert.Contains(Permissions.AppointmentCancelAll, perms);
        Assert.Contains(Permissions.FinanceDepositTake, perms);
    }

    [Fact]
    public void Receptionist_role_does_not_hold_finance_or_management_permissions()
    {
        var perms = RolePermissions.Map["Receptionist"];
        Assert.DoesNotContain(Permissions.FinanceRevenueView, perms);
        Assert.DoesNotContain(Permissions.FinancePeriodClose, perms);
        Assert.DoesNotContain(Permissions.FinancePayoutManage, perms);
        Assert.DoesNotContain(Permissions.SalonSettingsManage, perms);
        Assert.DoesNotContain(Permissions.StaffContractManage, perms);
        Assert.DoesNotContain(Permissions.InventoryManage, perms);
        Assert.DoesNotContain(Permissions.CatalogCreate, perms);
        Assert.DoesNotContain(Permissions.CatalogEdit, perms);
    }

    // ── SalonManager role matrix ──────────────────────────────────────

    [Fact]
    public void Manager_role_holds_all_salon_management_permissions()
    {
        var perms = RolePermissions.Map["SalonManager"];
        Assert.Contains(Permissions.SalonView, perms);
        Assert.Contains(Permissions.SalonEdit, perms);
        Assert.Contains(Permissions.SalonSettingsManage, perms);
        Assert.Contains(Permissions.StaffView, perms);
        Assert.Contains(Permissions.StaffCreate, perms);
        Assert.Contains(Permissions.StaffEdit, perms);
        Assert.Contains(Permissions.StaffDelete, perms);
        Assert.Contains(Permissions.StaffContractManage, perms);
    }

    [Fact]
    public void Manager_role_holds_all_catalog_and_appointment_permissions()
    {
        var perms = RolePermissions.Map["SalonManager"];
        Assert.Contains(Permissions.CatalogView, perms);
        Assert.Contains(Permissions.CatalogCreate, perms);
        Assert.Contains(Permissions.CatalogEdit, perms);
        Assert.Contains(Permissions.CatalogDelete, perms);
        Assert.Contains(Permissions.AppointmentViewAll, perms);
        Assert.Contains(Permissions.AppointmentConfirm, perms);
        Assert.Contains(Permissions.AppointmentComplete, perms);
        Assert.Contains(Permissions.AppointmentCancelAll, perms);
    }

    [Fact]
    public void Manager_role_holds_finance_and_inventory_permissions()
    {
        var perms = RolePermissions.Map["SalonManager"];
        Assert.Contains(Permissions.InventoryView, perms);
        Assert.Contains(Permissions.InventoryAdjust, perms);
        Assert.Contains(Permissions.InventoryManage, perms);
        Assert.Contains(Permissions.FinanceRevenueView, perms);
        Assert.Contains(Permissions.FinanceDepositTake, perms);
        Assert.Contains(Permissions.FinancePayoutManage, perms);
        Assert.Contains(Permissions.FinancePeriodClose, perms);
        Assert.Contains(Permissions.ReportSalonView, perms);
    }

    [Fact]
    public void Manager_role_does_not_hold_platform_permissions()
    {
        var perms = RolePermissions.Map["SalonManager"];
        Assert.DoesNotContain(Permissions.TenantManage, perms);
        Assert.DoesNotContain(Permissions.TenantBillingManage, perms);
        Assert.DoesNotContain(Permissions.PlatformConfigManage, perms);
        Assert.DoesNotContain(Permissions.PlatformAuditView, perms);
        Assert.DoesNotContain(Permissions.MarketplaceTemplateManage, perms);
    }

    // ── Cross-role boundary tests ─────────────────────────────────────

    [Fact]
    public void Client_role_has_no_overlap_with_platform_permissions()
    {
        var perms = RolePermissions.Map["Client"];
        var platformPerms = new[]
        {
            Permissions.TenantManage,
            Permissions.TenantBillingManage,
            Permissions.PlatformConfigManage,
            Permissions.PlatformAuditView,
            Permissions.MarketplaceTemplateManage,
        };
        Assert.DoesNotContain(perms, p => platformPerms.Contains(p));
    }

    [Fact]
    public void No_role_has_empty_permission_set()
    {
        foreach (var kv in RolePermissions.Map)
            Assert.NotEmpty(kv.Value);
    }

    [Fact]
    public void Non_client_roles_allow_catalog_or_salon_view()
    {
        var nonClientRoles = RolePermissions.Map
            .Where(kv => kv.Key != "Client")
            .ToList();
        foreach (var kv in nonClientRoles)
        {
            var hasViewPerm = kv.Value.Contains(Permissions.CatalogView)
                           || kv.Value.Contains(Permissions.SalonView);
            Assert.True(hasViewPerm, $"Role '{kv.Key}' lacks any view permission");
        }
    }

    [Fact]
    public void No_duplicate_permissions_within_a_role()
    {
        foreach (var kv in RolePermissions.Map)
        {
            Assert.Equal(kv.Value.Length, kv.Value.Distinct().Count());
        }
    }

    [Fact]
    public void ClientSelf_not_assigned_to_non_client_roles()
    {
        var nonClientRoles = new[] { "SalonManager", "Receptionist", "Artist" };
        foreach (var role in nonClientRoles)
        {
            var perms = RolePermissions.Map[role];
            Assert.DoesNotContain(Permissions.ClientSelf, perms);
        }
    }

    // ── Unauthenticated access tests ──────────────────────────────────

    [Fact]
    public async Task PermissionHandler_fails_for_anonymous_user()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var result = await EvaluatePermission(Permissions.ClientSelf, anonymous);
        Assert.False(result);
    }

    [Fact]
    public async Task PermissionHandler_fails_for_user_without_role_claim()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-id") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        var result = await EvaluatePermission(Permissions.AppointmentViewOwn, user);
        Assert.False(result);
    }
}
