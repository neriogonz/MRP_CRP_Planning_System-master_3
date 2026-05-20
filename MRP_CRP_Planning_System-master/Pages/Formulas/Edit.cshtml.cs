using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using CRP_MRP_Planning_Web.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MetallurgyAnalytics.Pages.Formulas
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Formula Formula { get; set; } = new Formula();

        public SelectList? EndProductOptions { get; set; }
        public SelectList? IngredientOptions { get; set; }
        public SelectList? WorkCenterOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var formula = await _context.Formulas.FindAsync(id);

            if (formula == null)
            {
                return NotFound();
            }

            Formula = formula;

            var products = await _context.Positions
                .OrderBy(p => p.Name)
                .ToListAsync();

            EndProductOptions = new SelectList(products, "Id", "Name", Formula.EndProductId);
            IngredientOptions = new SelectList(products, "Id", "Name", Formula.IngredientId);

            var workCenters = await _context.WorkCenters
                .Where(w => w.IsActive && w.RollingSpeed > 0)
                .OrderBy(w => w.Name)
                .ToListAsync();

            WorkCenterOptions = new SelectList(workCenters, "Id", "Name", Formula.WorkCenterId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var formula = _context.Formulas.Where(p => p.Id == Formula.Id).FirstOrDefault();
            if (formula == null)
            {
                return Page();
            }

            RecordUtils.CopyProperties(Formula, formula);
            await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var formula = await _context.Formulas.FindAsync(id);

            if (formula != null)
            {
                _context.Formulas.Remove(formula);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}