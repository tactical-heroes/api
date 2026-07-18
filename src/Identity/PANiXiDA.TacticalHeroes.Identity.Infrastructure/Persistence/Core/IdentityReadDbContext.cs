using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options)
    : ReadDbContext<IdentityReadDbContext>(options)
{
    private const string Schema = "identity";

    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureUserClaim(modelBuilder);
        ConfigureRoleClaim(modelBuilder);
        ConfigureUserToken(modelBuilder);
        ConfigureUserLogin(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_users", Schema);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_roles", Schema);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRoleReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_user_roles", Schema);
            builder.HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });
        });
    }

    private static void ConfigureUserClaim(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserClaimReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_user_claims", Schema);
        });
    }

    private static void ConfigureRoleClaim(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleClaimReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_role_claims", Schema);
        });
    }

    private static void ConfigureUserToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTokenReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_user_tokens", Schema);
            builder.HasKey(token => new
            {
                token.UserId,
                token.LoginProvider,
                token.Name
            });
        });
    }

    private static void ConfigureUserLogin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLoginReadDbModel>(builder =>
        {
            builder.ToTable("asp_net_user_logins", Schema);
            builder.HasKey(login => new
            {
                login.LoginProvider,
                login.ProviderKey
            });
        });
    }
}
