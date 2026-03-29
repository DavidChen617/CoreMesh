using CoreMesh.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CoreMesh.Examples.Outbox.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public virtual DbSet<Entities.Todo> Todos { get; set; }
    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
