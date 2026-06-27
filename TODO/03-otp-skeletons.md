# 03 — (optional) Remove the OTP class from skeletons 🟢

`all_screens_skeletons.dart` defines its OWN dead `OtpScreen` class. It's self-contained, so it does
NOT break when `otp_screen.dart` is deleted. Removing it is optional cleanup.

**File:** `smart_salon_app/lib/presentation/pages/all_screens_skeletons.dart`
Delete the entire `class OtpScreen extends StatelessWidget { ... }` block (from `class OtpScreen`
to its matching closing `}`).

**Done when:** `flutter analyze` is clean. If unsure where the class ends, SKIP this task — it's harmless.
