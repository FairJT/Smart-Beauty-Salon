import 'base_entity.dart';

class ServiceEntity extends BaseEntity {
  final String name;
  final String? description;
  final double price;
  final int durationMinutes;
  final String? imageUrl;
  final bool isActive;
  final String? templateId;

  const ServiceEntity({
    required super.id,
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
