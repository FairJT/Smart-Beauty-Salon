import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../domain/entities/salon_entity.dart';
import '../../domain/repositories/salon_repository.dart';
import '../../data/repositories/salon_repository_impl.dart';

class SalonListState {
  final List<SalonEntity> salons;
  final bool loading;
  final String? error;
  final String searchQuery;
  final bool vipOnly;
  final String serviceFilter;

  SalonListState({
    this.salons = const [],
    this.loading = true,
    this.error,
    this.searchQuery = '',
    this.vipOnly = false,
    this.serviceFilter = '',
  });

  SalonListState copyWith({
    List<SalonEntity>? salons,
    bool? loading,
    String? error,
    String? searchQuery,
    bool? vipOnly,
    String? serviceFilter,
  }) {
    return SalonListState(
      salons: salons ?? this.salons,
      loading: loading ?? this.loading,
      error: error,
      searchQuery: searchQuery ?? this.searchQuery,
      vipOnly: vipOnly ?? this.vipOnly,
      serviceFilter: serviceFilter ?? this.serviceFilter,
    );
  }
}

class SalonListNotifier extends StateNotifier<SalonListState> {
  final SalonRepository _salonRepository;

  SalonListNotifier(this._salonRepository) : super(SalonListState()) {
    load();
  }

  Future<void> load({String? search}) async {
    state = state.copyWith(loading: true, error: null);
    try {
      final salons = await _salonRepository.getSalons(searchQuery: search);
      state = state.copyWith(salons: salons, loading: false);
    } catch (e) {
      state = state.copyWith(
        error: e.toString().replaceAll('Exception: ', ''),
        loading: false,
      );
    }
  }

  void setSearch(String query) {
    state = state.copyWith(searchQuery: query);
    load(search: query);
  }

  void toggleVip() {
    state = state.copyWith(vipOnly: !state.vipOnly);
  }

  void setServiceFilter(String service) {
    state = state.copyWith(serviceFilter: service);
  }
}

final salonRepositoryProvider = Provider<SalonRepository>((ref) {
  return SalonRepositoryImpl();
});

final salonListProvider =
    StateNotifierProvider<SalonListNotifier, SalonListState>((ref) {
  final salonRepository = ref.watch(salonRepositoryProvider);
  return SalonListNotifier(salonRepository);
});
