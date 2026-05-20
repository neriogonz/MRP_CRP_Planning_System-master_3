using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetallurgyAnalytics.Models
{

    [Table("material_task")]
    public class ProjectTask
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("date_start")]
        public DateTime? DateStart { get; set; }

        [Column("state")] 
        public string? State { get; set; }
        
        [Column("project_id")]
        public int? ProjectId { get; set; }

    }
}