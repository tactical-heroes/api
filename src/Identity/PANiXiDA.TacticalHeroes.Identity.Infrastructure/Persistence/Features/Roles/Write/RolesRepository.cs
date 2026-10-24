using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserSessionService userSessionService,
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

        aggregateTracker.Track(role);

        return Result.Success(value: applicationRole.Id);
    }

    public async Task<Result> UpdateAsync(
        Role role,
        string? currentAccessToken,
        CancellationToken cancellationToken)
    {
        var applicationRole = await FindForUpdateAsync(role.Id.Value, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        var authorizationChanged = !string.Equals(applicationRole.Name, role.Name.Value, StringComparison.Ordinal)
            || !role.Claims.Select(claim => ((string?)claim.Type.Value, (string?)claim.Value.Value))
                .ToHashSet()
                .SetEquals(applicationRole.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)));
        var userIds = GetMemberIds(role.Id.Value);
        var sessionResult = authorizationChanged
            ? await userSessionService.GetCurrentAuthorizationIdAsync(
                userIds: userIds,
                accessToken: currentAccessToken,
                cancellationToken: cancellationToken)
            : Result.Success<string?>(value: null);
        if (sessionResult.IsFailure)
        {
            return Result.Failure(sessionResult.Errors);
        }

        ApplicationRoleMapper.MapToDbModel(
            role: role,
            dbModel: applicationRole,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        SyncClaims(
            applicationRole: applicationRole,
            role: role);

        var identityResult = await roleManager.UpdateAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        if (authorizationChanged)
        {
            await userSessionService.RevokeManyAsync(
                userIds: userIds,
                preservedAuthorizationId: sessionResult.Value,
                cancellationToken: cancellationToken);
        }
        aggregateTracker.Track(role);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        string? currentAccessToken,
        CancellationToken cancellationToken)
    {
        var applicationRole = await FindForUpdateAsync(id, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        if (applicationRole.IsDefault)
        {
            return Result.Failure(error: Error.Conflict(message: "Unset the default role before deleting it."));
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var userIds = GetMemberIds(id);
        var sessionResult = await userSessionService.GetCurrentAuthorizationIdAsync(
            userIds: userIds,
            accessToken: currentAccessToken,
            cancellationToken: cancellationToken);
        if (sessionResult.IsFailure)
        {
            return Result.Failure(sessionResult.Errors);
        }

        await userSessionService.RevokeManyAsync(
            userIds: userIds,
            preservedAuthorizationId: sessionResult.Value,
            cancellationToken: cancellationToken);

        var identityResult = await roleManager.DeleteAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Role was not found."));
    }

    private Task<ApplicationRole?> FindForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<ApplicationRole>()
            .FromSqlInterpolated($"SELECT * FROM identity.asp_net_roles WHERE id = {id} FOR UPDATE")
            .Include(role => role.Claims)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Guid> GetMemberIds(Guid roleId)
    {
        return dbContext.Set<ApplicationUserRole>()
            .Where(userRole => userRole.RoleId == roleId)
            .Select(userRole => userRole.UserId);
    }

    private void SyncClaims(
        ApplicationRole applicationRole,
        Role role)
    {
        var targetClaims = ApplicationRoleMapper.ToClaimDbModels(claims: role.Claims);

        foreach (var currentClaim in applicationRole.Claims.ToArray())
        {
            if (
                targetClaims.Any(
                    targetClaim =>
                        string.Equals(targetClaim.ClaimType, currentClaim.ClaimType, StringComparison.Ordinal)
                            && string.Equals(
                                targetClaim.ClaimValue,
                                currentClaim.ClaimValue,
                                StringComparison.Ordinal)))
            {
                continue;
            }

            dbContext.Set<ApplicationRoleClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (
                applicationRole.Claims.Any(
                    currentClaim =>
                        string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal)
                            && string.Equals(
                                currentClaim.ClaimValue,
                                targetClaim.ClaimValue,
                                StringComparison.Ordinal)))
            {
                continue;
            }

            applicationRole.Claims.Add(item: targetClaim);
        }
    }
}
