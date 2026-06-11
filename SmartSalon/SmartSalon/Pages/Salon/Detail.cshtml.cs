using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;

namespace SmartSalon.Pages.Salon
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public SmartSalon.Models.Salon? Salon { get; set; }

        public DetailModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            Salon = await _db.Salons
                .Include(s => s.Artists).ThenInclude(a => a.User)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

            if (Salon == null) return NotFound();
            return Page();
        }
    }
}