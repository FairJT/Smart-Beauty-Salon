import '../../types.dart';
import '../entities/notification_entity.dart';

abstract class NotificationRepository {
  Future<List<NotificationEntity>> getNotifications();
  Future<NotificationEntity> getNotificationById(NotificationId id);
  Future<void> markAsRead(NotificationId id);
  Future<void> markAllAsRead();
}
