using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Write;

internal sealed class HeroConfiguration : AuditableEntityConfiguration<Hero>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Hero> builder)
    {
        builder.HasKey(hero => hero.Id);

        builder.Property(hero => hero.Id)
            .HasConversion(
                id => id.Value,
                value => new HeroId(value))
            .ValueGeneratedNever();

        builder.Property(hero => hero.Name)
            .HasConversion(
                name => name.Value,
                value => HeroName.Create(value).Value)
            .HasMaxLength(HeroName.MaxLength)
            .IsRequired();

        builder.Property(hero => hero.Description)
            .HasConversion(
                description => description.Value,
                value => HeroDescription.Create(value).Value)
            .HasMaxLength(HeroDescription.MaxLength)
            .IsRequired();

        builder.ComplexProperty(hero => hero.Stats, stats =>
        {
            stats.Property(value => value.Attack)
                .IsRequired();

            stats.Property(value => value.Defense)
                .IsRequired();

            stats.Property(value => value.MinimumDamage)
                .IsRequired();

            stats.Property(value => value.MaximumDamage)
                .IsRequired();

            stats.Property(value => value.Initiative)
                .IsRequired();
        });

        builder.Property(hero => hero.Morale)
            .HasConversion(
                morale => morale.Value,
                value => HeroMorale.Create(value).Value)
            .IsRequired();

        builder.Property(hero => hero.Luck)
            .HasConversion(
                luck => luck.Value,
                value => HeroLuck.Create(value).Value)
            .IsRequired();

        builder.Property(hero => hero.FactionId)
            .HasConversion(
                id => id.Value,
                value => new FactionId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(hero => hero.FactionId);

        builder.HasOne<Faction>()
            .WithMany()
            .HasForeignKey(hero => hero.FactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
