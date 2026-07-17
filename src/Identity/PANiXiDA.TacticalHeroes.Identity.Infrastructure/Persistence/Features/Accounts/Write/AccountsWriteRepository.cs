using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Mappers;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Accounts.Write;

public sealed class AccountsWriteRepository(
    IdentityWriteDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOpenIddictTokenManager tokenManager,
    IAggregateTracker aggregateTracker,
    TimeProvider timeProvider)
    : IAccountsWriteRepository
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
        cancellationToken.ThrowIfCancellationRequested();

        var userResult = CreateUser(email, claims);
        var userNameResult = UserName.Create(userName);
        var statusResult = AccountStatus.Create(status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var applicationUser = new ApplicationUser
        {
            Id = userResult.Value.Id.Value,
            Email = userResult.Value.Email.Value,
            UserName = userNameResult.Value.Value,
            EmailConfirmed = isConfirmed,
            Status = statusResult.Value.Name,
            LockoutEnabled = true,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc,
            Claims = CreateClaims(claims)
        };

        var identityResult = await userManager.CreateAsync(applicationUser, password);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult<Guid>(identityResult);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success(applicationUser.Id);
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
        var userResult = CreateUser(id, email, isConfirmed, claims);
        var userNameResult = UserName.Create(userName);
        var statusResult = AccountStatus.Create(status);
        var validationResult = Result.Combine(userResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var applicationUser = await userManager.Users
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (applicationUser is null)
        {
            return AccountNotFound();
        }

        applicationUser.Email = userResult.Value.Email.Value;
        applicationUser.UserName = userNameResult.Value.Value;
        applicationUser.EmailConfirmed = isConfirmed;
        applicationUser.Status = statusResult.Value.Name;
        applicationUser.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        SyncClaims(applicationUser, claims);

        if (statusResult.Value.IsBlocked)
        {
            await RevokeAllTokensAsync(id, cancellationToken);
        }

        var identityResult = await userManager.UpdateAsync(applicationUser);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(identityResult);
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
            return AccountNotFound();
        }

        var userResult = CreateUser(
            applicationUser.Id,
            applicationUser.Email!,
            applicationUser.EmailConfirmed,
            applicationUser.Claims.Select(claim =>
                new Claim(claim.ClaimType!, claim.ClaimValue!)).ToArray());

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Errors);
        }

        await RevokeAllTokensAsync(id, cancellationToken);
        var identityResult = await userManager.DeleteAsync(applicationUser);

        if (!identityResult.Succeeded)
        {
            return IdentityResultMapper.ToResult(identityResult);
        }

        aggregateTracker.Track(userResult.Value);

        return Result.Success();
    }

    public Task<Result> BlockAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(id, AccountStatus.Blocked, cancellationToken);
    }

    public Task<Result> UnblockAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(id, AccountStatus.Active, cancellationToken);
    }

    private async Task<Result> UpdateStatusAsync(
        Guid id,
        AccountStatus status,
        CancellationToken cancellationToken)
    {
        var applicationUser = await userManager.FindByIdAsync(id.ToString());

        if (applicationUser is null)
        {
            return AccountNotFound();
        }

        if (status.IsBlocked)
        {
            await RevokeAllTokensAsync(id, cancellationToken);
        }

        applicationUser.Status = status.Name;
        applicationUser.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        var identityResult = await userManager.UpdateAsync(applicationUser);

        return IdentityResultMapper.ToResult(identityResult);
    }

    private async Task RevokeAllTokensAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await tokenManager.RevokeBySubjectAsync(id.ToString(), cancellationToken);
    }

    private static Result<User> CreateUser(
        string email,
        IReadOnlyCollection<Claim> claims)
    {
        var userResult = User.Register(email);

        if (userResult.IsFailure)
        {
            return userResult;
        }

        foreach (var claim in claims.Distinct(IdentityClaimComparer.Instance))
        {
            var claimResult = userResult.Value.GrantClaim(claim.Type, claim.Value);

            if (claimResult.IsFailure)
            {
                return Result.Failure<User>(claimResult.Errors);
            }
        }

        return userResult;
    }

    private static Result<User> CreateUser(
        Guid id,
        string email,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims)
    {
        return User.Create(
            id,
            email,
            isConfirmed,
            [],
            claims
                .Distinct(IdentityClaimComparer.Instance)
                .Select(claim => (claim.Type, claim.Value)));
    }

    private static List<ApplicationUserClaim> CreateClaims(
        IEnumerable<Claim> claims)
    {
        return
        [
            .. claims
                .Distinct(IdentityClaimComparer.Instance)
                .Select(claim => new ApplicationUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                })
        ];
    }

    private void SyncClaims(
        ApplicationUser applicationUser,
        IReadOnlyCollection<Claim> claims)
    {
        var targetClaims = claims
            .Distinct(IdentityClaimComparer.Instance)
            .ToArray();

        foreach (var currentClaim in applicationUser.Claims.ToArray())
        {
            var claim = new Claim(currentClaim.ClaimType!, currentClaim.ClaimValue!);

            if (targetClaims.Contains(claim, IdentityClaimComparer.Instance))
            {
                continue;
            }

            dbContext.Set<ApplicationUserClaim>().Remove(currentClaim);
        }

        foreach (var targetClaim in targetClaims)
        {
            if (applicationUser.Claims.Any(currentClaim =>
                    string.Equals(currentClaim.ClaimType, targetClaim.Type, StringComparison.Ordinal) &&
                    string.Equals(currentClaim.ClaimValue, targetClaim.Value, StringComparison.Ordinal)))
            {
                continue;
            }

            applicationUser.Claims.Add(
                new ApplicationUserClaim
                {
                    ClaimType = targetClaim.Type,
                    ClaimValue = targetClaim.Value
                });
        }
    }

    private static Result AccountNotFound()
    {
        return Result.Failure(Error.NotFound("Account was not found."));
    }
}
