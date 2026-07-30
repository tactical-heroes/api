using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options)
    : ReadDbContext<IdentityReadDbContext>(options: options)
{
    private const string Schema = "identity";

    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder: modelBuilder);

        ConfigureUser(modelBuilder: modelBuilder);
        ConfigureRole(modelBuilder: modelBuilder);
        ConfigureUserRole(modelBuilder: modelBuilder);
        ConfigureUserClaim(modelBuilder: modelBuilder);
        ConfigureRoleClaim(modelBuilder: modelBuilder);
        ConfigureUserToken(modelBuilder: modelBuilder);
        ConfigureUserLogin(modelBuilder: modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_users", schema: Schema);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_roles", schema: Schema);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRoleReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_roles", schema: Schema);
            builder.HasKey(keyExpression: userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });
        });
    }

    private static void ConfigureUserClaim(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserClaimReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_claims", schema: Schema);
        });
    }

    private static void ConfigureRoleClaim(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleClaimReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_role_claims", schema: Schema);
        });
    }

    private static void ConfigureUserToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTokenReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_tokens", schema: Schema);
            builder.HasKey(keyExpression: token => new
            {
                token.UserId,
                token.LoginProvider,
                token.Name
            });
        });
    }

    private static void ConfigureUserLogin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLoginReadDbModel>(buildAction: builder =>
        {
            builder.ToTable(name: "asp_net_user_logins", schema: Schema);
            builder.HasKey(keyExpression: login => new
            {
                login.LoginProvider,
                login.ProviderKey
            });
        });
    }
}
