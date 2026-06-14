using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class PackagesModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<ServicePackage> Packages { get; set; } = new();
        public List<SalonPackageSubscription> ActiveSubscriptions { get; set; } = new();
        public string? Message { get; set; }

        public PackagesModel(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<SmartSalon.Models.Salon?> GetManagerSalonAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return null;
            return await _db.Salons.FirstOrDefaultAsync(s => s.ManagerId == token);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();

            Packages = await _db.ServicePackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();

            if (salon != null)
            {
                ActiveSubscriptions = await _db.SalonPackageSubscriptions
                    .Include(s => s.Package)
                    .Where(s => s.SalonId == salon.Id && s.EndDate >= DateTime.Now)
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBuyAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null)
            {
                Message = "❌ سالنی یافت نشد";
                return await OnGetAsync() as PageResult ?? Page();
            }

            var package = await _db.ServicePackages.FindAsync(id);
            if (package == null)
            {
                Message = "❌ پکیج یافت نشد";
                return await OnGetAsync() as PageResult ?? Page();
            }

            // چک کنیم قبلاً خریده نشده باشد
            var existing = await _db.SalonPackageSubscriptions
                .FirstOrDefaultAsync(s => s.SalonId == salon.Id
                                       && s.PackageId == id
                                       && s.EndDate >= DateTime.Now);
            if (existing != null)
            {
                Message = "⚠️ این پکیج قبلاً خریداری شده و هنوز فعال است";
                return await OnGetAsync() as PageResult ?? Page();
            }

            // اضافه کردن خدمات پکیج به سالن
            await AddPackageServicesAsync(salon.Id, package);

            // ثبت اشتراک
            _db.SalonPackageSubscriptions.Add(new SalonPackageSubscription
            {
                SalonId = salon.Id,
                PackageId = id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(package.DurationMonths),
                PaidAmount = package.Price
            });

            await _db.SaveChangesAsync();

            Message = $"✅ پکیج «{package.Name}» با موفقیت خریداری شد و خدمات آن به سالن اضافه شد";
            return await OnGetAsync() as PageResult ?? Page();
        }

        public async Task<IActionResult> OnPostRenewAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null) return RedirectToPage("/Admin/Login");

            var package = await _db.ServicePackages.FindAsync(id);
            if (package == null)
            {
                Message = "❌ پکیج یافت نشد";
                return await OnGetAsync() as PageResult ?? Page();
            }

            var existing = await _db.SalonPackageSubscriptions
                .FirstOrDefaultAsync(s => s.SalonId == salon.Id && s.PackageId == id);

            if (existing != null)
            {
                // تمدید از تاریخ انقضا
                var baseDate = existing.EndDate >= DateTime.Now
                    ? existing.EndDate
                    : DateTime.Now;
                existing.EndDate = baseDate.AddMonths(package.DurationMonths);
                existing.PaidAmount += package.Price;
            }
            else
            {
                _db.SalonPackageSubscriptions.Add(new SalonPackageSubscription
                {
                    SalonId = salon.Id,
                    PackageId = id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(package.DurationMonths),
                    PaidAmount = package.Price
                });
            }

            await _db.SaveChangesAsync();

            Message = $"✅ پکیج «{package.Name}» با موفقیت تمدید شد";
            return await OnGetAsync() as PageResult ?? Page();
        }

        private async Task AddPackageServicesAsync(int salonId, ServicePackage package)
        {
            // خدمات هر پکیج
            var servicesByPackage = new Dictionary<string, List<(string name, int duration, decimal price)>>
            {
                ["مو"] = new()
                {
                    ("کوتاهی مو زنانه", 45, 200000), ("کوتاهی کودک", 30, 100000),
                    ("اصلاح چتری", 15, 80000), ("براشینگ", 60, 300000),
                    ("سشوار حرفه‌ای", 45, 250000), ("اتو مو", 60, 280000),
                    ("شینیون ساده", 90, 500000), ("شینیون عروس", 180, 1500000),
                    ("بافت مو", 60, 400000), ("اکستنشن مو", 120, 800000)
                },
                ["رنگ"] = new()
                {
                    ("رنگ ریشه", 60, 400000), ("رنگ کامل", 90, 600000),
                    ("مش", 120, 800000), ("هایلایت", 150, 1000000),
                    ("آمبره", 180, 1200000), ("سامبره", 180, 1200000),
                    ("بالیاژ", 180, 1500000), ("دکلره", 90, 700000),
                    ("ریموو رنگ", 60, 500000), ("تونر مو", 30, 200000)
                },
                ["احیا"] = new()
                {
                    ("کراتینه", 180, 1500000), ("پروتئین تراپی", 120, 800000),
                    ("بوتاکس مو", 120, 1000000), ("پلکس تراپی", 90, 700000),
                    ("صافی ژاپنی", 240, 2000000), ("ویتامینه مو", 60, 400000),
                    ("آبرسانی مو", 60, 350000)
                },
                ["ناخن"] = new()
                {
                    ("مانیکور", 45, 200000), ("پدیکور", 60, 250000),
                    ("کاشت ناخن", 90, 500000), ("ترمیم ناخن", 30, 150000),
                    ("ژلیش", 60, 350000), ("طراحی ناخن", 30, 200000),
                    ("لمینت ناخن", 60, 400000), ("پارافین تراپی دست", 30, 180000)
                },
                ["ابرو"] = new()
                {
                    ("اصلاح ابرو", 15, 80000), ("رنگ ابرو", 30, 150000),
                    ("لیفت ابرو", 60, 400000), ("لمینت ابرو", 60, 350000),
                    ("اکستنشن مژه", 90, 600000), ("لیفت مژه", 60, 450000),
                    ("لمینت مژه", 60, 400000), ("ریموو اکستنشن", 30, 200000)
                },
                ["پوست"] = new()
                {
                    ("فیشیال", 90, 500000), ("پاکسازی پوست", 60, 350000),
                    ("بخور صورت", 30, 150000), ("اسکراب", 45, 250000),
                    ("آبرسانی پوست", 60, 300000), ("ماساژ صورت", 45, 280000),
                    ("ماسک صورت", 30, 200000), ("میکرودرم", 60, 600000)
                },
                ["میکاپ"] = new()
                {
                    ("میکاپ ساده", 60, 400000), ("میکاپ حرفه‌ای", 90, 700000),
                    ("گریم عروس", 180, 2000000), ("شینیون عروس", 180, 1500000),
                    ("میکاپ نامزدی", 120, 1000000), ("میکاپ فرمالیته", 90, 800000)
                },
                ["اپیلاسیون"] = new()
                {
                    ("اصلاح صورت", 15, 80000), ("اصلاح بدن", 30, 150000),
                    ("وکس صورت", 20, 120000), ("وکس بدن", 60, 300000),
                    ("اپیلاسیون کامل", 90, 500000), ("اپیلاسیون موضعی", 30, 200000)
                },
                ["عروس"] = new()
                {
                    ("پکیج عروس", 360, 5000000), ("مشاوره عروس", 60, 200000),
                    ("تست میکاپ", 90, 500000), ("تست شینیون", 90, 500000),
                    ("خدمات ویژه روز مراسم", 480, 8000000)
                },
                ["اسپا"] = new()
                {
                    ("ماساژ ریلکسی", 60, 400000), ("ماساژ صورت", 45, 280000),
                    ("اسپا پا", 45, 250000), ("اسپا دست", 30, 200000)
                }
            };

            // اگر پکیج کامل بود همه را اضافه کن
            var categoriesToAdd = package.Category == "کامل"
                ? servicesByPackage.Keys.ToList()
                : new List<string> { package.Category };

            foreach (var category in categoriesToAdd)
            {
                if (!servicesByPackage.ContainsKey(category)) continue;

                foreach (var (name, duration, price) in servicesByPackage[category])
                {
                    // اگر قبلاً وجود دارد رد کن
                    var exists = await _db.SalonServices
                        .AnyAsync(s => s.SalonId == salonId && s.Name == name);

                    if (!exists)
                    {
                        _db.SalonServices.Add(new SalonService
                        {
                            SalonId = salonId,
                            Name = name,
                            Category = category,
                            BaseDurationMinutes = duration,
                            BasePrice = price,
                            IsActive = false // غیرفعال تا مدیر فعال کند
                        });
                    }
                    else
                    {
                        // فعال کردن خدمت موجود
                        var existing = await _db.SalonServices
                            .FirstOrDefaultAsync(s => s.SalonId == salonId && s.Name == name);
                        if (existing != null) existing.IsActive = true;
                    }
                }
            }
        }
    }
}