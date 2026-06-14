using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Controllers;

[Route("api/seed")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeedController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpPost("salons")]
    public async Task<IActionResult> SeedSalons()
    {
        if (await _db.Salons.AnyAsync())
            return Ok(new { message = "Salons already seeded", count = await _db.Salons.CountAsync() });

        var managerUser = await _userManager.FindByNameAsync("09110000002");
        if (managerUser == null)
            return BadRequest(new { message = "Run /api/auth/seed first to create users" });

        var managerId = managerUser.Id;

        // ── Create 8 artist users ─────────────────────────────────
        var artistData = new[]
        {
            ("09110000011", "مریم", "احمدی"),
            ("09110000012", "سارا", "محمدی"),
            ("09110000013", "زهرا", "حسینی"),
            ("09110000014", "نرگس", "رضایی"),
            ("09110000015", "لیلا", "موسوی"),
            ("09110000016", "فاطمه", "کریمی"),
            ("09110000017", "الناز", "جعفری"),
            ("09110000018", "شقایق", "صادقی"),
        };

        var artistUserIds = new List<string>();
        foreach (var (mobile, first, last) in artistData)
        {
            if (await _userManager.FindByNameAsync(mobile) != null) continue;
            var user = new ApplicationUser
            {
                UserName = mobile,
                PhoneNumber = mobile,
                FirstName = first,
                LastName = last,
                NationalCode = "1234567890",
                UserType = UserType.Artist,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, "Test@1234");
            if (result.Succeeded)
                artistUserIds.Add(user.Id);
        }

        // Also fetch any artist users that were already created
        foreach (var (mobile, _, _) in artistData)
        {
            var u = await _userManager.FindByNameAsync(mobile);
            if (u != null && !artistUserIds.Contains(u.Id))
                artistUserIds.Add(u.Id);
        }

        // ── 10 salons ─────────────────────────────────────────────
        var salons = new[]
        {
            new { Name = "آرایشگاه رویا", Slug = "roya-beauty", Phone = "02112345671", Address = "تهران، خیابان ولیعصر، پلاک ۱۲۳", Desc = "سالن زیبایی مدرن با بهترین خدمات", Color = "#8B5CF6", Vip = true, Lat = 35.721, Lon = 51.389 },
            new { Name = "سالن زیبایی الماس", Slug = "almas-beauty", Phone = "02112345672", Address = "شیراز، خیابان زند، پلاک ۴۵", Desc = "سالن تخصصی کاشت ناخن و زیبایی", Color = "#EC4899", Vip = true, Lat = 29.615, Lon = 52.541 },
            new { Name = "آرایشگاه مدرن", Slug = "modern-hair", Phone = "02112345673", Address = "اصفهان، خیابان چهارباغ، پلاک ۷۸", Desc = "کوتاهی و رنگ موی حرفه‌ای", Color = "#3B82F6", Vip = false, Lat = 32.654, Lon = 51.667 },
            new { Name = "سالن زیبایی نسیم", Slug = "nasim-beauty", Phone = "02112345674", Address = "مشهد، خیابان امام رضا، پلاک ۱۵", Desc = "خدمات زیبایی کامل با تیم مجرب", Color = "#10B981", Vip = true, Lat = 36.315, Lon = 59.567 },
            new { Name = "آرایشگاه پویا", Slug = "pouya-hair", Phone = "02112345675", Address = "تبریز، خیابان امام، پلاک ۳۴", Desc = "کوتاهی مردانه و زنانه با جدیدترین متد", Color = "#F59E0B", Vip = false, Lat = 38.079, Lon = 46.296 },
            new { Name = "سالن زیبایی سحر", Slug = "sahar-beauty", Phone = "02112345676", Address = "کرج، میدان آزادگان، پلاک ۸۸", Desc = "پاکسازی پوست و خدمات زیبایی", Color = "#8B5CF6", Vip = false, Lat = 35.817, Lon = 51.017 },
            new { Name = "آرایشگاه لوکس", Slug = "lux-hair", Phone = "02112345677", Address = "قم، خیابان هفت تیر، پلاک ۵۶", Desc = "بهترین خدمات کاشت مو و زیبایی", Color = "#EF4444", Vip = true, Lat = 34.641, Lon = 50.877 },
            new { Name = "سالن زیبایی ماه", Slug = "mah-beauty", Phone = "02112345678", Address = "رشت، خیابان مطهری، پلاک ۱۲", Desc = "خدمات تخصصی عروس و آرایشگاه زنانه", Color = "#F472B6", Vip = true, Lat = 37.279, Lon = 49.582 },
            new { Name = "آرایشگاه هنر", Slug = "honar-hair", Phone = "02112345679", Address = "کرمان، خیابان شریعتی، پلاک ۹۰", Desc = "هنر کوتاهی و رنگ مو با قیمت مناسب", Color = "#06B6D4", Vip = false, Lat = 30.292, Lon = 57.084 },
            new { Name = "سالن زیبایی یاس", Slug = "yas-beauty", Phone = "02112345680", Address = "اهواز، خیابان سلمان فارسی، پلاک ۲۳", Desc = "خدمات کامل زیبایی و مراقبت پوست", Color = "#F97316", Vip = false, Lat = 31.321, Lon = 48.679 },
        };

        var serviceTemplates = new[]
        {
            ("کوتاهی مدل‌دار", "مو", 45, 120000m),
            ("رنگ مو", "مو", 120, 350000m),
            ("مش و هایلایت", "مو", 150, 500000m),
            ("کاشت ناخن", "ناخن", 90, 250000m),
            ("پاکسازی پوست", "پوست", 60, 320000m),
            ("میکاپ", "آرایش", 60, 400000m),
        };

        var skillPool = new[] { "کوتاهی مو", "رنگ مو", "کاشت ناخن", "پاکسازی پوست", "میکاپ حرفه‌ای", "مش و هایلایت", "کوتاهی مردانه", "کراتینه مو", "لمینت مو", "طراحی ابرو" };

        var salonIds = new List<int>();
        var artistPoolIndex = 0;

        foreach (var s in salons)
        {
            var salon = new Salon
            {
                Name = s.Name,
                Slug = s.Slug,
                Phone = s.Phone,
                Address = s.Address,
                Description = s.Desc,
                Latitude = s.Lat,
                Longitude = s.Lon,
                LogoUrl = null,
                ThemeColor = s.Color,
                IsVip = s.Vip,
                IsActive = true,
                RatingAvg = Random.Shared.NextDouble() * 2 + 3,
                ManagerId = managerId
            };
            _db.Salons.Add(salon);
            await _db.SaveChangesAsync();
            salonIds.Add(salon.Id);

            // 2-3 services per salon
            var servicesForSalon = serviceTemplates.OrderBy(_ => Random.Shared.Next()).Take(Random.Shared.Next(2, 4));
            foreach (var (name, cat, dur, price) in servicesForSalon)
            {
                _db.SalonServices.Add(new SalonService
                {
                    Name = name,
                    Category = cat,
                    BaseDurationMinutes = dur,
                    BasePrice = price,
                    SalonId = salon.Id
                });
            }

            // 2-3 artists per salon
            var artistsForSalon = Random.Shared.Next(2, 4);
            for (int a = 0; a < artistsForSalon && artistPoolIndex < artistUserIds.Count; a++)
            {
                var uid = artistUserIds[artistPoolIndex % artistUserIds.Count];
                artistPoolIndex++;

                var skill = skillPool[Random.Shared.Next(skillPool.Length)];
                var bio = $"متخصص {skill} با {Random.Shared.Next(1, 8)} سال سابقه کار";

                _db.Artists.Add(new Artist
                {
                    UserId = uid,
                    SalonId = salon.Id,
                    BioShort = bio,
                    Skill = skill,
                    ContractType = (ContractType)Random.Shared.Next(1, 4),
                    RatingAvg = Math.Round((decimal)(Random.Shared.NextDouble() * 2 + 3), 1),
                    RatingCount = Random.Shared.Next(5, 200),
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "10 salons created successfully",
            salons = salons.Length,
            services = await _db.SalonServices.CountAsync(),
            artists = await _db.Artists.CountAsync(),
            artistUsers = artistUserIds.Count
        });
    }

    [HttpPost("demo")]
    public async Task<IActionResult> SeedDemo()
    {
        var client = await _userManager.FindByNameAsync("09110000004");
        var manager = await _userManager.FindByNameAsync("09110000002");
        if (client == null || manager == null)
            return BadRequest(new { message = "Run /api/auth/seed first" });

        var anySalon = await _db.Salons.FirstOrDefaultAsync(s => s.IsActive);
        if (anySalon == null)
            return BadRequest(new { message = "Run /api/seed/salons first" });

        var anyArtist = await _db.Artists.FirstOrDefaultAsync(a => a.SalonId == anySalon.Id && a.IsActive);
        var anyService = await _db.SalonServices.FirstOrDefaultAsync(s => s.SalonId == anySalon.Id && s.IsActive);
        if (anyArtist == null || anyService == null)
            return BadRequest(new { message = "No artist or service found in the first salon" });

        var results = new List<string>();

        // Create 3 sample appointments with different statuses
        var tomorrow = DateTime.Today.AddDays(1);

        if (!await _db.Appointments.AnyAsync(a => a.ClientId == client.Id))
        {
            var appts = new[]
            {
                new Appointment
                {
                    ClientId = client.Id,
                    ArtistId = anyArtist.Id,
                    SalonId = anySalon.Id,
                    ServiceId = anyService.Id,
                    StartTime = tomorrow.AddHours(10),
                    EndTime = tomorrow.AddHours(10).AddMinutes(anyService.BaseDurationMinutes),
                    DurationMinutes = anyService.BaseDurationMinutes,
                    EstimatedPrice = anyService.BasePrice,
                    DepositAmount = anyService.BasePrice * 0.3m,
                    Status = AppointmentStatus.Confirmed,
                    Notes = "رزرو آزمایشی"
                },
                new Appointment
                {
                    ClientId = client.Id,
                    ArtistId = anyArtist.Id,
                    SalonId = anySalon.Id,
                    ServiceId = anyService.Id,
                    StartTime = tomorrow.AddDays(1).AddHours(14),
                    EndTime = tomorrow.AddDays(1).AddHours(14).AddMinutes(anyService.BaseDurationMinutes),
                    DurationMinutes = anyService.BaseDurationMinutes,
                    EstimatedPrice = anyService.BasePrice,
                    DepositAmount = anyService.BasePrice * 0.3m,
                    Status = AppointmentStatus.Pending,
                    Notes = "رزرو آزمایشی ۲"
                },
                new Appointment
                {
                    ClientId = client.Id,
                    ArtistId = anyArtist.Id,
                    SalonId = anySalon.Id,
                    ServiceId = anyService.Id,
                    StartTime = DateTime.Today.AddDays(-3).AddHours(11),
                    EndTime = DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(anyService.BaseDurationMinutes),
                    DurationMinutes = anyService.BaseDurationMinutes,
                    EstimatedPrice = anyService.BasePrice,
                    DepositAmount = anyService.BasePrice * 0.3m,
                    FinalPrice = anyService.BasePrice,
                    Status = AppointmentStatus.Completed,
                    IsRated = true,
                    Rating = 4,
                    Comment = "خدمات عالی بود"
                }
            };

            _db.Appointments.AddRange(appts);
            await _db.SaveChangesAsync();
            results.Add("3 sample appointments created");
        }

        // Create notifications for the client
        if (!await _db.Notifications.AnyAsync(n => n.UserId == client.Id))
        {
            var notifs = new[]
            {
                new Notification { UserId = client.Id, Title = "رزرو تأیید شد", Message = $"رزرو شما در {anySalon.Name} برای فردا تأیید شد", Type = "success" },
                new Notification { UserId = client.Id, Title = "یادآوری رزرو", Message = $"فردا ساعت ۱۰ در {anySalon.Name} رزرو دارید", Type = "warning" },
                new Notification { UserId = client.Id, Title = "خدمات جدید", Message = $"خدمات جدیدی به {anySalon.Name} اضافه شد", Type = "info" },
                new Notification { UserId = client.Id, Title = "تغییر زمان", Message = "یک رزرو توسط مدیریت لغو شد", Type = "error" },
            };
            _db.Notifications.AddRange(notifs);
            await _db.SaveChangesAsync();
            results.Add("4 sample notifications created");
        }

        // Create notification for manager
        if (!await _db.Notifications.AnyAsync(n => n.UserId == manager.Id))
        {
            _db.Notifications.Add(new Notification
            {
                UserId = manager.Id,
                Title = "رزرو جدید",
                Message = "یک مشتری جدید در سالن شما رزرو کرد",
                Type = "info"
            });
            await _db.SaveChangesAsync();
            results.Add("1 manager notification created");
        }

        return Ok(new { message = "Demo data created", details = results });
    }
}
