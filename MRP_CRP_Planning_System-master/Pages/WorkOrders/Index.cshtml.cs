using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MetallurgyAnalytics.Pages.WorkOrders
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
        public IList<WorkCenter> WorkCenters { get; set; } = new List<WorkCenter>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? WorkCenterFilter { get; set; }

        [BindProperty(SupportsGet = true)] 
        public int PageId { get; set; } = 1;
        
        public int TotalPages { get; set; }
        private const int PageSize = 15;

        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.WorkOrders
                .Include(w => w.ProductionOrder).ThenInclude(p => p.Product)
                .Include(w => w.WorkCenter)
                .Include(w => w.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(w => w.Name.Contains(Search) || 
                                         (w.ProductionOrder != null && w.ProductionOrder.Product.Sku.Contains(Search)));
            }

            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<WorkOrderStatus>(StatusFilter, true, out var status))
            {
                query = query.Where(w => w.Status == status);
            }

            if (WorkCenterFilter.HasValue)
            {
                query = query.Where(w => w.WorkCenterId == WorkCenterFilter.Value);
            }

            WorkCenters = await _context.WorkCenters.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync();

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            WorkOrders = await query
                .OrderByDescending(w => w.DateStartPlanned)
                .Skip((PageId - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}