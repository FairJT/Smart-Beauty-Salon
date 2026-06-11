import 'package:flutter/material.dart';

class AppColors {
  static const Color primary = Color(0xFF1B3A5C);
  static const Color gold = Color(0xFF9A6F0A);
  static const Color green = Color(0xFF145A32);
  static const Color lightBlue = Color(0xFFEBF5FB);
  static const Color white = Colors.white;
  static const Color dark = Color(0xFF1C2833);
  static const Color gray = Color(0xFF717D7E);

  static Color statusColor(int status) {
    switch (status) {
      case 1: return Colors.orange;
      case 2: return Colors.green;
      case 3: return Colors.blue;
      case 4: return Colors.grey;
      case 5: return Colors.red;
      case 6: return Colors.red;
      default: return Colors.grey;
    }
  }
}
