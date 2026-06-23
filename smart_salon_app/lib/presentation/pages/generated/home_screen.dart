import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('کشف سالن‌ها'),
          backgroundColor: FCol.olive,
        ),
        body: ListView(
          padding: const EdgeInsets.all(12),
          children: [
            TextField(
              decoration: InputDecoration(
                hintText: 'جستجو',
                prefixIcon: const Icon(Icons.search),
                filled: true,
                fillColor: FCol.surface,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(color: FCol.line),
                ),
              ),
            ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: List.generate(
                5,
                (i) => FChip('دسته $i', icon: Icons.category),
              ),
            ),
            const SizedBox(height: 12),
            const FCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Featured Salon',
                      style:
                          TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
                  SizedBox(height: 8),
                  Text('Lorem ipsum dolor sit amet...'),
                ],
              ),
            ),
            const SizedBox(height: 12),
            ...List.generate(
              3,
              (i) => FCard(
                child: ListTile(
                  leading: const Icon(Icons.store),
                  title: Text('Salon $i'),
                  subtitle: Text('Address $i'),
                ),
              ),
            ),
          ],
        ),
        bottomNavigationBar: FBottomNav(index: 0, onTap: (_) {}, items: const [
          (icon: Icons.home, label: 'خانه'),
          (icon: Icons.calendar_today, label: 'رزرو'),
          (icon: Icons.bookmark, label: 'نوبتها'),
          (icon: Icons.person, label: 'پروفایل')
        ]),
      );
}
