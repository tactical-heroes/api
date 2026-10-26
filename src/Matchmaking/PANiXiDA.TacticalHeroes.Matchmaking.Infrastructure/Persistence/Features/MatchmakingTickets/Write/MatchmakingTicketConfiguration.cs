using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Write;

internal sealed class MatchmakingTicketConfiguration : AuditableEntityConfiguration<MatchmakingTicket>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MatchmakingTicket> builder)
    {
        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Id)
            .HasConversion(
                id => id.Value,
                value => MatchmakingTicketId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(ticket => ticket.PlayerId)
            .HasConversion(
                playerId => playerId.Value,
                value => PlayerId.Create(value).Value)
            .IsRequired();

        builder.Property(ticket => ticket.BuildSnapshotId)
            .HasConversion(
                snapshotId => snapshotId.Value,
                value => BuildSnapshotId.Create(value).Value)
            .IsRequired();

        builder.Property(ticket => ticket.Rating)
            .HasConversion(
                rating => rating.Value,
                value => RatingSnapshot.Create(value))
            .IsRequired();

        builder.Property(ticket => ticket.Mode)
            .HasConversion(
                mode => mode.Value,
                value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();

        builder.Property(ticket => ticket.QueuedAt)
            .HasConversion(
                queuedAt => queuedAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(ticket => ticket.ExpiresAt)
            .HasConversion(
                expiresAt => expiresAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(ticket => ticket.Status)
            .HasConversion(
                status => status.Name,
                value => MatchmakingTicketStatus.Create(value).Value)
            .HasMaxLength(MatchmakingTicketStatus.MaxLength)
            .IsRequired();

        builder.Property(ticket => ticket.MatchAssignmentId)
            .HasConversion(
                assignmentId => assignmentId!.Value.Value,
                value => MatchAssignmentId.Create(value).Value)
            .IsRequired(required: false);

        builder.HasOne<MatchAssignment>()
            .WithMany()
            .HasForeignKey(ticket => ticket.MatchAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ticket => ticket.PlayerId)
            .IsUnique()
            .HasFilter("deleted_at IS NULL AND status IN ('Searching', 'Reserved')");

        builder.HasIndex(ticket => new { ticket.Mode, ticket.Status, ticket.Rating, ticket.QueuedAt });
        builder.HasIndex(ticket => new { ticket.Status, ticket.ExpiresAt });
    }
}
