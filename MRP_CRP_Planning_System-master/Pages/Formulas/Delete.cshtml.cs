using MetallurgyAnalytics.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRP_MRP_Planning_Web.Pages.Formulas
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context) => _context = context;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var formula = await _context.Formulas.FindAsync(id);
            if (formula != null)
            {
                _context.Formulas.Remove(formula);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
