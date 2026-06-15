import '../../domain/entities/artist_entity.dart';
import '../../domain/repositories/artist_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';
import '../../types.dart';

class ArtistRepositoryImpl implements ArtistRepository {
  @override
  Future<List<ArtistEntity>> getArtistsBySalon(String slug) async {
    final response = await DioClient.instance.get(
      '${ApiConstants.artists}/salon/$slug',
    );

    final data = response.data as List;
    return data.map((json) => ArtistEntity(
      id: json['id']?.toString() ?? '',
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      profileImageUrl: json['profileImageUrl'],
      specialization: json['specialization'],
      isActive: json['isActive'] ?? true,
    )).toList();
  }

  @override
  Future<ArtistEntity> getArtistById(ArtistId id) async {
    final response = await DioClient.instance.get('${ApiConstants.artists}/$id');
    final json = response.data;

    return ArtistEntity(
      id: json['id']?.toString() ?? '',
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      profileImageUrl: json['profileImageUrl'],
      specialization: json['specialization'],
      isActive: json['isActive'] ?? true,
    );
  }
}
