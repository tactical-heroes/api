using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;

public sealed class RolesRepository(
    RoleManager<ApplicationRole> roleManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IRolesRepository
{
    private IQueryable<ApplicationRole> Query =>
        roleManager.Roles
            .Include(role => role.Claims);

    public async Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await Query
            .SingleOrDefaultAsync(role => role.Id == id, cancellationToken);

        return role is null ? null : MapToDomain(role);
    }

    public async Task<Result> AddAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken)
    {
        var role = new ApplicationRole
        {
            Id = aggregateRoot.Id.Value
        };

        ApplyRoleState(role, aggregateRoot);
        var createResult = await roleManager.CreateAsync(role);

        if (!createResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(createResult);
        }

        var syncClaimsResult = await SyncRoleClaimsAsync(role, aggregateRoot);

        if (syncClaimsResult.IsFailure)
        {
            return syncClaimsResult;
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken)
    {
        var role = await Query
            .SingleOrDefaultAsync(role => role.Id == aggregateRoot.Id.Value, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Role was not found."));
        }

        ApplyRoleState(role, aggregateRoot);
        var updateResult = await roleManager.UpdateAsync(role);

        if (!updateResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(updateResult);
        }

        var syncClaimsResult = await SyncRoleClaimsAsync(role, aggregateRoot);

        if (syncClaimsResult.IsFailure)
        {
            return syncClaimsResult;
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken)
    {
        var role = await Query
            .SingleOrDefaultAsync(role => role.Id == aggregateRoot.Id.Value, cancellationToken);

        if (role is null)
        {
            return Result.Success();
        }

        var deleteResult = await roleManager.DeleteAsync(role);

        if (!deleteResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(deleteResult);
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    private void ApplyRoleState(
        ApplicationRole role,
        Role aggregateRoot)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        role.Name = aggregateRoot.Name.Value;
        role.UpdatedAt = nowUtc;

        if (role.CreatedAt == default)
        {
            role.CreatedAt = nowUtc;
        }
    }

    private async Task<Result> SyncRoleClaimsAsync(
        ApplicationRole role,
        Role aggregateRoot)
    {
        var targetClaims = aggregateRoot.Claims
            .Select(claim => new Claim(claim.Type.Value, claim.Value.Value))
            .ToArray();
        var currentClaims = await roleManager.GetClaimsAsync(role);

        foreach (var claim in currentClaims.Except(targetClaims, IdentityClaimComparer.Instance))
        {
            var removeClaimResult = await roleManager.RemoveClaimAsync(role, claim);

            if (!removeClaimResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(removeClaimResult);
            }
        }

        foreach (var claim in targetClaims.Except(currentClaims, IdentityClaimComparer.Instance))
        {
            var addClaimResult = await roleManager.AddClaimAsync(role, claim);

            if (!addClaimResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(addClaimResult);
            }
        }

        return Result.Success();
    }

    private static Role MapToDomain(ApplicationRole role)
    {
        return Role.Create(
                role.Id,
                role.Name!,
                role.Claims.Select(claim => (claim.ClaimType!, claim.ClaimValue!)))
            .Value;
    }
}
