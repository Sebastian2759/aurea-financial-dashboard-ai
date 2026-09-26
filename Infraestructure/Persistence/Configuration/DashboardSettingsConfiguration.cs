using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Configuration;
public sealed class DashboardSettingsConfiguration : IEntityTypeConfiguration<DashboardSettingsEntity>
{
    public void Configure(EntityTypeBuilder<DashboardSettingsEntity> builder)
    {
        builder.ToTable("DashboardSettings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.SingletonKey).IsUnique();
    }
}
