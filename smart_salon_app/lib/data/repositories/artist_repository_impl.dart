import '../../domain/entities/artist_entity.dart';
import '../../domain/repositories/artist_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';

class ArtistRepositoryImpl implements ArtistRepository {
  @override
  Future<List<ArtistEntity>> getArtistsBySalon(int salonId) async {
    final response = await DioClient.instance.get(
      '${ApiConstants.artists}/salon/$salonId',
    );

    final data = response.data as List;
    return data.map((json) => ArtistEntity(
      id: json['id'],
      salonId: json['salonId'],
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      profileImageUrl: json['profileImageUrl'],
      specialization: json['specialization'],
      isActive: json['isActive'] ?? true,
    )).toList();
  }

  @override
  Future<ArtistEntity> getArtistById(int id) async {
    final response = await DioClient.instance.get('${ApiConstants.artists}/$id');
    final json = response.data;

    return ArtistEntity(
      id: json['id'],
      salonId: json['salonId'],
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      profileImageUrl: json['profileImageUrl'],
      specialization: json['specialization'],
      isActive: json['isActive'] ?? true,
    );
  }
}