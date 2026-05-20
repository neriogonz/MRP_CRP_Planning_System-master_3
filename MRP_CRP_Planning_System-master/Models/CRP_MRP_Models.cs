using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MetallurgyAnalytics.Models;

namespace MetallurgyAnalytics.Models
{
    /// <summary>
    /// ������� ���������
    /// </summary>
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("sku")]
        [StringLength(100)]
        public string Sku { get; set; }

        [Column("type")]
        public ProductType Type { get; set; } = ProductType.FinishedGood;

        [Column("uom")]
        [StringLength(50)]
        public string Uom { get; set; } = "units";

        [Column("lead_time")]
        public int LeadTimeDays { get; set; } = 1;

        [Column("height_mm")]
        public decimal? HeightMm { get; set; }

        [Column("width_mm")]
        public decimal? WidthMm { get; set; }
        
        [Column("preform_type")]
        public string? PreformType { get; set; } 

        public virtual ICollection<BillOfMaterials> BomVersions { get; set; }
        public virtual ICollection<StockMove> StockMoves { get; set; }
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; }
         public virtual ICollection<ProductPositionLink> ProductPositionLinks { get; set; } = new List<ProductPositionLink>();

    }

    /// <summary>
    /// ������� ������
    /// </summary>
    [Table("work_centers")]
    public class WorkCenter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Column("capacity_per_hour")]
        public double CapacityPerHour { get; set; } = 1.0;

        [Column("cost_per_hour")]
        public double CostPerHour { get; set; } = 0.0;
        [Column("rolling_speed")]
        public double RollingSpeed {get; set; } = 0.0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<WorkOrder> WorkOrders { get; set; }
    }

    /// <summary>
    /// ������������ (�� ������������)
    /// </summary>
    [Table("bill_of_materials")]
    public class BillOfMaterials
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Column("version")]
        [StringLength(20)]
        public string Version { get; set; } = "1.0";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<BomLine> Lines { get; set; }
    }

    /// <summary>
    /// ������ ������������ (�� ������������)
    /// </summary>
    [Table("bom_lines")]
    public class BomLine
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bom_id")]
        public int BomId { get; set; }

        [ForeignKey("BomId")]
        public virtual BillOfMaterials Bom { get; set; }

        [Column("component_product_id")]
        public int ComponentProductId { get; set; }

        [ForeignKey("ComponentProductId")]
        public virtual Product ComponentProduct { get; set; }

        [Column("quantity")]
        public double Quantity { get; set; }

        [Column("sequence")]
        public int Sequence { get; set; } = 10;
    }
    
    /// <summary>
    /// ���������������� ������ ������������
    /// </summary>
    [Table("production_orders")]
    public class ProductionOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Column("quantity")]
        public double Quantity { get; set; }

        [Column("state")]
        public OrderState State { get; set; } = OrderState.Draft;

        [Column("date_start_planned")]
        public DateTime DateStartPlanned { get; set; }

        [Column("date_finish_planned")]
        public DateTime DateFinishPlanned { get; set; }

        [Column("priority")]
        public int Priority { get; set; } = 0;

        [Column("created_by_user_id")]
        public int? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }

        public virtual ICollection<WorkOrder> WorkOrders { get; set; }
    }

    public class ProductionOrderInput
    {
        [Required(ErrorMessage = "�������� �������")]
        public int ProductId { get; set; }

        [Required, Range(0.01, 1000000, ErrorMessage = "���������� ������ ���� ������ 0")]
        public double Quantity { get; set; }

        public int Priority { get; set; } = 0;

        public int? AssignedToUserId { get; set; }

        public OrderState State { get; set; }

        public DateTime DateStartPlanned { get; set; }
        public DateTime DateFinishPlanned { get; set; }
    }

    /// <summary>
    /// ������ (������������ �����)
    /// </summary>
    [Table("work_orders")]
    public class WorkOrder
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("production_order_id")]
        public int ProductionOrderId { get; set; }

        [ForeignKey("ProductionOrderId")]
        public virtual ProductionOrder ProductionOrder { get; set; }

        [Column("work_center_id")]
        public int WorkCenterId { get; set; }

        [ForeignKey("WorkCenterId")]
        public virtual WorkCenter WorkCenter { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("sequence")]
        public int Sequence { get; set; }

        [Column("duration_expected")]
        public double DurationExpectedHours { get; set; }

        [Column("date_start_planned")]
        public DateTime DateStartPlanned { get; set; }

        [Column("date_end_planned")]
        public DateTime DateEndPlanned { get; set; }

        [Column("position_id")]
        public int PositionId { get; set; }

        [ForeignKey("PositionId")]

        public virtual Position Position { get; set; }

        [Column("status")]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;
    }

    /// <summary>
    /// ����������� �� ������ (�� ������������)
    /// </summary>
    [Table("stock_moves")]
    public class StockMove
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Column("production_order_id")]
        public int? ProductionOrderId { get; set; }

        [ForeignKey("ProductionOrderId")]
        public virtual ProductionOrder ProductionOrder { get; set; }

        [Column("type")]
        [StringLength(10)]
        public string MoveType { get; set; } 

        [Column("quantity")]
        public double Quantity { get; set; }

        [Column("date_expected")]
        public DateTime DateExpected { get; set; }

        [Column("state")]
        [StringLength(50)]
        public string State { get; set; } = "draft";
    }
}