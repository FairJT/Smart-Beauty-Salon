import '../../types.dart';
import 'base_entity.dart';

class NotificationEntity extends BaseEntity {
  final String title;
  final String message;
  final bool isRead;
  final String? type;
  final String? relatedId;

  const NotificationEntity({
    required super.id,
    required this.title,
    required this.message,
    this.isRead = false,
    this.type,
    this.relatedId,
    super.createdAt,
    super.updatedAt,
  });
}
