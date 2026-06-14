using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartSalon.Data;
using SmartSalon.Models;

namespace SmartSalon.Pages.Salon
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<SmartSalon.Models.Salon> Salons { get; set; } = new();
        public string? Search { get; set; }

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync(string? search)
        {
            Search = search;

            var query = _db.Salons.Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Name.Contains(search));

            Salons = await query
                .OrderByDescending(s => s.IsVip)
                .ThenByDescending(s => s.RatingAvg)
                .ToListAsync();
        }
    }
}