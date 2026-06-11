import '../entities/salon_entity.dart';

abstract class SalonRepository {
  Future<List<SalonEntity>> getSalons({String? searchQuery});
  Future<SalonEntity> getSalonById(int id);
  Future<List<SalonEntity>> getNearbySalons(double latitude, double longitude, double radiusKm);
}