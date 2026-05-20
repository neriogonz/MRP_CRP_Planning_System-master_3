using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetallurgyAnalytics.Models
{
    /// <summary>
    /// Номенклатурные позиции
    /// </summary>
    [Table("positions")]
    public class Position
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("preform")]
        public string? Preform { get; set; }

        [Column("Hmin")]
        public decimal Hmin { get; set; }

        [Column("Hmax")]
        public decimal Hmax { get; set; }

        [Column("Bmin")]
        public decimal Bmin { get; set; }

        [Column("Bmax")]
        public decimal Bmax { get; set; }

        [Column("steel_grade_group")]
        public decimal SteelGradeGroup { get; set; } //не знаю нужны ли нам тут вообще марки стали, можно убрать в принципе

        public virtual ICollection<WorkOrder> WorkOrders { get; set; }

         public virtual ICollection<ProductPositionLink> ProductPositionLinks { get; set; } = new List<ProductPositionLink>();

        public string GetVisibleName() => Name ?? Preform ?? "(нет названия)";
    }
}