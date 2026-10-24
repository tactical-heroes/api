using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;

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
            .HasConversion(new ValueConverter<BattleId, Guid>(
                id => id.Value,
                value => BattleId.Create(value).Value))
            .IsRequired();

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
