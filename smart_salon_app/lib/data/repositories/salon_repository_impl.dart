import '../../domain/entities/salon_entity.dart';
import '../../domain/repositories/salon_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class SalonRepositoryImpl implements SalonRepository {
  @override
  Future<List<SalonEntity>> getSalons({String? searchQuery}) async {
    final response = await DioClient.instance.get(
      ApiConstants.salons,
      queryParameters: searchQuery != null ? {'search': searchQuery} : null,
    );

    final data = response.data as List;
    return data.map((json) => SalonEntity(
      id: json['slug']?.toString() ?? '',
      slug: json['slug'] ?? '',
      name: json['name'],
      description: json['description'],
      address: json['address'],
      phoneNumber: json['phoneNumber'],
      imageUrl: json['imageUrl'],
      latitude: (json['latitude'] ?? 0).toDouble(),
      longitude: (json['longitude'] ?? 0).toDouble(),
      rating: (json['rating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
    )).toList();
  }

  @override
  Future<SalonEntity> getSalonBySlug(String slug) async {
    final response = await DioClient.instance.get('${ApiConstants.salons}/$slug');
    final json = response.data;

    return SalonEntity(
      id: json['slug']?.toString() ?? '',
      slug: json['slug'] ?? '',
      name: json['name'],
      description: json['description'],
      address: json['address'],
      phoneNumber: json['phoneNumber'],
      imageUrl: json['imageUrl'],
      latitude: (json['latitude'] ?? 0).toDouble(),
      longitude: (json['longitude'] ?? 0).toDouble(),
      rating: (json['rating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
    );
  }

  @override
  Future<List<SalonEntity>> getNearbySalons(double latitude, double longitude, double radiusKm) async {
    final response = await DioClient.instance.get(
      '${ApiConstants.salons}/nearby',
      queryParameters: {
        'latitude': latitude,
        'longitude': longitude,
        'radius': radiusKm,
      },
    );

    final data = response.data as List;
    return data.map((json) => SalonEntity(
      id: json['slug']?.toString() ?? '',
      slug: json['slug'] ?? '',
      name: json['name'],
      description: json['description'],
      address: json['address'],
      phoneNumber: json['phoneNumber'],
      imageUrl: json['imageUrl'],
      latitude: (json['latitude'] ?? 0).toDouble(),
      longitude: (json['longitude'] ?? 0).toDouble(),
      rating: (json['rating'] ?? 0).toDouble(),
      reviewCount: json['reviewCount'] ?? 0,
    )).toList();
  }
}
