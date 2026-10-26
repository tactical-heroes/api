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
        builder.HasKey(value => value.Id);
        builder.Property(value => value.Id)
            .HasConversion(value => value.Value, value => MatchAssignmentId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(value => value.Mode)
            .HasConversion(value => value.Value, value => MatchmakingMode.Create(value).Value)
            .HasMaxLength(MatchmakingMode.MaxLength)
            .IsRequired();
        builder.Property(value => value.ProposedAt)
            .HasConversion(value => value.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired();
        builder.Property(value => value.AcceptanceDeadline)
            .HasConversion(value => value.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired();
        builder.Property(value => value.CreationDeadline)
            .HasConversion(value => value.Value, value => MatchmakingTimestamp.Create(value))
            .IsRequired();
        builder.Property(value => value.GameId)
            .HasConversion(value => value!.Value, value => GameReference.Create(value).Value)
            .IsRequired(required: false);
        builder.Property(value => value.ClosureReason)
            .HasConversion(value => value!.Value, value => MatchmakingReason.Create(value).Value)
            .HasMaxLength(MatchmakingReason.MaxLength)
            .IsRequired(required: false);
        builder.Property(value => value.Status)
            .HasConversion(value => value.Name, value => MatchAssignmentStatus.Create(value).Value)
            .HasMaxLength(MatchAssignmentStatus.MaxLength)
            .IsRequired();
        builder.HasMany(value => value.Participants).WithOne().HasForeignKey(value => value.MatchAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(value => value.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.HasIndex(value => value.GameId).IsUnique();
        builder.HasIndex(value => new { value.Status, value.AcceptanceDeadline });
        builder.HasIndex(value => new { value.Status, value.CreationDeadline });
        builder.Metadata.AddCheckConstraint(
            "ck_assignment_window",
            "acceptance_deadline > proposed_at AND creation_deadline > acceptance_deadline");
        builder.Metadata.AddCheckConstraint(
            "ck_assignment_game",
            "(status = 'Ready') = (game_id IS NOT NULL)");
    }
}
