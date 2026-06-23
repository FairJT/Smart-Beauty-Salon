import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class ArtistPublicScreen extends StatelessWidget {
  const ArtistPublicScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('آرتیست'),
          backgroundColor: FCol.olive,
        ),
        body: ListView(
          padding: const EdgeInsets.all(12),
          children: [
            const CircleAvatar(radius: 40, backgroundColor: FCol.oliveSoft),
            const SizedBox(height: 8),
            const Center(
                child: const Text('نام آرتیست',
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.w600))),
            const SizedBox(height: 12),
            ...List.generate(
              4,
              (i) => Padding(
                padding: const EdgeInsets.symmetric(vertical: 4),
                child: LinearProgressIndicator(
                  value: (i + 1) / 5,
                  backgroundColor: FCol.surface,
                  color: FCol.olive,
                ),
              ),
            ),
            const SizedBox(height: 12),
            ...List.generate(
              2,
              (i) => FCard(
                child: ListTile(
                  leading: const Icon(Icons.rate_review),
                  title: Text('Review $i'),
                  subtitle: const Text('متن نقد...'),
                ),
              ),
            ),
            const SizedBox(height: 12),
            FPrimaryButton('رزرو', onTap: () {
              // TODO: navigate to booking screen for this artist
            })
          ],
        ),
      );
}
