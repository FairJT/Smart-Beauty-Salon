import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/fresha/fresha_ui.dart';
import '../../presentation/providers/auth_provider.dart';
import '../../core/role_router.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _loading = false;

  @override
  void dispose() {
    _phoneController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _login() async {
    final phone = _phoneController.text.trim();
    final password = _passwordController.text;
    if (phone.isEmpty || password.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('لطفاً شماره تلفن و رمز عبور را وارد کنید')),
      );
      return;
    }
    setState(() => _loading = true);
    try {
      final success =
          await ref.read(authProvider.notifier).login(phone, password);
      setState(() => _loading = false);
      if (success && mounted) {
        final ut = ref.read(authProvider).user?.userType ?? 4;
        Navigator.of(context).pushReplacementNamed(roleHome(ut));
      } else if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text('ورود ناموفق — شماره یا رمز عبور اشتباه است')),
        );
      }
    } on DioException catch (e) {
      setState(() => _loading = false);
      if (!mounted) return;
      final msg = e.type == DioExceptionType.connectionTimeout ||
              e.type == DioExceptionType.receiveTimeout ||
              e.type == DioExceptionType.connectionError
          ? 'خطا در اتصال به سرور — لطفاً اینترنت خود را بررسی کنید'
          : 'خطای شبکه: ${e.message}';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(msg), backgroundColor: Colors.red.shade700),
      );
    } catch (e) {
      setState(() => _loading = false);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text('خطای غیرمنتظره: $e'),
            backgroundColor: Colors.red.shade700),
      );
    }
  }

  void _guestBrowse() {
    ref.read(authProvider.notifier).loginAsGuest();
    Navigator.of(context).pushReplacementNamed('/home');
  }

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    return Scaffold(
      backgroundColor: FCol.surface,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 60),
              // Logo
              Center(
                child: Container(
                  width: 72,
                  height: 72,
                  decoration: BoxDecoration(
                    color: FCol.olive,
                    borderRadius: BorderRadius.circular(18),
                  ),
                  child: const Icon(Icons.spa_rounded,
                      size: 38, color: Colors.white),
                ),
              ),
              const SizedBox(height: 24),
              Center(
                child: Text(
                  'ورود به سالن زیبایی',
                  style: textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.w700,
                    color: FCol.ink,
                  ),
                ),
              ),
              const SizedBox(height: 8),
              Center(
                child: Text(
                  'شماره موبایل خود را وارد کنید',
                  style: textTheme.bodySmall?.copyWith(
                    color: FCol.muted,
                  ),
                ),
              ),
              const SizedBox(height: 36),
              // Phone field
              FCard(
                child: TextField(
                  controller: _phoneController,
                  keyboardType: TextInputType.phone,
                  maxLength: 11,
                  textDirection: TextDirection.ltr,
                  decoration: InputDecoration(
                    hintText: '۰۹۱۲...',
                    hintStyle: const TextStyle(color: FCol.muted),
                    border: InputBorder.none,
                    counterText: '',
                    prefixIcon: const Icon(Icons.phone_android,
                        color: FCol.olive, size: 20),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              // Password field
              FCard(
                child: TextField(
                  controller: _passwordController,
                  obscureText: true,
                  decoration: InputDecoration(
                    hintText: 'رمز عبور',
                    hintStyle: const TextStyle(color: FCol.muted),
                    border: InputBorder.none,
                    prefixIcon:
                        const Icon(Icons.lock, color: FCol.olive, size: 20),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              // CTA
              FPrimaryButton(
                'ورود',
                onTap: _login,
              ),
              if (_loading)
                const Padding(
                  padding: EdgeInsets.only(top: 16),
                  child: FLoading(),
                ),
              const SizedBox(height: 24),
              // Guest button - prominent guest login option
              Center(
                child: TextButton.icon(
                  onPressed: _guestBrowse,
                  icon: const Icon(
                    Icons.person_outline,
                    color: FCol.olive,
                    size: 20,
                  ),
                  label: Text(
                    'ورود به عنوان مهمان',
                    style: textTheme.titleMedium?.copyWith(
                      color: FCol.olive,
                    ),
                  ),
                  style: TextButton.styleFrom(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 8,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 8),
              Center(
                child: GestureDetector(
                  onTap: _guestBrowse,
                  child: Text(
                    'مرور بدون ورود',
                    style: textTheme.bodySmall?.copyWith(
                      color: FCol.muted,
                      decoration: TextDecoration.underline,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
