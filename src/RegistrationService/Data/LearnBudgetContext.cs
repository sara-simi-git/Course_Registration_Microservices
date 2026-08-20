using Microsoft.EntityFrameworkCore;
using RegistrationService.Enums;
using RegistrationService.Models;

namespace RegistrationService.Data
{
    public class LearnBudgetContext : DbContext
    {
        public LearnBudgetContext(DbContextOptions<LearnBudgetContext> options) : base(options)
        {
        }

        public DbSet<CourseRegistration> CourseRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourseRegistration>().ToTable("course_registrations");
            
            modelBuilder.Entity<CourseRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EmployeeId, e.CourseId });
            });
        }
    }
}