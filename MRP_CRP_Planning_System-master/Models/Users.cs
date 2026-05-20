// Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetallurgyAnalytics.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [Column("email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Column("full_name")]
        [StringLength(255)]
        public string FullName { get; set; }

        [Column("role")]
        [StringLength(50)]
        public string Role { get; set; } = "user"; // admin, planner, operator

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // Навигационные свойства
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; }
    }
}