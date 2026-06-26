using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

internal sealed class UserConfiguration : AuditableEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
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
            .HasConversion(UserConfirmationStatusConverter)
            .IsRequired();

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

    private static void ConfigureUserRole(
        OwnedNavigationBuilder<User, UserRole> builder)
    {
        builder.WithOwner()
            .HasForeignKey(role => role.UserId)
            .HasConstraintName("fk_user_roles_users_user_id");

        builder.Ignore(role => role.Id);

        builder.HasKey(role => new
        {
            role.UserId,
            role.RoleId
        });

        builder.Property(role => role.UserId)
            .HasConversion(UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(role => role.RoleId)
            .HasConversion(RoleIdConverter)
            .ValueGeneratedNever();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(role => role.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_user_roles_roles_role_id");

        builder.HasIndex(role => role.RoleId);
    }

    private static void ConfigureUserClaim(
        OwnedNavigationBuilder<User, UserClaim> builder)
    {
        builder.WithOwner()
            .HasForeignKey("user_id")
            .HasConstraintName("fk_user_claims_users_user_id");

        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id)
            .HasColumnOrder(0)
            .HasConversion(UserClaimIdConverter)
            .ValueGeneratedNever();

        builder.Property(claim => claim.Type)
            .HasConversion(ClaimTypeConverter)
            .HasMaxLength(ClaimType.MaxLength)
            .IsRequired();

        builder.Property(claim => claim.Value)
            .HasConversion(ClaimValueConverter)
            .HasMaxLength(ClaimValue.MaxLength)
            .IsRequired();

        builder.HasIndex("user_id", nameof(UserClaim.Type), nameof(UserClaim.Value))
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

    private static readonly ValueConverter<ClaimType, string> ClaimTypeConverter = new(
        claimType => claimType.Value,
        value => ClaimType.Create(value).Value);

    private static readonly ValueConverter<ClaimValue, string> ClaimValueConverter = new(
        claimValue => claimValue.Value,
        value => ClaimValue.Create(value).Value);
}
