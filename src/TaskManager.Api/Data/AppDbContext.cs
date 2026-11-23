using TaskManager.Api.Entities;

namespace TaskManager.Api.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        EntityTypeBuilder<TaskItem> entity = modelBuilder.Entity<TaskItem>();
        
        entity.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        entity.Property(t => t.Description)
            .HasMaxLength(2000);
        
        entity.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        entity.Property(t => t.DueDate)
            .IsRequired();

        entity.Property(t => t.CreatedAt)
            .IsRequired();

        entity.Property(t => t.UpdatedAt)
            .IsRequired();
    }
}