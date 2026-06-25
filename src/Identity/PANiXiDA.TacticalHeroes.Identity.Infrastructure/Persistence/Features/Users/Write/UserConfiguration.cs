using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.ValueObjects;

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

        builder.Property(user => user.IsConfirmed)
            .IsRequired();

        builder.Property(user => user.ConfirmationTokenHash)
            .HasConversion(NullableTokenHashConverter)
            .HasMaxLength(TokenHash.MaxLength)
            .IsRequired(false);

        builder.Property(user => user.ConfirmationTokenExpiresAtUtc)
            .IsRequired(false);

        builder.Property(user => user.PasswordResetTokenHash)
            .HasConversion(NullableTokenHashConverter)
            .HasMaxLength(TokenHash.MaxLength)
            .IsRequired(false);

        builder.Property(user => user.PasswordResetTokenExpiresAtUtc)
            .IsRequired(false);

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
    }

    private static void ConfigureUserRole(
        OwnedNavigationBuilder<User, UserRole> builder)
    {
        builder.ToTable("identity_user_roles");

        builder.WithOwner()
            .HasForeignKey("identity_user_id");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasColumnOrder(0)
            .ValueGeneratedNever();

        builder.Property(role => role.RoleId)
            .HasColumnName("identity_role_id")
            .HasConversion(RoleIdConverter)
            .IsRequired();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(role => role.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("identity_user_id", nameof(UserRole.RoleId))
            .IsUnique();

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

    private static readonly ValueConverter<TokenHash?, string?> NullableTokenHashConverter = new(
        tokenHash => tokenHash == null ? null : tokenHash.Value,
        value => value == null ? null : TokenHash.Create(value).Value);

    private static readonly ValueConverter<RoleId, Guid> RoleIdConverter = new(
        roleId => roleId.Value,
        value => RoleId.Create(value).Value);

    private static readonly ValueConverter<ClaimType, string> ClaimTypeConverter = new(
        claimType => claimType.Value,
        value => ClaimType.Create(value).Value);

    private static readonly ValueConverter<ClaimValue, string> ClaimValueConverter = new(
        claimValue => claimValue.Value,
        value => ClaimValue.Create(value).Value);
}
