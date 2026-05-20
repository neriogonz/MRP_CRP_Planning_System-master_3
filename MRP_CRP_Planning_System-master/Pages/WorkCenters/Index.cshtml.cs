using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace MetallurgyAnalytics.Pages.WorkCenters
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<WorkCenter> WorkCenters { get; set; } = new List<WorkCenter>();
        
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool? ActiveOnly { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PageId { get; set; } = 1;
        
        public int TotalPages { get; set; }
        public int PageSize => 20;

        public string? ImportMessage { get; set; }
        public bool IsImportSuccess { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.WorkCenters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(w => 
                    w.Name.Contains(Search) || 
                    (w.Code != null && w.Code.Contains(Search)));
            }

            if (ActiveOnly.HasValue)
            {
                query = query.Where(w => w.IsActive == ActiveOnly.Value);
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            WorkCenters = await query
                .OrderBy(w => w.Name)
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
                var records = reader.GetRecords<WorkCenter>().ToList();

                foreach (var record in records)
                {
                    if (string.IsNullOrWhiteSpace(record.Name)) continue;
                    if (record.IsActive == false) record.IsActive = true;
                    if (record.RollingSpeed <= 0) record.RollingSpeed = 1.0; 

                    _context.WorkCenters.Add(record);
                }

                await _context.SaveChangesAsync();
                ImportMessage = $"Успешно импортировано {records.Count} рабочих центров.";
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