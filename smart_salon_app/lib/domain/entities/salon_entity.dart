import '../../types.dart';

class SalonEntity {
  final SalonId id;
  final String slug;
  final String name;
  final String? description;
  final String? address;
  final String? phoneNumber;
  final String? imageUrl;
  final double latitude;
  final double longitude;
  final bool isActive;
  final double rating;
  final int reviewCount;

  const SalonEntity({
    required this.id,
    required this.slug,
    required this.name,
    this.description,
    this.address,
    this.phoneNumber,
    this.imageUrl,
    this.latitude = 0,
    this.longitude = 0,
    this.isActive = true,
    this.rating = 0,
    this.reviewCount = 0,
  });
}
