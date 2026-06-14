import 'base_entity.dart';

class ArtistEntity extends BaseEntity {
  final int salonId;
  final String name;
  final String? phoneNumber;
  final String? profileImageUrl;
  final String? specialization;
  final bool isActive;

  const ArtistEntity({
    required super.id,
    required this.salonId,
    required this.name,
    this.phoneNumber,
    this.profileImageUrl,
    this.specialization,
    this.isActive = true,
    super.createdAt,
    super.updatedAt,
  });
}