using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Queries;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthUsersRepository(IdentityReadDbContext dbContext)
    : IOAuthUsersRepository
{
    public async Task<Result<ExchangeTokenReadModel>> GetExchangeTokenByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var applicationUser = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == accountId, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<ExchangeTokenReadModel>(
                error: Error.NotFound(message: "Account was not found."));
        }

        var availabilityResult = EnsureAvailable(applicationUser);

        return availabilityResult.IsFailure
            ? Result.Failure<ExchangeTokenReadModel>(errors: availabilityResult.Errors)
            : Result.Success(
                value: new ExchangeTokenReadModel(
                    Claims: IdentityClaimsFactory.Create(user: applicationUser)));
    }

    public async Task<Result<UserInfoReadModel>> GetUserInfoByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var applicationUser = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .WithAuthorizationGraph()
            .SingleOrDefaultAsync(user => user.Id == accountId, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<UserInfoReadModel>(
                error: Error.NotFound(message: "Account was not found."));
        }

        var availabilityResult = EnsureAvailable(applicationUser);

        if (availabilityResult.IsFailure)
        {
            return Result.Failure<UserInfoReadModel>(errors: availabilityResult.Errors);
        }

        IReadOnlyCollection<string> roles =
        [
            .. applicationUser.Roles
                .Select(userRole => userRole.Role?.Name)
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(roleName => roleName!)
                .Distinct(StringComparer.Ordinal)
        ];

        return Result.Success(
            value: new UserInfoReadModel(
                AccountId: applicationUser.Id,
                Name: applicationUser.UserName,
                Email: applicationUser.Email,
                EmailVerified: applicationUser.EmailConfirmed,
                Roles: roles));
    }

    private static Result EnsureAvailable(UserReadDbModel applicationUser)
    {
        if (string.Equals(
                applicationUser.Status,
                AccountStatus.Blocked.Name,
                StringComparison.Ordinal))
        {
            return Result.Failure(error: Error.Forbidden(message: "Account is blocked."));
        }

        return applicationUser.EmailConfirmed
            ? Result.Success()
            : Result.Failure(error: Error.Forbidden(message: "Account is not confirmed."));
    }
}
