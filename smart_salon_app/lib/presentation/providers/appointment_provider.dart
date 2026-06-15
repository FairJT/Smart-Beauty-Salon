import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../domain/entities/appointment_entity.dart';
import '../../domain/repositories/appointment_repository.dart';
import '../../data/repositories/appointment_repository_impl.dart';
import '../../types.dart';

class AppointmentListState {
  final List<AppointmentEntity> appointments;
  final bool loading;
  final String? error;

  AppointmentListState({
    this.appointments = const [],
    this.loading = true,
    this.error,
  });

  AppointmentListState copyWith({
    List<AppointmentEntity>? appointments,
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
  final AppointmentRepository _appointmentRepository;

  AppointmentListNotifier(this._appointmentRepository) : super(AppointmentListState()) {
    load();
  }

  Future<void> load() async {
    state = state.copyWith(loading: true, error: null);
    try {
      final appointments = await _appointmentRepository.getMyAppointments();
      state = state.copyWith(appointments: appointments, loading: false);
    } catch (e) {
      state = state.copyWith(
        error: e.toString().replaceAll('Exception: ', ''),
        loading: false,
      );
    }
  }

  Future<bool> create({
    required String slug,
    required ArtistId artistId,
    required ServiceId serviceId,
    required DateTime startTime,
    required int durationMinutes,
    double estimatedPrice = 0,
  }) async {
    try {
      final endTime = startTime.add(Duration(minutes: durationMinutes));
      await _appointmentRepository.createAppointment(CreateAppointmentInput(
        slug: slug,
        artistId: artistId,
        serviceId: serviceId,
        startTime: startTime,
        endTime: endTime,
      ));
      await load();
      return true;
    } catch (e) {
      state = state.copyWith(
        error: e.toString().replaceAll('Exception: ', ''),
      );
      return false;
    }
  }

  Future<void> cancel(AppointmentId id) async {
    await _appointmentRepository.cancelAppointment(id);
    await load();
  }
}

final appointmentRepositoryProvider = Provider<AppointmentRepository>((ref) {
  return AppointmentRepositoryImpl();
});

final appointmentListProvider =
    StateNotifierProvider<AppointmentListNotifier, AppointmentListState>((ref) {
  final appointmentRepository = ref.watch(appointmentRepositoryProvider);
  return AppointmentListNotifier(appointmentRepository);
});
