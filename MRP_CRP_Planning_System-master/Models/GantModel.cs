using System;

namespace MetallurgyAnalytics.Models
{
    public class GanttTaskDto
    {
        public int id { get; set; }
        public string text { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int progress { get; set; }
        public int? parent { get; set; }
        public bool open { get; set; } = true;
        public string resource_name { get; set; }
        public int work_center_id { get; set; }
        public string status { get; set; }

        public int? position_id { get; set; } 
    }

    public class GanttResourceDto
    {
        public int id { get; set; }
        public string label { get; set; }
        public double capacity { get; set; }
    }
    
    public class CreateProductionOrderDto
    {
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public DateTime DateStartPlanned { get; set; }
        public int Priority { get; set; } = 0;
    }
}