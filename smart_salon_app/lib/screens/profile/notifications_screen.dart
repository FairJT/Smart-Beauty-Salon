import 'package:flutter/material.dart';
import '../../core/api_constants.dart';
import '../../core/api_service.dart';
import '../../core/app_colors.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  List<dynamic> _notifications = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadNotifications();
  }

  Future<void> _loadNotifications() async {
    try {
      final res = await ApiService.get(ApiConstants.notifications);
      setState(() {
        _notifications = res['notifications'] ?? [];
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
    }
  }

  Future<void> _markAllAsRead() async {
    try {
      await ApiService.put('${ApiConstants.notifications}/read-all', {});
      setState(() {
        for (var n in _notifications) {
          n['isRead'] = true;
        }
      });
    } catch (e) {}
  }

  Future<void> _markAsRead(int id, int index) async {
    try {
      await ApiService.put('${ApiConstants.notifications}/$id/read', {});
      setState(() => _notifications[index]['isRead'] = true);
    } catch (e) {}
  }

  Future<void> _delete(int id, int index) async {
    try {
      await ApiService.delete('${ApiConstants.notifications}/$id');
      setState(() => _notifications.removeAt(index));
    } catch (e) {}
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('اعلان‌ها'),
        actions: [
          if (_notifications.any((n) => n['isRead'] == false))
            TextButton(
              onPressed: _markAllAsRead,
              child: const Text('همه خوانده شد',
                  style: TextStyle(color: Colors.white)),
            ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _notifications.isEmpty
              ? const Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.notifications_none,
                          size: 60, color: Colors.grey),
                      SizedBox(height: 12),
                      Text('اعلانی ندارید',
                          style: TextStyle(
                              color: Colors.grey, fontSize: 16)),
                    ],
                  ),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _notifications.length,
                  itemBuilder: (_, i) {
                    final n       = _notifications[i];
                    final isRead  = n['isRead'] == true;
                    final type    = n['type'] ?? 'info';
                    final color   = _typeColor(type);
                    final icon    = _typeIcon(type);

                    return Dismissible(
                      key: Key(n['id'].toString()),
                      direction: DismissDirection.endToStart,
                      background: Container(
                        alignment: Alignment.centerLeft,
                        padding: const EdgeInsets.only(left: 20),
                        decoration: BoxDecoration(
                          color: Colors.red,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: const Icon(Icons.delete,
                            color: Colors.white),
                      ),
                      onDismissed: (_) => _delete(n['id'], i),
                      child: Card(
                        margin: const EdgeInsets.only(bottom: 8),
                        color: isRead
                            ? null
                            : color.withOpacity(0.05),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                          side: isRead
                              ? BorderSide.none
                              : BorderSide(
                                  color: color.withOpacity(0.3)),
                        ),
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: color.withOpacity(0.1),
                            child: Icon(icon, color: color, size: 20),
                          ),
                          title: Text(
                            n['title'] ?? '',
                            style: TextStyle(
                              fontWeight: isRead
                                  ? FontWeight.normal
                                  : FontWeight.bold,
                            ),
                          ),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(n['message'] ?? ''),
                              const SizedBox(height: 4),
                              Text(
                                _formatDate(n['createdAt']),
                                style: const TextStyle(
                                    fontSize: 11, color: Colors.grey),
                              ),
                            ],
                          ),
                          trailing: isRead
                              ? null
                              : Container(
                                  width: 10,
                                  height: 10,
                                  decoration: BoxDecoration(
                                    color: color,
                                    shape: BoxShape.circle,
                                  ),
                                ),
                          onTap: () => _markAsRead(n['id'], i),
                        ),
                      ),
                    );
                  },
                ),
    );
  }

  Color _typeColor(String type) {
    switch (type) {
      case 'success': return Colors.green;
      case 'warning': return Colors.orange;
      case 'error':   return Colors.red;
      default:        return AppColors.primary;
    }
  }

  IconData _typeIcon(String type) {
    switch (type) {
      case 'success': return Icons.check_circle_outline;
      case 'warning': return Icons.warning_amber_outlined;
      case 'error':   return Icons.cancel_outlined;
      default:        return Icons.notifications_outlined;
    }
  }

  String _formatDate(String? dateStr) {
    if (dateStr == null) return '';
    try {
      final date = DateTime.parse(dateStr);
      return '${date.year}/${date.month}/${date.day} '
             '${date.hour}:${date.minute.toString().padLeft(2, '0')}';
    } catch (_) {
      return '';
    }
  }
}