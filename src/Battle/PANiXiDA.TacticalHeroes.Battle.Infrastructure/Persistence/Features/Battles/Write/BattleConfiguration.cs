using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

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

        builder.Property(battle => battle.GameId)
            .HasConversion(id => id!.Value.Value, value => GameId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(battle => battle.FirstPlayerId)
            .HasConversion(id => id!.Value.Value, value => PlayerId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(battle => battle.SecondPlayerId)
            .HasConversion(id => id!.Value.Value, value => PlayerId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(battle => battle.Status)
            .HasConversion(status => status.Name, value => BattleStatus.Create(value).Value)
            .HasMaxLength(BattleStatus.MaxLength)
            .IsRequired();
        builder.HasIndex(battle => battle.GameId).IsUnique();

        builder.HasMany(battle => battle.Heroes)
            .WithOne()
            .HasForeignKey(hero => hero.BattleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(battle => battle.Heroes)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(battle => battle.Units)
            .WithOne()
            .HasForeignKey(unit => unit.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(battle => battle.Units)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
