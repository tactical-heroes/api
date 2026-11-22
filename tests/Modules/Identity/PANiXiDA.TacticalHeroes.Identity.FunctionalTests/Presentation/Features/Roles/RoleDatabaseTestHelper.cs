using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

internal static class RoleDatabaseTestHelper
{
    internal static async Task<ApplicationRole?> FindAsync(
        FunctionalTestFixture fixture,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

        return await dbContext.Set<ApplicationRole>()
            .Include(role => role.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }
}
