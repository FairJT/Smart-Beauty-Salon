class MoneyFormatter {
  MoneyFormatter._();

  /// Formats an integer amount in minor units (Rials) to a display string.
  /// 150000 → "۱۵۰,۰۰۰ ریال"
  static String format(int amount, {String currency = 'IRR'}) {
    final toman = amount ~/ 10;
    return '${_formatNumber(toman)} ${currency == 'IRR' ? 'تومان' : currency}';
  }

  /// Formats with Persian digits.
  /// 150000 → "۱۵۰,۰۰۰"
  static String formatRaw(int amount) {
    return _formatNumber(amount);
  }

  static String _formatNumber(int number) {
    final enDigits = _addCommas(number);
    return _toPersianDigits(enDigits);
  }

  static String _addCommas(int n) {
    final s = n.toString();
    final sb = StringBuffer();
    for (var i = 0; i < s.length; i++) {
      if (i > 0 && (s.length - i) % 3 == 0) sb.write(',');
      sb.write(s[i]);
    }
    return sb.toString();
  }

  static String _toPersianDigits(String s) {
    const en = '0123456789';
    const fa = '۰۱۲۳۴۵۶۷۸۹';
    return s.split('').map((c) {
      final idx = en.indexOf(c);
      return idx >= 0 ? fa[idx] : c;
    }).join();
  }

  /// Converts minor units to compact: 15000000 → "۱.۵M تومان"
  static String formatCompact(int amount, {String currency = 'IRR'}) {
    final toman = amount ~/ 10;
    final label = currency == 'IRR' ? 'تومان' : currency;
    if (toman >= 1000000000) {
      return '${_toPersianDigits((toman / 1000000000).toStringAsFixed(1))}B $label';
    } else if (toman >= 1000000) {
      return '${_toPersianDigits((toman / 1000000).toStringAsFixed(1))}M $label';
    } else if (toman >= 1000) {
      return '${_toPersianDigits((toman / 1000).toStringAsFixed(1))}K $label';
    }
    return '${_toPersianDigits(toman.toString())} $label';
  }
}
