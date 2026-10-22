using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersRepository(
    IdentityWriteDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOpenIddictTokenManager tokenManager,
    RoleAssignmentSessionService roleAssignmentSessionService,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IUsersRepository
{
    public async Task<Result<Guid>> AddAsync(
        User user,
        string password,
        CancellationToken cancellationToken)
    {
        var rolesResult = await ValidateRolesAsync(user, cancellationToken);
        if (rolesResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: rolesResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationUser = ApplicationUserMapper.ToDbModel(
            user: user,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        var identityResult = await userManager.CreateAsync(user: applicationUser, password: password);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        aggregateTracker.Track(user);

        return Result.Success(value: applicationUser.Id);
    }

    public async Task<Result<bool>> UpdateAsync(
        User user,
        CancellationToken cancellationToken,
        string? currentAccessToken = null)
    {
        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(item => item.Id == user.Id.Value, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<bool>(UserNotFound().Errors);
        }

        var rolesResult = await ValidateRolesAsync(user, cancellationToken);
        if (rolesResult.IsFailure)
        {
            return Result.Failure<bool>(rolesResult.Errors);
        }

        var rolesChanged = !user.RoleIds
            .Select(roleId => roleId.Value)
            .ToHashSet()
            .SetEquals(applicationUser.Roles.Select(role => role.RoleId));
        string? preservedAuthorizationId = null;
        if (rolesChanged)
        {
            if (!user.Status.IsBlocked && user.ConfirmationStatus.IsConfirmed)
            {
                var sessionResult = await roleAssignmentSessionService.GetCurrentAuthorizationIdAsync(
                    userId: user.Id.Value,
                    accessToken: currentAccessToken,
                    cancellationToken: cancellationToken);
                if (sessionResult.IsFailure)
                {
                    return Result.Failure<bool>(sessionResult.Errors);
                }

                preservedAuthorizationId = sessionResult.Value;
            }

            applicationUser.RoleAssignmentsVersion++;
            applicationUser.SecurityStamp = Guid.NewGuid().ToString();
        }

        SyncRoles(applicationUser, user);
        ApplicationUserMapper.MapToDbModel(
            user: user,
            dbModel: applicationUser,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        SyncClaims(
            applicationUser: applicationUser,
            user: user);

        if (user.Status.IsBlocked && !rolesChanged)
        {
            await RevokeAllTokensAsync(user.Id.Value, cancellationToken);
        }

        var identityResult = await userManager.UpdateAsync(applicationUser);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<bool>(result: identityResult);
        }

        if (rolesChanged)
        {
            await roleAssignmentSessionService.RevokeAsync(
                userId: user.Id.Value,
                preservedAuthorizationId: preservedAuthorizationId,
                cancellationToken: cancellationToken);
        }

        aggregateTracker.Track(user);

        return Result.Success(rolesChanged);
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        var userResult = ApplicationUserMapper.ToDomain(user: applicationUser);

        if (userResult.IsFailure)
        {
            return Result.Failure(errors: userResult.Errors);
        }

        await RevokeAllTokensAsync(id, cancellationToken);
        var identityResult = await userManager.DeleteAsync(applicationUser);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public Task<Result> BlockAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(id: id, status: UserStatus.Blocked, cancellationToken: cancellationToken);
    }

    public Task<Result> UnblockAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(id: id, status: UserStatus.Active, cancellationToken: cancellationToken);
    }

    private static Result UserNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "User was not found."));
    }

    private async Task<Result> UpdateStatusAsync(
        Guid id,
        UserStatus status,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.FindByIdAsync(id.ToString());

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        if (status.IsBlocked)
        {
            await RevokeAllTokensAsync(id, cancellationToken);
        }

        applicationUser.Status = status.Name;
        applicationUser.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        var identityResult = await userManager.UpdateAsync(applicationUser);

        return IdentityResultMapper.ToResult(result: identityResult);
    }

    private async Task RevokeAllTokensAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await tokenManager.RevokeBySubjectAsync(id.ToString(), cancellationToken);
    }

    private async Task<Result> ValidateRolesAsync(User user, CancellationToken cancellationToken)
    {
        var roleIds = user.RoleIds.Select(roleId => roleId.Value).ToArray();
        if (roleIds.Length == 0)
        {
            return Result.Success();
        }

        var count = await dbContext.Set<ApplicationRole>()
            .CountAsync(role => roleIds.Contains(role.Id), cancellationToken);
        if (count != roleIds.Length)
        {
            return Result.Failure(error: Error.Validation(message: "One or more roles were not found.")
                .WithField(nameof(User.RoleIds)));
        }

        return Result.Success();
    }

    private void SyncRoles(ApplicationUser applicationUser, User user)
    {
        var roleIds = user.RoleIds.Select(roleId => roleId.Value).ToHashSet();
        if (roleIds.SetEquals(applicationUser.Roles.Select(role => role.RoleId)))
        {
            return;
        }

        foreach (var currentRole in applicationUser.Roles.Where(role => !roleIds.Contains(role.RoleId)).ToArray())
        {
            dbContext.Set<ApplicationUserRole>().Remove(currentRole);
        }

        foreach (var roleId in roleIds.Where(id => applicationUser.Roles.All(role => role.RoleId != id)))
        {
            applicationUser.Roles.Add(new ApplicationUserRole { UserId = applicationUser.Id, RoleId = roleId });
        }
    }

    private void SyncClaims(
        ApplicationUser applicationUser,
        User user)
    {
        var targetClaims = ApplicationUserMapper.ToClaimDbModels(
            userId: user.Id.Value,
            claims: user.Claims);

        foreach (var currentClaim in applicationUser.Claims.ToArray())
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

            dbContext.Set<ApplicationUserClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (
                applicationUser.Claims.Any(
                    currentClaim =>
                        string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal)
                            && string.Equals(
                                currentClaim.ClaimValue,
                                targetClaim.ClaimValue,
                                StringComparison.Ordinal)))
            {
                continue;
            }

            applicationUser.Claims.Add(item: targetClaim);
        }
    }
}
