using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

internal sealed class RoleConfiguration : AuditableEntityConfiguration<Role>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasColumnOrder(0)
            .HasConversion(RoleIdConverter)
            .ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasConversion(RoleNameConverter)
            .HasMaxLength(RoleName.MaxLength)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.OwnsMany(
            role => role.Claims,
            ConfigureRoleClaim);

        builder.Navigation(role => role.Claims)
            .HasField("_claims")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureRoleClaim(
        OwnedNavigationBuilder<Role, RoleClaim> builder)
    {
        builder.ToTable("role_claims");

        builder.WithOwner()
            .HasForeignKey("role_id");

        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id)
            .HasColumnOrder(0)
            .HasConversion(RoleClaimIdConverter)
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

        builder.HasIndex("role_id", nameof(RoleClaim.Type), nameof(RoleClaim.Value))
            .IsUnique();
    }

    private static readonly ValueConverter<RoleId, Guid> RoleIdConverter = new(
        roleId => roleId.Value,
        value => RoleId.Create(value).Value);

    private static readonly ValueConverter<RoleName, string> RoleNameConverter = new(
        roleName => roleName.Value,
        value => RoleName.Create(value).Value);

    private static readonly ValueConverter<RoleClaimId, Guid> RoleClaimIdConverter = new(
        roleClaimId => roleClaimId.Value,
        value => RoleClaimId.Create(value).Value);

    private static readonly ValueConverter<ClaimType, string> ClaimTypeConverter = new(
        claimType => claimType.Value,
        value => ClaimType.Create(value).Value);

    private static readonly ValueConverter<ClaimValue, string> ClaimValueConverter = new(
        claimValue => claimValue.Value,
        value => ClaimValue.Create(value).Value);
}
