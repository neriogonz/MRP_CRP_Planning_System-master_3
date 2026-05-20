using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;

namespace MetallurgyAnalytics.Pages.Formulas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Formula> Formulas { get; set; } = new List<Formula>();
        
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PreformFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageId { get; set; } = 1;

        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public string? ImportMessage { get; set; }
        public bool IsImportSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.Formulas.AsQueryable();

            if (!string.IsNullOrEmpty(Search) && int.TryParse(Search, out int search))
            {
                query = query.Where(p => p.Id == search);
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            Formulas = await query
                .Include(f => f.EndProduct)
                .Include(f => f.Ingredient)
                .Include(f => f.WorkCenter)
                .OrderBy(f => f.Id)
                .Skip((PageId - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ImportMessage = "Файл не выбран.";
                IsImportSuccess = false;
                return RedirectToPage();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());

                if (stream.ReadLine() != "N;Готовый продукт;Ингредиент;Расходный коэффициент;Техкарта")
                {
                    throw new Exception("Неверный файл.");
                }

                int count = 0;
                string? line;
                while ((line = stream.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');
                    Formula? formula = null;
                    bool new_formula = true;
                    
                    if (int.TryParse(parts[0], out int formula_id))
                    {
                        Formula? search_formula = await _context.Formulas.FindAsync(formula_id);
                        if (search_formula != null)
                        {
                            formula = search_formula;
                            new_formula = false;
                        }
                        else
                        {
                            formula = new Formula()
                            {
                                Id = formula_id
                            };
                        }
                    }
                    formula ??= new Formula();

                    if (parts[1] != string.Empty)
                    {
                        Position? endProduct = await _context.Positions.Where(p => p.Name == parts[1]).FirstOrDefaultAsync();
                        if (endProduct != null)
                        {
                            Console.WriteLine($"{formula.Id}: найден конечный продукт: {endProduct.Name}");
                            formula.EndProductId = endProduct.Id;
                        }
                        else
                        {
                            Console.WriteLine($"{formula.Id}: конечный продукт не найден");
                        }
                    }

                    if (parts[2] != string.Empty)
                    {
                        Position? ingredient = await _context.Positions.Where(p => p.Name == parts[2] || p.Preform == parts[2]).FirstOrDefaultAsync();
                        if (ingredient != null)
                        {
                            Console.WriteLine($"{formula.Id}: найден ингедиент: {ingredient.GetVisibleName()}");
                            formula.IngredientId = ingredient.Id;
                        }
                        else
                        {
                            Console.WriteLine($"{formula.Id}: ингредиент не найден");
                        }
                    }

                    if (double.TryParse(parts[3], out double consumption_coeff))
                    {
                        formula.ConsumptionCoeff = consumption_coeff;
                    }

                    if (parts[4] != string.Empty)
                    {
                        WorkCenter? workCenter = await _context.WorkCenters.Where(w => w.Name == parts[4]).FirstOrDefaultAsync();
                        if (workCenter != null)
                        {
                            Console.WriteLine($"{formula.Id}: найден рабочий центр: {workCenter.Name}");
                            formula.WorkCenterId = workCenter.Id;
                        }
                        else
                        {
                            Console.WriteLine($"{formula.Id}: рабочий центр не найден");
                        }
                    }

                    if (new_formula)
                    {
                        _context.Formulas.Add(formula);
                    }
                    await _context.SaveChangesAsync();
                    count++;
                }

                await _context.SaveChangesAsync();

                ImportMessage = $"Успешно импортировано {count} формул.";
                IsImportSuccess = true;
            }
            catch (Exception ex)
            {
                ImportMessage = $"Ошибка при импорте: {ex.Message}";
                IsImportSuccess = false;
                // Для отладки можно вывести внутреннее исключение
                if (ex.InnerException != null)
                {
                    ImportMessage += $"\nДетали: {ex.InnerException.Message}";
                }
            }

            return RedirectToPage();
        }
    }
}