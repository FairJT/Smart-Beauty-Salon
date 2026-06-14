import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/app_colors.dart';
import '../../core/validators.dart';
import '../providers/auth_provider.dart';
import 'home_screen.dart';
import 'client_home_screen.dart';
import 'manager/manager_dashboard_screen.dart';
import 'artist/artist_dashboard_screen.dart';
import 'admin/admin_dashboard.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _mobileController = TextEditingController();
  final _nationalCodeController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _loading = false;
  bool _hidePass = true;

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _mobileController.dispose();
    _nationalCodeController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _register() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _loading = true);
    try {
      await ref.read(authProvider.notifier).register(
            mobile: _mobileController.text.trim(),
            password: _passwordController.text,
            firstName: _firstNameController.text.trim(),
            lastName: _lastNameController.text.trim(),
            nationalCode: _nationalCodeController.text.trim(),
          );

      if (!mounted) return;
      final auth = ref.read(authProvider);
      Widget destination;
      if (auth.isSuperAdmin) {
        destination = const AdminDashboard();
      } else if (auth.isSalonManager) {
        destination = const ManagerDashboardScreen();
      } else if (auth.isArtist) {
        destination = const ArtistDashboardScreen();
      } else {
        destination = const ClientHomeScreen();
      }
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (_) => destination),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.toString()), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(gradient: AppColors.bgGradient),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 40),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const SizedBox(height: 20),
                  Center(
                    child: Container(
                      width: 90,
                      height: 90,
                      decoration: BoxDecoration(
                        gradient: AppColors.primaryGradient,
                        borderRadius: BorderRadius.circular(24),
                        boxShadow: [
                          BoxShadow(
                            color: AppColors.primary.withValues(alpha: 0.3),
                            blurRadius: 20,
                            offset: const Offset(0, 8),
                          ),
                        ],
                      ),
                      child: const Icon(Icons.person_add_rounded, size: 50, color: Colors.white),
                    ),
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'ثبت‌نام در سالن هوشمند',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: AppColors.textPrimary, fontSize: 22, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 32),
                  TextFormField(
                    controller: _firstNameController,
                    textAlign: TextAlign.right,
                    validator: (v) => Validators.required(v, 'نام'),
                    decoration: const InputDecoration(hintText: 'نام', prefixIcon: Icon(Icons.person_outline)),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _lastNameController,
                    textAlign: TextAlign.right,
                    validator: (v) => Validators.required(v, 'نام خانوادگی'),
                    decoration: const InputDecoration(hintText: 'نام خانوادگی', prefixIcon: Icon(Icons.person_outline)),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _mobileController,
                    keyboardType: TextInputType.phone,
                    textAlign: TextAlign.right,
                    validator: Validators.mobile,
                    decoration: const InputDecoration(hintText: 'شماره موبایل', prefixIcon: Icon(Icons.phone_android)),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _nationalCodeController,
                    keyboardType: TextInputType.number,
                    textAlign: TextAlign.right,
                    validator: Validators.nationalCode,
                    decoration: const InputDecoration(hintText: 'کد ملی', prefixIcon: Icon(Icons.badge_outlined)),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _passwordController,
                    obscureText: _hidePass,
                    textAlign: TextAlign.right,
                    validator: Validators.password,
                    decoration: InputDecoration(
                      hintText: 'رمز عبور (حداقل ۸ کاراکتر)',
                      prefixIcon: const Icon(Icons.lock_outline),
                      suffixIcon: IconButton(
                        icon: Icon(_hidePass ? Icons.visibility_off : Icons.visibility, color: AppColors.gray),
                        onPressed: () => setState(() => _hidePass = !_hidePass),
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),
                  Container(
                    height: 52,
                    decoration: BoxDecoration(
                      gradient: AppColors.primaryGradient,
                      borderRadius: BorderRadius.circular(12),
                      boxShadow: [
                        BoxShadow(
                          color: AppColors.primary.withValues(alpha: 0.3),
                          blurRadius: 12,
                          offset: const Offset(0, 4),
                        ),
                      ],
                    ),
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.transparent,
                        shadowColor: Colors.transparent,
                        foregroundColor: Colors.white,
                        minimumSize: const Size(double.infinity, 52),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      onPressed: _loading ? null : _register,
                      child: _loading
                          ? const SizedBox(
                              width: 24,
                              height: 24,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : const Text('ثبت‌نام', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    ),
                  ),
                  const SizedBox(height: 16),
                  TextButton(
                    onPressed: () => Navigator.pop(context),
                    child: const Text('قبلاً ثبت‌نام کردید؟ وارد شوید', style: TextStyle(color: AppColors.primary)),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
