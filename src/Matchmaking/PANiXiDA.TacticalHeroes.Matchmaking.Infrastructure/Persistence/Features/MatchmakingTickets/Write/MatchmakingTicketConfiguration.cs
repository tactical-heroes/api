using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Write;

internal sealed class MatchmakingTicketConfiguration : AuditableEntityConfiguration<MatchmakingTicket>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MatchmakingTicket> builder)
    {
        builder.HasKey(value => value.Id);
        builder.Property(value => value.Id)
            .HasConversion(value => value.Value, value => MatchmakingTicketId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(value => value.PlayerId)
            .HasConversion(value => value.Value, value => PlayerReference.Create(value).Value)
            .IsRequired();
        builder.Property(value => value.BuildSnapshotId)
            .HasConversion(value => value.Value, value => BuildSnapshotReference.Create(value).Value)
            .IsRequired();
        builder.Property(value => value.Rating)
            .HasConversion(value => value.Value, value => RatingSnapshot.Create(value))
            .IsRequired();
        builder.Property(value => value.Mode)
            .HasConversion(value => value.Value, value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();
        builder.Property(value => value.QueuedAt)
            .HasConversion(value => value.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired();
        builder.Property(value => value.ExpiresAt)
            .HasConversion(value => value.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired();
        builder.Property(value => value.Status)
            .HasConversion(value => value.Name, value => MatchmakingTicketStatus.Create(value).Value)
            .HasMaxLength(MatchmakingTicketStatus.MaxLength)
            .IsRequired();
        builder.Property(value => value.MatchAssignmentId)
            .HasConversion(value => value!.Value.Value, value => MatchAssignmentId.Create(value).Value)
            .IsRequired(required: false);
        builder.HasOne<MatchAssignment>().WithMany().HasForeignKey(value => value.MatchAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(value => value.PlayerId).IsUnique()
            .HasFilter("deleted_at IS NULL AND status IN ('Searching', 'Reserved')");
        builder.HasIndex(value => new { value.Mode, value.Status, value.Rating, value.QueuedAt });
        builder.HasIndex(value => new { value.Status, value.ExpiresAt });
        builder.Metadata.AddCheckConstraint(
            "ck_ticket_window",
            "expires_at > queued_at");
        builder.Metadata.AddCheckConstraint(
            "ck_ticket_assignment",
            "status NOT IN ('Reserved', 'Assigned') OR match_assignment_id IS NOT NULL");
    }
}
