import 'package:shamsi_date/shamsi_date.dart';

class JalaaliHelper {
  JalaaliHelper._();

  static String formatDate(DateTime date) {
    final j = Jalali.fromDateTime(date);
    return '${_fa(j.day)} ${_monthName(j.month)} ${_fa(j.year)}';
  }

  static String formatTime(DateTime date) {
    final j = Jalali.fromDateTime(date);
    return '${_fa(j.hour)}:${_fa(j.minute).padLeft(2, '۰')}';
  }

  static String formatDateTime(DateTime date) {
    return '${formatDate(date)} ساعت ${formatTime(date)}';
  }

  static String formatRelative(DateTime date) {
    final now = DateTime.now();
    final diff = date.difference(now);

    if (diff.isNegative) {
      final past = -diff.inMinutes;
      if (past < 60) return '${_fa(past)} دقیقه پیش';
      if (past < 1440) return '${_fa(past ~/ 60)} ساعت پیش';
      return formatDate(date);
    }

    final future = diff.inMinutes;
    if (future < 60) return '${_fa(future)} دقیقه دیگر';
    if (future < 1440) return '${_fa(future ~/ 60)} ساعت دیگر';
    return formatDate(date);
  }

  static String _fa(int n) {
    const en = '0123456789';
    const fa = '۰۱۲۳۴۵۶۷۸۹';
    return n.toString().split('').map((c) {
      final idx = en.indexOf(c);
      return idx >= 0 ? fa[idx] : c;
    }).join();
  }

  static String _monthName(int month) {
    const names = [
      'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
      'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند',
    ];
    return names[month - 1];
  }
}
