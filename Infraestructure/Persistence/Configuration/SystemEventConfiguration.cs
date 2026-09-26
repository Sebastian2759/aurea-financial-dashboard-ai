using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Configuration;
public sealed class SystemEventConfiguration : IEntityTypeConfiguration<SystemEventEntity>
{
    public void Configure(EntityTypeBuilder<SystemEventEntity> builder)
    {
        builder.ToTable("SystemEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Level).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Component).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.CoinId).HasMaxLength(32);
        builder.HasIndex(x => new { x.CreatedAtUtc, x.Id });
    }
}
