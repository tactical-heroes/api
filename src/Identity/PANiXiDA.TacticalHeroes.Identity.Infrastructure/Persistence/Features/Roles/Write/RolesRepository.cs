using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
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
    : IRolesWriteRepository
{
    public async Task<Result<Guid>> AddAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        var roleResult = Role.Create(
            id: Guid.NewGuid(),
            name: name,
            claims: claims.Select(claim => (claim.Type, claim.Value)));

        if (roleResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: roleResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationRole = ApplicationRoleMapper.ToDbModel(
            role: roleResult.Value,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        var identityResult = await roleManager.CreateAsync(role: applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success(value: applicationRole.Id);
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken)
    {
        var roleResult = Role.Create(
            id: id,
            name: name,
            claims: claims.Select(claim => (claim.Type, claim.Value)));

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var applicationRole = await roleManager.Roles
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        ApplicationRoleMapper.MapToDbModel(
            role: roleResult.Value,
            dbModel: applicationRole,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        SyncClaims(
            applicationRole: applicationRole,
            role: roleResult.Value);

        var identityResult = await roleManager.UpdateAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var identityResult = await roleManager.DeleteAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    private void SyncClaims(
        ApplicationRole applicationRole,
        Role role)
    {
        var targetClaims = ApplicationRoleMapper.ToClaimDbModels(claims: role.Claims);

        foreach (var currentClaim in applicationRole.Claims.ToArray())
        {
            if (targetClaims.Any(targetClaim =>
                    string.Equals(targetClaim.ClaimType, currentClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(targetClaim.ClaimValue, currentClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            dbContext.Set<ApplicationRoleClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (applicationRole.Claims.Any(currentClaim =>
                    string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(currentClaim.ClaimValue, targetClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            applicationRole.Claims.Add(item: targetClaim);
        }
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Role was not found."));
    }
}
