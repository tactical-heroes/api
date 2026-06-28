using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options)
    : ReadDbContext<IdentityReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base registers ReadDbModel<TId> types before custom relationships are configured.
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigureUserRole(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadDbModel>(builder =>
        {
            builder.HasMany(user => user.Claims)
                .WithOne(claim => claim.User)
                .HasForeignKey(claim => claim.UserId);

            builder.HasMany(user => user.Roles)
                .WithOne(userRole => userRole.User)
                .HasForeignKey(userRole => userRole.UserId);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleReadDbModel>(builder =>
        {
            builder.HasMany(role => role.Claims)
                .WithOne(claim => claim.Role)
                .HasForeignKey(claim => claim.RoleId);

            builder.HasMany(role => role.Users)
                .WithOne(userRole => userRole.Role)
                .HasForeignKey(userRole => userRole.RoleId);
        });
    }

    private static void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRoleReadDbModel>(builder =>
        {
            builder.ToTable("user_roles", "identity");
            builder.HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });

            builder.HasOne(userRole => userRole.User)
                .WithMany(user => user.Roles)
                .HasForeignKey(userRole => userRole.UserId);

            builder.HasOne(userRole => userRole.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(userRole => userRole.RoleId);
        });
    }
}
