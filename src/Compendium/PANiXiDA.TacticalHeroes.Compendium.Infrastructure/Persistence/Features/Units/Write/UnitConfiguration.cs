using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Write;

internal sealed class UnitConfiguration : AuditableEntityConfiguration<Unit>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(keyExpression: unit => unit.Id);

        builder.Property(propertyExpression: unit => unit.Id)
            .HasConversion(
                convertToProviderExpression: id => id.Value,
                convertFromProviderExpression: value => UnitId.Create(value: value).Value)
            .ValueGeneratedNever();

        builder.Property(propertyExpression: unit => unit.Name)
            .HasConversion(
                convertToProviderExpression: name => name.Value,
                convertFromProviderExpression: value => UnitName.Create(value: value).Value)
            .HasMaxLength(maxLength: UnitName.MaxLength)
            .IsRequired();

        builder.Property(propertyExpression: unit => unit.Description)
            .HasConversion(
                convertToProviderExpression: description => description.Value,
                convertFromProviderExpression: value => UnitDescription.Create(value: value).Value)
            .HasMaxLength(maxLength: UnitDescription.MaxLength)
            .IsRequired();

        builder.ComplexProperty(propertyExpression: unit => unit.Stats, buildAction: stats =>
        {
            stats.Property(propertyExpression: value => value.Attack)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Defense)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Health)
                .IsRequired();

            stats.Property(propertyExpression: value => value.MinimumDamage)
                .IsRequired();

            stats.Property(propertyExpression: value => value.MaximumDamage)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Initiative)
                .IsRequired();

            stats.Property(propertyExpression: value => value.Speed)
                .IsRequired();
        });

        builder.ComplexProperty(propertyExpression: unit => unit.RangedAttack, buildAction: rangedAttack =>
        {
            rangedAttack.Property(propertyExpression: value => value.Shots);

            rangedAttack.Property(propertyExpression: value => value.RangedAttackRange);
        });

        builder.Property(propertyExpression: unit => unit.Morale)
            .HasConversion(
                convertToProviderExpression: morale => morale.Value,
                convertFromProviderExpression: value => UnitMorale.Create(value: value).Value)
            .IsRequired();

        builder.Property(propertyExpression: unit => unit.Luck)
            .HasConversion(
                convertToProviderExpression: luck => luck.Value,
                convertFromProviderExpression: value => UnitLuck.Create(value: value).Value)
            .IsRequired();

        builder.Property(propertyExpression: unit => unit.FactionId)
            .HasConversion(
                convertToProviderExpression: id => id.Value,
                convertFromProviderExpression: value => FactionId.Create(value: value).Value)
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(indexExpression: unit => unit.FactionId);

        builder.HasOne<Faction>()
            .WithMany()
            .HasForeignKey(foreignKeyExpression: unit => unit.FactionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);
    }
}
