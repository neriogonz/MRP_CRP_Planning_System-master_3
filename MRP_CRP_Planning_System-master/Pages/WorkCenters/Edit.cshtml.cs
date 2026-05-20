using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using CRP_MRP_Planning_Web.Utils;


namespace MetallurgyAnalytics.Pages.WorkCenters
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context) => _context = context;

        [BindProperty]
        public WorkCenter WorkCenter { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            WorkCenter = await _context.WorkCenters.FindAsync(id);
            if (WorkCenter == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // if (!ModelState.IsValid) return Page();

            var existing = await _context.WorkCenters.FindAsync(WorkCenter.Id);
            if (existing == null) return NotFound();

            RecordUtils.CopyProperties(WorkCenter, existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Продукт успешно обновлён";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var workCenter = await _context.WorkCenters.FindAsync(id);
            if (workCenter != null)
            {                
                /*if (hasRelations)
                {
                    TempData["Error"] = "Невозможно удалить: продукт используется в спецификациях или заказах";
                    return RedirectToPage("Edit", new { id });
                }*/

                _context.WorkCenters.Remove(workCenter);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Рабочий центр удалён";
            }
            return RedirectToPage("Index");
        }
    }
}