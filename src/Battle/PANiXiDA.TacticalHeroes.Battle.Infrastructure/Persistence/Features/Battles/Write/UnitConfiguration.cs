using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

using UnitDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Units.UnitId;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Write;

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(unit => unit.Id);
        builder.Property(unit => unit.Id)
            .HasConversion(id => id.Value, value => UnitId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(unit => unit.BattleId)
            .HasConversion(
                id => id.Value,
                value => BattleId.Create(value).Value)
            .IsRequired();

        builder.Property(unit => unit.OwnerPlayerId)
            .HasConversion(id => id!.Value.Value, value => PlayerId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(unit => unit.DefinitionId)
            .HasConversion(id => id!.Value.Value, value => UnitDefinitionId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(unit => unit.FactionId)
            .HasConversion(id => id!.Value.Value, value => FactionId.Create(value).Value)
            .IsRequired(required: false);
        builder.ComplexProperty(
            unit => unit.Traits,
            traits =>
            {
                traits.IsRequired(required: false);
                traits.Property(value => value.Initiative).IsRequired();
                traits.Property(value => value.Speed).IsRequired();
                traits.Property(value => value.Morale).IsRequired();
                traits.Property(value => value.Luck).IsRequired();
                traits.Property(value => value.Shots).IsRequired(required: false);
                traits.Property(value => value.RangedAttackRange).IsRequired(required: false);
            });

        builder.ComplexProperty(
            unit => unit.Stats,
            stats =>
            {
                stats.Property(value => value.Attack).IsRequired();
                stats.Property(value => value.Defense).IsRequired();
                stats.Property(value => value.MaxHealth).IsRequired();
            });
        builder.ComplexProperty(
            unit => unit.DamageRange,
            range =>
            {
                range.Property(value => value.Minimum).IsRequired();
                range.Property(value => value.Maximum).IsRequired();
            });
        builder.ComplexProperty(
            unit => unit.StackState,
            state =>
            {
                state.Property(value => value.Count).IsRequired();
                state.Property(value => value.CurrentUnitHealth).IsRequired();
            });

        builder.Ignore(unit => unit.Count);
        builder.Ignore(unit => unit.CurrentUnitHealth);
        builder.Ignore(unit => unit.TotalHealth);
        builder.Ignore(unit => unit.IsAlive);
    }
}
