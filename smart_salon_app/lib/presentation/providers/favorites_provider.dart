import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/api_service.dart';
import '../../core/api_constants.dart';
import '../../models/dashboard_models.dart';

class FavoritesState {
  final List<FavoriteSalon> favorites;
  final Set<String> favoriteSlugs;
  final bool loading;
  final String? error;

  const FavoritesState({
    this.favorites = const [],
    this.favoriteSlugs = const {},
    this.loading = true,
    this.error,
  });

  FavoritesState copyWith({
    List<FavoriteSalon>? favorites,
    Set<String>? favoriteSlugs,
    bool? loading,
    String? error,
  }) {
    return FavoritesState(
      favorites: favorites ?? this.favorites,
      favoriteSlugs: favoriteSlugs ?? this.favoriteSlugs,
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
        favoriteSlugs: list.map((f) => f.slug).toSet(),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(loading: false, error: e.toString());
    }
  }

  Future<bool> add(String slug, String salonName, {String? logoUrl}) async {
    try {
      await ApiService.post('${ApiConstants.favorites}/$slug', {
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

  Future<bool> remove(String slug) async {
    try {
      await ApiService.delete('${ApiConstants.favorites}/$slug');
      await load();
      return true;
    } catch (e) {
      state = state.copyWith(error: e.toString());
      return false;
    }
  }

  Future<bool> toggle(String slug, String salonName, {String? logoUrl}) async {
    if (state.favoriteSlugs.contains(slug)) {
      return remove(slug);
    } else {
      return add(slug, salonName, logoUrl: logoUrl);
    }
  }

  bool isFavorite(String slug) => state.favoriteSlugs.contains(slug);
}

final favoritesProvider =
    StateNotifierProvider<FavoritesNotifier, FavoritesState>((ref) {
  return FavoritesNotifier();
});
