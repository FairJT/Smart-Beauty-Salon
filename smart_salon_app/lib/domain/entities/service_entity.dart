import 'base_entity.dart';

class ServiceEntity extends BaseEntity {
  final int salonId;
  final String name;
  final String? description;
  final double price;
  final int durationMinutes;
  final String? imageUrl;
  final bool isActive;
  final int? templateId;

  const ServiceEntity({
    required super.id,
    required this.salonId,
    required this.name,
    this.description,
    this.price = 0,
    this.durationMinutes = 30,
    this.imageUrl,
    this.isActive = true,
    this.templateId,
    super.createdAt,
    super.updatedAt,
  });
}