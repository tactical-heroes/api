using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

public sealed class UsersWriteRepository(
    IdentityWriteDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOpenIddictTokenManager tokenManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IUsersWriteRepository
{
    public async Task<Result<Guid>> AddAsync(
        string email,
        string userName,
        string password,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        var userResult = User.Create(
            id: UserId.New().Value,
            email: email,
            confirmationStatus: isConfirmed,
            roleIds: [],
            claims: claims.Select(claim => (claim.Type, claim.Value)));
        var userNameResult = UserName.Create(value: userName);
        var statusResult = UserStatus.Create(value: status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationUser = ApplicationUserMapper.ToDbModel(
            user: userResult.Value,
            userName: userNameResult.Value,
            status: statusResult.Value,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        var identityResult = await userManager.CreateAsync(user: applicationUser, password: password);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(result: identityResult);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success(value: applicationUser.Id);
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken)
    {
        var userResult = User.Create(
            id: id,
            email: email,
            confirmationStatus: isConfirmed,
            roleIds: [],
            claims: claims.Select(claim => (claim.Type, claim.Value)));
        var userNameResult = UserName.Create(value: userName);
        var statusResult = UserStatus.Create(value: status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (applicationUser is null)
        {
            return UserNotFound();
        }

        ApplicationUserMapper.MapToDbModel(
            user: userResult.Value,
            userName: userNameResult.Value,
            status: statusResult.Value,
            dbModel: applicationUser,
            updatedAt: timeProvider.GetUtcNow().UtcDateTime);
        SyncClaims(
            applicationUser: applicationUser,
            user: userResult.Value);

        if (statusResult.Value.IsBlocked)
        {
            await RevokeAllTokensAsync(id, cancellationToken);
        }

        var identityResult = await userManager.UpdateAsync(applicationUser);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(result: identityResult);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
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
        return UpdateStatusAsync(id, UserStatus.Blocked, cancellationToken);
    }

    public Task<Result> UnblockAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(id, UserStatus.Active, cancellationToken);
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

    private void SyncClaims(
        ApplicationUser applicationUser,
        User user)
    {
        var targetClaims = ApplicationUserMapper.ToClaimDbModels(
            userId: user.Id.Value,
            claims: user.Claims);

        foreach (var currentClaim in applicationUser.Claims.ToArray())
        {
            if (targetClaims.Any(targetClaim =>
                    string.Equals(targetClaim.ClaimType, currentClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(targetClaim.ClaimValue, currentClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            dbContext.Set<ApplicationUserClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (applicationUser.Claims.Any(currentClaim =>
                    string.Equals(currentClaim.ClaimType, targetClaim.ClaimType, StringComparison.Ordinal) &&
                    string.Equals(currentClaim.ClaimValue, targetClaim.ClaimValue, StringComparison.Ordinal)))
            {
                continue;
            }

            applicationUser.Claims.Add(item: targetClaim);
        }
    }

    private static Result UserNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "User was not found."));
    }
}
