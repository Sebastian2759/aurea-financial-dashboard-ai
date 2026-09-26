using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Configuration;
public sealed class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItemEntity>
{
    public void Configure(EntityTypeBuilder<WatchlistItemEntity> builder)
    {
        builder.ToTable("WatchlistItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OwnerId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CoinId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(240);
        builder.HasIndex(x => new { x.OwnerId, x.CoinId }).IsUnique();
    }
}
