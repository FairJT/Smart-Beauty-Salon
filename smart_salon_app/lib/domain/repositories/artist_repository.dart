import '../../types.dart';
import '../entities/artist_entity.dart';

abstract class ArtistRepository {
  Future<List<ArtistEntity>> getArtistsBySalon(String slug);
  Future<ArtistEntity> getArtistById(ArtistId id);
}
