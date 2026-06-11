import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../domain/entities/notification_entity.dart';
import '../../domain/repositories/notification_repository.dart';
import '../../data/repositories/notification_repository_impl.dart';

class NotificationState {
  final List<NotificationEntity> notifications;
  final int unreadCount;
  final bool loading;

  NotificationState({
    this.notifications = const [],
    this.unreadCount = 0,
    this.loading = true,
  });

  NotificationState copyWith({
    List<NotificationEntity>? notifications,
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
  final NotificationRepository _notificationRepository;

  NotificationNotifier(this._notificationRepository) : super(NotificationState()) {
    load();
  }

  Future<void> load() async {
    state = state.copyWith(loading: true);
    try {
      final notifications = await _notificationRepository.getNotifications();
      final unreadCount = notifications.where((n) => !n.isRead).length;
      state = NotificationState(
        notifications: notifications,
        unreadCount: unreadCount,
        loading: false,
      );
    } catch (_) {
      state = state.copyWith(loading: false);
    }
  }

  Future<void> markAsRead(int id) async {
    await _notificationRepository.markAsRead(id);
    state = state.copyWith(
      notifications: state.notifications
          .map((n) => n.id == id ? NotificationEntity(
                id: n.id, title: n.title, message: n.message,
                type: n.type, isRead: true, createdAt: n.createdAt,
              ) : n)
          .toList(),
      unreadCount: (state.unreadCount - 1).clamp(0, 999),
    );
  }

  Future<void> markAllAsRead() async {
    await _notificationRepository.markAllAsRead();
    state = state.copyWith(
      notifications: state.notifications
          .map((n) => NotificationEntity(
                id: n.id, title: n.title, message: n.message,
                type: n.type, isRead: true, createdAt: n.createdAt,
              ))
          .toList(),
      unreadCount: 0,
    );
  }
}

final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  return NotificationRepositoryImpl();
});

final notificationProvider =
    StateNotifierProvider<NotificationNotifier, NotificationState>((ref) {
  final notificationRepository = ref.watch(notificationRepositoryProvider);
  return NotificationNotifier(notificationRepository);
});
