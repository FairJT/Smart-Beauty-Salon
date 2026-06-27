# ۰۳ — فارسی‌کردن صفحه‌ی خانه 🟡

صفحه‌ی `/home` فعلی (`generated/home_screen.dart`) هنوز placeholder انگلیسی داره.
این یه استاپ‌گپه تا متن انگلیسی دیده نشه؛ این صفحه در نهایت باید به دیتای واقعی
سالن‌ها وصل بشه (الان داده‌ی ساختگی نشون می‌ده).

**فایل:** `smart_salon_app/lib/presentation/pages/generated/home_screen.dart`

**۱) پیدا کن:** `Text('Featured Salon',` → **جایگزین:** `Text('سالن منتخب',`

**۲) پیدا کن:** `Text('Lorem ipsum dolor sit amet...'),`
**جایگزین:** `Text('توضیحات سالن در اینجا نمایش داده می‌شود.'),`

**۳) پیدا کن:** `title: Text('Salon $i'),` → **جایگزین:** `title: Text('سالن $i'),`

**۴) پیدا کن:** `subtitle: Text('Address $i'),` → **جایگزین:** `subtitle: Text('آدرس $i'),`

**تأیید (PowerShell):**
```powershell
Select-String -Path smart_salon_app/lib/presentation/pages/generated/home_screen.dart -Pattern "Salon|Lorem|Address|Featured"
# انتظار: هیچ نتیجه‌ای برنگرده
```

> نکته: این صفحه از پالت زیتونی (`FCol.olive`) استفاده می‌کنه نه سرمه‌ای برند.
> در ادامه بهتره به یه صفحه‌ی واقعی مرور سالن‌ها با پالت `AppColors` تبدیل بشه — STOP و گزارش بده اگه خواستی این کارو شروع کنی.

**تمام وقتی که:** هیچ متن انگلیسی توی صفحه‌ی خانه دیده نشه.
