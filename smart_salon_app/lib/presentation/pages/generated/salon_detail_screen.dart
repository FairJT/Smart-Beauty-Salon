import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class SalonDetailScreen extends StatelessWidget {
  const SalonDetailScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('اطلاعات سالن'),
          backgroundColor: FCol.olive,
        ),
        body: ListView(
          children: [
            Container(
              height: 200,
              color: FCol.oliveSoft,
              alignment: Alignment.center,
              child: const Icon(Icons.photo, size: 80, color: FCol.ink),
            ),
            const SizedBox(height: 12),
            const Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                FStat('4.5', 'امتیاز'),
                FStat('120', 'نظرات'),
                FStat('12', 'خدمات')
              ],
            ),
            const SizedBox(height: 12),
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 12),
              child: TabBar(
                tabs: [
                  Tab(text: 'خدمات'),
                  Tab(text: 'آرتیست‌ها'),
                  Tab(text: 'نظرات')
                ],
              ),
            ),
            const SizedBox(height: 12),
            ...List.generate(
              3,
              (i) => FServiceRow(
                name: 'Service $i',
                durationMin: 30 + i * 10,
                priceRials: 50000 + i * 10000,
                added: false,
                onAdd: null,
              ),
            ),
          ],
        ),
        bottomNavigationBar: Container(
          padding: const EdgeInsets.all(12),
          color: FCol.surface,
          child: FPrimaryButton('رزرو', onTap: () {
            // TODO: navigate to booking flow
          }),
        ),
      );
}
