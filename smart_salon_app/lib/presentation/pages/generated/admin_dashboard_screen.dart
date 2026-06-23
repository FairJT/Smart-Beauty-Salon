import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class AdminDashboardScreen extends StatelessWidget {
  const AdminDashboardScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('داشبورد ادمین'),
          backgroundColor: Colors.amber,
        ),
        body: GridView.count(
          crossAxisCount: 2,
          padding: const EdgeInsets.all(12),
          children: const [
            FStat('5', 'مجموع کاربران'),
            FStat('120', 'سالن‌ها'),
            FStat('200k', 'درآمد کل'),
            FStat('12', 'درخواست‌های فعال')
          ],
        ),
      );
}
