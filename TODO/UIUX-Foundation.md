# UI/UX — طرحِ پایه، ناوبری، و فهرستِ صفحات (فاز ۱)

> پایهٔ طراحیِ Flutter برای SalonOS. بک‌اندِ ۵ نقش کامل است؛ این سند نقشهٔ ساختِ UI است.
> فاز ۱ (این سند): design system + IA هر نقش + screen inventory. فاز ۲: صفحه‌به‌صفحه تسک.

---

## ۰. وضعیت فعلی

**هست:** ~۲۰ صفحه (auth: splash/login/register/otp؛ رزرو: booking/guest_booking/appointment_list؛
مرور: salon list/detail؛ داشبوردهای پایه: admin/artist/manager؛ client_home؛ profile؛ notifications).
مسیریابیِ نقش از splash/login کار می‌کند. RTL + شمسی + تومان + ارقام فارسی آماده‌اند.

**نیست:** UI برای **هیچ‌کدام** از feature‌های جدیدِ این چند هفته — مدیر (امکانات، اعلانات، تایم/تعطیلی،
قرارداد، مالی، تخفیف، استخدام، insights)، آرتیست (مرخصی، یادداشت، درخواست، check-in، جابجایی، مصرف محصول،
قرارداد)، مشتری (فاکتور، تخفیف، شکایت)، و SuperAdmin (CMS، بلاگ، نردبان، join requests، حسابداری).

---

## ۱. Design System

موجود (نگه‌دار و استفاده کن): `AppSpacing` (فاصله/شعاع)، `AppColors` (navy `#1B3A5C` + gold ادمین)،
Material3، `Vazirmatn`، `AppTextTheme.farsi`، تمِ دکمه/کارت/NavigationBar/SnackBar.

**کنوانسیون‌های الزامی (همه‌جا):**
- **RTL** پیش‌فرض؛ هیچ متنِ چپ‌چینِ دستی.
- **تاریخ شمسی** برای نمایش (از `core/format/jalaali_helper`)؛ **تومان + ارقام فارسی** برای پول
  (از `core/format/money_formatter`).
- هر صفحه سه حالت دارد: **loading / empty / error** (نه صفحهٔ سفید).
- فرم‌ها ساده، یک‌ستونه، دکمهٔ اصلی پایین و چسبیده.

**رنگِ accent به‌ازای نقش** (تا کاربر بداند کجاست): Client = navy، Artist = teal، Manager = indigo،
SuperAdmin = gold، Public = navy. (همه روی همان پایهٔ `AppColors`.)

**کامپوننت‌های مشترکی که باید ساخته شوند (یک‌بار، استفادهٔ همه‌جا):**
- `RoleScaffold` — شلِ هر نقش: AppBar + `NavigationBar` پایین (تب‌ها) + accent نقش.
- `SectionCard`, `StatTile`, `StatusChip` (وضعیت رزرو/درخواست با رنگ)، `MoneyText` (تومان)،
  `JalaliDateField` / `JalaliDatePicker`، `EmptyState`, `LoadingView`, `ErrorView`,
  `AppTextField`, `PrimaryButton`. اینها زیربنای همهٔ صفحات‌اند → **اولین batch**.

---

## ۲. معماریِ اطلاعات و ناوبریِ هر نقش

### Public / Guest (بدون لاگین) — accent navy
صفحهٔ اول (اسلایدر + سالن‌های ویژه/برتر + تیزرِ بلاگ) · جستجو/لیست سالن · صفحهٔ سالن ·
صفحهٔ آرتیست · بلاگ/خبر · فرمِ «سالن خود را ثبت کنید» · ورود/ثبت‌نام.

### Client — تب‌های پایین: خانه · رزرو · نوبت‌های من · پروفایل
خانه (نوبتِ پیش‌رو + رزروِ سریع) · جریانِ رزرو · نوبت‌های من (لغو/تغییر) · سابقهٔ خدمات ·
فاکتور · تخفیف‌ها/کد · ثبت نظر · پیشنهاد/شکایت · پروفایل · اعلان‌ها.

### Artist — تب‌ها: امروز · برنامه · مشتری‌ها · بیشتر
امروز (تعداد + نوبت‌های بعدی) · برنامه/شیفت · نوبت‌ها (check-in/اتمام/لغو/درخواستِ جابجایی) ·
مشتری‌های من + یادداشت · مصرف محصول · مرخصی · درخواست (مشکل/تجهیزات) · قراردادِ من · اعلانات/دستورالعمل.

### SalonManager — تب‌ها: داشبورد · سالن · پرسنل · مالی · بیشتر
داشبورد · پروفایلِ سالن (موقعیت/امکانات/اعلانات/تایم/تعطیلی) · خدمات (پدر-فرزندی) · پرسنل + قرارداد ·
نوبت‌ها (تأیید) · مشتری‌ها + نظرات · تخفیف‌ها · مالی (دفتر) · استخدام · صندوقِ ورودی
(درخواست‌های پرسنل + جابجایی + شکایتِ مشتری + تأیید مرخصی).

### SuperAdmin — تب‌ها: داشبورد · پلتفرم · محتوا · مالی
داشبورد/آمار · سالن‌ها/کاربران · service templates · فروشِ پنل · صفحهٔ اول (اسلایدر/منو) ·
بلاگ/خبر · نردبان/VIP · درخواست‌های پیوستن · حسابداریِ پلتفرم.

---

## ۳. فهرستِ صفحات (✅ موجود · 🆕 لازم) → endpoint

### Client
| صفحه | endpoint | وضعیت |
|---|---|---|
| خانه/رزرو/نوبت‌ها/پروفایل | appointments, salons, me | ✅ (بهبود) |
| فاکتور | `GET /api/invoices/{id}` | 🆕 |
| تخفیف‌ها + اعتبارسنجی کد | `GET /api/offers/discounts(/validate)` | 🆕 |
| پیشنهاد/شکایت | `POST /api/client-feedback` | 🆕 |

### Artist
| صفحه | endpoint | وضعیت |
|---|---|---|
| امروز/برنامه | `/api/artist-schedule/my(/stats)` | ✅ |
| نوبت‌ها + check-in/جابجایی | `/api/artist-visit/*` | 🆕 |
| مرخصی | `POST /api/leaves/my` | 🆕 |
| یادداشتِ مشتری | `/api/client-notes` | 🆕 |
| مصرف محصول | `/api/product-usage` | 🆕 |
| درخواست (مشکل/تجهیزات) | `/api/staff-requests` | 🆕 |
| قراردادِ من | `GET /api/staff-contracts/my` | 🆕 |
| اعلانات | `GET /api/salon/notices` | 🆕 |

### SalonManager
| صفحه | endpoint | وضعیت |
|---|---|---|
| پروفایلِ سالن + امکانات + اعلانات | salons, `/api/salon/amenities`, `/api/salon/notices` | 🆕 |
| تایم کاری + تعطیلی | `/api/salon/working-hours`, `/api/salon/closures` | 🆕 |
| خدمات (پدر-فرزندی) | `/api/catalog-services` | 🆕 |
| پرسنل + قرارداد | artists, `/api/staff-contracts` | 🆕 (بهبود) |
| تخفیف‌ها | `/api/salon/discounts` | 🆕 |
| مالی (دفتر) | `/api/salon/finance` | 🆕 |
| استخدام | `/api/salon/hiring/*` | 🆕 |
| نظرات/مشتری‌ها | `/api/salon/insights/*` | 🆕 |
| صندوق: درخواست‌ها/جابجایی/شکایت | staff-requests, artist-visit, client-feedback | 🆕 |

### SuperAdmin
| صفحه | endpoint | وضعیت |
|---|---|---|
| آمار/سالن‌ها/کاربران | `/api/admin/*` | ✅ (بهبود) |
| service templates / فروش پنل | `/api/service-templates`, `/api/package-listings` | 🆕 |
| صفحهٔ اول (اسلایدر/منو) | `/api/homepage/*` | 🆕 |
| بلاگ/خبر | `/api/blog` | 🆕 |
| نردبان/VIP | `/api/placements` | 🆕 |
| درخواست‌های پیوستن | `/api/join-requests` | 🆕 |
| حسابداری پلتفرم | `/api/admin/accounting/overview` | 🆕 |

### Public
صفحهٔ اول (`/api/homepage/*`, `/api/placements/active`) · بلاگ (`/api/blog`) ·
ثبت سالن (`POST /api/join-requests`) · صفحهٔ آرتیست (عمومی) — همه 🆕/بهبود.

---

## ۴. ترتیبِ ساخت + روشِ کار

چون agent رایگان در Flutter ضعیف است، هر صفحه به‌صورت **یک تسک با کدِ تقریباً کاملِ Flutter** داده می‌شود
(provider + screen)، نه صرفاً توضیح.

ترتیبِ پیشنهادی (از پرارزش/پرکاربرد به کم‌کاربرد):
1. **UI-0 — کامپوننت‌های مشترک** (`RoleScaffold`, `StatusChip`, `MoneyText`, `JalaliDateField`,
   `EmptyState/Loading/Error`, `AppTextField`, `PrimaryButton`). زیربنای همه.
2. **Client** (مشتری = درآمدِ کسب‌وکار): فاکتور، تخفیف‌ها/کد، شکایت + بهبودِ رزرو/نوبت‌ها.
3. **Artist** (استفادهٔ روزمره): نوبت‌ها+check-in، مرخصی، یادداشت، درخواست، مصرف محصول، قرارداد، اعلانات.
4. **SalonManager** (بیشترین صفحه): سالن/امکانات/تایم، خدمات، قرارداد، تخفیف، مالی، استخدام، صندوق.
5. **SuperAdmin**: CMS، بلاگ، نردبان، join، حسابداری.
6. **Public**: صفحهٔ اول پویا + بلاگ + ثبت سالن.

هر مرحله یک batch جدا با تسک‌های صفحه‌به‌صفحه. اول UI-0، چون بقیه به آن تکیه می‌کنند.
