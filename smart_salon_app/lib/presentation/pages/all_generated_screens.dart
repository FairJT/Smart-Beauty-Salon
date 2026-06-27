export 'generated/onboarding_screen.dart';
export 'generated/home_screen.dart';
export 'generated/salon_detail_screen.dart';
export 'generated/artist_public_screen.dart';
export 'generated/booking_flow_screen.dart';
export 'generated/my_appointments_screen.dart';
export 'generated/artist_dashboard_screen.dart';
export 'generated/manager_dashboard_screen.dart';
export 'generated/admin_dashboard_screen.dart';
// ----------------------------------------------------------------
// This file contains minimal scaffold implementations for every screen
// listed in `TODO/UI‑screens‑all.md`. Each screen is a StatelessWidget
// (or StatefulWidget where a state‑ful example is more appropriate) that
// imports the Fresha UI components and provides a placeholder layout.
// Real data fetching, state‑management and navigation will be added later
// following the build order defined in the checklist.
//
// NOTE: All screens are deliberately lightweight – only the widget tree
// required to display the basic design system widgets is included.
// ----------------------------------------------------------------

import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';
import 'package:smart_salon_app/presentation/widgets/dashboard_widgets.dart';

// --------------------------
// A) Public / Guest screens
// --------------------------

// Onboarding – new (mock 1)
class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: FCol.surface,
      body: Center(
        child: FPrimaryButton('شروع', onTap: () {
          // TODO: navigate to home after onboarding
        }),
      ),
    );
  }
}

// Home / discovery – restyle
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
            // Search bar placeholder
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
            // Category chips
            Wrap(
              spacing: 8,
              children: List.generate(
                5,
                (i) => FChip('دسته $i', icon: Icons.category),
              ),
            ),
            const SizedBox(height: 12),
            // Featured card
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
            // Nearby salons list
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

// Salon profile – restyle
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
            // Hero image placeholder
            Container(
              height: 200,
              color: FCol.oliveSoft,
              alignment: Alignment.center,
              child: const Icon(Icons.photo, size: 80, color: FCol.ink),
            ),
            const SizedBox(height: 12),
            // Stats row
            const Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                FStat('4.5', 'امتیاز'),
                FStat('120', 'نظرات'),
                FStat('12', 'خدمات')
              ],
            ),
            const SizedBox(height: 12),
            // Tabs placeholder
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
            // Service rows
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

// Artist public page – new
class ArtistPublicScreen extends StatelessWidget {
  const ArtistPublicScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('آرتیست'),
          backgroundColor: FCol.olive,
        ),
        body: ListView(
          padding: const EdgeInsets.all(12),
          children: [
            // Avatar & name
            const CircleAvatar(radius: 40, backgroundColor: FCol.oliveSoft),
            const SizedBox(height: 8),
            const Center(
                child: Text('نام آرتیست',
                    style:
                        TextStyle(fontSize: 18, fontWeight: FontWeight.w600))),
            const SizedBox(height: 12),
            // Skill bars (placeholder)
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
            // Reviews placeholder
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
            // Book button
            FPrimaryButton('رزرو', onTap: () {
              // TODO: navigate to booking screen for this artist
            })
          ],
        ),
      );
}

// Blog list – new
class BlogListScreen extends StatelessWidget {
  const BlogListScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('وبلاگ'),
          backgroundColor: FCol.olive,
        ),
        body: ListView(
          children: List.generate(
            5,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.article),
                title: Text('پست وبلاگ $i'),
                subtitle: const Text('خلاصه مطلب...'),
                onTap: () {
                  // TODO: navigate to BlogPostScreen
                },
              ),
            ),
          ),
        ),
      );
}

// Blog post – new
class BlogPostScreen extends StatelessWidget {
  final String postId;
  const BlogPostScreen(this.postId, {super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('جزئیات پست'),
          backgroundColor: FCol.olive,
        ),
        body: const Padding(
          padding: EdgeInsets.all(12),
          child: Text(
            'متن کامل پست وبلاگ اینجا قرار می‌گیرد.',
            style: TextStyle(fontSize: 16),
          ),
        ),
      );
}

// Join‑salon form – new
class JoinSalonFormScreen extends StatelessWidget {
  const JoinSalonFormScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('ثبت سالن'),
          backgroundColor: FCol.olive,
        ),
        body: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            children: [
              const TextField(
                decoration: InputDecoration(labelText: 'نام سالن'),
              ),
              const SizedBox(height: 8),
              const TextField(
                decoration: InputDecoration(labelText: 'آدرس'),
              ),
              const SizedBox(height: 8),
              FPrimaryButton('ثبت', onTap: () {
                // TODO: POST /api/join-requests
              })
            ],
          ),
        ),
      );
}

// Login / Register / OTP – restyle
class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('ورود'), backgroundColor: FCol.ink),
        body: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            children: [
              const TextField(
                decoration: InputDecoration(labelText: 'ایمیل یا شماره تلفن'),
              ),
              const SizedBox(height: 8),
              const TextField(
                obscureText: true,
                decoration: InputDecoration(labelText: 'رمز عبور'),
              ),
              const SizedBox(height: 12),
              FPrimaryButton('ورود', onTap: () {
                // TODO: authenticate
              })
            ],
          ),
        ),
      );
}

// --------------------------
// B) Client screens
// --------------------------

// Booking flow – restyle (mock 4)
class BookingFlowScreen extends StatefulWidget {
  const BookingFlowScreen({Key? key}) : super(key: key);
  @override
  State<BookingFlowScreen> createState() => _BookingFlowScreenState();
}

class _BookingFlowScreenState extends State<BookingFlowScreen> {
  int step = 0;
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('رزرو نوبت'),
          backgroundColor: FCol.ink,
        ),
        body: Stepper(
          currentStep: step,
          onStepContinue: () => setState(() => step = (step + 1).clamp(0, 3)),
          onStepCancel: () => setState(() => step = (step - 1).clamp(0, 3)),
          steps: [
            Step(
                title: const Text('انتخاب آرتیست'),
                content: Wrap(
                  spacing: 8,
                  children: List.generate(
                      4,
                      (i) => GestureDetector(
                          onTap: () {
                            // select artist
                          },
                          child: FAvatar('A$i'))),
                )),
            Step(
                title: const Text('انتخاب زمان'),
                content: Wrap(
                  spacing: 8,
                  children: List.generate(
                      6,
                      (i) => FSlot('${i + 9}:00',
                          state: FSlotState.free, onTap: () {})),
                )),
            const Step(
                title: Text('اطلاعات پرداخت'),
                content: Text('در اینجا فرم پرداخت نمایش می‌شود.')),
            const Step(
                title: Text('تایید نهایی'),
                content: Text('خلاصه رزرو و دکمه پرداخت نهایی.')),
          ],
        ),
        bottomNavigationBar: FBottomNav(index: 1, onTap: (_) {}, items: const [
          (icon: Icons.home, label: 'خانه'),
          (icon: Icons.calendar_today, label: 'رزرو'),
          (icon: Icons.bookmark, label: 'نوبتها'),
          (icon: Icons.person, label: 'پروفایل')
        ]),
      );
}

// My appointments – restyle
class MyAppointmentsScreen extends StatelessWidget {
  const MyAppointmentsScreen({Key? key}) : super(key: key);
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

// Service history – restyle (placeholder)
class ServiceHistoryScreen extends StatelessWidget {
  const ServiceHistoryScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('تاریخچه خدمات'),
          backgroundColor: FCol.ink,
        ),
        body: const Center(child: Text('لیست خدمات گذشته')),
      );
}

// Invoice – new
class InvoiceScreen extends StatelessWidget {
  final String invoiceId;
  const InvoiceScreen(this.invoiceId, {Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('فاکتور'),
          backgroundColor: FCol.ink,
        ),
        body: FCard(
          child: Padding(
            padding: const EdgeInsets.all(12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('شماره فاکتور: #12345',
                    style:
                        TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                const SizedBox(height: 8),
                const Text('مبلغ: 150,000 تومان'),
                const SizedBox(height: 12),
                FPrimaryButton('دانلود PDF', onTap: () {
                  // TODO: download invoice
                })
              ],
            ),
          ),
        ),
      );
}

// Offers / discounts – new
class OffersScreen extends StatelessWidget {
  const OffersScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('تخفیف‌ها'),
          backgroundColor: FCol.ink,
        ),
        body: ListView(
          children: List.generate(
            3,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.local_offer),
                title: Text('کد تخفیف $i'),
                subtitle: const Text('شرح تخفیف...'),
                trailing: ElevatedButton(
                  onPressed: () {
                    // TODO: validate discount
                  },
                  child: const Text('اعتبار'),
                ),
              ),
            ),
          ),
        ),
      );
}

// Feedback / complaint – new
class FeedbackScreen extends StatelessWidget {
  const FeedbackScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('بازخورد / شکایت'),
          backgroundColor: FCol.ink,
        ),
        body: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            children: [
              const TextField(
                maxLines: 5,
                decoration: InputDecoration(
                    labelText: 'متن بازخورد', border: OutlineInputBorder()),
              ),
              const SizedBox(height: 12),
              FPrimaryButton('ارسال', onTap: () {
                // TODO: POST /api/client-feedback
              })
            ],
          ),
        ),
      );
}

// Profile – restyle
class ProfileScreen extends StatelessWidget {
  const ProfileScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('پروفایل'), backgroundColor: FCol.ink),
        body: const Center(child: Text('اطلاعات کاربر')),
      );
}

// Notifications – restyle
class NotificationsScreen extends StatelessWidget {
  const NotificationsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('اعلان‌ها'), backgroundColor: FCol.ink),
        body: ListView(
          children: List.generate(
            4,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.notification_important),
                title: Text('اعلان $i'),
                subtitle: const Text('متن اعلان...'),
              ),
            ),
          ),
        ),
      );
}

// --------------------------
// C) Artist screens
// --------------------------

// Artist dashboard – restyle
class ArtistDashboardScreen extends StatelessWidget {
  const ArtistDashboardScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('داشبورد'), backgroundColor: FCol.olive),
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

// Artist schedule – restyle
class ArtistScheduleScreen extends StatelessWidget {
  const ArtistScheduleScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(title: const Text('تقویم'), backgroundColor: FCol.olive),
        body: ListView(
          children: List.generate(
            5,
            (i) => ListTile(
              leading: const Icon(Icons.schedule),
              title: Text('روز $i'),
              subtitle: const Text('ساعات کاری...'),
            ),
          ),
        ),
      );
}

// Appointments + check‑in – new
class ArtistAppointmentsScreen extends StatelessWidget {
  const ArtistAppointmentsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('نوبت‌ها'), backgroundColor: FCol.olive),
        body: ListView(
          children: List.generate(
            3,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.event_available),
                title: Text('نوبت $i'),
                trailing: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      icon: const Icon(Icons.check),
                      onPressed: () {
                        // TODO: check‑in
                      },
                    ),
                    IconButton(
                      icon: const Icon(Icons.close),
                      onPressed: () {
                        // TODO: cancel / complete
                      },
                    )
                  ],
                ),
              ),
            ),
          ),
        ),
      );
}

// Leave request – new
class LeaveRequestScreen extends StatelessWidget {
  const LeaveRequestScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('درخواست مرخصی'), backgroundColor: FCol.olive),
        body: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            children: [
              const TextField(
                decoration: InputDecoration(labelText: 'دلیل مرخصی'),
              ),
              const SizedBox(height: 8),
              const TextField(
                decoration: InputDecoration(labelText: 'تاریخ شروع'),
              ),
              const SizedBox(height: 8),
              const TextField(
                decoration: InputDecoration(labelText: 'تاریخ پایان'),
              ),
              const SizedBox(height: 12),
              FPrimaryButton('ارسال', onTap: () {
                // TODO: POST /api/leaves/my
              })
            ],
          ),
        ),
      );
}

// My clients + notes – new
class ClientNotesScreen extends StatelessWidget {
  const ClientNotesScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('یادداشت‌های مشتری'),
            backgroundColor: FCol.olive),
        body: ListView(
          children: List.generate(
            4,
            (i) => FCard(
              child: ListTile(
                leading: const Icon(Icons.person),
                title: Text('مشتری $i'),
                subtitle: const Text('یادداشت‌های برنامه‌ریزی...'),
                onTap: () {
                  // TODO: edit notes
                },
              ),
            ),
          ),
        ),
      );
}

// Product usage – new
class ProductUsageScreen extends StatelessWidget {
  const ProductUsageScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('مصرف محصولات'), backgroundColor: FCol.olive),
        body: const Center(child: Text('لیست محصولات مصرفی')),
      );
}

// Staff requests – new
class StaffRequestsScreen extends StatelessWidget {
  const StaffRequestsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('درخواست‌های پرسنل'),
            backgroundColor: FCol.olive),
        body: const Center(child: Text('لیست درخواست‌ها')),
      );
}

// My contracts – new
class MyContractsScreen extends StatelessWidget {
  const MyContractsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('قراردادها'), backgroundColor: FCol.olive),
        body: const Center(child: Text('نمایش قراردادها')),
      );
}

// Notices / instructions – new
class NoticesScreen extends StatelessWidget {
  const NoticesScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('اطلاعیه‌ها'), backgroundColor: FCol.olive),
        body: const Center(child: Text('متن اطلاعیه‌ها')),
      );
}

// --------------------------
// D) SalonManager screens
// --------------------------

// Manager dashboard – restyle
class ManagerDashboardScreen extends StatelessWidget {
  const ManagerDashboardScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('داشبورد مدیر'), backgroundColor: Colors.indigo),
        body: GridView.count(
          crossAxisCount: 2,
          padding: const EdgeInsets.all(12),
          children: const [
            FStat('12', 'سالن‌ها'),
            FStat('350', 'پرسنل'),
            FStat('120k', 'درآمد'),
            FStat('5', 'درخواست مرخصی')
          ],
        ),
      );
}

// Salon profile / amenities / notices – new
class ManagerSalonProfileScreen extends StatelessWidget {
  const ManagerSalonProfileScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('پروفایل سالن'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('فرم ویرایش اطلاعات سالن')),
      );
}

// Working hours / closures – new
class WorkingHoursScreen extends StatelessWidget {
  const WorkingHoursScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('ساعات کاری'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('تنظیم ساعات کاری و تعطیلات')),
      );
}

// Services management – new
class ServicesManagementScreen extends StatelessWidget {
  const ServicesManagementScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('مدیریت خدمات'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('لیست و ویرایش خدمات')),
      );
}

// Staff + contracts – restyle
class StaffManagementScreen extends StatelessWidget {
  const StaffManagementScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('پرسنل'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('مدیریت پرسنل و قراردادها')),
      );
}

// Appointments view/confirm – restyle
class ManagerAppointmentsScreen extends StatelessWidget {
  const ManagerAppointmentsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('نوبت‌ها'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('لیست نوبت‌ها برای مدیریت')),
      );
}

// Customers + reviews – new
class CustomersReviewsScreen extends StatelessWidget {
  const CustomersReviewsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('مشتریان و نظرات'),
            backgroundColor: Colors.indigo),
        body: const Center(child: Text('آمار مشتریان و نظرات')),
      );
}

// Discounts – new
class ManagerDiscountsScreen extends StatelessWidget {
  const ManagerDiscountsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('تخفیف‌ها'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('مدیریت تخفیف‌ها')),
      );
}

// Finance ledger – new
class FinanceLedgerScreen extends StatelessWidget {
  const FinanceLedgerScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('حسابداری'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('نمایش سندهای مالی')),
      );
}

// Hiring – new
class HiringScreen extends StatelessWidget {
  const HiringScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('استخدام'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('پست‌ها و درخواست‌های استخدام')),
      );
}

// Inbox – new
class ManagerInboxScreen extends StatelessWidget {
  const ManagerInboxScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('صندوق پیام'), backgroundColor: Colors.indigo),
        body: const Center(child: Text('درخواست‌ها، نظرات و پیام‌ها')),
      );
}

// --------------------------
// E) SuperAdmin screens
// --------------------------

// Admin dashboard – restyle
class AdminDashboardScreen extends StatelessWidget {
  const AdminDashboardScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('داشبورد ادمین'), backgroundColor: Colors.amber),
        body: GridView.count(
          crossAxisCount: 2,
          padding: const EdgeInsets.all(12),
          children: const [
            FStat('5', 'مجموع کاربران'),
            FStat('120', 'سالن‌ها'),
            FStat('200k', 'درآمد کل'),
            FStat('12', 'درخواست‌های فعال')
          ],
        ),
      );
}

// Tenants / salons – restyle
class TenantsScreen extends StatelessWidget {
  const TenantsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('مستاجرین'), backgroundColor: Colors.amber),
        body: const Center(child: Text('لیست مستاجرین و سالن‌ها')),
      );
}

// Users – restyle
class UsersScreen extends StatelessWidget {
  const UsersScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('کاربران'), backgroundColor: Colors.amber),
        body: const Center(child: Text('مدیریت کاربران')),
      );
}

// Service templates – new
class ServiceTemplatesScreen extends StatelessWidget {
  const ServiceTemplatesScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('قالب‌های سرویس'), backgroundColor: Colors.amber),
        body: const Center(child: Text('ایجاد/ویرایش قالب سرویس')),
      );
}

// Package listings – new
class PackageListingsScreen extends StatelessWidget {
  const PackageListingsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('بسته‌های خدمات'), backgroundColor: Colors.amber),
        body: const Center(child: Text('لیست بسته‌ها')),
      );
}

// Homepage CMS – new
class HomepageCMSScreen extends StatelessWidget {
  const HomepageCMSScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('مدیریت محتوا'), backgroundColor: Colors.amber),
        body: const Center(child: Text('اسلایدها و منوها')),
      );
}

// Blog / news editor – new
class BlogEditorScreen extends StatelessWidget {
  const BlogEditorScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('ویرایش بلاگ'), backgroundColor: Colors.amber),
        body: const Center(child: Text('ویرایش پست‌های بلاگ')),
      );
}

// Placements (VIP/ladder) – new
class PlacementsScreen extends StatelessWidget {
  const PlacementsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar:
            AppBar(title: const Text('پست‌ها'), backgroundColor: Colors.amber),
        body: const Center(child: Text('مدیریت جایگاه‌ها')),
      );
}

// Join requests – new
class AdminJoinRequestsScreen extends StatelessWidget {
  const AdminJoinRequestsScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('درخواست‌های پیوستن'),
            backgroundColor: Colors.amber),
        body: const Center(child: Text('مشاهده و تایید درخواست‌ها')),
      );
}

// Platform accounting – new
class PlatformAccountingScreen extends StatelessWidget {
  const PlatformAccountingScreen({Key? key}) : super(key: key);
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
            title: const Text('حسابداری پلتفرم'),
            backgroundColor: Colors.amber),
        body: const Center(child: Text('آمار مالی پلتفرم')),
      );
}

// ----------------------------------------------------------------
// END OF GENERATED SKELETONS
// ----------------------------------------------------------------
