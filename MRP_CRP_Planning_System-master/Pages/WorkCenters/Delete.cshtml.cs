using MetallurgyAnalytics.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRP_MRP_Planning_Web.Pages.WorkCenters
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context) => _context = context;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var workCenter = await _context.WorkCenters.FindAsync(id);
            if (workCenter != null)
            {
                _context.WorkCenters.Remove(workCenter);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
