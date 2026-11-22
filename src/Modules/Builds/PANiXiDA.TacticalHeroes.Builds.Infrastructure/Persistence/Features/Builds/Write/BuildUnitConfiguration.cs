using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;
using PANiXiDA.TacticalHeroes.Builds.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Write;

internal sealed class BuildUnitConfiguration : IEntityTypeConfiguration<BuildUnit>
{
    public void Configure(EntityTypeBuilder<BuildUnit> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => BuildUnitId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(item => item.BuildId)
            .HasConversion(id => id.Value, value => BuildId.Create(value).Value)
            .IsRequired();
        builder.Property(item => item.UnitId)
            .HasConversion(id => id.Value, value => UnitId.Create(value).Value)
            .IsRequired();

        builder.HasIndex(item => new { item.BuildId, item.UnitId }).IsUnique();
    }
}
