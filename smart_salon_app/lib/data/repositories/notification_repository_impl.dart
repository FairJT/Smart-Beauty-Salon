import '../../domain/entities/notification_entity.dart';
import '../../domain/repositories/notification_repository.dart';
import '../datasources/dio_client.dart';
import '../datasources/api_constants.dart';
import '../../types.dart';

class NotificationRepositoryImpl implements NotificationRepository {
  @override
  Future<List<NotificationEntity>> getNotifications() async {
    final response = await DioClient.instance.get(ApiConstants.notifications);
    final data = response.data as List;

    return data.map((json) => NotificationEntity(
      id: json['id']?.toString() ?? '',
      title: json['title'],
      message: json['message'],
      isRead: json['isRead'] ?? false,
      type: json['type'],
      relatedId: json['relatedId']?.toString(),
      createdAt: json['createdAt'] != null ? DateTime.parse(json['createdAt']) : null,
    )).toList();
  }

  @override
  Future<NotificationEntity> getNotificationById(NotificationId id) async {
    final response = await DioClient.instance.get('${ApiConstants.notifications}/$id');
    final json = response.data;

    return NotificationEntity(
      id: json['id']?.toString() ?? '',
      title: json['title'],
      message: json['message'],
      isRead: json['isRead'] ?? false,
      type: json['type'],
      relatedId: json['relatedId']?.toString(),
      createdAt: json['createdAt'] != null ? DateTime.parse(json['createdAt']) : null,
    );
  }

  @override
  Future<void> markAsRead(NotificationId id) async {
    await DioClient.instance.put(
      '${ApiConstants.notifications}/$id/read',
      data: {},
    );
  }

  @override
  Future<void> markAllAsRead() async {
    await DioClient.instance.put(
      '${ApiConstants.notifications}/read-all',
      data: {},
    );
  }
}
