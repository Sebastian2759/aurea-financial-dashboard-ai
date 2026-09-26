using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Configuration;
public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEventEntity>
{
    public void Configure(EntityTypeBuilder<AuditEventEntity> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OwnerId).HasMaxLength(64);
        builder.Property(x => x.Action).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Resource).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Before).HasMaxLength(2000);
        builder.Property(x => x.After).HasMaxLength(2000);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
