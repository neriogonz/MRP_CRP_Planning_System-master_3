using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetallurgyAnalytics.Services
{
    public class PlanningService
    {
        private readonly AppDbContext _context;

        public PlanningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> RunPlanningAsync()
        {
            var ordersToPlan = await _context.ProductionOrders
                .Include(o => o.Product)
                .Where(o => o.State == OrderState.Draft || o.State == OrderState.Confirmed)
                .ToListAsync();

            if (!ordersToPlan.Any()) return 0;

            ordersToPlan = ordersToPlan.OrderBy(o => o.Priority)
                           .ThenBy(o => o.DateFinishPlanned)
                           .ToList();

            var allFormulas = await _context.Formulas.Include(f => f.WorkCenter).ToListAsync();
            var allPositions = await _context.Positions.ToListAsync();
            var allProducts = await _context.Products.ToDictionaryAsync(p => p.Id);
            
            var workCenters = await _context.WorkCenters.Where(w => w.IsActive).ToListAsync();
            var workCenterAvailability = workCenters.ToDictionary(w => w.Id, w => DateTime.UtcNow);

            int totalCreatedOrders = 0;

            foreach (var order in ordersToPlan)
            {
                try
                {
                    var targetPosition = await FindPositionForProduct(order.ProductId, allProducts);
                    if (targetPosition == null)
                    {
                        Console.WriteLine($"Заказ {order.Id}: Не найдена позиция для продукта {order.ProductId}");
                        continue;
                    }

                    var allPaths = FindAllProductionPaths(targetPosition.Id, allFormulas, allPositions);

                    if (!allPaths.Any())
                    {
                        Console.WriteLine($"Заказ {order.Id}: Нет технологических маршрутов для позиции {targetPosition.Name}");
                        continue;
                    }

                    var bestPath = SelectOptimalPath(allPaths, order.Quantity, workCenters);

                    if (bestPath == null)
                    {
                        Console.WriteLine($"Заказ {order.Id}: Нет оптимальной цепочки");
                        continue;
                    }

                    
                    DateTime currentStartTime = DateTime.UtcNow;

                    foreach (var step in bestPath.Steps)
                    {
                        var workCenter = workCenters.FirstOrDefault(wc => wc.Id == step.WorkCenterId);
                        if (workCenter == null)
                        {
                            Console.WriteLine($"Заказ {order.Id}: Рабочий центр не найден");
                            continue;
                        }

                        double speed = workCenter.RollingSpeed > 0 ? workCenter.RollingSpeed : workCenter.CapacityPerHour;
                        if (speed <= 0) speed = 1;

                        double durationHours = step.RequiredQuantity / speed;
                        
                        DateTime earliestStart = workCenterAvailability[workCenter.Id];
                        
                        if (currentStartTime > earliestStart)
                        {
                            earliestStart = currentStartTime;
                        }

                        DateTime plannedStart = earliestStart;
                        DateTime plannedEnd = plannedStart.AddHours(durationHours);

                        var workOrder = new WorkOrder
                        {
                            ProductionOrderId = order.Id,
                            WorkCenterId = workCenter.Id,
                            PositionId = step.EndPositionId,
                            Name = $"Этап {step.StepIndex}: {step.EndPositionName}",
                            Sequence = step.StepIndex * 10,
                            DurationExpectedHours = durationHours,
                            DateStartPlanned = plannedStart,
                            DateEndPlanned = plannedEnd,
                            Status = WorkOrderStatus.Pending
                        };

                        _context.WorkOrders.Add(workOrder);

                        workCenterAvailability[workCenter.Id] = plannedEnd;
                        
                        currentStartTime = plannedEnd;
                    }

                    order.State = OrderState.InProgress;
                    order.DateStartPlanned = currentStartTime.AddHours(-bestPath.TotalDurationHours);
                    order.DateFinishPlanned = currentStartTime;
                    
                    totalCreatedOrders += bestPath.Steps.Count;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка планирования заказа {order.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return totalCreatedOrders;
        }

        private List<ProductionPath> FindAllProductionPaths(int endPositionId, List<Formula> allFormulas, List<Position> allPositions)
        {
            var paths = new List<ProductionPath>();

            var formulas = allFormulas.Where(f => f.EndProductId == endPositionId).ToList();

            if (!formulas.Any())
            {
                var rawPath = new ProductionPath();
                paths.Add(rawPath);
                return paths;
            }

            foreach (var formula in formulas)
            {
                var subPaths = FindAllProductionPaths(formula.IngredientId, allFormulas, allPositions);

                foreach (var subPath in subPaths)
                {
                    var newPath = new ProductionPath(subPath);

                    var posName = allPositions.FirstOrDefault(p => p.Id == formula.EndProductId)?.Name;
                    
                    newPath.Steps.Add(new ProductionStep
                    {
                        StepIndex = newPath.Steps.Count + 1,
                        WorkCenterId = formula.WorkCenterId,
                        EndPositionId = formula.EndProductId,
                        IngredientId = formula.IngredientId,
                        ConsumptionCoeff = formula.ConsumptionCoeff,
                        EndPositionName = posName
                    });

                    paths.Add(newPath);
                }
            }

            return paths;
        }

        private ProductionPath? SelectOptimalPath(List<ProductionPath> paths, double orderQuantity, List<WorkCenter> workCenters)
        {
            if (!paths.Any()) return null;

            foreach (var path in paths)
            {
                double totalHours = 0;
                
                foreach (var step in path.Steps)
                {
                    var wc = workCenters.FirstOrDefault(w => w.Id == step.WorkCenterId);
                    if (wc != null)
                    {
                        double speed = wc.RollingSpeed > 0 ? wc.RollingSpeed : wc.CapacityPerHour;
                        if (speed <= 0) speed = 1;

                        
                        double hours = orderQuantity / speed;
                        totalHours += hours;
                        
                        step.RequiredQuantity = orderQuantity; 
                    }
                }
                path.TotalDurationHours = totalHours;
            }

            return paths.OrderBy(p => p.TotalDurationHours).FirstOrDefault();
        }

        private async Task<Position> FindPositionForProduct(int productId, Dictionary<int, Product> allProducts)
        {
            var link = await _context.ProductPositionLinks
                .Include(l => l.Position)
                .FirstOrDefaultAsync(l => l.ProductId == productId);
            
            if (link?.Position != null) return link.Position;

            if (allProducts.TryGetValue(productId, out var product))
            {
                if (product.HeightMm.HasValue && product.WidthMm.HasValue)
                {
                    return await _context.Positions
                        .FirstOrDefaultAsync(p => 
                            p.Hmin <= product.HeightMm && product.HeightMm <= p.Hmax &&
                            p.Bmin <= product.WidthMm && product.WidthMm <= p.Bmax
                        );
                }
            }
            return null;
        }
    }


    public class ProductionPath
    {
        public List<ProductionStep> Steps { get; set; } = new List<ProductionStep>();
        public double TotalDurationHours { get; set; }

        public ProductionPath() { }
        
        public ProductionPath(ProductionPath source)
        {
            this.Steps = new List<ProductionStep>(source.Steps);
        }
    }

    public class ProductionStep
    {
        public int StepIndex { get; set; }
        public int WorkCenterId { get; set; }
        public int EndPositionId { get; set; }
        public int IngredientId { get; set; }
        public double ConsumptionCoeff { get; set; }
        public string EndPositionName { get; set; }
        
        public double RequiredQuantity { get; set; }
    }
}