import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api_service.dart';
import '../../core/api_constants.dart';
import '../../models/dashboard_models.dart';

// ─── SalonManager Dashboard ────────────────────────────────────

class SalonManagerDashboardState {
  final SalonManagerDashboard? data;
  final bool loading;
  final String? error;

  const SalonManagerDashboardState({this.data, this.loading = true, this.error});

  SalonManagerDashboardState copyWith({SalonManagerDashboard? data, bool? loading, String? error}) {
    return SalonManagerDashboardState(
      data: data ?? this.data,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class SalonManagerDashboardNotifier extends StateNotifier<SalonManagerDashboardState> {
  SalonManagerDashboardNotifier() : super(const SalonManagerDashboardState());

  Future<void> load([String? slug]) async {
    state = state.copyWith(loading: true, error: null);
    try {
      final json = await ApiService.get(ApiConstants.dashboardManager);
      state = SalonManagerDashboardState(
        data: SalonManagerDashboard.fromJson(json),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }
}

final salonManagerDashboardProvider =
    StateNotifierProvider<SalonManagerDashboardNotifier, SalonManagerDashboardState>((ref) {
  return SalonManagerDashboardNotifier();
});

// ─── Artist Dashboard ─────────────────────────────────────────

class ArtistDashboardState {
  final ArtistDashboard? data;
  final bool loading;
  final String? error;

  const ArtistDashboardState({this.data, this.loading = true, this.error});

  ArtistDashboardState copyWith({ArtistDashboard? data, bool? loading, String? error}) {
    return ArtistDashboardState(
      data: data ?? this.data,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class ArtistDashboardNotifier extends StateNotifier<ArtistDashboardState> {
  ArtistDashboardNotifier() : super(const ArtistDashboardState());

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final json = await ApiService.get(ApiConstants.dashboardArtist);
      state = ArtistDashboardState(
        data: ArtistDashboard.fromJson(json),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }
}

final artistDashboardProvider =
    StateNotifierProvider<ArtistDashboardNotifier, ArtistDashboardState>((ref) {
  return ArtistDashboardNotifier();
});

// ─── Client Dashboard ─────────────────────────────────────────

class ClientDashboardState {
  final ClientDashboard? data;
  final bool loading;
  final String? error;

  const ClientDashboardState({this.data, this.loading = true, this.error});

  ClientDashboardState copyWith({ClientDashboard? data, bool? loading, String? error}) {
    return ClientDashboardState(
      data: data ?? this.data,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class ClientDashboardNotifier extends StateNotifier<ClientDashboardState> {
  ClientDashboardNotifier() : super(const ClientDashboardState());

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final json = await ApiService.get(ApiConstants.dashboardClient);
      state = ClientDashboardState(
        data: ClientDashboard.fromJson(json),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }
}

final clientDashboardProvider =
    StateNotifierProvider<ClientDashboardNotifier, ClientDashboardState>((ref) {
  return ClientDashboardNotifier();
});

// ─── SuperAdmin / Platform Dashboard ───────────────────────────

class SuperAdminDashboardState {
  final SuperAdminDashboard? data;
  final bool loading;
  final String? error;

  const SuperAdminDashboardState({this.data, this.loading = true, this.error});

  SuperAdminDashboardState copyWith({SuperAdminDashboard? data, bool? loading, String? error}) {
    return SuperAdminDashboardState(
      data: data ?? this.data,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class SuperAdminDashboardNotifier extends StateNotifier<SuperAdminDashboardState> {
  SuperAdminDashboardNotifier() : super(const SuperAdminDashboardState());

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final json = await ApiService.get(ApiConstants.dashboardPlatform);
      state = SuperAdminDashboardState(
        data: SuperAdminDashboard.fromJson(json),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }
}

final superAdminDashboardProvider =
    StateNotifierProvider<SuperAdminDashboardNotifier, SuperAdminDashboardState>((ref) {
  return SuperAdminDashboardNotifier();
});
