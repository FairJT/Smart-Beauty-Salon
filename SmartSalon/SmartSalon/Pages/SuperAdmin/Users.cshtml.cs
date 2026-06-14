using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.SuperAdmin
{
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<ApplicationUser> Users { get; set; } = new();
        public string? Search { get; set; }
        public string? Message { get; set; }

        public UsersModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(string? search)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            Search = search;

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.UserName!.Contains(search));

            Users = await query
                .OrderByDescending(u => u.UserType)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(string id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var user = await _db.Users.FindAsync(id);
            if (user != null && user.UserType != UserType.SuperAdmin)
            {
                user.IsActive = !user.IsActive;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetManagerAsync(string id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.UserType = UserType.SalonManager;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostSetArtistAsync(string id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.UserType = UserType.Artist;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetClientAsync(string id)
        {
            var token = HttpContext.Session.GetString("SuperAdminToken");
            if (token == null) return RedirectToPage("/SuperAdmin/Login");

            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.UserType = UserType.Client;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}