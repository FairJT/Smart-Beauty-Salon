import '../entities/notification_entity.dart';

abstract class NotificationRepository {
  Future<List<NotificationEntity>> getNotifications();
  Future<NotificationEntity> getNotificationById(int id);
  Future<void> markAsRead(int id);
  Future<void> markAllAsRead();
}