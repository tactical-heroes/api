using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IRolesRepository
{
    public async Task<Result<Guid>> AddAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationRole = ApplicationRoleMapper.ToDbModel(
            role: role,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        var identityResult = await roleManager.CreateAsync(role: applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        aggregateTracker.Track(aggregateRoot: role);

        return Result.Success(value: applicationRole.Id);
    }

    public async Task<Result> UpdateAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .Include(navigationPropertyPath: item => item.Claims)
            .SingleOrDefaultAsync(predicate: item => item.Id == role.Id.Value, cancellationToken: cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        ApplicationRoleMapper.MapToDbModel(
            role: role,
            dbModel: applicationRole,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        SyncClaims(
            applicationRole: applicationRole,
            role: role);

        var identityResult = await roleManager.UpdateAsync(role: applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(aggregateRoot: role);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .Include(navigationPropertyPath: role => role.Claims)
            .SingleOrDefaultAsync(predicate: role => role.Id == id, cancellationToken: cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var identityResult = await roleManager.DeleteAsync(role: applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(aggregateRoot: roleResult.Value);

        return Result.Success();
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Role was not found."));
    }

    private void SyncClaims(
        ApplicationRole applicationRole,
        Role role)
    {
        var targetClaims = ApplicationRoleMapper.ToClaimDbModels(claims: role.Claims);

        foreach (var currentClaim in applicationRole.Claims.ToArray())
        {
            if (targetClaims.Any(predicate: targetClaim =>
                    string.Equals(targetClaim.ClaimType, currentClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(targetClaim.ClaimValue, currentClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            dbContext.Set<ApplicationRoleClaim>().Remove(entity: currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (applicationRole.Claims.Any(predicate: currentClaim =>
                    string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(currentClaim.ClaimValue, targetClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            applicationRole.Claims.Add(item: targetClaim);
        }
    }
}
