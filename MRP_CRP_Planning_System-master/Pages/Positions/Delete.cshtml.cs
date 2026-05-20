using MetallurgyAnalytics.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRP_MRP_Planning_Web.Pages.Positions
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context) => _context = context;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _context.Positions.FindAsync(id);
            if (product != null)
            {
                _context.Positions.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
