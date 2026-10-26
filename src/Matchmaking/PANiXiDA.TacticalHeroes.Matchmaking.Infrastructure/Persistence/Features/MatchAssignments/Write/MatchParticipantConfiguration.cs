using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Write;

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

        builder.Property(participant => participant.MatchAssignmentId)
            .HasConversion(
                assignmentId => assignmentId.Value,
                value => MatchAssignmentId.Create(value).Value);

        builder.Property(participant => participant.TicketId)
            .HasConversion(
                ticketId => ticketId.Value,
                value => MatchmakingTicketId.Create(value).Value);

        builder.Property(participant => participant.PlayerId)
            .HasConversion(
                playerId => playerId.Value,
                value => PlayerId.Create(value).Value)
            .IsRequired();

        builder.Property(participant => participant.BuildSnapshotId)
            .HasConversion(
                snapshotId => snapshotId.Value,
                value => BuildSnapshotId.Create(value).Value)
            .IsRequired();

        builder.Property(participant => participant.Rating)
            .HasConversion(
                rating => rating.Value,
                value => RatingSnapshot.Create(value))
            .IsRequired();

        builder.HasOne<MatchmakingTicket>()
            .WithMany()
            .HasForeignKey(participant => participant.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(participant => participant.TicketId).IsUnique();
        builder.HasIndex(participant => new { participant.MatchAssignmentId, participant.PlayerId }).IsUnique();
    }
}
