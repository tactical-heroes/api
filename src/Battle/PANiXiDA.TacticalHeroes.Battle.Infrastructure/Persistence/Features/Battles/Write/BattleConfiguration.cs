using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Write;

internal sealed class BattleConfiguration : AuditableEntityConfiguration<BattleAggregate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BattleAggregate> builder)
    {
        builder.HasKey(battle => battle.Id);
        builder.Property(battle => battle.Id)
            .HasConversion(id => id.Value, value => BattleId.Create(value).Value)
            .ValueGeneratedNever();

        builder.HasMany(battle => battle.Units)
            .WithOne()
            .HasForeignKey(unit => unit.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(battle => battle.Units).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
