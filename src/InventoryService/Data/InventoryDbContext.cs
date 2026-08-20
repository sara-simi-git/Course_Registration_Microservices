using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<CourseInventory> CourseInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourseInventory>().ToTable("course_inventories");

            modelBuilder.Entity<CourseInventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CourseId).IsUnique();
            });
        }
    }
}
