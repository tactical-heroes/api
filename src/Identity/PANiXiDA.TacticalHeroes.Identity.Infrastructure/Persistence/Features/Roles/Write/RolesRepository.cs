using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    IdentityWriteDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOpenIddictTokenManager tokenManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IRolesRepository
{
    public async Task<Result<Role?>> GetDefaultAsync(CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .AsNoTracking()
            .Include(role => role.Claims)
            .SingleOrDefaultAsync(role => role.IsDefault, cancellationToken);
        if (applicationRole is null)
        {
            return Result.Success<Role?>(value: null);
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);
        return roleResult.IsSuccess
            ? Result.Success<Role?>(value: roleResult.Value)
            : Result.Failure<Role?>(errors: roleResult.Errors);
    }

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
        CancellationToken cancellationToken)
    {
        var applicationRole = await roleManager.Roles
            .Include(item => item.Claims)
            .SingleOrDefaultAsync(item => item.Id == role.Id.Value, cancellationToken);

        if (applicationRole is null)
        {
            return RoleNotFound();
        }

        var currentClaims = applicationRole.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)).ToHashSet();
        var authorizationChanged = !string.Equals(applicationRole.Name, role.Name.Value, StringComparison.Ordinal)
            || !currentClaims.SetEquals(role.Claims.Select(claim => (claim.Type.Value, claim.Value.Value)));
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
            var users = await GetAssignedUsersAsync(role.Id.Value, cancellationToken);
            var revocationResult = await RevokeSessionsAsync(users, cancellationToken);
            if (revocationResult.IsFailure)
            {
                return revocationResult;
            }
        }

        aggregateTracker.Track(role);

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

        if (applicationRole.IsDefault)
        {
            return DefaultRoleConflict();
        }

        var roleResult = ApplicationRoleMapper.ToDomain(role: applicationRole);

        if (roleResult.IsFailure)
        {
            return Result.Failure(errors: roleResult.Errors);
        }

        var users = await GetAssignedUsersAsync(id, cancellationToken);
        var identityResult = await roleManager.DeleteAsync(applicationRole);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        var revocationResult = await RevokeSessionsAsync(users, cancellationToken);
        if (revocationResult.IsFailure)
        {
            return revocationResult;
        }

        aggregateTracker.Track(roleResult.Value);

        return Result.Success();
    }

    private static Result DefaultRoleConflict()
    {
        return Result.Failure(error: Error.Conflict(message: "Unset the default role before deleting it."));
    }

    private static Result RoleNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Role was not found."));
    }

    private Task<ApplicationUser[]> GetAssignedUsersAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ApplicationUser>()
            .Where(user => user.Roles.Any(role => role.RoleId == roleId))
            .ToArrayAsync(cancellationToken);
    }

    private async Task<Result> RevokeSessionsAsync(
        IReadOnlyCollection<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        foreach (var user in users)
        {
            var identityResult = await userManager.UpdateSecurityStampAsync(user);
            if (!identityResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(result: identityResult);
            }

            await tokenManager.RevokeBySubjectAsync(user.Id.ToString(), cancellationToken);
        }

        return Result.Success();
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
