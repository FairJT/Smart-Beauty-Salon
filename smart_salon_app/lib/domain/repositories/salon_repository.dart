import '../entities/salon_entity.dart';

abstract class SalonRepository {
  Future<List<SalonEntity>> getSalons({String? searchQuery});
  Future<SalonEntity> getSalonBySlug(String slug);
  Future<List<SalonEntity>> getNearbySalons(double latitude, double longitude, double radiusKm);
}
