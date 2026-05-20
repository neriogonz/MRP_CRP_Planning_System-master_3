using MetallurgyAnalytics.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetallurgyAnalytics.Models
{
    [Table("formulas")]
    public class Formula
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("end_product_id")]
        public int EndProductId { get; set; }

        [Column("ingredient_id")]
        public int IngredientId { get; set; }

        [Column("consumption_coeff")]
        public double ConsumptionCoeff { get; set; }

        [Column("work_center_id")]
        public int WorkCenterId { get; set; }

        public virtual Position EndProduct { get; set; }
        public virtual Position Ingredient { get; set; }
        public virtual WorkCenter WorkCenter { get; set; }
    }
}
