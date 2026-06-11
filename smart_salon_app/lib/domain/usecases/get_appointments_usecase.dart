import '../entities/appointment_entity.dart';
import '../repositories/appointment_repository.dart';

class GetAppointmentsUseCase {
  final AppointmentRepository _appointmentRepository;

  GetAppointmentsUseCase(this._appointmentRepository);

  Future<List<AppointmentEntity>> call() async {
    return await _appointmentRepository.getMyAppointments();
  }
}