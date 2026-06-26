using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

internal sealed class UserConfirmationTokenConfiguration : IEntityTypeConfiguration<UserConfirmationToken>
{
    public void Configure(EntityTypeBuilder<UserConfirmationToken> builder)
    {
        builder.Ignore(token => token.Id);

        builder.HasKey(token => token.UserId);

        builder.Property(token => token.UserId)
            .HasColumnName("user_id")
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(token => token.TokenHash)
            .HasColumnName("token_hash")
            .HasConversion(ConfirmationTokenHashConverter)
            .HasMaxLength(ConfirmationTokenHash.MaxLength)
            .IsRequired();

        builder.Property(token => token.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .HasConversion(ConfirmationTokenExpirationConverter)
            .IsRequired();

        builder.HasIndex(token => token.ExpiresAtUtc);

        builder.HasOne<User>()
            .WithOne(user => user.ConfirmationToken)
            .HasForeignKey<UserConfirmationToken>(token => token.UserId)
            .HasConstraintName("fk_user_confirmation_tokens_users_user_id")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static readonly ValueConverter<UserId, Guid> UserIdConverter = new(
        userId => userId.Value,
        value => UserId.Create(value).Value);

    private static readonly ValueConverter<ConfirmationTokenHash, string> ConfirmationTokenHashConverter = new(
        tokenHash => tokenHash.Value,
        value => ConfirmationTokenHash.Create(value).Value);

    private static readonly ValueConverter<ConfirmationTokenExpiration, DateTimeOffset>
        ConfirmationTokenExpirationConverter = new(
            expiration => expiration.Value,
            value => ConfirmationTokenExpiration.Create(value).Value);
}
