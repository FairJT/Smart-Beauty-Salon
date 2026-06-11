using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Admin
{
    public class ArtistsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<Artist> Artists { get; set; } = new();
        public string? Message { get; set; }

        public ArtistsModel(ApplicationDbContext db)
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
            if (salon == null)
            {
                Message = "سالنی برای این مدیر یافت نشد";
                return Page();
            }

            Artists = await _db.Artists
                .Where(a => a.SalonId == salon.Id)
                .Include(a => a.User)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(
            string mobile, string bioShort, int contractType)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null)
            {
                Message = "سالنی برای این مدیر یافت نشد";
                Artists = new();
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == mobile);
            if (user == null)
            {
                Message = "کاربری با این موبایل یافت نشد";
                Artists = await _db.Artists
                    .Where(a => a.SalonId == salon.Id)
                    .Include(a => a.User).ToListAsync();
                return Page();
            }

            var exists = await _db.Artists
                .AnyAsync(a => a.UserId == user.Id && a.SalonId == salon.Id);
            if (exists)
            {
                Message = "این کاربر قبلاً اضافه شده";
                Artists = await _db.Artists
                    .Where(a => a.SalonId == salon.Id)
                    .Include(a => a.User).ToListAsync();
                return Page();
            }

            _db.Artists.Add(new Artist
            {
                UserId = user.Id,
                SalonId = salon.Id,
                BioShort = bioShort,
                ContractType = (ContractType)contractType
            });
            await _db.SaveChangesAsync();

            Message = "✅ پرسنل با موفقیت اضافه شد";
            Artists = await _db.Artists
                .Where(a => a.SalonId == salon.Id)
                .Include(a => a.User).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (token == null) return RedirectToPage("/Admin/Login");

            var salon = await GetManagerSalonAsync();
            if (salon == null) return RedirectToPage();

            var artist = await _db.Artists
                .FirstOrDefaultAsync(a => a.Id == id && a.SalonId == salon.Id);

            if (artist != null)
            {
                artist.IsActive = !artist.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}