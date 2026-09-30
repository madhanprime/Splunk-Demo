using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Models;

namespace EmployeeManagement.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial realistic employee records
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 101, FirstName = "Sarah", LastName = "Connor", Email = "s.connor@cyberdyne.com", Department = "Engineering", Salary = 95000, Role = "Senior Developer", CreatedAt = DateTime.UtcNow.AddDays(-30) },
                new Employee { Id = 102, FirstName = "Alex", LastName = "Murphy", Email = "a.murphy@ocp.corp", Department = "Security", Salary = 88000, Role = "DevSecOps Lead", CreatedAt = DateTime.UtcNow.AddDays(-25) },
                new Employee { Id = 103, FirstName = "Elena", LastName = "Rostova", Email = "elena.r@techcorp.io", Department = "Product", Salary = 92000, Role = "Product Manager", CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Employee { Id = 104, FirstName = "David", LastName = "Kim", Email = "d.kim@techcorp.io", Department = "Engineering", Salary = 78000, Role = "Full Stack Engineer", CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new Employee { Id = 105, FirstName = "Priya", LastName = "Sharma", Email = "p.sharma@techcorp.io", Department = "Data Analytics", Salary = 105000, Role = "Splunk Architect", CreatedAt = DateTime.UtcNow.AddDays(-10) }
            );
        }
    }
}
