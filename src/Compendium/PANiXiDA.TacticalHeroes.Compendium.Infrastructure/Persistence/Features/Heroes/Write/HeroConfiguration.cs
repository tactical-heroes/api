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
        builder.HasKey(keyExpression: hero => hero.Id);

        builder.Property(propertyExpression: hero => hero.Id)
            .HasConversion(
                convertToProviderExpression: id => id.Value,
                convertFromProviderExpression: value => HeroId.Create(value: value).Value)
            .ValueGeneratedNever();

        builder.Property(propertyExpression: hero => hero.Name)
            .HasConversion(
                convertToProviderExpression: name => name.Value,
                convertFromProviderExpression: value => HeroName.Create(value: value).Value)
            .HasMaxLength(maxLength: HeroName.MaxLength)
            .IsRequired();

        builder.Property(propertyExpression: hero => hero.Description)
            .HasConversion(
                convertToProviderExpression: description => description.Value,
                convertFromProviderExpression: value => HeroDescription.Create(value: value).Value)
            .HasMaxLength(maxLength: HeroDescription.MaxLength)
            .IsRequired();

        builder.ComplexProperty(propertyExpression: hero => hero.Stats, buildAction: stats =>
        {
            stats.Property(propertyExpression: value => value.Attack)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Defense)
                .IsRequired();

            stats.Property(propertyExpression: value => value.MinimumDamage)
                .IsRequired();

            stats.Property(propertyExpression: value => value.MaximumDamage)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Initiative)
                .IsRequired();
        });

        builder.Property(propertyExpression: hero => hero.Morale)
            .HasConversion(
                convertToProviderExpression: morale => morale.Value,
                convertFromProviderExpression: value => HeroMorale.Create(value: value).Value)
            .IsRequired();

        builder.Property(propertyExpression: hero => hero.Luck)
            .HasConversion(
                convertToProviderExpression: luck => luck.Value,
                convertFromProviderExpression: value => HeroLuck.Create(value: value).Value)
            .IsRequired();

        builder.Property(propertyExpression: hero => hero.FactionId)
            .HasConversion(
                convertToProviderExpression: id => id.Value,
                convertFromProviderExpression: value => FactionId.Create(value: value).Value)
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(indexExpression: hero => hero.FactionId);

        builder.HasOne<Faction>()
            .WithMany()
            .HasForeignKey(foreignKeyExpression: hero => hero.FactionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);
    }
}
