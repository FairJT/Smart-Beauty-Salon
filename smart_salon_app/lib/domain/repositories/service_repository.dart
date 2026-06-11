import '../entities/service_entity.dart';

abstract class ServiceRepository {
  Future<List<ServiceEntity>> getServicesBySalon(int salonId);
  Future<ServiceEntity> getServiceById(int id);
}