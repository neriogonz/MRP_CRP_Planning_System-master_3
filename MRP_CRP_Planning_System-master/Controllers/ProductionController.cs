using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MetallurgyAnalytics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<List<GanttTaskDto>>> GetGanttTasks(
            [FromQuery] DateTime? start, 
            [FromQuery] DateTime? end)
        {
            var startDate = start ?? DateTime.Today.AddMonths(-1);
            var endDate = end ?? DateTime.Today.AddMonths(1);

            var tasks = await _context.WorkOrders
                .Where(w => w.DateStartPlanned <= endDate && w.DateEndPlanned >= startDate)
                .Select(w => new GanttTaskDto
                {
                    id = w.Id,
                    text = $"{w.ProductionOrder.Product.Sku}: {w.Name}",
                    start_date = w.DateStartPlanned,
                    end_date = w.DateEndPlanned,
                    progress = w.Status == WorkOrderStatus.Done ? 100 : (w.Status == WorkOrderStatus.Started ? 50 : 0),
                    parent = null,
                    resource_name = w.WorkCenter.Name,
                    work_center_id = w.WorkCenterId,
                    status = w.Status.ToString().ToLower()
                })
                .OrderBy(w => w.work_center_id)
                .ThenBy(w => w.start_date)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("resources")]
        public async Task<ActionResult<List<GanttResourceDto>>> GetResources()
        {
            var resources = await _context.WorkCenters
                .Where(w => w.IsActive)
                .Select(w => new GanttResourceDto
                {
                    id = w.Id,
                    label = w.Name,
                    capacity = w.CapacityPerHour
                })
                .ToListAsync();

            return Ok(resources);
        }

        [HttpPut("tasks/{id}")]
        public async Task<IActionResult> UpdateTaskDates(int id, [FromBody] GanttTaskDto dto)
        {
            var workOrder = await _context.WorkOrders.FindAsync(id);
            
            if (workOrder == null)
                return NotFound();

            workOrder.DateStartPlanned = dto.start_date;
            workOrder.DateEndPlanned = dto.end_date;


            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Ошибка сохранения: данные были изменены другим пользователем.");
            }
        }
    }
}