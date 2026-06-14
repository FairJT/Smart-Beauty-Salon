using SalonOS.Shared.Authorization;

namespace SalonOS.Tenancy.Tests;

/// <summary>
/// §R9 Test 4 — Contract visibility.
/// Artist financial visibility depends on contract type (§R5):
///   Salaried      → only own ratings &amp; completed count. NO revenue.
///   Chair rental  → own service revenue + own deposits.
///   Room rental   → same as chair rental + room utilisation.
///
/// The permission finance.payout.view.own is granted to ALL Artists,
/// but the service layer must gate revenue data on ContractType.
/// These tests verify the permission-bundle side and the contract-aware
/// response-shaping logic (modelled as a helper here).
/// </summary>
public class ContractVisibilityTests
{
    // ── Simulated contract types (mirrors §R5) ────────────────────────────────

    private enum ContractType { Salaried, ChairRental, RoomRental }

    /// <summary>
    /// Simulates the service-layer response shaping described in §R5.
    /// Returns whether revenue figures should be included in the response.
    /// </summary>
    private static bool ShouldIncludeRevenue(ContractType contract) =>
        contract != ContractType.Salaried;

    // ── §R9 Test 4 ────────────────────────────────────────────────────────────

    [Fact]
    public void Salaried_artist_sees_no_revenue()
    {
        Assert.False(ShouldIncludeRevenue(ContractType.Salaried),
            "A salaried artist must not see revenue figures");
    }

    [Fact]
    public void ChairRental_artist_sees_revenue()
    {
        Assert.True(ShouldIncludeRevenue(ContractType.ChairRental),
            "A chair-rental artist must see their own service revenue");
    }

    [Fact]
    public void RoomRental_artist_sees_revenue()
    {
        Assert.True(ShouldIncludeRevenue(ContractType.RoomRental),
            "A room-rental artist must see their own service revenue");
    }

    [Fact]
    public void All_artists_hold_payout_view_own_permission()
    {
        // The permission is granted to all Artists regardless of contract type.
        // Revenue vs no-revenue is a runtime shape decision, not a separate permission.
        var artistPerms = RolePermissions.Map["Artist"];
        Assert.Contains(Permissions.FinancePayoutViewOwn, artistPerms);
    }

    [Fact]
    public void SalonManager_holds_payout_manage_not_just_view()
    {
        var mgr = RolePermissions.Map["SalonManager"];
        Assert.Contains(Permissions.FinancePayoutManage, mgr);
        Assert.Contains(Permissions.FinanceRevenueView,  mgr);
    }

    [Fact]
    public void Client_does_not_hold_any_finance_permissions()
    {
        var client = RolePermissions.Map["Client"];
        Assert.DoesNotContain(Permissions.FinancePayoutViewOwn, client);
        Assert.DoesNotContain(Permissions.FinanceRevenueView,   client);
        Assert.DoesNotContain(Permissions.FinanceDepositTake,   client);
    }
}
