import 'base_entity.dart';

class SlotEntity extends BaseEntity {
  final DateTime startTime;
  final DateTime endTime;
  final int artistId;
  final bool isAvailable;

  const SlotEntity({
    required super.id,
    required this.startTime,
    required this.endTime,
    required this.artistId,
    this.isAvailable = true,
    super.createdAt,
    super.updatedAt,
  });
}