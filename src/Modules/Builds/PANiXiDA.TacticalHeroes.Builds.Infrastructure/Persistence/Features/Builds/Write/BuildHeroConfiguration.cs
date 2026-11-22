using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds.Entities.BuildHeroes;
using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Features.Builds.Write;

internal sealed class BuildHeroConfiguration : IEntityTypeConfiguration<BuildHero>
{
    public void Configure(EntityTypeBuilder<BuildHero> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => BuildHeroId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(item => item.BuildId)
            .HasConversion(id => id.Value, value => BuildId.Create(value).Value)
            .IsRequired();
        builder.Property(item => item.HeroId)
            .HasConversion(id => id.Value, value => HeroId.Create(value).Value)
            .IsRequired();

        builder.HasIndex(item => new { item.BuildId, item.HeroId }).IsUnique();
    }
}
