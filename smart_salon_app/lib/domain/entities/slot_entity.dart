import '../../types.dart';
import 'base_entity.dart';

class SlotEntity extends BaseEntity {
  final DateTime startTime;
  final DateTime endTime;
  final bool isAvailable;

  String get start => '${startTime.hour.toString().padLeft(2, '0')}:${startTime.minute.toString().padLeft(2, '0')}';

  const SlotEntity({
    required super.id,
    required this.startTime,
    required this.endTime,
    this.isAvailable = true,
    super.createdAt,
    super.updatedAt,
  });
}
