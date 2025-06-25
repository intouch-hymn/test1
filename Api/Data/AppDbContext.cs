using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

// Define DbSet for User entity 
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "สมชาย ใจดี", Email = "somchai@example.com", Age = 25 },
            new User { Id = 2, Name = "สมหญิง รักงาน", Email = "somying@example.com", Age = 28 },
            new User { Id = 3, Name = "John Doe", Email = "john@example.com", Age = 30 }
        );
    }
}