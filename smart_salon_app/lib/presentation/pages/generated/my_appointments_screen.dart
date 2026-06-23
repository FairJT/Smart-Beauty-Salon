import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class MyAppointmentsScreen extends StatelessWidget {
  const MyAppointmentsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('نوبت‌های من'),
          backgroundColor: FCol.ink,
        ),
        body: ListView(
          children: List.generate(
            3,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.event),
                title: Text('نوبت $i'),
                subtitle: const Text('جزئیات نوبت...'),
                trailing: const FStatusChip('در انتظار', Colors.orange),
              ),
            ),
          ),
        ),
      );
}
