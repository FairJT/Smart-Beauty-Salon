import '../entities/appointment_entity.dart';
import '../entities/slot_entity.dart';

abstract class AppointmentRepository {
  Future<List<AppointmentEntity>> getMyAppointments();
  Future<AppointmentEntity> getAppointmentById(int id);
  Future<AppointmentEntity> createAppointment(CreateAppointmentInput input);
  Future<AppointmentEntity> cancelAppointment(int id);
  Future<List<SlotEntity>> getAvailableSlots(int artistId, int serviceId, DateTime date);
}