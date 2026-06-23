import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

/// ---------------------------------------------------------------------------
/// Public / Guest Screens
/// ---------------------------------------------------------------------------

class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        body: Center(
          child: FPrimaryButton('شروع',
              onTap: () => Navigator.pushReplacementNamed(context, '/home')),
        ),
      );
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('خانه')),
        body: const Center(child: Text('Home Screen Placeholder')),
        bottomNavigationBar:
            FBottomNav(index: 0, onTap: (_) {}, items: const []),
      );
}

class SalonDetailScreen extends StatelessWidget {
  const SalonDetailScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('سالن')),
        body: const Center(child: Text('Salon Detail Placeholder')),
      );
}

class ArtistPublicScreen extends StatelessWidget {
  const ArtistPublicScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('آرتیست')),
        body: const Center(child: Text('Artist Public Placeholder')),
      );
}

class BlogListScreen extends StatelessWidget {
  const BlogListScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('بلاگ‌ها')),
        body: const Center(child: Text('Blog List Placeholder')),
      );
}

class BlogPostScreen extends StatelessWidget {
  const BlogPostScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مقاله')),
        body: const Center(child: Text('Blog Post Placeholder')),
      );
}

class JoinSalonFormScreen extends StatelessWidget {
  const JoinSalonFormScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('درخواست پیوستن')),
        body: const Center(child: Text('Join Salon Form Placeholder')),
      );
}

class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('ورود')),
        body: const Center(child: Text('Login Placeholder')),
      );
}

class RegisterScreen extends StatelessWidget {
  const RegisterScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('ثبت نام')),
        body: const Center(child: Text('Register Placeholder')),
      );
}

class OtpScreen extends StatelessWidget {
  const OtpScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('کد تایید')),
        body: const Center(child: Text('OTP Placeholder')),
      );
}

/// ---------------------------------------------------------------------------
/// Client Screens
/// ---------------------------------------------------------------------------

class BookingFlowScreen extends StatelessWidget {
  const BookingFlowScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('نوبت‌گیری')),
        body: const Center(child: Text('Booking Flow Placeholder')),
      );
}

class AppointmentListScreen extends StatelessWidget {
  const AppointmentListScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('نوبت‌های من')),
        body: const Center(child: Text('Appointment List Placeholder')),
      );
}

class ServiceHistoryScreen extends StatelessWidget {
  const ServiceHistoryScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('سابقه خدمات')),
        body: const Center(child: Text('Service History Placeholder')),
      );
}

class InvoiceScreen extends StatelessWidget {
  const InvoiceScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('فاکتور')),
        body: const Center(child: Text('Invoice Placeholder')),
      );
}

class OffersScreen extends StatelessWidget {
  const OffersScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('تخفیف‌ها')),
        body: const Center(child: Text('Offers Placeholder')),
      );
}

class FeedbackScreen extends StatelessWidget {
  const FeedbackScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('بازخورد')),
        body: const Center(child: Text('Feedback Placeholder')),
      );
}

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('پروفایل')),
        body: const Center(child: Text('Profile Placeholder')),
      );
}

class NotificationsScreen extends StatelessWidget {
  const NotificationsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('اعلان‌ها')),
        body: const Center(child: Text('Notifications Placeholder')),
      );
}

/// ---------------------------------------------------------------------------
/// Artist Screens
/// ---------------------------------------------------------------------------

class ArtistDashboardScreen extends StatelessWidget {
  const ArtistDashboardScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('داشبورد آرتیست')),
        body: const Center(child: Text('Artist Dashboard Placeholder')),
      );
}

class ArtistScheduleScreen extends StatelessWidget {
  const ArtistScheduleScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('زمان‌بندی')),
        body: const Center(child: Text('Artist Schedule Placeholder')),
      );
}

class ArtistAppointmentsScreen extends StatelessWidget {
  const ArtistAppointmentsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('نوبت‌ها')),
        body: const Center(child: Text('Artist Appointments Placeholder')),
      );
}

class LeaveRequestScreen extends StatelessWidget {
  const LeaveRequestScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('درخواست مرخصی')),
        body: const Center(child: Text('Leave Request Placeholder')),
      );
}

class ClientNotesScreen extends StatelessWidget {
  const ClientNotesScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('نکات مشتری‌ها')),
        body: const Center(child: Text('Client Notes Placeholder')),
      );
}

class ProductUsageScreen extends StatelessWidget {
  const ProductUsageScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مصرف محصول')),
        body: const Center(child: Text('Product Usage Placeholder')),
      );
}

class StaffRequestsScreen extends StatelessWidget {
  const StaffRequestsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('درخواست‌های پرسنل')),
        body: const Center(child: Text('Staff Requests Placeholder')),
      );
}

class ContractsScreen extends StatelessWidget {
  const ContractsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('قراردادها')),
        body: const Center(child: Text('Contracts Placeholder')),
      );
}

class NoticesScreen extends StatelessWidget {
  const NoticesScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('اعلان‌ها')),
        body: const Center(child: Text('Notices Placeholder')),
      );
}

/// ---------------------------------------------------------------------------
/// Manager Screens
/// ---------------------------------------------------------------------------

class ManagerDashboardScreen extends StatelessWidget {
  const ManagerDashboardScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('داشبورد مدیر')),
        body: const Center(child: Text('Manager Dashboard Placeholder')),
      );
}

class SalonProfileScreen extends StatelessWidget {
  const SalonProfileScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('پروفایل سالن')),
        body: const Center(child: Text('Salon Profile Placeholder')),
      );
}

class WorkingHoursScreen extends StatelessWidget {
  const WorkingHoursScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('ساعات کاری')),
        body: const Center(child: Text('Working Hours Placeholder')),
      );
}

class ServicesManagementScreen extends StatelessWidget {
  const ServicesManagementScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مدیریت خدمات')),
        body: const Center(child: Text('Services Management Placeholder')),
      );
}

class StaffManagementScreen extends StatelessWidget {
  const StaffManagementScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مدیریت پرسنل')),
        body: const Center(child: Text('Staff Management Placeholder')),
      );
}

class ManagerAppointmentsScreen extends StatelessWidget {
  const ManagerAppointmentsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('نوبت‌ها')),
        body: const Center(child: Text('Manager Appointments Placeholder')),
      );
}

class ManagerCustomersScreen extends StatelessWidget {
  const ManagerCustomersScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مشتریان')),
        body: const Center(child: Text('Manager Customers Placeholder')),
      );
}

class ManagerDiscountsScreen extends StatelessWidget {
  const ManagerDiscountsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('تخفیف‌ها')),
        body: const Center(child: Text('Manager Discounts Placeholder')),
      );
}

class FinanceLedgerScreen extends StatelessWidget {
  const FinanceLedgerScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('دفتر مالی')),
        body: const Center(child: Text('Finance Ledger Placeholder')),
      );
}

class HiringScreen extends StatelessWidget {
  const HiringScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('استخدام')),
        body: const Center(child: Text('Hiring Placeholder')),
      );
}

class InboxScreen extends StatelessWidget {
  const InboxScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('صندوق ورودی')),
        body: const Center(child: Text('Inbox Placeholder')),
      );
}

/// ---------------------------------------------------------------------------
/// SuperAdmin Screens
/// ---------------------------------------------------------------------------

class AdminDashboardScreen extends StatelessWidget {
  const AdminDashboardScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('داشبورد ادمین')),
        body: const Center(child: Text('Admin Dashboard Placeholder')),
      );
}

class TenantManagementScreen extends StatelessWidget {
  const TenantManagementScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مدیریت مستاجرین')),
        body: const Center(child: Text('Tenant Management Placeholder')),
      );
}

class UserManagementScreen extends StatelessWidget {
  const UserManagementScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مدیریت کاربران')),
        body: const Center(child: Text('User Management Placeholder')),
      );
}

class ServiceTemplatesScreen extends StatelessWidget {
  const ServiceTemplatesScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('قالب‌های سرویس')),
        body: const Center(child: Text('Service Templates Placeholder')),
      );
}

class PackageListingsScreen extends StatelessWidget {
  const PackageListingsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('بسته‌ها')),
        body: const Center(child: Text('Package Listings Placeholder')),
      );
}

class HomepageCmsScreen extends StatelessWidget {
  const HomepageCmsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('مدیریت صفحه اصلی')),
        body: const Center(child: Text('Homepage CMS Placeholder')),
      );
}

class BlogEditorScreen extends StatelessWidget {
  const BlogEditorScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('ویرایش بلاگ')),
        body: const Center(child: Text('Blog Editor Placeholder')),
      );
}

class PlacementsScreen extends StatelessWidget {
  const PlacementsScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('پیشنهادات ویژه')),
        body: const Center(child: Text('Placements Placeholder')),
      );
}

class JoinRequestsAdminScreen extends StatelessWidget {
  const JoinRequestsAdminScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('درخواست‌های پیوستن')),
        body: const Center(child: Text('Join Requests Admin Placeholder')),
      );
}

class PlatformAccountingScreen extends StatelessWidget {
  const PlatformAccountingScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('حسابداری پلتفرم')),
        body: const Center(child: Text('Platform Accounting Placeholder')),
      );
}
