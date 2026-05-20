using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MetallurgyAnalytics.Services;

namespace MetallurgyAnalytics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GanttController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly PlanningService _planningService;

        public GanttController(AppDbContext context)
        {
            _context = context;
            _planningService = new PlanningService(context);
        }

        // [HttpGet("tasks")]
        // public async Task<ActionResult<List<GanttTaskDto>>> GetGanttTasks(
        //     [FromQuery] DateTime? start, 
        //     [FromQuery] DateTime? end)
        // {
        //     var startDate = start ?? DateTime.Today.AddMonths(-1);
        //     var endDate = end ?? DateTime.Today.AddMonths(1);

        //     var tasks = await _context.WorkOrders
        //         .Where(w => w.DateStartPlanned <= endDate && w.DateEndPlanned >= startDate)
        //         .Select(w => new GanttTaskDto
        //         {
        //             id = w.Id,
        //             text = $"{w.ProductionOrder.Product.Sku}: {w.Name}",
        //             start_date = w.DateStartPlanned,
        //             end_date = w.DateEndPlanned,
        //             progress = w.Status == WorkOrderStatus.Done ? 100 : (w.Status == WorkOrderStatus.Started ? 50 : 0),
        //             parent = null,
        //             resource_name = w.WorkCenter.Name,
        //             work_center_id = w.WorkCenterId,
        //             status = w.Status.ToString().ToLower()
        //         })
        //         .OrderBy(w => w.work_center_id)
        //         .ThenBy(w => w.start_date)
        //         .ToListAsync();

        //     return Ok(tasks);
        // }

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

        [HttpPost("run-planning")]
        public async Task<IActionResult> RunPlanning()
        {
            try 
            {
                var count = await _planningService.RunPlanningAsync();
                // await _planningService.RunCapacityAsync();
                return Ok(new { message = $"Спланировано {count} партий (Work Orders). Расчет выполнен на основе скорости станков и даты окончания заказов.", count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        
        [HttpGet("tasks")]
        public async Task<ActionResult<List<GanttTaskDto>>> GetGanttTasks(
            [FromQuery] DateTime? start, 
            [FromQuery] DateTime? end)
        {
            var startDate = start ?? DateTime.Today.AddMonths(-1);
            var endDate = end ?? DateTime.Today.AddMonths(1);

            var tasks = (await _context.WorkOrders
                .Include(w => w.ProductionOrder).ThenInclude(p => p.Product)
                .Include(w => w.WorkCenter)
                .Include(w => w.Position) // Важно: загружаем позицию явно!
                .ToListAsync())
                .Where(w => w.DateStartPlanned <= endDate && w.DateEndPlanned >= startDate)
                .Select(w => new GanttTaskDto
                {
                    id = w.Id,
                    text = $"{w.ProductionOrder.Product.Sku} [{w.Position.Name ?? "Без калибра"}]", 
                    start_date = w.DateStartPlanned,
                    end_date = w.DateEndPlanned,
                    progress = w.Status == WorkOrderStatus.Done ? 100 : (w.Status == WorkOrderStatus.Started ? 50 : 0),
                    resource_name = w.WorkCenter.Name,
                    work_center_id = w.WorkCenterId,
                    status = w.Status.ToString().ToLower(),
                    
                    position_id = w.Position != null ? w.Position.Id : (int?)null 
                })
                .OrderBy(w => w.work_center_id)
                .ThenBy(w => w.start_date)
                .ToList();

            return Ok(tasks);
        }
    }

    
}