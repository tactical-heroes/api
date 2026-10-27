using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Write;

internal sealed class GameParticipantConfiguration : IEntityTypeConfiguration<GameParticipant>
{
    public void Configure(EntityTypeBuilder<GameParticipant> builder)
    {
        builder.HasKey(participant => participant.Id);
        builder.Property(participant => participant.Id)
            .HasConversion(id => id.Value, value => GameParticipantId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(participant => participant.GameId)
            .HasConversion(id => id.Value, value => GameId.Create(value).Value)
            .IsRequired();
        builder.Property(participant => participant.PlayerId)
            .HasConversion(id => id.Value, value => PlayerId.Create(value).Value)
            .IsRequired();
        builder.Property(participant => participant.BuildId)
            .HasConversion(id => id.Value, value => BuildId.Create(value).Value)
            .IsRequired();
        builder.Property(participant => participant.Rating)
            .HasConversion(rating => rating.Value, value => Rating.Create(value))
            .IsRequired();
        builder.Property(participant => participant.Side)
            .HasConversion(side => side.Name, value => GameSide.Create(value).Value)
            .HasMaxLength(GameSide.MaxLength)
            .IsRequired();

        builder.HasIndex(participant => new { participant.GameId, participant.PlayerId }).IsUnique();
        builder.HasIndex(participant => new { participant.GameId, participant.Side }).IsUnique();
    }
}
