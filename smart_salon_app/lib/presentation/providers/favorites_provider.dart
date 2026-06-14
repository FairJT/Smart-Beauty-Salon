import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api_service.dart';
import '../../core/api_constants.dart';
import '../../models/dashboard_models.dart';

class FavoritesState {
  final List<FavoriteSalon> favorites;
  final Set<int> favoriteIds;
  final bool loading;
  final String? error;

  const FavoritesState({
    this.favorites = const [],
    this.favoriteIds = const {},
    this.loading = true,
    this.error,
  });

  FavoritesState copyWith({
    List<FavoriteSalon>? favorites,
    Set<int>? favoriteIds,
    bool? loading,
    String? error,
  }) {
    return FavoritesState(
      favorites: favorites ?? this.favorites,
      favoriteIds: favoriteIds ?? this.favoriteIds,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class FavoritesNotifier extends StateNotifier<FavoritesState> {
  FavoritesNotifier() : super(const FavoritesState());

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final json = await ApiService.get(ApiConstants.favorites);
      final list = (json as List<dynamic>)
          .map((j) => FavoriteSalon.fromJson(j as Map<String, dynamic>))
          .toList();
      state = FavoritesState(
        favorites: list,
        favoriteIds: list.map((f) => f.salonId).toSet(),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }

  Future<bool> add(int salonId, String salonName, {String? logoUrl}) async {
    try {
      await ApiService.post('${ApiConstants.favorites}/$salonId', {
        'salonName': salonName,
        if (logoUrl != null) 'logoUrl': logoUrl,
      });
      await load();
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> remove(int salonId) async {
    try {
      await ApiService.delete('${ApiConstants.favorites}/$salonId');
      await load();
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> toggle(int salonId, String salonName, {String? logoUrl}) async {
    if (state.favoriteIds.contains(salonId)) {
      return remove(salonId);
    } else {
      return add(salonId, salonName, logoUrl: logoUrl);
    }
  }

  bool isFavorite(int salonId) => state.favoriteIds.contains(salonId);
}

final favoritesProvider =
    StateNotifierProvider<FavoritesNotifier, FavoritesState>((ref) {
  return FavoritesNotifier();
});
