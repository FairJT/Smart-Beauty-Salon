import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/auth_provider.dart';

/// Hides [child] when the current user does not hold [permission].
///
/// §R8: This is purely a UX improvement — dead buttons avoided.
/// The API will still return 403 for any action the server disallows.
/// Never use this as a security boundary.
///
/// Usage:
///   PermissionGate(
///     permission: AppPermissions.financeRevenueView,
///     child: RevenueCard(),
///   )
class PermissionGate extends ConsumerWidget {
  final String permission;
  final Widget child;

  /// Optional widget shown when permission is absent (defaults to SizedBox.shrink).
  final Widget? fallback;

  const PermissionGate({
    super.key,
    required this.permission,
    required this.child,
    this.fallback,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final perms = ref.watch(permissionProvider);
    if (perms.can(permission)) return child;
    return fallback ?? const SizedBox.shrink();
  }
}

/// Same as [PermissionGate] but requires ALL listed permissions.
class PermissionGateAll extends ConsumerWidget {
  final List<String> permissions;
  final Widget child;
  final Widget? fallback;

  const PermissionGateAll({
    super.key,
    required this.permissions,
    required this.child,
    this.fallback,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final perms = ref.watch(permissionProvider);
    if (perms.canAll(permissions)) return child;
    return fallback ?? const SizedBox.shrink();
  }
}

/// Same as [PermissionGate] but requires ANY of the listed permissions.
class PermissionGateAny extends ConsumerWidget {
  final List<String> permissions;
  final Widget child;
  final Widget? fallback;

  const PermissionGateAny({
    super.key,
    required this.permissions,
    required this.child,
    this.fallback,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final perms = ref.watch(permissionProvider);
    if (perms.canAny(permissions)) return child;
    return fallback ?? const SizedBox.shrink();
  }
}
