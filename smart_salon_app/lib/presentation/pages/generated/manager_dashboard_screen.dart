import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class ManagerDashboardScreen extends StatelessWidget {
  const ManagerDashboardScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('داشبورد مدیر'),
          backgroundColor: Colors.indigo,
        ),
        body: GridView.count(
          crossAxisCount: 2,
          padding: const EdgeInsets.all(12),
          children: const [
            FStat('12', 'سالن‌ها'),
            FStat('350', 'پرسنل'),
            FStat('120k', 'درآمد'),
            FStat('5', 'درخواست مرخصی')
          ],
        ),
      );
}
