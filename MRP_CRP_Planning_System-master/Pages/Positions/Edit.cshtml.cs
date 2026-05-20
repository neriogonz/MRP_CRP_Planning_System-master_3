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

namespace MetallurgyAnalytics.Pages.Positions
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Position Position { get; set; } = new Position();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);

            if (position == null)
            {
                return NotFound();
            }

            Position = position;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var position = _context.Positions.Where(p => p.Id == Position.Id).FirstOrDefault();
            if (position == null)
            {
                return Page();
            }

            RecordUtils.CopyProperties(Position, position);
            await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);

            if (position != null)
            {

                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}