using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MetallurgyAnalytics.Pages.ProductionOrders
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context) => _context = context;

        [BindProperty]
        public ProductionOrderInput Order { get; set; } = new();

        public SelectList? ProductOptions { get; set; }
        public SelectList? UserOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdowns();
            
            Order.DateStartPlanned = DateTime.UtcNow;
            Order.DateFinishPlanned = Order.DateStartPlanned.AddDays(1); 
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return Page();
            }

            var product = await _context.Products.FindAsync(Order.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("Order.ProductId", "Продукт не найден");
                await LoadDropdowns();
                return Page();
            }

            var startDate = DateTime.UtcNow;
            var finishDate = startDate.AddDays(product.LeadTimeDays);

            var entity = new ProductionOrder
            {
                ProductId = Order.ProductId,
                Quantity = Order.Quantity,
                Priority = Order.Priority,
                // AssignedToUserId = Order.AssignedToUserId,
                State = OrderState.Draft,
                DateStartPlanned = startDate,
                DateFinishPlanned = finishDate,
                CreatedByUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0")
            };

            _context.ProductionOrders.Add(entity);
            await _context.SaveChangesAsync();

            await GenerateWorkOrdersAsync(entity.Id, product, startDate);

            TempData["Success"] = $"Заказ #{entity.Id} успешно создан";
            return RedirectToPage("Index");
        }

        private async Task LoadDropdowns()
        {
            var products = await _context.Products
                .Where(p => p.Type == ProductType.FinishedGood || p.Type == ProductType.Component)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            ProductOptions = new SelectList(products, "Id", "Name", Order.ProductId);

            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            
            UserOptions = new SelectList(users, "Id", "FullName", Order.AssignedToUserId);
        }

        private async Task GenerateWorkOrdersAsync(int orderId, Product product, DateTime startDate)
        {
            var bom = await _context.BillOfMaterials
                .Include(b => b.Lines).ThenInclude(l => l.ComponentProduct)
                .FirstOrDefaultAsync(b => b.ProductId == product.Id && b.IsActive);

            if (bom?.Lines.Any() != true) return;

            int sequence = 10;
            var defaultWorkCenter = await _context.WorkCenters
                .FirstOrDefaultAsync(w => w.IsActive && w.CapacityPerHour > 0);

            if (defaultWorkCenter == null) return;

            foreach (var line in bom.Lines.OrderBy(l => l.Sequence))
            {
                var durationHours = line.Quantity / defaultWorkCenter.CapacityPerHour;
                
                if (durationHours < 0.5) durationHours = 0.5;

                _context.WorkOrders.Add(new WorkOrder
                {
                    ProductionOrderId = orderId,
                    WorkCenterId = defaultWorkCenter.Id,
                    Name = $"Операция: {line.ComponentProduct?.Name ?? "Обработка"}",
                    Sequence = sequence,
                    DurationExpectedHours = durationHours,
                    DateStartPlanned = startDate.AddHours(sequence * 0.5),
                    DateEndPlanned = startDate.AddHours((sequence * 0.5) + durationHours),
                    Status = WorkOrderStatus.Pending
                });
                sequence += 10;
            }
            await _context.SaveChangesAsync();
        }
    }
}