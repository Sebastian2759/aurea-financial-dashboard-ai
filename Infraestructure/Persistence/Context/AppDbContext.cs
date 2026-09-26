using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DemoArchitectureEntity> DemoArchitectures
        => Set<DemoArchitectureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        // Aplica todas las configuraciones IEntityTypeConfiguration<T> del ensamblado.
        // Cada entidad real debe agregar su clase <EntityName>Configuration en Persistence.Configuration.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
