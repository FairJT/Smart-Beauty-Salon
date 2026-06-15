import '../../domain/entities/service_entity.dart';
import '../../domain/repositories/service_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';
import '../../types.dart';

class ServiceRepositoryImpl implements ServiceRepository {
  @override
  Future<List<ServiceEntity>> getServicesBySalon(String slug) async {
    final response = await DioClient.instance.get(
      '${ApiConstants.services}/salon/$slug',
    );

    final data = response.data as List;
    return data.map((json) => ServiceEntity(
      id: json['id']?.toString() ?? '',
      name: json['name'],
      description: json['description'],
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 30,
      imageUrl: json['imageUrl'],
      isActive: json['isActive'] ?? true,
      templateId: json['templateId']?.toString(),
    )).toList();
  }

  @override
  Future<ServiceEntity> getServiceById(ServiceId id) async {
    final response = await DioClient.instance.get('${ApiConstants.services}/$id');
    final json = response.data;

    return ServiceEntity(
      id: json['id']?.toString() ?? '',
      name: json['name'],
      description: json['description'],
      price: (json['price'] ?? 0).toDouble(),
      durationMinutes: json['durationMinutes'] ?? 30,
      imageUrl: json['imageUrl'],
      isActive: json['isActive'] ?? true,
      templateId: json['templateId']?.toString(),
    );
  }
}
