using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;

namespace MetallurgyAnalytics.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public int ProductsCount { get; set; }
        public int WorkCentersCount { get; set; }
        public int OrdersCount { get; set; }
        public int PendingWorkOrdersCount { get; set; }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                ProductsCount = await _context.Products.CountAsync();
                WorkCentersCount = await _context.WorkCenters.CountAsync();
                OrdersCount = await _context.ProductionOrders.CountAsync(o => o.State != OrderState.Done);
                PendingWorkOrdersCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Pending);
            }
        }
    }
}