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
        builder.HasKey(unit => unit.Id);

        builder.Property(unit => unit.Id)
            .HasConversion(
                id => id.Value,
                value => new UnitId(value))
            .ValueGeneratedNever();

        builder.Property(unit => unit.Name)
            .HasConversion(
                name => name.Value,
                value => UnitName.Create(value).Value)
            .HasMaxLength(UnitName.MaxLength)
            .IsRequired();

        builder.Property(unit => unit.Description)
            .HasConversion(
                description => description.Value,
                value => UnitDescription.Create(value).Value)
            .HasMaxLength(UnitDescription.MaxLength)
            .IsRequired();

        builder.ComplexProperty(unit => unit.Stats, stats =>
        {
            stats.Property(value => value.Attack)
                .IsRequired();

            stats.Property(value => value.Defense)
                .IsRequired();

            stats.Property(value => value.Health)
                .IsRequired();

            stats.Property(value => value.MinimumDamage)
                .IsRequired();

            stats.Property(value => value.MaximumDamage)
                .IsRequired();

            stats.Property(value => value.Initiative)
                .IsRequired();

            stats.Property(value => value.Speed)
                .IsRequired();

            stats.Property(value => value.Shots);

            stats.Property(value => value.RangedAttackRange);
        });

        builder.Property(unit => unit.Morale)
            .HasConversion(
                morale => morale.Value,
                value => UnitMorale.Create(value).Value)
            .IsRequired();

        builder.Property(unit => unit.Luck)
            .HasConversion(
                luck => luck.Value,
                value => UnitLuck.Create(value).Value)
            .IsRequired();

        builder.Property(unit => unit.FactionId)
            .HasConversion(
                id => id.Value,
                value => new FactionId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(unit => unit.FactionId);

        builder.HasOne<Faction>()
            .WithMany()
            .HasForeignKey(unit => unit.FactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
