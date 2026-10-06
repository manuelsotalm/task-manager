namespace TaskManagement.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TaskEntity = TaskManagement.Domain.Entities.Task;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.ValueObjects;
using TaskManagement.Infrastructure.Persistence.Configurations;

public class AppDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
    }
}