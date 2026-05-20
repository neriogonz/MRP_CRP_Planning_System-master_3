using Microsoft.EntityFrameworkCore;
using MetallurgyAnalytics.Models;

namespace MetallurgyAnalytics.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Position> Positions { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<WorkCenter> WorkCenters { get; set; }
        public DbSet<BillOfMaterials> BillOfMaterials { get; set; }
        public DbSet<BomLine> BomLines { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<StockMove> StockMoves { get; set; }
        public DbSet<Formula> Formulas { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ProductPositionLink> ProductPositionLinks { get; set; }

        // public DbSet<GanttResourceDto> GanttResourceDto { get; set; }

        // public DbSet<GanttTaskDto> GanttTaskDto { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // modelBuilder.Entity<Product>()
            //     .HasMany(p => p.Positions)
            //     .WithMany(p => p.Products)
            //     .UsingEntity(j => j.ToTable("product_positions"));


            modelBuilder.Entity<ProductPositionLink>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.PositionId });

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ProductPositionLinks)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Position)
                      .WithMany(p => p.ProductPositionLinks)
                      .HasForeignKey(e => e.PositionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("pozitsiya");
                    
                entity.Property(e => e.Preform)
                    .HasMaxLength(255)
                    .HasColumnName("zagotovka");
                    
                entity.Property(e => e.Hmin)
                    .HasColumnType("numeric(10,2)")
                    .HasColumnName("hmin");
                    
                entity.Property(e => e.Hmax)
                    .HasColumnType("numeric(10,2)")
                    .HasColumnName("hmax");
                    
                entity.Property(e => e.Bmin)
                    .HasColumnType("numeric(10,2)")
                    .HasColumnName("bmin");
                    
                entity.Property(e => e.Bmax)
                    .HasColumnType("numeric(10,2)")
                    .HasColumnName("bmax");
                    
                entity.Property(e => e.SteelGradeGroup)
                    .HasColumnType("numeric(10,2)")
                    .HasColumnName("gruppamarokstali");

                entity.ToTable("positions");
            });
        }
    }
}