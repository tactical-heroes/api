using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Write;

internal sealed class MatchConfiguration : AuditableEntityConfiguration<Match>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(match => match.Id);

        builder.Property(match => match.Id)
            .HasConversion(
                id => id.Value,
                value => MatchId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(match => match.Mode)
            .HasConversion(
                mode => mode.Value,
                value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();

        builder.Property(match => match.MatchedAt)
            .HasConversion(
                matchedAt => matchedAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(match => match.CreationDeadline)
            .HasConversion(
                deadline => deadline.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(match => match.GameId)
            .HasConversion(
                gameId => gameId!.Value.Value,
                value => GameId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(match => match.Status)
            .HasConversion(
                status => status.Name,
                value => MatchStatus.Create(value).Value)
            .HasMaxLength(MatchStatus.MaxLength)
            .IsRequired();

        builder.HasMany(match => match.Participants)
            .WithOne()
            .HasForeignKey(participant => participant.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(match => match.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(match => match.GameId).IsUnique();
        builder.HasIndex(match => new { match.Status, match.CreationDeadline });
    }
}
