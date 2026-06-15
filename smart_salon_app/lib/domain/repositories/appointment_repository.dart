import '../../types.dart';
import '../entities/appointment_entity.dart';
import '../entities/slot_entity.dart';

abstract class AppointmentRepository {
  Future<List<AppointmentEntity>> getMyAppointments();
  Future<AppointmentEntity> getAppointmentById(AppointmentId id);
  Future<AppointmentEntity> createAppointment(CreateAppointmentInput input);
  Future<AppointmentEntity> cancelAppointment(AppointmentId id);
  Future<List<SlotEntity>> getAvailableSlots(ArtistId artistId, ServiceId serviceId, DateTime date);
}
