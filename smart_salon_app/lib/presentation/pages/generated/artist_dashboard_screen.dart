import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class ArtistDashboardScreen extends StatelessWidget {
  const ArtistDashboardScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('داشبورد'),
          backgroundColor: FCol.olive,
        ),
        body: GridView.count(
          crossAxisCount: 2,
          padding: const EdgeInsets.all(12),
          children: const [
            FStat('5', 'نوبت امروز'),
            FStat('2', 'در انتظار'),
            FStat('10', 'کل نوبت‌ها'),
            FStat('3', 'درخواست مرخصی')
          ],
        ),
      );
}
