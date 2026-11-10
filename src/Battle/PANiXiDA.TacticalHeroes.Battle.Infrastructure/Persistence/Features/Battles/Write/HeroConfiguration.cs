using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

using HeroDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Heroes.HeroId;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Write;

internal sealed class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.HasKey(hero => hero.Id);
        builder.Property(hero => hero.Id)
            .HasConversion(id => id.Value, value => HeroId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(hero => hero.BattleId)
            .HasConversion(id => id.Value, value => BattleId.Create(value).Value)
            .IsRequired();
        builder.Property(hero => hero.OwnerPlayerId)
            .HasConversion(id => id.Value, value => PlayerId.Create(value).Value)
            .IsRequired();
        builder.Property(hero => hero.DefinitionId)
            .HasConversion(id => id.Value, value => HeroDefinitionId.Create(value).Value)
            .IsRequired();
        builder.Property(hero => hero.FactionId)
            .HasConversion(id => id.Value, value => FactionId.Create(value).Value)
            .IsRequired();
        builder.ComplexProperty(
            hero => hero.Stats,
            stats =>
            {
                stats.Property(value => value.Attack).IsRequired();
                stats.Property(value => value.Defense).IsRequired();
                stats.Property(value => value.MinimumDamage).IsRequired();
                stats.Property(value => value.MaximumDamage).IsRequired();
                stats.Property(value => value.Initiative).IsRequired();
                stats.Property(value => value.Morale).IsRequired();
                stats.Property(value => value.Luck).IsRequired();
            });
    }
}
