using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Filters;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    public async Task<AuthenticatedUser?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync<AuthenticatedUserReadModel, AuthenticatedUserReadModelMapper>(
            userId,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var query = UsersFilter.Apply(Query)
            .Where(readUser => readUser.Id == userId);
        var roles = await query
            .SelectMany(readUser => readUser.Roles
                .Select(userRole => userRole.Role!.Name))
            .Distinct()
            .OrderBy(roleName => roleName)
            .ToArrayAsync(cancellationToken);
        var directClaims = await query
            .SelectMany(readUser => readUser.Claims
                .Select(claim => new AuthorizationClaim(
                    claim.Type,
                    claim.Value)))
            .ToArrayAsync(cancellationToken);
        var roleClaims = await query
            .SelectMany(readUser => readUser.Roles
                .SelectMany(userRole => userRole.Role!.Claims
                    .Select(claim => new AuthorizationClaim(
                        claim.Type,
                        claim.Value))))
            .ToArrayAsync(cancellationToken);
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
            roles,
            claims);
    }
}
