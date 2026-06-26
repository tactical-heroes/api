using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

internal sealed class UserPasswordResetTokenConfiguration : IEntityTypeConfiguration<UserPasswordResetToken>
{
    public void Configure(EntityTypeBuilder<UserPasswordResetToken> builder)
    {
        builder.ToTable("user_password_reset_tokens");

        builder.Ignore(token => token.Id);

        builder.HasKey(token => token.UserId);

        builder.Property(token => token.UserId)
            .HasColumnName("user_id")
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(token => token.TokenHash)
            .HasColumnName("token_hash")
            .HasConversion(PasswordResetTokenHashConverter)
            .HasMaxLength(PasswordResetTokenHash.MaxLength)
            .IsRequired();

        builder.Property(token => token.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .HasConversion(PasswordResetTokenExpirationConverter)
            .IsRequired();

        builder.HasIndex(token => token.ExpiresAtUtc);

        builder.HasOne<User>()
            .WithOne(user => user.PasswordResetToken)
            .HasForeignKey<UserPasswordResetToken>(token => token.UserId)
            .HasConstraintName("fk_user_password_reset_tokens_users_user_id")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static readonly ValueConverter<UserId, Guid> UserIdConverter = new(
        userId => userId.Value,
        value => UserId.Create(value).Value);

    private static readonly ValueConverter<PasswordResetTokenHash, string> PasswordResetTokenHashConverter = new(
        tokenHash => tokenHash.Value,
        value => PasswordResetTokenHash.Create(value).Value);

    private static readonly ValueConverter<PasswordResetTokenExpiration, DateTimeOffset>
        PasswordResetTokenExpirationConverter = new(
            expiration => expiration.Value,
            value => PasswordResetTokenExpiration.Create(value).Value);
}
