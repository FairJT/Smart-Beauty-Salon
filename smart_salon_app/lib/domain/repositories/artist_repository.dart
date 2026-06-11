import '../entities/artist_entity.dart';

abstract class ArtistRepository {
  Future<List<ArtistEntity>> getArtistsBySalon(int salonId);
  Future<ArtistEntity> getArtistById(int id);
}