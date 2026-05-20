using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace MetallurgyAnalytics.Pages.Products
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context) => _context = context;

        [BindProperty]
        public Product Product { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(Product.Sku) && 
                await _context.Products.AnyAsync(p => p.Sku == Product.Sku))
            {
                ModelState.AddModelError("Product.Sku", "Продукт с таким артикулом уже существует");
                return Page();
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Продукт \"{Product.Name}\" успешно добавлен";
            return RedirectToPage("Index");
        }
    }
}