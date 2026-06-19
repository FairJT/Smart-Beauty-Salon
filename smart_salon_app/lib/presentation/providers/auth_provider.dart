import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../domain/entities/user_entity.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../data/repositories/auth_repository_impl.dart';

class AuthState {
  final bool isLoggedIn;
  final UserEntity? user;
  final bool loading;

  AuthState({this.isLoggedIn = false, this.user, this.loading = true});

  AuthState copyWith({bool? isLoggedIn, UserEntity? user, bool? loading}) {
    return AuthState(
      isLoggedIn: isLoggedIn ?? this.isLoggedIn,
      user: user ?? this.user,
      loading: loading ?? this.loading,
    );
  }

  bool get isSuperAdmin => user?.isSuperAdmin ?? false;
  bool get isSalonManager => user?.isSalonManager ?? false;
  bool get isArtist => user?.isArtist ?? false;
  bool get isClient => user?.userType == 'Client';
}

class AuthNotifier extends StateNotifier<AuthState> {
  final AuthRepository _authRepository;

  AuthNotifier(this._authRepository) : super(AuthState()) {
    _init();
  }

  Future<void> _init() async {
    final isLoggedIn = await _authRepository.isLoggedIn();
    if (isLoggedIn) {
      try {
        final user = await _authRepository.getProfile();
        state = AuthState(
          isLoggedIn: true,
          user: user,
          loading: false,
        );
      } catch (_) {
        await _authRepository.logout();
        state = AuthState(loading: false);
      }
    } else {
      state = AuthState(loading: false);
    }
  }

  Future<bool> login(String phoneNumber, String password) async {
    final user = await _authRepository.login(phoneNumber, password);
    state = AuthState(
      isLoggedIn: true,
      user: user,
      loading: false,
    );
    return true;
  }

  Future<bool> register({
    String? mobile,
    required String password,
    required String firstName,
    required String lastName,
    String? nationalCode,
  }) async {
    final user = await _authRepository.register(
        mobile ?? '', password, firstName, lastName);
    state = AuthState(
      isLoggedIn: true,
      user: user,
      loading: false,
    );
    return true;
  }

  Future<void> logout() async {
    await _authRepository.logout();
    state = AuthState(loading: false);
  }

  Future<void> refreshProfile() async {
    try {
      final user = await _authRepository.getProfile();
      state = state.copyWith(user: user);
    } catch (_) {}
  }
}

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepositoryImpl();
});

final authProvider = StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  final authRepository = ref.watch(authRepositoryProvider);
  return AuthNotifier(authRepository);
});
