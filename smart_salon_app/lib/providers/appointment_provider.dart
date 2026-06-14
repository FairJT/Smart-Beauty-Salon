import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../core/api_service.dart';
import '../core/api_constants.dart';
import '../models/appointment.dart';

class AppointmentListState {
  final List<AppointmentItem> appointments;
  final bool loading;
  final String? error;

  AppointmentListState({
    this.appointments = const [],
    this.loading = true,
    this.error,
  });

  AppointmentListState copyWith({
    List<AppointmentItem>? appointments,
    bool? loading,
    String? error,
  }) {
    return AppointmentListState(
      appointments: appointments ?? this.appointments,
      loading: loading ?? this.loading,
      error: error,
    );
  }
}

class AppointmentListNotifier extends StateNotifier<AppointmentListState> {
  AppointmentListNotifier() : super(AppointmentListState());

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final res = await ApiService.get(ApiConstants.myAppointments);
      final list = (res as List<dynamic>)
          .map((a) => AppointmentItem.fromJson(a))
          .toList();
      state = state.copyWith(appointments: list, loading: false);
    } catch (e) {
      state = state.copyWith(
        error: e.toString().replaceAll('Exception: ', ''),
        loading: false,
      );
    }
  }

  Future<bool> create({
    required int artistId,
    required int salonId,
    required int serviceId,
    required DateTime startTime,
    required int durationMinutes,
    required double estimatedPrice,
    String? notes,
  }) async {
    try {
      await ApiService.post(ApiConstants.appointments, {
        'artistId': artistId,
        'salonId': salonId,
        'serviceId': serviceId,
        'startTime': startTime.toIso8601String(),
        'durationMinutes': durationMinutes,
        'estimatedPrice': estimatedPrice,
        'notes': notes,
      });
      await load();
      return true;
    } catch (e) {
      state = state.copyWith(
        error: e.toString().replaceAll('Exception: ', ''),
      );
      return false;
    }
  }

  Future<void> cancel(int id) async {
    await ApiService.put('${ApiConstants.appointments}/$id/cancel', {});
    await load();
  }
}

final appointmentListProvider =
    StateNotifierProvider<AppointmentListNotifier, AppointmentListState>((ref) {
  return AppointmentListNotifier();
});
