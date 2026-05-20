using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MetallurgyAnalytics.Pages.Products
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context) => _context = context;

        [BindProperty]
        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products.FindAsync(id);
            if (Product == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // if (!ModelState.IsValid) return Page();

            var existing = await _context.Products.FindAsync(Product.Id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(Product.Sku) && 
                await _context.Products.AnyAsync(p => p.Sku == Product.Sku && p.Id != Product.Id))
            {
                ModelState.AddModelError("Product.Sku", "Продукт с таким артикулом уже существует");
                return Page();
            }

            existing.Name = Product.Name;
            existing.Sku = Product.Sku;
            existing.Type = Product.Type;
            existing.Uom = Product.Uom;
            existing.LeadTimeDays = Product.LeadTimeDays;
            existing.HeightMm = Product.HeightMm;
            existing.WidthMm = Product.WidthMm;
            existing.PreformType = Product.PreformType;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Продукт успешно обновлён";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var hasRelations = await _context.BillOfMaterials.AnyAsync(b => b.ProductId == id) ||
                                await _context.StockMoves.AnyAsync(s => s.ProductId == id) ||
                                await _context.ProductionOrders.AnyAsync(p => p.ProductId == id);
                
                if (hasRelations)
                {
                    TempData["Error"] = "Невозможно удалить: продукт используется в спецификациях или заказах";
                    return RedirectToPage("Edit", new { id });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Продукт удалён";
            }
            return RedirectToPage("Index");
        }
    }
}