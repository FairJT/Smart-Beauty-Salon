import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../core/api_service.dart';
import '../core/api_constants.dart';
import '../models/app_notification.dart';

class NotificationState {
  final List<AppNotification> notifications;
  final int unreadCount;
  final bool loading;

  NotificationState({
    this.notifications = const [],
    this.unreadCount = 0,
    this.loading = true,
  });

  NotificationState copyWith({
    List<AppNotification>? notifications,
    int? unreadCount,
    bool? loading,
  }) {
    return NotificationState(
      notifications: notifications ?? this.notifications,
      unreadCount: unreadCount ?? this.unreadCount,
      loading: loading ?? this.loading,
    );
  }
}

class NotificationNotifier extends StateNotifier<NotificationState> {
  NotificationNotifier() : super(NotificationState());

  Future<void> load() async {
    state = state.copyWith(loading: true);
    try {
      final res = await ApiService.get(ApiConstants.notifications);
      final list = (res['notifications'] as List<dynamic>?)
              ?.map((n) => AppNotification.fromJson(n))
              .toList() ??
          [];
      state = NotificationState(
        notifications: list,
        unreadCount: res['unreadCount'] ?? 0,
        loading: false,
      );
    } catch (_) {
      state = state.copyWith(loading: false);
    }
  }

  Future<void> markAsRead(int id) async {
    await ApiService.put('${ApiConstants.notifications}/$id/read', {});
    state = state.copyWith(
      notifications: state.notifications
          .map((n) => n.id == id ? AppNotification(
                id: n.id, title: n.title, message: n.message,
                type: n.type, isRead: true, createdAt: n.createdAt,
              ) : n)
          .toList(),
      unreadCount: (state.unreadCount - 1).clamp(0, 999),
    );
  }

  Future<void> markAllAsRead() async {
    await ApiService.put('${ApiConstants.notifications}/read-all', {});
    state = state.copyWith(
      notifications: state.notifications
          .map((n) => AppNotification(
                id: n.id, title: n.title, message: n.message,
                type: n.type, isRead: true, createdAt: n.createdAt,
              ))
          .toList(),
      unreadCount: 0,
    );
  }

  Future<void> delete(int id) async {
    await ApiService.delete('${ApiConstants.notifications}/$id');
    state = state.copyWith(
      notifications: state.notifications.where((n) => n.id != id).toList(),
    );
  }

  Future<void> refreshUnreadCount() async {
    try {
      final res = await ApiService.get('${ApiConstants.notifications}/unread-count');
      state = state.copyWith(unreadCount: res['count'] ?? 0);
    } catch (_) {}
  }
}

final notificationProvider =
    StateNotifierProvider<NotificationNotifier, NotificationState>((ref) {
  return NotificationNotifier();
});
