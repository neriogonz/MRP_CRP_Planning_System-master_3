using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MetallurgyAnalytics.Pages.WorkCenters
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context) => _context = context;

        [BindProperty]
        public WorkCenter WorkCenter { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(WorkCenter.Code))
            {
                var exists = await _context.WorkCenters
                    .AnyAsync(w => w.Code == WorkCenter.Code);
                
                if (exists)
                {
                    ModelState.AddModelError("WorkCenter.Code", 
                        "Рабочий центр с таким кодом уже существует");
                    return RedirectToPage("Index");
                }
            }

            _context.WorkCenters.Add(WorkCenter);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Рабочий центр «{WorkCenter.Name}» добавлен";
            return RedirectToPage("Index");
        }
    }
}