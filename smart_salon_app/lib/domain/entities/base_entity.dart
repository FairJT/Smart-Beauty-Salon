abstract class BaseEntity {
  final int id;
  final DateTime? createdAt;
  final DateTime? updatedAt;

  const BaseEntity({
    required this.id,
    this.createdAt,
    this.updatedAt,
  });
}