import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../core/api_constants.dart';
import '../core/api_service.dart';
import '../models/user_profile.dart';

class AuthState {
  final bool isLoggedIn;
  final UserProfile? profile;
  final bool loading;

  AuthState({this.isLoggedIn = false, this.profile, this.loading = true});

  AuthState copyWith({bool? isLoggedIn, UserProfile? profile, bool? loading}) {
    return AuthState(
      isLoggedIn: isLoggedIn ?? this.isLoggedIn,
      profile: profile ?? this.profile,
      loading: loading ?? this.loading,
    );
  }
}

class AuthNotifier extends StateNotifier<AuthState> {
  AuthNotifier() : super(AuthState()) {
    _init();
  }

  Future<void> _init() async {
    final token = await ApiService.getToken();
    if (token != null) {
      try {
        final res = await ApiService.get(ApiConstants.profile);
        state = AuthState(
          isLoggedIn: true,
          profile: UserProfile.fromJson(res),
          loading: false,
        );
      } catch (_) {
        await ApiService.clearToken();
        state = AuthState(loading: false);
      }
    } else {
      state = AuthState(loading: false);
    }
  }

  Future<bool> login(String mobile, String password) async {
    final res = await ApiService.post(ApiConstants.login, {
      'mobile': mobile,
      'password': password,
    });
    await ApiService.saveToken(res['token']);
    state = AuthState(
      isLoggedIn: true,
      profile: UserProfile.fromJson(res['user']),
      loading: false,
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
    await ApiService.saveToken(res['token']);
    state = AuthState(
      isLoggedIn: true,
      profile: UserProfile.fromJson(res['user']),
      loading: false,
    );
    return true;
  }

  Future<void> logout() async {
    await ApiService.clearToken();
    state = AuthState(loading: false);
  }

  Future<void> refreshProfile() async {
    try {
      final res = await ApiService.get(ApiConstants.profile);
      state = state.copyWith(profile: UserProfile.fromJson(res));
    } catch (_) {}
  }
}

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier();
});
