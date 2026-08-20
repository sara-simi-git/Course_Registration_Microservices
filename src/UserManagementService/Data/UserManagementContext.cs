using Microsoft.EntityFrameworkCore;
using UserManagementService.Models;

namespace UserManagementService.Data
{
    public class UserManagementContext : DbContext
    {
        public UserManagementContext(DbContextOptions<UserManagementContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TotalCredits).IsRequired();
                entity.Property(e => e.UsedCredits).IsRequired();
                entity.Ignore(e => e.AvailableCredits);

                entity.HasOne(e => e.User)
                      .WithOne(u => u.Employee)
                      .HasForeignKey<Employee>("UserId")
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();
            });
        }
    }
}
