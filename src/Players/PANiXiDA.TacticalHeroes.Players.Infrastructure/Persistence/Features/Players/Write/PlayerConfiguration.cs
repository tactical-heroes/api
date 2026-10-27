using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Players.Domain.Files;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Write;

internal sealed class PlayerConfiguration : AuditableEntityConfiguration<Player>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id)
            .HasConversion(
                id => id.Value,
                value => PlayerId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(player => player.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value).Value)
            .IsRequired();

        builder.Property(player => player.AvatarFileId)
            .HasConversion(
                id => id!.Value.Value,
                value => FileId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(player => player.Rating)
            .HasConversion(
                rating => rating.Value,
                value => Rating.Create(value))
            .IsRequired();

        builder.HasIndex(player => player.UserId).IsUnique();
    }
}
