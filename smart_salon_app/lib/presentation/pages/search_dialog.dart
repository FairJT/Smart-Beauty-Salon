import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/salon_provider.dart';

void showSearchDialog(BuildContext context, WidgetRef ref) {
  final controller = TextEditingController();

  showDialog(
    context: context,
    builder: (_) => AlertDialog(
      title: const Text('جستجوی سالن'),
      content: TextField(
        controller: controller,
        autofocus: true,
        decoration: const InputDecoration(
          hintText: 'نام سالن...',
          prefixIcon: Icon(Icons.search),
          border: OutlineInputBorder(),
        ),
        onSubmitted: (val) {
          Navigator.pop(context);
          ref.read(salonListProvider.notifier).setSearch(val);
        },
      ),
      actions: [
        TextButton(
          onPressed: () {
            Navigator.pop(context);
            controller.clear();
            ref.read(salonListProvider.notifier).setSearch('');
          },
          child: const Text('پاک کردن'),
        ),
        ElevatedButton(
          onPressed: () {
            Navigator.pop(context);
            ref.read(salonListProvider.notifier).setSearch(controller.text);
          },
          child: const Text('جستجو'),
        ),
      ],
    ),
  );
}
