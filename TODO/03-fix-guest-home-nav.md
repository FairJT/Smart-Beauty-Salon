# ۰۳ — فعال‌کردن نوار پایینِ صفحه‌ی مهمان 🟡

صفحه‌ی `/home` (مهمان) هنوز stub است و نوار پایینش `onTap: (_) {}` خالی دارد.
حداقل کاری کن که تب‌ها به مسیرهای واقعی بروند. (محتوای این صفحه placeholder است و
در آینده باید به مرور واقعی سالن‌ها وصل شود — این فقط ناوبری را زنده می‌کند.)

**فایل:** `smart_salon_app/lib/presentation/pages/generated/home_screen.dart`

**پیدا کن (دقیق):**
```dart
        bottomNavigationBar: FBottomNav(index: 0, onTap: (_) {}, items: const [
```
**جایگزین کن با:**
```dart
        bottomNavigationBar: FBottomNav(index: 0, onTap: (i) {
          if (i == 0) return;
          Navigator.of(context).pushReplacementNamed(
            i == 3 ? '/profile' : '/login',
          );
        }, items: const [
```

> منطق ساده: تب «خانه» می‌ماند؛ «پروفایل» به `/profile` می‌رود؛ بقیه (رزرو/نوبت‌ها) چون
> نیاز به ورود دارند به `/login` می‌روند. اگر بعداً مسیر مرور سالن یا رزرو مهمان آماده شد،
> همین‌جا به آن مسیر وصلش کن.

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/generated/home_screen.dart -Pattern "onTap: \(i\)"
cd smart_salon_app ; flutter analyze
```

**تمام وقتی که:** زدن تب‌های نوار پایینِ مهمان دیگر بی‌اثر نباشد.
