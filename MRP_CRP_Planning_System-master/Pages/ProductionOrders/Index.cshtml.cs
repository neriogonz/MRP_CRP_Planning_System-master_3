using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;



namespace MetallurgyAnalytics.Pages.ProductionOrders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
        
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? StateFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? PriorityFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PageId { get; set; } = 1;
        
        public int TotalPages { get; set; }
        public int PageSize => 20;

        public string? ImportMessage { get; set; }
        public bool IsImportSuccess { get; set; }

        public class ProductionOrderImportDto
        {
            public string ProductSku { get; set; }
            public double Quantity { get; set; }
            public string DateFinishPlanned { get; set; }
            public int Priority { get; set; }
        }


        public async Task OnGetAsync()
        {
            var query = _context.ProductionOrders
                .Include(p => p.Product)
                .Include(p => p.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(p => 
                    p.Id.ToString().Contains(Search) ||
                    (p.Product != null && p.Product.Name.Contains(Search)));
            }

            if (!string.IsNullOrWhiteSpace(StateFilter) && 
                Enum.TryParse<OrderState>(StateFilter, out var state))
            {
                query = query.Where(p => p.State == state);
            }

            if (PriorityFilter.HasValue)
            {
                query = query.Where(p => p.Priority == PriorityFilter.Value);
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            ProductionOrders = await query
                .OrderByDescending(p => p.Priority)
                .ThenBy(p => p.DateStartPlanned)
                .Skip((PageId - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostImportAsync(IFormFile file)
        {
            ImportMessage = "";
            IsImportSuccess = false;

            if (file == null || file.Length == 0)
            {
                ImportMessage = "Файл не выбран.";
                return RedirectToPage();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8);
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreBlankLines = true
                };

                using var reader = new CsvReader(stream, config);
                var records = reader.GetRecords<ProductionOrderImportDto>().ToList();

                if (!records.Any())
                {
                    ImportMessage = "Файл пуст или не содержит данных.";
                    return RedirectToPage();
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                int? currentUserId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                    currentUserId = user?.Id;
                }

                foreach (var record in records)
                {
                    if (record == null || string.IsNullOrWhiteSpace(record.ProductSku))
                    {
                        continue;
                    }

                    try
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.Sku == record.ProductSku);

                        if (product == null)
                        {
                            errors.Add($"Продукт '{record.ProductSku}' не найден.");
                            errorCount++;
                            continue;
                        }

                        DateTime dateFinish;
                        if (!DateTime.TryParseExact(record.DateFinishPlanned, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFinish))
                        {
                            if (!DateTime.TryParse(record.DateFinishPlanned, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFinish))
                            {
                                errors.Add($"Неверная дата для {record.ProductSku}.");
                                errorCount++;
                                continue;
                            }
                        }

                        var localDate = DateTime.SpecifyKind(dateFinish, DateTimeKind.Local);
                        var utcDate = localDate.ToUniversalTime();
                        var startDate = utcDate.AddDays(-1);

                        var order = new ProductionOrder
                        {
                            ProductId = product.Id,
                            Quantity = record.Quantity,
                            DateFinishPlanned = utcDate,
                            DateStartPlanned = startDate,
                            Priority = record.Priority > 0 ? record.Priority : 0,
                            State = OrderState.Draft,
                            CreatedByUserId = currentUserId
                        };

                        _context.ProductionOrders.Add(order);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Ошибка для {record.ProductSku}: {ex.Message}");
                        errorCount++;
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Импорт завершен. Успешно: {successCount}.";
                if (errorCount > 0)
                {
                    message += $" Ошибок: {errorCount}.";
                    if (errors.Any())
                    {
                        message += " Первые ошибки: " + string.Join("; ", errors.Take(3));
                        if (errors.Count > 3) message += " ...";
                    }
                }

                ImportMessage = message;
                IsImportSuccess = errorCount == 0;
            }
            catch (Exception ex)
            {
                ImportMessage = $"Критическая ошибка: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ImportMessage += $"\nДетали: {ex.InnerException.Message}";
                }
                IsImportSuccess = false;
            }

            return RedirectToPage();
        }
    }
}