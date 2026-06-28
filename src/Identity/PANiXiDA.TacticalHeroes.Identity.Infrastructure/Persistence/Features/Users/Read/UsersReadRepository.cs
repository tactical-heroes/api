using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    private readonly IdentityReadDbContext _dbContext = dbContext;

    public async Task<AuthenticatedUser?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                user.Id,
                user.Email,
                user.ConfirmationStatus
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleNames = await (
                from userRole in _dbContext.Set<UserRoleReadDbModel>()
                join role in _dbContext.Set<RoleReadDbModel>() on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                select role.Name)
            .Distinct()
            .OrderBy(roleName => roleName)
            .ToArrayAsync(cancellationToken);

        var directClaimsQuery = _dbContext.Set<UserClaimReadDbModel>()
            .Where(claim => claim.UserId == userId)
            .Select(claim => new
            {
                claim.Type,
                claim.Value
            });

        var roleClaimsQuery =
            from userRole in _dbContext.Set<UserRoleReadDbModel>()
            join roleClaim in _dbContext.Set<RoleClaimReadDbModel>() on userRole.RoleId equals roleClaim.RoleId
            where userRole.UserId == userId
            select new
            {
                roleClaim.Type,
                roleClaim.Value
            };

        var claims = await directClaimsQuery
            .Concat(roleClaimsQuery)
            .Distinct()
            .OrderBy(claim => claim.Type)
            .ThenBy(claim => claim.Value)
            .Select(claim => new AuthorizationClaim(claim.Type, claim.Value))
            .ToArrayAsync(cancellationToken);

        return new AuthenticatedUser(
            user.Id,
            user.Email,
            user.ConfirmationStatus,
            roleNames,
            claims);
    }
}
