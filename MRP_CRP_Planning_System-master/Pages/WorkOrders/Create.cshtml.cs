using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace MetallurgyAnalytics.Pages.WorkOrders
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WorkOrder WorkOrder { get; set; }
        
        public SelectList ProductionOrdersSelectList { get; set; }
        public SelectList WorkCentersSelectList { get; set; }
        public SelectList PositionsSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var orders = await _context.ProductionOrders
                .Include(p => p.Product)
                .Where(p => p.State != OrderState.Done)
                .OrderBy(p => p.Product.Sku)
                .ToListAsync();

            WorkCentersSelectList = new SelectList(await _context.WorkCenters.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(), "Id", "Name");
            PositionsSelectList = new SelectList(await _context.Positions.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            
            var orderItems = orders.Select(o => new { Id = o.Id, Text = $"{o.Product.Sku} - {o.Product.Name} (Заказ #{o.Id})" });
            ProductionOrdersSelectList = new SelectList(orderItems, "Id", "Text");

            WorkOrder = new WorkOrder
            {
                Sequence = 10,
                Status = WorkOrderStatus.Pending,
                DateStartPlanned = System.DateTime.Now,
                DateEndPlanned = System.DateTime.Now.AddHours(8)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var orders = await _context.ProductionOrders.Include(p => p.Product).ToListAsync();
                ProductionOrdersSelectList = new SelectList(orders.Select(o => new { Id = o.Id, Text = $"{o.Product.Sku} - {o.Product.Name}" }), "Id", "Text");
                WorkCentersSelectList = new SelectList(await _context.WorkCenters.Where(w => w.IsActive).ToListAsync(), "Id", "Name");
                PositionsSelectList = new SelectList(await _context.Positions.ToListAsync(), "Id", "Name");
                return Page();
            }

            if (WorkOrder.DateEndPlanned <= WorkOrder.DateStartPlanned)
            {
                ModelState.AddModelError("", "Дата окончания должна быть позже даты начала.");
                return Page();
            }

            _context.WorkOrders.Add(WorkOrder);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}