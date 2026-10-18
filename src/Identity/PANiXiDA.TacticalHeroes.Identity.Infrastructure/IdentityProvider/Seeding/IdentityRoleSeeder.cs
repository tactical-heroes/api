using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;

internal sealed class IdentityRoleSeeder(RoleManager<ApplicationRole> roleManager, TimeProvider timeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var name in RoleNames.GetAll().Select(role => role.Name))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.FindByNameAsync(name) is not null)
            {
                continue;
            }

            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Id = Guid.CreateVersion7(),
                Name = name,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            });
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not seed role '{name}': {errors}");
            }
        }
    }
}
