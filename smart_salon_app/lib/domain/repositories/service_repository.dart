import '../../types.dart';
import '../entities/service_entity.dart';

abstract class ServiceRepository {
  Future<List<ServiceEntity>> getServicesBySalon(String slug);
  Future<ServiceEntity> getServiceById(ServiceId id);
}
