using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
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
        IdentityUserToken<Guid>>(options)
{
    private readonly IEnumerable<IInterceptor> _interceptors = interceptors;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(_interceptors);

        optionsBuilder.UseNpgsql(options =>
            options.MigrationsHistoryTable("__ef_migrations_history", "identity"));
        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseOpenIddict<Guid>();
        ConfigureAspNetIdentity(modelBuilder);
        ConfigureOpenIddict(modelBuilder);
    }

    private static void ConfigureAspNetIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.ToTable("asp_net_users", "identity");

            builder.Property(user => user.Id).ValueGeneratedNever();
            builder.Property(user => user.Email).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.NormalizedEmail).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.UserName).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.NormalizedUserName).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.PasswordHash).HasMaxLength(1024).IsRequired();

            builder.HasMany(user => user.Roles)
                .WithOne(role => role.User)
                .HasForeignKey(role => role.UserId)
                .IsRequired();

            builder.HasMany(user => user.Claims)
                .WithOne(claim => claim.User)
                .HasForeignKey(claim => claim.UserId)
                .IsRequired();

            builder.HasMany(user => user.Logins)
                .WithOne(login => login.User)
                .HasForeignKey(login => login.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationRole>(builder =>
        {
            builder.ToTable("asp_net_roles", "identity");

            builder.Property(role => role.Id).ValueGeneratedNever();
            builder.Property(role => role.Name).HasMaxLength(RoleName.MaxLength).IsRequired();
            builder.Property(role => role.NormalizedName).HasMaxLength(RoleName.MaxLength).IsRequired();

            builder.HasMany(role => role.Users)
                .WithOne(user => user.Role)
                .HasForeignKey(user => user.RoleId)
                .IsRequired();

            builder.HasMany(role => role.Claims)
                .WithOne(claim => claim.Role)
                .HasForeignKey(claim => claim.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<ApplicationUserRole>(builder =>
        {
            builder.ToTable("asp_net_user_roles", "identity");
            builder.HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });
        });

        modelBuilder.Entity<ApplicationUserClaim>(builder =>
        {
            builder.ToTable("asp_net_user_claims", "identity");
            builder.Property(claim => claim.ClaimType)
                .HasMaxLength(ClaimType.MaxLength)
                .IsRequired();
            builder.Property(claim => claim.ClaimValue)
                .HasMaxLength(ClaimValue.MaxLength)
                .IsRequired();
            builder.HasIndex(claim => new
            {
                claim.UserId,
                claim.ClaimType,
                claim.ClaimValue
            }).IsUnique();
        });

        modelBuilder.Entity<ApplicationRoleClaim>(builder =>
        {
            builder.ToTable("asp_net_role_claims", "identity");
            builder.Property(claim => claim.ClaimType)
                .HasMaxLength(ClaimType.MaxLength)
                .IsRequired();
            builder.Property(claim => claim.ClaimValue)
                .HasMaxLength(ClaimValue.MaxLength)
                .IsRequired();
            builder.HasIndex(claim => new
            {
                claim.RoleId,
                claim.ClaimType,
                claim.ClaimValue
            }).IsUnique();
        });

        modelBuilder.Entity<ApplicationUserLogin>(builder =>
        {
            builder.ToTable("asp_net_user_logins", "identity");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
        {
            builder.ToTable("asp_net_user_tokens", "identity");
        });
    }

    private static void ConfigureOpenIddict(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>()
            .ToTable("open_iddict_applications", "identity");

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .ToTable("open_iddict_authorizations", "identity");

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>()
            .ToTable("open_iddict_scopes", "identity");

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .ToTable("open_iddict_tokens", "identity");
    }
}
