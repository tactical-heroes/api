using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    public async Task<AuthenticatedUser?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .Include(user => user.Claims)
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role!)
            .ThenInclude(role => role.Claims)
            .Where(user => user.Id == userId)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleNames = user.Roles
            .Select(userRole => userRole.Role!.Name)
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToArray();

        var directClaims = user.Claims
            .Select(claim => new AuthorizationClaim(
                claim.Type,
                claim.Value));

        var roleClaims = user.Roles
            .SelectMany(userRole => userRole.Role!.Claims)
            .Select(claim => new AuthorizationClaim(
                claim.Type,
                claim.Value));

        var claims = directClaims
            .Concat(roleClaims)
            .Distinct()
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ThenBy(claim => claim.Value, StringComparer.Ordinal)
            .ToArray();

        return new AuthenticatedUser(
            user.Id,
            user.Email,
            user.ConfirmationStatus,
            roleNames,
            claims);
    }
}
