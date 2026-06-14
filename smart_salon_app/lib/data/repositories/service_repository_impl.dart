import '../../domain/entities/service_entity.dart';
import '../../domain/repositories/service_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class ServiceRepositoryImpl implements ServiceRepository {
  @override
  Future<List<ServiceEntity>> getServicesBySalon(int salonId) async {
    final response = await DioClient.instance.get(
      '${ApiConstants.services}/salon/$salonId',
    );

    final data = response.data as List;
    return data.map((json) => ServiceEntity(
      id: json['id'],
      salonId: json['salonId'],
      name: json['name'],
      description: json['description'],
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 30,
      imageUrl: json['imageUrl'],
      isActive: json['isActive'] ?? true,
      templateId: json['templateId'],
    )).toList();
  }

  @override
  Future<ServiceEntity> getServiceById(int id) async {
    final response = await DioClient.instance.get('${ApiConstants.services}/$id');
    final json = response.data;

    return ServiceEntity(
      id: json['id'],
      salonId: json['salonId'],
      name: json['name'],
      description: json['description'],
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 30,
      imageUrl: json['imageUrl'],
      isActive: json['isActive'] ?? true,
      templateId: json['templateId'],
    );
  }
}