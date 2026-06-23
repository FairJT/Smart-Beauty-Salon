import 'package:flutter/material.dart';
import 'package:smart_salon_app/core/fresha/fresha_ui.dart';

class BookingFlowScreen extends StatefulWidget {
  const BookingFlowScreen({super.key});
  @override
  State<BookingFlowScreen> createState() => _BookingFlowScreenState();
}

class _BookingFlowScreenState extends State<BookingFlowScreen> {
  int _step = 0;
  @override
  Widget build(BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text('رزرو نوبت'),
          backgroundColor: FCol.ink,
        ),
        body: Stepper(
          currentStep: _step,
          onStepContinue: () => setState(() => _step = (_step + 1).clamp(0, 3)),
          onStepCancel: () => setState(() => _step = (_step - 1).clamp(0, 3)),
          steps: [
            Step(
                title: const Text('انتخاب آرتیست'),
                content: Wrap(
                  spacing: 8,
                  children: List.generate(
                    4,
                    (i) => FAvatar('A$i'),
                  ),
                )),
            Step(
                title: const Text('انتخاب زمان'),
                content: Wrap(
                  spacing: 8,
                  children: List.generate(
                    6,
                    (i) => FSlot('\${i + 9}:00',
                        state: FSlotState.free, onTap: () {}),
                  ),
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
