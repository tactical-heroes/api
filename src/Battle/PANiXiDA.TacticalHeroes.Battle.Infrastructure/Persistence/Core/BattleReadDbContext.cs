using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

public sealed class BattleReadDbContext(
    DbContextOptions<BattleReadDbContext> options)
    : ReadDbContext<BattleReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BattleReadDbModel>(builder =>
        {
            builder.HasMany(battle => battle.Units)
                .WithOne()
                .HasForeignKey(unit => unit.BattleId);
        });
    }
}
