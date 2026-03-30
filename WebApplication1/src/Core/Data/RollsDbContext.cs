using Microsoft.EntityFrameworkCore;
using WebApplication1.Entity;
namespace WebApplication1.Core.Data;

public sealed class RollsDbContext(DbContextOptions<RollsDbContext> options) : DbContext(options)
{
    public DbSet<Roll> Rolls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Roll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Length).IsRequired();
            entity.Property(e => e.Weight).IsRequired();
            entity.Property(e => e.DateAdd).IsRequired();
            entity.Property(e => e.DateRemove).IsRequired(false);
        });
    }
}