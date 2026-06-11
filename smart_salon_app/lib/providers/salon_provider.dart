import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../core/api_service.dart';
import '../core/api_constants.dart';
import '../models/salon.dart';

class SalonListState {
  final List<SalonListItem> salons;
  final bool loading;
  final String? error;
  final String searchQuery;
  final String serviceFilter;
  final bool vipOnly;

  SalonListState({
    this.salons = const [],
    this.loading = true,
    this.error,
    this.searchQuery = '',
    this.serviceFilter = '',
    this.vipOnly = false,
  });

  SalonListState copyWith({
    List<SalonListItem>? salons,
    bool? loading,
    String? error,
    String? searchQuery,
    String? serviceFilter,
    bool? vipOnly,
  }) {
    return SalonListState(
      salons: salons ?? this.salons,
      loading: loading ?? this.loading,
      error: error,
      searchQuery: searchQuery ?? this.searchQuery,
      serviceFilter: serviceFilter ?? this.serviceFilter,
      vipOnly: vipOnly ?? this.vipOnly,
    );
  }
}

class SalonListNotifier extends StateNotifier<SalonListState> {
  SalonListNotifier() : super(SalonListState()) {
    load();
  }

  Future<void> load({String? search}) async {
    state = state.copyWith(loading: true, error: null);
    try {
      var params = <String>[];
      if (search != null && search.isNotEmpty) params.add('search=$search');
      if (state.serviceFilter.isNotEmpty) params.add('service=${state.serviceFilter}');
      if (state.vipOnly) params.add('vipOnly=true');

      final url = params.isEmpty
          ? ApiConstants.salons
          : '${ApiConstants.salons}?${params.join('&')}';

      final res = await ApiService.get(url);
      final data = (res['data'] as List<dynamic>?)
              ?.map((s) => SalonListItem.fromJson(s))
              .toList() ??
          [];
      state = state.copyWith(salons: data, loading: false);
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
    load(search: state.searchQuery.isNotEmpty ? state.searchQuery : null);
  }

  void setServiceFilter(String service) {
    state = state.copyWith(serviceFilter: service);
    load(search: state.searchQuery.isNotEmpty ? state.searchQuery : null);
  }
}

final salonListProvider =
    StateNotifierProvider<SalonListNotifier, SalonListState>((ref) {
  return SalonListNotifier();
});
