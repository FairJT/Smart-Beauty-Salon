# گزارش بهینه‌سازی دیتابیس — SalonOS

> بررسیِ کلِ schema بعد از پیاده‌سازیِ Manager + Artist + Client (روی `bb44ced`).
> دیتابیس: SQL Server. اکثر entityهای جدید در `AppDbContext` (که فقط `TenantId` را خودکار ایندکس می‌کند).
> توجه: batchِ SuperAdmin هنوز push نشده؛ جدول‌های سراسریِ آن جدا یادداشت شده‌اند.

---

## ۱. یافته‌های اصلی (به‌ترتیب اثر)

### 🔴 الف) ایندکس‌های گمشده روی ستون‌های کلیدی
`AppDbContext` برای هر `TenantEntity` فقط روی `TenantId` ایندکس می‌سازد. اما این entityها ستون‌های
`Guid` دارند که مستقیماً در `WHERE`/فیلتر استفاده می‌شوند و **ایندکس ندارند** → جدول‌اسکن:

| جدول | ستون‌های نیازمند ایندکس | کوئریِ نمونه |
|---|---|---|
| ClientNote | (ArtistId, ClientId) | یادداشت‌های من برای این مشتری |
| ProductUsage | BookingId, ArtistId, InventoryItemId | محصولاتِ این رزرو |
| StaffServiceContract | ArtistId, CatalogServiceId | قراردادهای این پرسنل |
| RescheduleRequest | BookingId, ArtistId | درخواست‌های جابجایی |
| StaffRequest / ArtistContract / ArtistLeave | ArtistId | موارد این پرسنل |
| JobApplication | JobPostingId | درخواست‌های این آگهی |
| ClientFeedback | ClientId | شکایات این مشتری |
| FinancialTransaction | CounterpartyUserId | حقوقِ این فرد |
| Discount | Code | پیداکردنِ کدِ تخفیف |

### 🔴 ب) همهٔ رشته‌ها `nvarchar(max)` هستند
هیچ‌کدام از string‌های entityهای جدید `MaxLength` ندارند → EF آن‌ها را `nvarchar(max)` می‌سازد.
مشکل: `nvarchar(max)` **قابل‌ایندکس نیست**، فضای بیشتری می‌گیرد، و کندتر است. ستون‌هایی که باید
کوتاه (و قابل‌ایندکس) شوند:
- `Discount.Code` (لازم است ایندکس شود → باید کوتاه شود) — مثلاً `nvarchar(64)`.
- شناسه‌های کاربری (`ClientId`, `ApplicantUserId`, `CounterpartyUserId`, `TargetClientId`) → `nvarchar(450)`
  (هم‌اندازهٔ کلیدِ AspNetUsers تا قابل‌ایندکس باشد).
- عنوان/نام (`Title`, `Name`) → `nvarchar(200)` / `nvarchar(100)`.
- زمان‌های کاری (`OpenTime`, `CloseTime` = "HH:mm") → `nvarchar(5)`.
- متن‌های آزاد (`Body`, `Detail`, `Note`, `Terms`, `Reason`) می‌توانند `nvarchar(max)` بمانند.

### 🟡 ج) سه جدولِ tenant هنوز زیر RLS نیستند
`Memberships`، `ArtistProfiles`، `SalonManagerProfiles` ستون `TenantId` دارند ولی در policyِ RLS
نیستند (تسکِ قبلیِ A2 اعمال نشده بود). دادهٔ tenant بدون backstopِ سطحِ دیتابیس.

### 🟢 د) دقتِ اعشار
چند entity انبار `decimal` خام دارند (EF پیش‌فرض `decimal(18,2)` می‌دهد + هشدار). `ProductUsage.Quantity`
درست تنظیم شده. این صرفاً پاکیزگیِ schema است، نه باگ.

---

## ۲. نقاطِ خوب (دست‌نخورده بماند)
- `Tenant.Slug` یکتا و ایندکس‌شده ✅ (پایهٔ صفحات عمومی).
- `Booking (ArtistId, StartsAt)` ایندکسِ یکتای فیلترشده ✅ (ضدِ double-booking).
- `OutboxMessage` ایندکس‌گذاریِ clustered حساب‌شده ✅.
- پوششِ RLS برای اکثر جدول‌های جدید کامل است (ClientNotes, ProductUsages, Discounts, …) ✅.
- پولِ صحیح به‌صورت `Money` (Amount عددِ صحیح + Currency)، نه float ✅.

---

## ۳. تسک‌های اصلاحی (در `agent-tasks-db/`)
- **DB1** — افزودنِ ایندکس‌های FK + `MaxLength`های کلیدی (یک بلوک config + یک migration).
- **DB2** — افزودنِ `Memberships`/`ArtistProfiles`/`SalonManagerProfiles` به RLS.
- **DB3** — (اختیاری) دقتِ اعشارِ جدول‌های انبار.

---

## ۴. مؤجل (وقتِ نشستنِ SuperAdmin)
وقتی batchِ SuperAdmin push شد، جدول‌های سراسری هم نیاز دارند:
- `BlogPost.Slug` → ایندکس + `nvarchar(200)`؛ `BlogPost (Type, IsPublished)`.
- `SalonPlacement (IsActive, EndsAt)` و `(SalonTenantId)`.
- `HomepageSlide (IsActive, SortOrder)`، `HomepageMenu (Location, SortOrder)`.
- `SalonJoinRequest (Status)`.
این‌ها بعد از پیاده‌سازیِ S1–S5 به‌صورت یک batchِ کوچک اضافه می‌شوند.

---

## ۵. توصیه‌های معماری (بلندمدت، نه تسکِ فوری)
- **ایندکس‌های مرکب برای کوئریِ داغ**: جایی که همیشه `TenantId + X` فیلتر می‌شود (مثل
  `Booking (TenantId, StartsAt)`)، یک ایندکسِ مرکب بهتر از ایندکسِ تک‌ستونیِ `TenantId` است.
- **بایگانیِ نرم‌حذف‌شده‌ها**: ردیف‌های `IsDeleted=1` با رشدِ داده انباشته می‌شوند؛ یک filtered index
  روی `WHERE IsDeleted = 0` کوئری‌های فعال را سریع‌تر می‌کند.
- **قراردادِ واحدِ پول**: تثبیتِ «همیشه ریال» (کارِ طراحیِ جداگانه) تا اعشار/تبدیل ثابت بماند.
