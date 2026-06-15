import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/datasources/dio_client.dart';
import '../../data/datasources/api_constants.dart';
import '../../domain/entities/appointment_entity.dart';
import '../../types.dart';

class ArtistScheduleState {
  final List<AppointmentEntity> appointments;
  final Map<String, dynamic>? stats;
  final bool loading;
  final String? error;

  ArtistScheduleState({
    this.appointments = const [],
    this.stats,
    this.loading = false,
    this.error,
  });

  ArtistScheduleState copyWith({
    List<AppointmentEntity>? appointments,
    Map<String, dynamic>? stats,
    bool? loading,
    String? error,
  }) =>
      ArtistScheduleState(
        appointments: appointments ?? this.appointments,
        stats: stats ?? this.stats,
        loading: loading ?? this.loading,
        error: error,
      );
}

class ArtistScheduleNotifier extends StateNotifier<ArtistScheduleState> {
  ArtistScheduleNotifier() : super(ArtistScheduleState()) {
    load();
  }

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final response = await DioClient.instance.get(ApiConstants.artistSchedule);
      final data = response.data as List;
      state = state.copyWith(
        appointments: data
            .map((j) => AppointmentEntity(
                  id: j['id']?.toString() ?? '',
                  startTime: DateTime.parse(j['startTime']),
                  endTime: DateTime.parse(j['endTime']),
                  status: j['status'] ?? 0,
                  estimatedPrice: (j['estimatedPrice'] ?? 0).toDouble(),
                  depositAmount: (j['depositAmount'] ?? 0).toDouble(),
                  isRated: j['isRated'] ?? false,
                  rating: j['rating'] ?? 0,
                  comment: j['comment'],
                  salonName: j['salonName'],
                  artistName: j['artistName'],
                  serviceName: j['serviceName'],
                ))
            .toList(),
        loading: false,
      );
    } catch (e) {
      state = state.copyWith(error: e.toString(), loading: false);
    }
  }

  Future<void> loadStats() async {
    try {
      final response =
          await DioClient.instance.get(ApiConstants.artistScheduleStats);
      state = state.copyWith(stats: response.data);
    } catch (e) {
      state = state.copyWith(error: e.toString());
    }
  }
}

final artistScheduleProvider =
    StateNotifierProvider<ArtistScheduleNotifier, ArtistScheduleState>((ref) {
  return ArtistScheduleNotifier();
});
