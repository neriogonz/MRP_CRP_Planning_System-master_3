using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using CRP_MRP_Planning_Web.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;


namespace MetallurgyAnalytics.Pages.ProductionOrders
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context) => _context = context;

        [BindProperty]
        public ProductionOrderInput Order { get; set; } = new();

        public SelectList? ProductOptions { get; set; }
        public SelectList? UserOptions { get; set; }

        [BindProperty]
        public int ProductionOrderId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ProductionOrderId = id;
            ProductionOrder? order = await _context.ProductionOrders.FindAsync(id);
            if (order == null) return NotFound();

            Order = new()
            {
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Priority = order.Priority,
                DateStartPlanned = order.DateStartPlanned,
                DateFinishPlanned = order.DateFinishPlanned,
                State = order.State,
            };

            await LoadDropdowns();
            return Page();
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

        public async Task<IActionResult> OnPostAsync()
        {
            // if (!ModelState.IsValid) return Page();

            var existing = await _context.ProductionOrders.FindAsync(ProductionOrderId);
            if (existing == null) return NotFound();

            if (Order.ProductId != 0 && 
                await _context.ProductionOrders.AnyAsync(p => p.ProductId == Order.ProductId && p.Id != ProductionOrderId))
            {
                TempData["Error"] = "Продукт с таким артикулом уже существует";
                return RedirectToPage("Index");
            }

            existing.ProductId = Order.ProductId;
            existing.Quantity = Order.Quantity;
            existing.Priority = Order.Priority;
            existing.State = Order.State;
            existing.DateStartPlanned = Order.DateStartPlanned;
            existing.DateFinishPlanned = Order.DateFinishPlanned;
            existing.CreatedByUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            await _context.SaveChangesAsync();

            TempData["Success"] = "Продукт успешно обновлён";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var order = await _context.ProductionOrders.FindAsync(id);
            if (order != null)
            {
                var hasRelations = await _context.Products.Include(p => p.ProductionOrders).AnyAsync(p => p.ProductionOrders.Contains(order)) ||
                    await _context.WorkOrders.AnyAsync(o => o.ProductionOrderId == order.Id) ||
                    await _context.StockMoves.AnyAsync(o => o.ProductionOrderId == order.Id);
                
                if (hasRelations)
                {
                    TempData["Error"] = "Невозможно удалить: заказ используется в продуктах или номенклатурах продукции.";
                    return RedirectToPage("Index");
                }

                _context.ProductionOrders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Заказ удалён";
            }
            return RedirectToPage("Index");
        }
    }
}