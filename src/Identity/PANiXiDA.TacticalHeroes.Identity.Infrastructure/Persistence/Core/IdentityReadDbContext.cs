using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options)
    : ReadDbContext<IdentityReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserRole(modelBuilder);
        ConfigureUserConfirmationToken(modelBuilder);
        ConfigureUserPasswordResetToken(modelBuilder);
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
        });
    }

    private static void ConfigureUserConfirmationToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserConfirmationTokenReadDbModel>(builder =>
        {
            builder.ToTable("user_confirmation_tokens", "identity");
            builder.HasKey(token => token.UserId);
        });
    }

    private static void ConfigureUserPasswordResetToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPasswordResetTokenReadDbModel>(builder =>
        {
            builder.ToTable("user_password_reset_tokens", "identity");
            builder.HasKey(token => token.UserId);
        });
    }
}
