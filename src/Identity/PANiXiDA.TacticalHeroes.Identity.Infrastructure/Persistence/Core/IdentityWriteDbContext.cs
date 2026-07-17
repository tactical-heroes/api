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
        IdentityUserToken<Guid>>(options),
    IDataProtectionKeyContext
{
    private const string Schema = "identity";

    private readonly IEnumerable<IInterceptor> _interceptors = interceptors;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(_interceptors);

        optionsBuilder.UseNpgsql(options =>
            options.MigrationsHistoryTable("__ef_migrations_history", Schema));
        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        base.OnModelCreating(builder);

        builder.UseOpenIddict<Guid>();
        ConfigureAspNetIdentity(builder);
        ConfigureOpenIddict(builder);
    }

    private static void ConfigureAspNetIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.ToTable("asp_net_users", Schema);

            builder.Property(user => user.Id).ValueGeneratedNever();
            builder.Property(user => user.Status)
                .HasMaxLength(AccountStatus.MaxNameLength)
                .HasDefaultValue(AccountStatus.Active.Name)
                .IsRequired();
            builder.Property(user => user.Email).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.NormalizedEmail).HasMaxLength(Email.MaxLength).IsRequired();
            builder.Property(user => user.UserName).HasMaxLength(UserName.MaxLength).IsRequired();
            builder.Property(user => user.NormalizedUserName).HasMaxLength(UserName.MaxLength).IsRequired();
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
            builder.ToTable("asp_net_roles", Schema);

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
            builder.ToTable("asp_net_user_roles", Schema);
            builder.HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });
        });

        modelBuilder.Entity<ApplicationUserClaim>(builder =>
        {
            builder.ToTable("asp_net_user_claims", Schema);
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
            builder.ToTable("asp_net_role_claims", Schema);
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
            builder.ToTable("asp_net_user_logins", Schema);
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
        {
            builder.ToTable("asp_net_user_tokens", Schema);
        });
    }

    private static void ConfigureOpenIddict(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>()
            .ToTable("open_iddict_applications", Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .ToTable("open_iddict_authorizations", Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>()
            .ToTable("open_iddict_scopes", Schema);

        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .ToTable("open_iddict_tokens", Schema);
    }
}
