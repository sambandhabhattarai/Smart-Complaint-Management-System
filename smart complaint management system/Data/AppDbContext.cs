using Microsoft.EntityFrameworkCore;
using smart_complaint_management_system.Models;

namespace smart_complaint_management_system.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> users { get; set; }
        public DbSet<Complaints> complaints { get; set; }
        public DbSet<Employees> employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Complaints>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Complaints>()
                .HasOne(c => c.AssignedEmployee)
                .WithMany(e => e.Complaints)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull); 
        }
    }
}
