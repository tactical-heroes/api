using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepository(IdentityReadDbContext dbContext)
    : IRolesReadRepository
{
    public async Task<Result<PaginationResult<RoleListItemReadModel>>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<RoleReadDbModel>()
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ThenBy(role => role.Id);

        var totalCount = await query.LongCountAsync(cancellationToken);
        var roles = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(role => new RoleListItemReadModel(
                role.Id,
                role.Name!))
            .ToArrayAsync(cancellationToken);

        return Result.Success(
            PaginationResult<RoleListItemReadModel>.Create(
                roles,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount));
    }

    public async Task<Result<RoleDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Set<RoleReadDbModel>()
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(role => new RoleDetailsReadModel(
                role.Id,
                role.Name!,
                role.Claims
                    .Select(claim => new Claim(
                        claim.ClaimType!,
                        claim.ClaimValue!))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        return role is null
            ? Result.Failure<RoleDetailsReadModel>(
                Error.NotFound("Role was not found."))
            : Result.Success(role);
    }
}
