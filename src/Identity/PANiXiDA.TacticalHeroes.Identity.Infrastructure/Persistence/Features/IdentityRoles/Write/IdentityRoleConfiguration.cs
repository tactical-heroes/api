using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.Entities;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.IdentityRoles.Write;

internal sealed class IdentityRoleConfiguration : AuditableEntityConfiguration<IdentityRole>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasColumnOrder(0)
            .HasConversion(IdentityRoleIdConverter)
            .ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasConversion(RoleNameConverter)
            .HasMaxLength(RoleName.MaxLength)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.OwnsMany(
            role => role.Permissions,
            ConfigureRolePermission);

        builder.Navigation(role => role.Permissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureRolePermission(
        OwnedNavigationBuilder<IdentityRole, IdentityRolePermission> builder)
    {
        builder.ToTable("identity_role_permissions");

        builder.WithOwner()
            .HasForeignKey("identity_role_id");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .HasColumnOrder(0)
            .ValueGeneratedNever();

        builder.Property(permission => permission.Name)
            .HasConversion(PermissionNameConverter)
            .HasMaxLength(PermissionName.MaxLength)
            .IsRequired();

        builder.HasIndex("identity_role_id", nameof(IdentityRolePermission.Name))
            .IsUnique();
    }

    private static readonly ValueConverter<IdentityRoleId, Guid> IdentityRoleIdConverter = new(
        roleId => roleId.Value,
        value => IdentityRoleId.Create(value).Value);

    private static readonly ValueConverter<RoleName, string> RoleNameConverter = new(
        roleName => roleName.Value,
        value => RoleName.Create(value).Value);

    private static readonly ValueConverter<PermissionName, string> PermissionNameConverter = new(
        permissionName => permissionName.Value,
        value => PermissionName.Create(value).Value);
}
