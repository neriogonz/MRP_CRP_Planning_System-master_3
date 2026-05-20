using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MetallurgyAnalytics.Pages.Formulas
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Formula Formula { get; set; } = new Formula();

        public SelectList? EndProductOptions { get; set; }
        public SelectList? IngredientOptions { get; set; }
        public SelectList? WorkCenterOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var products = await _context.Positions
                .OrderBy(p => p.Name)
                .ToListAsync();

            EndProductOptions = new SelectList(products, "Id", "Name", 0);
            IngredientOptions = new SelectList(products, "Id", "Name", 0);

            var workCenters = await _context.WorkCenters
                .Where(w => w.IsActive && w.RollingSpeed > 0)
                .OrderBy(w => w.Name)
                .ToListAsync();

            WorkCenterOptions = new SelectList(workCenters, "Id", "Name", 0);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _context.Formulas.Add(Formula);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}