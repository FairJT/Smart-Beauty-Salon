/// Permission string constants — mirrors the server-side Permissions.cs (§R3).
/// Keep in sync whenever server permissions change.
///
/// §R8: client-side gating is UX only — the server enforces every rule.
/// These constants are used only to hide tabs/buttons the user lacks.
/// Never move real authorization logic to the client.
library;

class AppPermissions {
  AppPermissions._();

  // Salon
  static const salonView           = 'salon.view';
  static const salonEdit           = 'salon.edit';
  static const salonSettingsManage = 'salon.settings.manage';

  // Staff
  static const staffView            = 'staff.view';
  static const staffCreate          = 'staff.create';
  static const staffEdit            = 'staff.edit';
  static const staffDelete          = 'staff.delete';
  static const staffContractManage  = 'staff.contract.manage';
  static const staffPerformanceView = 'staff.performance.view';

  // Catalog
  static const catalogView          = 'catalog.view';
  static const catalogCreate        = 'catalog.create';
  static const catalogEdit          = 'catalog.edit';
  static const catalogDelete        = 'catalog.delete';
  static const catalogPackageManage = 'catalog.package.manage';

  // Appointment
  static const appointmentViewAll   = 'appointment.view.all';
  static const appointmentViewOwn   = 'appointment.view.own';
  static const appointmentCreate    = 'appointment.create';
  static const appointmentConfirm   = 'appointment.confirm';
  static const appointmentComplete  = 'appointment.complete';
  static const appointmentCancelAll = 'appointment.cancel.all';
  static const appointmentCancelOwn = 'appointment.cancel.own';
  static const appointmentRate      = 'appointment.rate';

  // Inventory
  static const inventoryView   = 'inventory.view';
  static const inventoryAdjust = 'inventory.adjust';
  static const inventoryManage = 'inventory.manage';

  // Finance
  static const financeRevenueView   = 'finance.revenue.view';
  static const financeDepositTake   = 'finance.deposit.take';
  static const financePayoutViewOwn = 'finance.payout.view.own';
  static const financePayoutManage  = 'finance.payout.manage';
  static const financePeriodClose   = 'finance.period.close';

  // Reports
  static const reportSalonView     = 'report.salon.view';
  static const reportStaffViewOwn  = 'report.staff.view.own';
  static const reportPlatformView  = 'report.platform.view';

  // Loyalty
  static const loyaltyConfigManage = 'loyalty.config.manage';
  static const loyaltyViewOwn      = 'loyalty.view.own';

  // Notification
  static const notificationSend    = 'notification.send';
  static const notificationViewOwn = 'notification.view.own';

  // Marketplace
  static const marketplaceBrowse          = 'marketplace.browse';
  static const marketplaceLicensePurchase = 'marketplace.license.purchase';
  static const marketplaceTemplateManage  = 'marketplace.template.manage';

  // Platform / Tenant
  static const tenantManage         = 'tenant.manage';
  static const tenantBillingManage  = 'tenant.billing.manage';
  static const platformConfigManage = 'platform.config.manage';
  static const platformAuditView    = 'platform.audit.view';
}

/// Lightweight permission checker injected via Riverpod.
/// Reads the permission set decoded from the JWT after login.
///
/// §R8: THIS IS UX GATING ONLY. The server enforces every rule independently.
class PermissionService {
  final Set<String> _permissions;

  const PermissionService(Set<String> permissions) : _permissions = permissions;

  /// Returns true if the current user holds [permission].
  bool can(String permission) => _permissions.contains(permission);

  /// Returns true if the current user holds ALL of [permissions].
  bool canAll(List<String> permissions) =>
      permissions.every(_permissions.contains);

  /// Returns true if the current user holds ANY of [permissions].
  bool canAny(List<String> permissions) =>
      permissions.any(_permissions.contains);

  static PermissionService empty() => const PermissionService({});
}
