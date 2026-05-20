using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MetallurgyAnalytics.Pages.WorkOrders
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context) => _context = context;

        [BindProperty]
        public WorkOrder WorkOrder { get; set; }
        
        public SelectList ProductionOrdersSelectList { get; set; }
        public SelectList WorkCentersSelectList { get; set; }
        public SelectList PositionsSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workOrder == null) return NotFound();

            WorkOrder = workOrder;

            var orders = await _context.ProductionOrders.Include(p => p.Product).ToListAsync();
            ProductionOrdersSelectList = new SelectList(orders.Select(o => new { Id = o.Id, Text = $"{o.Product.Sku} - {o.Product.Name}" }), "Id", "Text", WorkOrder.ProductionOrderId);
            WorkCentersSelectList = new SelectList(await _context.WorkCenters.Where(w => w.IsActive).ToListAsync(), "Id", "Name", WorkOrder.WorkCenterId);
            PositionsSelectList = new SelectList(await _context.Positions.ToListAsync(), "Id", "Name", WorkOrder.PositionId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var orders = await _context.ProductionOrders.Include(p => p.Product).ToListAsync();
                ProductionOrdersSelectList = new SelectList(orders.Select(o => new { Id = o.Id, Text = $"{o.Product.Sku}" }), "Id", "Text", WorkOrder.ProductionOrderId);
                WorkCentersSelectList = new SelectList(await _context.WorkCenters.Where(w => w.IsActive).ToListAsync(), "Id", "Name", WorkOrder.WorkCenterId);
                PositionsSelectList = new SelectList(await _context.Positions.ToListAsync(), "Id", "Name", WorkOrder.PositionId);
                return Page();
            }

            var existingOrder = await _context.FindAsync<WorkOrder>(WorkOrder.Id);
            if (existingOrder == null) return NotFound();

            existingOrder.ProductionOrderId = WorkOrder.ProductionOrderId;
            existingOrder.WorkCenterId = WorkOrder.WorkCenterId;
            existingOrder.PositionId = WorkOrder.PositionId;
            existingOrder.Name = WorkOrder.Name;
            existingOrder.DateStartPlanned = WorkOrder.DateStartPlanned;
            existingOrder.DateEndPlanned = WorkOrder.DateEndPlanned;
            existingOrder.DurationExpectedHours = WorkOrder.DurationExpectedHours;
            existingOrder.Status = WorkOrder.Status;
            existingOrder.Sequence = WorkOrder.Sequence;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.WorkOrders.AnyAsync(e => e.Id == WorkOrder.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
    }
}