using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityUsers.Write;

internal sealed class IdentityUserConfiguration : AuditableEntityConfiguration<IdentityUser>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IdentityUser> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnOrder(0)
            .HasConversion(IdentityUserIdConverter)
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
            user => user.DirectPermissions,
            ConfigureUserPermission);

        builder.Navigation(user => user.Roles)
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.Navigation(user => user.DirectPermissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }

    private static void ConfigureUserRole(
        OwnedNavigationBuilder<IdentityUser, IdentityUserRole> builder)
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
            .HasConversion(IdentityRoleIdConverter)
            .IsRequired();

        builder.HasOne<IdentityRole>()
            .WithMany()
            .HasForeignKey(role => role.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("identity_user_id", nameof(IdentityUserRole.RoleId))
            .IsUnique();

        builder.HasIndex(role => role.RoleId);
    }

    private static void ConfigureUserPermission(
        OwnedNavigationBuilder<IdentityUser, IdentityUserPermission> builder)
    {
        builder.ToTable("identity_user_permissions");

        builder.WithOwner()
            .HasForeignKey("identity_user_id");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .HasColumnOrder(0)
            .ValueGeneratedNever();

        builder.Property(permission => permission.Name)
            .HasConversion(PermissionNameConverter)
            .HasMaxLength(PermissionName.MaxLength)
            .IsRequired();

        builder.HasIndex("identity_user_id", nameof(IdentityUserPermission.Name))
            .IsUnique();
    }

    private static readonly ValueConverter<IdentityUserId, Guid> IdentityUserIdConverter = new(
        userId => userId.Value,
        value => IdentityUserId.Create(value).Value);

    private static readonly ValueConverter<Email, string> EmailConverter = new(
        email => email.Value,
        value => Email.Create(value).Value);

    private static readonly ValueConverter<PasswordHash, string> PasswordHashConverter = new(
        passwordHash => passwordHash.Value,
        value => PasswordHash.Create(value).Value);

    private static readonly ValueConverter<TokenHash?, string?> NullableTokenHashConverter = new(
        tokenHash => tokenHash == null ? null : tokenHash.Value,
        value => value == null ? null : TokenHash.Create(value).Value);

    private static readonly ValueConverter<IdentityRoleId, Guid> IdentityRoleIdConverter = new(
        roleId => roleId.Value,
        value => IdentityRoleId.Create(value).Value);

    private static readonly ValueConverter<PermissionName, string> PermissionNameConverter = new(
        permissionName => permissionName.Value,
        value => PermissionName.Create(value).Value);
}
