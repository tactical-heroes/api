using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepository(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IUsersRepository
{
    private IQueryable<ApplicationUser> Query =>
        userManager.Users
            .Include(user => user.Roles)
            .Include(user => user.Claims);

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        return user is null ? null : ApplicationUserMapper.ToDomain(user).Value;
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<Result> AddAsync(
        User aggregateRoot,
        string password,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = aggregateRoot.Id.Value
        };

        ApplyUserState(user, aggregateRoot);
        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(createResult);
        }

        var syncRolesResult = await SyncUserRolesAsync(user, aggregateRoot, cancellationToken);

        if (syncRolesResult.IsFailure)
        {
            return syncRolesResult;
        }

        var syncClaimsResult = await SyncUserClaimsAsync(user, aggregateRoot);

        if (syncClaimsResult.IsFailure)
        {
            return syncClaimsResult;
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .SingleOrDefaultAsync(user => user.Id == aggregateRoot.Id.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User was not found."));
        }

        ApplyUserState(user, aggregateRoot);
        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(updateResult);
        }

        var syncRolesResult = await SyncUserRolesAsync(user, aggregateRoot, cancellationToken);

        if (syncRolesResult.IsFailure)
        {
            return syncRolesResult;
        }

        var syncClaimsResult = await SyncUserClaimsAsync(user, aggregateRoot);

        if (syncClaimsResult.IsFailure)
        {
            return syncClaimsResult;
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        var user = await Query
            .SingleOrDefaultAsync(user => user.Id == aggregateRoot.Id.Value, cancellationToken);

        if (user is null)
        {
            return Result.Success();
        }

        var deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(deleteResult);
        }

        aggregateTracker.Track(aggregateRoot);

        return Result.Success();
    }

    private void ApplyUserState(
        ApplicationUser user,
        User aggregateRoot)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var email = aggregateRoot.Email.Value;

        user.Email = email;
        user.UserName = email;
        user.EmailConfirmed = aggregateRoot.ConfirmationStatus.IsConfirmed;
        user.UpdatedAt = nowUtc;

        if (user.CreatedAt == default)
        {
            user.CreatedAt = nowUtc;
        }
    }

    private async Task<Result> SyncUserRolesAsync(
        ApplicationUser user,
        User aggregateRoot,
        CancellationToken cancellationToken)
    {
        var targetRoleIds = aggregateRoot.RoleIds
            .Select(roleId => roleId.Value)
            .ToHashSet();
        var targetRoles = await roleManager.Roles
            .Where(role => targetRoleIds.Contains(role.Id))
            .ToArrayAsync(cancellationToken);

        if (targetRoles.Length != targetRoleIds.Count)
        {
            var foundRoleIds = targetRoles
                .Select(role => role.Id)
                .ToHashSet();
            var missingRoleIds = targetRoleIds.Except(foundRoleIds);

            return Result.Failure(
                Error.NotFound($"Identity roles were not found: {string.Join(", ", missingRoleIds)}."));
        }

        var targetRoleNames = targetRoles
            .Select(role => role.Name!)
            .ToHashSet(StringComparer.Ordinal);
        var currentRoleNames = await userManager.GetRolesAsync(user);

        foreach (var roleName in currentRoleNames.Except(targetRoleNames, StringComparer.Ordinal))
        {
            var removeRoleResult = await userManager.RemoveFromRoleAsync(user, roleName);

            if (!removeRoleResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(removeRoleResult);
            }
        }

        foreach (var roleName in targetRoleNames.Except(currentRoleNames, StringComparer.Ordinal))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);

            if (!addRoleResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(addRoleResult);
            }
        }

        return Result.Success();
    }

    private async Task<Result> SyncUserClaimsAsync(
        ApplicationUser user,
        User aggregateRoot)
    {
        var targetClaims = aggregateRoot.Claims
            .Select(claim => new Claim(claim.Type.Value, claim.Value.Value))
            .ToArray();
        var currentClaims = await userManager.GetClaimsAsync(user);

        foreach (var claim in currentClaims.Except(targetClaims, IdentityClaimComparer.Instance))
        {
            var removeClaimResult = await userManager.RemoveClaimAsync(user, claim);

            if (!removeClaimResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(removeClaimResult);
            }
        }

        foreach (var claim in targetClaims.Except(currentClaims, IdentityClaimComparer.Instance))
        {
            var addClaimResult = await userManager.AddClaimAsync(user, claim);

            if (!addClaimResult.Succeeded)
            {
                return IdentityResultMapper.ToResult(addClaimResult);
            }
        }

        return Result.Success();
    }
}
