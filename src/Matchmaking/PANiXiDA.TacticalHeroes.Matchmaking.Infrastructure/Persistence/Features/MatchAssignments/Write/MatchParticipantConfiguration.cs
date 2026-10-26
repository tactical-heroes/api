using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Write;

internal sealed class MatchParticipantConfiguration : IEntityTypeConfiguration<MatchParticipant>
{
    public void Configure(EntityTypeBuilder<MatchParticipant> builder)
    {
        builder.HasKey(value => value.Id);
        builder.Property(value => value.Id)
            .HasConversion(value => value.Value, value => MatchParticipantId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(value => value.MatchAssignmentId)
            .HasConversion(value => value.Value, value => MatchAssignmentId.Create(value).Value);
        builder.Property(value => value.TicketId)
            .HasConversion(value => value.Value, value => MatchmakingTicketId.Create(value).Value);
        builder.Property(value => value.PlayerId)
            .HasConversion(value => value.Value, value => PlayerReference.Create(value).Value)
            .IsRequired();
        builder.Property(value => value.BuildSnapshotId)
            .HasConversion(value => value.Value, value => BuildSnapshotReference.Create(value).Value)
            .IsRequired();
        builder.Property(value => value.Rating)
            .HasConversion(value => value.Value, value => RatingSnapshot.Create(value))
            .IsRequired();
        builder.Property(value => value.AcceptedAt)
            .HasConversion(value => value!.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired(required: false);
        builder.HasOne<MatchmakingTicket>().WithMany().HasForeignKey(value => value.TicketId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(value => value.TicketId).IsUnique();
        builder.HasIndex(value => new { value.MatchAssignmentId, value.PlayerId }).IsUnique();
    }
}
