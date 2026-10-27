using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Write;

internal sealed class MatchParticipantConfiguration : IEntityTypeConfiguration<MatchParticipant>
{
    public void Configure(EntityTypeBuilder<MatchParticipant> builder)
    {
        builder.HasKey(participant => participant.Id);

        builder.Property(participant => participant.Id)
            .HasConversion(
                id => id.Value,
                value => MatchParticipantId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(participant => participant.MatchId)
            .HasConversion(
                matchId => matchId.Value,
                value => MatchId.Create(value).Value);

        builder.Property(participant => participant.MatchSearchId)
            .HasConversion(
                matchSearchId => matchSearchId.Value,
                value => MatchSearchId.Create(value).Value);

        builder.Property(participant => participant.PlayerId)
            .HasConversion(
                playerId => playerId.Value,
                value => PlayerId.Create(value).Value)
            .IsRequired();

        builder.Property(participant => participant.BuildId)
            .HasConversion(
                buildId => buildId.Value,
                value => BuildId.Create(value).Value)
            .IsRequired();

        builder.Property(participant => participant.Rating)
            .HasConversion(
                rating => rating.Value,
                value => RatingSnapshot.Create(value))
            .IsRequired();

        builder.HasOne<MatchSearch>()
            .WithMany()
            .HasForeignKey(participant => participant.MatchSearchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(participant => participant.MatchSearchId).IsUnique();
        builder.HasIndex(participant => new { participant.MatchId, participant.PlayerId }).IsUnique();
    }
}
