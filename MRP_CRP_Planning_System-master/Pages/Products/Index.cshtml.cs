using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace MetallurgyAnalytics.Pages.Products
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context) => _context = context;

        public IList<Product> Products { get; set; } = new List<Product>();
        
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string TypeFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PageId { get; set; } = 1;
        
        public int TotalPages { get; set; }
        private const int PageSize = 20;

        public string? ImportMessage { get; set; }
        public bool IsImportSuccess { get; set; }
        
        public async Task OnGetAsync()
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(p => 
                    p.Name.Contains(Search) || 
                    p.Sku.Contains(Search));
            }

            if (!string.IsNullOrWhiteSpace(TypeFilter) && 
                Enum.TryParse<ProductType>(TypeFilter, out var type))
            {
                query = query.Where(p => p.Type == type);
            }

            TotalPages = (int)Math.Ceiling((double)await query.CountAsync() / PageSize);
            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            Products = await query
                .OrderBy(p => p.Name)
                .Skip((PageId - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
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
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                    HeaderValidated = null,       // Игнорировать отсутствие заголовков (Id)
                    MissingFieldFound = null      // Игнорировать отсутствие полей в классе
                };

                using var reader = new CsvReader(stream, config);
                var records = reader.GetRecords<Product>().ToList();

                foreach (var record in records)
                {
                    if (string.IsNullOrWhiteSpace(record.Name)) continue;
                    if (string.IsNullOrWhiteSpace(record.Uom)) record.Uom = "шт";
                    if (record.LeadTimeDays == 0) record.LeadTimeDays = 1;
                    
                    _context.Products.Add(record);
                }

                await _context.SaveChangesAsync();
                ImportMessage = $"Успешно импортировано {records.Count} продуктов.";
                IsImportSuccess = true;
            }
            catch (Exception ex)
            {
                ImportMessage = $"Ошибка импорта: {ex.Message}";
                IsImportSuccess = false;
            }
            return RedirectToPage();
        }
    }
}