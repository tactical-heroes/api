using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserConfirmationTokens.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserPasswordResetTokens.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

internal sealed class UserConfiguration : AuditableEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("identity_users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnOrder(0)
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasConversion(EmailConverter)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasConversion(PasswordHashConverter)
            .HasMaxLength(PasswordHash.MaxLength)
            .IsRequired();

        builder.Property(user => user.ConfirmationStatus)
            .HasColumnName("is_confirmed")
            .HasConversion(UserConfirmationStatusConverter)
            .IsRequired();

        builder.OwnsOne(
            user => user.ConfirmationToken,
            ConfigureUserConfirmationToken);

        builder.OwnsOne(
            user => user.PasswordResetToken,
            ConfigureUserPasswordResetToken);

        builder.OwnsMany(
            user => user.Roles,
            ConfigureUserRole);

        builder.OwnsMany(
            user => user.Claims,
            ConfigureUserClaim);

        builder.Navigation(user => user.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.Navigation(user => user.Claims)
            .HasField("_claims")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.Navigation(user => user.ConfirmationToken)
            .AutoInclude();

        builder.Navigation(user => user.PasswordResetToken)
            .AutoInclude();
    }

    private static void ConfigureUserConfirmationToken(
        OwnedNavigationBuilder<User, UserConfirmationToken> builder)
    {
        builder.ToTable("identity_user_confirmation_tokens");

        builder.WithOwner()
            .HasForeignKey(token => token.UserId)
            .HasConstraintName("fk_identity_user_confirmation_tokens_identity_users_identity_user_id");

        builder.Ignore(token => token.Id);

        builder.HasKey(token => token.UserId);

        builder.Property(token => token.UserId)
            .HasColumnName("identity_user_id")
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
    }

    private static void ConfigureUserPasswordResetToken(
        OwnedNavigationBuilder<User, UserPasswordResetToken> builder)
    {
        builder.ToTable("identity_user_password_reset_tokens");

        builder.WithOwner()
            .HasForeignKey(token => token.UserId)
            .HasConstraintName("fk_identity_user_password_reset_tokens_identity_users_identity_user_id");

        builder.Ignore(token => token.Id);

        builder.HasKey(token => token.UserId);

        builder.Property(token => token.UserId)
            .HasColumnName("identity_user_id")
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
    }

    private static void ConfigureUserRole(
        OwnedNavigationBuilder<User, UserRole> builder)
    {
        builder.ToTable("identity_user_roles");

        builder.WithOwner()
            .HasForeignKey(role => role.UserId)
            .HasConstraintName("fk_identity_user_roles_identity_users_identity_user_id");

        builder.Ignore(role => role.Id);

        builder.HasKey(role => new
        {
            role.UserId,
            role.RoleId
        });

        builder.Property(role => role.UserId)
            .HasColumnName("identity_user_id")
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(role => role.RoleId)
            .HasColumnName("identity_role_id")
            .HasConversion(RoleIdConverter)
            .ValueGeneratedNever();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(role => role.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(role => role.RoleId);
    }

    private static void ConfigureUserClaim(
        OwnedNavigationBuilder<User, UserClaim> builder)
    {
        builder.ToTable("identity_user_claims");

        builder.WithOwner()
            .HasForeignKey("identity_user_id");

        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id)
            .HasColumnOrder(0)
            .HasConversion(UserClaimIdConverter)
            .ValueGeneratedNever();

        builder.Property(claim => claim.Type)
            .HasColumnName("type")
            .HasConversion(ClaimTypeConverter)
            .HasMaxLength(ClaimType.MaxLength)
            .IsRequired();

        builder.Property(claim => claim.Value)
            .HasColumnName("value")
            .HasConversion(ClaimValueConverter)
            .HasMaxLength(ClaimValue.MaxLength)
            .IsRequired();

        builder.HasIndex("identity_user_id", nameof(UserClaim.Type), nameof(UserClaim.Value))
            .IsUnique();
    }

    private static readonly ValueConverter<UserId, Guid> UserIdConverter = new(
        userId => userId.Value,
        value => UserId.Create(value).Value);

    private static readonly ValueConverter<Email, string> EmailConverter = new(
        email => email.Value,
        value => Email.Create(value).Value);

    private static readonly ValueConverter<PasswordHash, string> PasswordHashConverter = new(
        passwordHash => passwordHash.Value,
        value => PasswordHash.Create(value).Value);

    private static readonly ValueConverter<UserConfirmationStatus, bool> UserConfirmationStatusConverter = new(
        confirmationStatus => confirmationStatus.IsConfirmed,
        value => UserConfirmationStatus.From(value));

    private static readonly ValueConverter<RoleId, Guid> RoleIdConverter = new(
        roleId => roleId.Value,
        value => RoleId.Create(value).Value);

    private static readonly ValueConverter<UserClaimId, Guid> UserClaimIdConverter = new(
        userClaimId => userClaimId.Value,
        value => UserClaimId.Create(value).Value);

    private static readonly ValueConverter<ConfirmationTokenHash, string> ConfirmationTokenHashConverter = new(
        tokenHash => tokenHash.Value,
        value => ConfirmationTokenHash.Create(value).Value);

    private static readonly ValueConverter<ConfirmationTokenExpiration, DateTimeOffset>
        ConfirmationTokenExpirationConverter = new(
            expiration => expiration.Value,
            value => ConfirmationTokenExpiration.Create(value).Value);

    private static readonly ValueConverter<PasswordResetTokenHash, string> PasswordResetTokenHashConverter = new(
        tokenHash => tokenHash.Value,
        value => PasswordResetTokenHash.Create(value).Value);

    private static readonly ValueConverter<PasswordResetTokenExpiration, DateTimeOffset>
        PasswordResetTokenExpirationConverter = new(
            expiration => expiration.Value,
            value => PasswordResetTokenExpiration.Create(value).Value);

    private static readonly ValueConverter<ClaimType, string> ClaimTypeConverter = new(
        claimType => claimType.Value,
        value => ClaimType.Create(value).Value);

    private static readonly ValueConverter<ClaimValue, string> ClaimValueConverter = new(
        claimValue => claimValue.Value,
        value => ClaimValue.Create(value).Value);
}
