import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../core/api_constants.dart';
import '../core/api_service.dart';
import '../core/jwt_decoder.dart';
import '../core/permissions.dart';
import '../models/user_profile.dart';

/// Task 10.1 — §R7, §R8.
/// AuthState now carries the permission set decoded from the JWT.
/// Use [permissions.can('some.permission')] to gate tabs/buttons.
///
/// ⚠️  This is UX gating only. The server enforces every rule.
///    Never skip the API call because a button is hidden.
class AuthState {
  final bool isLoggedIn;
  final UserProfile? profile;
  final bool loading;
  final PermissionService permissions;
  final bool isPlatformOwner;
  final String? role;

  const AuthState({
    this.isLoggedIn = false,
    this.profile,
    this.loading = true,
    this.permissions = const PermissionService({}),
    this.isPlatformOwner = false,
    this.role,
  });

  AuthState copyWith({
    bool? isLoggedIn,
    UserProfile? profile,
    bool? loading,
    PermissionService? permissions,
    bool? isPlatformOwner,
    String? role,
  }) {
    return AuthState(
      isLoggedIn:      isLoggedIn      ?? this.isLoggedIn,
      profile:         profile         ?? this.profile,
      loading:         loading         ?? this.loading,
      permissions:     permissions     ?? this.permissions,
      isPlatformOwner: isPlatformOwner ?? this.isPlatformOwner,
      role:            role            ?? this.role,
    );
  }
}

class AuthNotifier extends StateNotifier<AuthState> {
  AuthNotifier() : super(const AuthState()) {
    _init();
  }

  Future<void> _init() async {
    final token = await ApiService.getToken();
    if (token != null) {
      try {
        final res = await ApiService.get(ApiConstants.profile);
        state = _stateFromToken(
          token: token,
          profile: UserProfile.fromJson(res),
        );
      } catch (_) {
        await ApiService.clearToken();
        state = const AuthState(loading: false);
      }
    } else {
      state = const AuthState(loading: false);
    }
  }

  Future<bool> login(String mobile, String password) async {
    final res = await ApiService.post(ApiConstants.login, {
      'mobile': mobile,
      'password': password,
    });
    final token = res['token'] as String;
    await ApiService.saveToken(token);
    state = _stateFromToken(
      token: token,
      profile: UserProfile.fromJson(res['user']),
    );
    return true;
  }

  Future<bool> register({
    required String mobile,
    required String password,
    required String firstName,
    required String lastName,
    required String nationalCode,
  }) async {
    final res = await ApiService.post(ApiConstants.register, {
      'mobile': mobile,
      'password': password,
      'firstName': firstName,
      'lastName': lastName,
      'nationalCode': nationalCode,
    });
    final token = res['token'] as String;
    await ApiService.saveToken(token);
    state = _stateFromToken(
      token: token,
      profile: UserProfile.fromJson(res['user']),
    );
    return true;
  }

  Future<void> logout() async {
    await ApiService.clearToken();
    state = const AuthState(loading: false);
  }

  Future<void> refreshProfile() async {
    try {
      final res = await ApiService.get(ApiConstants.profile);
      state = state.copyWith(profile: UserProfile.fromJson(res));
    } catch (_) {}
  }

  // ── helpers ────────────────────────────────────────────────────────────────

  /// Builds AuthState from a raw JWT — extracts permissions, role, and
  /// isPlatformOwner so the rest of the UI can gate without parsing again.
  static AuthState _stateFromToken({
    required String token,
    required UserProfile profile,
  }) {
    final perms    = JwtDecoder.extractPermissions(token);
    final isOwner  = JwtDecoder.isPlatformOwner(token);
    final roleName = JwtDecoder.role(token);

    return AuthState(
      isLoggedIn:      true,
      profile:         profile,
      loading:         false,
      permissions:     PermissionService(perms),
      isPlatformOwner: isOwner,
      role:            roleName,
    );
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier();
});

/// Convenience provider — use this to check permissions in widgets.
///
/// Example:
///   final perms = ref.watch(permissionProvider);
///   if (perms.can(AppPermissions.financeRevenueView)) { ... }
final permissionProvider = Provider<PermissionService>((ref) {
  return ref.watch(authProvider).permissions;
});
