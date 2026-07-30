using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityWriteDbContext(
    DbContextOptions<IdentityWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        IdentityUserToken<Guid>>(options: options),
    IDataProtectionKeyContext
{
    private const string Schema = "identity";

    private readonly IEnumerable<IInterceptor> _interceptors = interceptors;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(interceptors: _interceptors);

        optionsBuilder.UseNpgsql(npgsqlOptionsAction: options =>
            options.MigrationsHistoryTable(tableName: "__ef_migrations_history", schema: Schema));
        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(schema: Schema);

        base.OnModelCreating(builder: builder);

        builder.UseOpenIddict<Guid>();
        ConfigureAspNetIdentity(modelBuilder: builder);
        ConfigureOpenIddict(modelBuilder: builder);
    }

    private static void ConfigureAspNetIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_users", schema: Schema);

            builder.Property(propertyExpression: user => user.Id).ValueGeneratedNever();
            builder.Property(propertyExpression: user => user.Status)
                .HasMaxLength(maxLength: UserStatus.MaxNameLength)
                .HasDefaultValue(value: UserStatus.Active.Name)
                .IsRequired();
            builder.Property(propertyExpression: user => user.Email).HasMaxLength(maxLength: Email.MaxLength).IsRequired();
            builder.Property(propertyExpression: user => user.NormalizedEmail).HasMaxLength(maxLength: Email.MaxLength).IsRequired();
            builder.Property(propertyExpression: user => user.UserName).HasMaxLength(maxLength: UserName.MaxLength).IsRequired();
            builder.Property(propertyExpression: user => user.NormalizedUserName).HasMaxLength(maxLength: UserName.MaxLength).IsRequired();
            builder.Property(propertyExpression: user => user.PasswordHash).HasMaxLength(maxLength: 1024).IsRequired();

            builder.HasMany(navigationExpression: user => user.Roles)
                .WithOne(navigationExpression: role => role.User)
                .HasForeignKey(foreignKeyExpression: role => role.UserId)
                .IsRequired();

            builder.HasMany(navigationExpression: user => user.Claims)
                .WithOne(navigationExpression: claim => claim.User)
                .HasForeignKey(foreignKeyExpression: claim => claim.UserId)
                .IsRequired();

            builder.HasMany(navigationExpression: user => user.Logins)
                .WithOne(navigationExpression: login => login.User)
                .HasForeignKey(foreignKeyExpression: login => login.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationRole>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_roles", schema: Schema);

            builder.Property(propertyExpression: role => role.Id).ValueGeneratedNever();
            builder.Property(propertyExpression: role => role.Name).HasMaxLength(maxLength: RoleName.MaxLength).IsRequired();
            builder.Property(propertyExpression: role => role.NormalizedName).HasMaxLength(maxLength: RoleName.MaxLength).IsRequired();

            builder.HasMany(navigationExpression: role => role.Users)
                .WithOne(navigationExpression: user => user.Role)
                .HasForeignKey(foreignKeyExpression: user => user.RoleId)
                .IsRequired();

            builder.HasMany(navigationExpression: role => role.Claims)
                .WithOne(navigationExpression: claim => claim.Role)
                .HasForeignKey(foreignKeyExpression: claim => claim.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationUserRole>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_roles", schema: Schema);
            builder.HasKey(keyExpression: userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });
        });

        modelBuilder.Entity<ApplicationUserClaim>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_claims", schema: Schema);
            builder.Property(propertyExpression: claim => claim.ClaimType)
                .HasMaxLength(maxLength: ClaimType.MaxLength)
                .IsRequired();
            builder.Property(propertyExpression: claim => claim.ClaimValue)
                .HasMaxLength(maxLength: ClaimValue.MaxLength)
                .IsRequired();
            builder.HasIndex(indexExpression: claim => new
            {
                claim.UserId,
                claim.ClaimType,
                claim.ClaimValue
            }).IsUnique();
        });

        modelBuilder.Entity<ApplicationRoleClaim>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_role_claims", schema: Schema);
            builder.Property(propertyExpression: claim => claim.ClaimType)
                .HasMaxLength(maxLength: ClaimType.MaxLength)
                .IsRequired();
            builder.Property(propertyExpression: claim => claim.ClaimValue)
                .HasMaxLength(maxLength: ClaimValue.MaxLength)
                .IsRequired();
            builder.HasIndex(indexExpression: claim => new
            {
                claim.RoleId,
                claim.ClaimType,
                claim.ClaimValue
            }).IsUnique();
        });

        modelBuilder.Entity<ApplicationUserLogin>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_logins", schema: Schema);
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_tokens", schema: Schema);
        });
    }

    private static void ConfigureOpenIddict(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>()
            .ToTable(name: "open_iddict_applications", schema: Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .ToTable(name: "open_iddict_authorizations", schema: Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>()
            .ToTable(name: "open_iddict_scopes", schema: Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .ToTable(name: "open_iddict_tokens", schema: Schema);
    }
}
