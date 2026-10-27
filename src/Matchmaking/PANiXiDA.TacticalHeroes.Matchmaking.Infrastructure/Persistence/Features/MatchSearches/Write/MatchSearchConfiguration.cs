using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Write;

internal sealed class MatchSearchConfiguration : AuditableEntityConfiguration<MatchSearch>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MatchSearch> builder)
    {
        builder.HasKey(search => search.Id);

        builder.Property(search => search.Id)
            .HasConversion(
                id => id.Value,
                value => MatchSearchId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(search => search.PlayerId)
            .HasConversion(
                playerId => playerId.Value,
                value => PlayerId.Create(value).Value)
            .IsRequired();

        builder.Property(search => search.BuildId)
            .HasConversion(
                buildId => buildId.Value,
                value => BuildId.Create(value).Value)
            .IsRequired();

        builder.Property(search => search.GameId)
            .HasConversion(
                id => id!.Value.Value,
                value => GameId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(search => search.Rating)
            .HasConversion(
                rating => rating.Value,
                value => Raiting.Create(value))
            .IsRequired();

        builder.Property(search => search.Mode)
            .HasConversion(
                mode => mode.Name,
                value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();

        builder.Property(search => search.QueuedAt)
            .HasConversion(
                queuedAt => queuedAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(search => search.ExpiresAt)
            .HasConversion(
                expiresAt => expiresAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(search => search.Status)
            .HasConversion(
                status => status.Name,
                value => MatchSearchStatus.Create(value).Value)
            .HasMaxLength(MatchSearchStatus.MaxLength)
            .IsRequired();

        builder.HasIndex(search => search.PlayerId)
            .IsUnique()
            .HasFilter("deleted_at IS NULL AND status IN ('Searching', 'Matched')");

        builder.HasIndex(search => new { search.Mode, search.Status, search.Rating, search.QueuedAt });
        builder.HasIndex(search => new { search.Status, search.ExpiresAt });
        builder.HasIndex(search => search.GameId);
    }
}
