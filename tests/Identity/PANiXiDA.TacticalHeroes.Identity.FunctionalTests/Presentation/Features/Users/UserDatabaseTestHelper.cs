using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;

internal static class UserDatabaseTestHelper
{
    internal static async Task<ApplicationUser?> FindAsync(
        FunctionalTestFixture fixture,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();

        return await dbContext.Set<ApplicationUser>()
            .Include(user => user.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }
}
