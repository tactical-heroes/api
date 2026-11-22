using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Write;

internal sealed class GameConfiguration : AuditableEntityConfiguration<Game>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(game => game.Id);
        builder.Property(game => game.Id)
            .HasConversion(id => id.Value, value => GameId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(game => game.Mode)
            .HasConversion(mode => mode.Name, value => GameMode.Create(value).Value)
            .HasMaxLength(GameMode.MaxLength)
            .IsRequired();
        builder.Property(game => game.Status)
            .HasConversion(status => status.Name, value => GameStatus.Create(value).Value)
            .HasMaxLength(GameStatus.MaxLength)
            .IsRequired();
        builder.Property(game => game.BattleId)
            .HasConversion(id => id!.Value.Value, value => BattleId.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(game => game.StartedAt)
            .HasConversion(timestamp => timestamp!.Value, value => GameTimestamp.Create(value))
            .IsRequired(required: false);
        builder.Property(game => game.FinishedAt)
            .HasConversion(timestamp => timestamp!.Value, value => GameTimestamp.Create(value))
            .IsRequired(required: false);

        builder.ComplexProperty(
            game => game.Result,
            result =>
            {
                result.IsRequired(required: false);
                result.Property(value => value.IsDraw).IsRequired();
                result.Property(value => value.WinnerPlayerId)
                    .HasConversion(id => id!.Value.Value, value => PlayerId.Create(value).Value)
                    .IsRequired(required: false);
            });

        builder.HasIndex(game => game.BattleId).IsUnique();
        builder.HasIndex(game => game.Status);

        builder.HasMany(game => game.Participants)
            .WithOne()
            .HasForeignKey(participant => participant.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(game => game.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
