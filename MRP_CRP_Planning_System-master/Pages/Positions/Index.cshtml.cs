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

namespace MetallurgyAnalytics.Pages.Positions
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Position> Positions { get; set; } = new List<Position>();
        
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


        public string? LinkImportMessage { get; set; }
        public bool IsLinkImportSuccess { get; set; }

        public class LinkImportDto
        {
            public string ProductSku { get; set; }
            public string PositionName { get; set; }
        }

        public async Task<IActionResult> OnPostImportLinksAsync(IFormFile linkFile)
        {
            if (linkFile == null || linkFile.Length == 0)
            {
                LinkImportMessage = "Файл не выбран.";
                IsLinkImportSuccess = false;
                return RedirectToPage();
            }

            try
            {
                var allProducts = await _context.Products.ToDictionaryAsync(p => p.Sku);
                var allPositions = await _context.Positions.ToDictionaryAsync(p => p.Name);

                var existingLinks = await _context.ProductPositionLinks
                    .Select(l => new { l.ProductId, l.PositionId })
                    .ToListAsync();
                
                var existingLinksSet = new HashSet<string>(
                    existingLinks.Select(l => $"{l.ProductId}_{l.PositionId}")
                );

                using var stream = new StreamReader(linkFile.OpenReadStream());
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using var reader = new CsvReader(stream, config);
                var records = reader.GetRecords<LinkImportDto>().ToList();

                if (!records.Any())
                {
                    LinkImportMessage = "Файл пуст или неверного формата.";
                    IsLinkImportSuccess = false;
                    return RedirectToPage();
                }

                int createdCount = 0;
                int skippedCount = 0;
                int errorCount = 0;
                var errors = new List<string>();
                var linksToAdd = new List<ProductPositionLink>();

                foreach (var record in records)
                {
                    if (string.IsNullOrWhiteSpace(record.ProductSku) || string.IsNullOrWhiteSpace(record.PositionName))
                    {
                        errorCount++;
                        continue;
                    }

                    if (!allProducts.TryGetValue(record.ProductSku, out var product))
                    {
                        errorCount++;
                        errors.Add($"Продукт с SKU '{record.ProductSku}' не найден.");
                        continue;
                    }

                    if (!allPositions.TryGetValue(record.PositionName, out var position))
                    {
                        errorCount++;
                        errors.Add($"Позиция '{record.PositionName}' не найдена.");
                        continue;
                    }

                    string linkKey = $"{product.Id}_{position.Id}";
                    if (existingLinksSet.Contains(linkKey))
                    {
                        skippedCount++;
                        continue;
                    }

                    linksToAdd.Add(new ProductPositionLink
                    {
                        ProductId = product.Id,
                        PositionId = position.Id
                    });

                    existingLinksSet.Add(linkKey);
                    createdCount++;
                }

                if (linksToAdd.Any())
                {
                    _context.ProductPositionLinks.AddRange(linksToAdd);
                    await _context.SaveChangesAsync();
                }

                if (errorCount > 0)
                {
                    LinkImportMessage = $"Готово. Создано: {createdCount}, Пропущено (уже есть): {skippedCount}, Ошибок: {errorCount}.<br/>Примеры ошибок: {string.Join("<br/>", errors.Take(3))}";
                    IsLinkImportSuccess = false;
                }
                else
                {
                    LinkImportMessage = $"Успешно создано {createdCount} связей. ({skippedCount} дубликатов пропущено).";
                    IsLinkImportSuccess = true;
                }
            }
            catch (Exception ex)
            {
                LinkImportMessage = $"Ошибка системы: {ex.Message}";
                IsLinkImportSuccess = false;
            }

            return RedirectToPage();
        }
    


        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.Positions.AsQueryable();

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(p => p.Name.Contains(Search) || (p.Preform != null && p.Preform.Contains(Search)));
            }

            if (!string.IsNullOrEmpty(PreformFilter))
            {
                query = query.Where(p => p.Preform == PreformFilter);
            }

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            PageId = Math.Max(1, Math.Min(PageId, TotalPages));

            Positions = await query
                .OrderBy(p => p.Id)
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
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null
                };

                using var reader = new CsvReader(stream, config);

                var records = reader.GetRecords<Position>().ToList();

                if (!records.Any())
                {
                    ImportMessage = "Файл пуст или не содержит данных.";
                    IsImportSuccess = false;
                    return RedirectToPage();
                }

                int count = 0;
                foreach (var record in records)
                {
                    if (string.IsNullOrWhiteSpace(record.Name))
                        continue;

                    record.Id = 0;

                    _context.Positions.Add(record);
                    count++;
                }

                await _context.SaveChangesAsync();

                ImportMessage = $"Успешно импортировано {count} позиций прокатки.";
                IsImportSuccess = true;
            }
            catch (Exception ex)
            {
                ImportMessage = $"Ошибка при импорте: {ex.Message}";
                IsImportSuccess = false;
                if (ex.InnerException != null)
                {
                    ImportMessage += $"\nДетали: {ex.InnerException.Message}";
                }
            }

            return RedirectToPage();
        }
    }
}