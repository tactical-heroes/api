using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Write;

internal sealed class MatchAssignmentConfiguration : AuditableEntityConfiguration<MatchAssignment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MatchAssignment> builder)
    {
        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.Id)
            .HasConversion(
                id => id.Value,
                value => MatchAssignmentId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(assignment => assignment.Mode)
            .HasConversion(
                mode => mode.Value,
                value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();

        builder.Property(assignment => assignment.ProposedAt)
            .HasConversion(
                proposedAt => proposedAt.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(assignment => assignment.AcceptanceDeadline)
            .HasConversion(
                deadline => deadline.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(assignment => assignment.CreationDeadline)
            .HasConversion(
                deadline => deadline.Value,
                value => MatchmakingTimestamp.Create(value))
            .IsRequired();

        builder.Property(assignment => assignment.GameId)
            .HasConversion(
                gameId => gameId!.Value,
                value => GameReference.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(assignment => assignment.ClosureReason)
            .HasConversion(
                reason => reason!.Value,
                value => MatchmakingReason.Create(value).Value)
            .HasMaxLength(MatchmakingReason.MaxLength)
            .IsRequired(required: false);

        builder.Property(assignment => assignment.Status)
            .HasConversion(
                status => status.Name,
                value => MatchAssignmentStatus.Create(value).Value)
            .HasMaxLength(MatchAssignmentStatus.MaxLength)
            .IsRequired();

        builder.HasMany(assignment => assignment.Participants)
            .WithOne()
            .HasForeignKey(participant => participant.MatchAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(assignment => assignment.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(assignment => assignment.GameId).IsUnique();
        builder.HasIndex(assignment => new { assignment.Status, assignment.AcceptanceDeadline });
        builder.HasIndex(assignment => new { assignment.Status, assignment.CreationDeadline });
    }
}
