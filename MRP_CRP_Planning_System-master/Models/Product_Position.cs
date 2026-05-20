using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetallurgyAnalytics.Models
{
    [Table("product_positions")]
    public class ProductPositionLink
    {
        [Column("products_id")]
        public int ProductId { get; set; }

        [Column("positions_id")]
        public int PositionId { get; set; }

        public virtual Product Product { get; set; }

        // Ссылка на позицию
        public virtual Position Position { get; set; }
    }

    
}