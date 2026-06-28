using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    public async Task<AuthenticatedUserReadModel?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .Include(user => user.Claims)
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role!.Claims)
            .Where(user => user.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return user?.ToReadModel();
    }
}
