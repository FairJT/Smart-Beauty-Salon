import 'package:flutter/material.dart';

class ErrorBoundary extends StatefulWidget {
  final Widget child;

  const ErrorBoundary({super.key, required this.child});

  @override
  State<ErrorBoundary> createState() => _ErrorBoundaryState();
}

class _ErrorBoundaryState extends State<ErrorBoundary> {
  String? _error;

  @override
  void initState() {
    super.initState();
    FlutterError.onError = (details) {
      FlutterError.presentError(details);
      setState(() {
        _error = details.exceptionAsString();
      });
    };
  }

  @override
  Widget build(BuildContext context) {
    if (_error != null) {
      return Material(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.error_outline, size: 64, color: Colors.red),
                const SizedBox(height: 16),
                Text(
                  'خطایی رخ داده است',
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 8),
                Text(
                  'لطفاً برنامه را مجدداً راه‌اندازی کنید',
                  style: Theme.of(context).textTheme.bodyMedium,
                ),
                const SizedBox(height: 24),
                ElevatedButton(
                  onPressed: () => setState(() => _error = null),
                  child: const Text('تلاش مجدد'),
                ),
              ],
            ),
          ),
        ),
      );
    }
    return widget.child;
  }
}
